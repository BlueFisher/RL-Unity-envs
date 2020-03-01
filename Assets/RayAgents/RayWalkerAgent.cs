using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RayRoller {
    public class RayWalkerAgent : Agent {
        Rigidbody m_AgentRb;
        private bool wallCollided = false;

        public Transform Target;
        public float TargetRadius = 4;
        public float Speed = 2;
        public bool AvoidWall = false;

        public override void InitializeAgent() {
            m_AgentRb = GetComponent<Rigidbody>();
        }

        private void GenerateTarget() {
            var newPosition = new Vector3(TargetRadius * (Random.value * 2 - 1),
                                                2.5f,
                                                TargetRadius * (Random.value * 2 - 1));
            Target.localPosition = newPosition;
            Target.rotation = Quaternion.identity;
        }
        public override void AgentReset() {
            if (wallCollided) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
                m_AgentRb.velocity = Vector3.zero;
            }

            GenerateTarget();

            wallCollided = false;
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("wall")) {
                wallCollided = true;
            }
        }

        public override void CollectObservations() {
            AddVectorObs(transform.localPosition.x / 10f);
            AddVectorObs(transform.localPosition.z / 10f);
            // Agent velocity
            var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            AddVectorObs(velocity.x);
            AddVectorObs(velocity.z);
        }

        public override void AgentAction(float[] vectorAction) {
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
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
