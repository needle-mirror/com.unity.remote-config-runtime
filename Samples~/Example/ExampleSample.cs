// -----------------------------------------------------------------------------
//
// This sample example C# file can be used to quickly utilise usage of Remote Config APIs
// For more comprehensive code integration, visit https://docs.unity3d.com/Packages/com.unity.remote-config@latest
//
// -----------------------------------------------------------------------------

using System.Threading.Tasks;
using Unity.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;


public class ExampleSample : MonoBehaviour
{

    public struct userAttributes {}
    public struct appAttributes {}

    async Task InitializeRemoteConfigAsync()
    {
            // initialize handlers for unity game services
            await UnityServices.InitializeAsync();

            // options can be passed in the initializer, e.g if you want to set analytics-user-id use two lines from below:
            // var options = new InitializationOptions().SetOption("com.unity.services.core.analytics-user-id", "my-user-id-123");
            // await UnityServices.InitializeAsync(options);
            // for all valid settings and options, check
            // https://pages.prd.mz.internal.unity3d.com/mz-developer-handbook/docs/sdk/operatesolutionsdk/settings-and-options-for-services

            // remote config requires authentication for managing environment information
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
    }

    async Task Start()
    {
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }

        ConfigManager.FetchCompleted += ConfigManager_FetchCompleted;
        ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());

        // -- Example on how to fetch configuration settings using filter attributes:
        // var fAttributes = new filterAttributes();
        // fAttributes.key = new string[] { "sword","cannon" };
        // ConfigManager.FetchConfigs(new userAttributes(), new appAttributes(), fAttributes);

        // -- Example on how to fetch configuration settings if you have dedicated configType:
        // var configType = "specialConfigType";
        // ConfigManager.FetchConfigs(configType, new userAttributes(), new appAttributes());
        // -- Configuration can be fetched with both configType and fAttributes passed
        // ConfigManager.FetchConfigs(configType, new userAttributes(), new appAttributes(), fAttributes);

        // -- All examples from above will also work asynchronously, returning Task<RuntimeConfig>
        // await ConfigManager.FetchConfigsAsync(new userAttributes(), new appAttributes());
        // await ConfigManager.FetchConfigsAsync(new userAttributes(), new appAttributes(), fAttributes);
        // await ConfigManager.FetchConfigsAsync(configType, new userAttributes(), new appAttributes());
        // await ConfigManager.FetchConfigsAsync(configType, new userAttributes(), new appAttributes(), fAttributes);
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