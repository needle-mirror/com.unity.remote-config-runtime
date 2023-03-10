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
    public int enemyVolume;

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
                Debug.Log("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                Debug.Log("ConfigManager.appConfig fetched: " + ConfigManager.appConfig.config.ToString());
                break;
        }

        enemyVolume = ConfigManager.appConfig.GetInt("enemyVolume");

        // These calls could also be used with the 2nd optional arg to provide a default value, e.g:
        // enemyVolume = ConfigManager.appConfig.GetInt("enemyVolume", 100);

    }

}
