# RCR - Caching Mechanism

Remote Config Runtime uses caching mechanism, which works as follows:
- RCR is reading the last saved config from the cache file on the disk, and if successful, that config gets applied
- If the app is connecting for the first time and there is no cache file, RCR will apply the default values for each variable type (0 for int, "" for string, etc...)
- RCR makes a request, and if successful, it overrides the previously applied cached config
- The last received config also overrides the values in the cache file
- This mechanism is applied for all platforms except for consoles, as writing on the disc for consoles requires special platform permissions


## Cache file location
The location of the cache file is in
`Application.persistentDataPath/{YourApp}/RemoteConfigCache.json`, which differs by platform,
as depicted in [persistentDataPath docs](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)

E.g for mac, the cache file can be found under:
**/Users/YourName/Library/Application Support/DefaultCompany/{YourApp}/RemoteConfigCache.json**

Contents of the example cache file:
```json
{
    "settings": {
        "requestOrigin": 2,
        "status": 2,
        "body": {
            "configs": {
                "settings": {
                    "rotateY": 30.0,
                    "rotSpeed": 44.0,
                    "swordName": "Singing Sword",
                    "rotateZ": 4.0,
                    "jsonCubeCustom": {
                        "rotateX": 20,
                        "color": {
                            "r": 0.2,
                            "g": 0.2,
                            "b": 0.3,
                            "a": 1
                        }
                    },
                    "swordPrice": 5,
                    "testdateString": "2021-10-21T10:00:00.0000001-07:00",
                    "rotateX": 777.0
                }
            },
            "metadata": {
                "assignmentId": "355faf1d-9ce0-4528-9267-95ffe07ed891",
                "environmentId": "11daa94d-0ffa-41e2-8579-f4acbbdb3987",
                "bundle": "657abf6ee46c9dc227581a7adba48bca89dfb620"
            }
        },
        "headers": {
            "Date": "Mon, 15 Nov 2021 17:44:08 GMT",
            "Content-Type": "application/json;charset=utf-8",
            "Access-Control-Allow-Origin": "\*",
            "Content-Length": "429",
            "Server": "Jetty(9.4.z-SNAPSHOT)",
            "Via": "1.1 google",
            "Alt-Svc": "clear",
            "Connection": "keep-alive"
        }
    }
}
```

## Caching Scenarios

Within caching mechanism, we maintain two values, `requestOrigin` and `status` 
which can be utilized to understand caching scenarios.

They can be accessed from `configResponse` variable, e.g this is how we utilize requestOrigin within the callback:

```c#
void ApplyRemoteSettings (ConfigResponse configResponse) {
        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin) {
            case ConfigOrigin.Default:
                Debug.Log ("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log ("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log ("New settings loaded this session; update values accordingly.");
                break;
        }
    }
```

Possible values for requestOrigin are:  `{0:Default, 1:Cached, 2:Remote}`

Possible values for status are: `{0:None, 1:Failed, 2:Success, 3:Pending}`

Based on those values, we recognize the following caching scenarios:

1. if the user was never connected before and has no connection, default values should be applied.
2. if the user was never connected before and now has a connection, remote values should be applied and written in the cache on the local disk
3. If the user was connected at least once, and now has no connection, cached values should be applied
4. If the user was connected at least once, and now has a connection, remote values should be applied as they overwrite cached values

Those scenarios should fit the table below:

| Scenario | Connected Before | Has Connection | CacheFile | Origin | Status |
| --- | --- | --- | --- | --- | --- |
| 1 | No | No | none | Default | Failed |
| 2 | No | Yes | yes | Remote | Success |
| 3 | Yes | No | yes | Cached | Failed |
| 4 | Yes | Yes | yes | Remote | Success |
