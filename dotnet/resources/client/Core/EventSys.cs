using GTANetworkAPI;
using Redage.SDK;
using System;
using System.Collections.Generic;

namespace NeptuneEvo.Core
{
    class EventSys : Script
    {
        private class CustomEvent
        {
            public string Name { get; set; }
            public Player Admin { get; set; }
            public Vector3 Position { get; set; }
            public uint Dimension { get; set; }
            public ushort MembersLimit { get; set; }
            public Player Winner { get; set; }
            public uint Reward { get; set; }
            public ColShape Zone { get; set; } = null;
            public byte EventState { get; set; } = 0; // 0 - МП не создано, 1 - Создано, но не началось, 2 - Создано и началось.
            public DateTime Started { get; set; }
            public uint RewardLimit { get; set; } = 0;
            public List<Player> EventMembers = new List<Player>();
            public List<Vehicle> EventVehicles = new List<Vehicle>();
        }
        private static CustomEvent AdminEvent = new CustomEvent(); // Одновременно можно будет создать только одно мероприятие.
        private static nLog Log = new nLog("EventSys");
        private static Config config = new Config("EventSys");

        public static void Init()
        {
            AdminEvent.RewardLimit = config.TryGet<uint>("RewardLimit", 20000);
        }
        
        private void DeleteClientFromEvent(Player player)
        {
            AdminEvent.EventMembers.Remove(player);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (AdminEvent.EventState != 0)
            {
                if (AdminEvent.EventMembers.Contains(player))
                {
                    DeleteClientFromEvent(player);
                    if (AdminEvent.EventState == 2)
                    {
                        if (AdminEvent.EventMembers.Count == 0) CloseAdminEvent(AdminEvent.Admin, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColshape(ColShape colshape, Player player)
        {
            if (AdminEvent.EventState == 2)
            { // Проверяет только после начала мп, когда телепорт закрыт
                if (AdminEvent.Zone != null)
                {
                    if (AdminEvent.EventMembers.Contains(player))
                    {
                        if (colshape == AdminEvent.Zone)
                        {
                            player.Health = 0;
                            player.Armor = 0;
                            player.SendChatMessage("You left the territory of the event.");
                        }
                    }
                }
            }
        }

        [Command("createmp", "Use: /createmp [Limit of participants] [Radius of zone] [event title]", GreedyArg = true)]
        public void CreateEvent(Player player, ushort members, float radius, string eventname)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "createmp")) return;
                if (AdminEvent.EventState == 0)
                {
                    if (eventname.Length < 50)
                    {
                        if (radius >= 10) AdminEvent.Zone = NAPI.ColShape.CreateSphereColShape(player.Position, radius, player.Dimension);
                        AdminEvent.EventState = 1;
                        AdminEvent.EventMembers = new List<Player>();
                        AdminEvent.EventVehicles = new List<Vehicle>();
                        if (members >= NAPI.Server.GetMaxPlayers()) members = 0;
                        AdminEvent.Started = DateTime.Now;
                        AdminEvent.MembersLimit = members;
                        AdminEvent.Name = eventname;
                        AdminEvent.Winner = null;
                        AdminEvent.Position = player.Position;
                        AdminEvent.Dimension = player.Dimension;
                        AdminEvent.Admin = player;
                        AddAdminEventLog();
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Dear players, event will soon begin '" + eventname + "'!");
                        if (members != 0) NAPI.Chat.SendChatMessageToAll("!{#A87C33}This event has a limit of participants: " + members + ".");
                        else NAPI.Chat.SendChatMessageToAll("!{#A87C33}The limit of participants is not installed at this event..");
                        if (AdminEvent.Zone != null) NAPI.Chat.SendChatMessageToAll("!{#A87C33}The event is valid in the zone " + radius + "meters from the point of teleport.");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}To teleport to the event, enter the command /mp");
                    }
                    else player.SendChatMessage("Too long name of the event, come up with shorter.");
                }
                else player.SendChatMessage("One event has already been created, you can not create a new while old actively.");
            }
        }

        [Command("startmp")]
        public void StartEvent(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "startmp")) return;
                if (AdminEvent.EventState == 1)
                {
                    if (AdminEvent.EventMembers.Count >= 1)
                    {
                        AdminEvent.EventState = 2;
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Event '" + AdminEvent.Name + "' It began, teleport is closed!");
                        NAPI.Chat.SendChatMessageToAll("!{#A87C33}Players at the event: " + AdminEvent.EventMembers.Count + ".");
                    }
                    else player.SendChatMessage("Unable to launch an event without participants.");
                }
                else player.SendChatMessage("The event is either not created or has already been launched..");
            }
        }

        [Command("stopmp", "Use: /stopmp [ID player] [Prize]")]
        public void MPReward(Player player, int pid, uint reward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "stopmp")) return;
                if (AdminEvent.EventState == 2)
                {
                    if (reward <= AdminEvent.RewardLimit)
                    {
                        if (AdminEvent.Winner == null)
                        {
                            Player target = Main.GetPlayerByID(pid);
                            if (target != null)
                            {
                                if (AdminEvent.EventMembers.Contains(target) || AdminEvent.Admin == target) CloseAdminEvent(target, reward);
                                else player.SendChatMessage("This player was found, but he is not a participant in the event.");
                            }
                            else player.SendChatMessage("Player with such id was not found.");
                        }
                        else player.SendChatMessage("The winner was already appointed.");
                    }
                    else player.SendChatMessage("The award can not exceed the exposed limit: " + AdminEvent.RewardLimit + ".");
                }
                else player.SendChatMessage("The event is either not created or has not yet been launched..");
            }
        }

        [Command("mpveh", "Use: /mpveh [Model name] [Color] [Color] [Number of cars]")]
        public void CreateMPVehs(Player player, string model, byte c1, byte c2, byte count)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpveh")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (count >= 1 && count <= 10)
                    {
                        VehicleHash vehHash = (VehicleHash)NAPI.Util.GetHashKey(model);
                        if (vehHash != 0)
                        {
                            for (byte i = 0; i != count; i++)
                            {
                                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehHash, new Vector3((player.Position.X + (4 * i)), player.Position.Y, player.Position.Z), player.Rotation.Z, c1, c2, "EVENTCAR");
                                vehicle.Dimension = player.Dimension;
                                VehicleStreaming.SetEngineState(vehicle, true);
                                AdminEvent.EventVehicles.Add(vehicle);
                            }
                            AdminEvent.Admin = player;
                        }
                        else player.SendChatMessage("Machines with such title not found in the database.");
                    }
                    else player.SendChatMessage("ЗAnd once you can create from 1 to 10 cars.");
                }
                else player.SendChatMessage("Create transport can only be created before the event is started..");
            }
        }

        [Command("mpreward", "Use: /mpreward [New limit]")]
        public void SetMPReward(Player player, uint newreward)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].AdminLVL >= 6)
                {
                    if (newreward <= 999999)
                    {
                        AdminEvent.RewardLimit = newreward;
                        try
                        {
                            MySQL.Query($"UPDATE `eventcfg` SET `RewardLimit`={newreward}");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You installed limit on " + newreward, 3000);
                        }
                        catch (Exception e)
                        {
                            Log.Write("EXCEPTION AT \"SetMPReward\":\n" + e.ToString(), nLog.Type.Error);
                        }
                    }
                    else player.SendChatMessage("You have introduced too big limit.Maximum possible limit: 999999");
                }
            }
        }

        [Command("mp")]
        public void TpToMp(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].DemorganTime == 0 && Main.Players[player].ArrestTime == 0 && player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false)
                {
                    if (AdminEvent.EventState == 1)
                    {
                        if (!AdminEvent.EventMembers.Contains(player))
                        {
                            if (AdminEvent.MembersLimit == 0 || AdminEvent.EventMembers.Count < AdminEvent.MembersLimit)
                            {
                                AdminEvent.EventMembers.Add(player);
                                player.Position = AdminEvent.Position;
                                player.Dimension = AdminEvent.Dimension;
                                player.SendChatMessage("You were teleported to the event '" + AdminEvent.Name + "'.");
                            }
                            else player.SendChatMessage("Unfortunately, the list of participants is full.");
                        }
                        else player.SendChatMessage("You are already listed in the list of participants..");
                    }
                    else player.SendChatMessage("Teleport is closed.");
                }
                else player.SendChatMessage("Teleport for you unavailable.");
            }
        }

        [Command("mpkick", "Use: /mpkick [ID player]")]
        public void MPKick(Player player, int pid)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpkick")) return;
                if (AdminEvent.EventState == 1)
                {
                    Player target = Main.GetPlayerByID(pid);
                    if (target != null)
                    {
                        if (AdminEvent.EventMembers.Contains(target))
                        {
                            AdminEvent.Admin = player;
                            target.Health = 0;
                            target.Armor = 0;
                            player.SendChatMessage("You kicked out " + target.Name + " from the event.");
                        }
                        else player.SendChatMessage("The player with data ID was found, but he is not a participant in the event.");
                    }
                    else player.SendChatMessage("Player with such id was not found.");
                }
                else player.SendChatMessage("You can drive out the player only after creating the event before.");
            }
        }

        [Command("mphp", "Use: /mphp [Number of HP.]")]
        public void MPHeal(Player player, byte health)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mphp")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (health >= 1 && health <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Player target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerHealth(target, health);
                        }
                        player.SendChatMessage("You have successfully installed the entire member of the MP " + health + " HP.");
                    }
                    else player.SendChatMessage("Number of HP, which can be set, is in the range from 1 to 100.");
                }
                else player.SendChatMessage("Note HP players only before the event.");
            }
        }

        [Command("mpar", "Use: /mpar [number Armor]")]
        public void MPArmor(Player player, byte armor)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpar")) return;
                if (AdminEvent.EventState >= 1)
                {
                    if (armor >= 0 && armor <= 100)
                    {
                        AdminEvent.Admin = player;
                        foreach (Player target in AdminEvent.EventMembers)
                        {
                            NAPI.Player.SetPlayerArmor(target, armor);
                        }
                        player.SendChatMessage("You have successfully installed the entire member of the MP " + armor + " armor.");
                    }
                    else player.SendChatMessage("The number of Armor, which can be set, is in the range from 0 to 100.");
                }
                else player.SendChatMessage("Note Armor Players only before the event.");
            }
        }

        [Command("mpplayers")]
        public void MpPlayerList(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (!Group.CanUseCmd(player, "mpplayers")) return;
                if (AdminEvent.EventState != 0)
                {
                    short memcount = Convert.ToInt16(AdminEvent.EventMembers.Count);
                    if (memcount > 0)
                    {
                        if (memcount <= 15)
                        {
                            foreach (Player target in AdminEvent.EventMembers)
                            {
                                player.SendChatMessage("ID: " + target.Value + " | Name: " + target.Name);
                            }
                            player.SendChatMessage("Players at the event: " + memcount);
                        }
                        else player.SendChatMessage("Players at the event: " + memcount);
                    }
                    else player.SendChatMessage("Players at the event not detected.");
                }
                else player.SendChatMessage("The event has not yet been created.");
            }
        }
        
        private void AddAdminEventLog()
        {
            try
            {
                GameLog.EventLogAdd(AdminEvent.Admin.Name, AdminEvent.Name, AdminEvent.MembersLimit, MySQL.ConvertTime(AdminEvent.Started));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"AddAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void UpdateLastAdminEventLog()
        {
            try
            {
                GameLog.EventLogUpdate(AdminEvent.Admin.Name,AdminEvent.EventMembers.Count,AdminEvent.Winner.Name,AdminEvent.Reward,MySQL.ConvertTime(DateTime.Now),AdminEvent.RewardLimit, AdminEvent.MembersLimit, AdminEvent.Name);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"UpdateLastAdminEventLog\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void CloseAdminEvent(Player winner, uint reward)
        {
            if (AdminEvent.Zone != null)
            {
                AdminEvent.Zone.Delete();
                AdminEvent.Zone = null;
            }
            if (AdminEvent.EventVehicles.Count != 0)
            {
                foreach (Vehicle vehicle in AdminEvent.EventVehicles)
                {
                    vehicle.Delete();
                }
            }
            AdminEvent.Winner = winner;
            AdminEvent.Reward = reward;
            AdminEvent.EventState = 0;
            UpdateLastAdminEventLog();
            NAPI.Chat.SendChatMessageToAll("!{#A87C33}Event '" + AdminEvent.Name + "' ended thanks for participation!");
            if (winner != AdminEvent.Admin)
            {
                if (reward != 0)
                {
                    NAPI.Chat.SendChatMessageToAll("!{#A87C33}Winner " + winner.Name + " Received a prize in the amount of " + reward + "$.");
                    MoneySystem.Wallet.Change(winner, (int)reward);
                }
                else NAPI.Chat.SendChatMessageToAll("!{#A87C33}Winner: " + winner.Name + ".");
            }
        }
    }
}
