﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace Crawler {
    [RequireComponent(typeof(JointDriveController))] // Required to set joint forces
    public class CrawlerAgent : Agent {
        [Header("Target To Walk Towards")]
        [Space(10)]
        public Transform target;
        public Transform ground;
        public float targetSpawnRadius;

        [Header("Body Parts")] [Space(10)] public Transform body;
        public Transform leg0Upper;
        public Transform leg0Lower;
        public Transform leg1Upper;
        public Transform leg1Lower;
        public Transform leg2Upper;
        public Transform leg2Lower;
        public Transform leg3Upper;
        public Transform leg3Lower;

        [Header("Joint Settings")] [Space(10)] JointDriveController jdController;
        Vector3 dirToTarget;
        float movingTowardsDot;
        float facingDot;

        [Header("Reward Functions To Use")]
        [Space(10)]
        public bool rewardMovingTowardsTarget; // Agent should move towards target

        public bool rewardFacingTarget; // Agent should face the target
        public bool rewardUseTimePenalty; // Hurry up

        [Header("Foot Grounded Visualization")]
        [Space(10)]
        public bool useFootGroundedVisualization;

        public MeshRenderer foot0;
        public MeshRenderer foot1;
        public MeshRenderer foot2;
        public MeshRenderer foot3;
        public Material groundedMaterial;
        public Material unGroundedMaterial;
        bool isNewDecisionStep;
        int currentDecisionStep;

        private CrawlerAcademy academy;
        private int rewardIdx = 0;
        private bool isStatic = true;

        public override void InitializeAgent() {
            academy = GameObject.Find("Academy").GetComponent<CrawlerAcademy>();

            jdController = GetComponent<JointDriveController>();
            currentDecisionStep = 1;

            //Setup each body part
            jdController.SetupBodyPart(body);
            jdController.SetupBodyPart(leg0Upper);
            jdController.SetupBodyPart(leg0Lower);
            jdController.SetupBodyPart(leg1Upper);
            jdController.SetupBodyPart(leg1Lower);
            jdController.SetupBodyPart(leg2Upper);
            jdController.SetupBodyPart(leg2Lower);
            jdController.SetupBodyPart(leg3Upper);
            jdController.SetupBodyPart(leg3Lower);
        }

        /// <summary>
        /// We only need to change the joint settings based on decision freq.
        /// </summary>
        public void IncrementDecisionTimer() {
            if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions
                || agentParameters.numberOfActionsBetweenDecisions == 1) {
                currentDecisionStep = 1;
                isNewDecisionStep = true;
            }
            else {
                currentDecisionStep++;
                isNewDecisionStep = false;
            }
        }

        /// <summary>
        /// Add relevant information on each body part to observations.
        /// </summary>
        public void CollectObservationBodyPart(BodyPart bp) {
            var rb = bp.rb;
            AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Whether the bp touching the ground
            AddVectorObs(rb.velocity);
            AddVectorObs(rb.angularVelocity);

            if (bp.rb.transform != body) {
                Vector3 localPosRelToBody = body.InverseTransformPoint(rb.position);
                AddVectorObs(localPosRelToBody);
                AddVectorObs(bp.currentXNormalizedRot); // Current x rot
                AddVectorObs(bp.currentYNormalizedRot); // Current y rot
                AddVectorObs(bp.currentZNormalizedRot); // Current z rot
                AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
            }
        }

        public override void CollectObservations() {
            jdController.GetCurrentJointForces();
            // Normalize dir vector to help generalize
            AddVectorObs(dirToTarget.normalized);

            // Forward & up to help with orientation
            AddVectorObs(body.transform.position.y);
            AddVectorObs(body.forward);
            AddVectorObs(body.up);
            foreach (var bodyPart in jdController.bodyPartsDict.Values) {
                CollectObservationBodyPart(bodyPart);
            }
        }

        private bool resetCrawler = true;

        public override void AgentAction(float[] vectorAction, string textAction) {
            // Update pos to target
            dirToTarget = target.position - jdController.bodyPartsDict[body].rb.position;

            // If enabled the feet will light up green when the foot is grounded.
            // This is just a visualization and isn't necessary for function
            if (useFootGroundedVisualization) {
                foot0.material = jdController.bodyPartsDict[leg0Lower].groundContact.touchingGround
                    ? groundedMaterial
                    : unGroundedMaterial;
                foot1.material = jdController.bodyPartsDict[leg1Lower].groundContact.touchingGround
                    ? groundedMaterial
                    : unGroundedMaterial;
                foot2.material = jdController.bodyPartsDict[leg2Lower].groundContact.touchingGround
                    ? groundedMaterial
                    : unGroundedMaterial;
                foot3.material = jdController.bodyPartsDict[leg3Lower].groundContact.touchingGround
                    ? groundedMaterial
                    : unGroundedMaterial;
            }

            // Joint update logic only needs to happen when a new decision is made
            if (isNewDecisionStep) {
                // The dictionary with all the body parts in it are in the jdController
                var bpDict = jdController.bodyPartsDict;

                int i = -1;
                // Pick a new target joint rotation
                bpDict[leg0Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
                bpDict[leg1Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
                bpDict[leg2Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
                bpDict[leg3Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
                bpDict[leg0Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
                bpDict[leg1Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
                bpDict[leg2Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
                bpDict[leg3Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);

                // Update joint strength
                bpDict[leg0Upper].SetJointStrength(vectorAction[++i]);
                bpDict[leg1Upper].SetJointStrength(vectorAction[++i]);
                bpDict[leg2Upper].SetJointStrength(vectorAction[++i]);
                bpDict[leg3Upper].SetJointStrength(vectorAction[++i]);
                bpDict[leg0Lower].SetJointStrength(vectorAction[++i]);
                bpDict[leg1Lower].SetJointStrength(vectorAction[++i]);
                bpDict[leg2Lower].SetJointStrength(vectorAction[++i]);
                bpDict[leg3Lower].SetJointStrength(vectorAction[++i]);
            }

            // touch target
            foreach (var bodyPart in jdController.bodyPartsDict.Values) {
                if (bodyPart.targetContact && !IsDone() && bodyPart.targetContact.touchingTarget) {
                    resetCrawler = false;
                    bodyPart.targetContact.touchingTarget = false;
                    SetReward(1f);
                    Done();

                    foreach (var t in jdController.bodyPartsDict.Values)
                        if (t.targetContact && t.targetContact.touchingTarget)
                            t.targetContact.touchingTarget = false;

                    break;
                }
            }

            // Set reward for this step according to mixture of the following elements.
            if (!IsDone()) {
                float reward = 0f;

                if (rewardIdx % 2 == 1)
                    reward += RewardFunctionMovingTowards();

                if (rewardIdx / 10 % 2 == 1)
                    reward += RewardFunctionFacingTarget();

                if (rewardIdx / 100 % 2 == 1)
                    reward += RewardFunctionTimePenalty();

                SetReward(reward);
            }

            IncrementDecisionTimer();
        }

        /// <summary>
        /// Reward moving towards target & Penalize moving away from target.
        /// </summary>
        float RewardFunctionMovingTowards() {
            movingTowardsDot = Vector3.Dot(jdController.bodyPartsDict[body].rb.velocity, dirToTarget.normalized);
            return 0.03f * movingTowardsDot;
        }

        /// <summary>
        /// Reward facing target & Penalize facing away from target
        /// </summary>
        float RewardFunctionFacingTarget() {
            facingDot = Vector3.Dot(dirToTarget.normalized, body.forward);
            return 0.01f * facingDot;
        }

        /// <summary>
        /// Existential penalty for time-contrained tasks.
        /// </summary>
        float RewardFunctionTimePenalty() {
            return -0.001f;
        }

        /// <summary>
        /// Moves target to a random position within specified radius.
        /// </summary>
        public void GenerateTarget() {
            while (true) {
                float angle = Random.value * Mathf.PI * 2;
                float radius = Random.value * targetSpawnRadius;
                var newPosition = new Vector3(radius * Mathf.Cos(angle),
                                                  1f,
                                                  radius * Mathf.Sin(angle));

                newPosition += ground.position;

                if (Vector3.Distance(body.transform.position, newPosition) > 10f) {
                    target.position = newPosition;
                    break;
                }
            }
        }

        /// <summary>
        /// Loop over body parts and reset them to initial conditions.
        /// </summary>
        public override void AgentReset() {
            rewardIdx = (int)academy.resetParameters["reward"];
            isStatic = academy.resetParameters["static"] != 0f;

            if (isStatic || resetCrawler) {
                if (dirToTarget != Vector3.zero) {
                    transform.rotation = Quaternion.LookRotation(dirToTarget);
                }
                foreach (var bodyPart in jdController.bodyPartsDict.Values) {
                    bodyPart.Reset(bodyPart);
                }
            }
            else {
                resetCrawler = true;
            }

            if (!isStatic)
                GenerateTarget();

            isNewDecisionStep = true;
            currentDecisionStep = 1;
        }
    }
}
