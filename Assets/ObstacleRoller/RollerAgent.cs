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
        public Transform Floor;

        private Vector3 convertToFloorRelativePos(Vector3 v) {
            return new Vector3(Floor.position.x + v.x, Floor.position.y + v.y, Floor.position.z + v.z);
        }
        private Vector3 converToWorldRelativePos(Vector3 v) {
            return new Vector3(v.x - Floor.position.x, v.y - Floor.position.y, v.z - Floor.position.z);
        }
        private bool IsOutOfRegion() {
            return transform.position.x > Floor.position.x + 5 || transform.position.x < Floor.position.x - 5
                || transform.position.z > Floor.position.z + 5 || transform.position.z < Floor.position.z - 5;
        }
        private bool IsHitObstacle() {
            float distanceToObstacle = Vector3.Distance(transform.position, Obstacle.position);
            return distanceToObstacle < 1.1f;
        }


        public override void AgentReset() {
            previousDistanceToTarget = float.PositiveInfinity;
            if (IsOutOfRegion() || IsHitObstacle()) {
                float angle = Random.value * Mathf.PI * 2;

                float randomRadius = Random.value * 2 + 2;
                float x = Mathf.Cos(angle) * randomRadius;
                float z = Mathf.Sin(angle) * randomRadius;
                transform.position = convertToFloorRelativePos(new Vector3(x, 0.5f, z));

                randomRadius = Random.value * 2 + 2;
                x = Mathf.Cos(angle) * randomRadius;
                z = Mathf.Sin(angle) * randomRadius;
                Target.position = convertToFloorRelativePos(new Vector3(-x, 0.5f, -z));

                angle = angle + Mathf.PI / 2;
                x = Mathf.Cos(angle) * (Random.value * 4 - 2);
                z = Mathf.Sin(angle) * (Random.value * 4 - 2);
                Obstacle.position = convertToFloorRelativePos(new Vector3(x, 0.5f, z));

                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
            }
            else {
                var angle = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(converToWorldRelativePos(transform.position), Vector3.right)),
                    Vector3.Dot(converToWorldRelativePos(transform.position), Vector3.right));
                float randomRadius = Random.value * 2 + 2;
                float x = Mathf.Cos(angle) * randomRadius;
                float z = Mathf.Sin(angle) * randomRadius;
                Target.position = convertToFloorRelativePos(new Vector3(-x, 0.5f, -z));

                angle = angle + Mathf.PI / 2;
                x = Mathf.Cos(angle) * (Random.value * 4 - 2);
                z = Mathf.Sin(angle) * (Random.value * 4 - 2);
                Obstacle.position = convertToFloorRelativePos(new Vector3(x, 0.5f, z));
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
            var agentPos = converToWorldRelativePos(transform.position);
            var targetPos = converToWorldRelativePos(Target.position);
            var obstaclePos = converToWorldRelativePos(Obstacle.position);
            AddVectorObs(agentPos.x / 5);
            AddVectorObs(agentPos.z / 5);
            AddVectorObs(targetPos.x / 5);
            AddVectorObs(targetPos.z / 5);
            AddVectorObs(obstaclePos.x / 5);
            AddVectorObs(obstaclePos.z / 5);

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public float speed = 10;

        private float previousDistanceToTarget = float.PositiveInfinity;
        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(transform.position, Target.position);


            if (distanceToTarget < 1.42f) { // Reached target
                AddReward(1.0f);
                Done();
            }
            else if (IsHitObstacle() || IsOutOfRegion()) {
                AddReward(-1.0f);
                Done();
            }
            else {
                if (distanceToTarget < previousDistanceToTarget) {
                    AddReward(0.01f);
                }
                else {
                    AddReward(-0.01f);
                }
                previousDistanceToTarget = distanceToTarget;
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);
        }
    }
}