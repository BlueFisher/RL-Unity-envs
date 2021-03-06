﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;

public class CopySettingsOverrides : MonoBehaviour {
    public GameObject AreaPrefab;
    public float CopyGap = 10;
    public int DefaultNAgents = 1;

    private void Awake() {
        int nAgents = DefaultNAgents;

        List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        int index = commandLineArgs.IndexOf("--n_agents");
        if (index != -1) {
            nAgents = Convert.ToInt32(commandLineArgs[index + 1]);
        }

        if (nAgents > 1) {
            int rowNum = Mathf.CeilToInt(Mathf.Sqrt(nAgents));

            for (int i = 0; i < rowNum; i++) {
                for (int j = 0; j < rowNum; j++) {
                    if (i == 0 && j == 0)
                        continue;
                    if (i * rowNum + j + 1 > nAgents) {
                        break;
                    }

                    int mid = rowNum / 2;
                    if (j <= mid)
                        Instantiate(AreaPrefab, new Vector3(j * CopyGap, 0, i * CopyGap), Quaternion.identity);
                    else
                        Instantiate(AreaPrefab, new Vector3((mid - j) * CopyGap, 0, i * CopyGap), Quaternion.identity);
                }
            }
        }
    }
}
