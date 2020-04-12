using System;
using System.Collections.Generic;
using Assets.Scripts.Managers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    //Helper Classes
    public class Util {

        public class Device
        {
            public Device(string id, string name, string path, string ip, int port)
            {
                Id = id;
                this.Name = name;
                this.Path = path;
                this.Ip = ip;
                this.Port = port;
                Uri = String.Format("http://{0}:{1}/", Ip, Port);
            }

            [JsonProperty("_id")] public string Id { get; private set; }

            [JsonProperty("name")] public string Name { get; private set; }

            [JsonProperty("path")] public string Path { get; private set; }

            [JsonProperty("ip")] public string Ip { get; private set; }

            [JsonProperty("port")] public int Port { get; private set; }

            public string Uri { get; private set; }

            public List<ApiInfo> ApiInfoList { get; private set; }

            public GameObject GObject { get; private set; }

            public void GetApiInformation()
            {
                isAccessible = true;
                if (!isAccessible) return;
                isUpdating = true;
                ApiInfoList = new List<ApiInfo>();
                ServerManager.Instance.GetDeviceApiDescription(this, (list, status) =>
                {
                    ApiInfoList = list ?? new List<ApiInfo>();
                    isUpdating = status;
                });

            }

            public void AddGameObject(GameObject go)
            {
                GObject = go;
            }

            public void SetRotation(bool rotation)
            {
                if(GObject == null) return;
                GObject.GetComponentInChildren<Rotate>().enabled = rotation;
            } 

            public bool isAccessible { get; private set; }
            public bool isAvailable { get; private set; }
            public bool isUpdating { get; private set; }
            public bool IsPlaced { get; set; }
        }

        public class ApiInfo
        {
            public ApiInfo(string path, string method, string type, string code)
            {
                this.Path =  path;
                this.Method = method;
                this.Type = type;
            }

            [JsonProperty("name")]
            public string Name { get; private set; }

            [JsonProperty("path")]
            public string Path { get; private set; }

            [JsonProperty("method")]
            public string Method { get; private set; }

            [JsonProperty("type")]
            public string Type { get; private set; }

            public string Uri { get; set; }

            public string Value { get; set; }

            public delegate void AnswerDelegate(bool status, string response = null);
            public event AnswerDelegate OnAnswerReceived;

            public void GenerateUri(string deviceUri)
            {
                Uri = String.Format("{0}{1}", deviceUri, Path);
            }

            public void ClearAllHandlers()
            {
                if(OnAnswerReceived == null) return;
                foreach (var d in OnAnswerReceived.GetInvocationList())
                {
                    OnAnswerReceived -= (AnswerDelegate) d;
                }
            }

            public void PerformAction(string data = null)
            {
                switch (Type)
                {
                    case "button":
                        //if button perform get request
                        ServerManager.Instance.ApiGetRequest(Uri, result =>
                        {
                            if (OnAnswerReceived != null)
                            {
                                OnAnswerReceived(Convert.ToBoolean(result["success"]));
                            }
                        });
                        break;
                    case "input":
                        //if input post request
                        ServerManager.Instance.ApiPostRequest(Uri, data);
                        break;
                    case "output":
                        //if output get request
                        ServerManager.Instance.ApiGetRequest(Uri, result =>
                        {
                            if (OnAnswerReceived != null)
                            {
                                OnAnswerReceived(true, result["data"]);
                            }
                        });
                        break;
                    case "slider":
                        //if slider post request
                        ServerManager.Instance.ApiPostRequest(Uri, data);
                        break;
                }
            }
        }

    }
}
