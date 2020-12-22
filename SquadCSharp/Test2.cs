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

        //C_ID = Key, UserName = Value
        public Dictionary<string, string> C_IDWithUser;

        //SteamID = Key, C_ID = Value
        private Dictionary<string, string> SteamWithC_ID;

        //UserName = Key, TeamID = Value
        public Dictionary<string, string> UserWithTeam;
        public List<string> adminInCameraList;
        public Dictionary<string, string> adminInCameraDic;
        private string C_ID;
        private string steamID;
        public String voidTest()
        {
            return "It worked";
        }

        string _unidenfiedKill;
        string _TeamKill;
        string _unidenfiedVictim;
        string _unidenfiedKiller;
        string _allKills;
        public DiscordClient Client { get; set; }

        public Test2()
        {
            C_ID = "";
            adminInCameraList = new List<string>();
            adminInCameraDic = new Dictionary<string, string>();
            UserWithTeam = new Dictionary<string, string>();

            C_IDWithUser = new Dictionary<string, string>();
            SteamWithC_ID = new Dictionary<string, string>();

            _AllPatterns = new Dictionary<string, string>();
            _internalClass = new InternalClass();
            regexSetup();
            //var prog = new Test2();
            //MainAsync().GetAwaiter().GetResult();
            _unidenfiedKill = "";
            _TeamKill = "";
            _unidenfiedVictim = "";
            _unidenfiedKiller= "";
            _allKills = "";


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
            _AllPatterns.Add("playerDied", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQSoldier::)?Die\\(\\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from BP_PlayerController_([A-z_0-9]+) caused by ([A-z_0-9]+)");
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
            switch (stringType)
            {
                case "playerWounded":
                    _allKills += line + "\n";
                    System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\allKills.txt", _allKills);

                    //foreach (var sub in substring)
                    //    Console.WriteLine(sub);


                    //if (substring[5].Equals("nullptr"))
                    //    break;
                    ////substring = 7 length, 0 - 7 are empty
                    /*
                     * 0 = Empty
                     * 1 = Date
                     * 2 = Time
                     * 3 = Victim
                     * 4 = Assault (C_ID)
                     * 5 = Weapon/Team
                     * 6 = Empty
                     */


                    Console.WriteLine("I'm inside checking out: " + substring[3]);

                    string victimResult, attackerResult, cIDResult;
                    if (UserWithTeam.TryGetValue(substring[3], out victimResult))
                    {
                        handleTeamKills(substring, substring[3], victimResult);
                    }
                    else
                    {
                        foreach(var player in UserWithTeam)
                        {
                            //Console.WriteLine("The player is: " + player);
                            if (substring[3].Contains(player.Key))
                            {
                                
                                handleTeamKills(substring, player.Key, player.Value);
                                break;
                            }
                        }
                    }
                    break;
                case "playerUnPosses":
                    //Add Future Logic for seeing if someone is using admin cam to cheat. 
                    if (adminInCameraDic.ContainsKey(substring[2])){
                        //Console.WriteLine(line);
                        adminInCameraDic[substring[2]] = "Inactive";
                    }
                    break;
                case "playerName":
                    //foreach (var sub in substring)
                    //    Console.WriteLine(sub);
                    C_IDWithUser[C_ID] = substring[2];
                    //if (steamID.Equals(""))
                    //    steamID = "00000000000000000";
                    //_SQL = "INSERT IGNORE INTO userNameList (steamID, userName) VALUES (@steamID, @userName);";
                    //cmd = new MySqlCommand(_SQL, conn);
                    //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                    //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                    //cmd.ExecuteNonQuery();

                    //_SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                    //         ON DUPLICATE KEY UPDATE
                    //         userName = VALUES(userName),
                    //         connected = VALUES(connected)";
                    //cmd = new MySqlCommand(_SQL, conn);
                    //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                    //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                    //cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = 1;
                    //cmd.ExecuteNonQuery();

                    steamID = "";
                    C_ID = "";

                    break;
                case "playerConnected":
                    C_ID = substring[3];
                    //Console.WriteLine(C_ID);
                    //Adding the User C_ID to the Dictionary First, since we don't know the UserName yet. 
                    C_IDWithUser.Add(substring[3], "not defined");

                    break;
                case "UserJoining":
                    if(newUser)
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

                        SteamWithC_ID.Add(substring[3], C_ID);
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
                        }
                    }
                    break;
                case "removeUser":
                    if (SteamWithC_ID.ContainsKey(substring[2]))
                    {
                        //Console.WriteLine("This user: " + userBP_C[userSteamToBP_C[substring[2]]] + " Has decided to leave the server");
                        //Console.WriteLine(setUserNameToC_ID[userSteamToC_ID[substring[2]]]);
                        if (adminInCameraDic.ContainsKey(C_IDWithUser[SteamWithC_ID[substring[2]]]))
                            adminInCameraDic[C_IDWithUser[SteamWithC_ID[substring[2]]]] = "Inactive";

                        try
                        {
                            //_SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                            ////Console.WriteLine(_SQL);
                            //cmd = new MySqlCommand(_SQL, conn);
                            //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                            //cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Disconnected";
                            //cmd.Parameters.Add("@message", MySqlDbType.Text).Value = C_IDWithUser[SteamWithC_ID[substring[2]]] + " left the server";
                            //cmd.ExecuteNonQuery();

                            //_SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                            //         ON DUPLICATE KEY UPDATE
                            //         userName = VALUES(userName),
                            //         connected = VALUES(connected)";
                            //cmd = new MySqlCommand(_SQL, conn);
                            //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                            //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = C_IDWithUser[SteamWithC_ID[substring[2]]];
                            //cmd.Parameters.Add("@connected", MySqlDbType.UInt32).Value = 0;
                            //cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e);
                        }
                        

                        //Console.WriteLine("SteamID/UserName Removed");

                        C_IDWithUser.Remove(SteamWithC_ID[substring[2]]);
                        SteamWithC_ID.Remove(substring[2]);
                    }
                    else
                    {

                        //Console.WriteLine("This user: " + substring[2] + " Has decided to leave the server");
                    }
                    break;
                case "playerPosses":
                    string bp_soldier = "bp_soldier";

                    if (substring[3].ToLower().StartsWith(bp_soldier))
                    {
                        string[] teamID = substring[3].Split("_");
                        if (UserWithTeam.ContainsKey(substring[2]))
                        {
                            UserWithTeam[substring[2]] = teamID[2];
                        }
                        else
                        {
                            UserWithTeam.Add(substring[2], teamID[2]);
                        }
                        //Console.WriteLine("substring[2]: " + substring[2] + "With teamID: " + teamID[2]);

                    }
                    else if (substring[3].ToLower().StartsWith("cameraman"))
                    {
                        adminInCameraList.Add(line);
                        if (adminInCameraDic.ContainsKey(substring[2])){
                            adminInCameraDic[substring[2]] = "Active";
                        }
                        else
                        {
                            adminInCameraDic.Add(substring[2], "Active");
                        }
                        //sendMessageAdmin(substring);
                        //_SQL = "INSERT INTO adminlog (userName, logMessage) VALUES (@userName, @logMessage)";
                        //cmd = new MySqlCommand(_SQL, conn);
                        //cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[2];
                        //cmd.Parameters.Add("@logMessage", MySqlDbType.Text).Value = line;
                        //cmd.ExecuteNonQuery();
                    }
                    break;
                case "chatMessage":
                    //Console.WriteLine(line);
                    
                    //_internalClass.Add(substring);
                    try
                    {
                        _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType, @message)";
                        //Console.WriteLine(_SQL);
                        //cmd = new MySqlCommand(_SQL, conn);
                        //cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        //cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = substring[1];
                        //cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4];
                        //cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e);
                    }
                    
                    break;
                default:
                    //Console.WriteLine("Default was Called");
                    //Console.WriteLine(line);
                    //Console.WriteLine("Default ends");
                    break;
            }
        }

        private void handleTeamKills(string[] substring, string victimName, string victimTeam)
        {
            string attackerResult, cIDResult;
            Console.WriteLine("I'm inside and found player and his team: " + victimName + " : " + victimTeam);
            C_IDWithUser.TryGetValue(substring[4], out cIDResult);
            if (String.IsNullOrEmpty(cIDResult)) { attackerResult = ""; } else { UserWithTeam.TryGetValue(cIDResult, out attackerResult); }


            if (String.IsNullOrEmpty(attackerResult))
            {
                //Console.WriteLine("There was a Potential TeamKill");
                //Console.WriteLine("The Victim Team was: " + victimResult);
                //Console.WriteLine(line);
                _unidenfiedKiller += "Attacker is not part of PlayerList/TeamList; Their ID is: " + substring[4] + " ---  The Victim is: " + victimName + ":" + victimTeam + " \n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\AttackerOnly.txt", _unidenfiedKiller);
            }else 

            if (String.IsNullOrEmpty(victimTeam))
            {
                Console.WriteLine("I'm Inside this victimResult thing");
                _unidenfiedVictim += "The Victim is Unknown in this situation, their name is: " + victimTeam + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\VictimOnly.txt", _unidenfiedVictim);
            }else 

            //Console.WriteLine(substring[2]);
            //Console.WriteLine(result);
            //Console.WriteLine("");
            //Console.WriteLine(C_IDWithUser[substring[4]]);
            //Console.WriteLine(UserWithTeam[C_IDWithUser[substring[4]]]);
            if (victimTeam.Equals(attackerResult))
            {
                //sendMessageTeamKill(substring[2], cIDResult);
                _TeamKill += "True TK, Maybe?, Attacker: " + cIDResult + ":" + attackerResult + " -- The Victim: " + victimName + ":" + victimTeam + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\RealTeamKills.txt", _TeamKill);
                    //Console.WriteLine("There was a TeamKill");
                    //Console.WriteLine(line);
            }
            else
            {
                _unidenfiedKill += "We don't know -- The Attacker: " + cIDResult + ":" + attackerResult + " and the Victim: " + victimName + ":" + victimTeam + "--- The weapon: " + substring[5] + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\UnidenfitedKills.txt", _unidenfiedKill);
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
