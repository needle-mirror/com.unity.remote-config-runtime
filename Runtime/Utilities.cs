using System.Net;

namespace Unity.Services.RemoteConfig
{
    public static class Utilities
    {
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                    using (client.OpenRead("http://unity3d.com")) 
                        return true; 
            }
            catch
            {
                return false;
            }
        }

    }
}