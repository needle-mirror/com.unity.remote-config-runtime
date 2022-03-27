using System.Threading.Tasks;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Device.Internal;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.RemoteConfig
{
    /// <summary>
    /// This is the package initializer.
    /// By implementing <see cref="IInitializablePackage"/>, it will be initialized in the right order, based on dependencies
    /// </summary>
    class CorePackageInitializer : IInitializablePackage
    {
        /// <summary>
        /// Register to Core through a static method that is called before scene load.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // Pass an instance of this class to Core
            CoreRegistry.Instance.RegisterPackage(new CorePackageInitializer())
            // And specify what components it requires, or provides.
            .DependsOn<IProjectConfiguration>()
            .DependsOn<IInstallationId>()
            .DependsOn<IAccessToken>()
            .DependsOn<IPlayerId>();
        }

        /// <summary>
        /// This is the Initialize callback that will be triggered by the Core package.
        /// This method will be invoked when the game developer calls UnityServices.InitializeAsync().
        /// </summary>
        /// <param name="registry">
        /// The registry containing components from different packages.
        /// </param>
        /// <returns>
        /// Return a Task representing your initialization.
        /// </returns>
        public async Task Initialize(CoreRegistry registry)
        {
            // Get components, and cache references to them for later use if needed.
            var IprojectConfig = registry.GetServiceComponent<IProjectConfiguration>();
            var IinstallationId = registry.GetServiceComponent<IInstallationId>();
            var IplayerId = registry.GetServiceComponent<IPlayerId>();
            var Itoken = registry.GetServiceComponent<IAccessToken>();

            // The project configuration stores all service settings available at runtime.
            // analyticsUserId and installationId are coming from Core and they are immediately available
            // IplayerId and Itoken are coming from Auth and they will be ready upon users login
            CoreConfig.analyticsUserId = IprojectConfig.GetString("com.unity.services.core.analytics-user-id");
            CoreConfig.installationId = IinstallationId.GetOrCreateIdentifier();
            CoreConfig.Itoken = Itoken;
            CoreConfig.IplayerId = IplayerId;

            // Do any other initialization needed.
            await Task.Yield();
        }

    }
    public static class CoreConfig
    {
        public static string analyticsUserId;
        public static string installationId;
        public static IAccessToken Itoken;
        public static IPlayerId IplayerId;
    }
}