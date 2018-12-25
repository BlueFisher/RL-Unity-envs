using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace SimpleRoller {
    public class RollerAgent : Agent {
        private Rigidbody rBody;
        void Start() {
            rBody = GetComponent<Rigidbody>();
        }

        public Transform Target;
        private bool IsOutOfRegion() {
            return transform.position.x > 5 || transform.position.x < -5 || transform.position.z > 5 || transform.position.z < -5;
            //return transform.position.y < -0.4f;
        }
        private void GenerateTarget() {
            while (true) {
                var newPosition = new Vector3(Random.value * 8 - 4,
                                                  0.5f,
                                                  Random.value * 8 - 4);
                if (Vector3.Distance(this.transform.position, newPosition) > 2f) {
                    Target.position = newPosition;
                    break;
                }
            }
        }
        public override void AgentReset() {
            if (IsOutOfRegion()) {
                this.transform.position = Vector3.zero;
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
            }
            GenerateTarget();
        }

        public override void CollectObservations() {
            // Calculate relative position
            Vector3 relativePosition = Target.position - this.transform.position;

            // Relative position
            AddVectorObs(relativePosition.x / 5);
            AddVectorObs(relativePosition.z / 5);

            // Distance to edges of platform
            AddVectorObs((this.transform.position.x + 5) / 5);
            AddVectorObs((this.transform.position.x - 5) / 5);
            AddVectorObs((this.transform.position.z + 5) / 5);
            AddVectorObs((this.transform.position.z - 5) / 5);

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public float speed = 10;

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);


            if (distanceToTarget < 1.42f) { // Reached target
                AddReward(1.0f);
                Done();
            }
            else { // Time penalty
                AddReward(-0.01f);
            }

            // Fell off platform
            if (IsOutOfRegion()) {
                AddReward(-1.0f);
                Done();
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);
        }
    }
}