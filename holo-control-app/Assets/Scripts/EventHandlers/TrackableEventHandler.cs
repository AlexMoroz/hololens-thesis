using UnityEngine;
using Vuforia;
using Assets.Scripts.Managers;
using UnityEngine.UI;

namespace Assets.Scripts.EventHandlers
{
    public class TrackableEventHandler : MonoBehaviour, ITrackableEventHandler {


        protected TrackableBehaviour mTrackableBehaviour;


        protected virtual void Start()
        {
            mTrackableBehaviour = GetComponent<ImageTargetBehaviour>();
            if (mTrackableBehaviour)
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }

        public void OnTrackableStateChanged(
            TrackableBehaviour.Status previousStatus,
            TrackableBehaviour.Status newStatus)
        {
            var go = gameObject.transform.GetChild(0);

            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
                if (go != null)
                {
                    go.Find("Canvas").gameObject.SetActive(true);
                }
                EventManager.Instance.NewObjectDetected(gameObject, mTrackableBehaviour.TrackableName);
            }
            else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                     newStatus == TrackableBehaviour.Status.NOT_FOUND)
            {
                Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
                if (go != null)
                {
                    go.Find("Canvas").gameObject.SetActive(false);
                }
                EventManager.Instance.ObjectLost(gameObject, mTrackableBehaviour.TrackableName);
            }

            else
            {
                // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
                // Vuforia is starting, but tracking has not been lost or found yet
                // Call OnTrackingLost() to hide the augmentations
                if (go != null)
                {
                    go.Find("Canvas").gameObject.SetActive(false);
                }
                EventManager.Instance.ObjectLost(gameObject, mTrackableBehaviour.TrackableName);
            }
        }
    }
}
