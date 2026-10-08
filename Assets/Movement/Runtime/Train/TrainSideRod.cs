using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainSideRod : MonoBehaviour
    {
        [SerializeField] private TrainWheel first;
        [SerializeField] private TrainWheel last;
        [SerializeField] private float side;
        public void Configure(TrainWheel a, TrainWheel b, float outside) { first = a; last = b; side = outside; }
        private void LateUpdate()
        {
            if (first == null || last == null) return;
            Vector3 offset = Quaternion.AngleAxis(first.Angle, Vector3.right) * Vector3.up * 0.25f + Vector3.right * side;
            Vector3 a = first.transform.localPosition + offset, b = last.transform.localPosition + offset;
            transform.localPosition = (a + b) * 0.5f;
            transform.localRotation = Quaternion.LookRotation(b - a, Vector3.up);
            transform.localScale = new Vector3(0.07f, 0.09f, Vector3.Distance(a, b));
        }
    }
}
