namespace TarbikMap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using TarbikMap.Common;

    public class EnvironmentConfig
    {
        private static readonly ReasonableStringChecker StringCheckerForEnvironmentName = new ReasonableStringChecker().AllowMaxLength(20).AllowLowercaseLetters().AllowNumbers();

        private string environment;
        private Dictionary<string, string> privateConfigValues;

        public EnvironmentConfig()
        {
            var environmentName = EnvironmentVariables.GetValue("TARBIKMAP_ENVIRONMENT");

            if (!StringCheckerForEnvironmentName.Check(environmentName))
            {
                throw new ArgumentException("Invalid environment name: " + environmentName);
            }

            this.environment = environmentName;

            this.privateConfigValues = this.LoadPrivateConfigValues();
        }

        public Stream GetPublicConfigFile(string fileName)
        {
            return new FileStream(Path.Combine("config", this.environment, "public", fileName), FileMode.Open, FileAccess.Read);
        }

        public bool HasPrivateConfigValue(string key)
        {
            string? result;

            this.privateConfigValues.TryGetValue(key, out result);
            return !string.IsNullOrEmpty(result);
        }

        public string GetPrivateConfigValue(string key)
        {
            string? result;

            this.privateConfigValues.TryGetValue(key, out result);
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentException("Value not configured: " + key);
            }

            return result;
        }

        private Dictionary<string, string> LoadPrivateConfigValues()
        {
            string str;
            using (StreamReader sr = new StreamReader(Path.Combine("config", this.environment, "private", "values.json")))
            {
                str = sr.ReadToEnd();
            }

            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(str);
            if (result == null)
            {
                throw new ArgumentException("Config values are null");
            }

            return result;
        }
    }
}