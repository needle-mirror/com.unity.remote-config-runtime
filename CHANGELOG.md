# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


## [2.1.3-exp.7] - 2022-06-18

- using ENABLE_CLOUD_SERVICES_ANALYTICS flag for platforms using Analytics within 2.x namespace

## [2.1.3-exp.6] - 2022-06-09

- Whitelisting platforms for getting userId from Analytics package within 2.x namespace

## [2.1.3-exp.5] - 2022-06-06

- Allowing projectId and userId params to be in the payload for consoles
- Upgraded Newtonsoft version from 3.0.1 to 3.0.2

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
