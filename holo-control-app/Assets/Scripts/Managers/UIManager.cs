using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.UI.Keyboard;
using HoloToolkit.Unity;
using HoloToolkit.Unity.UX;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class UIManager : Singleton<UIManager>
    {
        public GameObject ButtonPrefab;
        public GameObject InputPrefab;
        public GameObject OutputPrefab;
        public GameObject SliderPrefab;


        public GameObject UserInterface;
        public GameObject Content;
        public GameObject DevicePanel;
        public GameObject DeviceCanvas;
        public GameObject Objects;

        public Simulation Gears;

        public GameObject NoResponseGameObject;

        private List<GameObject> _panelsList;

        private int _deviceNum = 1000;

        void Start()
        {
            _panelsList = new List<GameObject>();
        }


        void Update()
        {
            var devices = EventManager.Instance.GetTrackedDevices();
            
            if (devices.Count < 1 && !EventManager.Instance.IsPlaced())
            {
                UserInterface.SetActive(false);
            }

            if (devices.Count > 0 || EventManager.Instance.IsPlaced())
            {
                UserInterface.SetActive(true);
            }

            if (devices.Count == _deviceNum) return;

            float height = 0;
            _panelsList.FindAll(p => devices.Find(d => d.Name + "_panel" == p.name) == null).ForEach(p => p.SetActive(false));
            foreach (var device in devices)
            {
                var name = device.Name + "_panel";
                var panel = _panelsList.Find(o => o.name == name);
                if (panel != null)
                {
                    if (panel.GetComponent<Image>().color == Color.red && !device.isUpdating  && !device.IsPlaced)
                    {
                        _panelsList.Remove(panel);
                        Destroy(panel);
                        AddPanel(device, name);
                    }
                    else panel.SetActive(true);
                }
                else
                {
                    panel = AddPanel(device, name);
                }
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, - 5 - height);
                height += 5 + panel.GetComponent<RectTransform>().sizeDelta.y;

                device.SetRotation(true);
            }
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(Content.GetComponent<RectTransform>().sizeDelta.x, height + 5);
            if (height < 300)
            {
                UserInterface.GetComponent<RectTransform>().sizeDelta = new Vector2(UserInterface.GetComponent<RectTransform>().sizeDelta.x, height + 5 + 25);
                UserInterface.GetComponent<BoxCollider>().size = new Vector2(UserInterface.GetComponent<RectTransform>().sizeDelta.x, height + 5 + 25);
            }

            _deviceNum = devices.Count;
        }

        private GameObject CreateExpernalDeviceCanvas(Util.Device device, GameObject panel)
        {
            var canvas = Instantiate(DeviceCanvas);
            canvas.name = device.Name + "_canvas";
            canvas.transform.Find("HiddenName").gameObject.GetComponent<Text>().text = device.Name;
            var content = canvas.transform.Find("Content");
            content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, panel.GetComponent<RectTransform>().sizeDelta.y + 10);
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-5);
            panel.transform.Find("MoveBack").gameObject.GetComponent<Button>().interactable = true;
            panel.transform.Find("MoveBack").gameObject.GetComponent<Button>().onClick.AddListener(delegate
            {
                device.SetRotation(false);
                panel.SetActive(false);
                panel.transform.SetParent(Content.transform, false);
                _panelsList.Add(panel);
                panel.transform.Find("MoveBack").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                panel.transform.Find("MoveBack").gameObject.GetComponent<Button>().interactable = false;
                device.IsPlaced = false;
                PanelBackClick(canvas);
            });
            panel.transform.Find("TapToPlaceButton").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            panel.transform.Find("TapToPlaceButton").gameObject.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (!device.IsPlaced)
                {
                    EventManager.Instance.RemoveDevice(device.GObject, device.Name);
                    device.SetRotation(true);
                    device.IsPlaced = true;
                    _panelsList.Remove(panel);
                    var newCanvas = CreateExpernalDeviceCanvas(device, panel);
                    PanelPlaceClick(newCanvas);
                }else PanelPlaceClick(canvas);
            });
            panel.transform.SetParent(content.transform, false);
            //resize content
            var height = panel.GetComponent<RectTransform>().sizeDelta.y;
            var size = canvas.GetComponent<RectTransform>().sizeDelta;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, height + 10);
            canvas.GetComponent<BoxCollider>().size = new Vector2(size.x, height);
            canvas.transform.parent = transform.Find("/Objects");
            canvas.SetActive(true);
            return canvas;
        }

        private GameObject AddPanel(Util.Device device, string name)
        {
            var panel = CreatePanel(device, name);
            panel.transform.Find("TapToPlaceButton").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            panel.transform.Find("TapToPlaceButton").gameObject.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (!device.IsPlaced)
                {
                    EventManager.Instance.RemoveDevice(device.GObject, device.Name);
                    device.SetRotation(true);
                    device.IsPlaced = true;
                    _panelsList.Remove(panel);
                    var canvas = CreateExpernalDeviceCanvas(device, panel);
                    PanelPlaceClick(canvas);
                }
            });
            panel.transform.Find("MoveBack").gameObject.GetComponent<Button>().interactable = false;
            panel.transform.SetParent(Content.transform, false);
            _panelsList.Add(panel);
            return panel;
        }

        private GameObject CreatePanel(Util.Device device, string name)
        {
            var panel = Instantiate(DevicePanel);
            panel.name = name;
            panel.transform.Find("Text").GetComponent<Text>().text = device.Name;
            float width = panel.GetComponent<RectTransform>().sizeDelta.x;
            float height = 0;
            var buttons = panel.transform.Find("Buttons").gameObject;
            if (!device.isUpdating)
            {
                foreach (var apiInfo in device.ApiInfoList)
                {
                    var actor = AddActor(apiInfo);
                    actor.transform.SetParent(buttons.transform, false);
                    actor.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -5 - height);
                    height += 5 + actor.GetComponent<RectTransform>().sizeDelta.y;
                }
            }

            height += 5;
            buttons.GetComponent<RectTransform>().sizeDelta = new Vector2(buttons.GetComponent<RectTransform>().sizeDelta.x, height);

            if (device.ApiInfoList.Count == 0)
            {
                panel.GetComponent<Image>().color = Color.red;
                panel.transform.Find("TapToPlaceButton").GetComponent<Button>().interactable = false;
                var noresponse = Instantiate(NoResponseGameObject);
                height += 5 + noresponse.GetComponent<RectTransform>().sizeDelta.y;
                noresponse.name = NoResponseGameObject.name;
                noresponse.transform.SetParent(panel.transform.Find("Buttons"), false);
                if(!device.isUpdating) device.GetApiInformation();
            }

            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height + 5 + 25);

            return panel;
        }

        private GameObject AddActor(Util.ApiInfo apiInfo)
        {
            switch (apiInfo.Type)
            {
                case "button":
                    var button = Instantiate(ButtonPrefab);
                    button.GetComponentInChildren<Text>().text = apiInfo.Name;
                    button.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        apiInfo.PerformAction();
                        apiInfo.Value = "click";
                    });
                    return button;
                case "input":
                    var input = Instantiate(InputPrefab);
                    input.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate
                    {
                        var textInput = input.GetComponentInChildren<KeyboardInputField>().textComponent.text;
                        apiInfo.Value = textInput;
                        input.transform.Find("PasswordField").Find("Text").gameObject.GetComponent<Text>().text = "";
                        apiInfo.PerformAction(textInput);
                    });
                    return input;
                case "output":
                    var output = Instantiate(OutputPrefab);
                    StartCoroutine(GetText(output, apiInfo));
                    return output;
                case "slider":
                    var slider = Instantiate(SliderPrefab);
                    slider.transform.Find("Name").GetComponent<Text>().text = apiInfo.Name;
                    slider.GetComponent<Slider>().onValueChanged.AddListener(delegate
                    {
                        apiInfo.Value = ((int)slider.GetComponent<Slider>().value).ToString();
                        apiInfo.PerformAction(apiInfo.Value);
                    });
                    return slider;
                default:
                    Debug.LogError("Cannot find type for " + apiInfo.Type);
                    break;
            }

            return null;
        }

        IEnumerator GetText(GameObject go, Util.ApiInfo apiInfo)
        {
            apiInfo.ClearAllHandlers();
            apiInfo.OnAnswerReceived += (status, response) =>
            {
                apiInfo.Value = response;
                if (go != null) go.GetComponent<InputField>().text = "Current temperature: " + response + " C";
            };

            while (true)
            {
                yield return new WaitForSeconds(2);
                if(go == null || !go.activeInHierarchy) continue;
                apiInfo.PerformAction();
            }
        }

        public void PanelPlaceClick(GameObject go)
        {
            var panelName = go.transform.Find("HiddenName").gameObject.GetComponent<Text>().text;
            if (panelName == "Main")
            {
                //main
                EventManager.Instance.SetPlacement(true);
                go.GetComponent<Billboard>().enabled = false;
                go.GetComponent<Tagalong>().enabled = false;
                go.GetComponent<Interpolator>().enabled = false;
                go.GetComponent<FixedAngularSize>().enabled = false;
            }
            //go.GetComponent<Placeable>().enabled = true;
            go.GetComponent<Placeable>().OnPlacementStart();

        }

        public void PanelBackClick(GameObject go)
        {
            var panelName = go.transform.Find("HiddenName").gameObject.GetComponent<Text>().text;
            if (panelName == "Main")
            {
                //main
                EventManager.Instance.SetPlacement(false);
                go.GetComponent<Billboard>().enabled = true;
                go.GetComponent<Tagalong>().enabled = true;
                go.GetComponent<Interpolator>().enabled = true;
                go.GetComponent<FixedAngularSize>().enabled = true;
                EventManager.Instance.ClearTrackedDevices();
            }
            else
            {
                EventManager.Instance.DeviceList.Find(d => d.Name + "_canvas" == go.name).SetRotation(false);
                Destroy(go);
            }

        }
    }
}