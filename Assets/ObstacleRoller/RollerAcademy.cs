﻿using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ObstacleRoller {
    public class RollerAcademy : Academy {
        public GameObject AreaPrefab;
        private List<GameObject> Areas;
        private int LastCopy = 1;
        public override void InitializeAcademy() {
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
                    GameObject area = Instantiate(AreaPrefab, new Vector3((i + 1) * 11, 0, 0), Quaternion.identity) as GameObject;
                    Areas.Add(area);
                }
            }
        }
    }
}
