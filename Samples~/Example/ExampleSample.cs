// -----------------------------------------------------------------------------
//
// This sample example C# file can be used to quickly utilise usage of Remote Config APIs
// For more comprehensive code integration, visit https://docs.unity3d.com/Packages/com.unity.remote-config@latest
//
// -----------------------------------------------------------------------------

using UnityEngine;
using Unity.RemoteConfig;

public class ExampleSample : MonoBehaviour
{

    public struct userAttributes {}
    public struct appAttributes {}

    void Start()
    {
        ConfigManager.FetchCompleted += ConfigManager_FetchCompleted;
        ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
    }

    void ConfigManager_FetchCompleted(ConfigResponse configResponse)
    {

        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("Default values will be returned");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("Cached values loaded");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("Remote Values changed");
                Debug.Log("ConfigManager.appConfig fetched: " + ConfigManager.appConfig.config.ToString());
                break;
        }

    }

}