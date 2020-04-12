using HoloToolkit.Unity.InputModule;
using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Rendering;

namespace Assets.Scripts
{
    public class Fun : MonoBehaviour, IInputHandler
    {

        // Use this for initialization
        void Start () {
		
        }
	
        // Update is called once per frame
        void Update () {
		
        }

        public void OnInputDown(InputEventData eventData)
        {
            //do nothing
        }

        IEnumerator DoFun()
        {
            gameObject.AddComponent<Rigidbody>();
            yield return new WaitForSeconds(2);
            gameObject.SetActive(false);
        }

        public void OnInputUp(InputEventData eventData)
        {
            StartCoroutine(DoFun());
        }
    }
}
