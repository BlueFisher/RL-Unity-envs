using UnityEngine;
using MLAgents;

namespace AttackGuard {
    public class GameAgent : Agent {
        private Rigidbody rBody;
        void Start() {
            rBody = GetComponent<Rigidbody>();
        }

        public Transform Target;
        public Transform AttackAgent;

        private bool IsOutOfRegion() {
            return transform.position.x > 5 || transform.position.x < -5 || transform.position.z > 5 || transform.position.z < -5;
        }

        public override void AgentReset() {
            //transform.position = new Vector3(0, 0.5f, -2);
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
        }

        public override void CollectObservations() {
            // Calculate relative position to target
            Vector3 relativePositionToTarget = Target.position - transform.position;
            AddVectorObs(relativePositionToTarget.x / 5);
            AddVectorObs(relativePositionToTarget.z / 5);

            // Calculate relative position to attack agent
            Vector3 relativePositionToGuard = AttackAgent.position - transform.position;
            AddVectorObs(relativePositionToGuard.x / 5);
            AddVectorObs(relativePositionToGuard.z / 5);

            // Distance to edges of platform
            AddVectorObs((transform.position.x + 5) / 5);
            AddVectorObs((transform.position.x - 5) / 5);
            AddVectorObs((transform.position.z + 5) / 5);
            AddVectorObs((transform.position.z - 5) / 5);

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public float speed = 10;

        public override void AgentAction(float[] vectorAction, string textAction) {
            float distanceFromAttackAgentToTarget =
                Vector3.Distance(Target.position, AttackAgent.position);


            if (distanceFromAttackAgentToTarget < 1.2f) { // Reached target
                AddReward(-1.0f);
                Done();
            }
            else { // Time reward
                AddReward(0.01f);
            }

            if (IsOutOfRegion()) {
                AddReward(-1.0f);
                Done();
            }

            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);
        }
    }
}