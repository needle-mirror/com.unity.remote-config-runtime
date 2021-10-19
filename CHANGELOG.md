# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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
