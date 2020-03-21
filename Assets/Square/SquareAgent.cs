﻿using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;
using MLAgents.Sensors;
using MLAgents.SideChannels;

namespace Square {
    public class SquareAgent : Agent {
        protected Rigidbody m_AgentRb;
        protected Rigidbody m_TargetRb;
        protected FloatPropertiesChannel m_ResetParams;

        protected bool wallCollided = false;
        protected bool targetCollided = false;

        public Transform Target;
        public float SpawnRadius = 9;
        public float Speed = 2;
        public bool AvoidWall = false;

        public override void Initialize() {
            m_AgentRb = GetComponent<Rigidbody>();
            m_TargetRb = Target.GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.FloatProperties;
        }

        protected virtual void GenerateAgent() {
            transform.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                        0.5f,
                                                        SpawnRadius * (Random.value * 2 - 1));
            transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_AgentRb.velocity = Vector3.zero;
        }

        protected virtual void GenerateTarget() {
            Target.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                1f,
                                                SpawnRadius * (Random.value * 2 - 1));
            Target.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_TargetRb.velocity = Vector3.zero;
            m_TargetRb.angularVelocity = Vector3.zero;
        }

        public override void OnEpisodeBegin() {
            AvoidWall = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("avoid_wall", System.Convert.ToSingle(AvoidWall)));
            bool forceReset = System.Convert.ToBoolean(m_ResetParams.GetPropertyWithDefault("force_reset", 0));

            // Generate agent
            if (forceReset || (AvoidWall && wallCollided)) {
                GenerateAgent();
            }

            GenerateTarget();

            wallCollided = false;
            targetCollided = false;
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("wall")) {
                wallCollided = true;
            }
            else if (collision.gameObject.CompareTag("target")) {
                targetCollided = true;
            }
        }

        public override void CollectObservations(VectorSensor sensor) {
            m_ResetParams.SetProperty("force_reset", 0);

            sensor.AddObservation(Target.localPosition.x / 10f);
            sensor.AddObservation(Target.localPosition.z / 10f);
            sensor.AddObservation(transform.localPosition.x / 10f);
            sensor.AddObservation(transform.localPosition.z / 10f);

            // Agent forward direction
            sensor.AddObservation(transform.forward.x);
            sensor.AddObservation(transform.forward.z);

            // Agent velocity
            var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
            sensor.AddObservation(velocity.x);
            sensor.AddObservation(velocity.z);
        }

        public override void OnActionReceived(float[] vectorAction) {
            if (targetCollided) {
                SetReward(1.0f);
                EndEpisode();
            }
            else if (AvoidWall && wallCollided) {
                SetReward(-1.0f);
                EndEpisode();
            }
            else {
                AddReward(-1f / maxStep);
            }

            var dirToGo = transform.forward * vectorAction[0];
            var rotateDir = transform.up * vectorAction[1];
            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            m_AgentRb.AddForce(dirToGo * Speed, ForceMode.VelocityChange);
        }

        public override float[] Heuristic() {
            var action = new float[2];

            action[0] = Input.GetAxis("Vertical");
            action[1] = Input.GetAxis("Horizontal");
            return action;
        }
    }
}