using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Unity.Services.RemoteConfig
{
    public class RCUnityWebRequest : IRCUnityWebRequest
    {

        UnityWebRequest _unityWebRequest { get; set; }

        /// <summary>
        /// Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)
        /// </summary>
        public bool isHttpError => _unityWebRequest.isHttpError;

        /// <summary>
        /// Returns a floating-point value between 0.0 and 1.0, indicating the progress of downloading body data from the server. (Read Only)
        /// </summary>
        public float downloadProgress => _unityWebRequest.downloadProgress;

        /// <summary>
        /// Returns the number of bytes of body data the system has uploaded to the remote server. (Read Only)
        /// </summary>
        public ulong uploadedBytes => _unityWebRequest.uploadedBytes;

        /// <summary>
        /// Returns the number of bytes of body data the system has downloaded from the remote server. (Read Only)
        /// </summary>
        public ulong downloadedBytes => _unityWebRequest.downloadedBytes;

        /// <summary>
        /// Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error.
        /// </summary>
        public int redirectLimit
        {
            get => _unityWebRequest.redirectLimit;
            set => redirectLimit = value;
        }

        /// <summary>
        /// Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
        /// </summary>
        public bool isDone => _unityWebRequest.isDone;

        /// <summary>
        /// Returns true after this UnityWebRequest encounters a system error. (Read Only)
        /// </summary>
        public bool isNetworkError => _unityWebRequest.isNetworkError;

        ///<summary>
        /// Defines the target URL for the UnityWebRequest to communicate with.
        /// </summary>
        public string url
        {
            get => _unityWebRequest.url;
            set => _unityWebRequest.url = value;
        }

        /// <summary>
        /// Defines the target URI for the UnityWebRequest to communicate with.
        /// </summary>
        public Uri uri
        {
            get => _unityWebRequest.uri;
            set => _unityWebRequest.uri = value;
        }

        /// <summary>
        /// The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
        /// </summary>
        public long responseCode => _unityWebRequest.responseCode;

        public float uploadProgress => _unityWebRequest.uploadProgress;
        public bool isModifiable => _unityWebRequest.isModifiable;

        /// <summary>
        /// If true, any UploadHandler attached to this UnityWebRequest will have UploadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
        /// </summary>
        public bool disposeUploadHandlerOnDispose
        {
            get => _unityWebRequest.disposeUploadHandlerOnDispose;
            set => _unityWebRequest.disposeUploadHandlerOnDispose = value;
        }

        /// <summary>
        /// Defines the HTTP verb used by this UnityWebRequest, such as GET or POST.
        /// </summary>
        public string method
        {
            get => _unityWebRequest.method;
            set => _unityWebRequest.method = value;
        }

        /// <summary>
        /// A human-readable string describing any system errors encountered by this UnityWebRequest object while handling HTTP requests or responses. (Read Only)
        /// </summary>
        public string error => _unityWebRequest.error;

        /// <summary>
        /// Determines whether this UnityWebRequest will include Expect: 100-Continue in its outgoing request headers. (Default: true).
        /// </summary>
        public bool useHttpContinue
        {
            get => _unityWebRequest.useHttpContinue;
            set => _unityWebRequest.useHttpContinue = value;
        }

        /// <summary>
        /// UnityWebRequest object handles the flow of HTTP communication with web servers
        /// <summary>
        public UnityWebRequest unityWebRequest
        {
            get { return _unityWebRequest; }
            set { _unityWebRequest = value; }
        }

        /// <summary>
        /// If true, any CertificateHandler attached to this UnityWebRequest will have CertificateHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
        /// </summary>
        public bool disposeCertificateHandlerOnDispose
        {
            get => _unityWebRequest.disposeCertificateHandlerOnDispose;
            set => _unityWebRequest.disposeCertificateHandlerOnDispose = value;
        }

        /// <summary>
        /// If true, any DownloadHandler attached to this UnityWebRequest will have DownloadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.
        /// </summary>
        public bool disposeDownloadHandlerOnDispose
        {
            get => _unityWebRequest.disposeDownloadHandlerOnDispose;
            set => _unityWebRequest.disposeDownloadHandlerOnDispose = value;
        }

        /// <summary>
        /// Holds a reference to a DownloadHandler object, which manages body data received from the remote server by this UnityWebRequest.
        /// </summary>
        public DownloadHandler downloadHandler
        {
            get { return _unityWebRequest.downloadHandler; }
            set { _unityWebRequest.downloadHandler = value; }
        }

        /// <summary>
        /// Holds a reference to a CertificateHandler object, which manages certificate validation for this UnityWebRequest.
        /// </summary>
        public CertificateHandler certificateHandler
        {
            get => _unityWebRequest.certificateHandler;
            set => _unityWebRequest.certificateHandler = value;
        }

        /// <summary>
        /// Sets UnityWebRequest to attempt to abort after the number of seconds in timeout have passed.
        /// </summary>
        public int timeout
        {
            get => _unityWebRequest.timeout;
            set => _unityWebRequest.timeout = value;
        }

        /// <summary>
        /// Signals that this UnityWebRequest is no longer being used, and should clean up any resources it is using.
        /// </summary>
        public void Dispose()
        {
            _unityWebRequest.Dispose();
        }

        /// <summary>
        /// Holds a reference to the UploadHandler object which manages body data to be uploaded to the remote server.
        /// </summary>
        public UploadHandler uploadHandler
        {
            get { return _unityWebRequest.uploadHandler; }
            set { _unityWebRequest.uploadHandler = value; }
        }

        /// <summary>
        /// Begin communicating with the remote server.
        /// </summary>
        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return _unityWebRequest.SendWebRequest();
        }

        /// <summary>
        /// If in progress, halts the UnityWebRequest as soon as possible.
        /// </summary>
        public void Abort()
        {
            _unityWebRequest.Abort();
        }

        /// <summary>
        /// Retrieves the value of a custom request header.
        /// </summary>
        /// <param name="name">Name of the custom request header. Case-insensitive.</param>
        /// <returns>
        /// The value of the custom request header. If no custom header with a matching name has been set, returns an empty string.
        /// </returns>
        public string GetRequestHeader(string name)
        {
            return _unityWebRequest.GetRequestHeader(name);
        }

        /// <summary>
        /// Set a HTTP request header to a custom value.
        /// </summary>
        /// <param name="name">The key of the header to be set. Case-sensitive.</param>
        /// <param name="value">The header's intended value.</param>
        public void SetRequestHeader(string name, string value)
        {
            _unityWebRequest.SetRequestHeader(name, value);
        }

        /// <summary>
        /// Retrieves the value of a response header from the latest HTTP response received.
        /// </summary>
        /// <param name="name">The name of the HTTP header to retrieve. Case-insensitive.</param>
        /// <returns>
        /// The value of the HTTP header from the latest HTTP response. If no header with a matching name has been received, or no responses have been received, returns null.
        /// </returns>
        public string GetResponseHeader(string name)
        {
            return _unityWebRequest.GetResponseHeader(name);
        }

        /// <summary>
        /// Retrieves a dictionary containing all the response headers received by this UnityWebRequest in the latest HTTP response.
        /// </summary>
        /// <returns>
        /// A dictionary containing all the response headers received in the latest HTTP response. If no responses have been received, returns null.
        /// </returns>
        public Dictionary<string, string> GetResponseHeaders()
        {
            return _unityWebRequest.GetResponseHeaders();
        }
    }
}