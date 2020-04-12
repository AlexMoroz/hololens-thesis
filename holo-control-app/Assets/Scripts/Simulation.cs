using System;
using Assets.Scripts.Managers;
using HoloToolkit.Unity;
using UnityEngine;

namespace Assets.Scripts
{
    public class Simulation : MonoBehaviour
    {
        private Animator _animator;
        private readonly float _right = 0.6f, _left = -0.8f;

        public GameObject MoveButon;

        private bool _isActive = false;

        void Start()
        {
            _animator = GetComponent<Animator>();
            //WorldAnchorManager.Instance.RemoveAllAnchors();
            WorldAnchorManager.Instance.AttachAnchor(gameObject, "Simulation");

            MoveButon.SetActive(false);
            ChangeStatus(false);
        }

        private void ChangeStatus(bool active)
        {
            for (int i = 0; i < transform.Find("/Objects/Simulation").childCount; i++)
            {
                var child = transform.Find("/Objects/Simulation").GetChild(i);
                if(child.name != "ControlPanel" && child.name != "Cube") child.gameObject.SetActive(active);
            }
        }


        private string speedValue = null;
        private string gearAValue = null;
        private string gearA1Value = null;

        void Update()
        {
            if (!_isActive) return;
            //set speed
            var pi = EventManager.Instance.DeviceList.Find(d => d.Name == "banana_pi");
            string value = null;
            if (pi.ApiInfoList.Count > 0)
            {
                value = pi.ApiInfoList.Find(api => api.Name == "Start").Value;
                pi.ApiInfoList.Find(api => api.Name == "Start").Value = null;
                if (value != null) SetSpeed(1f);

                value = pi.ApiInfoList.Find(api => api.Name == "Stop").Value;
                pi.ApiInfoList.Find(api => api.Name == "Stop").Value = null;
                if (value != null) SetSpeed(0f);

                value = pi.ApiInfoList.Find(api => api.Type == "slider").Value;
                Debug.Log("bananaVAL: " + value);
                if (value != speedValue)
                {
                    if (Math.Abs(_animator.GetFloat("Speed") - 0) > 0) SetSpeed((float) (Convert.ToDouble(value) - 70));
                    speedValue = value;
                }
            }


            //set A gear
            pi = EventManager.Instance.DeviceList.Find(d => d.Name == "raspberry_pi");
            if (pi.ApiInfoList.Count > 0)
            {
                value = pi.ApiInfoList.Find(api => api.Type == "slider").Value;
                Debug.Log("raspberryVAL: " + value);
                if (value != gearAValue)
                {
                    if (value != null) MoveGear("A", 100, 50, (float) Convert.ToDouble(value));
                    gearAValue = value;
                }
            }

            //set A1 gear
            pi = EventManager.Instance.DeviceList.Find(d => d.Name == "raspberry_pi2");
            if (pi.ApiInfoList.Count > 0)
            {
                value = pi.ApiInfoList.Find(api => api.Type == "output").Value;
                Debug.Log("ufoVAL: " + value);
                if (value != gearA1Value)
                {
                    if (value != null) MoveGear("A1", 70, 20, (float) Convert.ToDouble(value));
                    gearA1Value = value;
                }
            }
        }

        public void SetSpeed(float speed)
        {
            _animator.SetFloat("Speed", speed);
        }

        public void MoveGear(string name, float max, float min, float position)
        {
            var gear = gameObject.transform.Find("Gear " + name).Find("gear").gameObject;
            var result = CalculatePosition(max, min, position);
            _animator.SetBool("InPosition" + name, result < 0.1 && result > -0.1);
            gear.GetComponent<Move>().StartMovement(new Vector3(gear.transform.localPosition.x,
                    gear.transform.localPosition.y, result));
        }

        private float CalculatePosition(float max, float min, float position)
        {
            if (position > max) position = max;
            if (position < min) position = min;
            return (position - min) / (max - min) * (_right - _left) + _left; ;
        }


        public void ActivateSimulation()
        {
            //save acnhor
            WorldAnchorManager.Instance.RemoveAnchor("Simulation");
            WorldAnchorManager.Instance.AttachAnchor(gameObject, "Simulation");

            _isActive = true;
            MoveButon.SetActive(true);
            ChangeStatus(true);
        }

        public void Movement()
        {
            WorldAnchorManager.Instance.RemoveAnchor("Simulation");
            GetComponent<Placeable>().OnPlacementStart();
        }
    }
}
