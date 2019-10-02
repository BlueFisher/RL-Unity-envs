using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleRoller {
    public class ChasingTarget : MonoBehaviour {
        public Transform FloorTransform;
        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            var dis = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, Camera.main.transform.position.y - 0.5f));
            dis.x = Mathf.Clamp(dis.x,
                FloorTransform.position.x - FloorTransform.localScale.x * 5 + 1,
                FloorTransform.position.x + FloorTransform.localScale.x * 5 - 1);
            dis.z = Mathf.Clamp(dis.z,
                FloorTransform.position.z - FloorTransform.localScale.z * 5 + 1,
                FloorTransform.position.z + FloorTransform.localScale.z * 5 - 1);

            Vector3 speed = Vector3.zero;
            this.transform.position = Vector3.SmoothDamp(this.transform.position, dis, ref speed, 0.01f);
        }
    }
}