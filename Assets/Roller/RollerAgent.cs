using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;
using MLAgents.SideChannels;
using MLAgents.Sensors;

namespace Roller {
    public class RollerAgent : Agent {
        Rigidbody m_AgentRb;
        FloatPropertiesChannel m_ResetParams;

        public Transform Target;
        public bool IsHardMode = false;

        public override void Initialize() {
            m_AgentRb = GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.FloatProperties;
        }

        private bool IsOutOfRegion() {
            return transform.localPosition.x > 5 || transform.localPosition.x < -5
                || transform.localPosition.z > 5 || transform.localPosition.z < -5;
        }

        public override void OnEpisodeBegin() {
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            if (forceReset || IsOutOfRegion()) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                m_AgentRb.angularVelocity = Vector3.zero;
                m_AgentRb.velocity = Vector3.zero;
            }

            Target.localPosition = new Vector3(Random.value * 8 - 4,
                                                  0.5f,
                                                  Random.value * 8 - 4);
        }

        public override void CollectObservations(VectorSensor sensor) {
            sensor.AddObservation(Target.localPosition.x / 5);
            sensor.AddObservation(Target.localPosition.z / 5);
            
            sensor.AddObservation(transform.localPosition.x / 5);
            sensor.AddObservation(transform.localPosition.z / 5);

            if (!IsHardMode) {
                // Agent velocity
                sensor.AddObservation(m_AgentRb.velocity.x / 5);
                sensor.AddObservation(m_AgentRb.velocity.z / 5);
            }
        }

        public float speed = 10;

        public override void OnActionReceived(float[] vectorAction) {
            m_ResetParams.SetProperty("force_reset", 0);

            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
                SetReward(1.0f);
                EndEpisode();
            }
            else { // Time penalty
                SetReward(-0.01f);
            }

            // Fell off platform
            if (IsOutOfRegion()) {
                SetReward(-1.0f);
                EndEpisode();
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            m_AgentRb.AddForce(controlSignal * speed);
        }

        public override float[] Heuristic() {
            var action = new float[2];

            action[0] = Input.GetAxis("Horizontal");
            action[1] = Input.GetAxis("Vertical");
            return action;
        }
    }
}