using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Services.RemoteConfig.Tests
{
    internal class FetchConfigs_QueueImmediately_MonobehaviorTest : MonoBehaviour, IMonoBehaviourTest, IRCSTest
    {
        private bool testFinished = false;
        public bool IsTestFinished
        {
            get { return testFinished; }
        }

        public void StartTest()
        {
            FetchConfigs();
        }

        private void Awake()
        {

        }

        void FetchConfigs()
        {
            RemoteConfigService.Instance.FetchConfigs<
                RemoteConfigServiceTestUtils.UserAttributes,
                RemoteConfigServiceTestUtils.AppAttributes,
                RemoteConfigServiceTestUtils.FilterAttributes>
                (new RemoteConfigServiceTestUtils.UserAttributes(),
                new RemoteConfigServiceTestUtils.AppAttributes(),
                new RemoteConfigServiceTestUtils.FilterAttributes());
            testFinished = true;
        }
    }
}
