using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CoreRCON;
using CoreRCON.Parsers.Standard;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
//using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using DSharpPlus;
using DSharpPlus.Entities;

namespace SquadCSharp
{
    //Index 0: ID: 76
    //Index 1:  SteamID: 76561198044820233
    //Index 2:  Since Disconnect: 03m.11s
    //Index 3:  Name: OperatorAF
    //testing if git pushes to gitlab
    

    class Program
    {

        static string _20rIP = ConfigurationManager.AppSettings.Get("20r_IP");
        static ushort _20rPort = Convert.ToUInt16(ConfigurationManager.AppSettings.Get("20r_Port"));
        static string _20rPass = ConfigurationManager.AppSettings.Get("20r_Pass");
        static string _login = ConfigurationManager.AppSettings.Get("Login");
        static string _dataBase = ConfigurationManager.AppSettings.Get("DB");
        public DiscordClient Client { get; set; }
        //private SqlConnection _conn;
        static async Task Main(string[] args)
        {

            string lineReturn = "";

            //var test = await prog.Client.GetChannelAsync(787524708546510848);

            InhertianceClass test22 = new InhertianceClass();
            Console.WriteLine(test22._InternalTest);
            Boolean userJoining = false; 

            Test2 tryingSomething = new Test2();

            

            Console.WriteLine(tryingSomething._internalClass._InternalTest);
            Console.WriteLine(tryingSomething._internalClass._InternalTest);
            Regex rg;
            //string fileName = "G:/SquadTestServer/servers/squad_server/SquadGame/Saved/Logs/ChatExample.txt";
            //string fileName = "G:/SquadTestServer/servers/squad_server/SquadGame/Saved/Logs/ServerLog.log";
            //string fileName = "G:/SquadTestServer/servers/squad_server/SquadGame/Saved/Logs/SquadGame.log";
            //string fileName = "G:/SquadTestServer/servers/squad_server/SquadGame/Saved/Logs/SquadReal.log";
            //string fileName = "C:/Users/FubarP/Documents/SquadServerLogs/fb.txt";
            string fileName = "C:/Users/FubarP/Documents/SquadServerLogs/SquadGameWeek.log";
            //string fileName = "G:/SquadTestServer/servers/squad_server/SquadGame/Saved/Logs/SquadGameReal.log";
            Console.WriteLine("Hello World!");
            Console.WriteLine(System.DateTime.Now);
            Boolean done = true;

            string connetionString = null;
            //SqlDataAdapter adapter = new SqlDataAdapter();
            //string str = "INSERT INTO [test] (id) VALUES (78)";
            //string str = @"CREATE TABLE Persons( 
            //              PersonID int, 
            //              LastName varchar(255),  
            //              Address varchar(255), 
            //              City varchar(255) 
            //              )";

            string _sql = "";
            connetionString = $"server={_20rIP};user={_login};Password={_20rPass};port={_20rPort};Database={_dataBase}";
            //connetionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";
            //SqlConnection cnn = new SqlConnection(connetionString);
            MySqlConnection conn = new MySqlConnection(connetionString);
            ////SqlCommand command = new SqlCommand(str, cnn);
            try
            {
                conn.Open();
                Console.WriteLine("Connection Open ! ");
                //MySqlCommand cmd = new MySqlCommand(str, conn);
                //MySqlDataReader rdr = cmd.ExecuteReader();


                //command.ExecuteNonQuery();
                //adapter.InsertCommand = new SqlCommand(str, cnn);
                //adapter.InsertCommand.ExecuteNonQuery();
                //command.Dispose();
                //cnn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection ! ");
            }


            using (StreamReader reader = new StreamReader(new FileStream(fileName,
                     FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                //start at the end of the file
                long lastMaxOffset = reader.BaseStream.Length;

                while (done)
                {
                    System.Threading.Thread.Sleep(100);

                    ////if the file size has not changed, idle
                    //if (reader.BaseStream.Length == lastMaxOffset)
                    //    continue;

                    ////seek to the last max offset
                    //reader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

                    ////read out of the file until the EOF
                    string line = "";

                    //string[] substrings = Regex.Split(line, pattern);
                    string[] subStrings;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) | Regex.IsMatch(line, "SendOutboundMessage")) { continue; }
                        else
                        {
                            foreach (var tester in tryingSomething._AllPatterns)
                            {
                                rg = new Regex(tester.Value);
                                Match match = rg.Match(line);
                                if (match.Success)
                                {
                                    ////Console.WriteLine("We found match with: " + tester);
                                    ///*|| Regex.IsMatch(match.Value, "Error: No teams exist yet")*/
                                    //if (userJoining & Regex.IsMatch(match.Value, "LogEasyAntiCheatServer"))
                                    //{
                                    //    subStrings = Regex.Split(line, tester.Value);
                                    //    tryingSomething.matchList("UserJoining", match.Value, subStrings, conn, userJoining);
                                    //    lineReturn = match.Value;
                                    //    userJoining = false;
                                    //    break;
                                    //}
                                    //else if (Regex.IsMatch(match.Value, "NewPlayer: BP_PlayerController_C"))
                                    //{
                                    //    subStrings = Regex.Split(line, tester.Value);
                                    //    tryingSomething.matchList("playerConnected", match.Value, subStrings, conn);
                                    //    lineReturn = match.Value;
                                    //    userJoining = true;
                                    //    break;
                                    //}
                                    //else 
                                    if (!lineReturn.Equals(match.Value))
                                    {
                                        //Console.WriteLine(match.Value);
                                        subStrings = Regex.Split(line, tester.Value);
                                        tryingSomething.matchList(tester.Key, match.Value, subStrings, conn);

                                        lineReturn = match.Value;
                                    }
                                    //Console.WriteLine(match.Value);

                                    //

                                }
                            }
                        }

                    }


                    //        //Console.WriteLine(lineReturn);
                    //        //update the last max offset
                    //        lastMaxOffset = reader.BaseStream.Position;
                    //    }
                    //}

                    ////foreach (var test in Enum.GetValues(typeof(regexParsers)))
                    ////    Console.WriteLine(test.ToString());

                    ////foreach (object tester in regexPattern)
                    ////    Console.WriteLine(tester.voidTest());

                    done = false;
                }
            }


            conn.Close();
            Console.WriteLine("Done.");

            Console.WriteLine($"Number of peeps connected: {tryingSomething.playerStats.Count}");


            foreach(var p in tryingSomething.playerStats)
            {
                Console.WriteLine($"{p.Key} -> {p.Value[0]} | {p.Value[1]} | {p.Value[2]} | {p.Value[3]} | {p.Value[4]}");
            }


            Console.WriteLine(System.DateTime.Now);


        }

        

        //public static async Task<string> TestAllAsync()
        //{
        //    var rcon = new RCON(IPAddress.Parse(_20rIP), _20rPort, _20rPass, 10000, true);
        //    await rcon.ConnectAsync();



        //    Console.WriteLine("Passed Connection Async");
        //    string test = await rcon.SendCommandAsync("ListPlayers");
        //    rcon.Dispose();
        //    return test;
        //}
    }


}
