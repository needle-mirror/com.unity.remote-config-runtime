# Environments

Unity Remote Config Runtime uses environments to group Game Overrides and Settings, and deliver specific environments to the client (Unity Runtime Instance) based on the `environmentId`. If no `environmentId` is provided in the request - the default environment, which is the environment named `production` is returned to that instance.

Use the [Web Dashboard](http://dashboard.unity3d.com/remote-config) to manage your Remote Config Runtime environments. Upon initialization in a new project, only the default environment is available.

## Restrictions

- A project can have a maximum of 10 environments
- Environment names must be unique
- The `development` environment name is reserved, and cannot be created by users for users of `Remote Config SDK Version` <= `@1.1.x`
- The `production` environment name is reserved, and it is the default environment. If an environmentID is NOT specified in the request the environment `production` will be served.

## Working with environments
The [Web Dashboard](http://dashboard.unity3d.com/remote-config) lets you create, edit, and delete environments. This functionality is also available in the [REST APIs](RESTAPI.md).

Each environment has the following main parameters.

| **Parameter** | **Description** |
| ------------- | --------------- |
| **Environment Name** | The readable identifier for the environment. For example, you might name an environment to match a given build of your game `com_Unity_MyGame-1_1_1`; or for a team member, to let them work independently of the settings used by others `yourNameGoesHere`.|
| **Environment Id** | The generated UUID (universally unique identifier) for the environment. The client uses this when requesting the Rules and Settings. <br>This parameter is required for integration.|
| **is Default** | This is not a user controllable flag as of 2021-07-30; the Environment Named `production` will be served as the default environment for builds that do not specify an EnvironmentID|
