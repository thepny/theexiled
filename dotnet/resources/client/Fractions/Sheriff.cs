﻿using System;
using System.Collections.Generic;
using System.Data;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using NeptuneEvo.Core.Character;
using Newtonsoft.Json;

namespace NeptuneEvo.Fractions
{
    class Sheriff : Script
    {
        private static nLog Log = new nLog("Sheriff");

        
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> sheriffCheckpoints = new List<Vector3>()
        {
            new Vector3(463.2361, -998.3675, 23.91487), // место посадки игрока 0
            new Vector3(-430.7331, 5999.503, 30.59653), // Оружейный склад ( на против стоит пед) 1
            new Vector3(-433.6318, 5990.971, 30.59653), // Начать рабочий день Шерифа ( без педа) (взять одежду) 2
            new Vector3(-455.9738, 6014.119, 30.59654), // Чек поинт режима ЧП ( + Пед Alonzo) 3
            new Vector3(-441.9835, 5987.603, 30.59653), // Точка в тюрьме куда сажают игрока за решоткой. 4
            new Vector3(-436.764, 6020.909, 30.37011), // Место спавна во фракции Шерифа (перед зданием можно) 5
            new Vector3(441.9336, -981.5965, 29.6896), // Покупка лицензии  6
            new Vector3(-448.1254, 6014.227, 30.59655), // Место куда сдают мешок с деньгами 7
            new Vector3(-426.0116, 5998.237, 30.59653),  // Склад оружия     8
            new Vector3(-464.8731, 6042.925, 30.22054),  // Буст авто Шерифа      9
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_arm_secdoor"), new Vector3(453.0793, -983.1894, 30.83926), 30f);

                NAPI.TextLabel.CreateTextLabel("~r~Bot Koltr", new Vector3(-455.9738, 6014.119, 32.59654), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); // Режим ЧП
                NAPI.TextLabel.CreateTextLabel("~q~Kira", new Vector3(-449.8658, 6012.458, 32.59655), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); // в здании за стойков на входе
                NAPI.TextLabel.CreateTextLabel("~p~Стёпа БДСМ", new Vector3(-429.0482, 5997.3, 32.59655), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); // Оружейный склад

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[0], 6, 3, 0));
                Cols[0].OnEntityEnterColShape += arrestShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += arrestShape_onEntityExitColShape;

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 100);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Press E. To open the menu"), new Vector3(sheriffCheckpoints[1].X, sheriffCheckpoints[1].Y, sheriffCheckpoints[1].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[2], 1, 2, 0));
                Cols[2].SetData("INTERACT", 110);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Press E. To change to disguise"), new Vector3(sheriffCheckpoints[2].X, sheriffCheckpoints[2].Y, sheriffCheckpoints[2].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 120);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Press E. To open ES menu"), new Vector3(sheriffCheckpoints[3].X, sheriffCheckpoints[3].Y, sheriffCheckpoints[3].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[7], 1, 2, 0));
                Cols[5].SetData("INTERACT", 420);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Pass the Summku"), new Vector3(sheriffCheckpoints[7].X, sheriffCheckpoints[7].Y, sheriffCheckpoints[7].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[8], 1, 2, 0));
                Cols[6].SetData("INTERACT", 590);
                Cols[6].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[6].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Open menu"), new Vector3(sheriffCheckpoints[8].X, sheriffCheckpoints[8].Y, sheriffCheckpoints[8].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(sheriffCheckpoints[9], 4, 5, 0));
                Cols[7].SetData("INTERACT", 660);
                Cols[7].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[7].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~o~Improvement"), new Vector3(sheriffCheckpoints[9].X, sheriffCheckpoints[9].Y, sheriffCheckpoints[9].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[7] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[8] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, sheriffCheckpoints[9] - new Vector3(0, 0, 3.7), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220));
            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void callSheriff(Player player, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try {
                    if (Manager.countOfFractionMembers(18) == 0 && Manager.countOfFractionMembers(9) == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "No policemen in your area.try later", 3000);
                        return;
                    }
                    if (player.HasData("NEXTCALL_SHERIFF") && DateTime.Now < player.GetData<DateTime>("NEXTCALL_SHERIFF"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You have already called the police, try later", 3000);
                        return;
                    }
                    player.SetData("NEXTCALL_SHERIFF", DateTime.Now.AddMinutes(7));

                    if (player.HasData("CALLSHERIFF_BLIP"))
                        NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLSHERIFF_BLIP"));

                    var Blip = NAPI.Blip.CreateBlip(0, player.Position, 1, 70, "Call from " + player.Name.Replace('_', ' ') + $" ({player.Value})", 0, 0, true, 0, 0);
                    Blip.Transparency = 0;
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                        {
                            if (!Main.Players.ContainsKey(p)) continue;
                            if (Main.Players[p].FractionID != 18 && Main.Players[p].FractionID != 9) continue;
                            p.TriggerEvent("changeBlipAlpha", Blip, 255);
                        }
                    player.SetData("CALLSHERIFF_BLIP", Blip);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(player.Position, 70, 4, 0);
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        if (e == player)
                        {
                            try
                            {
                                Blip.Delete();
                                e.ResetData("CALLSHERIFF_BLIP");

                                Manager.sendFractionMessage(18, $"{e.Name.Replace('_', ' ')} canceled call");
                                Manager.sendFractionMessage(9, $"{e.Name.Replace('_', ' ')} canceled call");

                                colshape.Delete();

                                e.ResetData("CALLSHERIFF_COL");
                                e.ResetData("IS_CALLSHERIFF");
                            }
                            catch (Exception ex) { Log.Write("EnterSheriffCall: " + ex.Message); }
                        }
                    };
                    player.SetData("CALLSHERIFF_COL", colshape);

                    player.SetData("IS_CALLSHERIFF", true);
                    Manager.sendFractionMessage(18, $"Received a call from the player ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(18, $"~b~Received a call from the player ({player.Value}) - {reason}", true);
                    Manager.sendFractionMessage(9, $"Received a call from the player ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(9, $"~b~Received a call from the player ({player.Value}) - {reason}", true);
                }
                catch { }
            });
        }

        public static void acceptCall(Player player, Player target)
        {
            try
            {
                if (!Manager.canUseCommand(player, "pd")) return;
                if(target == null || !NAPI.Entity.DoesEntityExist(target)) return;
                if (!target.HasData("IS_CALLSHERIFF"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The player did not cause the police or this challenge had already accepted", 3000);
                    return;
                }
                Blip blip = target.GetData<Blip>("CALLSHERIFF_BLIP");

                Trigger.ClientEvent(player, "changeBlipColor", blip, 38);
                Trigger.ClientEvent(player, "createWaypoint", blip.Position.X, blip.Position.Y);

                ColShape colshape = target.GetData<ColShape>("CALLSHERIFF_COL");
                colshape.OnEntityEnterColShape += (s, e) =>
                {
                    if (e == player)
                    {
                        try
                        {
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLSHERIFF_BLIP"));
                                    target.ResetData("CALLSHERIFF_BLIP");
                                    colshape.Delete();
                                }
                                catch { }
                            });
                        }
                        catch (Exception ex) { Log.Write("EnterSheriffCall: " + ex.Message); }
                    }
                };

                Manager.sendFractionMessage(18, $"{player.Name.Replace('_', ' ')} accepted a call from the player ({target.Value})");
                Manager.sendFractionMessage(18, $"~b~{player.Name.Replace('_', ' ')} accepted a call from the player ({target.Value})", true);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) Accepted your call", 3000);
            }
            catch {
            }
        }
        
        public static void Event_PlayerDeath(Player player, Player killer, uint reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    if (NAPI.Data.GetEntityData(player, "IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            if (!Main.Players.ContainsKey(player)) return;
            switch (interact)
            {
                case 100:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee Sheriff", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[18].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Warehouse is closed", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                        return;
                    }
                    OpenSheriffGunMenu(player);
                    return;
                case 110:
                    if (Main.Players[player].FractionID == 18)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You started working day", 3000);
                            Manager.setSkin(player, 18, Main.Players[player].FractionLVL);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                            break;
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have finished working day", 3000);
                            Customization.ApplyCharacter(player);
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                            break;
                        }
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee Sheriff", 3000);
                    return;
                case 120:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                        return;
                    }
                    if (!is_warg)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Did not turn the MP mode on", 3000);
                        return;
                    }
                    OpenSpecialSheriffMenu(player);
                    return;
                case 420:
                    if (!player.HasData("HAND_MONEY") && !player.HasData("HEIST_DRILL"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have any bags with money, no bags with a drill", 3000);
                        return;
                    }
                    if (player.HasData("HAND_MONEY"))
                    {
                        nInventory.Remove(player, ItemType.BagWithMoney, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HAND_MONEY");
                    }
                    if (player.HasData("HEIST_DRILL"))
                    {
                        nInventory.Remove(player, ItemType.BagWithDrill, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HEIST_DRILL");
                    }
                    MoneySystem.Wallet.Change(player, 200);
                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", 200, $"sheriffAward");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You received $ 200 reward", 3000);
                    return;
                case 440:
                    if (Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You already have a weapon license", 3000);
                        return;
                    }
                    if (!MoneySystem.Wallet.Change(player, -30000))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have enough funds.", 3000);
                        return;
                    }
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You bought a license for weapons", 3000);
                    Main.Players[player].Licenses[6] = true;
                    Dashboard.sendStats(player);
                    return;
                case 590:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[18].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Warehouse is closed", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 18);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[18].Weapons, "Armory", 6);
                    return;
                case 660:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee Sheriff", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                        return;
                    }
                    if (!player.IsInVehicle || (player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff") && 
                        player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff2") && player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff3") && player.Vehicle.Model != NAPI.Util.GetHashKey("sheriff4")))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the work machine", 3000);
                        return;
                    }
                    Trigger.ClientEvent(player, "svem", 20, 20);
                    player.Vehicle.SetSharedData("BOOST", 20);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Evacuated", 3000);
                    return;
            }
        }

        #region shapes
        private void arrestShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "IS_IN_ARREST_AREA", true);
                NAPI.Data.SetEntityData(entity, "ARREST_AREA_NAME", "SHERIFF");
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void arrestShape_onEntityExitColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                NAPI.Data.SetEntityData(player, "IS_IN_ARREST_AREA", false);
                if (Main.Players[player].ArrestTime != 0)
                {
                    NAPI.Entity.SetEntityPosition(player, Sheriff.sheriffCheckpoints[4]);
                }
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityEnterColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterColshape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityExitColshape(ColShape shape, Player entity)
        {
            try
            {
                entity.ResetData("INTERACTIONCHECK");
            }
            catch (Exception ex) { Log.Write("onEntityExitColshape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion

        public static void onPlayerDisconnectedhandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (NAPI.Data.HasEntityData(player, "ARREST_TIMER"))
                {
                    //Main.StopT(NAPI.Data.GetEntityData(player, "ARREST_TIMER"), "onPlayerDisconnectedhandler_arrest");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER"));
                }

                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    NAPI.Data.ResetEntityData(target, "FOLLOWER");
                }
                else if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    NAPI.Data.ResetEntityData(target, "FOLLOWING");
                    //target.FreezePosition = false;
                    Trigger.ClientEvent(target, "follow", false);
                }
                
                if (player.HasData("CALLSHERIFF_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLSHERIFF_BLIP"));

                    Manager.sendFractionMessage(18, $"{player.Name.Replace('_', ' ')} canceled call");
                    Manager.sendFractionMessage(9, $"{player.Name.Replace('_', ' ')} canceled call");
                }
                if (player.HasData("CALLSHERIFF_COL"))
                {
                    NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("CALLSHERIFF_COL"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static bool is_warg = false;

        #region menus
        public static void OpenSheriffGunMenu(Player player)
        {
            Trigger.ClientEvent(player, "sheriffg");
        }
        [RemoteEvent("sheriffgun")]
        public static void callback_sheriffGuns(Player client, int index)
        {
            try
            {
                switch (index)
                {
                    case 0: //nightstick
                        Fractions.Manager.giveGun(client, Weapons.Hash.Nightstick, "Nightstick");
                        return;
                    case 1: //pistol
                        Fractions.Manager.giveGun(client, Weapons.Hash.Pistol, "Pistol");
                        return;
                    case 2: //smg
                        Fractions.Manager.giveGun(client, Weapons.Hash.SMG, "SMG");
                        return;
                    case 3: //pumpshotgun
                        Fractions.Manager.giveGun(client, Weapons.Hash.PumpShotgun, "PumpShotgun");
                        return;
                    case 4: //stungun
                        Fractions.Manager.giveGun(client, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(client, "armor")) return;
                        if (Fractions.Stocks.fracStocks[18].Materials < Fractions.Manager.matsForArmor)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "In stock insufficient material", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[client].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "You already have a body armor", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[18].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"You got a body armor", 3000);
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "armor", 1, false);
                        return;
                    case 6: // medkit
                        if (!Manager.canGetWeapon(client, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[18].Medkits == 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "There are no aidhechk in stock", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[client].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "You already have a first-aid kit", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[18].Medkits--;
                        Fractions.Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(client, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "medkit", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"You got a first-aid kit", 3000);
                        return;
                    case 7: // pistol ammo
                        if (!Manager.canGetWeapon(client, "PistolAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.PistolAmmo, 12);
                        return;
                    case 8: // smg ammo
                        if (!Manager.canGetWeapon(client, "SMGAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.SMGAmmo, 30);
                        return;
                    case 9: // shotgun ammo
                        if (!Manager.canGetWeapon(client, "ShotgunsAmmo")) return;
                        Fractions.Manager.giveAmmo(client, ItemType.ShotgunsAmmo, 6);
                        return;
                    case 10:
                        if (Stocks.fracStocks[18].Materials < Manager.matsForDrone)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "На складе недостаточно материала", 3000);
                            return;
                        }

                        var tryAdd = nInventory.TryAdd(client, new nItem(ItemType.LSPDDrone));
                        if (tryAdd == 3 || tryAdd > 0)
                        {
                            Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "У вас с собой уже есть 3 квадрокоптера.", 3000);
                            return;
                        }

                        nInventory.Add(client, new nItem(ItemType.LSPDDrone));
                        Stocks.fracStocks[18].Materials -= Manager.matsForDrone;
                        Stocks.fracStocks[18].UpdateLabel();
                        GameLog.Stock(Main.Players[client].FractionID, Main.Players[client].UUID, "lsdpdrone", 1, false);
                        Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили LSPD квадрокоптер", 3000);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write($"Sheriffgun: " + e.Message, nLog.Type.Error);
            }
        }

        public static void OpenSpecialSheriffMenu(Player player)
        {
            Menu menu = new Menu("sheriffSpecial", false, false);
            menu.Callback += callback_sheriffSpecial;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Armory";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changeclothes", Menu.MenuItem.Button);
            menuItem.Text = "Change clothes";
            menu.Add(menuItem);

            menuItem = new Menu.Item("pistol50", Menu.MenuItem.Button);
            menuItem.Text = "Desert Eagle";
            menu.Add(menuItem);

            menuItem = new Menu.Item("carbineRifle", Menu.MenuItem.Button);
            menuItem.Text = "Assault rifle";
            menu.Add(menuItem);

            menuItem = new Menu.Item("riflesammo", Menu.MenuItem.Button);
            menuItem.Text = "Automatic caliber x30";
            menu.Add(menuItem);

            menuItem = new Menu.Item("heavyshotgun", Menu.MenuItem.Button);
            menuItem.Text = "Heavy shotgun";
            menu.Add(menuItem);
            
            menuItem = new Menu.Item("stungun", Menu.MenuItem.Button);
            menuItem.Text = "Tazer";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Close";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_sheriffSpecial(Player client, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "changeclothes":
                    if (!NAPI.Data.GetEntityData(client, "IN_CP_MODE"))
                    {
                        bool gender = Main.Players[client].Gender;
                        Customization.ApplyCharacter(client);
                        Customization.ClearClothes(client, gender);
                        if (gender)
                        {
                            Customization.SetHat(client, 39, 0);
                            //client.SetClothes(1, 52, 0);
                            client.SetClothes(11, 53, 0);
                            client.SetClothes(4, 31, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 15, 2);
                            client.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(client, 38, 0);
                            //client.SetClothes(1, 57, 0);
                            client.SetClothes(11, 46, 0);
                            client.SetClothes(4, 30, 0);
                            client.SetClothes(6, 25, 0);
                            client.SetClothes(9, 17, 2);
                            client.SetClothes(3, 53, 0);
                        }
                        if (client.HasData("HAND_MONEY")) client.SetClothes(5, 45, 0);
                        else if (client.HasData("HEIST_DRILL")) client.SetClothes(5, 41, 0);
                        NAPI.Data.SetEntityData(client, "IN_CP_MODE", true);
                        return;
                    }
                    Fractions.Manager.setSkin(client, 18, Main.Players[client].FractionLVL);
                    client.SetData("IN_CP_MODE", false);
                    return;
                case "pistol50":
                    Fractions.Manager.giveGun(client, Weapons.Hash.Pistol50, "pistol50");
                    return;
                case "carbineRifle":
                    Fractions.Manager.giveGun(client, Weapons.Hash.CarbineRifle, "carbineRifle");
                    return;
                case "riflesammo":
                    if (!Manager.canGetWeapon(client, "RiflesAmmo")) return;
                    Fractions.Manager.giveAmmo(client, ItemType.RiflesAmmo, 30);
                    return;
                case "heavyshotgun":
                    Fractions.Manager.giveGun(client, Weapons.Hash.HeavyShotgun, "heavyshotgun");
                    return;
                case "stungun":
                    Fractions.Manager.giveGun(client, Weapons.Hash.StunGun, "stungun");
                    return;
                case "close":
                    MenuManager.Close(client);
                    return;
            }
        }
        #endregion
    }
}