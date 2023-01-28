using System;
using GTANetworkAPI;
using NeptuneEvo;
using Redage.SDK;

namespace NeptuneEvo.Families
{
    class Commands
    {
        private static readonly nLog Log = new nLog("FamilyCommands");
        public static void InviteToFamily(Player sender, Player target)
        {
            try
            {
                if (sender.Position.DistanceTo(target.Position) > 3)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far from you", 3000);
                    return;
                }
                if (Manager.isHaveFamily(target))
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

                target.SetData("INVITEFAMILY", Main.Players[sender].FamilyCID);
                target.SetData("SENDERFAMILY", sender);
                Trigger.ClientEvent(target, "openDialog", "INVITEDTOFAMILY", $"{sender.Name} invited you B. {Family.GetFamilyName(sender)}");

                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You invited to the family {target.Name}", 3000);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void SetFamilyRank(Player sender, Player target, int newrank)
        {
            try
            {
                if (newrank <= 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, "It is impossible to install a negative or zero rank", 3000);
                    return;
                }
                int senderlvl = Main.Players[sender].FamilyRank;
                int playerlvl = Main.Players[target].FamilyRank;
                string senderfamily = Main.Players[sender].FamilyCID;
                if (!Manager.isHaveFamily(target, senderfamily)) return;

                if (newrank >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't boost before that rank.", 3000);
                    return;
                }
                if (playerlvl > senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can not change the rank of this player", 3000);
                    return;
                };
                if (sender == target)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't change your rank", 3000);
                    return;
                }
                Family family = Family.GetFamilyToCid(senderfamily);
                int memberindex = family.Players.FindIndex(x => x.Name == target.Name);

                Main.Players[target].FamilyRank = newrank;
                Member.LoadMembers(target, Main.Players[target].FamilyCID, Main.Players[target].FamilyRank);
                int uuid = Main.Players[target].UUID;
                if (Manager.AllMembers.ContainsKey(uuid))
                {
                    Manager.AllMembers[uuid].FamilyRank = newrank;
                    Manager.AllMembers[uuid].FamilyRankName = Ranks.GetFamilyRankName(senderfamily, newrank);
                }
                if (memberindex > -1)
                {
                    family.Players[memberindex].FamilyRank = newrank;
                    family.Players[memberindex].FamilyRankName = Ranks.GetFamilyRankName(senderfamily, newrank);
                }
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Now you {Manager.Members[target].FamilyRankName} in the fraction ", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"You changed player rank {target.Name} on {Manager.Members[target].FamilyRankName}", 3000);

                Main.Players[target].Save(target).Wait();
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }

        internal static void DeleteFamilyMember(Player sender, Player target, string reason)
        {
            try
            {
                Family family = Family.GetFamilyToCid(Main.Players[sender].FamilyCID);
                string senderfamily = family.FamilyCID;
                int senderlvl = Main.Players[sender].FamilyRank;
                int playerlvl = Main.Players[target].FamilyRank;

                if (!Manager.isHaveFamily(target, senderfamily)) return;

                if (playerlvl >= senderlvl)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't piss this player", 3000);
                    return;
                };
                if (sender == target)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't piss yourself", 3000);
                    return;
                }
                Member.UnLoadMember(target);
                string msg = reason == null || reason == "" ? "Without any reasons" : $"Because of: {reason}";
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"You were expelled out {family.Name}. {msg}", 3000);
                Notify.Send(sender, NotifyType.Warning, NotifyPosition.BottomCenter, $"You kicked out the player {target.Name} of {family.Name}", 3000);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
            
        }
    }
}
