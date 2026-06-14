using MySql.Data.MySqlClient;  // MySQL için gerekli namespace
using System;
using System.IO;

namespace personelizintakip
{
    internal class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            // .txt dosyasındaki IP adresi ve portu oku
            var ipAndPort = GetIpAddressAndPortFromConfigFile().Split(':');
            string server = ipAndPort[0];  // IP adresini al
            string port = ipAndPort[1];    // Port numarasını al

            var databaseSettings = GetDatabaseSettings();

            // Bağlantı dizesini oluştur
            return $"Server={server};Database={databaseSettings.Database};User ID={databaseSettings.UserId};Password={databaseSettings.Password};Port={port};SslMode=Required;AllowPublicKeyRetrieval=True;";
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(GetConnectionString());
        }

        private static string GetIpAddressAndPortFromConfigFile()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.txt");
            if (File.Exists(configFilePath))
            {
                return File.ReadAllText(configFilePath).Trim();
            }
            else
            {
                throw new FileNotFoundException("The configuration file was not found.");
            }
        }

        private static (string Database, string UserId, string Password) GetDatabaseSettings()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.local.txt");
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("database.local.txt dosyasi bulunamadi.");
            }

            string database = "personeldb";
            string userId = "personel";
            string? password = null;

            foreach (string rawLine in File.ReadAllLines(configFilePath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim().ToLowerInvariant();
                string value = parts[1].Trim();

                switch (key)
                {
                    case "database":
                        database = value;
                        break;
                    case "user":
                        userId = value;
                        break;
                    case "password":
                        password = value;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("database.local.txt icinde password alani tanimli degil.");
            }

            return (database, userId, password);
        }
    }
}
