using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

namespace Crawler {
    public class CrawlerAcademy : Academy {
        public GameObject AreaPrefab;
        public float CopyGap = 10;
        private List<GameObject> Areas;
        private int LastCopy = 1;

        public override void InitializeAcademy() {
            Monitor.verticalOffset = 1f;
            Physics.defaultSolverIterations = 12;
            Physics.defaultSolverVelocityIterations = 12;
            Time.fixedDeltaTime = 0.01333f; // (75fps). default is .2 (60fps)
            Time.maximumDeltaTime = .15f; // Default is .33

            Areas = new List<GameObject>();
        }

        public override void AcademyReset() {
            int copy = (int)resetParameters["copy"];
            if (copy > 1 && copy != LastCopy) {
                LastCopy = copy;
                foreach (var area in Areas) {
                    Destroy(area);
                }
                Areas.Clear();
                for (int i = 0; i < copy - 1; i++) {
                    GameObject area = Instantiate(AreaPrefab, new Vector3((i + 1) * CopyGap, 0, 0), Quaternion.identity) as GameObject;
                    Areas.Add(area);
                }
            }
        }

        public override void AcademyStep() {
        }
    }

}
