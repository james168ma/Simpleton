﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.james168ma.Simpleton
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables

        public Gun[] loadout;
        public Transform weaponParent;
        public GameObject bulletholePrefab;
        public LayerMask canBeShot;

        private float currentCooldown;
        private int currentIndex;
        private GameObject currentWeapon;

        #endregion

        #region MonoBehaviour Callbacks

        void Update()
        {
            if(!photonView.IsMine) return;

            if(Input.GetKeyDown(KeyCode.Alpha1)) // press 1 to equip weapon 0
            {
                photonView.RPC("Equip", RpcTarget.All, 0); // send to all computers on the server that you equipped
            }

            if(currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1)); // 1 is right mouse

                if(Input.GetMouseButtonDown(0) && currentCooldown <= 0) // left clicked
                {
                    photonView.RPC("Shoot", RpcTarget.All);
                }

                // weapon position elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }
        }

        void FixedUpdate()
        {
            if(!photonView.IsMine) return;

            // cooldown for shooting
            if(currentCooldown > 0) 
            {
                currentCooldown -= Time.deltaTime;
            }
        }

        #endregion

        #region Private Methods

        [PunRPC] // make Equip an RPC (Remote Procedure Call)
        void Equip(int p_ind)
        {
            if(currentWeapon != null)
            {
                Destroy(currentWeapon);
            }

            currentIndex = p_ind;

            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().enabled = photonView.IsMine;

            currentWeapon = t_newWeapon;
        }

        void Aim(bool p_isAiming) 
        {
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if(p_isAiming)
            {
                // aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            else
            {
                // hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }

        }

        [PunRPC]
        void Shoot()
        {
            Transform t_spawn = transform.Find("Cameras/Normal Camera");

            // bloom
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();
            
            // cooldown
            currentCooldown = loadout[currentIndex].firerate;

            // raycast
            RaycastHit t_hit = new RaycastHit();
            if(Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal * 1f);
                Destroy(t_newHole, 5f);

                if(photonView.IsMine)
                {
                    // shooting other player on network
                    if(t_hit.collider.gameObject.layer == 11)
                    {
                        // RPC Call to Damage Player Goes Here
                    }
                }
            }

            // gun fx
            currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
        }

        #endregion
    }
}
