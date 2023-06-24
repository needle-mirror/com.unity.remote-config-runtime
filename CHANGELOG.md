# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [4.0.1] - 2023-06-24

- Dropped support for Unity 2020.3, minimum supported is 2021.3 now

## [4.0.0] - 2023-06-14

- Removed 2.x namespace and corresponding API's for backward compatibility with 2.x implementation
- Removed static ConfigManager class
- Updated SDK documentation regarding caching and unity attributes
- Updated `com.unity.services.core` dependency from `1.5.2` to `1.8.1`
- Promote Candidate Experimental package to Verified Production
- Dropped support for Unity 2019, minimum supported is 2020.3 now
- Updated delivery endpoint from `https://config.unity3d.com/settings` to `https://config.services.api.unity.com/settings`
- Added fix for inadvertently persisted auth token

## [3.2.0-pre.1] - 2022-10-19

- Renamed CorePackageInitializer to RemoteConfigInitializer
- Initialize method is now synchronous
- Removed await Task.Delay(1); from FetchConfigsAsync method
- Refactored WebRequest in order to adhere to WebGL implementation
- Added documentation for caching mechanism, updated restApi links
- ExampleSample made visible in package manager
- Updated `com.unity.services.core` dependency from `1.4.3` to `1.5.2`
- <IProjectConfiguration> component replaced with <IExternalUserId> component within initialization, changing the way RCR accesses analyticsUserId
- environmentId extracted from component instead of the accessToken

## [3.1.3] - 2022-06-17

- using ENABLE_CLOUD_SERVICES_ANALYTICS flag for platforms using Analytics within 2.x namespace

## [3.1.2] - 2022-06-09

- Whitelisting platforms for getting userId from Analytics package within 2.x namespace

## [3.1.1] - 2022-06-08

- Allowing projectId and userId params to be in the payload for consoles

## [3.1.0] - 2022-05-26

- Updated `com.unity.services.core` dependency from `1.3.2` to `1.4.0`
- Added 2.x namespace and corresponding API's for backward compatibility with 2.x implementation
- Promote Candidate Experimental package to Verified Production

## [3.0.0] - 2022-04-29

- Updated `com.unity.services.core` dependency from `1.3.1` to `1.3.2`
- Updated SDK documentation
- Promote Candidate Experimental package to Verified Production

## [3.0.0-pre.30] - 2022-04-21

- Updated `com.unity.services.authentication` dependency from `1.0.0-pre.37` to `2.0.0`

## [3.0.0-pre.29] - 2022-04-08

- Updated `com.unity.services.core` dependency from `1.2.0` to `1.3.1`
- Updated `com.unity.nuget.newtonsoft-json` dependency from `3.0.1` to `3.0.2`

## [3.0.0-pre.28] - 2022-03-27

- Added `RemoteConfigService.Instance` as an access pattern instead of static class `ConfigManager`
- Added API Updater [Obsolete] for ConfigManager
- Refactored CorePackageInitializer
- Adjusted unit tests
- Project namespace changed from `Unity.RemoteConfig` to `Unity.Services.RemoteConfig`

## [3.0.0-pre.27] - 2022-03-17

- Fixed bug for origin value if there is no internet

## [3.0.0-pre.26] - 2022-03-08

- Updated `com.unity.services.core` dependency from `1.1.0-pre.41` to `1.2.0`
- Updated `com.unity.nuget.newtonsoft-json` dependency from `2.0.2` to `3.0.1`
- Updated documentation regarding metadata parameters within web response

## [3.0.0-pre.25] - 2022-02-16

- changed encoding from ASCII to UTF8 when reading from cache file

## [3.0.0-pre.24] - 2022-02-14

- added FetchCompleted listener on RuntimeConfig

## [3.0.0-pre.23] - 2022-02-10

- Bypassing cache mechanism for consoles
- Refactored unit tests
- Updated useCasesUrl in package.json

## [3.0.0-pre.22] - 2022-02-05

- FetchCompleted callback works within the app for multiple configs
- WebRequest properly disposed for async FetchConfig
- Updated unit tests to use standard c# `while (!condition) yield return null` instead of Unity's `WaitUntil()`

## [3.0.0-pre.21] - 2022-02-03

- Utilized configAssignmentHash parameter in the request in order to ensure persistent config in response
- Updated delivery endpoint from `https://remote-config-prd.uca.cloud.unity3d.com/settings` to `https://config.unity3d.com/settings`
- Removed unnecessary auth and core assembly dependencies
- Removed unnecessary using directives
- Refactored unit tests
- Set userAnalyticsId to installationId by default, overwrite possible if userAnalyticsId is set from within the app
- Resolved CS1998 warning when installing new package version

## [3.0.0-pre.19] - 2022-01-13

- Fixed bug for successive requests with different config types

## [3.0.0-pre.18] - 2021-12-16

- Removed obsolete chunkedTransfer property from UnityWebRequest
- Added playerId and installationId in the request headers
- Updated `com.unity.services.core` dependency from `1.0.0-pre.11` to `1.0.0-pre.41`
- Updated `com.unity.services.authentication` dependency from `1.0.0-pre.6` to `1.0.0-pre.37`
- Added warnings if core / auth services are not initialized

## [3.0.0-pre.17] - 2021-12-06

- Bypassed exception error from auth if token or playerId is not available
- Retrieving projectId from UnityEngine Application.cloudProjectId static property
- Updated integration docs for using different configType

## [3.0.0-pre.16] - 2021-11-26

- Retrieving projectId from core services, removing preprocessor directives for consoles

## [3.0.0-pre.15] - 2021-11-23

- Reverted `com.unity.services.core` dependency from `1.0.0-pre.18` to `1.0.0-pre.11`

## [3.0.0-pre.14] - 2021-11-17

- Bypassed exception error from core if there is no internet connection
- Added analyticsCustomId from core package in the request payload
- Updated `com.unity.services.core` dependency from `1.0.0-pre.11` to `1.0.0-pre.18`
- Upgraded Newtonsoft version from `2.0.0` to `2.0.2`
- Fixed bug for returning incorrect request origin in case of a failed request

## [3.0.0-pre.13] - 2021-10-25

- Added playerId from auth package in the request payload
- Updated `com.unity.services.core` dependency from `1.0.0-pre.10` to `1.0.0-pre.11`

## [3.0.0-pre.12] - 2021-10-19

- Bug fix to address editor memory leak
- Removing dependency on `com.unity.modules.unityanalytics@1.0.0` package
- Sending installationId as userId instead of AnalyticsSessionInfo.userId
- Update with upm package docs
- Add signature method to publishing
- Cleaned up internal manifest.json file

## [3.0.0-pre.11] - 2021-10-18

- Updated FetchConfigsAsync method in ConfigManager
- Updated - Updated `com.unity.services.authentication` dependency from `1.0.0-pre.4` to `1.0.0-pre.6`
- Updated - Updated `com.unity.services.core` dependency from `1.0.0-pre.8` to `1.0.0-pre.10`

## [3.0.0-pre.10] - 2021-09-22

- Updated `com.unity.services.authentication` dependency from `1.0.0-pre.5` to `1.0.0-pre.4`

## [3.0.0-pre.9] - 2021-09-21

- Updated Docs for Unity Gaming Services (UGS)
- Fixed links in Documentation, and removed unneeded APIs Docs
- Updated `com.unity.services.authentication` dependency from `1.0.0-pre.4` to `1.0.0-pre.5`
- Updated ExampleSample

## [3.0.0-pre.8] - 2021-08-31

- Updated Docs for Environments
- Added additional Platform support
- Updated tests to run initialization only in editor

## [3.0.0-pre.7] - 2021-08-23

- Updated editor versions for release

## [3.0.0-pre.6] - 2021-08-19

- Updated test fixtures for instabilities

## [3.0.0-pre.5] - 2021-08-18

- Updated test fixtures to allow cache creation
- Update documentation and example to new async methods

## [3.0.0-pre.4] - 2021-08-17

- Updated `com.unity.services.authentication` dependency

## [3.0.0-pre.3] - 2021-08-17

- Updated `com.unity.services.core` dependency
- Updated `com.unity.services.authentication` dependency

## [3.0.0-pre.2] - 2021-08-10

- Added async remote config request for fetching filtered settings
- Updated `com.unity.services.core` dependency
- Updated `com.unity.services.authentication` dependency
- Updated Documentation

## [3.0.0-pre.1] - 2021-07-28

- Minimum Editor version is now 2019.4 with the addition of com.unity.services dependencies
- Platform support is currently restricted to PC, Mac, Android, iOS with the 3.0.x versions of Remote Config Runtime
- Added	`com.unity.services.core` as a dependency
- Added `com.unity.services.authentication` as a dependency
- Updated Documentation

## [2.1.3-exp.4] - 2022-03-17

- Fixed bug for origin value if there is no internet

## [2.1.3-exp.3] - 2022-03-09

- Upgraded Newtonsoft version from 2.0.2 to 3.0.1
- Updated documentation regarding metadata parameters within web response

## [2.1.3-exp.2] - 2022-02-22

- Changed encoding from ASCII to UTF8 when reading from cache file
- Bypassing cache mechanism for consoles
- Refactored unit tests to reflect bypassing cache for consoles and support multiple configs
- Updated useCasesUrl in package.json
- FetchCompleted callback works within the app for multiple configs
- Updated unit tests to use standard c# `while (!condition) yield return null` instead of Unity's `WaitUntil()`
- Utilized configAssignmentHash parameter in the request in order to ensure persistent config in response
- Updated delivery endpoint from `https://remote-config-prd.uca.cloud.unity3d.com/settings` to `https://config.unity3d.com/settings`
- Set userId to non-null value so it can work for consoles, added SetUserId() method in ConfigManager
- Fixed bug for successive requests with different config types

## [2.1.3-exp.1] - 2021-11-16

- Upgraded Newtonsoft version from 2.0.0 to 2.0.2
- Fixed bug for returning incorrect request origin in case of a failed request

## [2.1.2] - 2021-10-05

- Update with upm package docs
- Add signature method to publishing
- Bug fix to address editor memory leak

## [2.1.1] - 2021-09-28

- Bump version to match RC for Verified Package Set for 2021 LTS
- Doc Fixes
- Update Console Flags

## [2.0.1] - 2021-09-28

- Updated docs
- Verify Package 2.0.x release stream for 2021 LTS

## [2.0.1-exp.1] - 2021-06-16

- Added support for filtering settings

## [2.0.0] - 2021-05-17
- Promote Candidate experimental package to Verified Production

## [2.0.0-exp.1] - 2021-03-16

- Added support for multiple configs with different configTypes
- Added support for Player Identity tokens

## [1.0.2-exp.1] - 2021-02-09

- Adjusted upm files for tests in isolation
- Added documentation for Apple Privacy Survey
- Added .sample.json file

## [1.0.1] - 2020-12-02

- Documented previously undocumented methods within IRCUnityWebRequest and ConfigManagerImpl
- Updated originService to 'remote-config'
- Updated yamato files

## [1.0.0] - 2020-10-21

- Promote Candidate Preview package to Verified Production

## [1.0.0-exp.3] - 2020-10-21

- Updated Documentation Files

## [1.0.0-exp.2] - 2020-10-13

- Removed Core folder with corresponding .asmdef files

## [1.0.0-exp.1] - 2020-10-13

- published first experimental version

## [1.0.0] - 2020-10-10

- Upped the version before release

## [0.0.1] - 2020-07-19

- Initial commit
