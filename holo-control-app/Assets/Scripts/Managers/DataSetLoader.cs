using System.Collections.Generic;
using Assets.Scripts.EventHandlers;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

namespace Assets.Scripts.Managers
{
    public class DataSetLoader : MonoBehaviour
    {

        public GameObject SuccessGameObject;
        public GameObject ErrorGameObject;
        public GameObject ObjectsGameObject;

        void Update()
        {
            if (ServerManager.Instance.IsVuforiaFileLoaded && VuforiaARController.Instance.HasStarted && LoadDataSet(System.IO.Path.Combine(Application.persistentDataPath, "dataset.xml")))
            {
                gameObject.SetActive(false);
            }
        }

        // Load and activate a data set at the given path.
        private bool LoadDataSet(string dataSetPath)
        {
            ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

            DataSet dataSet = objectTracker.CreateDataSet();

            if (dataSet.Load(dataSetPath, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
            {

                objectTracker.Stop();  // stop tracker so that we can add new dataset

                if (!objectTracker.ActivateDataSet(dataSet))
                {
                    // Note: ImageTracker cannot have more than 100 total targets activated
                    Debug.Log("<color=yellow>Failed to Activate DataSet: " + dataSet + "</color>");
                }

                if (!objectTracker.Start())
                {
                    Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
                }

                int counter = 0;

                IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
                foreach (TrackableBehaviour tb in tbs)
                {
                    if (tb.name == "New Game Object")
                    {
                        tb.gameObject.transform.parent = ObjectsGameObject.transform;
                        // change generic name to include trackable name
                        tb.gameObject.name = ++counter + ":DynamicImageTarget-" + tb.TrackableName;

                        // add additional script components for trackable
                        tb.gameObject.AddComponent<TrackableEventHandler>();
                        tb.gameObject.AddComponent<TurnOffBehaviour>();

                        if (SuccessGameObject != null && ErrorGameObject != null)
                        {
                            // instantiate augmentation object and parent to trackable
                            GameObject augmentation =
                                EventManager.Instance.IsDeviceAccessible(tb.TrackableName)
                                    ? Instantiate(SuccessGameObject)
                                    : Instantiate(ErrorGameObject);
                            augmentation.transform.parent = tb.gameObject.transform;
                            augmentation.transform.localPosition = new Vector3(0f, 0f, 0f);
                            augmentation.transform.localRotation = Quaternion.identity;
                            augmentation.transform.Find("Canvas").gameObject.SetActive(false);
                            //add button event
                            if (EventManager.Instance.IsDeviceAccessible(tb.TrackableName))
                            {
                                augmentation.transform.Find("HiddenName").gameObject.GetComponent<Text>().text =
                                    tb.TrackableName;
                                augmentation.transform.Find("Canvas").gameObject.GetComponentInChildren<Button>().onClick
                                    .AddListener(
                                        delegate
                                        {
                                            var canvas = transform.Find("/Objects")
                                                .Find(tb.TrackableName + "_canvas");
                                            if (canvas != null)
                                            {
                                                UIManager.Instance.PanelBackClick(canvas.gameObject);
                                            }
                                            EventManager.Instance.ObjectClicked(augmentation, augmentation.transform.Find("HiddenName").gameObject.GetComponent<Text>().text);
                                        });
                            }
                            augmentation.SetActive(true);
                        }
                        else
                        {
                            Debug.Log("<color=yellow>Warning: No augmentation object specified for: " + tb.TrackableName + "</color>");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("<color=yellow>Failed to load dataset: '" + dataSetPath + "'</color>");
            }

            return true;
        }
    }
}
