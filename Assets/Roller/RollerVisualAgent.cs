using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;
using MLAgents.SideChannels;
using MLAgents.Sensors;

namespace Roller {
    public class RollerVisualAgent : RollerAgent {
        public override void CollectObservations(VectorSensor sensor) {
            if (sensor != null) {
                if (!IsHardMode) {
                    // Agent velocity
                    sensor.AddObservation(m_AgentRb.velocity.x / 5);
                    sensor.AddObservation(m_AgentRb.velocity.z / 5);
                }
            }
        }
    }
}