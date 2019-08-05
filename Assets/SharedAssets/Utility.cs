using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility {
    public static float GetXZDistance(Vector3 a, Vector3 b) {
        Vector2 tmpA = new Vector2(a.x, a.z);
        Vector2 tmpB = new Vector2(b.x, b.z);
        return Vector2.Distance(tmpA, tmpB);
    }
}
