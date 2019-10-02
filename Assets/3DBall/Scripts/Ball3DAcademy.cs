using UnityEngine;
using MLAgents;

namespace Ball3D {
    public class Ball3DAcademy : CopyAcademy {
        public override void AcademyReset() {
            Physics.gravity = new Vector3(0, -resetParameters["gravity"], 0);
        }

        public override void AcademyStep() {
        }
    }
}