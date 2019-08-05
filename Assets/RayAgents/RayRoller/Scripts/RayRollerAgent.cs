using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RayRoller {
    public class RayRollerAgent : Agent {
        private Rigidbody rBody;
        private bool wallCollided = false;
        private RayPerception rayPer;

        public Transform Target;
        public float TargetRadius = 4;
        public float RayDistance = 5f;
        public int RayLines = 16;
        public float Speed = 10;

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
            float[] rayAngles = new float[16];
            for (int i = 0; i < RayLines; i++) {
                rayAngles[i] = 360f / RayLines * i;
            }

            string[] detectableObjects = { "wall", "target" };

            AddVectorObs(rayPer.Perceive(RayDistance, rayAngles, detectableObjects, 0f, 0f));

            AddVectorObs(transform.localPosition.x / 6f);
            AddVectorObs(transform.localPosition.z / 6f);
            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5f);
            AddVectorObs(rBody.velocity.z / 5f);
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

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * Speed);
        }
    }
}
