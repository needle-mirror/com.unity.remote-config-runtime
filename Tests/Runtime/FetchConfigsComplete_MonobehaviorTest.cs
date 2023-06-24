using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Services.RemoteConfig.Tests
{
    internal class FetchConfigsComplete_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCSTest
    {
        private bool testFinished = false;
        public bool IsTestFinished
        {
            get { return testFinished; }
        }

        void FetchConfigs()
        {
            // mocking successful response 
            var responseHeaders = new Dictionary<string,string>();
            responseHeaders.Add("Date", DateTime.Now.ToString());
            responseHeaders.Add("Access-Control-Allow-Origin", "*");
            responseHeaders.Add("X-REMOTE-CONFIG-SIGNATURE", "MEQCIHmk1PeRMSDNG7+U8QJ8YtTDKqwL+8OcNd4AnLKnrGoTAiBilRvsutIQkPZo7qC20P+Up0/lm2knDIPW5ULV7lJhJw==");
            responseHeaders.Add("Content-Type", "application/json;charset=utf-8");
            responseHeaders.Add("Content-Length", "410");
            responseHeaders.Add("Server", "Jetty(9.4.z-SNAPSHOT)");

            var downloadHandlerText =
                @"{""configs"": {""settings"":{""testInt"":232, ""bool"":true, ""madBro"": ""madAF"", ""someInt"":12, ""heloe"":0.12999999523162842, ""longSomething"": 9223372036854775806, ""stringFormattedAsDate"": ""2020-04-03T10:01:00Z"", ""stringFormattedAsJson"": ""{\""a\"":2.0,\""b\"":4,\""c\"":\""someString\""}"", ""jsonSetting"": ""{\""a\"":1.0,\""b\"":2,\""c\"":\""someString\""}"" }},""metadata"":{""assignmentId"":""3049bfea-05fa-4ddf-acc6-ce43c888fe92"",""environmentId"":""83fff3e2-a945-4601-9ccc-5e9d16d12ea8""}}";
            var managerImpl = RemoteConfigService.Instance.ConfigManagerImpl;
            var configResponse = managerImpl.ParseResponse(ConfigOrigin.Remote, responseHeaders, downloadHandlerText);
            managerImpl.HandleConfigResponse(ConfigManagerImpl.DefaultConfigKey, configResponse);
            testFinished = true;
        }

        public void StartTest()
        {
            FetchConfigs();
        }
    }
}