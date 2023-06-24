using System;
using UnityEngine.Scripting;

namespace Unity.Services.RemoteConfig
{
    [Serializable]
    class AccessToken
    {
        [Preserve]
        public AccessToken() {}

        public string[] aud;
    }

    /// <summary>
    /// Trimmed-down and specialized version of:
    /// https://github.com/monry/JWT-for-Unity/blob/master/JWT/JWT.cs
    /// At time of writing, this source was public domain (Creative Commons 0)
    /// </summary>
    class JwtDecoder
    {
        public static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding

            var mod4 = input.Length % 4;
            if (mod4 > 0)
            {
                output += new string('=', 4 - mod4);
            }

            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }
}
