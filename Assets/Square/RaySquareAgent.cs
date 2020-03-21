using MLAgents.Sensors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Square {
    public class RaySquareAgent : SquareAgent {
        public override void CollectObservations(VectorSensor sensor) {
            m_ResetParams.SetProperty("force_reset", 0);

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
    }

}