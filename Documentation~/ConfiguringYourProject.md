# Configuring your project for Unity Remote Config Runtime

## Requirements

* Unity Remote Config Runtime requires Unity version 2018.4 or higher.
* Set your [Editor scripting runtime](https://docs.unity3d.com/2018.4/Documentation/Manual/ScriptingRuntimeUpgrade.html) to **.NET 4.X Equivalent** (or above).
* Set your [API Compatibility Level](https://docs.unity3d.com/2019.3/Documentation/Manual/dotnetProfileSupport.html) to **.NET 4.x**.
* [Enable Unity Services](https://docs.unity3d.com/2019.3/Documentation/Manual/SettingUpProjectServices.html) for your project.
* Install the Remote Config Runtime package (detailed below).  

## Installing the Remote Config Runtime package
See documentation on [packages](https://docs.unity3d.com/Manual/Packages.html) for more information on working with packages in a project. These steps may vary depending on which version of the Unity Editor you’re using.

<a name ="verified"></a>
### Verified release

1. In the Unity Editor, select **Window** > **Package Manager**.
2. From the Package Manager window, find Remote Config Runtime in the Packages List view and select it.
3. In the Package Specific Detail view, select the version and install to import the package into your project.

### Preview release
1. In the Package Manager window, the **Advanced** button lets you toggle **Show Preview Packages** to display them in the Package List view.
2. Follow the instructions for the [Verified Release installation](#verified).

### Beta customers
Upon receiving the Remote Config Runtime package from your account manager, follow these steps:

1. Download and unzip the package.
2. In the Unity Editor, select **Window** > **Package Manager**.
3. In the Package Manager window, select **Add** (**+**) to open the **Add package from disk...** dialog.
4. Locate the _package.json_ file inside your unzipped copy of the Remote Config Runtime package.
5. Select **Open** to import the package into your project.

## Remote Config Runtime environments

To get started, create an environment and give it a name.

1. Go to the [Web Dashboard](http://dashboard.unity3d.com/remote-config).
2. Select the corresponding project.
3. Select **Add Environment**.
4. Enter a name for the environment and select **Create**.

**Note environment names are immutable.**

The first environment you create is set as the default environment. This is the environment which is requested unless otherwise specified by the client. You can assign the default environment to an **EnvironmentID** in the Web Dashboard, or via the REST API.

Once you’ve configured your project, configure your rules and settings in the [Web Dashboard](http://dashboard.unity3d.com/remote-config)
