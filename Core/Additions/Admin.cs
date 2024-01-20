using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MySqlConnector;

namespace WpCShpRpg
{
    public class Admin
    {
        private List<Admin> admins;

        public Admin(string connectionString)
        {
            admins = LoadAdminsFromDatabase(connectionString);
        }

        public Admin()
        {
        }

        public string? SteamID { get; set; }
        public string? Name { get; set; }
        public string? Flags { get; set; }
        public int Immunity { get; set; }
        public long EndTime { get; set; }
        public string? Comment { get; set; }

        public const char ADMFLAG_RESERVATION = 'a';
        public const char ADMFLAG_GENERIC = 'b';
        public const char ADMFLAG_KICK = 'c';
        public const char ADMFLAG_BAN = 'd';
        public const char ADMFLAG_UNBAN = 'e';
        public const char ADMFLAG_SLAY = 'f';
        public const char ADMFLAG_CHANGEMAP = 'g';
        public const char ADMFLAG_CONVARS = 'h';
        public const char ADMFLAG_CONFIG = 'i';
        public const char ADMFLAG_CHAT = 'j';
        public const char ADMFLAG_VOTE = 'k';
        public const char ADMFLAG_PASSWORD = 'l';
        public const char ADMFLAG_RCON = 'm';
        public const char ADMFLAG_CHEATS = 'n';
        public const char ADMFLAG_CUSTOM1 = 'o';
        public const char ADMFLAG_CUSTOM2 = 'p';
        public const char ADMFLAG_CUSTOM3 = 'q';
        public const char ADMFLAG_CUSTOM4 = 'r';
        public const char ADMFLAG_CUSTOM5 = 's';
        public const char ADMFLAG_CUSTOM6 = 't';
        public const char ADMFLAG_CUSTOM7 = 'u';
        public const char ADMFLAG_CUSTOM8 = 'v';
        public const char ADMFLAG_CUSTOM9 = 'w';
        public const char ADMFLAG_CUSTOM10 = 'x';
        public const char ADMFLAG_CUSTOM11 = 'y';
        public const char ADMFLAG_ROOT = 'z';

        public bool HasFlag(char requiredFlag)
        {
            return Flags?.IndexOf(requiredFlag) >= 0;
        }

        public List<Admin> LoadAdminsFromDatabase(string connectionString)
        {
            List<Admin> admins = new();

            using (MySqlConnection connection = new(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM `as_admins`";
                MySqlCommand command = new(query, connection);
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Admin admin = new()
                    {
                        SteamID = reader.GetString("steamid"),
                        Name = reader.GetString("name"),
                        Flags = reader.GetString("flags"),
                        Immunity = reader.GetInt32("immunity"),
                        EndTime = reader.GetInt64("end"),
                        Comment = reader.GetString("comment")
                    };

                    admins.Add(admin);
                }
            }

            if (admins.Count > 0)
            {
                Server.PrintToConsole($" List of administrators received:");
                foreach (var admin in admins)
                {
                    Server.PrintToConsole($"SteamID={admin.SteamID}, Name={admin.Name}, Flags={admin.Flags}, Immunity={admin.Immunity}, EndTime={admin.EndTime}, Comment={admin.Comment}");
                }
            }
            else
            {
                Server.PrintToConsole($" No administrators found in the database.");
            }

            return admins;
        }

        private bool IsClientHavingThatFlag(CCSPlayerController? Client, char Flag)
        {
            if (Client != null)
            {
                string m_steamID = Client.SteamID.ToString();
                Admin? adminInfo = admins.Find(a => a.SteamID == m_steamID);
                if (adminInfo != null && adminInfo.HasFlag(Flag))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
