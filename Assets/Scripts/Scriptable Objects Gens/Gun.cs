using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.james168ma.Simpleton
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]

    public class Gun : ScriptableObject
    {
        new public string name;
        public int damage;
        public float firerate;
        public float bloom;
        public float recoil;
        public float kickback;
        public float aimSpeed;
        public GameObject prefab;
    }
}