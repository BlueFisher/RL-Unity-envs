using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CopySettingsOverrides : MonoBehaviour {
    public GameObject AreaPrefab;
    public float CopyGap = 10;
    public int DefaultCopy = 1;
    protected List<GameObject> areas;
    protected int lastCopy = 1;

    private void Awake() {
        areas = new List<GameObject>();

        Academy.Instance.OnEnvironmentReset += Academy_OnEnvironmentReset;
    }

    private void Academy_OnEnvironmentReset() {
        int copy = (int)Academy.Instance.FloatProperties.GetPropertyWithDefault("copy", DefaultCopy);

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
