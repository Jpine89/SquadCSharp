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

        public List<string> adminInCameraList;
        public Dictionary<string, string> adminInCameraDic;

        //Info about this
        //{Kills, Deaths, Wounds, Team, Connected}
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

        }

        private void regexSetup()
        {
            _AllPatterns.Add("playerConnected", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: PostLogin: NewPlayer: BP_PlayerController_C (.+).BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("steamID", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogEasyAntiCheatServer: \\[[0-9:]+]\\[[A-z]+]\\[EAC Server] \\[Info]\\[RegisterClient] Client: ([A-z0-9]+) PlayerGUID: ([0-9]{17}) PlayerIP: [0-9]{17} OwnerGUID: [0-9]{17} PlayerName: (.+)");
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
                case "UserJoining": 
                    {
                        if (newUser)
                        {
                            //Error: No teams exist yet
                            /*
                             * Need to find logic to handle when this error gets thrown
                             */
                            if (String.IsNullOrEmpty(C_ID))
                            {
                                break;
                            }

                            //Console.WriteLine("We are inside the if..");
                            //for (int i = 0; i < substring.Length; i++)
                            //{
                            //    Console.WriteLine("I is: " + i + " the string is: " + substring[i]);
                            //}

                            //Console.WriteLine("User: " + substring[4] + " Has Joined the Server, with C_ID: " + C_ID + " and SteamID: " + substring[3]);
                            //Now that we have the User Name, we are combining that with the C_ID. 
                            //C_IDWithUser[C_ID] = substring[4];

                            steamID = substring[3];
                            //Game Logs use SteamID to see who leaves. 
                            //We are setting that as the Key, and it's value as the C_ID
                            //Console.WriteLine("I'm about to add to SteamWithC_ID");

                            SteamWithC_ID.Add(steamID, C_ID);
                            cID_to_steam.Add(C_ID, steamID);

                            try
                            {
                                _SQL = "INSERT IGNORE INTO steamuser (steamID) VALUES (@steamID);";
                                cmd = new MySqlCommand(_SQL, conn);
                                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                                cmd.ExecuteNonQuery();



                                _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                                cmd = new MySqlCommand(_SQL, conn);
                                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                                cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Connected";
                                cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4] + " joined the server";
                                cmd.ExecuteNonQuery();



                                //Console.WriteLine("SteamID/UserName Added");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                _errorLog += e.ToString() + "\n";
                                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                            }
                        }
                        break;
                    }
                case "playerName":
                    {
                        //foreach (var sub in substring)
                        //    Console.WriteLine(sub);
                        cID_to_user.Add(C_ID, substring[2]);
                        user_to_cID.Add(substring[2], C_ID);
                        if (steamID.Equals(""))
                            steamID = "00000000000000000";


                        try
                        {
                            _SQL = "INSERT IGNORE INTO userNameList (steamID, userName) VALUES (@steamID, @userName);";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                            cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                            cmd.ExecuteNonQuery();

                            _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                             ON DUPLICATE KEY UPDATE
                             userName = VALUES(userName),
                             connected = VALUES(connected)";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                            cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                            cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = 1;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            _errorLog += e.ToString() + "\n";
                            System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                        }
                        

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
                        if (SteamWithC_ID.ContainsKey(substring[2]))
                        {
                            //Console.WriteLine("This user: " + userBP_C[userSteamToBP_C[substring[2]]] + " Has decided to leave the server");
                            //Console.WriteLine(setUserNameToC_ID[userSteamToC_ID[substring[2]]]);
                            if (adminInCameraDic.ContainsKey(cID_to_user[SteamWithC_ID[substring[2]]]))
                                adminInCameraDic[cID_to_user[SteamWithC_ID[substring[2]]]] = "Inactive";

                            try
                            {
                                _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                                //Console.WriteLine(_SQL);
                                cmd = new MySqlCommand(_SQL, conn);
                                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                                cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Disconnected";
                                cmd.Parameters.Add("@message", MySqlDbType.Text).Value = cID_to_user[SteamWithC_ID[substring[2]]] + " left the server";
                                cmd.ExecuteNonQuery();

                                _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                                     ON DUPLICATE KEY UPDATE
                                     userName = VALUES(userName),
                                     connected = VALUES(connected)";
                                cmd = new MySqlCommand(_SQL, conn);
                                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                                cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = cID_to_user[SteamWithC_ID[substring[2]]];
                                cmd.Parameters.Add("@connected", MySqlDbType.UInt32).Value = 0;
                                cmd.ExecuteNonQuery();


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
                            //sendMessageAdmin(substring);
                            try
                            {
                                _SQL = "INSERT INTO adminlog (userName, logMessage) VALUES (@userName, @logMessage)";
                                cmd = new MySqlCommand(_SQL, conn);
                                cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                                cmd.Parameters.Add("@logMessage", MySqlDbType.Text).Value = line;
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                _errorLog += e.ToString() + "\n";
                                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                            }

                        }
                        break;
                    } 
                case "chatMessage":
                    {
                        //Console.WriteLine(line);

                        //_internalClass.Add(substring);
                        try
                        {
                            _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType, @message)";
                            Console.WriteLine(_SQL);
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                            cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = substring[1];
                            cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4];
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine(e);
                        }

                        break;
                    }
                default:
                    //Console.WriteLine("Default was Called");
                    //Console.WriteLine(line);
                    //Console.WriteLine("Default ends");
                    break;
            }
        }

        private void handleTeamKills(string[] substring, string victimName, string victimTeam, MySqlConnection conn, Boolean wound = false)
        {
            String _SQL;
            string format = "yyyy-MM-dd";
            string _line;
            MySqlCommand cmd;
            string attackerTeam, attackerName;
            Console.WriteLine("I'm inside and found player and his team: " + victimName + " : " + victimTeam);
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
                        try
                        {
                            _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = 00000000000000000;
                            cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "TeamKill";
                            cmd.Parameters.Add("@message", MySqlDbType.Text).Value = ($"Player: {victimName} was killed by {attackerName}");
                            cmd.ExecuteNonQuery();

                            _SQL = @"INSERT INTO playerWounded (victim, victimTeam, victimSteamID, attacker, attackerTeam, attackerSteamID, weaponUsed, teamKill, date, time, serverID) 
                                    VALUES (@victim, @victimTeam, @victimSteamID, @attacker, @attackerTeam, @attackerSteamID, @weaponUsed, @teamKill, @date, @time, @serverID)";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@victim", MySqlDbType.VarChar).Value = victimName;
                            cmd.Parameters.Add("@victimTeam", MySqlDbType.VarChar).Value = victimTeam;
                            cmd.Parameters.Add("@victimSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(user_to_cID[victimName]) ? Int64.Parse(cID_to_steam[user_to_cID[victimName]]) : 1111;
                            cmd.Parameters.Add("@attacker", MySqlDbType.VarChar).Value = attackerName;
                            cmd.Parameters.Add("@attackerTeam", MySqlDbType.VarChar).Value = attackerTeam;
                            cmd.Parameters.Add("@attackerSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(substring[4]) ? Int64.Parse(cID_to_steam[substring[4]]) : 0000;
                            cmd.Parameters.Add("@weaponUsed", MySqlDbType.VarChar).Value = substring[5];
                            cmd.Parameters.Add("@teamKill", MySqlDbType.Int32).Value = 1;
                            cmd.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.Parse(substring[1]).ToString("yyyy-MM-dd");
                            cmd.Parameters.Add("@time", MySqlDbType.Time).Value = TimeSpan.Parse(substring[2].Replace(".", ":"));
                            cmd.Parameters.Add("@serverID", MySqlDbType.Int32).Value = 0;
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            _errorLog += e.ToString() + "\n";
                            System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                        }




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
                    try
                    {
                        _NormalWounds += "We don't know -- The Attacker: " + attackerName + ":" + attackerTeam + " and the Victim: " + victimName + ":" + victimTeam + "--- The weapon: " + substring[5] + "\n";
                        System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_NormalWounds.txt", _NormalWounds);

                        _SQL = @"INSERT INTO playerWounded (victim, victimTeam, victimSteamID, attacker, attackerTeam, attackerSteamID, weaponUsed, teamKill, date, time, serverID) 
                                    VALUES (@victim, @victimTeam, @victimSteamID, @attacker, @attackerTeam, @attackerSteamID, @weaponUsed, @teamKill, @date, @time, @serverID)";
                        cmd = new MySqlCommand(_SQL, conn);
                        cmd.Parameters.Add("@victim", MySqlDbType.VarChar).Value = victimName;
                        cmd.Parameters.Add("@victimTeam", MySqlDbType.VarChar).Value = victimTeam;
                        cmd.Parameters.Add("@victimSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(user_to_cID[victimName]) ? Int64.Parse(cID_to_steam[user_to_cID[victimName]]) : 1111;
                        cmd.Parameters.Add("@attacker", MySqlDbType.VarChar).Value = attackerName;
                        cmd.Parameters.Add("@attackerTeam", MySqlDbType.VarChar).Value = attackerTeam;
                        cmd.Parameters.Add("@attackerSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(substring[4]) ? Int64.Parse(cID_to_steam[substring[4]]) : 0000;
                        cmd.Parameters.Add("@weaponUsed", MySqlDbType.VarChar).Value = substring[5];
                        cmd.Parameters.Add("@teamKill", MySqlDbType.Int32).Value = 0;
                        cmd.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.Parse(substring[1]).ToString("yyyy-MM-dd");
                        cmd.Parameters.Add("@time", MySqlDbType.Time).Value = TimeSpan.Parse(substring[2].Replace(".", ":"));
                        cmd.Parameters.Add("@serverID", MySqlDbType.Int32).Value = 0;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _errorLog += e.ToString() + "\n";
                        System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                    }


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
                    try
                    {
                        _NormalKills += "We don't know -- The Attacker: " + attackerName + ":" + attackerTeam + " and the Victim: " + victimName + ":" + victimTeam + "\n";
                        playerStats[victimName][1] += 1;
                        playerStats[attackerName][0] += 1;


                        _SQL = @"INSERT INTO playerKilled (victim, victimSteamID, attacker, attackerSteamID, date, time, serverID) 
                                    VALUES (@victim, @victimSteamID, @attacker,  @attackerSteamID, @date, @time, @serverID)";
                        cmd = new MySqlCommand(_SQL, conn);
                        cmd.Parameters.Add("@victim", MySqlDbType.VarChar).Value = victimName;
                        cmd.Parameters.Add("@victimSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(user_to_cID[victimName]) ? Int64.Parse(cID_to_steam[user_to_cID[victimName]]) : 1111;
                        cmd.Parameters.Add("@attacker", MySqlDbType.VarChar).Value = attackerName;
                        cmd.Parameters.Add("@attackerSteamID", MySqlDbType.Int64).Value = cID_to_steam.ContainsKey(substring[4]) ? Int64.Parse(cID_to_steam[substring[4]]) : 0000;
                        cmd.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.Parse(substring[1]).ToString("yyyy-MM-dd");
                        cmd.Parameters.Add("@time", MySqlDbType.Time).Value = TimeSpan.Parse(substring[2].Replace(".", ":"));
                        cmd.Parameters.Add("@serverID", MySqlDbType.Int32).Value = 0;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _errorLog += e.ToString() + "\n";
                        System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                    }




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
