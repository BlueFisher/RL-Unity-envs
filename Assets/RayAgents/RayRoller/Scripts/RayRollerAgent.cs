using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RayRoller {
    public class RayRollerAgent : Agent {
        private Rigidbody rBody;
        private bool wallCollided = false;
        private RayRollerPerception rayPer;

        public Transform Target;
        public float TargetRadius = 4;
        public int RayLines = 16;
        public float Speed = 10;
        public bool AvoidWall = false;

        public override void InitializeAgent() {
            rBody = GetComponent<Rigidbody>();
            rayPer = GetComponent<RayRollerPerception>();

            float[] rayAngles = new float[16];
            for (int i = 0; i < RayLines; i++) {
                rayAngles[i] = 360f / RayLines * i;
            }

            rayPer.Initialize(rayAngles);
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
            //print("reset");
            if (AvoidWall && wallCollided) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
            }

            GenerateTarget();

            wallCollided = false;
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("wall")) {
                //print("collision");
                wallCollided = true;
            }
        }

        public override void CollectObservations() {
            //print("ob");
            AddVectorObs(rayPer.Perceive());

            AddVectorObs(transform.localPosition.x / 9f);
            AddVectorObs(transform.localPosition.z / 9f);
            // Agent velocity
            AddVectorObs(rBody.velocity.x / 6f);
            AddVectorObs(rBody.velocity.z / 6f);
        }

        public override void AgentAction(float[] vectorAction) {
            //print("action");
            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
                //print("done hit");
                SetReward(1.0f);
                Done();
            }
            else if (AvoidWall && wallCollided) {
                //print("done wallCollided");
                SetReward(-1.0f);
                Done();
            }
            else {
                SetReward(-0.01f);
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * Speed);

            rayPer.Act();
        }

        public override float[] Heuristic() {
            var action = new float[2];

            action[0] = Input.GetAxis("Horizontal");
            action[1] = Input.GetAxis("Vertical");
            return action;
        }
    }
}
