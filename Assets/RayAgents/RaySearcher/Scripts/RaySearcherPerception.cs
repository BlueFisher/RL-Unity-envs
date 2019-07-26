using System.Collections.Generic;
using UnityEngine;

namespace RaySearcher {
    /// <summary>
    /// Ray perception component. Attach this to agents to enable "local perception"
    /// via the use of ray casts directed outward from the agent. 
    /// </summary>
    public class RaySearcherPerception : RayPerception {
        private RaycastHit hit;
        private List<GameObject> lineGameObjects = new List<GameObject>();

        /// <summary>
        /// Creates perception vector to be used as part of an observation of an agent.
        /// </summary>
        /// <returns>The partial vector observation corresponding to the set of rays</returns>
        /// <param name="rayDistance">Radius of rays</param>
        /// <param name="rayAngles">Angles of rays (starting from (1,0) on unit circle).</param>
        /// <param name="detectableObjects">List of tags which correspond to object types agent can see</param>
        /// <param name="startOffset">Starting height offset of ray from center of agent.</param>
        /// <param name="endOffset">Ending height offset of ray from center of agent.</param>
        public override List<float> Perceive(float rayDistance,
            float[] rayAngles, string[] detectableObjects,
            float startOffset, float endOffset) {
            perceptionBuffer.Clear();
            if (rayAngles.Length != lineGameObjects.Count) {
                foreach (var g in lineGameObjects) {
                    Destroy(g);
                }
                for (int i = 0; i < rayAngles.Length; i++) {
                    GameObject g = new GameObject(i.ToString());
                    g.transform.parent = gameObject.transform;
                    LineRenderer line = g.AddComponent<LineRenderer>();
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startWidth = 0.02f;
                    line.startColor = line.endColor = Color.green;
                    lineGameObjects.Add(g);
                }
            }
            // For each ray sublist stores categorical information on detected object
            // along with object distance.
            for (int i = 0; i < rayAngles.Length; i++) {
                float angle = rayAngles[i];

                Vector3 startPosition = transform.position + new Vector3(0f, startOffset, 0f);
                Vector3 offsetDirection = transform.TransformDirection(PolarToCartesian(rayDistance, angle));
                offsetDirection.y = endOffset;
                Vector3 endPosition = startPosition + offsetDirection;

                LineRenderer lineRenderer = lineGameObjects[i].GetComponent<LineRenderer>();
                lineRenderer.startColor = lineRenderer.endColor = Color.green;
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, endPosition);

                float[] subList = new float[detectableObjects.Length + 2];
                if (Physics.SphereCast(startPosition, 0.02f, offsetDirection, out hit, rayDistance)) {
                    lineRenderer.startColor = lineRenderer.endColor = Color.red;
                    for (int j = 0; j < detectableObjects.Length; j++) {
                        if (hit.collider.gameObject.CompareTag(detectableObjects[j])) {
                            subList[j] = 1;
                            subList[detectableObjects.Length + 1] = hit.distance / rayDistance;
                            break;
                        }
                    }
                }
                else {
                    subList[detectableObjects.Length] = 1f;
                }

                perceptionBuffer.AddRange(subList);
            }

            return perceptionBuffer;
        }

        /// <summary>
        /// Converts polar coordinate to cartesian coordinate.
        /// </summary>
        public static Vector3 PolarToCartesian(float radius, float angle) {
            float x = radius * Mathf.Sin(DegreeToRadian(angle));
            float z = radius * Mathf.Cos(DegreeToRadian(angle));
            return new Vector3(x, 0f, z);
        }

    }
}
