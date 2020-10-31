﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.james168ma.Simpleton
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]

    public class Gun : ScriptableObject
    {

        #region Variables

        new public string name;
        public int damage;
        public int ammo;
        public int clipsize;
        public float firerate;
        public float bloom;
        public float recoil;
        public float kickback;
        public float aimSpeed;
        public float reload;
        public GameObject prefab;

        private int stash; // current ammo
        private int clip;  // current bullets in clip

        #endregion


        #region Public Methods

        public void Initialize()
        {
            stash = ammo;
            clip = clipsize;
        }

        public bool FireBullet()
        {
            if(clip > 0)
            {
                clip--;
                return true;
            }
            else return false;
        }

        public void Reload()
        {
            // clip reload math logic
            stash += clip;
            clip = Mathf.Min(clipsize, stash);
            stash -= clip;
        }

        public int GetStash() { return stash; }
        public int GetClip() { return clip; }

        #endregion
    }
}