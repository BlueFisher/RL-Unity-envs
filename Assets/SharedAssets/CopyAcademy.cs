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
            int rowNum = Mathf.CeilToInt(Mathf.Sqrt(copy));

            for (int i = 0; i < rowNum; i++) {
                for (int j = 0; j < rowNum; j++) {
                    if (i == 0 && j == 0)
                        continue;
                    if (i * rowNum + j + 1 > copy) {
                        break;
                    }

                    GameObject area = Instantiate(AreaPrefab, new Vector3(i * CopyGap, 0, j * CopyGap), Quaternion.identity);
                    areas.Add(area);
                }
            }
        }
    }
}