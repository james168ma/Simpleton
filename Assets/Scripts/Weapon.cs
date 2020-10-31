using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
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

        private bool isReloading;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            foreach(Gun g in loadout) g.Initialize(); // initialize the guns
            photonView.RPC("Equip", RpcTarget.AllBuffered, 0);
        }

        void Update()
        {
            if(currentWeapon != null)
            {
                // weapon position elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }

            if(!photonView.IsMine) return;

            if(Input.GetKeyDown(KeyCode.Alpha1)) // press 1 to equip weapon 0
            {
                photonView.RPC("Equip", RpcTarget.AllBuffered, 0); // send to all computers on the server that you equipped
            }

            if(currentWeapon != null)
            {
                Aim(Input.GetMouseButton(1)); // 1 is right mouse

                if(Input.GetMouseButtonDown(0) && currentCooldown <= 0) // left clicked
                {
                    if(loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                    else StartCoroutine(Reload(loadout[currentIndex].reload));
                }

                if(Input.GetKeyDown(KeyCode.R)) StartCoroutine(Reload(loadout[currentIndex].reload)); // reload with R
            }
        }

        void FixedUpdate()
        {
            if(!photonView.IsMine) return;

            // cooldown for shooting
            if(currentWeapon != null && currentCooldown > 0) 
            {
                currentCooldown -= Time.deltaTime;
            }
        }

        #endregion

        #region Private Methods

        IEnumerator Reload(float p_wait)
        {
            isReloading = true;
            currentWeapon.SetActive(false);

            yield return new WaitForSeconds(p_wait);

            loadout[currentIndex].Reload();
            currentWeapon.SetActive(true);
            isReloading = false;
        }

        [PunRPC] // make Equip an RPC (Remote Procedure Call)
        void Equip(int p_ind)
        {
            if(currentWeapon != null)
            {
                if(isReloading) StopCoroutine("Reload");
                Destroy(currentWeapon);
            }

            currentIndex = p_ind;

            GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

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
                        // RPC Call to Damage Player
                        t_hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
                    }
                }
            }

            // gun fx
            currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
        }

        [PunRPC]
        private void TakeDamage (int p_damage)
        {
            GetComponent<Player>().TakeDamage(p_damage);
        }

        #endregion


        #region Public Methods

        public void RefreshAmmo (Text p_text)
        {
            int t_clip = loadout[currentIndex].GetClip();
            int t_stash = loadout[currentIndex].GetStash();

            p_text.text = t_clip.ToString("D2") + " / " + t_stash.ToString("D2");
        }

        #endregion
    }
}
