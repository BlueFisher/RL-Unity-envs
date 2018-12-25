using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace ObstacleRoller {
    public class RollerAgent : Agent {
        private Rigidbody rBody;
        void Start() {
            rBody = GetComponent<Rigidbody>();
        }

        public Transform Target;
        public Transform Obstacle;
        private bool IsOutOfRegion() {
            return transform.position.x > 5 || transform.position.x < -5 || transform.position.z > 5 || transform.position.z < -5;
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
        public override void AgentReset() {
            if (IsOutOfRegion()) {
                List<int> arr = new List<int> { 0, 1, 2, 3 };
                List<Transform> transforms = new List<Transform> { transform, Obstacle, Target };
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
                    transforms[i].position = new Vector3(Random.value * x * 4, 0.5f, Random.value * z * 4);
                }
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
            }
            else {
                while (true) {
                    var newPosition = new Vector3(Random.value * 8 - 4,
                                                      0.5f,
                                                      Random.value * 8 - 4);
                    if (Vector3.Distance(transform.position, newPosition) > 2f) {
                        Target.position = newPosition;
                        break;
                    }
                }
                while (true) {
                    var newPosition = new Vector3(Random.value * 8 - 4,
                                                      0.5f,
                                                      Random.value * 8 - 4);
                    if (Vector3.Distance(transform.position, newPosition) > 2f && Vector3.Distance(Target.position, newPosition) > 2f) {
                        Obstacle.position = newPosition;
                        break;
                    }
                }
            }
        }

        public override void CollectObservations() {
            //Vector3 relativePositionToTarget = Target.position - transform.position;
            //AddVectorObs(relativePositionToTarget.x / 5);
            //AddVectorObs(relativePositionToTarget.z / 5);

            //Vector3 relativePositionToObstacle = Obstacle.position - transform.position;
            //AddVectorObs(relativePositionToObstacle.x / 5);
            //AddVectorObs(relativePositionToObstacle.z / 5);

            //// Distance to edges of platform
            //AddVectorObs((transform.position.x + 5) / 5);
            //AddVectorObs((transform.position.x - 5) / 5);
            //AddVectorObs((transform.position.z + 5) / 5);
            //AddVectorObs((transform.position.z - 5) / 5);

            AddVectorObs(transform.position.x / 5);
            AddVectorObs(transform.position.z / 5);
            AddVectorObs(Target.position.x / 5);
            AddVectorObs(Target.position.z / 5);
            AddVectorObs(Obstacle.position.x / 5);
            AddVectorObs(Obstacle.position.z / 5);

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public float speed = 10;

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);
            float distanceToObstacle = Vector3.Distance(this.transform.position, Obstacle.position);


            if (distanceToTarget < 1.42f) { // Reached target
                AddReward(1.0f);
                Done();
            }
            else if (distanceToObstacle < 1.42f || IsOutOfRegion()) {
                AddReward(-1.0f);
                Done();
            }
            else { // Time penalty
                AddReward(-0.01f);
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);
        }
    }
}