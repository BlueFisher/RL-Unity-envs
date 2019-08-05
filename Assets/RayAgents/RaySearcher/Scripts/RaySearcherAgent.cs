using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace RaySearcher {
    public class RaySearcherAgent : Agent {
        private Rigidbody rBody;
        //private bool wallCollided = true;
        private RayPerception rayPer;

        public Transform Target;
        public float RayDistance = 35f;

        private Transform[] spawnAreas;
        public Transform SpawnArea;

        public override void InitializeAgent() {
            rBody = GetComponent<Rigidbody>();
            rayPer = GetComponent<RayPerception>();

            List<Transform> tmpSpawnAreas = SpawnArea.GetComponentsInChildren<Transform>().ToList();
            tmpSpawnAreas.RemoveAt(0);
            spawnAreas = tmpSpawnAreas.ToArray();
        }

        private void GenerateTarget() {
            int index = Random.Range(0, spawnAreas.Length);
            var spawnTransform = spawnAreas[index].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;
            while (true) {
                var newPosition = new Vector3(Random.Range(-xRange, xRange), 2f, Random.Range(-zRange, zRange)) + spawnTransform.localPosition;
                if (Utility.GetXZDistance(transform.localPosition, newPosition) > 8f) {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }
        private void GenerateAgent() {
            int index = Random.Range(0, spawnAreas.Length);
            var spawnTransform = spawnAreas[index].transform;
            var xRange = spawnTransform.localScale.x / 2.1f;
            var zRange = spawnTransform.localScale.z / 2.1f;

            transform.localPosition = new Vector3(Random.Range(-xRange, xRange), 0.5f, Random.Range(-zRange, zRange)) + spawnTransform.localPosition;
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
        }


        public override void AgentReset() {
            GenerateAgent();
            GenerateTarget();
        }

        public override void CollectObservations() {
            string[] detectableObjects = { "wall", "target" };

            float[] rayAngles = new float[] { -40f, -30f, -20f, -10f, 0f, 10f, 20f, 30f, 40f };
            AddVectorObs(rayPer.Perceive(RayDistance, rayAngles, detectableObjects, 0f, 0f));

            AddVectorObs(transform.localPosition.x / 40f);
            AddVectorObs(transform.localPosition.z / 40f);
            AddVectorObs(transform.forward.x);
            AddVectorObs(transform.forward.z);
            AddVectorObs(transform.InverseTransformDirection(rBody.velocity).x / 5);
            AddVectorObs(transform.InverseTransformDirection(rBody.velocity).z / 5);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Rewards
            float distanceToTarget = Utility.GetXZDistance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 5f) { // Reached target
                SetReward(2f);
                Done();
            }
            else { // Time penalty
                AddReward(-1f / agentParameters.maxStep);
            }

            var rotateDir = transform.up * Mathf.Clamp(vectorAction[0], -1f, 1f);
            var dirToGo = transform.forward * Mathf.Clamp(vectorAction[1], -1f, 1f);

            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            rBody.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
        }
    }
}