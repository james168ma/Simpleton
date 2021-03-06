﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.james168ma.Simpleton
{
    public class Sway : MonoBehaviour
    {
        #region Variables

        public float intensity;
        public float smooth;
        public bool isMine;

        private Quaternion origin_rotation;

        #endregion
        

        #region MonoBehaviour Callbacks

        void Start()
        {
            origin_rotation = transform.localRotation;
        }

        void Update()
        {
            UpdateSway();
        }

        #endregion


        #region Private Methods

        private void UpdateSway()
        {
            // controls
            float t_x_mouse = Input.GetAxis("Mouse X"); // get how much the mouse is moving in the X direction
            float t_y_mouse = Input.GetAxis("Mouse Y");

            if(!isMine) // if not yours then don't sway with your mouse input
            {
                t_x_mouse = 0;
                t_y_mouse = 0;
            }

            // calculate target rotation
            Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
            Quaternion t_y_adj = Quaternion.AngleAxis(2 * intensity * t_y_mouse, Vector3.right);
            Quaternion target_rotation = origin_rotation * t_x_adj * t_y_adj;

            // rotate towards target rotation
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth);

        }

        #endregion
    }
}
