using UnityEngine;
using MLAgents;
using System.Collections.Generic;

namespace AttackGuard {
    public class AttackAgent : Agent {
        private Rigidbody rBody;
        void Start() {
            rBody = GetComponent<Rigidbody>();
        }

        public Transform Target;
        public Transform GuardAgent;

        private bool IsOutOfRegion() {
            return transform.position.x > 5 || transform.position.x < -5 || transform.position.z > 5 || transform.position.z < -5;
        }

        public override void AgentReset() {
            List<int> arr = new List<int> { 0, 1, 2, 3 };
            List<Transform> transforms = new List<Transform> { this.transform, GuardAgent, Target };
            arr = ListRandom(arr);
            for (int i = 0; i < transforms.Count; i++) {
                int x = 1;
                int z = 1;
                if (arr[i] == 2 || arr[i] == 3) {
                    x = -1;
                }
                if (arr[i] == 1 || arr[i] == 3) {
                    z = -1;
                }
                transforms[i].position = new Vector3(UnityEngine.Random.value * x * 4, 0.5f, UnityEngine.Random.value * z * 4);
            }
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
        }
        private List<int> ListRandom(List<int> myList) {
            System.Random ran = new System.Random();
            List<int> newList = new List<int>();
            int index = 0;
            int temp = 0;
            for (int i = 0; i < myList.Count; i++) {
                index = ran.Next(0, myList.Count - 1);
                if (index != i) {
                    temp = myList[i];
                    myList[i] = myList[index];
                    myList[index] = temp;
                }
            }
            return myList;
        }

        public override void CollectObservations() {
            // Calculate relative position to target
            Vector3 relativePositionToTarget = Target.position - transform.position;
            AddVectorObs(relativePositionToTarget.x / 5);
            AddVectorObs(relativePositionToTarget.z / 5);

            // Calculate relative position to guard agent
            Vector3 relativePositionToGuard = GuardAgent.position - transform.position;
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
            float distanceToTarget = Vector3.Distance(transform.position, Target.position);
            float distanceToGuard = Vector3.Distance(transform.position, GuardAgent.position);


            if (distanceToTarget < 1.2f) { // Reached target
                AddReward(1.0f);
                Done();
            }
            else if (distanceToGuard < 1.2f) {
                AddReward(-1.0f);
            }
            else { // Time penalty
                AddReward(-0.01f);
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