using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class EventManager : Singleton<EventManager>
    {
        private List<Util.Device> _deviceList;

        //list of devices that is visible to the user
        private readonly List<Util.Device> _trackedDevices = new List<Util.Device>();
        private bool _isPlaced = false;

        void Start()
        {
        }

        public List<Util.Device> DeviceList
        {
            get { return _deviceList; }

            set
            {
                _deviceList = value;
                _deviceList.ForEach(d =>
                {
                    d.GetApiInformation();
                    d.IsPlaced = false;
                });
            }
        }


        public List<Util.Device> GetTrackedDevices()
        {
            return _trackedDevices;
        }

        public void SetPlacement(bool place)
        {
            _isPlaced = place;
        }

        public bool IsPlaced()
        {
            return _isPlaced;
        }

        public void ClearTrackedDevices()
        {
            _trackedDevices.Clear();
        }

        public void AddDevice(GameObject go, string label)
        {
            var device = _deviceList.Find(d => d.Name == label);
            if (device.IsPlaced) return;
            device.AddGameObject(go);
            _trackedDevices.Add(device);
        }

        public void RemoveDevice(GameObject go, string label)
        {
            var device = _deviceList.Find(d => d.Name == label);
            if (device == null) return;
            if (!device.IsPlaced)
            {
                device.SetRotation(false);
            }
            _trackedDevices.Remove(device);
        }

        // Methods for traget tracker ***

        public void NewObjectDetected(GameObject go, string label)
        {
            if (go.transform.Find("AccessRestricted(Clone)")) return;
            if (_isPlaced) return;
            AddDevice(go, label);
        }

        public void ObjectLost(GameObject go, string label)
        {
            if (_isPlaced) return;
            RemoveDevice(go, label);
        }

        // END ***

        // Methods for buttons ***

        public void ObjectClicked(GameObject go, string label)
        {
            if (!_isPlaced) return;
            var device = _deviceList.Find(d => d.Name == label);
            if (device == null) return;
            if (_trackedDevices.Contains(device))
            {
                RemoveDevice(go, label);
            }
            else
            {
                AddDevice(go, label);
            }
        }

        //END ***

        public bool IsDeviceAccessible(string label)
        {
            return DeviceList.Find(d => d.Name == label) != null;
        }

        public void SetPlaced(string name, bool status)
        {
            _deviceList.Find(d => d.Name == name).IsPlaced = status;
        }
    }
}