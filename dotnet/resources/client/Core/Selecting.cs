﻿using GTANetworkAPI;
using System;
using NeptuneEvo.GUI;
using NeptuneEvo.Houses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Redage.SDK;

namespace NeptuneEvo.Core
{
    class Selecting : Script
    {
        private static nLog Log = new nLog("Selecting");

        [RemoteEvent("oSelected")]
        public static void objectSelected(Player player, GTANetworkAPI.Object entity)
        {
            try
            {
                // var entity = (GTANetworkAPI.Object)arguments[0]; // error "Object referance not set to an instance of an object"
                if (entity == null || player == null || !Main.Players.ContainsKey(player)) return;
                //  if (entity.GetSharedData<bool>("PICKEDT") == true)
                if (player.HasData("PICKEDT") && player.GetData<bool>("PICKEDT") == true) //вот эту дичь ебался долго!!!
                {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[PICKUP-ITEMS-EXPLOIT] {player.Name} ({player.Value}) ");
                    return;
                }
                entity.SetSharedData("PICKEDT", true);
                var objType = entity.GetSharedData<string>("TYPE");
                switch (objType)
                {
                    case "DROPPED":
                        {
                            if (player.HasData("isRemoveObject"))
                            {
                                NAPI.Task.Run(() => {
                                    try
                                    {
                                        NAPI.Entity.DeleteEntity(entity);
                                    }
                                    catch { }
                                });
                                player.ResetData("isRemoveObject");
                                return;
                            }

                            var id = entity.GetData<int>("ID");
                            if (Items.InProcessering.Contains(id))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                return;
                            }
                            Items.InProcessering.Add(id);

                            nItem item = NAPI.Data.GetEntityData(entity, "ITEM");
                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough places in inventory", 3000);
                                Items.InProcessering.Remove(id);
                                return;
                            }

                            var tryAdd = nInventory.TryAdd(player, item);
                            if (tryAdd == -1 || (tryAdd > 0 && nInventory.WeaponsItems.Contains(item.Type)))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough places in inventory", 3000);
                                Items.InProcessering.Remove(id);
                                return;
                            }
                            else if (tryAdd > 0)
                            {
                                entity.SetSharedData("PICKEDT", false);
                                nInventory.Add(player, new nItem(item.Type, item.Count - tryAdd, item.Data));
                                GameLog.Items($"ground", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), item.Count - tryAdd, $"{item.Data}");
                                item.Count = tryAdd;
                                entity.SetData("ITEM", item);
                                Items.InProcessering.Remove(id);
                            }
                            else
                            {
                                NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                                nInventory.Add(player, item);
                                GameLog.Items($"ground", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), item.Count, $"{item.Data}");
                            }
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                        }
                    case "WeaponSafe":
                    case "SubjectSafe":
                    case "ClothesSafe":
                        {
                            entity.SetSharedData("PICKEDT", false);
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                            if (house == null) return;
                            if (!house.Owner.Equals(player.Name))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Only the owner of the house can use furniture..", 3000);
                                return;
                            }
                            var furnID = NAPI.Data.GetEntityData(entity, "ID");
                            HouseFurniture furniture = FurnitureManager.HouseFurnitures[houseID][furnID];
                            var items = FurnitureManager.FurnituresItems[houseID][furnID];
                            if (items == null) return;
                            player.SetData("OpennedSafe", furnID);
                            player.SetData("OPENOUT_TYPE", FurnitureManager.SafesType[furniture.Name]);
                            Dashboard.OpenOut(player, items, furniture.Name, FurnitureManager.SafesType[furniture.Name]);
                            return;
                        }
                    case "MoneyBag":
                        {
                            if (player.HasData("HEIST_DRILL") || NAPI.Data.HasEntityData(player, "HAND_MONEY"))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You already have a bag", 3000);
                                return;
                            }

                            var money = NAPI.Data.GetEntityData(entity, "MONEY_IN_BAG");

                            player.SetClothes(5, 45, 0);
                            var item = new nItem(ItemType.BagWithMoney, 1, $"{money}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            Dashboard.sendItems(player);
                            player.SetData("HAND_MONEY", true);
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                        }
                    case "DrillBag":
                        {
                            if (player.HasData("HEIST_DRILL") || player.HasData("HAND_MONEY"))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You already have a drill or money in your hands", 3000);
                                return;
                            }

                            player.SetClothes(5, 41, 0);
                            nInventory.Add(player, new nItem(ItemType.BagWithDrill));
                            player.SetData("HEIST_DRILL", true);

                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                        }
                }
            }
            catch (Exception e) { Log.Write($"oSelected/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("vehicleSelected")]
        public static void vehicleSelected(Player player, params object[] arguments)
        {
            try
            {
                var vehicle = (Vehicle)arguments[0];
                int index = (int)arguments[1];
                if (vehicle == null || player.Position.DistanceTo(vehicle.Position) > 5)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The car is far from you", 3000);
                    return;
                }
                switch (index)
                {
                    case 0:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can not open / close the hood, being in the car", 3000);
                            return;
                        }
                        if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorHood) == DoorState.DoorClosed)
                        {
                            if (VehicleStreaming.GetLockState(vehicle))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can not open the hood while the machine is closed", 3000);
                                return;
                            }
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorHood, DoorState.DoorOpen);
                        }
                        else VehicleStreaming.SetDoorState(vehicle, DoorID.DoorHood, DoorState.DoorClosed);
                        return;
                    case 1:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You cannot open / close the trunk, being in the car", 3000);
                            return;
                        }
                        if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorOpen)
                        {
                            Commands.RPChat("me", player, $"Closed the trunk");
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorTrunk, DoorState.DoorClosed);
                            foreach (var p in Main.Players.Keys.ToList())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.Close(p);
                            }
                        }
                        else
                        {
                            if (vehicle.HasData("ACCESS") && (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE"))
                            {
                                var access = VehicleManager.canAccessByNumber(player, vehicle.NumberPlate);
                                if (!access)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have no keys from this transport", 3000);
                                    return;
                                }
                            }
                            if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "FRACTION" && vehicle.GetData<int>("FRACTION") != Main.Players[player].FractionID)
                            {
                                if (Main.Players[player].FractionID != 7 && Main.Players[player].FractionID != 9)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can not open the trunk from this car", 3000);
                                    return;
                                }
                            }
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorTrunk, DoorState.DoorOpen);
                            Commands.RPChat("me", player, $"Opened trunk");
                        }
                        return;
                    case 2:
                        VehicleManager.ChangeVehicleDoors(player, vehicle);
                        return;
                    case 3:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can not open the inventory while in the car", 3000);
                            return;
                        }
                        if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" || vehicle.Class == 13 || vehicle.Class == 8)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This vehicle does not support inventory", 3000);
                            return;
                        }
                        if (Main.Players[player].AdminLVL == 0 && VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorClosed)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can not open the car inventory until the trunk is closed", 3000);
                            return;
                        }
                        if (vehicle.GetData<bool>("BAGINUSE") == true)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Wait until another person stops using the trunk.", 3000);
                            return;
                        }
                        vehicle.SetData("BAGINUSE", true);
                        GUI.Dashboard.OpenOut(player, vehicle.GetData<List<nItem>>("ITEMS"), "Trunk", 2);
                        player.SetData("SELECTEDVEH", vehicle);
                        return;
                    case 4:
                        CarMarket.vehicleSelected(player, vehicle);
                        return;
                }
            }
            catch (Exception e) { Log.Write("vSelected: " + e.Message, nLog.Type.Error); }

        }

        [RemoteEvent("pSelected")]
        public static void playerSelected(Player player, params object[] arguments)
        {
            try
            {
                var target = (Player)arguments[0];
                if (target == null || player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The player is far from you", 3000);
                    return;
                }
                player.SetData("SELECTEDPLAYER", target);

                if (arguments.Length == 1) return;
                var action = arguments[1].ToString();
                switch (action)
                {
                    case "Shake hands":
                        if (player.IsInVehicle) return;
                        playerHandshakeTarget(player, target);
                        return;
                    case "To lead":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.targetFollowPlayer(player, target);
                        return;
                    case "Rob":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.robberyTarget(player, target);
                        return;
                    case "Let go":
                        if (player.IsInVehicle) return;
                        if (!target.HasData("FOLLOWING"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This player nobody drags", 3000);
                            return;
                        }
                        if (!player.HasData("FOLLOWER") || player.GetData<Player>("FOLLOWER") != target)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This player drags someone else", 3000);
                            return;
                        }
                        Fractions.FractionCommands.unFollow(player, target);
                        return;
                    case "Wake":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in handcuffs", 3000);
                                return;
                            }

                            var items = nInventory.Items[Main.Players[target].UUID];
                            List<string> itemNames = new List<string>();
                            List<string> weapons = new List<string>();
                            foreach (var i in items)
                            {
                                if (nInventory.ClothesItems.Contains(i.Type)) continue;
                                if (nInventory.WeaponsItems.Contains(i.Type))
                                    weapons.Add($"{nInventory.ItemsNames[(int)i.Type]} {i.Data}");
                                else
                                    itemNames.Add($"{nInventory.ItemsNames[(int)i.Type]} x{i.Count}");
                            }

                            var data = new SearchObject();
                            data.Name = target.Name.Replace('_', ' ');
                            data.Weapons = weapons;
                            data.Items = itemNames;

                            Trigger.ClientEvent(player, "newPassport", target, Main.Players[target].UUID);
                            Trigger.ClientEvent(player, "bsearchOpen", JsonConvert.SerializeObject(data));
                            return;
                        }
                    case "View passport":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in handcuffs", 3000);
                                return;
                            }

                            var acc = Main.Players[target];
                            string gender = (acc.Gender) ? "Male" : "Female";
                            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Not";
                            string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID] : "Unemployed";
                            List<object> data = new List<object>
                            {
                                acc.UUID,
                                acc.FirstName,
                                acc.LastName,
                                acc.CreateDate.ToString("dd.MM.yyyy"),
                                gender,
                                fraction,
                                work
                            };
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                            Trigger.ClientEvent(player, "passport", json);
                            Trigger.ClientEvent(player, "newPassport", target, acc.UUID);
                        }
                        return;
                    case "View licenses":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not in handcuffs", 3000);
                                return;
                            }

                            var acc = Main.Players[target];
                            string gender = (acc.Gender) ? "Male" : "Female";

                            var lic = "";
                            for (int i = 0; i < acc.Licenses.Count; i++)
                                if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                            if (lic == "") lic = "Absent";

                            List<string> data = new List<string>
                            {
                                acc.FirstName,
                                acc.LastName,
                                acc.CreateDate.ToString("dd.MM.yyyy"),
                                gender,
                                lic
                            };

                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                            Trigger.ClientEvent(player, "licenses", json);
                        }
                        return;
                    case "Separate weapons":
                        if (player.IsInVehicle) return;
                        playerTakeGuns(player, target);
                        return;
                    case "Has illegal":
                        if (player.IsInVehicle) return;
                        playerTakeIlleagal(player, target);
                        return;
                    case "Sell a first-aid kit":
                        Trigger.ClientEvent(player, "openInput", "Sell a first-aid kit", "Price $$$", 4, "player_medkit");
                        return;
                    case "Suggest treatment":
                        if (player.IsInVehicle) return;
                        Trigger.ClientEvent(player, "openInput", "Suggest treatment", "Price $$$", 4, "player_heal");
                        return;
                    case "Cure":
                        if (player.IsInVehicle) return;
                        playerHealTarget(player, target);
                        return;
                    case "Sell a car":
                        VehicleManager.sellCar(player, target);
                        return;
                    case "Sell a house":
                        House house = HouseManager.GetHouse(player, true);
                        if (house == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have a home", 3000);
                            return;
                        }
                        Trigger.ClientEvent(player, "openInput", "Sell a house", "Price $$$", 8, "player_offerhousesell");
                        return;
                    case "Take up to the house":
                        HouseManager.InviteToRoom(player, target);
                        return;
                    case "Invite to the house":
                        HouseManager.InvitePlayerToHouse(player, target);
                        return;
                    case "Give money":
                        if (Main.Players[player].LVL < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Money transfer is available after the first level", 3000);
                            return;
                        }
                        Trigger.ClientEvent(player, "openInput", "Give money", "Sum $$$", 4, "player_givemoney");
                        return;
                    case "Offer exchange":
                        target.SetData("OFFER_MAKER", player);
                        target.SetData("REQUEST", "OFFER_ITEMS");
                        target.SetData("IS_REQUESTED", true);
                        Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Suggested for you to exchange objects.Y / N - take / reject", 3000);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered a player ({target.Value}) Exchange objects.", 3000);
                        return;
                    case "Bag":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.playerChangePocket(player, target);
                        return;
                    case "Tear a mask":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.playerTakeoffMask(player, target);
                        return;
                    case "Write a fine":
                        if (player.IsInVehicle) return;
                        player.SetData("TICKETTARGET", target);
                        Trigger.ClientEvent(player, "openInput", "Write a fine (sum)", "Amount from 0 to 7000 $", 4, "player_ticketsum");
                        return;
                }
            }
            catch (Exception e) { Log.Write($"pSelected: " + e.ToString(), nLog.Type.Error); }
        }

        public static void playerTransferMoney(Player player, string arg)
        {
            if (Main.Players[player].LVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Money transfer will be available from 1 level.", 3000);
                return;
            }
            try
            {
                Convert.ToInt32(arg);
            }
            catch
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Enter the correct data", 3000);
                return;
            }
            var amount = Convert.ToInt32(arg);
            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Enter the correct data", 3000);
                return;
            }
            Player target = player.GetData<Player>("SELECTEDPLAYER");
            if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                return;
            }
            if (amount > Main.Players[player].Money)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have enough money", 3000);
                return;
            }
            if (player.HasData("NEXT_TRANSFERM") && DateTime.Now < player.GetData<DateTime>("NEXT_TRANSFERM") && Main.Players[player].AdminLVL == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "From the moment of the last transfer of money a little time passed.", 3000);
                return;
            }
            player.SetData("NEXT_TRANSFERM", DateTime.Now.AddMinutes(1));
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) Passing you {amount}$", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have passed the player ({target.Value}) {amount}$", 3000);
            MoneySystem.Wallet.Change(target, amount);
            MoneySystem.Wallet.Change(player, -amount);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", amount, $"transfer");
            Commands.RPChat("me", player, $"Pass {amount}$ " + "{name}", target);
        }
        public static void playerHealTarget(Player player, Player target)
        {
            try
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (item == null || item.Count < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have no first aid kit", 3000);
                    return;
                }

                nInventory.Remove(player, ItemType.HealthKit, 1);
                if (target.HasData("IS_DYING"))
                {
                    player.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_a", 39);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You started reanimation of the player ({target.Value})", 3000);
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) He began to reanimate you", 3000);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            player.StopAnimation();
                            NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 0.5));

                            if (Main.Players[player].FractionID != 8)
                            {
                                var random = new Random();
                                if (random.Next(0, 11) <= 5)
                                {
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({target.Value}) Slightly flipped.You did not reach it to reanimate", 3000);
                                    return;
                                }
                            }
                            else
                            {
                                if (!target.HasData("NEXT_DEATH_MONEY") || DateTime.Now > target.GetData<DateTime>("NEXT_DEATH_MONEY"))
                                {
                                    MoneySystem.Wallet.Change(player, 150);
                                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", 150, $"revieve({Main.Players[target].UUID})");
                                    target.SetData("NEXT_DEATH_MONEY", DateTime.Now.AddMinutes(15));
                                }
                            }

                            target.StopAnimation();
                            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 0.5));
                            target.SetSharedData("InDeath", false);
                            Trigger.ClientEvent(target, "DeathTimer", false);
                            target.Health = 50;
                            target.ResetData("IS_DYING");
                            target.ResetSharedData("IS_DYING");
                            Main.Players[target].IsAlive = true;
                            Main.OffAntiAnim(target);
                            if (target.HasData("DYING_TIMER"))
                            {
                                //Main.StopT(target.GetData<string>("DYING_TIMER"), "timer_18");
                                Timers.Stop(target.GetData<string>("DYING_TIMER"));
                                target.ResetData("DYING_TIMER");
                            }
                            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) reanimated you", 3000);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have reanimated the player ({target.Value})", 3000);

                            if (target.HasData("CALLEMS_BLIP"))
                            {
                                NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
                            }
                            if (target.HasData("CALLEMS_COL"))
                            {
                                NAPI.ColShape.DeleteColShape(target.GetData<ColShape>("CALLEMS_COL"));
                            }
                        }
                        catch (Exception e) { Log.Write("playerHealedtarget: " + e.Message, nLog.Type.Error); }
                    }, 15000);
                }
                else
                {
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) cured you using the first aid kit", 3000);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You cured players ({target.Value}) Using the aid kit", 3000);
                    target.Health = 100;
                }
                return;
            }
            catch (Exception e) { Log.Write("playerHealTarget: " + e.Message); }
        }
        public static void playerTakeGuns(Player player, Player target)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                return;
            }
            if (!Fractions.Manager.canUseCommand(player, "takeguns")) return;
            Weapons.RemoveAll(target, true);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) I seized all the weapons", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You seized all weapons from the player ({target.Value})", 3000);
            return;
        }
        public static void playerTakeIlleagal(Player player, Player target)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                return;
            }
            var matItem = nInventory.Find(Main.Players[target].UUID, ItemType.Material);
            var drugItem = nInventory.Find(Main.Players[target].UUID, ItemType.Drugs);
            var materials = (matItem == null) ? 0 : matItem.Count;
            var drugs = (drugItem == null) ? 0 : drugItem.Count;
            if (materials < 1 && drugs < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has nothing forbidden", 3000);
                return;
            }
            nInventory.Remove(target, ItemType.Material, materials);
            nInventory.Remove(target, ItemType.Drugs, drugs);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) I seized forbidden items", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You were withdrawn from the player {target.Value} Forbidden subjects", 3000);
            return;
        }
        public static void playerOfferChangeItems(Player player)
        {
            if (!Main.Players.ContainsKey(player) || !player.HasData("OFFER_MAKER") || !Main.Players.ContainsKey(player.GetData<Player>("OFFER_MAKER"))) return;
            Player offerMaker = player.GetData<Player>("OFFER_MAKER");
            if (Main.Players[player].ArrestTime > 0 || Main.Players[offerMaker].ArrestTime > 0)
            {
                player.ResetData("OFFER_MAKER");
                return;
            }
            if (player.Position.DistanceTo(offerMaker.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The selected player is too far", 3000);
                return;
            }

            player.SetData("CHANGE_WITH", offerMaker);
            offerMaker.SetData("CHANGE_WITH", player);

            GUI.Dashboard.OpenOut(player, new List<nItem>(), offerMaker.Name, 5);
            GUI.Dashboard.OpenOut(offerMaker, new List<nItem>(), player.Name, 5);

            player.ResetData("OFFER_MAKER");
        }
        public static void playerHandshakeTarget(Player player, Player target)
        {
            if ((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false)
            {
                if ((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false)
                {
                    target.SetData("HANDSHAKER", player);
                    target.SetData("REQUEST", "HANDSHAKE");
                    target.SetData("IS_REQUESTED", true);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) Wants to shake your hand.Y / N - take / reject", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered a player ({target.Value}) shake hands.", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "It is impossible to shake the hand player at the moment", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "It is impossible to shake the hand player at the moment", 3000);
        }
        public static void hanshakeTarget(Player player)
        {
            if (!Main.Players.ContainsKey(player) || !player.HasData("HANDSHAKER") || !Main.Players.ContainsKey(player.GetData<Player>("HANDSHAKER"))) return;
            Player target = player.GetData<Player>("HANDSHAKER");
            if ((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false)
            {
                if ((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false)
                {
                    player.PlayAnimation("mp_ped_interaction", "handshake_guy_a", 39);
                    target.PlayAnimation("mp_ped_interaction", "handshake_guy_a", 39);

                    Trigger.ClientEvent(player, "newFriend", target);
                    Trigger.ClientEvent(target, "newFriend", player);

                    Main.OnAntiAnim(player);
                    Main.OnAntiAnim(target);

                    NAPI.Task.Run(() => { try { Main.OffAntiAnim(player); Main.OffAntiAnim(target); player.StopAnimation(); target.StopAnimation(); } catch { } }, 4500);
                }
            }
        }
        internal class SearchObject
        {
            public string Name { get; set; }
            public List<string> Weapons { get; set; }
            public List<string> Items { get; set; }
        }

        [RemoteEvent("aSelected")]
        public static void animationSelected(Player player, int category, int animation)
        {
            try
            {
                if (player.HasData("AntiAnimDown") || player.HasData("FOLLOWING") || player.IsInVehicle
                || Main.Players[player].ArrestTime > 0 || Main.Players[player].DemorganTime > 0) return;

                if (category == -1)
                {
                    player.ResetData("HANDS_UP");
                    player.StopAnimation();
                    if (player.HasData("LastAnimFlag") && player.GetData<int>("LastAnimFlag") == 39)
                        NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 0.2));
                    return;
                }
                if (category == 6)
                { // Лицевые эмоции
                    player.SetSharedData("playermood", animation);
                    NAPI.ClientEvent.TriggerClientEventInRange(player.Position, 250, "Player_SetMood", player, animation);
                    return;
                }
                else if (category == 12)
                { // Стили походки
                    player.SetSharedData("playerws", animation);
                    NAPI.ClientEvent.TriggerClientEventInRange(player.Position, 250, "Player_SetWalkStyle", player, animation);
                    return;
                }
                else
                {
                    if (animation >= AnimList[category].Count) return;
                    player.PlayAnimation(AnimList[category][animation].Dictionary, AnimList[category][animation].Name, AnimList[category][animation].Flag);
                    if (category == 0 && animation == 0) NAPI.Entity.SetEntityPosition(player, player.Position - new Vector3(0, 0, 0.3));

                    if (AnimList[category][animation].Dictionary == "random@arrests@busted" && AnimList[category][animation].Name == "idle_c") player.SetData("HANDS_UP", true);

                    player.SetData("LastAnimFlag", AnimList[category][animation].Flag);
                    if (AnimList[category][animation].StopDelay != -1)
                    {
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && !player.HasData("AntiAnimDown") && !player.HasData("FOLLOWING"))
                                {
                                    player.StopAnimation();
                                }
                            }
                            catch { }
                        }, AnimList[category][animation].StopDelay);
                    }
                }
            }
            catch (Exception e) { Log.Write("aSelected: " + e.Message, nLog.Type.Error); }
        }

        public static List<List<Animation>> AnimList = new List<List<Animation>>()
        {
            new List<Animation>()
            {
                new Animation("amb@world_human_picnic@female@base", "base", 39),
                new Animation("amb@medic@standing@tendtodead@base", "base", 39),
                new Animation("amb@world_human_stupor@male@base", "base", 39),
                new Animation("amb@world_human_sunbathe@male@back@base", "base", 39),
                new Animation("missfinale_c1@", "lying_dead_player0", 39),
                new Animation("amb@medic@standing@kneel@base", "base", 39),
                new Animation("mp_safehouse", "lap_dance_player", 39),
                new Animation("misstrevor2", "gang_chatting_idle02_a", 39),
            },
            new List<Animation>()
            {
                new Animation("random@arrests@busted", "idle_c", 49),
                new Animation("amb@medic@standing@timeofdeath@idle_a", "idle_a", 49),
                new Animation("anim@mp_player_intselfiethumbs_up", "idle_a", 49),
                new Animation("anim@mp_player_intuppersalute", "idle_a", 49),

                new Animation("anim@mp_player_intupperyou_loco", "idle_a", 49),
                new Animation("anim@mp_player_intupperwave", "idle_a", 49),
                new Animation("anim@mp_player_intupperv_sign", "idle_a", 49),

            },
            new List<Animation>()
            {
                new Animation("amb@world_human_yoga@female@base", "base_a", 39),
                new Animation("amb@world_human_yoga@male@base", "base_b", 39),
                new Animation("amb@world_human_sit_ups@male@base", "base", 39),
                new Animation("amb@world_human_push_ups@male@base", "base", 39),
                new Animation("rcmcollect_paperleadinout@", "meditiate_idle", 39),
            },
            new List<Animation>()
            {
                new Animation("anim@mp_player_intselfiethe_bird", "idle_a", 49),
                new Animation("anim@mp_player_intincardockstd@ps@", "idle_a", 49),
                new Animation("anim@mp_player_intuppernose_pick", "idle_a", 49),
                new Animation("anim@mp_player_intupperfinger", "idle_a", 49),
                new Animation("mp_player_intfinger", "mp_player_int_finger", 39),
            },
            new List<Animation>()
            {
                new Animation("amb@world_human_cop_idles@male@base", "base", 39),
                new Animation("anim@mp_player_intupperknuckle_crunch", "idle_a", 49),
                new Animation("anim@amb@nightclub@peds@", "rcmme_amanda1_stand_loop_cop", 39),
                new Animation("anim@amb@nightclub@peds@", "mini_strip_club_idles_bouncer_go_away_go_away", 39),
                new Animation("anim@amb@nightclub@peds@", "mini_strip_club_idles_bouncer_stop_stop", 39),
                new Animation("anim@amb@nightclub@peds@", "amb_world_human_muscle_flex_arms_in_front_base", 39),
                new Animation("amb@world_human_muscle_flex@arms_at_side@base", "base", 39),
            },
            new List<Animation>()
            {
                new Animation("amb@world_human_partying@female@partying_beer@base", "base", 39),
                new Animation("amb@world_human_strip_watch_stand@male_a@idle_a", "idle_c", 39),
                new Animation("mini@strip_club@idles@dj@idle_04", "idle_04", 39),
                new Animation("mini@strip_club@lap_dance@ld_girl_a_song_a_p1", "ld_girl_a_song_a_p1_f", 39),
                new Animation("special_ped@mountain_dancer@monologue_3@monologue_3a", "mnt_dnc_buttwag", 39),
                new Animation("mini@strip_club@private_dance@part1", "priv_dance_p1", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v2_female^1", 39),
            },
            new List<Animation>() // NOT USED
            {
                null,
            },
            new List<Animation>()
            {
                new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v2_female^3", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v2_male^2", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v2_male^4", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v1_female^1", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^1", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^3", 39),
                new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_female^1", 39),
            },
            new List<Animation>()
            {
                new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_13_v2_female^1", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_crotchgrab_laz", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_teapotthrust_laz", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_09_mi_hi_bellydancer_laz", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_10_mi_hi_crotchhold_laz", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_12_mi_hi_bootyshake_laz", 39),
                new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_13_mi_hi_sexualgriding_laz", 39),
            },
            new List<Animation>()
            {
                new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "med_center", 39),
                new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "med_center", 39),
            },
            new List<Animation>()
            {
                new Animation("anim@mp_player_intupperthumbs_up", "idle_a", 49),
                new Animation("anim@mp_player_intupperthumb_on_ears", "idle_a", 49),
                new Animation("anim@mp_player_intuppersurrender", "idle_a", 49),
                new Animation("anim@mp_player_intupperslow_clap", "idle_a", 49),
                new Animation("anim@mp_player_intupperpeace", "idle_a", 49),
                new Animation("anim@mp_player_intupperno_way", "idle_a", 49),
                new Animation("anim@mp_player_intupperjazz_hands", "idle_a", 49),
            },
            new List<Animation>()
            {
                new Animation("anim@mp_player_intupperfind_the_fish", "idle_a", 49),
                new Animation("anim@mp_player_intupperface_palm", "idle_a", 49),
                new Animation("anim@mp_player_intupperchicken_taunt", "idle_a", 49),
                new Animation("anim@mp_player_intselfiedock", "idle_a", 49),
                new Animation("friends@frf@ig_1", "over_here_idle_b", 49),
                new Animation("mp_player_int_upperrock", "mp_player_int_rock", 49),
                new Animation("mp_player_int_upperpeace_sign", "mp_player_int_peace_sign", 49),
            },
            new List<Animation>() // NOT USED
            {
                null,
            },
            new List<Animation>()
            {
                new Animation("amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_a", 39),
                new Animation("amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_c", 39),
                new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_a", 39),
                new Animation("amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b", 39),
            },
        };

        internal class Animation
        {
            public string Dictionary { get; }
            public string Name { get; }
            public int Flag { get; }
            public int StopDelay { get; }

            public Animation(string dict, string name, int flag, int stopDelay = -1)
            {
                Dictionary = dict;
                Name = name;
                Flag = flag;
                StopDelay = stopDelay;
            }
        }
    }
}
