using DSharpPlus;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SquadCSharp
{
    class Test2
    {
        public InternalClass _internalClass;
        public Dictionary<string, string> _AllPatterns;

        /*
        * It has beend decided that havin the quick ability to swap between Key/Value to return an item is more beneficial
        * Then continously looping through the Data when we run into a dumb scenario where we do not have easy access to the SteamID. 
        * That said.. The only time we ever see SteamID is when a user joins the server. The Log's only ever use the C_ID or UserName
        * Thus it may be beneficial in the future to refactor this code completely to take advantage of that scenario, which being we have three dictionaries
        * cID_to_user
        * cID_to_Steam
        * user_to_cID
        *  
        *  Yes this is a mix of Camel and whatever the other term is.. Don't care
        */


        //UserName = Key, TeamID = Value
        public Dictionary<string, string> UserWithTeam;

        //userName = Key, cID = Value
        public Dictionary<string, string> user_to_cID;


        //C_ID = Key, UserName = Value
        public Dictionary<string, string> cID_to_user;

        //cID = Key, steam = Value
        public Dictionary<string, string> cID_to_steam;


        //SteamID = Key, C_ID = Value
        private Dictionary<string, string> SteamWithC_ID;

        //Name -> Key, CID -> Value
        public Dictionary<string, string> Name_to_cIDTemp;

        public List<string> adminInCameraList;
        public Dictionary<string, string> adminInCameraDic;

        //Info about this
        //UserName -> Key, {Kills, Deaths, Wounds, Team, Connected} -> Values
        public Dictionary<string, int[]> playerStats;

        public List<string> _playerList;

        private string C_ID;
        private string steamID;
        public String voidTest()
        {
            return "It worked";
        }

        string _teamID;
        string _NormalWounds;
        string _TeamKill;
        string _unidenfiedVictim;
        string _unidenfiedKiller;
        string _allKills;
        string _NormalKills;
        string _errorLog;

        Util _Util;
        public DiscordClient Client { get; set; }

        public Test2()
        {
            C_ID = "";
            adminInCameraList = new List<string>();
            adminInCameraDic = new Dictionary<string, string>();
            UserWithTeam = new Dictionary<string, string>();

            cID_to_steam = new Dictionary<string, string>();
            user_to_cID = new Dictionary<string, string>();


            cID_to_user = new Dictionary<string, string>();
            SteamWithC_ID = new Dictionary<string, string>();


            Name_to_cIDTemp = new Dictionary<string, string>();


            _AllPatterns = new Dictionary<string, string>();
            _internalClass = new InternalClass();

            _playerList = new List<string>();

            playerStats = new Dictionary<string, int[]>();
 
            regexSetup();
            //var prog = new Test2();
            //MainAsync().GetAwaiter().GetResult();

            _teamID = "";
            _NormalWounds = "";
            _TeamKill = "";
            _unidenfiedVictim = "";
            _unidenfiedKiller= "";
            _allKills = "";
            _NormalKills = "";
            _errorLog = "";
            _Util = new Util();

        }

        private void regexSetup()
        {
            _AllPatterns.Add("playerConnected", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: PostLogin: NewPlayer: BP_PlayerController_C (.+).BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("steamID", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogEasyAntiCheatServer: \\[[0-9:]+]\\[[A-z]+]\\[EAC Server] \\[Info]\\[RegisterClient] Client: ([A-z0-9]+) PlayerGUID: ([0-9]{17}) PlayerIP: [0-9]{17} OwnerGUID: [0-9]{17} PlayerName: (.+)");
            _AllPatterns.Add("errorTeam", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogSquad: Error: No teams .+ Name: (.+)");
            _AllPatterns.Add("playerName", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogNet: Join succeeded: (.+)");
            _AllPatterns.Add("chatMessage", "\\[(ChatAll|ChatTeam|ChatSquad|ChatAdmin)] \\[SteamID:([0-9]{17})] (.+?) : (.*)");
            _AllPatterns.Add("removeUser", "\\[([0-9.:-]+)][[ 0-9]+]LogNet: UChannel::Close: [A-z0-9_ ,.=:]+ RemoteAddr: ([0-9]+):[A-z0-9_ ,.=:]+ BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("adminBroadcast", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: ADMIN COMMAND: Message broadcasted <(.+)> from (.+)");
            _AllPatterns.Add("newGame", "\\[([0-9.:-]+)][[ 0-9]*]LogWorld: Bringing World \\/([A-z]+)\\/Maps\\/([A-z]+)\\/(?:Gameplay_Layers\\/)?([A-z0-9_]+)");
            _AllPatterns.Add("playerDamaged", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: Player:(.+) ActualDamage=([0-9.]+) from (.+) caused by ([A-z_0-9]+)_C");
            _AllPatterns.Add("playerDied", "\\[([0-9.]+)-([0-9.]+):[0-9\\[\\]]+].+Die\\(\\): Player:(.+) KillingDamage=.+ from BP_PlayerController_(.+) caused .+");
            _AllPatterns.Add("playerPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: .+ PC=(.+) Pawn=([A-z0-9_]+)_C");
            _AllPatterns.Add("playerUnPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQPlayerController::)?OnUnPossess\\(\\): PC=(.+)");
            _AllPatterns.Add("playerRevived", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: (.+) has revived (.+)\\.");
            _AllPatterns.Add("playerWounded", "\\[([0-9.]+)-([0-9.]+):[0-9\\[\\]]+].+Wound\\(\\): Player:(.+) KillingDamage.+ from BP_PlayerController_(.+) caused by (.+)");
            //_AllPatterns.Add("serverTick", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: USQGameState: Server Tick Rate: ([0-9.]+)");
            _AllPatterns.Add("roundWinner", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer]ASQGameMode::DetermineMatchWinner\\(\\): (.+) won on (.+)");
            _AllPatterns.Add("playerList", "/ID: ([0-9]+) \\| SteamID: ([0-9]{17}) \\| Name: (.+) \\| Team ID: ([0-9]+) \\| Squad ID: ([0-9]+|N\\/A)");
            _AllPatterns.Add("currentMap", "/^Current map is (.+), Next map is (.*)/");
        }

        public void matchList(string stringType, string line , string[] substring, MySqlConnection conn, Boolean newUser = false)
        {
            String _SQL;
            string _line;
            MySqlCommand cmd;
            var prog = new Program();
            string victimResult;
            switch (stringType)
            {
                case "playerConnected": //This will get called before UserJoining/PlayerName
                    {
                        C_ID = substring[3];
                        //Console.WriteLine(C_ID);
                        //Adding the User C_ID to the Dictionary First, since we don't know the UserName yet. 
                        //cID_to_user.Add(substring[3], "not defined");

                        break;
                    }
                case "errorTeam":
                    {
                        /*
                         * 0 Empty
                         * 1 Time
                         * 2 Name
                         * 3 empty
                         */
                        Name_to_cIDTemp[substring[2]] = C_ID;
                        Console.WriteLine(line);
                        break;
                    }
                case "steamID":
                    {
                        if(Name_to_cIDTemp.Count > 0 && Name_to_cIDTemp.ContainsKey(substring[4]))
                        {
                            userJoining(Name_to_cIDTemp[substring[4]], substring, conn, true);
                            Name_to_cIDTemp.Remove(substring[4]);
                        }
                        else
                        {
                            userJoining(C_ID, substring, conn);
                        }
                        break;
                    }
                case "playerName":
                    {
                        try
                        {
                            bool internalLog;
                            string chatType, message;
                            //foreach (var sub in substring)
                            //    Console.WriteLine(sub);
                            cID_to_user.Add(C_ID, substring[2]);
                            user_to_cID.Add(substring[2], C_ID);
                            if (steamID.Equals(""))
                                steamID = "00000000000000000";

                            internalLog = _Util.logUserNameList(conn, steamID, substring[2]);
                            internalLog = _Util.logPlayerList(conn, steamID, substring[2], true);
                            chatType = "Connected";
                            message = (Int64.Parse(steamID) > 0) ? $"{substring[2]} Has joined the Server" : $"{substring[2]} Has Joined with missing SteamID";
                            internalLog = _Util.logChats(conn, steamID, chatType, message);
                            _playerList.Add(substring[2]);
                            //{Kill, Death, Wound, Team, Connected}
                            //{0     1      2      3     4        }
                            if (playerStats.ContainsKey(substring[2]))
                            {
                                playerStats[substring[2]][4] = 1;
                            }
                            else
                            {
                                playerStats[substring[2]] = new int[] { 0, 0, 0, 0, 0 };
                            }


                            steamID = "";
                            C_ID = "";
                        }
                        catch (Exception e)
                        {
                            string cID = "";
                            string user = "";
                            Console.WriteLine(line);
                            foreach(var p in cID_to_user)
                            {
                                cID += $"{p.Key} ::: {p.Value} \n";
                            }
                            foreach (var p in user_to_cID)
                            {
                                user += $"{p.Key} ::: {p.Value} \n";
                            }
                            Console.WriteLine(e);
                            _errorLog += e.ToString() + "\n";
                            System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                            System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\cID.txt", cID);
                            System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\user.txt", user);
                        }
                        break;
                    }
                case "playerDied":
                    {
                        ////substring = 6 length, 0 - 6 are empty
                        /*
                         * 0 = Empty
                         * 1 = Date
                         * 2 = Time
                         * 3 = Victim
                         * 4 = Assault (C_ID)
                         * 5 = Empty
                         */

                        if (UserWithTeam.TryGetValue(substring[3], out victimResult))
                        {
                            handleTeamKills(substring, substring[3], victimResult, conn);
                        }
                        else
                        {
                            foreach (var player in UserWithTeam)
                            {
                                //Console.WriteLine("The player is: " + player);
                                if (substring[3].Contains(player.Key))
                                {

                                    handleTeamKills(substring, player.Key, player.Value, conn);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case "playerRevived":
                    {
                        //substring[3].Contains(player.Key)
                        /*
                         * 0 = Empty
                         * 1 = date
                         * 2 = Medic
                         * 3 = DeadPerson
                         * 4 = Empty
                         */
                        string medic = "";
                        //string revived = "";
                        foreach (var p in user_to_cID) {
                            if (substring[2].Contains(p.Key))
                                medic = p.Key;
                            //if (substring[3].Contains(p.Key))
                            //    revived = p.Key;
                        }
                        if(string.IsNullOrEmpty(medic) /*|| string.IsNullOrEmpty(revived)*/)
                        {
                            Console.WriteLine(line);
                        }
                        else
                        {
                            bool internalLog = _Util.logRevives(conn, cID_to_steam[user_to_cID[medic]]);
                        }
                        break;
                    }
                case "playerWounded":
                    {
                        _allKills += line + "\n";
                        System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\allKills.txt", _allKills);

                        //foreach (var sub in substring)
                        //    Console.WriteLine(sub);


                        //if (substring[5].Equals("nullptr"))
                        //    break;
                        ////substring = 7 length, 0 - 7 are empty
                        /*
                         * 0 = Empty
                         * 2 = Time
                         * 3 = Victim
                         * 4 = Assault (C_ID)
                         * 5 = Weapon/Team
                         * 6 = Empty
                         */

                        //Console.WriteLine("I'm inside checking out: " + substring[3]);
                        if (UserWithTeam.TryGetValue(substring[3], out victimResult))
                        {
                            handleTeamKills(substring, substring[3], victimResult, conn, true);
                        }
                        else
                        {
                            foreach (var player in UserWithTeam)
                            {
                                //Console.WriteLine("The player is: " + player);
                                if (substring[3].Contains(player.Key))
                                {

                                    handleTeamKills(substring, player.Key, player.Value, conn, true);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case "playerUnPosses":
                    {
                        //Add Future Logic for seeing if someone is using admin cam to cheat. 
                        if (adminInCameraDic.ContainsKey(substring[2]))
                        {
                            //Console.WriteLine(line);
                            adminInCameraDic[substring[2]] = "Inactive";
                        }
                        break;
                    }
                case "removeUser":
                    {
                        /*
                         * 0 = Empty
                         * 1 = Time
                         * 2 = Steamid
                         * 3 = C_ID
                         * 4 = Empty
                         */
                        if (SteamWithC_ID.ContainsKey(substring[2]))
                        {
                            //Console.WriteLine("This user: " + userBP_C[userSteamToBP_C[substring[2]]] + " Has decided to leave the server");
                            //Console.WriteLine(setUserNameToC_ID[userSteamToC_ID[substring[2]]]);
                            if (adminInCameraDic.ContainsKey(cID_to_user[SteamWithC_ID[substring[2]]]))
                                adminInCameraDic[cID_to_user[SteamWithC_ID[substring[2]]]] = "Inactive";

                            try
                            {
                                string chatType, message;
                                chatType = "Disconnected";
                                message = cID_to_user[SteamWithC_ID[substring[2]]] + " left the server";
                                bool internalLog = _Util.logChats(conn, substring[2], chatType, message);
                                internalLog = _Util.logPlayerList(conn, substring[2], cID_to_user[SteamWithC_ID[substring[2]]]);
                                //Console.WriteLine("Person removed: " + C_IDWithUser[SteamWithC_ID[substring[2]]]);
                                _playerList.Remove(cID_to_user[SteamWithC_ID[substring[2]]]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                _errorLog += e.ToString() + "\n";
                                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                            }


                            //Console.WriteLine("SteamID/UserName Removed");
                            string userName = UserWithTeam.ContainsKey(cID_to_user[substring[3]]) ? cID_to_user[substring[3]] : "";
                            if (string.IsNullOrEmpty(userName))
                            {
                                foreach (var player in UserWithTeam)
                                {
                                    //Console.WriteLine("The player is: " + player);
                                    if (cID_to_user[substring[3]].Contains(player.Key))
                                    {

                                        userName = player.Key;
                                        break;
                                    }
                                }
                            }
                            UserWithTeam.Remove(userName);
                            user_to_cID.Remove(cID_to_user[substring[3]]);
                            cID_to_user.Remove(SteamWithC_ID[substring[2]]);
                            cID_to_steam.Remove(SteamWithC_ID[substring[2]]);

                            SteamWithC_ID.Remove(substring[2]);

                        }
                        else
                        {

                            //Console.WriteLine("This user: " + substring[2] + " Has decided to leave the server");
                        }
                        break;
                    }
                case "playerPosses":
                    {
                        string bp_soldier = "bp_soldier";

                        if (substring[3].ToLower().StartsWith(bp_soldier))
                        {
                            string[] teamID = substring[3].Split("_");
                            UserWithTeam[substring[2]] = teamID[2];
                            if (String.IsNullOrEmpty(_teamID))
                            {
                                _teamID = teamID[2];
                            }
                            playerStats[substring[2]][3] = _teamID.Equals(teamID[2]) ? 1 : 0;
                            //Console.WriteLine("substring[2]: " + substring[2] + "With teamID: " + teamID[2]);

                        }
                        else if (substring[3].ToLower().StartsWith("cameraman"))
                        {
                            adminInCameraList.Add(line);
                            adminInCameraDic[substring[2]] = "Active";
                            bool internalLog = _Util.logAdmin(conn, substring[2], line);
                            //sendMessageAdmin(substring);
                        }
                        break;
                    } 
                case "chatMessage":
                    {
                        //Console.WriteLine(line);
                        string chatType, message;
                        chatType = substring[1];
                        message = substring[4];
                        bool internalLog = _Util.logChats(conn, substring[2], chatType, message);
                        //_internalClass.Add(substring);
                        break;
                    }
                default:
                    //Console.WriteLine("Default was Called");
                    //Console.WriteLine(line);
                    //Console.WriteLine("Default ends");
                    break;
            }
        }

        private void userJoining(string _CID, string[] substring, MySqlConnection conn, Boolean errorTeam = false)
        {
            String _SQL;
            string _line;
            MySqlCommand cmd;
            string _steamID = "";

            //Error: No teams exist yet
            /*
            * Need to find logic to handle when this error gets thrown
            */
            if (String.IsNullOrEmpty(_CID))
            {
                return;
            }


            if (errorTeam)
            {
                _steamID = substring[3];
                SteamWithC_ID.Add(_steamID, _CID);
                cID_to_steam.Add(_CID, _steamID);
                

            }
            else 
            {
                steamID = substring[3];
                _steamID = steamID;
                SteamWithC_ID.Add(steamID, _CID);
                cID_to_steam.Add(_CID, steamID);
            }

            bool internalLog = _Util.logSteamUser(conn, _steamID);
            if (errorTeam)
            {
                updatePlayerList(_CID, _steamID, conn);
            }
            return;           
        }

        private void updatePlayerList(string _CID, string _steamID, MySqlConnection conn)
        {
            String _SQL;
            string _line;
            MySqlCommand cmd;

            string steamID = (Int64.Parse(_steamID) > 0) ? _steamID : "0000000";


            bool internalLog;
            string chatType, message;
            internalLog = _Util.logUserNameList(conn, steamID, cID_to_user[_CID]);
            internalLog = _Util.logPlayerList(conn, steamID, cID_to_user[_CID], true);

            chatType = "Connected";
            message = (Int64.Parse(steamID) > 0) ? $"{cID_to_user[_CID]} SteamID has Been Established" : $"{cID_to_user[_CID]} ::: Despite our best efforts, we have failed to acquire the SteamID again";
            internalLog = _Util.logChats(conn, steamID, chatType, message);
            return;
        }

        private void handleTeamKills(string[] substring, string victimName, string victimTeam, MySqlConnection conn, Boolean wound = false)
        {
            string _SQL;
            string format = "yyyy-MM-dd";
            string _line;
            MySqlCommand cmd;
            string attackerTeam, attackerName;
            //Console.WriteLine("I'm inside and found player and his team: " + victimName + " : " + victimTeam);
            cID_to_user.TryGetValue(substring[4], out attackerName);
            if (String.IsNullOrEmpty(attackerName)) { attackerTeam = ""; } else { UserWithTeam.TryGetValue(attackerName, out attackerTeam); }


            if (String.IsNullOrEmpty(attackerTeam))
            {
                //Console.WriteLine("There was a Potential TeamKill");
                //Console.WriteLine("The Victim Team was: " + victimResult);
                //Console.WriteLine(line);
                _unidenfiedKiller += "Attacker is not part of PlayerList/TeamList; Their ID is: " + substring[4] + " ---  The Victim is: " + victimName + ":" + victimTeam + " \n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_unidenfiedKiller.txt", _unidenfiedKiller);
            }else 

            if (String.IsNullOrEmpty(victimTeam))
            {
                Console.WriteLine("I'm Inside this victimResult thing");
                _unidenfiedVictim += "The Victim is Unknown in this situation, their name is: " + victimTeam + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_unidenfiedVictim.txt", _unidenfiedVictim);
            }else 

            //Console.WriteLine(substring[2]);
            //Console.WriteLine(result);
            //Console.WriteLine("");
            //Console.WriteLine(C_IDWithUser[substring[4]]);
            //Console.WriteLine(UserWithTeam[C_IDWithUser[substring[4]]]);
            if (victimTeam.Equals(attackerTeam))
            {
                if (wound)
                {
                    if (!attackerName.Equals(victimName))
                    {
                        _TeamKill += "The Attacker: " + attackerName + ":" + attackerTeam + " -- Killed The Victim: " + victimName + ":" + victimTeam + " -- Using " + substring[5] + "\n";
                        //System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_TeamKill.txt", _TeamKill);
                        string steamID, chatType, message;
                        steamID = "00000000000000000";
                        chatType = "TeamKill";
                        message = $"Player: {victimName} was killed by {attackerName}";
                        bool internalLog = _Util.logChats(conn, steamID, chatType, message);

                        string[] victim, attacker;
                        victim = new string[] { victimName, victimTeam, cID_to_steam.ContainsKey(user_to_cID[victimName]) ? cID_to_steam[user_to_cID[victimName]] : "1111" };
                        attacker = new string[] { attackerName, attackerTeam, cID_to_steam.ContainsKey(substring[4]) ? cID_to_steam[substring[4]] : "0000" };
                        internalLog = _Util.logWounds(conn, victim, attacker, substring, true);
                    }
                    //We don't record Suicides. 
                }
                else
                {
                    //This is a Teamkill or Suicide.. that resulted in death, thus needs to be recorded. But Teamkills aren't recored during Death only wounded
                    playerStats[victimName][1] += 1;
                }
            }
            else
            {
                if (wound)
                {
                    /*
                    * 0 = Empty
                    * 1 = Date
                    * 2 = Time
                    * 3 = Victim
                    * 4 = Assault (C_ID)
                    * 5 = Weapon/Team
                    * 6 = Empty
                    */
                    _NormalWounds += "We don't know -- The Attacker: " + attackerName + ":" + attackerTeam + " and the Victim: " + victimName + ":" + victimTeam + "--- The weapon: " + substring[5] + "\n";
                    System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_NormalWounds.txt", _NormalWounds);
                    string[] victim, attacker;
                    victim = new string[] { victimName, victimTeam, cID_to_steam.ContainsKey(user_to_cID[victimName]) ? cID_to_steam[user_to_cID[victimName]] : "1111" };
                    attacker = new string[] { attackerName, attackerTeam, cID_to_steam.ContainsKey(substring[4]) ? cID_to_steam[substring[4]] : "0000" };
                    bool internalLog = _Util.logWounds(conn, victim, attacker, substring);
                    playerStats[attackerName][2] += 1;
                }
                else
                {
                    /*
                     * 0 = Empty
                     * 1 = Date
                     * 2 = Time
                     * 3 = Victim
                     * 4 = Assault (C_ID)
                     * 5 = Empty
                     */
                    _NormalKills += "We don't know -- The Attacker: " + attackerName + ":" + attackerTeam + " and the Victim: " + victimName + ":" + victimTeam + "\n";

                    string[] victim, attacker;
                    victim = new string[] { victimName, cID_to_steam.ContainsKey(user_to_cID[victimName]) ? cID_to_steam[user_to_cID[victimName]] : "1111" };
                    attacker = new string[] { attackerName, cID_to_steam.ContainsKey(substring[4]) ? cID_to_steam[substring[4]] : "0000" };
                    bool internalLog = _Util.logKills(conn, victim, attacker, substring);
                    playerStats[victimName][1] += 1;
                    playerStats[attackerName][0] += 1;
                    //System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_NormalKills.txt", _NormalKills);
                }

            }

        }

        //public async Task sendMessageAdmin(string[] _subString)
        //{
        //    var returnValue = "```" + _subString[2] + " \nHas Entered Admin Cam at\n" + DateTime.Now.ToString("ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’" + "```");

        //    var AdminCam = await Client.GetChannelAsync(787524708546510848);
        //    await Client.SendMessageAsync(AdminCam, returnValue);
        //}

        //public async Task sendMessageTeamKill(string Victim, string Attacker)
        //{
        //    var returnValue = "```" + Victim + " \nWas Killed by:\n" + Attacker + "```";


        //    var TeamKill = await Client.GetChannelAsync(787524643564814366);
        //    Client.SendMessageAsync(TeamKill, returnValue);
        //}
        //public async Task sendMessagePotentialTeamKill(string Victim, string Attacker)
        //{
        //    var returnValue = "```" + Victim + " \nWas Killed by:\n" + Attacker + "```";


        //    var TeamKill = await Client.GetChannelAsync(788904101471453195);
        //    await Client.SendMessageAsync(TeamKill, returnValue);
        //}

        public async Task MainAsync()
        {

            //var cfg = new DiscordConfiguration
            //{
            //    Token = ConfigurationManager.AppSettings.Get("20r_token"),
            //    TokenType = TokenType.Bot
            //};

            //this.Client = new DiscordClient(cfg);

            //Client.MessageCreated += async (e) =>
            //{
            //    if (e.Message.Content.ToLower().StartsWith("ping"))
            //    {
            //        await e.Channel.SendMessageAsync(e.Channel.ToString());
            //    }
            //};

            //var test = await Client.GetChannelAsync(787524708546510848);
            //Client.SendMessageAsync(test, "Test");

            //await Client.ConnectAsync();
            //await Task.Delay(-1);
        }
    }
}
