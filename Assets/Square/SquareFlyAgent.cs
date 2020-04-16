using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents.Sensors;

namespace Square {
    public class SquareFlyAgent : BaseSquareAgent {
        protected void GenerateAgent() {
            transform.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                        5f,
                                                        SpawnRadius * (Random.value * 2 - 1));
            transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_AgentRb.velocity = Vector3.zero;
        }

        public bool IsAgentOut() {
            return transform.localPosition.x < -15 || transform.localPosition.x > 15
                || transform.localPosition.z < -15 || transform.localPosition.z > 15;
        }

        public override void OnEpisodeBegin() {
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            foreach (var ray in rays) {
                float rayLength = m_ResetParams.GetPropertyWithDefault("ray_length", ray.rayLength);
                ray.rayLength = rayLength;
            }

            if (forceReset || IsAgentOut()) {
                GenerateAgent();
            }

            GenerateTarget();
        }

        public override void CollectObservations(VectorSensor sensor) {
            m_ResetParams.SetProperty("force_reset", 0);

            if (sensor != null) {
                sensor.AddObservation(transform.localPosition.x / 10f);
                sensor.AddObservation(transform.localPosition.z / 10f);

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
            if (Vector2.Distance(new Vector2(Target.transform.localPosition.x, Target.transform.localPosition.z),
                    new Vector2(transform.localPosition.x, transform.localPosition.z)) <= 2f) {
                SetReward(1.0f);
                EndEpisode();
            }
            else if (IsAgentOut()) {
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