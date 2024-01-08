using System.Text.Json;

namespace WpCShpRpg
{
    public class Config
    {
        public CShpRpgDatabaseConfig CShpRpgDatabase { get; set; }

        public static Dictionary<string, string> ParseConfigFile(string filePath)
        {
            var configData = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                {
                    continue; // Skip comments and empty lines
                }

                var parts = line.Split(new[] { ' ' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"'); // Remove extra quotes around the value
                    configData[key] = value;
                }
            }
            return configData;
        }

        private Config CreateDatabaseConfig(string configPath)
        {
            var config = new Config
            {
                CShpRpgDatabase = new CShpRpgDatabaseConfig
                {
                    Host = "",
                    Name = "",
                    User = "",
                    Password = "",
                }
            };

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            return config;
        }

        public Config LoadDatabaseConfig(string ModulePath)
        {
            var moduleDirectoryParent = Directory.GetParent(ModulePath);
            if (moduleDirectoryParent == null)
            {
                throw new InvalidOperationException("Не удалось найти родительский каталог модуля.");
            }

            var parentDirectory = moduleDirectoryParent.Parent;
            if (parentDirectory == null)
            {
                throw new InvalidOperationException("Не удалось найти родительский каталог родительского каталога модуля.");
            }

            var configPath = Path.Combine(parentDirectory.FullName, "configs/mysql.json");

            if (!File.Exists(configPath))
            {
                Console.WriteLine("Не удалось найти базу данных!");
                return CreateDatabaseConfig(ModulePath);
            }

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }

        public class CShpRpgDatabaseConfig
        {
            public required string Host { get; init; }
            public required string Name { get; init; }
            public required string User { get; init; }
            public required string Password { get; init; }
        }
    }
}
