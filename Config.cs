namespace WpCShpRpg
{
    public class Config
    {
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
    }
}
