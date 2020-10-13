using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.RemoteConfig.Tests
{
    internal class FetchConfigsComplete_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCTest
    {
        private bool testFinished = false;
        public bool IsTestFinished
        {
            get { return testFinished; }
        }

        void FetchConfigs()
        {
            // mocking successful response 
            var origin = ConfigOrigin.Remote;
            var responseHeaders = new Dictionary<string,string>();
            responseHeaders.Add("Date", DateTime.Now.ToString());
            responseHeaders.Add("Access-Control-Allow-Origin", "*");
            responseHeaders.Add("X-REMOTE-CONFIG-SIGNATURE", "MEQCIHmk1PeRMSDNG7+U8QJ8YtTDKqwL+8OcNd4AnLKnrGoTAiBilRvsutIQkPZo7qC20P+Up0/lm2knDIPW5ULV7lJhJw==");
            responseHeaders.Add("Content-Type", "application/json;charset=utf-8");
            responseHeaders.Add("Content-Length", "410");
            responseHeaders.Add("Server", "Jetty(9.4.z-SNAPSHOT)");

            var downloadHandlerText = "{'analytics':{'enabled':true},'connect':{'limit_user_tracking':false,'player_opted_out':false,'enabled':true},'performance':{'enabled':true},'settings':{'testInt':232},'settingsMetadata':{'assignmentId':'3049bfea-05fa-4ddf-acc6-ce43c888fe92','environmentId':'83fff3e2-a945-4601-9ccc-5e9d16d12ea8'}}";
            ConfigManager.requestStatus = ConfigRequestStatus.Success;
            ConfigManager.appConfig.origin = origin;
            ConfigManager.SaveCache(origin, responseHeaders, downloadHandlerText);
            testFinished = true;
        }

        public void StartTest()
        {
            FetchConfigs();
        }
    }
}
