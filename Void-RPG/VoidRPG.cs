#region Discalimer
/*  
 *  The plugin has some features that I got from other authors.
 *  I don't claim any ownership over those elements which were made by someone else.
 *  The plugin has been customized to fit our need at Geldar,
 *  and because of this, it's useless for anyone else.
 *  I know timers are shit, and If someone knows a way to keep them after relog, tell me.
*/
#endregion

#region Refs
using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Newtonsoft.Json;
#endregion

namespace VoidRPG
{
    [ApiVersion(1,22)]
    public class VoidRPG : TerrariaPlugin
    {
        #region Info & other things
        public DateTime LastCheck = DateTime.UtcNow;
        public DateTime SLastCheck = DateTime.UtcNow;
        public VPlayer[] Playerlist = new VPlayer[256];
        DateTime DLastCheck = DateTime.UtcNow;
        public TShockAPI.DB.Region Region { get; set; }
        public override string Name
        { get { return "RPG Commands"; } }
        public override string Author
        { get { return "Tygra"; } }
        public override string Description
        { get { return "Geldar VoidRPG Commads"; } }
        public override Version Version
        { get { return new Version(1, 3); } }

        public VoidRPG(Main game)
            : base(game)
        {
            Order = 1;
        }
        #endregion

        #region Initialize
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("geldar.admin", Reloadcfg, "rpgreload"));
            Commands.ChatCommands.Add(new Command(Starter, "starter"));
            Commands.ChatCommands.Add(new Command("geldar.mod", Exban, "exban"));
            Commands.ChatCommands.Add(new Command("geldar.mod", Exui, "exui"));
            Commands.ChatCommands.Add(new Command("geldar.mod", Clearall, "ca"));
            Commands.ChatCommands.Add(new Command(staff, "staff"));
            Commands.ChatCommands.Add(new Command("geldar.level30", Facepalm, "facepalm"));            
            Commands.ChatCommands.Add(new Command("geldar.vip", VIP, "vip"));
            Commands.ChatCommands.Add(new Command("geldar.vip", Buffme, "buffme"));
            Commands.ChatCommands.Add(new Command(Geldar, "geldar"));
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameUpdate.Register(this, Cooldowns);
            if (!Config.ReadConfig())
            {
                TShock.Log.ConsoleError("Config loading failed. Consider deleting it.");
            }
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, Cooldowns);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }

        }
        #endregion

        #region Playerlist Join/Leave
        public void OnJoin(JoinEventArgs args)
        {
            Playerlist[args.Who] = new VPlayer(args.Who);
        }

        public void OnLeave(LeaveEventArgs args)
        {
            Playerlist[args.Who] = null;
        }
        #endregion

        #region Cooldown
        private void Cooldowns(EventArgs args)
        {
            if ((DateTime.UtcNow - LastCheck).TotalMilliseconds >= 1)
            {
                LastCheck = DateTime.UtcNow;
                foreach (var player in Playerlist)
                {
                    if (player == null || player.TSPlayer == null)
                    {
                        continue;
                    }
                    if (player.startercd > 0)
                    {
                        player.startercd--;
                    }
                    if (player.facepalmcd > 0)
                    {
                        player.facepalmcd--;
                    }
                }
            }
        }
        #endregion

        #region Staffcommands

        #region Extended ban
        private void Exban(CommandArgs args)
        {
            if (args.Parameters.Count != 1)
            {
                args.Player.SendErrorMessage("Invalid syntax: /exban \"<player name>\"");
            }
            else
            {
                string text = args.Parameters[0];
                Ban banByName = TShock.Bans.GetBanByName(text, true);
                if (banByName == null)
                {
                    args.Player.SendErrorMessage("No bans by this name were found.");
                }
                else
                {
                    args.Player.SendInfoMessage(string.Concat(new string[] { "Account name: ", banByName.Name, " (", banByName.IP, ")" }));
                    args.Player.SendInfoMessage("Date banned: " + banByName.Date);
                    if (banByName.Expiration != "")
                    {
                        args.Player.SendInfoMessage("Expiration date: " + banByName.Expiration);
                    }
                    args.Player.SendInfoMessage("Banned by: " + banByName.BanningUser);
                    args.Player.SendInfoMessage("Reason: " + banByName.Reason);
                }
            }
        }
        #endregion

        #region Exui
        private void Exui(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                string text = string.Join(" ", args.Parameters);
                if (text != null & text != "")
                {
                    User userByName = TShock.Users.GetUserByName(text);
                    if (userByName != null)
                    {
                        args.Player.SendMessage("Query result: ", Color.Goldenrod);
                        args.Player.SendMessage(string.Format("User {0} exists.", text), Color.SkyBlue);
                        try
                        {
                            DateTime dateTime = DateTime.Parse(userByName.Registered);
                            DateTime dateTime2 = DateTime.Parse(userByName.LastAccessed);
                            List<string> list = JsonConvert.DeserializeObject<List<string>>(userByName.KnownIps);
                            string arg = list[list.Count - 1];
                            args.Player.SendMessage(string.Format("{0}'s group is {1}.", text, userByName.Group), Color.SkyBlue);
                            args.Player.SendMessage(string.Format("{0}'s last known IP is {1}.", text, arg), Color.SkyBlue);
                            args.Player.SendMessage(string.Format("{0} registered on {1}.", text, dateTime.ToShortDateString()), Color.SkyBlue);
                            args.Player.SendMessage(string.Format("{0} was last seen {1}.", text, dateTime2.ToShortDateString(), dateTime2.ToShortTimeString()), Color.SkyBlue);
                        }
                        catch
                        {
                            DateTime dateTime = DateTime.Parse(userByName.Registered);
                            args.Player.SendMessage(string.Format("{0}'s group is {1}.", text, userByName.Group), Color.SkyBlue);
                            args.Player.SendMessage(string.Format("{0} registered on {1}.", text, dateTime.ToShortDateString()), Color.SkyBlue);
                        }
                    }
                    else
                    {
                        args.Player.SendMessage(string.Format("User {0} does not exist.", text), Color.Red);
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("Syntax: /exui \"<player name>\"");
                }
            }
            else
            {
                args.Player.SendErrorMessage("Syntax: /exui \"<player name>\"");
            }
        }
        #endregion

        #region Clearall
        private void Clearall(CommandArgs args)
        {
            TShockAPI.Commands.HandleCommand(args.Player, "/clear item 9000");
            TShockAPI.Commands.HandleCommand(args.Player, "/clear projectile 9000");
        }
        #endregion        

        #endregion

        #region Other commands

        #region Staff
        public void staff(CommandArgs args)
        {
            List<TSPlayer> list = new List<TSPlayer>(TShock.Players).FindAll((TSPlayer t) => t != null && t.Group.HasPermission("geldar.mod"));
            if (list.Count == 0)
            {
                args.Player.SendErrorMessage("No staff members currently online. If you have a problem check our website at www.geldar.net.");
            }
            else
            {
                args.Player.SendMessage("[Currently online staff members]", Color.Goldenrod);
                foreach (TSPlayer current in list)
                {
                    if (current != null)
                    {
                        Color color = new Color((int)current.Group.R, (int)current.Group.G, (int)current.Group.B);
                        args.Player.SendMessage(string.Format("{0}{1}", current.Group.Prefix, current.Name), color);
                    }
                }
            }
        }
        #endregion        

        #region Facepalm
        private void Facepalm(CommandArgs args)
        {
            var player = Playerlist[args.Player.Index];
            if (player.facepalmcd != 0)
            {
                args.Player.SendErrorMessage("This command is on cooldown for {0} seconds.", (player.facepalmcd));
                args.Player.SendErrorMessage("Chill, facepalming repeatedly can be fatal.");
                return;
            }
            else
            {
                TSPlayer.All.SendMessage(string.Format("{0} facepalmed.", args.Player.Name), Color.Goldenrod);
                if (!args.Player.Group.HasPermission("geldar.bypasscd"))
                {
                    player.facepalmcd = Config.contents.facepalmcd;
                }
            }
        }
        #endregion                

        #region Bunny
        private void Bunny(CommandArgs args)
        {
            args.Player.SendMessage("Don't feed the rabbit after midnight.", Color.Goldenrod);
            args.Player.SetBuff(40, 60, true);
        }
        #endregion        

        #region Starter
        private void Starter(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Info: If you lost your starter weapon here you can replace it.", Color.Goldenrod);
                args.Player.SendMessage("Info: Use the command appropriate to your class /starter mage/warrior/ranger/summoner", Color.Goldenrod);
                return;
            }

            switch (args.Parameters[0])
            {
                #region Starter Mage
                case "mage":
                    {
                        if (args.Player.Group.HasPermission("geldar.starter.mage"))
                        {
                            var player = Playerlist[args.Player.Index];
                            if (player.startercd != 0)
                            {
                                args.Player.SendErrorMessage("This command is on cooldown for {0} seconds.", (player.startercd));
                                return;
                            }
                            else
                            {
                                if (args.Player.InventorySlotAvailable)
                                {
                                    Item itemById = TShock.Utils.GetItemById(Config.contents.startermage);
                                    args.Player.GiveItem(itemById.type, itemById.name, itemById.width, itemById.height, 1, 0);
                                    if (!args.Player.Group.HasPermission("geldar.bypasscd"))
                                    {
                                        player.startercd = Config.contents.startercd;
                                    }
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("Your inventory seems to be full. Free up one slot.");
                                }

                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("You are not level 10 or under, or you executed the wrong class command.");
                            return;
                        }
                    }
                    break;
                #endregion

                #region Starter Warrior
                case "warrior":
                    {
                        if (args.Player.Group.HasPermission("geldar.starter.warrior"))
                        {
                            var player = Playerlist[args.Player.Index];
                            if (player.startercd != 0)
                            {
                                args.Player.SendErrorMessage("This command is on cooldown for {0} seconds.", (player.startercd));
                                return;
                            }
                            else
                            {
                                if (args.Player.InventorySlotAvailable)
                                {
                                    Item itemById = TShock.Utils.GetItemById(Config.contents.starterwarrior);
                                    args.Player.GiveItem(itemById.type, itemById.name, itemById.width, itemById.height, 1, 0);
                                    if (!args.Player.Group.HasPermission("geldar.bypasscd"))
                                    {
                                        player.startercd = Config.contents.startercd;
                                    }
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("Your inventory seems to be full. Free up one slot.");
                                }

                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("You are not level 10 or under, or you executed the wrong class command.");
                            return;
                        }
                    }
                    break;
                #endregion

                #region Starter Ranger
                case "ranger":
                    {
                        if (args.Player.Group.HasPermission("geldar.starter.ranger"))
                        {
                            var player = Playerlist[args.Player.Index];
                            if (player.startercd != 0)
                            {
                                args.Player.SendErrorMessage("This command is on cooldown for {0} seconds.", (player.startercd));
                                return;
                            }
                            else
                            {
                                if (args.Player.InventorySlotAvailable)
                                {
                                    Item itemById = TShock.Utils.GetItemById(Config.contents.starterranger);
                                    args.Player.GiveItem(itemById.type, itemById.name, itemById.width, itemById.height, 1, 0);
                                    if (!args.Player.Group.HasPermission("geldar.bypasscd"))
                                    {
                                        player.startercd = Config.contents.startercd;
                                    }
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("Your inventory seems to be full. Free up one slot.");
                                }

                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("You are not level 10 or under, or you executed the wrong class command.");
                            return;
                        }
                    }
                    break;
                #endregion

                #region Starter Summoner
                case "summoner":
                    {
                        if (args.Player.Group.HasPermission("geldar.starter.summoner"))
                        {
                            var player = Playerlist[args.Player.Index];
                            if (player.startercd != 0)
                            {
                                args.Player.SendErrorMessage("This command is on cooldown for {0} seconds.", (player.startercd));
                                return;
                            }
                            else
                            {
                                if (args.Player.InventorySlotAvailable)
                                {
                                    Item itemById = TShock.Utils.GetItemById(Config.contents.startersummoner);
                                    args.Player.GiveItem(itemById.type, itemById.name, itemById.width, itemById.height, 1, 0);
                                    if (!args.Player.Group.HasPermission("geldar.bypasscd"))
                                    {
                                        player.startercd = Config.contents.startercd;
                                    }
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("Your inventory seems to be full. Free up one slot.");
                                }

                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("You are not a level 20 or under summoner, or you executed the wrong class command.");
                            return;
                        }
                    }
                    break;
                    #endregion
            }
        }
        #endregion

        #endregion

        #region VIP
        private void VIP(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Use the commands below.", Color.Goldenrod);
                args.Player.SendMessage("/vip info - General Info about VIP ranks.", Color.SkyBlue);
                args.Player.SendMessage("/vip <elite/lite/champion/king/supreme/ultimate>", Color.SkyBlue);
                args.Player.SendMessage("You can contact us at admin@geldar.net", Color.SkyBlue);
                args.Player.SendMessage("Press enter and use the up arrow to scroll the chat.", Color.Goldenrod);
                return;
            }

            switch (args.Parameters[0])
            {
                #region Info
                case "info":
                    {
                        args.Player.SendMessage("Here are some basic things to know about the VIP status.", Color.Goldenrod);
                        args.Player.SendMessage("As a VIP you still need to folow the rules. You can check some of them at /geldar.", Color.SkyBlue);
                        args.Player.SendMessage("If you make a non-VIP character you still need to use your VIP house with that character.", Color.SkyBlue);
                        args.Player.SendMessage("You can add your non-VIP to your VIP house with /house allow \"player name>\" \"house name\".", Color.SkyBlue);
                        args.Player.SendMessage("You can contact us at admin@geldar.net or on the forums at www.geldar.net.", Color.SkyBlue);
                    }
                    break;
                #endregion

                #region Lite
                case "lite":
                    {

                    }
                    break;
                #endregion

                #region Elite
                case "elite":
                    {
                        args.Player.SendMessage("You are allowed to have a 20x20 house with no chest amount restriction.", Color.Goldenrod);
                        args.Player.SendMessage("You can join anytime, even if the server if full.", Color.SkyBlue);
                        args.Player.SendMessage("Elite prefix and Royal-blue chat color.", Color.SkyBlue);
                        args.Player.SendMessage("No leveling system, no item restirction, you can start invasions with items.", Color.SkyBlue);
                    }
                    break;
                #endregion

                #region Champion
                case "champion":
                    {
                        args.Player.SendMessage("All the Elite rank benefits.", Color.Goldenrod);
                        args.Player.SendMessage("You are allowed to have a 25x25 house with no chest amount restriction.", Color.SkyBlue);
                        args.Player.SendMessage("You can summon the Collector's Edition Bunny with /bunny.", Color.SkyBlue);
                        args.Player.SendMessage("Champion prefix and Orange chat color.", Color.SkyBlue);
                        args.Player.SendMessage("Teleport back where you died with /b.", Color.SkyBlue);
                    }
                    break;
                #endregion

                #region King
                case "king":
                    {
                        args.Player.SendMessage("All the Elite and Champion rank benefits.", Color.Goldenrod);
                        args.Player.SendMessage("You are allowed to have a 30x30 house with no chest amount restriction.", Color.SkyBlue);
                        args.Player.SendMessage("King prefix and chat color of your liking.", Color.SkyBlue);
                        args.Player.SendMessage("You can use /home and /tp with all subcommands.", Color.SkyBlue);
                        args.Player.SendMessage("For available buff commands for your rank use /buffme.", Color.SkyBlue);
                    }
                    break;
                #endregion

                #region Supreme
                case "supreme":
                    {
                        args.Player.SendMessage("All the Elite, Champion and King rank benefits.", Color.Goldenrod);
                        args.Player.SendMessage("You are allowed to have a 30x30 house with no chest amount restriction.", Color.SkyBlue);
                        args.Player.SendMessage("One pet of your choice.", Color.SkyBlue);
                        args.Player.SendMessage("For available buff commands for your rank use /buffme.", Color.SkyBlue);
                    }
                    break;
                #endregion

                #region Ultimate
                case "ultimate":
                    {
                        args.Player.SendMessage("All the Elite, Champion, King and Supreme rank benefits.", Color.Goldenrod);
                        args.Player.SendMessage("You are allowed to have a 30x30 house with no chest amount restriction.", Color.SkyBlue);
                        args.Player.SendMessage("One mount of your choice.", Color.SkyBlue);
                        args.Player.SendMessage("For available buff commands for your rank use /buffme.", Color.SkyBlue);
                    }
                    break;
                    #endregion
            }
        }
        #endregion

        #region Buffme
        private void Buffme(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Use the commands below to buff youself. Minimum rank for the commands is King.", Color.Goldenrod);
                args.Player.SendMessage("Info: /buffme sixthsense - Required rank: King or above.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme defense - Require rank: Supreme or above.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme misc - Required rank: Supreme or above.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme melee - Required rank : Ultimate.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme ranged - Required rank : Ultimate.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme magic - Required rank : Ultimate.", Color.SkyBlue);
                args.Player.SendMessage("Info: /buffme summoner - Required rank : Ultimate.", Color.SkyBlue);
                args.Player.SendMessage("Press enter then the up arrow to scroll the chat", Color.Goldenrod);
                return;
            }

            switch (args.Parameters[0])
            {
                #region Sixthsense
                case "sixthsense":
                    {
                        if (!args.Player.Group.HasPermission("geldar.king"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(12, 14400);
                            args.Player.SetBuff(11, 18000);
                            args.Player.SetBuff(17, 18000);
                            args.Player.SetBuff(111, 36000);
                            args.Player.SendMessage("You have been buffed with Night Owl, Hunter, Dangersense and Shine Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Defense
                case "defense":
                    {
                        if (!args.Player.Group.HasPermission("geldar.supreme"))
                        {
                            args.Player.
                                SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(5, 18000);
                            args.Player.SetBuff(1, 14400);
                            args.Player.SetBuff(113, 18000);
                            args.Player.SetBuff(124, 54000);
                            args.Player.SetBuff(3, 14400);
                            args.Player.SetBuff(114, 14400);
                            args.Player.SetBuff(2, 18000);
                            args.Player.SetBuff(116, 14400);
                            args.Player.SendMessage("You have been buffed with Obisidian Skin, Warmth, Inferno, Swiftness, Endurance, Regeneration, Lifeforce and Ironskin Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Misc
                case "misc":
                    {
                        if (!args.Player.Group.HasPermission("geldar.supreme"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(122, 14400);
                            args.Player.SetBuff(121, 28800);
                            args.Player.SetBuff(123, 10800);
                            args.Player.SetBuff(109, 28800);
                            args.Player.SetBuff(104, 28800);
                            args.Player.SetBuff(9, 18000);
                            args.Player.SetBuff(4, 7200);
                            args.Player.SetBuff(15, 18000);
                            args.Player.SetBuff(106, 18000);
                            args.Player.SendMessage("You have been buffed with Water Walking, Fishing, Crate, Sonar, Gills, Mining, Spelunker, Flipper and Calming Potion", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Melee
                case "melee":
                    {
                        if (!args.Player.Group.HasPermission("geldar.ultimate"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(115, 14400);
                            args.Player.SetBuff(108, 14400);
                            args.Player.SetBuff(14, 7200);
                            args.Player.SetBuff(117, 14400);
                            args.Player.SendMessage("You have been buffed with Rage, Titan, Thorns and Wrath Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Ranged
                case "ranged":
                    {
                        if (!args.Player.Group.HasPermission("geldar.ultimate"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(16, 14400);
                            args.Player.SetBuff(112, 25200);
                            args.Player.SetBuff(115, 14400);
                            args.Player.SetBuff(117, 14400);
                            args.Player.SendMessage("You  have been buffed with Archery, Ammo Reservation, Rage and Wrath Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Magic
                case "magic":
                    {
                        if (!args.Player.Group.HasPermission("geldar.ultimate"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(7, 7200);
                            args.Player.SetBuff(6, 25200);
                            args.Player.SetBuff(115, 14400);
                            args.Player.SetBuff(117, 14400);
                            args.Player.SendMessage("You  have been buffed with Magic Power, Mana Regeneration, Rage and Wrath Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                #endregion

                #region Summoner
                case "summoner":
                    {
                        if (!args.Player.Group.HasPermission("geldar.ultimate"))
                        {
                            args.Player.SendErrorMessage("You don't have permission to use this buff command");
                            return;
                        }
                        else
                        {
                            args.Player.SetBuff(6, 25200);
                            args.Player.SetBuff(110, 21600);
                            args.Player.SetBuff(115, 14400);
                            args.Player.SetBuff(117, 14400);
                            args.Player.SetBuff(14, 7200);
                            args.Player.SendMessage("You  have been buffed with Mana Regeneration, Summoning, Rage, Wrath and Thorn Potion.", Color.Goldenrod);
                        }
                    }
                    break;
                    #endregion
            }
        }
        #endregion

        #region Geldar
        private void Geldar(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Info: For the most basic commands use /geldar info.", Color.Goldenrod);
                args.Player.SendMessage("info: For rules use /geldar <general/chat/housing/itemdrop/further>", Color.Goldenrod);
                args.Player.SendMessage("Info: Be warned! The full list of rules can be found on our website www.geldar.net", Color.Goldenrod);
                args.Player.SendMessage("Info: The rules here are just the most important ones, shortened to fit.", Color.Goldenrod);
                args.Player.SendMessage("Info: If you lost your starter weapon, you can replace it with the command /starter <mage/warrior/ranger/summoner>", Color.Goldenrod);
                return;
            }

            switch (args.Parameters[0])
            {
                #region Info
                case "info":
                    {
                        args.Player.SendMessage("Welcome to our server, Geldar.", Color.Goldenrod);
                        args.Player.SendMessage("Info: You need level 10 for mining and level 20 to have a house.", Color.SkyBlue);
                        args.Player.SendMessage("Houses can be built above or under spawn.", Color.SkyBlue);
                        args.Player.SendMessage("Info: You can use /spawn to teleport to the map's spawnpoint.", Color.SkyBlue);
                        args.Player.SendMessage("Info: The server uses an ingame serverside currency name Terra Coins.", Color.SkyBlue);
                        args.Player.SendMessage("Info: You need these Terra Coins (tc) to level up, trade, or use ceratin commands.", Color.SkyBlue);
                        args.Player.SendMessage("Info: To check your tc balance use /bank bal or /bb. Earn tc by killing monsters.", Color.SkyBlue);
                        args.Player.SendMessage("Info: For tutorials please use /tutorial.", Color.SkyBlue);
                        args.Player.SendMessage("Press enter to scroll the chat.", Color.Goldenrod);
                    }
                    break;
                #endregion

                #region General
                case "general":
                    {
                        args.Player.SendMessage("------------------------ General Rules ------------------------", Color.Goldenrod);
                        args.Player.SendMessage("Info: On the Main map you are not allowed to build arenas on the surface.", Color.SkyBlue);
                        args.Player.SendMessage("Info: No massive terraforming. ( Destroying mountains, making skybridges, walls.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Do not obstruct players in free movement. (walls,barricades,holes)", Color.SkyBlue);
                        args.Player.SendMessage("Info: Afk farms are not allowed. (boxes around you, mob trapholes)", Color.SkyBlue);
                        args.Player.SendMessage("Info: Going afk while you are protected(gaining tc while afk), is not allowed.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Check the \"Is it Cheating\" thread on our forum.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Using any kind of bug/exploit/glitch will get you banned.", Color.SkyBlue);
                        args.Player.SendMessage("Info: We will ban for the smallest grief.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Using modified/hacked clients will get you banned permanently.", Color.SkyBlue);
                        args.Player.SendMessage("Press enter to scroll the chat.", Color.Goldenrod);
                    }
                    break;
                #endregion

                #region Chat
                case "chat":
                    {
                        args.Player.SendMessage("------------------------ Chat Rules ------------------------", Color.Goldenrod);
                        args.Player.SendMessage("Info: Write in English or you will get muted, kicked or banned.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Don't use offensive, allcaps spammy character names.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Spamming/flooding the chat will get you muted or banned.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Keep the swearing to a minimum.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Racist and discriminative comments will be harshly dealt with.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Advertising anything will get you banned.", Color.SkyBlue);
                        args.Player.SendMessage("Press enter to scroll the chat.", Color.Goldenrod);
                    }
                    break;
                #endregion

                #region Housing
                case "housing":
                    {
                        args.Player.SendMessage("------------------------ Housing Rules ------------------------", Color.Goldenrod);
                        args.Player.SendMessage("Info: You can only have one house. All your characters must use the same one.", Color.SkyBlue);
                        args.Player.SendMessage("Info: House size limit is 15 blocks wide and 12 blocks high. Walls counted in.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Unprotected houses will be removed after 2 days.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Do not put spikes or anything else on your house that can obstruct players.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Only build houses abouve or under spawn where we marked spots.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Bigger clouds are for more than one player. Build on the side of the island.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Do not overlap houses, do not create one big house with your friends.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Every house is limited to 5 chests. (Piggy banks, safes included)", Color.SkyBlue);
                        args.Player.SendMessage("Press enter to scroll the chat.", Color.Goldenrod);
                    }
                    break;
                #endregion

                #region Itemdrop
                case "itemdrop":
                    {
                        args.Player.SendMessage("------------------------ Item Drop Rules ------------------------", Color.Goldenrod);
                        args.Player.SendMessage("Info: You are only allowed to give away vanity, furniture, consumables, money and ammo.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Every item has a tooltip. Check it before dropping.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Treasure bags are not allowed to be given away.", Color.SkyBlue);
                        args.Player.SendMessage("Info: Use /trade to exchange items with others.", Color.SkyBlue);
                        args.Player.SendMessage("Press enter to scroll the chat.", Color.Goldenrod);
                    }
                    break;
                    #endregion
            }
        }
        #endregion

        #region Config reload

        private void Reloadcfg(CommandArgs args)
        {
            if (Config.ReadConfig())
            {
                args.Player.SendMessage("VoidRPG config reloaded.", Color.Goldenrod);
            }

            else
            {
                args.Player.SendErrorMessage("Nope. Check logs.");
            }
        }

        #endregion
    }
}
