// WARNING: Auto generated code. Modifications will be lost!
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Unity.Services.RemoteConfig.WebRequest
{
    /// <summary>
    /// Web Request Extensions
    /// </summary>
    static class WebRequestExtensions
    {
        /// <summary>
        /// Function for awaiting on an async operation.
        /// </summary>
        /// <param name="asyncOp">The asynchronous operation.</param>
        /// <returns>The TaskAwaiter for the async operation.</returns>
        public static TaskAwaiter<WebRequestResponse> GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<WebRequestResponse>();

            if (asyncOp.isDone)
            {
                OnCompleted(tcs, asyncOp);
            }
            else
            {
                asyncOp.completed += asyncOperation =>
                {
                    OnCompleted(tcs, (UnityWebRequestAsyncOperation)asyncOperation);
                };
            }

            return tcs.Task.GetAwaiter();
        }

        /// <summary>
        /// Function for awaiting on an async operation.
        /// </summary>
        /// <param name="asyncOp">The asynchronous operation.</param>
        /// <returns>The TaskAwaiter for the async operation.</returns>
        public static async Task<T> SendWebRequest<T>(this UnityWebRequest webRequest, JsonSerializerSettings settings = null)
        {
            var response = await webRequest.SendWebRequest();
            return DeserializeWebRequest<T>(response);
        }

        internal static T DeserializeWebRequest<T>(WebRequestResponse response, JsonSerializerSettings settings = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.Data, settings);
            }
            catch (Exception e)
            {
                throw new WebRequestException(response, "Deserialization failed.", e);
            }
        }

        static void OnCompleted(TaskCompletionSource<WebRequestResponse> tcs, UnityWebRequestAsyncOperation asyncOperation)
        {
            var response = CreateWebRequestResponse(asyncOperation);

            if (response.IsHttpError || response.IsNetworkError)
            {
                tcs.SetException(new WebRequestException(response));
            }
            else
            {
                tcs.SetResult(response);
            }
        }

        static WebRequestResponse CreateWebRequestResponse(UnityWebRequestAsyncOperation unityResponse)
        {
            var response = unityResponse.webRequest;

            var result = new WebRequestResponse(
                response.GetResponseHeaders(),
                response.responseCode,
#if UNITY_2020_1_OR_NEWER
                response.result == UnityWebRequest.Result.ProtocolError,
                response.result == UnityWebRequest.Result.ConnectionError,
#else
                response.isHttpError,
                response.isNetworkError,
#endif
                response.downloadHandler?.text,
                response.error);

            return result;
        }
    }
}
