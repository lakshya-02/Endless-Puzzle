using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class BoundaryDetector : MonoBehaviour
    {
        public float FailY { get; private set; }

        public void Initialize(float failY)
        {
            FailY = failY;
        }

        public bool HasCrossedFailBoundary(float yPosition)
        {
            return yPosition <= FailY;
        }
    }
}
