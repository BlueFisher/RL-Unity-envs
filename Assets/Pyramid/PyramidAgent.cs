using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;
using MLAgents.Sensors;

namespace Pyramid {
    public class PyramidAgent : Square.BaseSquareAgent {
        public bool AvoidWall = false;
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

        public override void OnEpisodeBegin() {
            AvoidWall = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("avoid_wall", System.Convert.ToSingle(AvoidWall)));
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            foreach (var ray in rays) {
                float rayLength = m_ResetParams.GetPropertyWithDefault("ray_length", ray.rayLength);
                ray.rayLength = rayLength;
            }

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

        public override void CollectObservations(VectorSensor sensor) {
            m_ResetParams.SetProperty("force_reset", 0);

            if (sensor != null) {
                sensor.AddObservation(transform.localPosition.x / 40f);
                sensor.AddObservation(transform.localPosition.z / 40f);

                // Agent forward direction
                sensor.AddObservation(transform.forward.x);
                sensor.AddObservation(transform.forward.z);

                // Agent velocity
                var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
                sensor.AddObservation(velocity.x);
                sensor.AddObservation(velocity.z);
            }
        }

        public override void OnActionReceived(float[] vectorAction) {
            if (targetCollided) {
                SetReward(1.0f);
                EndEpisode();
            }
            else if (AvoidWall && wallCollided) {
                SetReward(-1.0f);
                EndEpisode();
            }
            else {
                AddReward(-1f / maxStep);
            }

            var dirToGo = transform.forward * vectorAction[0];
            var rotateDir = transform.up * vectorAction[1];
            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            m_AgentRb.AddForce(dirToGo * Speed, ForceMode.VelocityChange);
        }
    }
}
