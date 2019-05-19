using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace MLBoat {
    public class SimpleBoatAgent : Agent {
        public Transform Target;
        private AdvancedShipController controller;
        private SimpleBoatAcadamy acadamy;

        private int action_idx = 0;
        private int reward_idx = 0;

        public override void InitializeAgent() {
            acadamy = GameObject.Find("Acadamy").GetComponent<SimpleBoatAcadamy>();
            controller = GetComponent<AdvancedShipController>();
            controller.Initialize();
        }

        private bool IsOutOfRegion() {
            float dis = (new Vector2(transform.localPosition.x, transform.localPosition.z)).magnitude;
            return dis > 38;
        }

        private void GenerateTarget() {
            while (true) {
                float angle = Random.value * Mathf.PI * 2;
                float radius = Random.value * 35;
                var newPosition = new Vector3(radius * Mathf.Cos(angle),
                                                  2f,
                                                  radius * Mathf.Sin(angle));

                if (Vector3.Distance(transform.localPosition, newPosition) > 6f) {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }

        public override void AgentReset() {
            if (IsOutOfRegion()) {
                //transform.rotation = new Quaternion(0, 0, 0, 0);
                transform.localPosition = Vector3.zero;
                controller.ShipRigidbody.angularVelocity = Vector3.zero;
                controller.ShipRigidbody.velocity = Vector3.zero;
            }

            GenerateTarget();

            action_idx = (int)acadamy.resetParameters["action"];
            reward_idx = (int)acadamy.resetParameters["reward"];
        }

        public override void CollectObservations() {
            AddVectorObs(transform.localPosition / 40);
            AddVectorObs(Target.localPosition / 40);
            AddVectorObs(controller.ShipRigidbody.velocity / 10);
            AddVectorObs(transform.forward);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (action_idx == 0) {
                controller.Act(Mathf.Clamp(vectorAction[0], -1, 1), Mathf.Clamp(vectorAction[1], -1, 1));
            }
            else if (action_idx == 1) {
                vectorAction[0] = vectorAction[0] * 0.5f + 0.5f;
                controller.Act(Mathf.Clamp(vectorAction[0], 0, 1), Mathf.Clamp(vectorAction[1], -1, 1));
            }


            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
            Vector3 toTarget = Target.localPosition - transform.localPosition;

            float velocityReward = Vector3.Dot(toTarget.normalized, controller.ShipRigidbody.velocity.normalized);
            float directionReward = Vector3.Dot(toTarget.normalized, transform.forward.normalized);

            if (distanceToTarget < 4f) {
                SetReward(1.0f);
                Done();
            }
            else if (IsOutOfRegion()) {
                SetReward(-1.0f);
                Done();
            }
            else {
                if (!IsDone()) {
                    if (reward_idx == 0) {
                        SetReward(-0.01f + 0.005f * velocityReward + 0.005f * directionReward);
                    }
                    else if (reward_idx == 1) {
                        SetReward(-0.01f);
                    }
                }
            }
        }
    }
}