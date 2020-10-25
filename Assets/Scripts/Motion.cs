using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.james168ma.FPSTutorial
{
    public class Motion : MonoBehaviour
    {
        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public Camera normalCam;
        public Transform groundDetector;
        public LayerMask ground;

        private Rigidbody rig;

        private float baseFOV;
        private float sprintFOVModifier = 1.5f;

        private void Start()
        {
            baseFOV = normalCam.fieldOfView;
            Camera.main.enabled = false;
            rig = GetComponent<Rigidbody>();
        }

        // FixedUpdate because it is useful for physics especially if it will be multiplayer game for sync issues
        void FixedUpdate()
        {   
            // t_ for temporary
            // Axis
            float t_hmove = Input.GetAxisRaw("Horizontal"); // WASD -> A=-1, D=1
            float t_vmove = Input.GetAxisRaw("Vertical");   // WASD -> W=1, S=-1\


            // Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); // left or right shift
            bool jump = Input.GetKey(KeyCode.Space);


            // States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground); // if the ground is 0.1f below our unity character
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded; // input on the vertical axis > 0 means moving forward (holding w)


            // Jumping
            if(isJumping) 
            {
                rig.AddForce(Vector3.up * jumpForce);
            }

            // Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize();

            float t_adjustedSpeed = speed;
            if(isSprinting) t_adjustedSpeed *= sprintModifier;

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime; // Time.deltaTime is the time between each frame
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;


            // Field of View
            if(isSprinting) 
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else
            { 
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
            }
        }
    }
}
