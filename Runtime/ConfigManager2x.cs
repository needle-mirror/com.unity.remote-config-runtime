using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Services.RemoteConfig.Tests")]

namespace Unity.RemoteConfig
{
    /// <summary>
    /// Use this class to fetch and apply your configuration settings at runtime.
    /// ConfigManager is wrapper class to mimic the functionality of underlying ConfigManagerImpl class.
    /// It uses an instance of ConfigManagerImpl class, making it a primitive class of ConfigManagerImpl.
    /// </summary>
    [Obsolete("The Remote Config implementation you are using is now deprecated as of version 3.x. It will continue to function, but in order to use the newly supported implementation, please refer to the following docs (https://docs.unity3d.com/Packages/com.unity.remote-config-runtime@3.0/manual/CodeIntegration.html) or ExampleSample.cs", false)]
    public static class ConfigManager
    {
        internal static ConfigManagerImpl ConfigManagerImpl = new ConfigManagerImpl("remote-config");

        /// <summary>
        /// Returns the status of the current configuration request from the service.
        /// </summary>
        /// <returns>
        /// An enum representing the status of the current Remote Config request.
        /// </returns>
        public static ConfigRequestStatus requestStatus
        {
            get { return ConfigManagerImpl.appConfig.RequestStatus; }
            set { ConfigManagerImpl.appConfig.RequestStatus = value; }
        }

        /// <summary>
        /// This event fires when the configuration manager successfully fetches settings from the service.
        /// </summary>
        /// <returns>
        /// A struct representing the response of a Remote Config fetch.
        /// </returns>
        public static event Action<ConfigResponse> FetchCompleted
        {
            add { ConfigManagerImpl.FetchCompleted += value; }
            remove { ConfigManagerImpl.FetchCompleted -= value; }
        }

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
        public static RuntimeConfig appConfig
        {
            get { return ConfigManagerImpl.appConfig; }
            set { ConfigManagerImpl.appConfig = value; }
        }

        /// <summary>
        /// Retrieves the particular config from multiple config object by passing config type.
        /// </summary>
        /// <param name="configType">Config type identifier.</param>
        /// <returns>
        /// Corresponding config as a RuntimeConfig.
        /// </returns>
        public static RuntimeConfig GetConfig(string configType)
        {
            return ConfigManagerImpl.GetConfig(configType);
        }

        /// <summary>
        /// Sets a custom user identifier for the Remote Config delivery request payload.
        /// </summary>
        /// <param name="customUserID">Custom user identifier.</param>
        public static void SetCustomUserID(string customUserID)
        {
            ConfigManagerImpl.SetCustomUserID(customUserID);
        }

        /// <summary>
        /// Sets an environment identifier in the Remote Config delivery request payload.
        /// </summary>
        /// <param name="environmentID">Environment unique identifier.</param>
        public static void SetEnvironmentID(string environmentID)
        {
            ConfigManagerImpl.SetEnvironmentID(environmentID);
        }

        /// <summary>
        /// Sets userId identifier in the Remote Config request payload.
        /// </summary>
        /// <param name="userID">User identifier.</param>
        public static void SetUserID(string userID)
        {
            ConfigManagerImpl.SetUserID(userID);
        }

        /// <summary>
        /// Sets player Identity Token.
        /// </summary>
        /// <param name="playerIdentityToken">Player Identity identifier.</param>
        public static void SetPlayerIdentityToken(string playerIdentityToken)
        {
            ConfigManagerImpl.SetPlayerIdentityToken(playerIdentityToken);
        }

        /// <summary>
        /// Sets configAssignmentHash identifier coming from core services.
        /// </summary>
        /// <param name="configAssignmentHashID">configAssignmentHash unique identifier.</param>
        public static void SetConfigAssignmentHash(string configAssignmentHashID)
        {
            ConfigManagerImpl.SetConfigAssignmentHash(configAssignmentHashID);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public static void FetchConfigs<T, T2>(T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            ConfigManagerImpl.FetchConfigs(userAttributes, appAttributes);
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
        public static void FetchConfigs<T, T2, T3>(T userAttributes, T2 appAttributes, T3 filterAttributes) where T : struct where T2 : struct where T3 : struct
        {
            ConfigManagerImpl.FetchConfigs(userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server passing a configType.
        /// </summary>
        /// <param name="configType">A string containing configType. If none apply, use null.</param>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public static void FetchConfigs<T, T2>(string configType, T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            ConfigManagerImpl.FetchConfigs(configType, userAttributes, appAttributes);
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
        public static void FetchConfigs<T, T2, T3>(string configType, T userAttributes, T2 appAttributes, T3 filterAttributes) where T : struct where T2 : struct where T3 : struct
        {
            ConfigManagerImpl.FetchConfigs(configType, userAttributes, appAttributes, filterAttributes);
        }

        /// <summary>
        /// Saves origin, headers and result from last fetch in a file.
        /// </summary>
        /// <param name="origin">Request origin.</param>
        /// <param name="headers">Request headers.</param>
        /// <param name="result">Config from last fetch.</param>
        internal static void SaveCache(ConfigOrigin origin, Dictionary<string, string> headers, string result)
        {
            var configResponse = ConfigManagerImpl.ParseResponse(origin, headers, result);
            ConfigManagerImpl.SaveCache(configResponse);
        }

        /// <summary>
        /// Loads last config from a cache file.
        /// </summary>
        internal static void LoadFromCache()
        {
            ConfigManagerImpl.LoadFromCache();
        }

    }
}
