using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace ObstacleRoller {
    public class RollerAgentLarge : Agent {
        private Rigidbody rBody;
        private Rigidbody obstacleRBody;
        public Transform Target;
        public GameObject Obstacle;
        public override void InitializeAgent() {
            rBody = GetComponent<Rigidbody>();
            obstacleRBody = Obstacle.GetComponent<Rigidbody>();
        }

        private bool IsOutOfRegion() {
            return transform.localPosition.x > 10 || transform.localPosition.x < -10
                || transform.localPosition.z > 10 || transform.localPosition.z < -10;
        }
        private bool IsHitObstacle() {
            float distanceToObstacle = Vector3.Distance(transform.localPosition, Obstacle.transform.localPosition);
            return distanceToObstacle < 1.1f;
        }

        public override void AgentReset() {
            float angle = 0;
            if (IsOutOfRegion() || IsHitObstacle()) {
                angle = Random.value * Mathf.PI * 2;

                float _randomRadius = Random.value * 2 + 2;
                float _x = Mathf.Cos(angle) * _randomRadius;
                float _z = Mathf.Sin(angle) * _randomRadius;
                transform.localPosition = new Vector3(_x, 0.5f, _z);
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
            }
            else {
                angle = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(transform.localPosition, Vector3.right)),
                    Vector3.Dot(transform.localPosition, Vector3.right));
            }
            float randomRadius = Random.value * 2 + 2;
            float x = Mathf.Cos(angle) * randomRadius;
            float z = Mathf.Sin(angle) * randomRadius;
            Target.localPosition = new Vector3(-x, 0.5f, -z);

            Obstacle.transform.localPosition = new Vector3(0, 0.5f, 0);
            obstacleRBody.angularVelocity = Vector3.zero;
            obstacleRBody.velocity = Vector3.zero;
        }

        public override void CollectObservations() {
            var agentPos = transform.localPosition;
            var targetPos = Target.localPosition;
            var obstaclePos = Obstacle.transform.localPosition;
            AddVectorObs(agentPos.x / 10);
            AddVectorObs(agentPos.z / 10);
            AddVectorObs(targetPos.x / 10);
            AddVectorObs(targetPos.z / 10);
            AddVectorObs(obstaclePos.x / 10);
            AddVectorObs(obstaclePos.z / 10);
            AddVectorObs(obstacleRBody.velocity.x / 5);
            AddVectorObs(obstacleRBody.velocity.z / 5);

            // Agent velocity
            AddVectorObs(rBody.velocity.x / 5);
            AddVectorObs(rBody.velocity.z / 5);
        }

        public float speed = 10;

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);


            if (distanceToTarget < 1.42f) { // Reached target
                AddReward(1.0f);
                Done();
            }
            else if (IsHitObstacle() || IsOutOfRegion()) {
                AddReward(-1.0f);
                Done();
            }
            else {
                AddReward(-0.01f);
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);


            float x_delta = transform.position.x + rBody.velocity.x - Obstacle.transform.position.x;
            float z_delta = transform.position.z + rBody.velocity.z - Obstacle.transform.position.z;
            float t = Mathf.Max(Mathf.Abs(x_delta), Mathf.Abs(z_delta));

            if (obstacleRBody.velocity.x >= 1) {
                x_delta = 0;
            }
            if (obstacleRBody.velocity.z >= 1) {
                z_delta = 0;
            }

            obstacleRBody.AddForce(new Vector3(x_delta, 0, z_delta));
        }
    }
}