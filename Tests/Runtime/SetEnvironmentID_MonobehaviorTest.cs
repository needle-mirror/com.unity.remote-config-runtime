using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Services.RemoteConfig.Tests
{
    internal class SetEnvironmentID_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCSTest
    {
        private bool testFinished = false;
        public bool IsTestFinished
        {
            get { return testFinished; }
        }

        public void StartTest()
        {
            RemoteConfigService.Instance.SetEnvironmentID(RemoteConfigServiceTestUtils.environmentId);
            testFinished = true;
        }
    }
}
