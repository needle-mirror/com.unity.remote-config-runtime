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

namespace Unity.RemoteConfig.Tests
{
    internal class ConfigManagerTests
    {
        [UnityTest]
        public IEnumerator SetCustomUserID_SetsCustomUserID()
        {
            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<SetCustomUserID_MonobehaviorTest>(false);

            FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("ConfigManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
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
            if (File.Exists(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile)))
            {
                File.Delete(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile));
            }

            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;

            yield return new WaitForSeconds(2f);
            Assert.That(File.Exists(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile)));
            string text = File.ReadAllText(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile));
            Assert.That(text.Contains("testInt"));
            ConfigManager.ConfigManagerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
            ConfigManager.ConfigManagerImpl.LoadFromCache();
            Assert.That(ConfigManager.appConfig.GetInt("testInt") == 232);
        }

        [UnityTest]
        public IEnumerator SaveCache_CachesMultipleRuntimeConfigs()
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile)))
            {
                File.Delete(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile));
            }

            ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
            var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
            monoTest.component.StartTest();
            yield return monoTest;
            var managerImpl = ConfigManager.ConfigManagerImpl;
            var configResponse = managerImpl.ParseResponse(
                ConfigOrigin.Remote,
                new Dictionary<string, string>(),
                ConfigManagerTestUtils.jsonPayloadEconomyConfig);
            managerImpl.HandleConfigResponse("economy", configResponse);
            yield return new WaitForSeconds(2f);
            Assert.That(File.Exists(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile)));
            string text = File.ReadAllText(Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile));
            Assert.That(text.Contains("testInt"));
            Assert.That(text.Contains("economy"));
            Assert.That(managerImpl.configs.Count == 2);
            Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
            managerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
            managerImpl.configs.Remove("economy");
            managerImpl.LoadFromCache();
            Assert.That(ConfigManager.appConfig.GetInt("testInt") == 232);
            Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
            Assert.That(ConfigManager.GetConfig("economy").assignmentId ==
                        JObject.Parse(ConfigManagerTestUtils.jsonPayloadEconomyConfig)["metadata"]["assignmentId"].ToString());
        }
    }

    internal class RuntimeConfigTests
    {
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ProperlySetsAssignmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.assignmentId == "a04fb7ec-26e4-4247-b8b4-70dd6967a858");
        }
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ReturnsNullAssignmentIdWhenBadResponse()
        {
            var assignmentIdBeforeRequest = ConfigManager.appConfig.assignmentId;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.assignmentId == assignmentIdBeforeRequest);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsEnvironmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.environmentId == "7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2");
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ReturnsNullEnvironmentIdWhenBadResponse()
        {
            var environmentIdBeforeRequest = ConfigManager.appConfig.environmentId;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.environmentId == environmentIdBeforeRequest);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfig()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.config.ToString() == ((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["configs"]?["settings"])?.ToString());
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfigWhenBadResponse()
        {
            var configBeforeRequest = ConfigManager.appConfig.config;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.config.ToString() == configBeforeRequest.ToString());
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetBool("bool") == true);
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValueWhenBadResponse()
        {
            var boolBeforeRequest = ConfigManager.appConfig.GetBool("bool");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetBool("bool") == boolBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetFloat("helloe") == 0.12999999523162842);
        }
       [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValueWhenBadResponse()
        {
            var floatBeforeRequest = ConfigManager.appConfig.GetFloat("helloe");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetFloat("helloe") == floatBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetInt_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetInt("someInt") == 12);
        }
       [UnityTest]
        public IEnumerator GetInt_ReturnsRightValueWhenBadResponse()
        {
            var intBeforeRequest = ConfigManager.appConfig.GetFloat("someInt");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetInt("someInt") == intBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("madBro") == "madAF");
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsDate()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("stringFormattedAsDate") == "2020-04-03T10:01:00Z");
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueIfFormattedAsJson()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("stringFormattedAsJson") == "{\"a\":2.0,\"b\":4,\"c\":\"someString\"}");
        }


        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueWhenBadResponse()
        {
            var stringBeforeRequest = ConfigManager.appConfig.GetString("madBro");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("madBro") == stringBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetLong("longSomething") == 9223372036854775806);
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValueWhenBadResponse()
        {
            var longBeforeRequest = ConfigManager.appConfig.GetLong("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetLong("longSomething") == longBeforeRequest);
        }

        [UnityTest]
         public IEnumerator GetJson_ReturnsRightValue()
         {
              ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
              yield return null;
              Assert.That(ConfigManager.appConfig.GetJson("jsonSetting") == "{\"a\":1.0,\"b\":2,\"c\":\"someString\"}");
         }

         [UnityTest]
         public IEnumerator GetJson_ReturnsRightValueWhenBadResponse()
         {
              var jsonBeforeRequest = ConfigManager.appConfig.GetJson("jsonSetting");
              ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
              yield return null;
              Assert.That(ConfigManager.appConfig.GetJson("jsonSetting") == jsonBeforeRequest);
         }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.HasKey("longSomething"));
        }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValueWhenBadResponse()
        {
            var hasKeylongBeforeRequest = ConfigManager.appConfig.HasKey("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.HasKey("longSomething") == hasKeylongBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetKeys().Length == ((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["configs"]["settings"]).Properties().Select(prop => prop.Name).ToArray<string>().Length);
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValueWhenBadResponse()
        {
            var keyLengthBeforeRequest = ConfigManager.appConfig.GetKeys().Length;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetKeys().Length == keyLengthBeforeRequest);
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

        public static void SendPayloadToConfigManager(string payload)
        {

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
            FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("ConfigManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
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