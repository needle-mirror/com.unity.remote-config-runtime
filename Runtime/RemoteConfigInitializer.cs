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
    class RemoteConfigInitializer : IInitializablePackage
    {
        /// <summary>
        /// Register to Core through a static method that is called before scene load.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // Pass an instance of this class to Core
            CoreRegistry.Instance.RegisterPackage(new RemoteConfigInitializer())
            // And specify what components it requires, or provides.
            .DependsOn<IInstallationId>()
            .DependsOn<IAccessToken>()
            .DependsOn<IPlayerId>()
            .DependsOn<IEnvironmentId>()
            .DependsOn<IExternalUserId>();
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
        public Task Initialize(CoreRegistry registry)
        {
            // Get components, and cache references to them for later use if needed.
            var IexternalUserId = registry.GetServiceComponent<IExternalUserId>();
            var IinstallationId = registry.GetServiceComponent<IInstallationId>();
            var IplayerId = registry.GetServiceComponent<IPlayerId>();
            var Itoken = registry.GetServiceComponent<IAccessToken>();
            var IenvironmentId = registry.GetServiceComponent<IEnvironmentId>();

            // The project configuration stores all service settings available at runtime.
            // analyticsUserId and installationId are coming from Core and they are immediately available
            // IplayerId and Itoken are coming from Auth and they will be ready upon users login
            CoreConfig.analyticsUserId = IexternalUserId.UserId;
            IexternalUserId.UserIdChanged += (id) => CoreConfig.analyticsUserId = id;
            CoreConfig.installationId = IinstallationId.GetOrCreateIdentifier();
            CoreConfig.Itoken = Itoken;
            CoreConfig.IplayerId = IplayerId;
            CoreConfig.IenvironmentId = IenvironmentId;

            // Do any other initialization needed.
            return Task.CompletedTask;
        }

    }
    public static class CoreConfig
    {
        public static string analyticsUserId;
        public static string installationId;
        public static IAccessToken Itoken;
        public static IPlayerId IplayerId;
        public static IEnvironmentId IenvironmentId;
    }
}