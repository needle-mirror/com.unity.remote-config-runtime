using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Reflection;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;

namespace Unity.RemoteConfig.Tests
{
#if UNITY_EDITOR
    internal class ConfigManagerTests
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
            _projectSettingsObject = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(ProjectSettingsAssetPath)[0]);  
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
            
            var init = UnityServices.InitializeAsync();
            while (!init.IsCompleted)
            {
                yield return null;
            }

            Task signin = Task.CompletedTask;
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                signin = AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            while (!AuthenticationService.Instance.IsSignedIn)
            {
                yield return null;
            }

            ConfigManager.ConfigManagerImpl.FetchConfigs(null, null);
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
            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<SetCustomUserID_MonobehaviorTest>(false);

            FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("_configManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
            var configmanagerImpl = configmanagerImplInfo.GetValue(null);

            monoTest.component.StartTest();
            yield return monoTest;

            FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo customUserIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("customUserId");
            var customUserId = customUserIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(configmanagerImpl));

            Assert.That(Equals(customUserId, ConfigManagerTestUtils.userId));
        }

        [UnityTest]
        public IEnumerator SetEnvironmentID_SetsEnvironmentID()
        {
            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<SetEnvironmentID_MonobehaviorTest>(false);

            monoTest.component.StartTest();
            yield return monoTest;

            FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo environmentIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("environmentId");
            var environmentId = environmentIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(ConfigManager.ConfigManagerImpl));

            Assert.That(Equals(environmentId, ConfigManagerTestUtils.environmentId));
        }

        [UnityTest]
        public IEnumerator FetchConfigs_OnCompleteGetsFiredWithCorrectInfo()
        {
            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;
            Assert.That(ConfigManager.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", ConfigManager.requestStatus, ConfigRequestStatus.Success);
            Assert.That(ConfigManager.appConfig.origin == ConfigOrigin.Remote, "ConfigManager.appConfig.origin was {0}, should have been {1}", ConfigManager.appConfig.origin, ConfigOrigin.Remote);
        }

        [UnityTest]
        public IEnumerator FetchConfigs_NullWorks()
        {
            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigs_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;
            Assert.That(ConfigManager.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", ConfigManager.requestStatus, ConfigRequestStatus.Pending);
        }

        [UnityTest]
        public IEnumerator ConfigFieldInitializedAsEmpty()
        {
            yield return null;
            var emptyJObject = new JObject();
            Assert.That(ConfigManager.appConfig != null);
            Assert.That(ConfigManager.appConfig.config != null);
            Assert.AreEqual(ConfigManager.appConfig.config.GetType(), emptyJObject.GetType());
        }

        [UnityTest]
        public IEnumerator SaveCache_CreatesFileIfNotThereAndCachesRightContent()
        {
            var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            yield return waitForFileToBeCreated();

            async Task waitForFileToBeCreated()
            {
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                string text = File.ReadAllText(fileName);
                Assert.That(text.Contains("testInt"));
                ConfigManager.ConfigManagerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
                ConfigManager.ConfigManagerImpl.LoadFromCache();
                Assert.AreEqual(66, ConfigManager.appConfig.GetInt("testInt"));
            }

        }

        [UnityTest]
        public IEnumerator SaveCache_CachesMultipleRuntimeConfigs()
        {
            var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            yield return waitForFileWithMultipleConfigsToBeCreated();

            async Task waitForFileWithMultipleConfigsToBeCreated()
            {
                await Task.Run(() => File.Exists(fileName));
                var managerImpl = ConfigManager.ConfigManagerImpl;
                var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),ConfigManagerTestUtils.jsonPayloadEconomyConfig);
                managerImpl.HandleConfigResponse("economy", configResponse);

                Assert.That(File.Exists(fileName));
                string text = File.ReadAllText(fileName);
                Assert.That(text.Contains("testInt"));
                Assert.That(text.Contains("economy"));
                Assert.That(managerImpl.configs.Count == 2);
                Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
                managerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
                managerImpl.configs.Remove("economy");
                managerImpl.LoadFromCache();
                Assert.AreEqual(66, ConfigManager.appConfig.GetInt("testInt"));
                Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
                Assert.That(ConfigManager.GetConfig("economy").assignmentId == JObject.Parse(ConfigManagerTestUtils.jsonPayloadEconomyConfig)["metadata"]["assignmentId"].ToString()); 
            }
        }
    }
#endif
    
    internal class RuntimeConfigTests
    {
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ProperlySetsAssignmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("a04fb7ec-26e4-4247-b8b4-70dd6967a858", ConfigManager.appConfig.assignmentId);
            }
        }
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ReturnsNullAssignmentIdWhenBadResponse()
        {
            var assignmentIdBeforeRequest = ConfigManager.appConfig.assignmentId;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(assignmentIdBeforeRequest, ConfigManager.appConfig.assignmentId);

            }
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsEnvironmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2", ConfigManager.appConfig.environmentId);
            }
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ReturnsNullEnvironmentIdWhenBadResponse()
        {
            var environmentIdBeforeRequest = ConfigManager.appConfig.environmentId;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(environmentIdBeforeRequest, ConfigManager.appConfig.environmentId);
            }
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfig()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["configs"]?["settings"])?.ToString(), ConfigManager.appConfig.config.ToString());
            }
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfigWhenBadResponse()
        {
            var configBeforeRequest = ConfigManager.appConfig.config;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(configBeforeRequest.ToString(), ConfigManager.appConfig.config.ToString());
            }
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();

            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.That(ConfigManager.appConfig.GetBool("bool") == true);
            }        
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValueWhenBadResponse()
        {
            var boolBeforeRequest = ConfigManager.appConfig.GetBool("bool");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(boolBeforeRequest, ConfigManager.appConfig.GetBool("bool"));
            }
        }

        [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(0.12999999523162842, ConfigManager.appConfig.GetFloat("helloe"));
            }
        }
       [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValueWhenBadResponse()
        {
            var floatBeforeRequest = ConfigManager.appConfig.GetFloat("helloe");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(floatBeforeRequest, ConfigManager.appConfig.GetFloat("helloe"));
            }
        }

        [UnityTest]
        public IEnumerator GetInt_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(12, ConfigManager.appConfig.GetInt("someInt"));
            }
        }
       [UnityTest]
        public IEnumerator GetInt_ReturnsRightValueWhenBadResponse()
        {
            var intBeforeRequest = ConfigManager.appConfig.GetFloat("someInt");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(intBeforeRequest, ConfigManager.appConfig.GetInt("someInt"));
            }
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("madAF", ConfigManager.appConfig.GetString("madBro"));
            }
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsDate()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("2020-04-03T10:01:00Z", ConfigManager.appConfig.GetString("stringFormattedAsDate"));
            }
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsJson()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("{\"a\":2.0,\"b\":4,\"c\":\"someString\"}", ConfigManager.appConfig.GetString("stringFormattedAsJson"));
            }
        }
        
        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueWhenBadResponse()
        {
            var stringBeforeRequest = ConfigManager.appConfig.GetString("madBro");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual("madAF", ConfigManager.appConfig.GetString("madBro"));
            }
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(9223372036854775806, ConfigManager.appConfig.GetLong("longSomething"));
            }
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValueWhenBadResponse()
        {
            var longBeforeRequest = ConfigManager.appConfig.GetLong("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(longBeforeRequest, ConfigManager.appConfig.GetLong("longSomething"));
            }
        }

        [UnityTest]
         public IEnumerator GetJson_ReturnsRightValue()
         {
              ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
              yield return waitForFileToBeCreated();
              
              async Task waitForFileToBeCreated()
              {
                  var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                  await Task.Run(() => File.Exists(fileName));
                  Assert.That(File.Exists(fileName));
                  Assert.AreEqual("{\"a\":1.0,\"b\":2,\"c\":\"someString\"}", ConfigManager.appConfig.GetJson("jsonSetting"));
              }              
         }

         [UnityTest]
         public IEnumerator GetJson_ReturnsRightValueWhenBadResponse()
         {
             var jsonBeforeRequest = ConfigManager.appConfig.GetJson("jsonSetting");
             ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

             yield return waitForFileToBeCreated();
             
             async Task waitForFileToBeCreated()
             {
                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                 await Task.Run(() => File.Exists(fileName));
                 Assert.That(File.Exists(fileName));
                 Assert.AreEqual(jsonBeforeRequest, ConfigManager.appConfig.GetJson("jsonSetting") );
             }
         }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.That(ConfigManager.appConfig.HasKey("longSomething"));
            }
        }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValueWhenBadResponse()
        {
            var hasKeylongBeforeRequest = ConfigManager.appConfig.HasKey("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(hasKeylongBeforeRequest, ConfigManager.appConfig.HasKey("longSomething"));
            }
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["configs"]["settings"]).Properties().Select(prop => prop.Name).ToArray<string>().Length, ConfigManager.appConfig.GetKeys().Length);
            }
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValueWhenBadResponse()
        {
            var keyLengthBeforeRequest = ConfigManager.appConfig.GetKeys().Length;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return waitForFileToBeCreated();
            
            async Task waitForFileToBeCreated()
            {
                var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
                await Task.Run(() => File.Exists(fileName));
                Assert.That(File.Exists(fileName));
                Assert.AreEqual(keyLengthBeforeRequest, ConfigManager.appConfig.GetKeys().Length );
            }
        }
    }

    internal static class ConfigManagerTestUtils
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
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a858"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2"",
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
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a859"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db3"",
                }
            }";

        public async static void SendPayloadToConfigManager(string payload)
        {
            var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);

            await Task.Run(() => File.Exists(fileName));
            Assert.That(File.Exists(fileName));

            ConfigManager.ConfigManagerImpl.HandleConfigResponse(
                "settings", 
                new ConfigResponse
                {
                    requestOrigin = ConfigOrigin.Remote,
                    status = ConfigRequestStatus.Success,
                    body = JObject.Parse(payload),
                    headers = new Dictionary<string, string>()
                }
            );
        }

        public static void SetProjectIdOnRequestPayload()
        {
            FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("_configManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
            var configmanagerImpl = configmanagerImplInfo.GetValue(null);

            FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
            var common = rcRequestFieldInfo.GetValue(configmanagerImpl);

            FieldInfo projectIdFieldInfo = common.GetType().GetField("projectId");
            projectIdFieldInfo.SetValue(common, "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4");
            rcRequestFieldInfo.SetValue(configmanagerImpl, common);
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

    internal interface IRCTest
    {
        void StartTest();
    }
}