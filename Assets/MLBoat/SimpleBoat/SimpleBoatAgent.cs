using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace MLBoat {
    public class SimpleBoatAgent : Agent {
        public Transform Target;
        private AdvancedShipController controller;
        private SimpleBoatAcadamy acadamy;

        private int actionIdx = 0;
        private int rewardIdx = 0;

        public override void InitializeAgent() {
            acadamy = GameObject.Find("Academy").GetComponent<SimpleBoatAcadamy>();
            controller = GetComponent<AdvancedShipController>();
            controller.Initialize();
        }

        //private bool IsOutOfRegion() {
        //    float dis = (new Vector2(transform.localPosition.x, transform.localPosition.z)).magnitude;
        //    return dis > 38;
        //}

        public float TargetRadius = 35;
        private bool hitted = false;

        private void GenerateTarget() {
            while (true) {
                float angle = Random.value * Mathf.PI * 2;
                float radius = Random.value * TargetRadius;
                var newPosition = new Vector3(radius * Mathf.Cos(angle),
                                                  5f,
                                                  radius * Mathf.Sin(angle));

                if (Vector3.Distance(transform.localPosition, newPosition) > 6f) {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }

        public override void AgentReset() {
            if (!hitted) {
                //transform.rotation = new Quaternion(0, 0, 0, 0);
                transform.localPosition = Vector3.zero;
                controller.ShipRigidbody.angularVelocity = Vector3.zero;
                controller.ShipRigidbody.velocity = Vector3.zero;
            }

            GenerateTarget();
            hitted = false;

            actionIdx = (int)acadamy.resetParameters["action"];
            rewardIdx = (int)acadamy.resetParameters["reward"];
        }

        public override void CollectObservations() {
            AddVectorObs(transform.localPosition / 40);
            AddVectorObs(Target.localPosition / 40);
            AddVectorObs(controller.ShipRigidbody.velocity / 10);
            AddVectorObs(transform.forward);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (actionIdx == 0) {
                controller.Act(Mathf.Clamp(vectorAction[0], -1, 1), Mathf.Clamp(vectorAction[1], -1, 1));
            }
            else if (actionIdx == 1) {
                vectorAction[0] = vectorAction[0] * 0.5f + 0.5f;
                controller.Act(Mathf.Clamp(vectorAction[0], 0, 1), Mathf.Clamp(vectorAction[1], -1, 1));
            }

            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
            Vector3 toTarget = Target.localPosition - transform.localPosition;

            float velocityReward = Vector3.Dot(toTarget.normalized, controller.ShipRigidbody.velocity.normalized);
            float directionReward = Vector3.Dot(toTarget.normalized, transform.forward.normalized);

            if (distanceToTarget < 4f) {
                hitted = true;
                SetReward(1.0f);
                Done();
            }
            //else if (IsOutOfRegion()) {
            //    SetReward(-1.0f);
            //    Done();
            //}
            else {
                if (!IsDone()) {
                    if (rewardIdx == 0) {
                        SetReward(-0.01f + 0.005f * velocityReward + 0.005f * directionReward);
                    }
                    else if (rewardIdx == 1) {
                        SetReward(-0.01f);
                    }
                }
            }
        }
    }
}