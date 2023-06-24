using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Services.RemoteConfig.Tests
{
    internal class FetchConfigsIncomplete_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCSTest
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
                @"{""configs"": {""someOtherConfigType"":{""someTestInt"":555}},""metadata"":{""assignmentId"":""5555555-05fa-4ddf-acc6-ce43c888fe92"",""environmentId"":""5555555-a945-4601-9ccc-5e9d16d12ea8""}}";
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
