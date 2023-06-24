using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig.WebRequest;

[assembly: InternalsVisibleTo("Unity.Services.RemoteConfig.Tests")]

namespace Unity.Services.RemoteConfig
{
    public class ConfigManagerImpl
    {
        internal const string DefaultConfigKey = "settings";
        internal const string DefaultCacheFile = "RemoteConfigCache.json";
        /// <summary>
        /// Retrieves the <c>RuntimeConfig</c> object for handling Remote Config settings.
        /// </summary>
        /// <remarks>
        /// <para> Use this property to access the following <c>RuntimeConfig</c> methods and classes:</para>
        /// <para><c>public string assignmentID</c> is a unique string identifier used for reporting and analytic purposes. The Remote Config service generate this ID upon configuration requests.</para>
        /// <para><c>public bool GetBool (string key, bool defaultValue)</c> retrieves the boolean value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public float GetFloat (string key, float defaultValue)</c> retrieves the float value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public long GetLong (string key, long defaultValue)</c> retrieves the long value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public int GetInt (string key, int defaultValue)</c> retrieves the integer value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public string GetString (string key, string defaultValue)</c> retrieves the string value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public bool HasKey (string key)</c> checks if a corresponding key exists in your remote settings.</para>
        /// <para><c>public string[] GetKeys ()</c> returns all keys in your remote settings, as an array.</para>
        /// <para><c>public string[] GetJson ()</c> returns string representation of the JSON value of a corresponding key from the remote service, if one exists.</para>
        /// </remarks>
        /// <returns>
        /// A class representing a single runtime settings configuration.
        /// </returns>

        private RuntimeConfig _appConfig;
        public RuntimeConfig appConfig
        {
            get
            {
                if (_appConfig != null)
                {
                    return _appConfig;
                }

                return configs.ContainsKey(DefaultConfigKey) ? configs[DefaultConfigKey] : null;
            }
            internal set
            {
                if (value != null && !string.IsNullOrEmpty(value.configType))
                {
                    configs[value.configType] = value;
                    _appConfig = value;
                }
            }
        }
        internal Dictionary<string, RuntimeConfig> configs;
        /// <summary>
        /// Player Identity Token used for Remote Config request authentication.
        /// </summary>
        string _playerIdentityToken;

        internal RemoteConfigRequest _remoteConfigRequest;
        UnityAttributes _unityAttributes;

        internal List<Func<RequestHeaderTuple>> requestHeaderProviders = new List<Func<RequestHeaderTuple>>();
        internal List<Func<Dictionary<string, string>, string, bool>> rawResponseValidators = new List<Func<Dictionary<string, string>, string, bool>>();

        internal string cacheFile;
        internal string originService;
        internal string attributionMetadataStr;
        internal const string pluginVersion = "4.0.1";

        internal const string remoteConfigUrl = "https://config.services.api.unity.com/settings";

        internal const string authInitError = "Auth Service not initialized.\nRequest might result in empty or incomplete response\nPlease refer to https://docs.unity3d.com/Packages/com.unity.remote-config@3.0/manual/CodeIntegration.html";
        internal const string coreInitError = "Core Service not initialized.\nRequest might result in empty or incomplete response\nPlease refer to https://docs.unity3d.com/Packages/com.unity.remote-config@3.0/manual/CodeIntegration.html";

        /// <summary>
        /// This event fires when the configuration manager successfully fetches settings from the service.
        /// </summary>
        /// <returns>
        /// A struct representing the response of a Remote Config fetch.
        /// </returns>
        internal event Action<ConfigResponse> FetchCompleted;

        /// <summary>
        /// Constructor for the ConfigManagerImpl.
        /// </summary>
        /// <param name="originService">Represents the origin for request, e.g 'GameSim'</param>
        /// <param name="attributionMetadataStr">An attribution string to ascribe metadata </param>
        /// <param name="cacheFileRC">remote config cache file</param>
        public ConfigManagerImpl(string originService, string attributionMetadataStr = "", string cacheFileRC = DefaultCacheFile)
        {
            configs = new Dictionary<string, RuntimeConfig>();
            cacheFile = cacheFileRC;
            appConfig = new RuntimeConfig("settings");

            _remoteConfigRequest = new RemoteConfigRequest
            {
                projectId = Application.cloudProjectId ?? "",
                userId = "",
                isDebugBuild = Debug.isDebugBuild,
                configType = "",
                playerId = "",
                analyticsUserId = "",
                configAssignmentHash = null,
                packageVersion = pluginVersion + "+RCR",
                originService = originService,
            };

            if (!string.IsNullOrEmpty(attributionMetadataStr))
            {
                try
                {
                    _remoteConfigRequest.attributionMetadata = JObject.Parse(attributionMetadataStr);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("attributionMetadata is not valid JSON:\n" + attributionMetadataStr + "\n" + e);
                }
            }

            _unityAttributes = new UnityAttributes();

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
            FetchCompleted += SaveCache;
            LoadFromCache();
            #endif
        }

        internal ConfigResponse ParseResponse(ConfigOrigin origin, Dictionary<string, string> headers, string body)
        {
            var configResponse = new ConfigResponse
            {
                requestOrigin = origin,
                headers = headers
            };
            if (body == null || headers == null)
            {
                configResponse.status = ConfigRequestStatus.Failed;
                return configResponse;
            }
            foreach (var validationFunc in rawResponseValidators)
            {
                if (validationFunc(headers, body) == false)
                {
                    configResponse.status = ConfigRequestStatus.Failed;
                    return configResponse;
                }
            }

            try
            {
                var responseJObj = JObject.Parse(body);
                configResponse.body = responseJObj;
                configResponse.status = ConfigRequestStatus.Success;
            }
            catch (Exception e)
            {
                Debug.LogWarning("config response is not valid JSON:\n" + configResponse.body + "\n" + e);
                configResponse.status = ConfigRequestStatus.Failed;
            }

            return configResponse;
        }

        /// <summary>
        /// Sets a custom user identifier for the Remote Config request payload.
        /// </summary>
        /// <param name="customUserID">Custom user identifier.</param>
        public void SetCustomUserID(string customUserID)
        {
            _remoteConfigRequest.customUserId = customUserID;
        }

        /// <summary>
        /// Sets an environment identifier in the Remote Config request payload.
        /// </summary>
        /// <param name="environmentID">Environment unique identifier.</param>
        public void SetEnvironmentID(string environmentID)
        {
            _remoteConfigRequest.environmentId = environmentID;
        }

        /// <summary>
        /// Sets player Identity Token.
        /// </summary>
        /// <param name="identityToken">Player Identity identifier.</param>
        public void SetPlayerIdentityToken(string identityToken)
        {
            _playerIdentityToken = identityToken;
        }

        /// <summary>
        /// Sets userId to InstallationID identifier coming from core services.
        /// </summary>
        /// <param name="iid">Installation unique identifier.</param>
        public void SetUserID(string iid)
        {
            _remoteConfigRequest.userId = iid;
        }

        /// <summary>
        /// Sets playerId identifier coming from auth services.
        /// </summary>
        /// <param name="playerID">Player Id unique identifier.</param>
        public void SetPlayerID(string playerID)
        {
            _remoteConfigRequest.playerId = playerID;
        }

        /// <summary>
        /// Sets analyticsUserId identifier coming from core services.
        /// </summary>
        /// <param name="analyticsUserID">analyticsUserId unique identifier.</param>
        public void SetAnalyticsUserID(string analyticsUserID)
        {
            _remoteConfigRequest.analyticsUserId = analyticsUserID;
        }

        /// <summary>
        /// Sets configAssignmentHash identifier.
        /// </summary>
        /// <param name="configAssignmentHashID">configAssignmentHash unique identifier.</param>
        public void SetConfigAssignmentHash(string configAssignmentHashID)
        {
            _remoteConfigRequest.configAssignmentHash = configAssignmentHashID;
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        public async Task<RuntimeConfig> FetchConfigsAsync(string configType, object userAttributes, object appAttributes, object filterAttributes)
        {
            if (string.IsNullOrEmpty(configType))
            {
                configType = "settings";
            }

            appConfig = GetConfig(configType);
            appConfig.RequestStatus = ConfigRequestStatus.Pending;
            var jsonText = PreparePayloadWithConfigType(configType, userAttributes, appAttributes, filterAttributes);

            ConfigResponse configResponse;

            using (var request = new UnityWebRequest
                   {
                       method = UnityWebRequest.kHttpVerbPOST,
                       timeout = 10,
                       url = remoteConfigUrl
                   })
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + _playerIdentityToken);
                request.SetRequestHeader("unity-installation-id", _remoteConfigRequest.userId);
                request.SetRequestHeader("unity-player-id", _remoteConfigRequest.playerId);

                if (string.IsNullOrEmpty(_remoteConfigRequest.userId))
                {
                    Debug.LogWarning(coreInitError);
                }

                if (string.IsNullOrEmpty(_remoteConfigRequest.playerId))
                {
                    Debug.LogWarning(authInitError);
                }

                foreach (var headerProvider in requestHeaderProviders)
                {
                    var header = headerProvider.Invoke();
                    request.SetRequestHeader(header.key, header.value);
                }

                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonText));
                request.downloadHandler = new DownloadHandlerBuffer();

                var response = await request.SendWebRequest();

                if (response.IsHttpError || response.IsNetworkError)
                {
                    var configTypeHasKeys = configs[configType].GetKeys().Length > 0;
                    var origin =
                        File.Exists(Path.Combine(Application.persistentDataPath, cacheFile)) && configTypeHasKeys
                        ? ConfigOrigin.Cached
                        : ConfigOrigin.Default;
                    configResponse = ParseResponse(origin, null, null);
                }
                else
                {
                    configResponse = ParseResponse(ConfigOrigin.Remote, request.GetResponseHeaders(),
                        request.downloadHandler.text);
                }
            }

            appConfig.HandleConfigResponse(configResponse);
            FetchCompleted?.Invoke(configResponse);
            return appConfig;
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        public async Task<RuntimeConfig> FetchConfigsAsync(string configType, object userAttributes, object appAttributes)
        {
            return await FetchConfigsAsync(configType, userAttributes, appAttributes, null);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing filterAttributes.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        public async Task<RuntimeConfig> FetchConfigsAsync(object userAttributes, object appAttributes, object filterAttributes)
        {
            return await FetchConfigsAsync("settings", userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        public async Task<RuntimeConfig> FetchConfigsAsync(object userAttributes, object appAttributes)
        {
            return await FetchConfigsAsync("settings", userAttributes, appAttributes, null);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public void FetchConfigs<T, T2>(T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            PostConfigWithConfigType("settings", userAttributes, appAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing filterAttributes.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        /// <typeparam name="T3">The type of the <c>filterAttributes</c> struct.</typeparam>
        public void FetchConfigs<T, T2, T3>(T userAttributes, T2 appAttributes, T3 filterAttributes) where T : struct where T2 : struct where T3 : struct
        {
            PostConfigWithConfigType("settings", userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public void FetchConfigs<T, T2>(string configType, T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            PostConfigWithConfigType(configType, userAttributes, appAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType and filterAttributes.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use empty string.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        /// <typeparam name="T3">The type of the <c>filterAttributes</c> struct.</typeparam>
        public void FetchConfigs<T, T2, T3>(string configType, T userAttributes, T2 appAttributes, T3 filterAttributes) where T : struct where T2 : struct where T3 : struct
        {
            PostConfigWithConfigType(configType, userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        public void FetchConfigs(object userAttributes, object appAttributes)
        {
            PostConfigWithConfigType("settings", userAttributes, appAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing filterAttributes.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        public void FetchConfigs(object userAttributes, object appAttributes, object filterAttributes)
        {
            PostConfigWithConfigType("settings", userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        public void FetchConfigs(string configType, object userAttributes, object appAttributes)
        {
            if (string.IsNullOrEmpty(configType))
            {
                configType = "settings";
            }
            PostConfigWithConfigType(configType, userAttributes, appAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        /// <param name="filterAttributes">A struct containing filter attributes. If none apply, use an empty struct.</param>
        public void FetchConfigs(string configType, object userAttributes, object appAttributes, object filterAttributes)
        {
            PostConfigWithConfigType(configType, userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Retrieves the particular config from multiple config object by passing config type.
        /// </summary>
        /// <param name="configType">Config type identifier.</param>
        /// <returns>
        /// Corresponding config as a RuntimeConfig.
        /// </returns>
        public RuntimeConfig GetConfig(string configType)
        {
            if (configs.ContainsKey(configType))
            {
                return configs[configType];
            }
            configs[configType] = new RuntimeConfig(configType);
            return configs[configType];
        }

        internal void PostConfigWithConfigType(string configType, object userAttributes, object appAttributes, object filterAttributes = null)
        {
            if (string.IsNullOrEmpty(configType))
            {
                configType = "settings";
            }

            appConfig = GetConfig(configType);
            appConfig.RequestStatus = ConfigRequestStatus.Pending;
            var jsonText = PreparePayloadWithConfigType(configType, userAttributes, appAttributes, filterAttributes);
            DoRequest(configType, jsonText);
        }

        internal string PreparePayloadWithConfigType(string configType, object userAttributes, object appAttributes, object filterAttributes)
        {
            var commonJobj = JObject.FromObject(_remoteConfigRequest);
            commonJobj["configType"] = configType;
            commonJobj["attributes"] = new JObject();
            commonJobj["attributes"]["unity"] = JObject.FromObject(_unityAttributes);
            commonJobj["attributes"]["unity"]["platform"] = Application.platform.ToString();
            commonJobj["attributes"]["app"] = (appAttributes != null) ? JObject.FromObject(appAttributes) : new JObject();
            commonJobj["attributes"]["user"] = (userAttributes != null) ? JObject.FromObject(userAttributes) : new JObject();

            var filterAttributesObj = (filterAttributes != null) ? JObject.FromObject(filterAttributes) : new JObject();
            if (filterAttributesObj.ContainsKey("key"))
            {
                commonJobj["key"] = filterAttributesObj["key"];
            }
            if (filterAttributesObj.ContainsKey("type"))
            {
                commonJobj["type"] = filterAttributesObj["type"];
            }
            if (filterAttributesObj.ContainsKey("schemaId"))
            {
                commonJobj["schemaId"] = filterAttributesObj["schemaId"];
            }
            return commonJobj.ToString();
        }

        internal void DoRequest(string configType, string jsonText)
        {
            var request = new RCUnityWebRequest();
            request.unityWebRequest = new UnityWebRequest
            {
                method = UnityWebRequest.kHttpVerbPOST,
                timeout = 10,
                url = remoteConfigUrl
            };

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + _playerIdentityToken);
            request.SetRequestHeader("unity-installation-id", _remoteConfigRequest.userId);
            request.SetRequestHeader("unity-player-id", _remoteConfigRequest.playerId);

            if (string.IsNullOrEmpty(_remoteConfigRequest.userId))
            {
                Debug.LogWarning(coreInitError);
            }
            if (string.IsNullOrEmpty(_remoteConfigRequest.playerId))
            {
                Debug.LogWarning(authInitError);
            }

            foreach (var headerProvider in requestHeaderProviders)
            {
                var header = headerProvider.Invoke();
                request.SetRequestHeader(header.key, header.value);
            }
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonText));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += op => {
                var webRequest = ((UnityWebRequestAsyncOperation)op).webRequest;
                if (webRequest.isHttpError || webRequest.isNetworkError)
                {
                    var configTypeHasKeys = configs[configType].GetKeys().Length > 0;
                    var origin = File.Exists(Path.Combine(Application.persistentDataPath, cacheFile)) && configTypeHasKeys ? ConfigOrigin.Cached : ConfigOrigin.Default;
                    var configResponse = ParseResponse(origin, null, null);
                    HandleConfigResponse(configType, configResponse);
                }
                else
                {
                    var configResponse = ParseResponse(ConfigOrigin.Remote, request.GetResponseHeaders(), request.downloadHandler.text);
                    HandleConfigResponse(configType, configResponse);
                }
                webRequest.Dispose();
            };
        }

        internal void HandleConfigResponse(string configType, ConfigResponse configResponse)
        {
            if (!configs.ContainsKey(configType)) configs[configType] = new RuntimeConfig(configType);
            appConfig = GetConfig(configType);
            appConfig.HandleConfigResponse(configResponse);
            FetchCompleted?.Invoke(configResponse);
        }

        /// <summary>
        /// Caches all configs previously fetched, called whenever FetchConfigs completes.
        /// </summary>
        /// <param name="response">the ConfigResponse resulting from the FetchConfigs call</param>
        public void SaveCache(ConfigResponse response)
        {
            if (response.requestOrigin == ConfigOrigin.Remote)
            {
                var responsesToCache = new Dictionary<string, ConfigResponse>();
                foreach (var configType in configs.Keys)
                {
                    responsesToCache[configType] = configs[configType].ConfigResponse;
                }

                try
                {
                    using (var writer = File.CreateText(Path.Combine(Application.persistentDataPath, cacheFile)))
                    {
                        writer.Write(JsonConvert.SerializeObject(responsesToCache));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>
        /// Tries to read from cache files (config and headers) and invokes RawResponseReturned action with cached parameters
        /// </summary>
        public void LoadFromCache()
        {
            try
            {
                byte[] bodyResult;
                using (var reader = File.Open(Path.Combine(Application.persistentDataPath, cacheFile), FileMode.Open))
                {
                    bodyResult = new byte[reader.Length];
                    reader.Read(bodyResult, 0, (int)reader.Length);
                }
                var bodyString = Encoding.UTF8.GetString(bodyResult);
                var bodyJObject = JObject.Parse(bodyString);
                foreach (var kv in bodyJObject)
                {
                    var configType = kv.Key;
                    if (kv.Value.Type == JTokenType.Object)
                    {
                        var cachedConfigResponse = kv.Value.ToObject<ConfigResponse>();
                        configs[configType] = new RuntimeConfig(configType);
                        configs[configType].HandleConfigResponse(cachedConfigResponse);
                    }
                }
            }
            catch
            {
                Debug.Log("Failed to load config from cache.");
            }
        }
    }

    /// <summary>
    /// An enum describing the origin point of your most recently loaded configuration settings.
    /// </summary>
    public enum ConfigOrigin
    {
        /// <summary>
        /// Indicates that no configuration settings loaded in the current session.
        /// </summary>
        Default,
        /// <summary>
        /// Indicates that the configuration settings loaded in the current session are cached from a previous session (in other words, no new configuration settings loaded).
        /// </summary>
        Cached,
        /// <summary>
        /// Indicates that new configuration settings loaded from the remote server in the current session.
        /// </summary>
        Remote
    }

    /// <summary>
    /// An enum representing the status of the current Remote Config request.
    /// </summary>
    public enum ConfigRequestStatus
    {
        /// <summary>
        /// Indicates that no Remote Config request has been made.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the Remote Config request failed.
        /// </summary>
        Failed,
        /// <summary>
        /// Indicates that the Remote Config request succeeded.
        /// </summary>
        Success,
        /// <summary>
        /// Indicates that the Remote Config request is still processing.
        /// </summary>
        Pending
    }

    /// <summary>
    /// A struct representing the response of a Remote Config fetch.
    /// </summary>
    public struct ConfigResponse
    {
        /// <summary>
        /// The origin point of the last retrieved configuration settings.
        /// </summary>
        /// <returns>
        /// An enum describing the origin point of your most recently loaded configuration settings.
        /// </returns>
        public ConfigOrigin requestOrigin;
        /// <summary>
        /// The status of the current Remote Config request.
        /// </summary>
        /// <returns>
        /// An enum representing the status of the current Remote Config request.
        /// </returns>
        public ConfigRequestStatus status;
        /// <summary>
        /// The body of the Remote Config backend response.
        /// </summary>
        /// <returns>
        /// The full response body as a JObject.
        /// </returns>
        public JObject body;
        /// <summary>
        /// The headers from the Remote Config backend response.
        /// </summary>
        /// <returns>
        /// A Dictionary containing the headers..
        /// </returns>
        public Dictionary<string, string> headers;
    }

    [Serializable]
    internal struct RemoteConfigRequest
    {
        public string projectId;
        public string userId;
        public bool isDebugBuild;
        public string configType;
        public string playerId;
        public string analyticsUserId;
        public string configAssignmentHash;
        public string[] key;
        public string[] type;
        public string[] schemaId;
        public string customUserId;
        public string environmentId;
        public string packageVersion;
        public string originService;
        public JObject attributionMetadata;
    }
 #pragma warning disable CS0649
    [Serializable]
    internal struct RequestHeaderTuple
    {
        public string key;
        public string value;
    }

    internal class UnityAttributes
    {
        public string osVersion;
        public string appVersion;
        public bool rootedJailbroken;
        public string model;
        public string cpu;
        public int cpuCount;
        public int cpuFrequency;
        public int ram;
        public int vram;
        public string screen;
        public int dpi;
        public string language;
        public string appName;
        public string appInstallMode;
        public string appInstallStore;
        public int graphicsDeviceId;
        public int graphicsDeviceVendorId;
        public string graphicsName;
        public string graphicsDeviceVendor;
        public string graphicsVersion;
        public int graphicsShader;
        public int maxTextureSize;

        public UnityAttributes()
        {
            osVersion = SystemInfo.operatingSystem;
            appVersion = Application.version;
            rootedJailbroken = Application.sandboxType == ApplicationSandboxType.SandboxBroken;
            model = GetDeviceModel();
            cpu = SystemInfo.processorType;
            cpuCount = SystemInfo.processorCount;
            cpuFrequency = SystemInfo.processorFrequency;
            ram = SystemInfo.systemMemorySize;
            vram = SystemInfo.graphicsMemorySize;
            screen = Screen.currentResolution.ToString();
            dpi = (int)Screen.dpi;
            language = GetISOCodeFromLangStruct(Application.systemLanguage);
            appName = Application.identifier;
            appInstallMode = Application.installMode.ToString();
            appInstallStore = Application.installerName;
            graphicsDeviceId = SystemInfo.graphicsDeviceID;
            graphicsDeviceVendorId = SystemInfo.graphicsDeviceVendorID;
            graphicsName = SystemInfo.graphicsDeviceName;
            graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
            graphicsVersion = SystemInfo.graphicsDeviceVersion;
            graphicsShader = SystemInfo.graphicsShaderLevel;
            maxTextureSize = SystemInfo.maxTextureSize;
        }

        string GetDeviceModel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Get manufacturer, model, and device
            AndroidJavaClass jc = new AndroidJavaClass("android.os.Build");
            string manufacturer = jc.GetStatic<string>("MANUFACTURER");
            string model = jc.GetStatic<string>("MODEL");
            string device = jc.GetStatic<string>("DEVICE");
            return string.Format("{0}/{1}/{2}", manufacturer, model, device);
#else
            return SystemInfo.deviceModel;
#endif
        }

        string GetISOCodeFromLangStruct(SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.Afrikaans:
                    return "af";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.Basque:
                    return "eu";
                case SystemLanguage.Belarusian:
                    return "be";
                case SystemLanguage.Bulgarian:
                    return "bg";
                case SystemLanguage.Catalan:
                    return "ca";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.Czech:
                    return "cs";
                case SystemLanguage.Danish:
                    return "da";
                case SystemLanguage.Dutch:
                    return "nl";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Estonian:
                    return "et";
                case SystemLanguage.Faroese:
                    return "fo";
                case SystemLanguage.Finnish:
                    return "fi";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Greek:
                    return "el";
                case SystemLanguage.Hebrew:
                    return "he";
                case SystemLanguage.Hungarian:
                    return "hu";
                case SystemLanguage.Icelandic:
                    return "is";
                case SystemLanguage.Indonesian:
                    return "id";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Latvian:
                    return "lv";
                case SystemLanguage.Lithuanian:
                    return "lt";
                case SystemLanguage.Norwegian:
                    return "no";
                case SystemLanguage.Polish:
                    return "pl";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Romanian:
                    return "ro";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.SerboCroatian:
                    return "sr";
                case SystemLanguage.Slovak:
                    return "sk";
                case SystemLanguage.Slovenian:
                    return "sl";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Swedish:
                    return "sv";
                case SystemLanguage.Thai:
                    return "th";
                case SystemLanguage.Turkish:
                    return "tk";
                case SystemLanguage.Ukrainian:
                    return "uk";
                case SystemLanguage.Unknown:
                    return "en";
                case SystemLanguage.Vietnamese:
                    return "vi";
                default:
                    return "en";
            }
        }
    }
}
