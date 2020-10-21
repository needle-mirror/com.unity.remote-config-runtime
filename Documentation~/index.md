# Unity Remote Config Runtime

Unity Remote Config Runtime is a cloud service that lets you tune your game design without deploying new versions of your application. With Remote Config Runtime, you can:

* Adapt your game to different types of players.
* Tune your game difficulty curve in near real time.
* Alter graphic quality based on device to optimize performance.
* Roll out new features gradually while monitoring impact.
* Tailor game settings to different regions or other player segments.
* Run campaign tests comparing colors, styles, prices, etc.
* Enable or disable seasonal, holiday, or other time-sensitive events.
* Enable or disable features for specific player segments or across the entire user base.

In the Web Dashboard, you can create and manage [environments](Environments.md) to group rules and settings together in ways that support your development and deployment workflow. You can structure environments to fit your application so that specific rules and settings are retrieved and updated only when needed. This enables reuse of rules and settings keys.

Define rules that control which players receive what settings updates, and when. Unity manages the delivery and assignment of those settings with minimal impact to performance. No update to your application is necessary. When a player launches your game, Remote Config Runtime detects contextual attributes used as rule conditions, based on Unity, the application, the user, or custom criteria that you define. The service then returns customized settings for each player according to the rules that apply to them. This allows different players using the same version of your game to have slightly different experiences. It also allows you to understand the impact each experience has on your business.

`Verified Package`
**Note**: This is a verified version of the Remote Config Runtime package, however the service is under active development and subject to changes that may impact the service's stability. If you encounter any issues, or have any questions, please [contact us](mailto:remote-config@unity3d.com).
