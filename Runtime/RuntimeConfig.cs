using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unity.RemoteConfig
{
    /// <summary>
    /// This class represents a single runtime settings configuration. Access its methods and properties via the <c>ConfigManager.appConfig</c> wrapper.
    /// </summary>
    public class RuntimeConfig
    {
        /// <summary>
        /// The Remote Config service generates this unique ID on configuration requests, for reporting and analytic purposes. Returns null if there is no assignmentId yet.
        /// </summary>
        /// <returns>
        /// A unique string.
        /// </returns>
        public string assignmentID {
            get {
                try
                {
                    return metadata["assignmentId"].Value<string>();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// The Environment ID that has been returned by the Remote Config Delivery service.
        /// </summary>
        /// <returns>
        /// A string of the environmentID returned.
        /// </returns>
        public string environmentID {
            get {
                try
                {
                    return metadata["environmentId"].Value<string>();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrieves the origin point from which your configuration settings loaded.
        /// </summary>
        /// <returns>
        /// An enum describing the origin point of your most recently loaded configuration settings.
        /// </returns>
        public ConfigOrigin origin { get; internal set; }

        internal JObject _config;
        internal JObject metadata;
        string configKey;
        JsonSerializerSettings rawDateSettings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

        /// <summary>
        /// Returns a copy of the entire config as a JObject.
        /// </summary>
        public JObject config
        {
            get
            {
                return (JObject)_config.DeepClone();
            }
        }

        internal RuntimeConfig(ConfigManagerImpl _configmanagerImpl, string configKey)
        {
            this.configKey = configKey;
            _configmanagerImpl.ResponseParsed += ConfigManager_ResponseParsed;
            origin = ConfigOrigin.Default;
            _config = new JObject();
        }

        private void ConfigManager_ResponseParsed(ConfigResponse configResponse, JObject obj)
        {
            if(configResponse.status == ConfigRequestStatus.Success)
            {
                if (obj[configKey] != null && obj[configKey + "Metadata"] != null)
                {
                    _config = (JObject)obj[configKey];
                    metadata = (JObject)obj[configKey + "Metadata"];
                    origin = configResponse.requestOrigin;
                }
            }
        }

        /// <summary>
        /// Retrieves the boolean value of a corresponding key, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>A bool representation of the key from the remote service, if one exists. If one does not exist, the defaultValue is returned (false if none is supplied.)</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                return _config[key].Value<bool>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieves the float value of a corresponding key from the remote service, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>A float representation of the key from the remote service, if one exists. If one does not exist, the defaultValue is returned (0.0F if none is supplied.)</returns>
        public float GetFloat(string key, float defaultValue = 0.0F)
        {
            try
            {
                return _config[key].Value<float>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieves the int value of a corresponding key, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>An int representation of the key from the remote service, if one exists. If one does not exist, the defaultValue is returned (0 if none is supplied.)</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            try
            {
                return _config[key].Value<int>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieves the string value of a corresponding key from the remote service, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>A string representation of the key from the remote service, if one exists. If one does not exist, the defaultValue is returned ("" if none is supplied.)</returns>
        public string GetString(string key, string defaultValue = "")
        {
            try
            {
                var formattedInputString = string.IsNullOrEmpty(_config[key].Value<string>()) ? "" : _config[key].Value<string>();
                DateTime dateValue;
                if (DateTime.TryParse(formattedInputString, out dateValue))
                {
                    formattedInputString = JsonConvert.SerializeObject(_config[key], rawDateSettings).Replace("\"", "");
                }
                return formattedInputString;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieves the long value of a corresponding key from the remote service, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>A long representation of the key from the remote service, if one exists. If one does not exist, the defaultValue is returned (0L if none is supplied.)</returns>
        public long GetLong(string key, long defaultValue = 0L)
        {
            try
            {
                return _config[key].Value<long>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Checks if a corresponding key exists in your remote settings.
        /// </summary>
        /// <returns><c>true</c>, if the key exists, or <c>false</c> if it doesn't.</returns>
        /// <param name="key">The key to search for.</param>
        public bool HasKey(string key)
        {
            if(_config == null)
            {
                return false;
            }
            return _config[key] == null ? false : true;
        }

        /// <summary>
        /// Returns all keys in your remote settings, as an array.
        /// </summary>
        /// <returns>An array of properties within config, if one exists. If one does not exist, empty string array is supplied.</returns>
        public string[] GetKeys()
        {
            try
            {
                return _config.Properties().Select(prop => prop.Name).ToArray<string>();
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Retrieves the string representation of the JSON value of a corresponding key from the remote service, if one exists.
        /// </summary>
        /// <param name="key">The key identifying the corresponding setting.</param>
        /// <param name="defaultValue">The default value to use if the specified key cannot be found or is unavailable.</param>
        /// <returns>A string representation of the JSON value of a corresponding key from the remote service, if one exists. If one does not exist, the defaultValue is returned ("{}" if none is supplied.)</returns>
        public string GetJson(string key, string defaultValue = "{}")
        {
            try
            {
                return _config[key].ToString();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}