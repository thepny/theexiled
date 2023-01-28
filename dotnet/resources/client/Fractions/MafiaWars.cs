﻿using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;

namespace NeptuneEvo.Fractions
{
    class MafiaWars : Script
    {
        public static bool warIsGoing = false;
        public static bool warStarting = false;
        private static string warTimer;
        private static string toStartWarTimer;
        private static int attackersFracID = -1;
        private static int timerCount = 0;
        private static int attackersSt = 0;
        private static int defendersSt = 0;
        private static int bizID = -1;
        private static int whereWarIsGoing = -1;
        private static bool smbTryCapture = false;
        private static Dictionary<int, string> pictureNotif = new Dictionary<int, string>
        {
            { 10, "CHAR_MARTIN" }, // la cosa nostra
            { 11, "CHAR_JOSEF" }, // russian
            { 12, "CHAR_HAO" }, // yakuza
            { 13, "CHAR_SIMEON" }, // armenian
        };
        private static Dictionary<int, DateTime> nextCaptDate = new Dictionary<int, DateTime>
        {
            { 10, DateTime.Now },
            { 11, DateTime.Now },
            { 12, DateTime.Now },
            { 13, DateTime.Now },
        };
        private static Dictionary<int, DateTime> protectDate = new Dictionary<int, DateTime>
        {
            { 10, DateTime.Now },
            { 11, DateTime.Now },
            { 12, DateTime.Now },
            { 13, DateTime.Now },
        };
        private static List<Vector3> warPoints = new List<Vector3>()
        {
            new Vector3(1714.411, -1646.583, 110.5078),
            new Vector3(1018.687, 2363.665, 49.2389),
            new Vector3(525.6157, -3163.575, 2.183115),
            new Vector3(1666.619, -15.92353, 171.7745),
            new Vector3(-575.5857, 5332.536, 68.23749),
        };
        private static Dictionary<int, Blip> warBlips = new Dictionary<int, Blip>();
        private static List<ColShape> warPointColshape = new List<ColShape>();

        private static nLog Log = new nLog("MafiaWars");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                int i = 0;
                foreach (var vec in warPoints)
                {
                    warPointColshape.Add(NAPI.ColShape.CreateCylinderColShape(vec, 100, 10, 0));
                    warPointColshape[i].SetData("ID", i);
                    warPointColshape[i].OnEntityEnterColShape += onPlayerEnterBizWar;
                    warPointColshape[i].OnEntityExitColShape += onPlayerExitBizWar;
                    warBlips.Add(i, NAPI.Blip.CreateBlip(543, vec, 1, 40, Main.StringToU16("War for business"), 255, 0, true, 0, 0));
                    i++;
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [Command("stopbizwar")]
        public static void CMD_adminStopBizwar(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "stopbizwar")) return;

            if (!warStarting)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент не проходит захват территории", 3000);
                return;
            }

            endCapture(1); // остановка войны за бизнес

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Администратор {player.Name.Replace('_', ' ')} принудительно остановил текущий захват территории мафии.");
        }

        [Command("takebiz")]
        public static void CMD_takeBiz(Player player)
        {
            if (!Manager.canUseCommand(player, "takebiz")) return;
            if (player.GetData<int>("BIZ_ID") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not at any business", 3000);
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            biz.Mafia = Main.Players[player].FractionID;
            biz.UpdateLabel();
        }

        [Command("bizwar")]
        public static void CMD_startBizwar(Player player)
        {
            if (!Manager.canUseCommand(player, "bizwar")) return;
            if (!player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not at any business", 3000);
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            if (biz.Mafia == Main.Players[player].FractionID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't start war for your business", 3000);
                return;
            }
            if (biz.Mafia == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Business does not own any mafia", 3000);
                return;
            }
            if (DateTime.Now.Hour < 13 || DateTime.Now.Hour > 23)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can start the war from 13:00 to 23:00", 3000);
                return;
            }
            if (DateTime.Now < nextCaptDate[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((nextCaptDate[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can start war only through {min}:{sec}", 3000);
                return;
            }
            if (DateTime.Now < protectDate[biz.Mafia])
            {
                DateTime g = new DateTime((protectDate[biz.Mafia] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can start a war with this mafia only through {min}:{sec}", 3000);
                return;
            }
            if (Manager.countOfFractionMembers(biz.Mafia) < 4)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Insufficient online in the enemy mafia", 3000);
                return;
            }
            if (smbTryCapture) return;
            smbTryCapture = true;
            if (warIsGoing || warStarting)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"War for the territory already goes", 3000);
                smbTryCapture = false;
                return;
            }
            var ter = Jobs.WorkManager.rnd.Next(0, 5);
            warBlips[ter].Color = 49;
            Manager.sendFractionMessage(biz.Mafia, $"Ahtung!We have 20 minutes for fees! {Manager.getName(Main.Players[player].FractionID)} decided to grab our business");
            Manager.sendFractionMessage(Main.Players[player].FractionID, "Shoot!Press!About 20 minutes will fly opponents");
            
            timerCount = 0;
            bizID = biz.ID;
            
            attackersFracID = Main.Players[player].FractionID;
            nextCaptDate[attackersFracID] = DateTime.Now.AddMinutes(60); // NEXT BIZWAR
            whereWarIsGoing = ter;

            //toStartWarTimer = Main.StartT(1200000, 99999999, (o) => timerStart(), "MWARSTART_TIMER");
            toStartWarTimer = Timers.StartOnce(1200000, () => timerStart());

            warStarting = true;
            smbTryCapture = false;
        }

        private static void timerStart()
        {
            var attackers = 0;
            var defenders = 0;

            var biz = BusinessManager.BizList[bizID];

            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || !p.HasData("WARZONE") || p.GetData<int>("WARZONE") != whereWarIsGoing) continue;
                if (Main.Players[p].FractionID == biz.Mafia) defenders++;
                else if (Main.Players[p].FractionID == attackersFracID) attackers++;
            }
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || !p.HasData("WARZONE") || p.GetData<int>("WARZONE") != whereWarIsGoing) continue;
                if (Main.Players[p].FractionID == biz.Mafia || Main.Players[p].FractionID == attackersFracID)
                {
                    Trigger.ClientEvent(p, "sendCaptureInformation", attackers, defenders, 0, 0);
                    Trigger.ClientEvent(p, "captureHud", true);
                }
            }

            //warTimer = Main.StartT(1000, 1000, (o) => timerUpdate(), "MWUPDATE_TIMER");
            warTimer = Timers.Start(1000, () => timerUpdate());
            //Main.StopT(toStartWarTimer, "toStartWarTimer");
            warStarting = false;
            warIsGoing = true;

            Manager.sendFractionMessage(biz.Mafia, $"Akhtung!We attacked us! {Manager.getName(attackersFracID)} decided to grab our business");
            Manager.sendFractionMessage(attackersFracID, "Shoot!Press!You started war for business");
        }

        private static void timerUpdate()
        {
            try
            {
                var attackers = 0;
                var defenders = 0;
                Business biz = BusinessManager.BizList[bizID];
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p) || !p.HasData("WARZONE") || p.GetData<int>("WARZONE") != whereWarIsGoing) continue;
                    if (Main.Players[p].FractionID == biz.Mafia) defenders++;
                    else if (Main.Players[p].FractionID == attackersFracID) attackers++;
                }

                attackersSt = attackers;
                defendersSt = defenders;

                if (timerCount >= 300 && (attackers == 0 || defenders == 0))
                {
                    endCapture();
                }

                timerCount++;
                int minutes = timerCount / 60;
                int seconds = timerCount % 60;

                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p) || !p.HasData("WARZONE") || p.GetData<int>("WARZONE") != whereWarIsGoing) continue;
                    if (Main.Players[p].FractionID == biz.Mafia || Main.Players[p].FractionID == attackersFracID)
                    {
                        Trigger.ClientEvent(p, "sendCaptureInformation", attackers, defenders, minutes, seconds);
                    }
                }
            }
            catch (Exception e) { Log.Write("MafiaWars: " + e.Message, nLog.Type.Error); }
        }

        private static void endCapture(int type = 0)
        {
            try
            {
                var biz = BusinessManager.BizList[bizID];

                if (type == 0)
                {
                    if (attackersSt <= defendersSt)
                    {
                    Manager.sendFractionMessage(biz.Mafia, $"The SUCCS ran away!You gave them under the tail!You defended business");
                    Manager.sendFractionMessage(attackersFracID, "You dug!The enemies were stronger!You could not capture business");

                        foreach (var m in Manager.Members.Keys)
                        {
                            if (Main.Players[m].FractionID == biz.Mafia)
                            {
                                MoneySystem.Wallet.Change(m, 300);
                                GameLog.Money($"server", $"player({Main.Players[m].UUID})", 300, $"winBiz");
                            }
                        }
                    }
                    else if (attackersSt > defendersSt)
                    {
                    Manager.sendFractionMessage(biz.Mafia, $"You looked through a business ..");
                    Manager.sendFractionMessage(attackersFracID, "They shigped them as children!You captured a business!");
                        biz.Mafia = attackersFracID;
                        foreach (var m in Manager.Members.Keys)
                        {
                            if (Main.Players[m].FractionID == attackersFracID)
                            {
                                MoneySystem.Wallet.Change(m, 300);
                                GameLog.Money($"server", $"player({Main.Players[m].UUID})", 300, $"winBiz");
                            }
                        }
                        biz.UpdateLabel();
                    }
                }
                else
                {
                    Manager.sendFractionMessage(biz.Mafia, $"Внимание! Война за бизнес была досрочно остановлена администратором.");
                    Manager.sendFractionMessage(attackersFracID, "Внимание! Война за бизнес была досрочно остановлена администратором.");
                }

                //Main.StopT(warTimer, "warTimer");
                if (toStartWarTimer != null) Timers.Stop(toStartWarTimer);
                if (warTimer != null) Timers.Stop(warTimer);
                Main.ClientEventToAll("captureHud", false);

                protectDate[biz.Mafia] = DateTime.Now.AddMinutes(20);
                protectDate[attackersFracID] = DateTime.Now.AddMinutes(20);

                warStarting = false;
                warIsGoing = false;
                warBlips[whereWarIsGoing].Color = 40;
            }
            catch (Exception e) { Log.Write($"EndMafiaWar: " + e.Message, nLog.Type.Error); }
        }

        private static void onPlayerEnterBizWar(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID >= 10 && Main.Players[player].FractionID <= 13)
                {
                    player.SetData("WARZONE", shape.GetData<int>("ID"));
                    if (warIsGoing && (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == BusinessManager.BizList[bizID].Mafia) && whereWarIsGoing == shape.GetData<int>("ID"))
                    {
                        int minutes = timerCount / 60;
                        int seconds = timerCount % 60;
                        Trigger.ClientEvent(player, "sendCaptureInformation", attackersSt, defendersSt, minutes, seconds);
                        Trigger.ClientEvent(player, "captureHud", true);
                    }
                }
            }
            catch (Exception ex) { Log.Write("onPlayerEnterBizWar: " + ex.Message, nLog.Type.Error); }
        }

        private static void onPlayerExitBizWar(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID >= 10 && Main.Players[player].FractionID <= 13)
                {
                    player.SetData("WARZONE", -1);
                    Trigger.ClientEvent(player, "captureHud", false);
                }
            }
            catch (Exception ex) { Log.Write("onPlayerExitBizWar: " + ex.Message, nLog.Type.Error); }
        }
    }
}
