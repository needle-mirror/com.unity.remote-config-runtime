// using System.Collections;
// using UnityEngine.TestTools;
// using NUnit.Framework;
// using System.Reflection;
// using System;
// using System.Collections.Generic;
// using Newtonsoft.Json.Linq;
// using UnityEngine;
// using System.IO;
// using UnityEditor;

// namespace Unity.Services.RemoteConfig.Tests
// {
// #if UNITY_EDITOR
//     internal class ConfigManagerTests
//     {
//         private SerializedObject _projectSettingsObject;
//         private SerializedProperty _cloudProjectIdProperty;
//         private SerializedProperty _cloudProjectNameProperty;
//         private SerializedProperty _versionProperty;
//         private string _previousProjectId;
//         private string _previousProjectName;
//         private string _previousVersion;
        
//         private void FixYamatoProjectSettings()
//         {
//             // Cloud Project ID needs to be linked or the SDK will fail to start.
//             // Since this cannot be set in Yamato's transient test projects, we need to do a little hackery...
//             const string ProjectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
//             _projectSettingsObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(ProjectSettingsAssetPath)[0]);
//             _cloudProjectIdProperty = _projectSettingsObject.FindProperty("cloudProjectId");
//             _cloudProjectNameProperty = _projectSettingsObject.FindProperty("projectName");
//             _versionProperty = _projectSettingsObject.FindProperty("bundleVersion"); // NOTE: this is Project Settings -> Player -> Version
            
//             _previousProjectId = _cloudProjectIdProperty.stringValue;
//             _previousProjectName = _cloudProjectNameProperty.stringValue;
//             _previousVersion = _versionProperty.stringValue;
//             _cloudProjectIdProperty.stringValue = "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4";
//             _cloudProjectNameProperty.stringValue = "RS Package Dev Project";
//             _versionProperty.stringValue = "1.3.3.7";
//             _projectSettingsObject.ApplyModifiedProperties();
//         }
        
//         [UnitySetUp]
//         public IEnumerator Setup()
//         {
//             FixYamatoProjectSettings();
            
//             // var init = UnityServices.InitializeAsync();
//             // while (!init.IsCompleted)
//             // {
//             //     yield return null;
//             // }

//             // Task signin = Task.CompletedTask;
//             // if (!AuthenticationService.Instance.IsSignedIn)
//             // {
//             //     signin = AuthenticationService.Instance.SignInAnonymouslyAsync();
//             // }

//             // while (!AuthenticationService.Instance.IsSignedIn)
//             // {
//             //     yield return null;
//             // }
//             yield break;
//         }

//         [UnityTearDown]
//         public IEnumerator TearDown()
//         {
//             _cloudProjectIdProperty.stringValue = _previousProjectId;
//             _cloudProjectNameProperty.stringValue = _previousProjectName;
//             _versionProperty.stringValue = _previousVersion;
//             _projectSettingsObject.ApplyModifiedProperties();

//             yield return null;
//         }
        
//         [UnityTest]
//         public IEnumerator SetCustomUserID_SetsCustomUserID()
//         {
//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<SetCustomUserID_MonobehaviorTest>(false);

//             FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("_configManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
//             var configmanagerImpl = configmanagerImplInfo.GetValue(null);

//             monoTest.component.StartTest();
//             yield return monoTest;

//             FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
//             FieldInfo customUserIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("customUserId");
//             var customUserId = customUserIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(configmanagerImpl));

//             Assert.That(Equals(customUserId, ConfigManagerTestUtils.userId));
//         }

//         [UnityTest]
//         public IEnumerator SetEnvironmentID_SetsEnvironmentID()
//         {
//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<SetEnvironmentID_MonobehaviorTest>(false);

//             monoTest.component.StartTest();
//             yield return monoTest;

//             FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
//             FieldInfo environmentIdFieldInfo = rcRequestFieldInfo.FieldType.GetField("environmentId");
//             var environmentId = environmentIdFieldInfo.GetValue(rcRequestFieldInfo.GetValue(ConfigManager.ConfigManagerImpl));

//             Assert.That(Equals(environmentId, ConfigManagerTestUtils.environmentId));
//         }

//         [UnityTest]
//         public IEnumerator FetchConfigs_OnCompleteGetsFiredWithCorrectInfo()
//         {
//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;
//             Assert.That(ConfigManager.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", ConfigManager.requestStatus, ConfigRequestStatus.Success);
//             Assert.That(ConfigManager.appConfig.origin == ConfigOrigin.Remote, "ConfigManager.appConfig.origin was {0}, should have been {1}", ConfigManager.appConfig.origin, ConfigOrigin.Remote);
//         }

//         [UnityTest]
//         public IEnumerator FetchConfigs_NullWorks()
//         {
//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<FetchConfigs_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;
//             Assert.That(ConfigManager.requestStatus == ConfigRequestStatus.Success, "ConfigManager.requestStatus was {0}, should have been {1}", ConfigManager.requestStatus, ConfigRequestStatus.Pending);
//         }

//         [UnityTest]
//         public IEnumerator ConfigFieldInitializedAsEmpty()
//         {
//             yield return null;
//             var emptyJObject = new JObject();
//             Assert.That(ConfigManager.appConfig != null);
//             Assert.That(ConfigManager.appConfig.config != null);
//             Assert.AreEqual(ConfigManager.appConfig.config.GetType(), emptyJObject.GetType());
//         }

//         [UnityTest]
//         public IEnumerator SaveCache_CreatesFileIfNotThereAndCachesRightContent()
//         {
//             var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//             if (File.Exists(fileName))
//             {
//                 File.Delete(fileName);
//             }

//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             while(!File.Exists(fileName)) yield return null;
//             Assert.That(File.Exists(fileName));
//             string text = File.ReadAllText(fileName);
//             Assert.That(text.Contains("testInt"));
//             ConfigManager.ConfigManagerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
//             ConfigManager.ConfigManagerImpl.LoadFromCache();
//             Assert.AreEqual(232, ConfigManager.appConfig.GetInt("testInt"));
//         }

//         [UnityTest]
//         public IEnumerator SaveCache_CachesMultipleRuntimeConfigs()
//         {
//             var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//             if (File.Exists(fileName))
//             {
//                 File.Delete(fileName);
//             }

//             ConfigManagerTestUtils.SetProjectIdOnRequestPayload();
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             while(!File.Exists(fileName)) yield return null;
//             var managerImpl = ConfigManager.ConfigManagerImpl;

//             var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),ConfigManagerTestUtils.jsonPayloadEconomyConfig);
//             managerImpl.HandleConfigResponse("economy", configResponse);

//             var configResponseSettings = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),ConfigManagerTestUtils.jsonPayloadString);
//             managerImpl.HandleConfigResponse(ConfigManagerImpl.DefaultConfigKey, configResponseSettings);

//             Assert.That(File.Exists(fileName));
//             string text = File.ReadAllText(fileName);
//             Assert.That(text.Contains(ConfigManagerImpl.DefaultConfigKey));
//             Assert.That(text.Contains("economy"));
//             Assert.That(managerImpl.configs.Count == 2);
//             Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
//             managerImpl.configs.Remove(ConfigManagerImpl.DefaultConfigKey);
//             managerImpl.configs.Remove("economy");
//             managerImpl.LoadFromCache();
//             Assert.AreEqual(12, ConfigManager.appConfig.GetInt("someInt"));
//             Assert.That(ConfigManager.GetConfig("economy").GetString("item") == "sword");
//             Assert.That(ConfigManager.GetConfig("economy").assignmentId == JObject.Parse(ConfigManagerTestUtils.jsonPayloadEconomyConfig)["metadata"]["assignmentId"].ToString()); 
//         }
//     }
// #endif
    
//     internal class RuntimeConfigTests
//     {
//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ProperlySetsAssignmentId()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("3049bfea-05fa-4ddf-acc6-ce43c888fe92", ConfigManager.appConfig.assignmentId);
//         }
//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ReturnsNullAssignmentIdWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("3049bfea-05fa-4ddf-acc6-ce43c888fe92", ConfigManager.appConfig.assignmentId);
//         }

//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ProperlySetsEnvironmentId()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("83fff3e2-a945-4601-9ccc-5e9d16d12ea8", ConfigManager.appConfig.environmentId);
//         }

//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ReturnsNullEnvironmentIdWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("83fff3e2-a945-4601-9ccc-5e9d16d12ea8", ConfigManager.appConfig.environmentId);
//         }

//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ProperlySetsConfig()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(9, ConfigManager.appConfig.GetKeys().Length);
//             Assert.AreEqual(true, ConfigManager.appConfig.HasKey("testInt"));
//             Assert.AreEqual(true, ConfigManager.appConfig.HasKey("longSomething"));
//         }

//         [UnityTest]
//         public IEnumerator ResponseParsedEventHandler_ProperlySetsConfigWhenBadResponse()
//         {
//             var configBeforeRequest = ConfigManager.appConfig.config;

//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(configBeforeRequest.ToString(), ConfigManager.appConfig.config.ToString());
//         }

//         [UnityTest]
//         public IEnumerator GetBool_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.That(ConfigManager.appConfig.GetBool("bool") == true);
//         }

//         [UnityTest]
//         public IEnumerator GetBool_ReturnsRightValueWhenBadResponse()
//         {
//             var managerImpl = ConfigManager.ConfigManagerImpl;
//             var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote,new Dictionary<string, string>(),ConfigManagerTestUtils.jsonPayloadString);
//             managerImpl.HandleConfigResponse(ConfigManagerImpl.DefaultConfigKey, configResponse);

//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             // at this point we have multiple configs in cache, but we are still able to get "bool" 
//             // from "settings" configType, eventhough we loaded "someOtherConfig" configType
//             Assert.AreEqual(true, ConfigManager.appConfig.GetBool("bool"));
//         }

//         [UnityTest]
//         public IEnumerator GetFloat_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(0.12999999523162842, ConfigManager.appConfig.GetFloat("heloe"));
//         }

//        [UnityTest]
//         public IEnumerator GetFloat_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             // at this point we have multiple configs in cache, but we are still able to get "heloe"
//             // from "settings" configType, eventhough we loaded "someOtherConfig" configType
//             Assert.AreEqual(0.12999999523162842, ConfigManager.appConfig.GetFloat("heloe"));
//         }

//         [UnityTest]
//         public IEnumerator GetInt_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(12, ConfigManager.appConfig.GetInt("someInt"));
//         }
//        [UnityTest]
//         public IEnumerator GetInt_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(12, ConfigManager.appConfig.GetInt("someInt"));
//         }

//         [UnityTest]
//         public IEnumerator GetString_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("madAF", ConfigManager.appConfig.GetString("madBro"));
//         }

//         [UnityTest]
//         public IEnumerator GetString_ReturnsRightValueIfFormattedAsDate()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("2020-04-03T10:01:00Z", ConfigManager.appConfig.GetString("stringFormattedAsDate"));
//         }

//         [UnityTest]
//         public IEnumerator GetString_ReturnsRightValueIfFormattedAsJson()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("{\"a\":2.0,\"b\":4,\"c\":\"someString\"}", ConfigManager.appConfig.GetString("stringFormattedAsJson"));
//         }
        
//         [UnityTest]
//         public IEnumerator GetString_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("madAF", ConfigManager.appConfig.GetString("madBro"));
//         }

//         [UnityTest]
//         public IEnumerator GetLong_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(9223372036854775806, ConfigManager.appConfig.GetLong("longSomething"));
//         }

//         [UnityTest]
//         public IEnumerator GetLong_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(9223372036854775806, ConfigManager.appConfig.GetLong("longSomething"));
//         }

//         [UnityTest]
//          public IEnumerator GetJson_ReturnsRightValue()
//          {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("{\"a\":1.0,\"b\":2,\"c\":\"someString\"}", ConfigManager.appConfig.GetJson("jsonSetting"));
//          }

//          [UnityTest]
//          public IEnumerator GetJson_ReturnsRightValueWhenBadResponse()
//          {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual("{\"a\":1.0,\"b\":2,\"c\":\"someString\"}", ConfigManager.appConfig.GetJson("jsonSetting"));
//          }

//         [UnityTest]
//         public IEnumerator HasKey_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.That(ConfigManager.appConfig.HasKey("longSomething"));
//         }

//         [UnityTest]
//         public IEnumerator HasKey_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(true, ConfigManager.appConfig.HasKey("longSomething"));
//         }

//         [UnityTest]
//         public IEnumerator GetKeys_ReturnsRightValue()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsComplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(9, ConfigManager.appConfig.GetKeys().Length );
//         }

//         [UnityTest]
//         public IEnumerator GetKeys_ReturnsRightValueWhenBadResponse()
//         {
//             var monoTest = new MonoBehaviourTest<FetchConfigsIncomplete_MonobehaviorTest>(false);
//             monoTest.component.StartTest();
//             yield return monoTest;

//             #if !UNITY_SWITCH && !UNITY_PS4 && !UNITY_XBOXONE && !UNITY_WII
//                 var fileName = Path.Combine(Application.persistentDataPath, ConfigManagerImpl.DefaultCacheFile);
//                 while(!File.Exists(fileName)) yield return null;
//                 Assert.That(File.Exists(fileName));
//             #endif

//             Assert.AreEqual(9, ConfigManager.appConfig.GetKeys().Length);
//         }
//     }

//     internal static class ConfigManagerTestUtils
//     {
//         public const string userId = "some-user-id";
//         public const string environmentId = "7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2";

//         public struct UserAttributes
//         {

//         }

//         public struct AppAttributes
//         {

//         }

//         public struct FilterAttributes
//         {

//         }

//         public static string jsonPayloadString =
//             @"{
//                 ""configs"": {
//                     ""settings"": {
//                         ""raywagagwawd"": """",
//                         ""settingsConfig"": ""{\""hello\"":2.0,\""someInt\"":32,\""madBro\"":\""fdsfadsf\""}"",
//                         ""fghfg"": ""ffgf"",
//                         ""longSomething"": 9223372036854775806,
//                         ""someInt"": 12,
//                         ""helloe"": 0.12999999523162842,
//                         ""blah"": ""blahd"",
//                         ""stringFormattedAsDate"": ""2020-04-03T10:01:00Z"",
//                         ""stringFormattedAsJson"": ""{\""a\"":2.0,\""b\"":4,\""c\"":\""someString\""}"",
//                         ""bool"": true,
//                         ""madBro"": ""madAF"",
//                         ""jsonKeys"": ""settingsConfig,gameConfig"",
//                         ""skiwdafsdwas"": ""hello"",
//                         ""jsonSetting"": ""{\""a\"":1.0,\""b\"":2,\""c\"":\""someString\""}"",
//                     }
//                 },
//                 ""metadata"": {
//                     ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a858"",
//                     ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2"",
//                 }
//             }";

//         public static string jsonPayloadStringNoRCSection =
//             @"{
//                 ""configs"": {
//                     ""someOtherKey"": {
//                     }
//                 },
//                 ""metadata"": {
//                     ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a811"",
//                     ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708d11"",
//                 }
//             }";

//         public static string jsonPayloadEconomyConfig = 
//             @"{
//                 ""configs"": {
//                     ""economy"": {
//                         ""item"": ""sword""
//                     }
//                 },
//                 ""metadata"": {
//                     ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a822"",
//                     ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708d22"",
//                 }
//             }";

//         public static void SetProjectIdOnRequestPayload()
//         {
//             FieldInfo configmanagerImplInfo = typeof(ConfigManager).GetField("_configManagerImpl", BindingFlags.Static | BindingFlags.NonPublic);
//             var configmanagerImpl = configmanagerImplInfo.GetValue(null);

//             FieldInfo rcRequestFieldInfo = typeof(ConfigManagerImpl).GetField("_remoteConfigRequest", BindingFlags.Instance | BindingFlags.NonPublic);
//             var common = rcRequestFieldInfo.GetValue(configmanagerImpl);

//             FieldInfo projectIdFieldInfo = common.GetType().GetField("projectId");
//             projectIdFieldInfo.SetValue(common, "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4");
//             rcRequestFieldInfo.SetValue(configmanagerImpl, common);
//         }

//         public static void FieldInvestigation(Type t)
//         {
//             Debug.Log("--------------FieldInvestigation--------------");
//             Debug.Log("Type: "+t);
//             FieldInfo[] fields = t.GetFields();
//             Debug.Log("field count: "+fields.Length);
//             foreach (var field in fields)
//             {
//                 Debug.Log("field: "+field.Name);
//             }
//         }

//         public static void EventInvestigation(Type t)
//         {
//             Debug.Log("--------------EventInvestigation--------------");
//             Debug.Log("Type: "+t);
//             EventInfo[] events = t.GetEvents();
//             Debug.Log("event count: "+events.Length);
//             foreach (var e in events)
//             {
//                 Debug.Log("event: "+e.Name);
//             }
//         }

//         public static void PropertyInvestigation(Type t)
//         {
//             Debug.Log("--------------PropertyInvestigation--------------");
//             Debug.Log("Type: "+t);
//             PropertyInfo[] properties = t.GetProperties();
//             Debug.Log("property count: "+properties.Length);
//             foreach (var property in properties)
//             {
//                 Debug.Log("property: "+property.Name);
//             }
//         }

//         public static void MethodInvestigation(Type t)
//         {
//             Debug.Log("--------------MethodInvestigation--------------");
//             Debug.Log("Type: "+t);
//             MethodInfo[] methods = t.GetMethods();
//             Debug.Log("methods count: "+methods.Length);
//             foreach (var method in methods)
//             {
//                 Debug.Log("method: "+method.Name);
//             }
//         }

//         public static void FullInvestigation(Type t)
//         {
//             FieldInvestigation(t);
//             EventInvestigation(t);
//             PropertyInvestigation(t);
//             MethodInvestigation(t);
//         }
//     }

//     internal interface IRCTest
//     {
//         void StartTest();
//     }
// }