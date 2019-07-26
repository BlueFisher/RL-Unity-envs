using MLAgents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CopyAcademy : Academy {
    public GameObject AreaPrefab;
    public float CopyGap = 10;
    protected List<GameObject> areas;
    protected int lastCopy = 1;

    public override void InitializeAcademy() {
        areas = new List<GameObject>();
    }
    public override void AcademyReset() {
        int copy = (int)resetParameters["copy"];
        if (copy > 1 && copy != lastCopy) {
            lastCopy = copy;
            foreach (var area in areas) {
                Destroy(area);
            }
            areas.Clear();
            for (int i = 0; i < copy - 1; i++) {
                GameObject area = Instantiate(AreaPrefab, new Vector3((i + 1) * CopyGap, 0, 0), Quaternion.identity) as GameObject;
                areas.Add(area);
            }
        }
    }
}