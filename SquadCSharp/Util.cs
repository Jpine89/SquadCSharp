using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SquadCSharp
{
    class Util
    {
        public Boolean logRevives(MySqlConnection conn, string medic)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {

                _SQL = $"UPDATE steamuser SET revives = revives + 1 WHERE steamID = {Int64.Parse(medic)}";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += _SQL.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }

        public Boolean logWounds(MySqlConnection conn, string[] victim, string[] attacker, string[] substring, Boolean teamkill = false, int serverID = 0)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = @"INSERT INTO playerWounded (victim, victimTeam, victimSteamID, attacker, attackerTeam, attackerSteamID, weaponUsed, teamKill, date, time, serverID) 
                                    VALUES (@victim, @victimTeam, @victimSteamID, @attacker, @attackerTeam, @attackerSteamID, @weaponUsed, @teamKill, @date, @time, @serverID)";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@victim", MySqlDbType.VarChar).Value = victim[0];
                cmd.Parameters.Add("@victimTeam", MySqlDbType.VarChar).Value = victim[1];
                cmd.Parameters.Add("@victimSteamID", MySqlDbType.Int64).Value = Int64.Parse(victim[2]);
                cmd.Parameters.Add("@attacker", MySqlDbType.VarChar).Value = attacker[0];
                cmd.Parameters.Add("@attackerTeam", MySqlDbType.VarChar).Value = attacker[1];
                cmd.Parameters.Add("@attackerSteamID", MySqlDbType.Int64).Value = Int64.Parse(attacker[2]);
                cmd.Parameters.Add("@weaponUsed", MySqlDbType.VarChar).Value = substring[5];
                cmd.Parameters.Add("@teamKill", MySqlDbType.Int32).Value = teamkill ? 1 : 0;
                cmd.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.Parse(substring[1]).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@time", MySqlDbType.Time).Value = TimeSpan.Parse(substring[2].Replace(".", ":"));
                cmd.Parameters.Add("@serverID", MySqlDbType.Int32).Value = serverID;
                cmd.ExecuteNonQuery();

                _SQL = $"UPDATE steamuser SET wounded = wounded + 1 WHERE steamID = {Int64.Parse(attacker[2])}";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.ExecuteNonQuery();


                _SQL = "UPDATE steamuser SET teamkills = teamkills + 1 WHERE steamID = " + Int64.Parse(attacker[2]);
                cmd = new MySqlCommand(_SQL, conn);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += _SQL.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }

        public Boolean logKills(MySqlConnection conn, string[] victim, string[] attacker, string[] substring, int serverID = 0)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = @"INSERT INTO playerKilled (victim, victimSteamID, attacker, attackerSteamID, date, time, serverID) 
                                    VALUES (@victim, @victimSteamID, @attacker,  @attackerSteamID, @date, @time, @serverID)";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@victim", MySqlDbType.VarChar).Value = victim[0];
                cmd.Parameters.Add("@victimSteamID", MySqlDbType.Int64).Value = Int64.Parse(victim[1]);
                cmd.Parameters.Add("@attacker", MySqlDbType.VarChar).Value = attacker[0];
                cmd.Parameters.Add("@attackerSteamID", MySqlDbType.Int64).Value = Int64.Parse(attacker[1]);
                cmd.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.Parse(substring[1]).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@time", MySqlDbType.Time).Value = TimeSpan.Parse(substring[2].Replace(".", ":"));
                cmd.Parameters.Add("@serverID", MySqlDbType.Int32).Value = serverID;
                cmd.ExecuteNonQuery();

                _SQL = $"UPDATE steamuser SET kills = kills + 1 WHERE steamID = {Int64.Parse(attacker[1])}";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.ExecuteNonQuery();

                _SQL = $"UPDATE steamuser SET deaths = deaths + 1 WHERE steamID = {Int64.Parse(victim[1])}";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += e.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }

        }

        public Boolean logChats(MySqlConnection conn, string steamID, string chatType, string message)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {

                _SQL = "INSERT INTO chatLog (steamID, chatType, message) VALUES (@steamID, @chatType , @message)";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                cmd.Parameters.Add("@chatType", MySqlDbType.VarChar).Value = chatType;
                cmd.Parameters.Add("@message", MySqlDbType.Text).Value = message;
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += e.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }

        }

        public Boolean logPlayerList(MySqlConnection conn, string steamID, string userName, bool connected = false)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = @"INSERT INTO playerList(steamID, userName, connected) VALUES(@steamID, @userName, @connected)
                             ON DUPLICATE KEY UPDATE
                             userName = VALUES(userName),
                             connected = VALUES(connected)";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@connected", MySqlDbType.Int32).Value = connected ? 1 : 0;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += _SQL + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }

        public Boolean logUserNameList(MySqlConnection conn, string steamID, string userName)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = "INSERT IGNORE INTO userNameList (steamID, userName) VALUES (@steamID, @userName);";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = userName;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += e.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }

        /*
         * This needs to be refactored to record date/time separately
         */
        public Boolean logAdmin(MySqlConnection conn, string userName, string line)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = "INSERT INTO adminlog (userName, logMessage) VALUES (@userName, @logMessage)";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = userName;
                cmd.Parameters.Add("@logMessage", MySqlDbType.Text).Value = line;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += e.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }

        public Boolean logSteamUser(MySqlConnection conn, string steamID)
        {
            string _SQL = "";
            MySqlCommand cmd;
            string _errorLog = "";
            try
            {
                _SQL = "INSERT IGNORE INTO steamuser (steamID) VALUES (@steamID);";
                cmd = new MySqlCommand(_SQL, conn);
                cmd.Parameters.Add("@steamID", MySqlDbType.Int64).Value = Int64.Parse(steamID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _errorLog += e.ToString() + "\n";
                System.IO.File.WriteAllText(@"C:\Users\FubarP\Documents\SquadTestFiles\_ErrorLog.txt", _errorLog);
                return false;
            }
        }
    }
}
