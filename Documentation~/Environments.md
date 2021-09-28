# Environments

Unity Remote Config Runtime uses environments to group Rules and Settings, and deliver specific environments to the client (Unity Runtime Instance) based on the `environmentId`. If no `environmentId` is provided in the request the default environment is returned to that instance.

Use the [Web Dashboard](http://dashboard.unity3d.com/remote-config) to manage your Remote Config Runtime environments. Upon initialization in a new project, only the default environment is available.

## Restrictions

- A project can have a maximum of 10 environments
- Environment names must be unique
- The `Development` environment name is reserved and cannot be created by users

## Working with environments
The [Web Dashboard](http://dashboard.unity3d.com/remote-config) lets you create, edit, and delete environments. The [REST APIs](RESTAPI.md) contain further information about this functionality.

Each environment has the following main parameters.

| **Parameter** | **Description** |
| ------------- | --------------- |
| **Environment Name** | The readable identifier for the environment. For example, you might name an environment to match a given build of your game `com.Unity.MyGame@1.1.1`; or for a team member, to let them work independently of the settings used by others `@yourNameGoesHere`.|
| **Environment Id** | The generated UUID (universally unique identifier) for the environment. The client uses this when requesting the Rules and Settings. <br>This parameter is required for integration.|
| **is Default** | Sets the environment as the default environment, which is returned to a Unity Instance when the request contains no `environmentId` or an invalid `environmentId`.|
