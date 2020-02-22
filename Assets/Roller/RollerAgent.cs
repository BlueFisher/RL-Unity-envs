﻿using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

namespace Roller {
    public class RollerAgent : Agent {
        private Rigidbody rBody;
        public Transform Target;
        public bool IsHardMode = false;

        public override void InitializeAgent() {
            rBody = GetComponent<Rigidbody>();
        }

        private bool IsOutOfRegion() {
            return transform.localPosition.x > 5 || transform.localPosition.x < -5
                || transform.localPosition.z > 5 || transform.localPosition.z < -5;
        }
        private void GenerateTarget() {
            while (true) {
                var newPosition = new Vector3(Random.value * 8 - 4,
                                                  0.5f,
                                                  Random.value * 8 - 4);
                if (Vector3.Distance(transform.localPosition, newPosition) > 2f) {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }
        public override void AgentReset() {
            if (IsOutOfRegion()) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
            }

            GenerateTarget();
        }

        public override void CollectObservations() {
            // Calculate relative localPosition
            Vector3 relativePosition = Target.localPosition - transform.localPosition;

            // Relative localPosition
            AddVectorObs(relativePosition.x / 5);
            AddVectorObs(relativePosition.z / 5);

            // Distance to edges of platform
            AddVectorObs((transform.localPosition.x + 5) / 5);
            AddVectorObs((transform.localPosition.x - 5) / 5);
            AddVectorObs((transform.localPosition.z + 5) / 5);
            AddVectorObs((transform.localPosition.z - 5) / 5);

            if (!IsHardMode) {
                // Agent velocity
                AddVectorObs(rBody.velocity.x / 5);
                AddVectorObs(rBody.velocity.z / 5);
            }
        }

        public float speed = 10;

        public override void AgentAction(float[] vectorAction) {
            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
                SetReward(1.0f);
                Done();
            }
            else { // Time penalty
                SetReward(-0.01f);
            }

            // Fell off platform
            if (IsOutOfRegion()) {
                SetReward(-1.0f);
                Done();
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            rBody.AddForce(controlSignal * speed);
        }

        public override float[] Heuristic() {
            var action = new float[2];

            action[0] = Input.GetAxis("Horizontal");
            action[1] = Input.GetAxis("Vertical");
            return action;
        }
    }
}