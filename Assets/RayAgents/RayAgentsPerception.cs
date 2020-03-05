using System;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using MLAgents.Sensor;

namespace RayAgents {
    public class RayAgentsPerception : RayPerceptionSensorComponent3D {
        bool needWrite = false;
        List<LineRenderer> lines;

        private void FixedUpdate() {
            if (m_RaySensor?.debugDisplayInfo == null)
                return;

            var debugInfo = m_RaySensor.debugDisplayInfo;
            if (debugInfo.rayInfos == null)
                needWrite = true;

            if (needWrite)
                m_RaySensor.Write(null);

            var alpha = Mathf.Pow(.5f, debugInfo.age);
            if (lines == null) {
                lines = new List<LineRenderer>();
                for (int i = 0; i < debugInfo.rayInfos.Length; i++) {
                    GameObject g = new GameObject(i.ToString());
                    LineRenderer line = g.AddComponent<LineRenderer>();
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startWidth = 0.02f;
                    lines.Add(line);
                }
            }
            for (int i = 0; i < debugInfo.rayInfos.Length; i++) {
                var rayInfo = debugInfo.rayInfos[i];
                var line = lines[i];

                var startPositionWorld = rayInfo.worldStart;
                var endPositionWorld = rayInfo.worldEnd;
                if (!useWorldPositions) {
                    startPositionWorld = transform.TransformPoint(rayInfo.localStart);
                    endPositionWorld = transform.TransformPoint(rayInfo.localEnd);
                }
                var rayDirection = endPositionWorld - startPositionWorld;
                rayDirection *= rayInfo.hitFraction;

                var lerpT = rayInfo.hitFraction * rayInfo.hitFraction;
                var color = Color.Lerp(rayHitColor, rayMissColor, lerpT);
                color.a *= alpha;

                line.startColor = color;
                line.endColor = color;

                line.SetPosition(0, startPositionWorld);
                line.SetPosition(1, startPositionWorld + rayDirection);
            }
        }
    }
}
