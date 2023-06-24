using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Reflection;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Unity.Services.RemoteConfig.Tests
{
#if UNITY_EDITOR
    internal class RemoteConfigServiceTests
    {
        private SerializedObject _projectSettingsObject;
        private SerializedProperty _cloudProjectIdProperty;
        private SerializedProperty _cloudProjectNameProperty;
        private SerializedProperty _versionProperty;
        private string _previousProjectId;
        private string _previousProjectName;
        private string _previousVersion;

        private void FixYamatoProjectSettings()
        {
            // Cloud Project ID needs to be linked or the SDK will fail to start.
            // Since this cannot be set in Yamato's transient test projects, we need to do a little hackery...
            const string ProjectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
            _projectSettingsObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(ProjectSettingsAssetPath)[0]);
            _cloudProjectIdProperty = _projectSettingsObject.FindProperty("cloudProjectId");
            _cloudProjectNameProperty = _projectSettingsObject.FindProperty("projectName");
            _versionProperty = _projectSettingsObject.FindProperty("bundleVersion"); // NOTE: this is Project Settings -> Player -> Version

            _previousProjectId = _cloudProjectIdProperty.stringValue;
            _previousProjectName = _cloudProjectNameProperty.stringValue;
            _previousVersion = _versionProperty.stringValue;
            _cloudProjectIdProperty.stringValue = "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4";
            _cloudProjectNameProperty.stringValue = "RS Package Dev Project";
            _versionProperty.stringValue = "1.3.3.7";
            _projectSettingsObject.ApplyModifiedProperties();
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            FixYamatoProjectSettings();

            // var init = UnityServices.InitializeAsync();
            // while (!init.IsCompleted)
            // {
            //     yield return null;
            // }

            // Task signin = Task.CompletedTask;
            // if (!AuthenticationService.Instance.IsSignedIn)
            // {
            //     signin = AuthenticationService.Instance.SignInAnonymouslyAsync();
            // }

            // while (!AuthenticationService.Instance.IsSignedIn)
            // {
            //     yield return null;
            // }
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _cloudProjectIdProperty.stringValue = _previousProjectId;
            _cloudProjectNameProperty.stringValue = _previousProjectName;
            _versionProperty.stringValue = _previousVersion;
            _projectSettingsObject.ApplyModifiedProperties();

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetCustomUserID_SetsCustomUserID()
        {
            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<SetCustomUserID_MonobehaviorTest>(false);

            FieldInfo configmanagerImplInfo = typeof(RemoteConfigService).GetField("_configManagerImpl", BindingFlags.Instance | BindingFlags.NonPublic);
            var configmanagerImpl = configmanagerImplInfo.GetValue(RemoteConfigService.Instance);

            monoTest.component.StartTest();
            yield return monoTest;

            FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo customUserIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("customUserId");
            var customUserId = customUserIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(configmanagerImpl));

            Assert.That(Equals(customUserId, RemoteConfigServiceTestUtils.userId));
        }

        [UnityTest]
        public IEnumerator SetEnvironmentID_SetsEnvironmentID()
        {
            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<SetEnvironmentID_MonobehaviorTest>(false);

            monoTest.component.StartTest();
            yield return monoTest;

            FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo environmentIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("environmentId");
            var environmentId = environmentIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(RemoteConfigService.Instance.ConfigManagerImpl));

            Assert.That(Equals(environmentId, RemoteConfigServiceTestUtils.environmentId));
        }

        [UnityTest]
        public IEnumerator FetchConfigs_OnCompleteGetsFiredWithCorrectInfo()
        {
            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;
            Assert.That(RemoteConfigService.Instance.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", RemoteConfigService.Instance.requestStatus, ConfigRequestStatus.Success);
            Assert.That(RemoteConfigService.Instance.appConfig.origin == ConfigOrigin.Remote, "ConfigManager.appConfig.origin was {0}, should have been {1}", RemoteConfigService.Instance.appConfig.origin, ConfigOrigin.Remote);
        }

        [UnityTest]
        public IEnumerator FetchConfigs_NullWorks()
        {
            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigs_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;
            Assert.That(RemoteConfigService.Instance.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", RemoteConfigService.Instance.requestStatus, ConfigRequestStatus.Pending);
        }

        [UnityTest]
        public IEnumerator ConfigFieldInitializedAsEmpty()
        {
            yield return null;
            var emptyJObject = new JObject();
            Assert.That(RemoteConfigService.Instance.appConfig != null);
            Assert.That(RemoteConfigService.Instance.appConfig.config != null);
            Assert.AreEqual(RemoteConfigService.Instance.appConfig.config.GetType(), emptyJObject.GetType());
        }

        [UnityTest]
        public IEnumerator SaveCache_CreatesFileIfNotThereAndCachesRightContent()
        {
            var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            while(!File.Exists(fileName)) yield return null;
            Assert.That(File.Exists(fileName));
            string text = File.ReadAllText(fileName);
            Assert.That(text.Contains("testInt"));
            RemoteConfigService.Instance.ConfigManagerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
            RemoteConfigService.Instance.ConfigManagerImpl.LoadFromCache();
            Assert.AreEqual(232, RemoteConfigService.Instance.appConfig.GetInt("testInt"));
        }

        [UnityTest]
        public IEnumerator SaveCache_CachesMultipleRuntimeConfigs()
        {
            var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            RemoteConfigServiceTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            while(!File.Exists(fileName)) yield return null;
            var managerImpl = RemoteConfigService.Instance.ConfigManagerImpl;

            var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),RemoteConfigServiceTestUtils.jsonPayloadEconomyConfig);
            managerImpl.HandleConfigResponse("economy", configResponse);

            var configResponseSettings = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),RemoteConfigServiceTestUtils.jsonPayloadString);
            managerImpl.HandleConfigResponse(ConfigManagerImpl.DefaultConfigKey, configResponseSettings);

            Assert.That(File.Exists(fileName));
            string text = File.ReadAllText(fileName);
            Assert.That(text.Contains(ConfigManagerImpl.DefaultConfigKey));
            Assert.That(text.Contains("economy"));
            Assert.That(managerImpl.configs.Count == 2);
            Assert.That(RemoteConfigService.Instance.GetConfig("economy").GetString("item") == "sword");
            managerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
            managerImpl.configs.Remove("economy");
            managerImpl.LoadFromCache();
            Assert.AreEqual(12, RemoteConfigService.Instance.appConfig.GetInt("someInt"));
            Assert.That(RemoteConfigService.Instance.GetConfig("economy").GetString("item") == "sword");
            Assert.That(RemoteConfigService.Instance.GetConfig("economy").assignmentId == JObject.Parse(RemoteConfigServiceTestUtils.jsonPayloadEconomyConfig)["metadata"]["assignmentId"].ToString());
        }
    }
#endif

    internal class RuntimeRemoteConfigServiceTests
    {
        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsAssignmentId()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("3049bfea-05fa-4ddf-acc6-ce43c888fe92", RemoteConfigService.Instance.appConfig.assignmentId);
        }
        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ReturnsNullAssignmentIdWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("3049bfea-05fa-4ddf-acc6-ce43c888fe92", RemoteConfigService.Instance.appConfig.assignmentId);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsEnvironmentId()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("83fff3e2-a945-4601-9ccc-5e9d16d12ea8", RemoteConfigService.Instance.appConfig.environmentId);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ReturnsNullEnvironmentIdWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("83fff3e2-a945-4601-9ccc-5e9d16d12ea8", RemoteConfigService.Instance.appConfig.environmentId);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfig()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(9, RemoteConfigService.Instance.appConfig.GetKeys().Length);
            Assert.AreEqual(true, RemoteConfigService.Instance.appConfig.HasKey("testInt"));
            Assert.AreEqual(true, RemoteConfigService.Instance.appConfig.HasKey("longSomething"));
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfigWhenBadResponse()
        {
            var configBeforeRequest = RemoteConfigService.Instance.appConfig.config;

            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(configBeforeRequest.ToString(), RemoteConfigService.Instance.appConfig.config.ToString());
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.That(RemoteConfigService.Instance.appConfig.GetBool("bool") == true);
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValueWhenBadResponse()
        {
            var managerImpl = RemoteConfigService.Instance.ConfigManagerImpl;
            var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),RemoteConfigServiceTestUtils.jsonPayloadString);
            managerImpl.HandleConfigResponse(ConfigManagerImpl.DefaultConfigKey, configResponse);

            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            // at this point we have multiple configs in cache, but we are still able to get "bool"
            // from "settings" configType, eventhough we loaded "someOtherConfig" configType
            Assert.AreEqual(true, RemoteConfigService.Instance.appConfig.GetBool("bool"));
        }

        [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(0.12999999523162842, RemoteConfigService.Instance.appConfig.GetFloat("heloe"));
        }

       [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            // at this point we have multiple configs in cache, but we are still able to get "heloe"
            // from "settings" configType, eventhough we loaded "someOtherConfig" configType
            Assert.AreEqual(0.12999999523162842, RemoteConfigService.Instance.appConfig.GetFloat("heloe"));
        }

        [UnityTest]
        public IEnumerator GetInt_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(12, RemoteConfigService.Instance.appConfig.GetInt("someInt"));
        }
       [UnityTest]
        public IEnumerator GetInt_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(12, RemoteConfigService.Instance.appConfig.GetInt("someInt"));
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("madAF", RemoteConfigService.Instance.appConfig.GetString("madBro"));
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsDate()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("2020-04-03T10:01:00Z", RemoteConfigService.Instance.appConfig.GetString("stringFormattedAsDate"));
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsJson()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("{\"a\":2.0,\"b\":4,\"c\":\"someString\"}", RemoteConfigService.Instance.appConfig.GetString("stringFormattedAsJson"));
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("madAF", RemoteConfigService.Instance.appConfig.GetString("madBro"));
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(9223372036854775806, RemoteConfigService.Instance.appConfig.GetLong("longSomething"));
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(9223372036854775806, RemoteConfigService.Instance.appConfig.GetLong("longSomething"));
        }

        [UnityTest]
         public IEnumerator GetJson_ReturnsRightValue()
         {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("{\"a\":1.0,\"b\":2,\"c\":\"someString\"}", RemoteConfigService.Instance.appConfig.GetJson("jsonSetting"));
         }

         [UnityTest]
         public IEnumerator GetJson_ReturnsRightValueWhenBadResponse()
         {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual("{\"a\":1.0,\"b\":2,\"c\":\"someString\"}", RemoteConfigService.Instance.appConfig.GetJson("jsonSetting"));
         }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.That(RemoteConfigService.Instance.appConfig.HasKey("longSomething"));
        }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(true, RemoteConfigService.Instance.appConfig.HasKey("longSomething"));
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValue()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(9, RemoteConfigService.Instance.appConfig.GetKeys().Length );
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValueWhenBadResponse()
        {
            var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                while(!File.Exists(fileName)) yield return null;
                Assert.That(File.Exists(fileName));
            #endif

            Assert.AreEqual(9, RemoteConfigService.Instance.appConfig.GetKeys().Length);
        }
    }

    internal static class RemoteConfigServiceTestUtils
    {
        public const string userId = "some-user-id";
        public const string environmentId = "7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2";

        public struct UserAttributes
        {

        }

        public struct AppAttributes
        {

        }

        public struct FilterAttributes
        {

        }

        public static string jsonPayloadString =
            @"{
                ""configs"": {
                    ""settings"": {
                        ""raywagagwawd"": """",
                        ""settingsConfig"": ""{\""hello\"":2.0,\""someInt\"":32,\""madBro\"":\""fdsfadsf\""}"",
                        ""fghfg"": ""ffgf"",
                        ""longSomething"": 9223372036854775806,
                        ""someInt"": 12,
                        ""helloe"": 0.12999999523162842,
                        ""blah"": ""blahd"",
                        ""stringFormattedAsDate"": ""2020-04-03T10:01:00Z"",
                        ""stringFormattedAsJson"": ""{\""a\"":2.0,\""b\"":4,\""c\"":\""someString\""}"",
                        ""bool"": true,
                        ""madBro"": ""madAF"",
                        ""jsonKeys"": ""settingsConfig,gameConfig"",
                        ""skiwdafsdwas"": ""hello"",
                        ""jsonSetting"": ""{\""a\"":1.0,\""b\"":2,\""c\"":\""someString\""}"",
                    }
                },
                ""metadata"": {
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a858"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2"",
                }
            }";

        public static string jsonPayloadStringNoRCSection =
            @"{
                ""configs"": {
                    ""someOtherKey"": {
                    }
                },
                ""metadata"": {
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a811"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708d11"",
                }
            }";

        public static string jsonPayloadEconomyConfig =
            @"{
                ""configs"": {
                    ""economy"": {
                        ""item"": ""sword""
                    }
                },
                ""metadata"": {
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a822"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708d22"",
                }
            }";

        public static void SetProjectIdOnRequestPayload()
        {
            const string projectId = "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4";
            RemoteConfigService.Instance.ConfigManagerImpl._remoteConfigRequest.projectId = projectId;
        }

        public static void FieldInvestigation(Type t)
        {
            Debug.Log("--------------FieldInvestigation--------------");
            Debug.Log("Type: "+t);
            FieldInfo[] fields = t.GetFields();
            Debug.Log("field count: "+fields.Length);
            foreach (var field in fields)
            {
                Debug.Log("field: "+field.Name);
            }
        }

        public static void EventInvestigation(Type t)
        {
            Debug.Log("--------------EventInvestigation--------------");
            Debug.Log("Type: "+t);
            EventInfo[] events = t.GetEvents();
            Debug.Log("event count: "+events.Length);
            foreach (var e in events)
            {
                Debug.Log("event: "+e.Name);
            }
        }

        public static void PropertyInvestigation(Type t)
        {
            Debug.Log("--------------PropertyInvestigation--------------");
            Debug.Log("Type: "+t);
            PropertyInfo[] properties = t.GetProperties();
            Debug.Log("property count: "+properties.Length);
            foreach (var property in properties)
            {
                Debug.Log("property: "+property.Name);
            }
        }

        public static void MethodInvestigation(Type t)
        {
            Debug.Log("--------------MethodInvestigation--------------");
            Debug.Log("Type: "+t);
            MethodInfo[] methods = t.GetMethods();
            Debug.Log("methods count: "+methods.Length);
            foreach (var method in methods)
            {
                Debug.Log("method: "+method.Name);
            }
        }

        public static void FullInvestigation(Type t)
        {
            FieldInvestigation(t);
            EventInvestigation(t);
            PropertyInvestigation(t);
            MethodInvestigation(t);
        }
    }

    internal interface IRCSTest
    {
        void StartTest();
    }
}
