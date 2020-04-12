using UnityEngine;

namespace Assets.Scripts
{
    public class Move : MonoBehaviour
    {
        private Vector3 _moveTo;

        void Start()
        {
        }
        void Update()
        {
            float dist = Mathf.Abs((transform.localPosition - _moveTo).magnitude);
            transform.localPosition = Vector3.Lerp(transform.localPosition, _moveTo, 0.1f / dist);
            if (transform.localPosition == _moveTo) enabled = false;
        }

        public void StartMovement(Vector3 end)
        {
            _moveTo = end;
            enabled = true;
        }
    }
}
