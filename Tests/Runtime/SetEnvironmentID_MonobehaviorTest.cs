using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.RemoteConfig.Tests
{
    internal class SetEnvironmentID_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCTest
    {
        private bool testFinished = false;
        public bool IsTestFinished
        {
            get { return testFinished; }
        }

        public void StartTest()
        {
            ConfigManager.SetEnvironmentID(ConfigManagerTestUtils.environmentId);
            testFinished = true;
        }
    }
}
