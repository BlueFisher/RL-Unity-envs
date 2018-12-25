using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace SimpleRoller {
    public class RollerDecision : Decision {
        public override float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory) {
            var action = new float[] { 0, 0 };
            action[0] = vectorObs[0] > 0 ? 1 : -1;
            action[1] = vectorObs[1] > 0 ? 1 : -1;
            return action;
        }
        public override List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory) {
            return new List<float>();
        }
    }
}
