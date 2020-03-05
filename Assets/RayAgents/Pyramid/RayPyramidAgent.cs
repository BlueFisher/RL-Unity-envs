using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RayAgents {
    public class RayPyramidAgent : RayAgent {
        public GameObject[] SpawnAreas;

        void GenerateTarget(int spawnAreaIndex) {
            var spawnTransform = SpawnAreas[spawnAreaIndex].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;

            Target.localPosition = new Vector3(Random.Range(-xRange, xRange), 2f, Random.Range(-zRange, zRange))
                + spawnTransform.localPosition;
            Target.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_TargetRb.velocity = Vector3.zero;
            m_TargetRb.angularVelocity = Vector3.zero;
        }

        void GenerateAgent(int spawnAreaIndex) {
            var spawnTransform = SpawnAreas[spawnAreaIndex].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;

            transform.localPosition = new Vector3(Random.Range(-xRange, xRange), 0.5f, Random.Range(-zRange, zRange))
                + spawnTransform.localPosition;
            transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_AgentRb.velocity = Vector3.zero;
        }

        public override void AgentReset() {
            AvoidWall = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("avoid_wall", System.Convert.ToSingle(AvoidWall)));
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            var enumerable = Enumerable.Range(0, 9).OrderBy(x => System.Guid.NewGuid()).Take(2);
            var items = enumerable.ToArray();

            // Generate agent
            if (forceReset || (AvoidWall && wallCollided)) {
                GenerateAgent(items[0]);
            }

            GenerateTarget(items[1]);

            wallCollided = false;
            targetCollided = false;
        }

        public override void CollectObservations() {
            m_ResetParams.SetProperty("force_reset", 0);

            AddVectorObs(transform.localPosition.x / 40f);
            AddVectorObs(transform.localPosition.z / 40f);

            // Agent velocity
            var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            AddVectorObs(velocity.x);
            AddVectorObs(velocity.z);
        }
    }
}
