using UnityEngine;
using MLAgents;

namespace Ball3D {
    public class Ball3DAcademy : CopyAcademy {
        public override void AcademyReset() {
            base.AcademyReset();
            FloatProperties.RegisterCallback("gravity", f => { Physics.gravity = new Vector3(0, -f, 0); });
        }
    }
}