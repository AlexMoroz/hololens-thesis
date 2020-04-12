using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class Authentication : Singleton<Authentication>
    {
        public Dropdown LoginDropdown;
        public InputField PasswordField;
        public Button Button;

        public bool IsAuthenticated { get; private set; }
        public string TokenId { get; private set; }

        public delegate void AuthenticationDelegate(bool status, string message = null);

        public event AuthenticationDelegate OnAuthentication;

        void Start()
        {
            gameObject.SetActive(false);

        }

        public void StartAuthentication(string uri)
        {
            //FillDropdownList(uri);
            //Button.onClick.AddListener(() =>
            //{
            //    if (InputHasError()) return;
            //    Authenticate(uri + "api/authenticate", LoginDropdown.options[LoginDropdown.value].text, PasswordField.text);
            //});
            //TODO: delete after test
            Authenticate(uri + "api/authenticate", "user", "password");
        }

        private void Authenticate(string uri, string user, string password)
        {
            if (OnAuthentication == null) return;

            var postFields = new Dictionary<string, string>()
            {
                {"name", user},
                {"password", password}
            };

            UnityWebRequest.Post(uri, postFields).SendWebRequest().completed += operation =>
            {
                var request = ((UnityWebRequestAsyncOperation) operation).webRequest;
                if (String.IsNullOrEmpty(request.error))
                {
                    Dictionary<string, string> jObject =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    if (jObject.Count < 0 || jObject["success"] == "false" || !jObject.ContainsKey("token"))
                    {
                        PasswordField.text = "";
                        InputHasError();
                        OnAuthentication(false,
                            jObject.ContainsKey("message") ? jObject["message"] : "Response is empty");
                        return;
                    }

                    TokenId = jObject["token"];
                    Debug.Log("<color=green>Tocken received!</color>");
                    IsAuthenticated = true;
                    OnAuthentication(IsAuthenticated);
                    //TODO: add clear
                    gameObject.SetActive(false);
                    return;
                }

                OnAuthentication(false, request.error);
            };
        }

        private void FillDropdownList(string uri)
        {
            UnityWebRequest.Get(uri + "api/users").SendWebRequest().completed += op =>
            {
                var request = ((UnityWebRequestAsyncOperation) op).webRequest;
                if (String.IsNullOrEmpty(request.error))
                {
                    LoginDropdown.ClearOptions();
                    var mDropOptions =
                        JsonConvert.DeserializeObject<List<string>>(request.downloadHandler.text);
                    mDropOptions.Sort();
                    LoginDropdown.AddOptions(mDropOptions);
                    gameObject.SetActive(true);
                }
            };
        }

        private bool InputHasError()
        {
            if (String.IsNullOrEmpty(PasswordField.text))
            {
                PasswordField.placeholder.color = Color.red;
            }
            else
            {
                return false;
            }

            gameObject.transform.Find("Canvas").GetComponent<Animator>().SetTrigger("Run");
            return true;
        }
    }
}