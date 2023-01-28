using NeptuneEvo.Core;
using Redage.SDK;
using System;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.GUI;
using NeptuneEvo.Core.Character;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

namespace NeptuneEvo.Fractions
{
    class FractionCommands : Script
    {
        private static nLog Log = new nLog("FractionCommangs");

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                player.TriggerEvent("blip_remove", "FractionCarSpawn");

                if (NAPI.Data.GetEntityData(player, "CUFFED") && player.VehicleSeat == 0)
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Release a man", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        private static Dictionary<int, DateTime> NextCarRespawn = new Dictionary<int, DateTime>()
        {
            { 1, DateTime.Now.AddMinutes(1) },
            { 2, DateTime.Now.AddMinutes(1) },
            { 3, DateTime.Now.AddMinutes(1) },
            { 4, DateTime.Now.AddMinutes(1) },
            { 5, DateTime.Now.AddMinutes(1) },
            { 6, DateTime.Now.AddMinutes(1) },
            { 7, DateTime.Now.AddMinutes(1) },
            { 8, DateTime.Now.AddMinutes(1) },
            { 9, DateTime.Now.AddMinutes(1) },
            { 10, DateTime.Now.AddMinutes(1) },
            { 11, DateTime.Now.AddMinutes(1) },
            { 12, DateTime.Now.AddMinutes(1) },
            { 13, DateTime.Now.AddMinutes(1) },
            { 14, DateTime.Now.AddMinutes(1) },
            { 15, DateTime.Now.AddMinutes(1) },
            { 16, DateTime.Now.AddMinutes(1) },
            { 17, DateTime.Now.AddMinutes(1) },
            { 18, DateTime.Now.AddMinutes(1) },
        };
        public static void respawnFractionCars(Player player)
        {
            if (Main.Players[player].FractionID == 0 || Main.Players[player].FractionLVL < (Configs.FractionRanks[Main.Players[player].FractionID].Count - 1)) return;
            if (DateTime.Now < NextCarRespawn[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((NextCarRespawn[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can only do this through {g.Minute}:{g.Second}", 3000);
                return;
            }

            var all_vehicles = NAPI.Pools.GetAllVehicles();
            foreach (var vehicle in all_vehicles)
            {
                var occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    var newOccupants = new List<Player>();
                    foreach (var occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (var vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                var color1 = vehicle.PrimaryColor;
                var color2 = vehicle.SecondaryColor;
                if (!vehicle.HasData("ACCESS")) continue;

                if (vehicle.GetData<string>("ACCESS") == "FRACTION" && vehicle.GetData<int>("FRACTION") == Main.Players[player].FractionID)
                    Admin.RespawnFractionCar(vehicle);
            }

            NextCarRespawn[Main.Players[player].FractionID] = DateTime.Now.AddHours(2);
            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have discarded all fractional cars", 3000);
        }
        public static void playerPressCuffBut(Player player)
        {
            var fracid = Main.Players[player].FractionID;
            if (!Manager.canUseCommand(player, "cuff")) return;
            if (NAPI.Data.GetEntityData(player, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are in handcuffs or related", 3000);
                return;
            }
            var target = Main.GetNearestPlayer(player, 2);
            if (target == null) return;
            var cuffmesp = ""; // message for Player after cuff
            var cuffmest = ""; // message for Target after cuff
            var uncuffmesp = ""; // message for Player after uncuff
            var uncuffmest = ""; // message for Target after uncuff
            var cuffme = ""; // message /me after cuff
            var uncuffme = ""; // message /me after uncuff

            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            if (Manager.FractionTypes[fracid] == 2) // for gov factions
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must first start working day", 3000);
                    return;
                }
                if (target.GetData<bool>("CUFFED_BY_MAFIA"))
                {
                    uncuffmesp = $"You untied the player {target.Name}";
                    uncuffmest = $"Player {player.Name} Unleashed you";
                    uncuffme = "Unleashed the player {name}";
                }
                else
                {
                    cuffmesp = $"You got handcuffs on the player {target.Name}";
                    cuffmest = $"Player {player.Name} put on you handcuffs";
                    cuffme = "put on the player on the player {name}";
                    uncuffmesp = $"You removed your handcuffs from the player {target.Name}";
                    uncuffmest = $"Player {player.Name} He took off your handcuffs";
                    uncuffme = "Removed handcuffs from the player {name}";
                }
            }
            else // for mafia
            {
                if (target.GetData<bool>("CUFFED_BY_COP"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys from handcuffs", 3000);
                    return;
                }
                var cuffs = nInventory.Find(Main.Players[player].UUID, ItemType.Cuffs);
                var count = (cuffs == null) ? 0 : cuffs.Count;

                if (!target.GetData<bool>("CUFFED") && count == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have no hand screeds", 3000);
                    return;
                }
                else if (!target.GetData<bool>("CUFFED"))
                    nInventory.Remove(player, ItemType.Cuffs, 1);

                cuffmesp = $"You tied the player {target.Name}";
                cuffmest = $"Player {player.Name} Tied you";
                cuffme = "Tied player {name}";
                uncuffmesp = $"You untied the player {target.Name}";
                uncuffmest = $"Player {player.Name} Unleashed you";
                uncuffme = "Unleashed the player {name}";
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are in the car", 3000);
                return;
            }
            if (NAPI.Player.IsPlayerInAnyVehicle(target))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player in the car", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING") || NAPI.Data.HasEntityData(target, "FOLLOWER") || Main.Players[target].ArrestTime != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Unable to apply on this player", 3000);
                return;
            }
            if (!target.GetData<bool>("CUFFED"))
            {
                // cuff target
                if (NAPI.Data.HasEntityData(target, "HAND_MONEY")) SafeMain.dropMoneyBag(target);
                if (NAPI.Data.HasEntityData(target, "HEIST_DRILL")) SafeMain.dropDrillBag(target);

                NAPI.Data.SetEntityData(target, "CUFFED", true);
                Voice.Voice.PhoneHCommand(target);

                Main.OnAntiAnim(player);
                NAPI.Player.PlayPlayerAnimation(target, 49, "mp_arresting", "idle");
                // -0.02 0.063 0 75 0 76
                BasicSync.AttachObjectToPlayer(target, NAPI.Util.GetHashKey("p_cs_cuffs_02_s"), 6286, new Vector3(-0.02f, 0.063f, 0.0f), new Vector3(75.0f, 0.0f, 76.0f));

                Trigger.ClientEvent(target, "CUFFED", true);
                if (fracid == 6 || fracid == 7 || fracid == 9 || fracid == 18) target.SetData("CUFFED_BY_COP", true);
                else target.SetData("CUFFED_BY_MAFIA", true);

                GUI.Dashboard.Close(target);
                Trigger.ClientEvent(target, "blockMove", true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, cuffmesp, 3000);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, cuffmest, 3000);
                Commands.RPChat("me", player, cuffme, target);
                return;
            }
            // uncuff target
            unCuffPlayer(target);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, uncuffmesp, 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, uncuffmest, 3000);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_COP", false);
            NAPI.Data.SetEntityData(target, "CUFFED_BY_MAFIA", false);
            Commands.RPChat("me", player, uncuffme, target);
            return;
        }
        
        public static void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "CUFFED"))
                {
                    unCuffPlayer(player);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    unFollow(player, target);
                }
                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Player cop = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    unFollow(cop, player);
                }
                if (player.HasData("HEAD_POCKET"))
                {
                    player.ClearAccessory(1);
                    player.SetClothes(1, 0, 0);

                    Trigger.ClientEvent(player, "setPocketEnabled", false);
                    player.ResetData("HEAD_POCKET");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        #region every fraction commands

        [Command("delad", GreedyArg = true)]
        public static void CMD_deleteAdvert(Player player, int AdID, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if(Main.Players[player].FractionID == 15) {
                    if (!Manager.canUseCommand(player, "delad")) return;
                    LSNews.AddAnswer(player, AdID, reason, true);
                }
                else if (Group.CanUseCmd(player, "delad")) LSNews.AddAnswer(player, AdID, reason, true);
            }
            catch (Exception e) { Log.Write("delad: " + e.Message, nLog.Type.Error); }
        }

        [Command("openstock")]
        public static void CMD_OpenFractionStock(Player player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The warehouse is already open", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = true;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You opened the warehouse", 3000);
        }

        [Command("closestock")]
        public static void CMD_CloseFractionStock(Player player)
        {
            if (!Manager.canUseCommand(player, "openstock")) return;

            if (!Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (!Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The warehouse is already closed", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].IsOpen = false;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You closed the warehouse", 3000);
        }

        public static void GetMembers(Player sender)
        {
            if (Manager.canUseCommand(sender, "members"))
            {
                sender.SendChatMessage("Members of the organization online:");
                int fracid = Main.Players[sender].FractionID;
                foreach (var m in Manager.Members)
                    if (m.Value.FractionID == fracid) sender.SendChatMessage($"[{m.Value.inFracName}] {m.Value.Name}");
            }
        }

        public static void GetAllMembers(Player sender)
        {
            if (Manager.canUseCommand(sender, "offmembers"))
            {
                string message = "All members of the organization: ";
                NAPI.Chat.SendChatMessageToPlayer(sender, message);
                int fracid = Main.Players[sender].FractionID;
                var result = MySQL.QueryRead($"SELECT * FROM `characters` WHERE `fraction`='{fracid}'");
                foreach (DataRow Row in result.Rows)
                {
                    var fraclvl = Convert.ToInt32(Row["fractionlvl"]);
                    NAPI.Chat.SendChatMessageToPlayer(sender, $"~g~[{Manager.getNickname(fracid, fraclvl)}]: ~w~" + Row["name"].ToString().Replace('_', ' '));
                }
                return;
            }
        }

        public static void SetFracRank(Player sender, Player target, int newrank)
        {
            if (Manager.canUseCommand(sender, "setrank"))
            {
                if(newrank <= 0) {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, "It is impossible to install a negative or zero rank", 3000);
                    return;
                }
                int senderlvl = Main.Players[sender].FractionLVL;
                int playerlvl = Main.Players[target].FractionLVL;
                int senderfrac = Main.Players[sender].FractionID;
                if (!Manager.inFraction(target, senderfrac)) return;

                if (newrank >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't boost before that rank.", 3000);
                    return;
                }
                if (playerlvl > senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't boost this player", 3000);
                    return;
                };
                Manager.UNLoad(target);

                Main.Players[target].FractionLVL = newrank;
                Manager.Load(target, Main.Players[target].FractionID, Main.Players[target].FractionLVL);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1)
                {
                    Manager.AllMembers[index].FractionLVL = newrank;
                    Manager.AllMembers[index].inFracName = Manager.getNickname(senderfrac, newrank);
                }
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Now you {Manager.Members[target].inFracName} in fractions", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"You raised players {target.Name} to {Manager.Members[target].inFracName}", 3000);
                Dashboard.sendStats(target);
                return;
            }
        }

        public static void InviteToFraction(Player sender, Player target)
        {
            if (Manager.canUseCommand(sender, "invite"))
            {
                if (sender.Position.DistanceTo(target.Position) > 3)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                    return;
                }
                if (Manager.isHaveFraction(target))
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player already consists of organizations", 3000);
                    return;
                }
                if (Main.Players[target].LVL < 1)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"I need at least 1 LVL to invit the player in the fraction", 3000);
                    return;
                }
                if (Main.Players[target].Warns > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"It is impossible to take this player", 3000);
                    return;
                }
                if (Manager.FractionTypes[Main.Players[sender].FractionID] == 2 && !Main.Players[target].Licenses[7])
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player does not have medical maps", 3000);
                    return;
                }

                target.SetData("INVITEFRACTION", Main.Players[sender].FractionID);
                target.SetData("SENDERFRAC", sender);
                Trigger.ClientEvent(target, "openDialog", "INVITED", $"{sender.Name} invited you B. {Manager.FractionNames[Main.Players[sender].FractionID]}");
                
                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You invited to the fraction {target.Name}", 3000);
                Dashboard.sendStats(target);
            }
        }

        public static void UnInviteFromFraction(Player sender, Player target, bool mayor = false)
        {
            if (!Manager.canUseCommand(sender, "uninvite")) return;
            if (sender == target)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't dismiss yourself", 3000);
                return;
            }

            int senderlvl = Main.Players[sender].FractionLVL;
            int playerlvl = Main.Players[target].FractionLVL;
            int senderfrac = Main.Players[sender].FractionID;

            if (senderlvl <= playerlvl)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't drive this player", 3000);
                return;
            }

            if (mayor)
            {
                if (Manager.FractionTypes[Main.Players[target].FractionID] != 2)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't drive this player", 3000);
                    return;
                }
            }
            else
            {
                if (senderfrac != Main.Players[target].FractionID)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player consists in another organization", 3000);
                    return;
                }
            }

            Manager.UNLoad(target);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Manager.AllMembers.RemoveAt(index);

            if(Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            Customization.ApplyCharacter(target);
            if (target.HasData("HAND_MONEY")) target.SetClothes(5, 45, 0);
            else if (target.HasData("HEIST_DRILL")) target.SetClothes(5, 41, 0);
            target.SetData("ON_DUTY", false);
            GUI.MenuManager.Close(sender);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"You were expelled from the fraction {Manager.FractionNames[Main.Players[sender].FractionID]}", 3000);
            Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You kicked out of the fraction {target.Name}", 3000);
            Dashboard.sendStats(target);
            return;
        }

        #endregion

        #region cops and cityhall commands
        public static void ticketToTarget(Player player, Player target, int sum, string reason)
        {
            if (!Manager.canUseCommand(player, "ticket")) return;
            if (sum > 7000 || sum < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Fine limit from $ 1 to $ 7000", 3000);
                return;
            }
            if (reason.Length > 100)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Too big cause", 3000);
                return;
            }
            if (Main.Players[target].Money < sum && MoneySystem.Bank.Accounts[Main.Players[target].Bank].Balance < sum)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has enough funds", 3000);
                return;
            }

            target.SetData("TICKETER", player);
            target.SetData("TICKETSUM", sum);
            target.SetData("TICKETREASON", reason);
            Trigger.ClientEvent(target, "openDialog", "TICKET", $"{player.Name} discharged you fine in size {sum}$ for {reason}. Pay?");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You discharged a fine for {target.Name} at the rate of {sum}$ for {reason}", 3000);
        }
        public static void ticketConfirm(Player target, bool confirm)
        {
            Player player = target.GetData<Player>("TICKETER");
            if (player == null || !Main.Players.ContainsKey(player)) return;
            int sum = target.GetData<int>("TICKETSUM");
            string reason = target.GetData<string>("TICKETREASON");

            if (confirm)
            {
                if (!MoneySystem.Wallet.Change(target, -sum) && !MoneySystem.Bank.Change(Main.Players[target].Bank, -sum, false))
                {
                    Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"Insufficient funds", 3000);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has enough funds", 3000);
                }

                Stocks.fracStocks[6].Money += Convert.ToInt32(sum * 0.9);
                MoneySystem.Wallet.Change(player, Convert.ToInt32(sum * 0.1));
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"You paid a fine of {sum}$ for {reason}", 3000);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{target.Name} paid a fine of {sum}$ for {reason}", 3000);
                Commands.RPChat("me", player, " Disposal a fine for {name}", target);
                Manager.sendFractionMessage(Main.Players[player].FractionID, $"{player.Name} fined {target.Name} on {sum}$ ({reason})", true);
                GameLog.Ticket(Main.Players[player].UUID, Main.Players[target].UUID, sum, reason, player.Name, target.Name);
            }
            else
            {
                Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"You refused to pay a fine of {sum}$ for {reason}", 3000);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{target.Name} refused to pay a fine of {sum}$ for {reason}", 3000);
            }
        }
        public static void arrestTarget(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "arrest")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Impossible to apply on yourself", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be near the camera", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player already in prison", 3000);
                return;
            }
            if (Main.Players[target].WantedLVL == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not wanted", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in handcuffs", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                unFollow(target.GetData<Player>("FOLLOWING"), target);
            }
            unCuffPlayer(target);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You planted the player ({target.Value}) on {Main.Players[target].WantedLVL.Level * 20} minutes", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) planted you by {Main.Players[target].WantedLVL.Level * 20} minutes", 3000);
            Commands.RPChat("me", player, " Placed {name} in the CPP", target);
            Manager.sendFractionMessage(Main.Players[player].FractionID, $"{player.Name} planted in KPZ. {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Manager.sendFractionMessage(9, $"{player.Name} planted in KPZ. {target.Name} ({Main.Players[target].WantedLVL.Reason})", true);
            Main.Players[target].ArrestTime = Main.Players[target].WantedLVL.Level * 20 * 60;
            GameLog.Arrest(Main.Players[player].UUID, Main.Players[target].UUID, Main.Players[target].WantedLVL.Reason, Main.Players[target].WantedLVL.Level, player.Name, target.Name);
            arrestPlayer(target, NAPI.Data.GetEntityData(player, "ARREST_AREA_NAME"));
        }

        public static void releasePlayerFromPrison(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "rfp")) return;
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                return;
            }
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Impossible to apply on yourself", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be near the camera", 3000);
                return;
            }
            if (Main.Players[target].ArrestTime == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in prison", 3000);
                return;
            }
            freePlayer(target, NAPI.Data.GetEntityData(player, "ARREST_AREA_NAME"));
            Main.Players[target].ArrestTime = 0;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You freed the player ({target.Value}) From prison", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Release you From prison", 3000);
            Commands.RPChat("me", player, " Freed {name} from the CPP", target);
        }

        public static void arrestTimer(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].ArrestTime == 0)
                {
                    freePlayer(player, NAPI.Data.GetEntityData(player, "ARREST_AREA_NAME"));
                    return;
                }
                Main.Players[player].ArrestTime--;
            } catch(Exception e)
            {
                Log.Write("ARRESTTIMER: " + e.ToString(), nLog.Type.Error);
            }
            
        }

        public static void freePlayer(Player player, string freeType)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("ARREST_TIMER")) return;
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER")); // still not fixed
                    NAPI.Data.ResetEntityData(player, "ARREST_TIMER");
                    Police.setPlayerWantedLevel(player, null);

                    switch (freeType)
                    {
                        default:
                            NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[5]);
                            break;
                        case "LSPD":
                            NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[5]);
                            break;
                        case "SHERIFF":
                            NAPI.Entity.SetEntityPosition(player, Sheriff.sheriffCheckpoints[5]);
                            break;
                    }
                    NAPI.Entity.SetEntityDimension(player, 0);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"You were released from prison", 3000);
                }
                catch { }
            });
        }

        public static void arrestPlayer(Player target, string arrestType)
        {
            switch(arrestType)
            {
                default:
                    NAPI.Entity.SetEntityPosition(target, Police.policeCheckpoints[4]);
                    Police.setPlayerWantedLevel(target, null);
                    break;
                case "LSPD":
                    NAPI.Entity.SetEntityPosition(target, Police.policeCheckpoints[4]);
                    Police.setPlayerWantedLevel(target, null);
                    break;
                case "SHERIFF":
                    NAPI.Entity.SetEntityPosition(target, Sheriff.sheriffCheckpoints[4]);
                    Police.setPlayerWantedLevel(target, null);
                    break;
            }
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => arrestTimer(target), "ARREST_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.Start(1000, () => arrestTimer(target)));
            Weapons.RemoveAll(target, true);
        }

        public static void unCuffPlayer(Player player)
        {
            Trigger.ClientEvent(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Player.StopPlayerAnimation(player);
            BasicSync.DetachObject(player);
            Trigger.ClientEvent(player, "blockMove", false);
            Main.OffAntiAnim(player);
        }

        [RemoteEvent("playerPressFollowBut")]
        public void ClientEvent_playerPressFollow(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Manager.canUseCommand(player, "follow", false)) return;
                if (player.HasData("FOLLOWER"))
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You let go player ({player.GetData<Player>("FOLLOWER").Value})", 3000);
                    Notify.Send(player.GetData<Player>("FOLLOWER"), NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) let go of you", 3000);
                    unFollow(player, player.GetData<Player>("FOLLOWER"));
                }
                else
                {
                    var target = Main.GetNearestPlayer(player, 2);
                    if (target == null || !Main.Players.ContainsKey(target)) return;
                    targetFollowPlayer(player, target);
                }
            }
            catch (Exception e) { Log.Write($"PlayerPressFollow: {e.ToString()} // {e.TargetSite} // ", nLog.Type.Error); }
        }

        public static void unFollow(Player cop, Player suspect)
        {
            NAPI.Data.ResetEntityData(cop, "FOLLOWER");
            NAPI.Data.ResetEntityData(suspect, "FOLLOWING");
            Trigger.ClientEvent(suspect, "setFollow", false);
        }

        public static void targetFollowPlayer(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            var fracid = Main.Players[player].FractionID;
            if (Manager.FractionTypes[fracid] == 2) // for gov factions
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must first start working day", 3000);
                    return;
                }
            }
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Impossible to apply on yourself", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You already drag the player", 3000);
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }

            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in handcuffs", 3000);
                return;
            }

            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is already dragging", 3000);
                return;
            }

            NAPI.Data.SetEntityData(player, "FOLLOWER", target);
            NAPI.Data.SetEntityData(target, "FOLLOWING", player);
            Trigger.ClientEvent(target, "setFollow", true, player);
            Commands.RPChat("me", player, "dragged {name}", target);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You dragged the player ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) dragged you", 3000);
        }
        public static void targetUnFollowPlayer(Player player)
        {
            if (!Manager.canUseCommand(player, "follow")) return;
            var fracid = Main.Players[player].FractionID;
            if (!NAPI.Data.HasEntityData(player, "FOLLOWER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not drag anyone", 3000);
                return;
            }
            Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
            unFollow(player, target);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You let go player ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) let go of you", 3000);
        }

        public static void suPlayer(Player player, int pasport, int stars, string reason)
        {
            if (!Manager.canUseCommand(player, "su")) return;
            if (!Main.PlayerNames.ContainsKey(pasport))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Passports with this number does not exist", 3000);
                return;
            }
            Player target = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The owner of the passport should be online", 3000);
                return;
            }
            if (player != target)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player in prison", 3000);
                    return;
                }

                if (stars > 5)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't give such a number of stars", 3000);
                    return;
                }

                if (Main.Players[target].WantedLVL == null || Main.Players[target].WantedLVL.Level + stars <= 5)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have announced a player " + target.Name.Replace('_', ' ') + " wanted", 3000);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} announced you wanted ({reason})", 3000);
                    var oldStars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
                    var wantedLevel = new WantedLevel(oldStars + stars, player.Name, DateTime.Now, reason);
                    Police.setPlayerWantedLevel(target, wantedLevel);
                    return;
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't give such a number of stars", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can not declare myself", 3000);
        }

        // Садит игрока в машину
        public static void playerInCar(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "incar")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Impossible to use on yourself", 3000);
                return;
            }
            var vehicle = VehicleManager.getNearestVehicle(player, 3);
            if (vehicle == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Near there are no cars", 3000);
                return;
            }

            // Для удобства убираем проверку на водителя при посадке в авто
            /*
            if (player.VehicleSeat != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the driver's seat", 3000);
                return;
            }
            */
            if (player.Position.DistanceTo(target.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(target, "CUFFED"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player must be handcuffed", 3000);
                return;
            }
            if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
            {
                var cop = NAPI.Data.GetEntityData(target, "FOLLOWING");
                unFollow(cop, target);
            }

            var emptySlots = new List<int>
            {
                2,
                1,
                0
            };

            var players = NAPI.Pools.GetAllPlayers();
            foreach (var p in players)
            {
                if (p == null || !p.IsInVehicle || p.Vehicle != vehicle) continue;
                if (emptySlots.Contains(p.VehicleSeat)) emptySlots.Remove(p.VehicleSeat);
            }

            if (emptySlots.Count == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"There is no place in the car", 3000);
                return;
            }

            NAPI.Player.SetPlayerIntoVehicle(target, vehicle, emptySlots[0]);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You blocked the player ({target.Value}) in the car", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Put you in the car", 3000);
            Commands.RPChat("me", player, " opened the door and sat down {name} in the car", target);
        }

        public static void playerOutCar(Player player, Player target)
        {
            if (player != target)
            {
                if (!Manager.canUseCommand(player, "pull")) return;
                Vector3 posPlayer = NAPI.Entity.GetEntityPosition(player);
                Vector3 posTarget = NAPI.Entity.GetEntityPosition(target);
                if (player.Position.DistanceTo(target.Position) < 5)
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(target))
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You pound a player ({target.Value}) From the car", 3000);
                        Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Throw you out of the car", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(target);
                        Commands.RPChat("me", player, " opened the door and pulled out {name} from the car", target);
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in the car", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't throw yourself out of the car", 3000);
        }

        public static void setWargPoliceMode(Player player)
        {
            if (!Manager.canUseCommand(player, "warg")) return;

            var message = "";
            Police.is_warg = !Police.is_warg;
            if (Police.is_warg) message = $"{NAPI.Player.GetPlayerName(player)} announced the PE mode !!!";
            else message = $"{NAPI.Player.GetPlayerName(player)} Disconnected PP mode.";
            Manager.sendFractionMessage(Main.Players[player].FractionID, message);
        }

        public static void takeGunLic(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "takegunlic")) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }
            if (!Main.Players[target].Licenses[6])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has no weapon license", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You selected a weapon license from the player ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Player ({player.Value}) Selected your weapon license", 3000);
            Main.Players[target].Licenses[6] = false;
            Dashboard.sendStats(target);
        }

        public static void giveGunLic(Player player, Player target, int price)
        {
            if (!Manager.canUseCommand(player, "givegunlic")) return;
            if (player == target) return;
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }
            if (price < 5000 || price > 6000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The price is incorrect", 3000);
                return;
            }
            if (Main.Players[target].Licenses[6])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player already has a weapon license", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has enough funds", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered to buy a license for a weapon player ({target.Value}) за ${price}", 3000);

            Trigger.ClientEvent(target, "openDialog", "GUN_LIC", $"Player ({player.Value}) invited you to buy a license for weapons for ${price}");
            target.SetData("SELLER", player);
            target.SetData("GUN_PRICE", price);
        }

        public static void acceptGunLic(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;

            Player seller = player.GetData<Player>("SELLER");
            if (!Main.Players.ContainsKey(seller)) return;
            int price = player.GetData<int>("GUN_PRICE");
            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The seller is too far", 3000);
                return;
            }

            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Insufficient funds", 3000);
                return;
            }

            MoneySystem.Wallet.Change(seller, price / 20);
            Fractions.Stocks.fracStocks[6].Money += Convert.ToInt32(price * 0.95);
            GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", price, $"buyGunlic({Main.Players[seller].UUID})");
            GameLog.Money($"frac(6)", $"player({Main.Players[seller].UUID})", price / 20, $"sellGunlic({Main.Players[player].UUID})");

            Main.Players[player].Licenses[6] = true;
            Dashboard.sendStats(player);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You bought a license for weapon from the player ({seller.Value}) for {price}$", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) I bought a weapon license", 3000);
        }

        public static void playerTakeoffMask(Player player, Player target)
        {
            if (player.IsInVehicle || target.IsInVehicle) return;

            if (!target.HasData("IS_MASK") || !target.GetData<bool>("IS_MASK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has no mask", 3000);
                return;
            }

            var maskItem = nInventory.Items[Main.Players[target].UUID].FirstOrDefault(i => i.Type == ItemType.Mask && i.IsActive);
            nInventory.Remove(target, maskItem);
            Customization.CustomPlayerData[Main.Players[target].UUID].Clothes.Mask = new ComponentItem(0, 0);
            if (maskItem != null) Items.onDrop(player, maskItem, null);

            Customization.SetMask(target, 0, 0);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You broke the mask from the player ({target.Value})", 3000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Tedded with you a mask", 3000);
            Commands.RPChat("me", player, " Tedpid Mask S. {name}", target);
        }
        #endregion

        #region crimeCommands
        public static void robberyTarget(Player player, Player target)
        {
            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (!target.GetData<bool>("CUFFED") && !target.HasData("HANDS_UP"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player must be connected or with raised hands", 3000);
                return;
            }

            if (!player.HasData("IS_MASK") || !player.GetData<bool>("IS_MASK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Robbery is possible only in mask", 3000);
                return;
            }

            if (Main.Players[target].LVL < 2 || Main.Players[target].Money <= 1000 || (target.HasData("NEXT_ROB") && DateTime.Now < target.GetData<DateTime>("NEXT_ROB")))
            {
                Commands.RPChat("me", player, "Pretty shaking {name}, did not find anything", target);
                return;
            }

            var max = (Main.Players[target].Money >= 3000) ? 3000 : Convert.ToInt32(Main.Players[target].Money) - 200;
            var min = (max - 200 < 0) ? max : max - 200;

            var found = Main.rnd.Next(min, max + 1);
            MoneySystem.Wallet.Change(target, -found);
            MoneySystem.Wallet.Change(player, found);
            GameLog.Money($"player({Main.Players[target].UUID})", $"player({Main.Players[player].UUID})", found, $"robbery");
            target.SetData("NEXT_ROB", DateTime.Now.AddMinutes(60));

            Commands.RPChat("me", player, "Pretty oblastic {name}" + $", found ${found}", target);
        }
        public static void playerChangePocket(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "pocket")) return;
            if (player.IsInVehicle) return;
            if (target.IsInVehicle) return;

            if (target.HasData("HEAD_POCKET"))
            {
                target.ClearAccessory(1);
                target.SetClothes(1, 0, 0);

                Trigger.ClientEvent(target, "setPocketEnabled", false);
                target.ResetData("HEAD_POCKET");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You shot a bag from the player ({target.Value})", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) took off your bag", 3000);
                Commands.RPChat("me", player, "Removed bag {name}", target);
            }
            else
            {
                if (nInventory.Find(Main.Players[player].UUID, ItemType.Pocket) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have no bags", 3000);
                    return;
                }

                target.SetAccessories(1, 24, 2);
                target.SetClothes(1, 56, 1);

                Trigger.ClientEvent(target, "setPocketEnabled", true);
                target.SetData("HEAD_POCKET", true);

                nInventory.Remove(player, ItemType.Pocket, 1);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You put the bag on the player ({target.Value})", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) put on you bag", 3000);
                Commands.RPChat("me", player, "Puting a bag on {name}", target);
            }
        }
        #endregion
        
        #region EMS commands
        public static void giveMedicalLic(Player player, Player target)
        {
            if (!Manager.canUseCommand(player, "givemedlic")) return;

            if (Main.Players[target].Licenses[7])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player already has honey.map", 3000);
                return;
            }

            Main.Players[target].Licenses[7] = true;
            GUI.Dashboard.sendStats(target);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You issued a player {target.Name} Medical map", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} gave you a medical map", 3000);
        }
        public static void sellMedKitToTarget(Player player, Player target, int price)
        {
            if (Manager.canUseCommand(player, "medkit"))
            {
                if (!player.GetData<bool>("ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have to start working day", 3000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (item == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must take first-aid kits from the warehouse", 3000);
                    return;
                }
                if (price < 500 || price > 1500)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must set the price of $ 500 to $ 1500", 3000);
                    return;
                }
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                    return;
                }
                if (Main.Players[target].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has so much money", 3000);
                    return;
                }
                Trigger.ClientEvent(target, "openDialog", "PAY_MEDKIT", $"Medic ({player.Value}) suggested buying you a first-aid kit for ${price}.");
                target.SetData("SELLER", player);
                target.SetData("PRICE", price);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered to buy a player ({target.Value}) first aid kit {price}$", 3000);
            }
        }

        public static void acceptEMScall(Player player, Player target)
        {
            if (Manager.canUseCommand(player, "accept"))
            {
                if (!player.GetData<bool>("ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You did not start working day", 3000);
                    return;
                }
                if (!target.HasData("IS_CALL_EMS"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player did not cause an ambulance", 3000);
                    return;
                }
                Trigger.ClientEvent(player, "createWaypoint", target.Position.X, target.Position.Y);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Medic ({player.Value}) Accepted your call", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You took a player call ({target.Value})", 3000);
                target.ResetData("IS_CALL_EMS");
                return;
            }
        }

        public static void healTarget(Player player, Player target, int price)
        {
            if (Manager.canUseCommand(player, "heal"))
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                    return;
                }
                if (price < 50 || price > 400)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must set the price of $ 50 to $ 400", 3000);
                    return;
                }
                if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.IsPlayerInAnyVehicle(target))
                {
                    var pveh = player.Vehicle;
                    var tveh = target.Vehicle;
                    Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(pveh);
                    if (veh.GetData<string>("ACCESS") != "FRACTION" || veh.GetData<string>("TYPE") != "EMS" || !veh.HasData("CANMEDKITS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not in the emission carriage", 3000);

                        return;
                    }
                    if (pveh != tveh)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player sits in another car", 3000);
                        return;
                    }
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Medic ({player.Value}) suggested treatment for ${price}");

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered treatment to the player ({target.Value}) for {price}$", 3000);
                    return;
                }
                else if (player.GetData<bool>("IN_HOSPITAL") && target.GetData<bool>("IN_HOSPITAL"))
                {
                    target.SetData("SELLER", player);
                    target.SetData("PRICE", price);
                    Trigger.ClientEvent(target, "openDialog", "PAY_HEAL", $"Медик ({player.Value}) suggested treatment for ${price}");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered treatment to the player ({target.Value}) for {price}$", 3000);
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a hospital or ambulance", 3000);
                    return;
                }
            }
        }

        #endregion

    }
}