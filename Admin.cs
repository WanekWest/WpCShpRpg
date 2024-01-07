using MySqlConnector;

namespace WpCShpRpg
{
    public class Admin
    {
        public string? SteamID { get; set; }
        public string? Name { get; set; }
        public string? Flags { get; set; }
        public int Immunity { get; set; }
        public long EndTime { get; set; }
        public string? Comment { get; set; }

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
                Console.WriteLine($" List of administrators received:");
                foreach (var admin in admins)
                {
                    Console.WriteLine($"SteamID={admin.SteamID}, Name={admin.Name}, Flags={admin.Flags}, Immunity={admin.Immunity}, EndTime={admin.EndTime}, Comment={admin.Comment}");
                }
            }
            else
            {
                Console.WriteLine($" No administrators found in the database.");
            }

            return admins;
        }
    }
}
