using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Managers
{
    public class ServerManager : Singleton<ServerManager>
    {
        private String _serverUrl = "http://10.42.0.1:8080/";

        public bool IsOnline { get; private set; }

        public bool IsVuforiaFileLoaded
        {
            get { return _isVuforiaFileLoaded >= 2; }
            private set
            {
                if (value)
                {
                    _isVuforiaFileLoaded += 1;
                }
            }
        }

        private int _isVuforiaFileLoaded;

        // Use this for initialization
        void Start()
        {
            CheckIfServerIsOnline();
        }

        private void CheckIfServerIsOnline()
        {
            UnityWebRequest.Get(_serverUrl).SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (!isError(request))
                {
                    if (request.responseCode == 200)
                    {
                        IsOnline = true;
                        Authentication.Instance.OnAuthentication += (status, message) =>
                        {
                            if (status)
                            {
                                GetAllDevices();
                                DownloadVuforiaFile("dataset", "xml");
                                DownloadVuforiaFile("dataset", "dat");
                                return;
                            }
                            Debug.Log(message);
                        };
                        Authentication.Instance.StartAuthentication(_serverUrl);
                        
                    }
                    else
                    {
                        IsOnline = false;
                    }
                }
            };
        }

        //fix for https://issuetracker.unity3d.com/issues/wsa-isnetworkerror-always-return-true-when-running-an-uwp-app-on-86x-architecture
        private bool isError(UnityWebRequest request)
        {
            if (String.IsNullOrEmpty(request.error)) return false;
            Debug.Log("Error: " + request.error + " : " + request.url);
            return true;
        }

        private UnityWebRequest Get(string url)
        {
            var uwr = UnityWebRequest.Get(url);
            return AddToken(uwr);
        }

        private UnityWebRequest Post(string url, Dictionary<string, string> parameters)
        {
            var uwr = UnityWebRequest.Post(url, parameters);
            return AddToken(uwr);
        }

        private UnityWebRequest AddToken(UnityWebRequest uwr)
        {
            uwr.SetRequestHeader("x-access-token", Authentication.Instance.TokenId);
            return uwr;
        }

        public void GetAllDevices()
        {
            Get(_serverUrl + "api/devices").SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (!isError(request))
                {
                    Debug.Log(request.downloadHandler.text);
                    EventManager.Instance.DeviceList =
                        JsonConvert.DeserializeObject<List<Util.Device>>(request.downloadHandler.text).FindAll(d => d.Ip != "dummy"); 
                    //TODO: delete dummy ignore after performance tests
                }
            };
        }


        public void GetDeviceApiDescription(Util.Device device, Action<List<Util.ApiInfo>, bool> callback)
        {
            var url = string.Format("{0}{1}", device.Uri, device.Path);
            Get(url).SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (!isError(request))
                {
                    var list = JsonConvert.DeserializeObject<List<Util.ApiInfo>>(request.downloadHandler.text);
                    list.ForEach(r => r.GenerateUri(device.Uri));
                    callback(list, false);
                }
                else
                {
                    callback(null, false);
                }
            };
        }

        public void ApiGetRequest(string url, Action<Dictionary<string, string>> callback)
        {
            Get(url).SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (!isError(request))
                {
                    var statusObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    callback(statusObject);
                }
            };
        }

        //public void ApiPostRequest(string uri, string data)
        //{
        //    var answerData = new JObject
        //    {
        //        { "data", data }
        //    };
        //    var uwr = UnityWebRequest.Post(uri, answerData.ToString());
        //    uwr.SetRequestHeader("Content-Type", "application/json");
        //    uwr.SendWebRequest().completed += operation =>
        //    {
        //        var request = ((UnityWebRequestAsyncOperation)operation).webRequest;
        //        if (!isError(request))
        //        {
        //            Debug.Log(request.downloadHandler.text);
        //        }
        //    };
        //}


        public void ApiPostRequest(string uri, string data)
        {
            var json = new JObject
            {
                { "data", data }
            };
            var uwr = new UnityWebRequest(uri, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json.ToString());
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation)operation).webRequest;
                if (!isError(request))
                {
                    Debug.Log(request.downloadHandler.text);
                }
            };
        }


        public void DownloadVuforiaFile(string name,  string type)
        {
            var uwr = new UnityWebRequest(String.Format("{0}api/dataset/{1}", _serverUrl, type), UnityWebRequest.kHttpVerbGET);
            string path = System.IO.Path.Combine(Application.persistentDataPath, name + "." + type);
            uwr.downloadHandler = new DownloadHandlerFile(path);
            uwr = AddToken(uwr);
            uwr.SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (!isError(request))
                {
                    IsVuforiaFileLoaded = true;
                }
            };
        }

    }
}