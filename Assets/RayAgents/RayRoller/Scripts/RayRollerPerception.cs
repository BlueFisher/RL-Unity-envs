using System.Collections.Generic;
using UnityEngine;

namespace RayRoller {
    public class RayRollerPerception : MonoBehaviour {
        public float RayDistance = 3;

        public List<string> DetectableObjects;
        public float StartOffset = 0f;
        public float EndOffset = 0f;

        private RaycastHit hit;
        private List<float> rayAngles;
        private List<GameObject> lineGameObjects = new List<GameObject>();
        private List<float> m_PerceptionBuffer = new List<float>();

        public void Initialize(float[] rayAngles) {
            this.rayAngles = new List<float>(rayAngles);

            if (this.rayAngles.Count != lineGameObjects.Count) {
                for (int i = 0; i < this.rayAngles.Count; i++) {
                    GameObject g = new GameObject(i.ToString());
                    LineRenderer line = g.AddComponent<LineRenderer>();
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startWidth = 0.02f;
                    line.startColor = line.endColor = Color.green;
                    lineGameObjects.Add(g);
                }
            }

            Act();
        }

        public void Act() {
            m_PerceptionBuffer.Clear();

            for (int i = 0; i < rayAngles.Count; i++) {
                float angle = rayAngles[i];

                Vector3 startPosition = transform.position + new Vector3(0f, StartOffset, 0f);
                Vector3 offsetDirection = PolarToCartesian(RayDistance, angle);
                offsetDirection.y = EndOffset;
                Vector3 endPosition = startPosition + offsetDirection;

                LineRenderer lineRenderer = lineGameObjects[i].GetComponent<LineRenderer>();
                lineRenderer.startColor = lineRenderer.endColor = Color.green;
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, endPosition);

                float[] subList = new float[DetectableObjects.Count + 2];
                if (Physics.SphereCast(startPosition, 0.02f, offsetDirection, out hit, RayDistance)) {
                    lineRenderer.startColor = lineRenderer.endColor = Color.red;
                    for (int j = 0; j < DetectableObjects.Count; j++) {
                        if (hit.collider.gameObject.CompareTag(DetectableObjects[j])) {
                            subList[j] = 1;
                            subList[DetectableObjects.Count + 1] = hit.distance / RayDistance;
                            break;
                        }
                    }
                }
                else {
                    subList[DetectableObjects.Count] = 1f;
                }

                m_PerceptionBuffer.AddRange(subList);
            }
        }

        public List<float> Perceive() {
            return m_PerceptionBuffer;
        }

        /// <summary>
        /// Converts polar coordinate to cartesian coordinate.
        /// </summary>
        public static Vector3 PolarToCartesian(float radius, float angle) {
            float x = radius * Mathf.Cos(DegreeToRadian(angle));
            float z = radius * Mathf.Sin(DegreeToRadian(angle));
            return new Vector3(x, 0f, z);
        }

        public static float DegreeToRadian(float degree) {
            return degree * Mathf.PI / 180f;
        }

    }
}
