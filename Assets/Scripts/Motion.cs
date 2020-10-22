using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.james168ma.FPSTutorial
{
    public class Motion : MonoBehaviour
    {
        public float speed;

        private Rigidbody rig;

        private void Start()
        {
            Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
        }

        // FixedUpdate because it is useful for physics especially if it will be multiplayer game for sync issues
        void FixedUpdate()
        {   
            // t_ for temporary
            float t_hmove = Input.GetAxisRaw("Horizontal"); // WASD -> A=-1, D=1
            float t_vmove = Input.GetAxisRaw("Vertical");   // WASD -> W=1, S=-1

            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            rig.velocity = transform.TransformDirection(t_direction) * speed * Time.deltaTime; // Time.deltaTime is the time between each frame
        }
    }
}
