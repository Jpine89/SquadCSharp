using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SquadCSharp
{
    class Test2
    {
        public InternalClass _internalClass;
        public Dictionary<string, string> _AllPatterns;

        //C_ID = Key, UserName = Value
        public Dictionary<string, string> setUserNameToC_ID;

        //SteamID = Key, C_ID = Value
        private Dictionary<string, string> userSteamToC_ID;

        //Sets something to Team
        public Dictionary<string, string> userSetToTeam;
        public List<string> adminInCameraList;
        public Dictionary<string, string> adminInCameraDic;
        private string C_ID;
        public String voidTest()
        {
            return "It worked";
        }

        public Test2()
        {
            C_ID = "";
            adminInCameraList = new List<string>();
            adminInCameraDic = new Dictionary<string, string>();
            userSetToTeam = new Dictionary<string, string>();
            setUserNameToC_ID = new Dictionary<string, string>();
            userSteamToC_ID = new Dictionary<string, string>();
            _AllPatterns = new Dictionary<string, string>();
            _internalClass = new InternalClass();
            regexSetup();
        }

        private void regexSetup()
        {
            _AllPatterns.Add("playerConnected", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: PostLogin: NewPlayer: BP_PlayerController_C \\/Game\\/Maps\\/[A-z]+\\/(?:Gameplay_Layers\\/)?[A-z0-9_]+.[A-z0-9_]+:PersistentLevel.BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("steamID", "\\[([0-9.:-]+)]\\[[ 0-9]*]LogEasyAntiCheatServer: \\[[0-9:]+]\\[[A-z]+]\\[EAC Server] \\[Info]\\[RegisterClient] Client: ([A-z0-9]+) PlayerGUID: ([0-9]{17}) PlayerIP: [0-9]{17} OwnerGUID: [0-9]{17} PlayerName: (.+)");
            _AllPatterns.Add("chatMessage", "\\[(ChatAll|ChatTeam|ChatSquad|ChatAdmin)] \\[SteamID:([0-9]{17})] (.+?) : (.*)");
            _AllPatterns.Add("removeUser", "\\[([0-9.:-]+)][[ 0-9]+]LogNet: UChannel::Close: [A-z0-9_ ,.=:]+ RemoteAddr: ([0-9]+):[A-z0-9_ ,.=:]+ BP_PlayerController_(C_[0-9]+)");
            _AllPatterns.Add("adminBroadcast", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: ADMIN COMMAND: Message broadcasted <(.+)> from (.+)");
            _AllPatterns.Add("newGame", "\\[([0-9.:-]+)][[ 0-9]*]LogWorld: Bringing World \\/([A-z]+)\\/Maps\\/([A-z]+)\\/(?:Gameplay_Layers\\/)?([A-z0-9_]+)");
            _AllPatterns.Add("playerDamaged", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: Player:(.+) ActualDamage=([0-9.]+) from (.+) caused by ([A-z_0-9]+)_C");
            _AllPatterns.Add("playerDied", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQSoldier::)?Die\\(\\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from BP_PlayerController_([A-z_0-9]+) caused by ([A-z_0-9]+)");
            _AllPatterns.Add("playerPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQPlayerController::)?OnPossess\\(\\): PC=(.+) Pawn=([A-z0-9_]+)_C");
            _AllPatterns.Add("playerUnPosses", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQPlayerController::)?OnUnPossess\\(\\): PC=(.+)");
            _AllPatterns.Add("playerRevived", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: (.+) has revived (.+)\\.");
            _AllPatterns.Add("playerWounded", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer](?:ASQSoldier::)?Wound\\(\\): Player:(.+) KillingDamage=(?:-)*([0-9.]+) from ([A-z_0-9]+) caused by ([A-z_0-9]+)_C");
            //_AllPatterns.Add("serverTick", "\\[([0-9.:-]+)][[ 0-9]*]LogSquad: USQGameState: Server Tick Rate: ([0-9.]+)");
            _AllPatterns.Add("roundWinner", "\\[([0-9.:-]+)][[ 0-9]*]LogSquadTrace: \\[DedicatedServer]ASQGameMode::DetermineMatchWinner\\(\\): (.+) won on (.+)");
            _AllPatterns.Add("playerList", "/ID: ([0-9]+) \\| SteamID: ([0-9]{17}) \\| Name: (.+) \\| Team ID: ([0-9]+) \\| Squad ID: ([0-9]+|N\\/A)");
            _AllPatterns.Add("currentMap", "/^Current map is (.+), Next map is (.*)/");
        }

        public void matchList(string stringType, string line , string[] substring, MySqlConnection conn, Boolean newUser = false)
        {
            String _SQL;
            switch (stringType)
            {
                case "playerDied":
                    //substring = 7 length, 0 - 7 are empty
                    //userSetToTeam
                    //setUserNameToC_ID
                    //userSteamToC_ID
                    //Console.WriteLine(line);
                    //foreach (var test in substring)
                    //    Console.WriteLine(test);
                    break;
                case "playerUnPosses":
                    //Add Future Logic for seeing if someone is using admin cam to cheat. 
                    if (adminInCameraDic.ContainsKey(substring[2])){
                        adminInCameraDic[substring[2]] = "Inactive";
                    }
                    break;
                case "UserJoining":
                    if (!newUser)
                    {
                        //Console.WriteLine("We are inside the if..");
                        //for (int i = 0; i < substring.Length; i++)
                        //{
                        //    Console.WriteLine("I is: " + i + " the string is: " + substring[i]);
                        //}
                        //Console.WriteLine(substring.Length);
                        C_ID = substring[2];
                        //Console.WriteLine(C_ID);
                        //Adding the User C_ID to the Dictionary First, since we don't know the UserName yet. 
                        setUserNameToC_ID.Add(substring[2], "not defined");
                    }
                    else if(newUser)
                    {
                        //Console.WriteLine("We are inside the if..");
                        //for (int i = 0; i < substring.Length; i++)
                        //{
                        //    Console.WriteLine("I is: " + i + " the string is: " + substring[i]);
                        //}

                        //Console.WriteLine("User: " + substring[4] + " Has Joined the Server, with C_ID: " + C_ID + " and SteamID: " + substring[3]);
                        //Now that we have the User Name, we are combining that with the C_ID. 
                        setUserNameToC_ID[C_ID] = substring[4];

                        //Game Logs use SteamID to see who leaves. 
                        //We are setting that as the Key, and it's value as the C_ID
                        userSteamToC_ID.Add(substring[3], C_ID);
                        try
                        {
                            _SQL = "INSERT INTO steamuser (steamID) VALUES (" + Int64.Parse(substring[3]) + ");";
                            MySqlCommand cmd = new MySqlCommand(_SQL, conn);
                            cmd.ExecuteNonQuery();

                            _SQL = "INSERT INTO userNameList (steamID, userName) VALUES (" + Int64.Parse(substring[3]) + ", '" + substring[4] + "');";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.ExecuteNonQuery();

                            _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                            Console.WriteLine(_SQL);
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                            cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Connected";
                            cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4] + " joined the server";
                            cmd.ExecuteNonQuery();

                            _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                                     ON DUPLICATE KEY UPDATE
                                     userName = VALUES(@userName),
                                     connected = VALUES(@connected)";
                            cmd = new MySqlCommand(_SQL, conn);
                            cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[3]);
                            cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = substring[4];
                            cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = 1;
                            cmd.ExecuteNonQuery();

                            Console.WriteLine("SteamID/UserName Added");
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine(e);
                        }
                    }
                    break;
                case "removeUser":
                    if (userSteamToC_ID.ContainsKey(substring[2]))
                    {
                        //Console.WriteLine("This user: " + userBP_C[userSteamToBP_C[substring[2]]] + " Has decided to leave the server");
                        //Console.WriteLine(setUserNameToC_ID[userSteamToC_ID[substring[2]]]);
                        if (adminInCameraDic.ContainsKey(setUserNameToC_ID[userSteamToC_ID[substring[2]]]))
                            adminInCameraDic[setUserNameToC_ID[userSteamToC_ID[substring[2]]]] = "Inactive";


                        _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                        Console.WriteLine(_SQL);
                        MySqlCommand cmd = new MySqlCommand(_SQL, conn);
                        cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = "Disconnected";
                        cmd.Parameters.Add("@message", MySqlDbType.Text).Value = setUserNameToC_ID[userSteamToC_ID[substring[2]]] + " left the server";
                        cmd.ExecuteNonQuery();

                        _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                                     ON DUPLICATE KEY UPDATE
                                     userName = VALUES(@userName),
                                     connected = VALUES(@connected)";
                        cmd = new MySqlCommand(_SQL, conn);
                        cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = setUserNameToC_ID[userSteamToC_ID[substring[2]]];
                        cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = 0;
                        cmd.ExecuteNonQuery();

                        setUserNameToC_ID.Remove(userSteamToC_ID[substring[2]]);
                        userSteamToC_ID.Remove(substring[2]);
                    }
                    else
                    {
                        //Console.WriteLine("This user: " + substring[2] + " Has decided to leave the server");
                    }
                    break;
                case "playerPosses":
                    string bp_soldier = "bp_soldier";
                    //foreach (var sub in substring)
                    //    Console.WriteLine(sub);
                    if (substring[3].ToLower().StartsWith(bp_soldier))
                    {
                        string[] teamID = substring[3].Split("_");
                        if (userSetToTeam.ContainsKey(substring[2]))
                        {
                            userSetToTeam[substring[2]] = teamID[2];
                        }
                        else
                        {
                            userSetToTeam.Add(substring[2], teamID[2]);
                        }

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
                    }
                    break;
                case "chatMessage":
                    //Console.WriteLine(line);
                    //_internalClass.Add(substring);
                    try
                    {
                        _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType, @message)";
                        Console.WriteLine(_SQL);
                        MySqlCommand cmd = new MySqlCommand(_SQL, conn);
                        cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(substring[2]);
                        cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = substring[1];
                        cmd.Parameters.Add("@message", MySqlDbType.Text).Value = substring[4];
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    break;
                default:
                    //Console.WriteLine("Default was Called");
                    //Console.WriteLine(line);
                    //Console.WriteLine("Default ends");
                    break;
            }
        }
    }
}
