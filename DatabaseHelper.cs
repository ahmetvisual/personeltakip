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

            string database = Environment.GetEnvironmentVariable("PERSONEL_DB_NAME") ?? "personeldb";
            string userId = Environment.GetEnvironmentVariable("PERSONEL_DB_USER") ?? "personel";
            string password = Environment.GetEnvironmentVariable("PERSONEL_DB_PASSWORD")
                ?? throw new InvalidOperationException("PERSONEL_DB_PASSWORD ortam degiskeni tanimli degil.");

            // Bağlantı dizesini oluştur
            return $"Server={server};Database={database};User ID={userId};Password={password};Port={port};SslMode=Required;AllowPublicKeyRetrieval=True;";
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
    }
}
