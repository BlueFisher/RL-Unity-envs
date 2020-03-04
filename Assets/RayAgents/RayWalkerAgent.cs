using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RayRoller {
    public class RayWalkerAgent : Agent {
        Rigidbody m_AgentRb;
        Rigidbody m_TargetRb;

        bool wallCollided = false;
        bool targetCollided = false;

        IFloatProperties m_ResetParams;

        public Transform Target;
        public float TargetRadius = 9;
        public float Speed = 2;
        public bool AvoidWall = false;

        public override void InitializeAgent() {
            m_AgentRb = GetComponent<Rigidbody>();
            m_TargetRb = Target.GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.FloatProperties;
        }

        public override void AgentReset() {
            AvoidWall = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("avoid_wall", System.Convert.ToSingle(AvoidWall)));
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            // Generate agent
            if (forceReset || (AvoidWall && wallCollided)) {
                transform.localPosition = new Vector3(TargetRadius * (Random.value * 2 - 1),
                                                        0.5f,
                                                        TargetRadius * (Random.value * 2 - 1));
                transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
                m_AgentRb.velocity = Vector3.zero;
            }

            // Generate target
            Target.localPosition = new Vector3(TargetRadius * (Random.value * 2 - 1),
                                                1.75f,
                                                TargetRadius * (Random.value * 2 - 1));
            Target.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_TargetRb.velocity = Vector3.zero;
            m_TargetRb.angularVelocity = Vector3.zero;

            wallCollided = false;
            targetCollided = false;
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("wall")) {
                wallCollided = true;
            }
            else if (collision.gameObject.CompareTag("target")) {
                targetCollided = true;
            }
        }

        public override void CollectObservations() {
            m_ResetParams.SetProperty("force_reset", 0);

            AddVectorObs(transform.localPosition.x / 10f);
            AddVectorObs(transform.localPosition.z / 10f);

            // Agent velocity
            var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            AddVectorObs(velocity.x);
            AddVectorObs(velocity.z);
        }

        public override void AgentAction(float[] vectorAction) {
            if (targetCollided) {
                SetReward(1.0f);
                Done();
            }
            else if (AvoidWall && wallCollided) {
                SetReward(-1.0f);
                Done();
            }
            else {
                SetReward(-0.01f);
            }

            var dirToGo = transform.forward * vectorAction[0];
            var rotateDir = transform.up * vectorAction[1];
            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            m_AgentRb.AddForce(dirToGo * Speed, ForceMode.VelocityChange);
        }

        public override float[] Heuristic() {
            var action = new float[2];

            action[0] = Input.GetAxis("Vertical");
            action[1] = Input.GetAxis("Horizontal");
            return action;
        }
    }
}
