using System;
using System.IO;
using System.Linq;
using System.Net;

namespace RXInstanceManager
{
    public static class AppHelper
    {
        public static string Base64EncodeFromUTF8(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64EncodeFromASCII(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Encode(byte[] plainText)
        {
            return System.Convert.ToBase64String(plainText);
        }

        public static byte[] Base64Decode(string base64EncodedData)
        {
            return System.Convert.FromBase64String(base64EncodedData);
        }

        public static string Base64DecodeToUTF8(string base64EncodedData)
        {
            return System.Text.Encoding.UTF8.GetString(Base64Decode(base64EncodedData));
        }

        public static string Base64DecodeToASCII(string base64EncodedData)
        {
            return System.Text.Encoding.ASCII.GetString(Base64Decode(base64EncodedData));
        }

        public static bool ValidateInputCode(string code)
        {
            return code.Length > 3 && code.Length <= 10 && code.All(x => (x >= 'a' && x <= 'z') || (x >= '0' && x <= '9'));
        }

        public static bool ValidateInputDBName(string name)
        {
            return name.Length >= 3 && name.Length <= 25 && name.All(x => (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9'));
        }

        public static bool ValidateInputPort(string port)
        {
            return port.Length <= 10 && port.All(x => (x >= '0' && x <= '9'));
        }

        public static string GetConfigYamlPath(string instancePath)
        {
            return Path.Combine(instancePath, "etc", "config.yml");
        }

        public static string GetVersionsPath(string instancePath)
        {
            return Path.Combine(instancePath, "etc", "_builds", "version.txt");
        }

        public static string GetDoPath(string instancePath)
        {
            return Path.Combine(instancePath, "do.bat");
        }

        public static string GetDDSPath(string instancePath)
        {
            return Path.Combine(instancePath, "etc", "_builds", "DevelopmentStudio", "bin", "DevelopmentStudio.exe");
        }

        public static string GetClientURL(string protocol, string host, int port)
        {
            return $"{protocol}://{host}:{port}/Client";
        }

        public static string GetDBNameFromConnectionString(string engine, string connectionString)
        {
            var databaseNameParam = connectionString.Split(';').FirstOrDefault(x => x.Contains("initial catalog"));
            if (databaseNameParam != null)
                return databaseNameParam.Split('=')[1];

            return null;
        }

        public static bool CheckInstance(string url)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
