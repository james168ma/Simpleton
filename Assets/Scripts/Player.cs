using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

namespace Com.james168ma.Simpleton
{
    public class Player : MonoBehaviourPunCallbacks
    {
        #region Variables

        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public float lengthOfSlide;
        public float slideModifier;
        public int maxHealth;
        public Camera normalCam;
        public GameObject cameraParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public LayerMask ground;

        private Transform uiHealthbar;
        private Text uiAmmo;

        private Rigidbody rig;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;
        private Vector3 weaponParentCurrentPosition;

        private float movementCounter;
        private float idleCounter;

        private float baseFOV;
        private float sprintFOVModifier = 1.5f;
        private Vector3 origin;

        private int currentHealth;

        private Manager manager;
        private Weapon weapon;

        private bool sliding;
        private float slideTime;
        private Vector3 slideDirection;

        private GameObject eyes;

        #endregion


        #region Monobehavior Callbacks
        
        private void Start()
        {
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            weapon = GetComponent<Weapon>();
            eyes = GameObject.Find("Design/Eyes");
            currentHealth = maxHealth;

            // if it is your camera, enable it for you
            cameraParent.SetActive(photonView.IsMine);

            if(!photonView.IsMine) // if its not your player then make them a regular player
            {
                gameObject.layer = 11;
            }

            baseFOV = normalCam.fieldOfView;
            origin = normalCam.transform.localPosition;

            if(Camera.main) Camera.main.enabled = false;

            rig = GetComponent<Rigidbody>();
            weaponParentOrigin = weaponParent.localPosition;
            weaponParentCurrentPosition = weaponParentOrigin;

            if(photonView.IsMine)
            {
                uiHealthbar = GameObject.Find("HUD/Health/Bar").transform;
                uiAmmo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
                RefreshHealthBar();
                eyes.SetActive(false); // disable your eyes so they don't get in the way
            }
        }

        private void Update()
        {
            if(!photonView.IsMine) return; // if it isn't your player then skip

            // Axis
            float t_hmove = Input.GetAxisRaw("Horizontal"); // WASD -> A=-1, D=1
            float t_vmove = Input.GetAxisRaw("Vertical");   // WASD -> W=1, S=-1

            // Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); // left or right shift
            bool jump = Input.GetKeyDown(KeyCode.Space);

            // States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.2f, ground); // if the ground is 0.2f below our unity character
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && (t_vmove > 0) && !isJumping && isGrounded; // input on the vertical axis > 0 means moving forward (holding w)

            // Jumping
            if(isJumping) 
            {
                rig.AddForce(Vector3.up * jumpForce);
            }

            if(Input.GetKeyDown(KeyCode.U)) // just for testing purposes
            {
                TakeDamage(100);
            }

            // Headbob
            if (sliding) { }
            else if(t_hmove == 0 && t_vmove == 0) // idle
            {
                Headbob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if(!isSprinting) // walk
            {
                Headbob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * 3;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else // sprint
            {
                Headbob(movementCounter, 0.15f, 0.075f);
                movementCounter += Time.deltaTime * 7;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }

            // UI Refresh
            RefreshHealthBar();
            weapon.RefreshAmmo(uiAmmo);
        }

        // FixedUpdate because it is useful for physics especially if it will be multiplayer game for sync issues
        void FixedUpdate()
        {   
            if(!photonView.IsMine) return;
            
            // t_ for temporary
            // Axis
            float t_hmove = Input.GetAxisRaw("Horizontal"); // WASD -> A=-1, D=1
            float t_vmove = Input.GetAxisRaw("Vertical");   // WASD -> W=1, S=-1\


            // Controls
            bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); // left or right shift
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool slide = Input.GetKey(KeyCode.C);


            // States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.2f, ground); // if the ground is 0.2f below our unity character
            bool isJumping = jump && isGrounded;
            bool isSprinting = sprint && (t_vmove > 0) && !isJumping && isGrounded; // input on the vertical axis > 0 means moving forward (holding w)
            bool isSliding = isSprinting && slide && !sliding;


            // Movement
            Vector3 t_direction = Vector3.zero;
            float t_adjustedSpeed = speed;

            if(!sliding)
            {
                t_direction = new Vector3(t_hmove, 0, t_vmove);
                t_direction.Normalize();
                t_direction = transform.TransformDirection(t_direction);

                if(isSprinting) t_adjustedSpeed *= sprintModifier;
            }
            else
            {
                t_direction = slideDirection;
                t_adjustedSpeed *= slideModifier;
                slideTime -= Time.deltaTime;
                if(slideTime <= 0) 
                {
                    sliding = false;
                    weaponParentCurrentPosition += Vector3.up * 0.5f;
                }
            }

            
            Vector3 t_targetVelocity =  t_direction * t_adjustedSpeed * Time.deltaTime; // Time.deltaTime is the time between each frame
            t_targetVelocity.y = rig.velocity.y;
            rig.velocity = t_targetVelocity;

            // Sliding
            if(isSliding)
            {
                sliding = true;
                slideDirection = t_direction;
                slideTime = lengthOfSlide;
                // adjust camera
                weaponParentCurrentPosition += Vector3.down * 0.5f;
            }


            // Camera Stuff
            if(sliding)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier * 1.25f, Time.deltaTime * 8f);
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin + Vector3.down * 0.5f, Time.deltaTime * 6f);
            }
            else
            {
                if(isSprinting) normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
                else normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);

                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, origin, Time.deltaTime * 6f);
            }
        }

        #endregion


        #region Public Methods

        public void TakeDamage(int p_damage)
        {
            if(photonView.IsMine) // only wanna take away damage on yours
            {
                currentHealth -= p_damage;
                RefreshHealthBar();

                if(currentHealth <= 0)
                {
                    manager.Spawn();
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        #endregion


        #region Private Methods

        void Headbob(float p_z, float p_x_intensity, float p_y_intensity)
        {
            targetWeaponBobPosition = weaponParentCurrentPosition + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z * 2) * p_y_intensity, 0);
        }

        void RefreshHealthBar()
        {
            float t_healthRatio = (float)currentHealth / (float)maxHealth;
            uiHealthbar.localScale = Vector3.Lerp(uiHealthbar.localScale, new Vector3(t_healthRatio, 1, 1), Time.deltaTime * 8f);
        }

        #endregion
    }
}
