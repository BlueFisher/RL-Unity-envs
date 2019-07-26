using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RaySearcher {
    public class RaySearcherAgent : Agent {
        private Rigidbody rBody;
        private bool wallCollided = false;
        private RayPerception rayPer;

        public Transform Target;
        public float TargetRadius = 4;
        public float RayDistance = 5f;
        public float[] RayAngles = new float[] { -40f, -20f, 0f, 20f, 40f, -140f, 180f, 140f };

        public override void InitializeAgent() {
            rBody = GetComponent<Rigidbody>();
            rayPer = GetComponent<RayPerception>();
        }

        private void GenerateTarget() {
            while (true) {
                var newPosition = new Vector3(TargetRadius * (Random.value * 2 - 1),
                                                  0.5f,
                                                  TargetRadius * (Random.value * 2 - 1));
                if (Vector3.Distance(transform.localPosition, newPosition) > 2f) {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }
        public override void AgentReset() {
            if (wallCollided) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
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
            string[] detectableObjects = { "wall", "target" };

            AddVectorObs(rayPer.Perceive(RayDistance, RayAngles, detectableObjects, 0f, 0f));

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
                SetReward(1.0f);
                Done();
            }
            else if (wallCollided) {
                SetReward(-1.0f);
                Done();
            }
            else { // Time penalty
                SetReward(-0.01f);
            }

            var rotateDir = transform.up * Mathf.Clamp(vectorAction[0], -1f, 1f);
            var dirToGo = transform.forward * Mathf.Clamp(vectorAction[1], -1f, 1f);

            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            rBody.AddForce(dirToGo, ForceMode.VelocityChange);
        }
    }
}