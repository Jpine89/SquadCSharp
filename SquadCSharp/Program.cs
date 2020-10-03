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

namespace SquadCSharp
{
    //Index 0: ID: 76
    //Index 1:  SteamID: 76561198044820233
    //Index 2:  Since Disconnect: 03m.11s
    //Index 3:  Name: OperatorAF

    class Program
    {
        static string _20rIP = ConfigurationManager.AppSettings.Get("20r_IP");
        static ushort _20rPort = Convert.ToUInt16(ConfigurationManager.AppSettings.Get("20r_Port"));
        static string _20rPass = ConfigurationManager.AppSettings.Get("20r_Pass");
        static void Main(string[] args)
        {

            Console.WriteLine("Hello World!");
            //PlayerListDic(SteamID, PlayerName
            Dictionary<string, string> PlayerListDic = new Dictionary<string, string>();

            //var rcon = new RCON(IPAddress.Parse("108.61.238.42"), 7789, "=7WteTA}cT={kkAs");
            var response = TestAllAsync().Result;
            int i = 1;
            string[] playListString = response.Split("\n");
            foreach (string value in playListString)
            {
                //ID: 107 | SteamID: 76561198125413977 | Name: [20R] Twomad | Team ID: 2 | Squad ID: 9
                //Console.WriteLine(i++ + ": " + value);
                i++;
                string[] parsedValue = value.Split("|");
                if (!parsedValue[0].Equals("----- Active Players -----\r"))
                {
                    //value   "----- Recently Disconnected Players [Max of 15] -----\r"   string
                    //This second if statement is to tell us when the Active PlayerList ends and the Disconencted Player List starts. 
                    if (!parsedValue[0].Equals("----- Recently Disconnected Players [Max of 15] -----\r"))
                    {
                        //
                        string[] secondParseValue = parsedValue[1].Split(": ");
                        //Console.WriteLine((secondParseValue[1]));
                        string[] ThirdParseValue = parsedValue[2].Split(": ");
                        //Console.WriteLine((ThirdParseValue[1]));
                        PlayerListDic.Add(secondParseValue[1], ThirdParseValue[1]);
                    }
                    else { break; }

                }

            }

            foreach (KeyValuePair<String, string> kvp in PlayerListDic)
            {
                Console.WriteLine("Steam = {0}, Name = {1}", kvp.Key, kvp.Value);
            }

        }

        public static async Task<string> TestAllAsync()
        {
            var rcon = new RCON(IPAddress.Parse(_20rIP), _20rPort, _20rPass, 10000, true);
            await rcon.ConnectAsync();
            Console.WriteLine("Passed Connection Async");
            string test = await rcon.SendCommandAsync("ListPlayers");
            rcon.Dispose();
            return test;
        }
    }


}
