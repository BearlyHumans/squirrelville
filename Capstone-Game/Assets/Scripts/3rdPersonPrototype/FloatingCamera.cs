using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class FloatingCamera : MonoBehaviour
    {

        public Transform horizontalPivot;
        public CameraSettings settings = new CameraSettings();

        private float cameraAngle = 0;
        private Vector3 velocity = Vector3.zero;
        private float speed = 0;

        public void UpdCamera(Transform charTransform, Rigidbody RB)
        {
            //CAMERA TRANSLATION
            if (settings.lockToPlayer)
                transform.position = charTransform.position;
            else
            {
                float playerDist = Vector3.Distance(transform.position, charTransform.position);
                if (playerDist >= settings.maxDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, charTransform.position, playerDist - settings.maxDistance);
                    speed = playerDist - settings.maxDistance;
                }
                else
                {
                    if (settings.useSpeedNotVelocity)
                    {
                        speed = Mathf.MoveTowards(speed, RB.velocity.magnitude, settings.acceleration * Time.deltaTime);
                        transform.position = Vector3.MoveTowards(transform.position, charTransform.position, speed * Time.deltaTime);
                    }
                    else
                    {
                        velocity = Vector3.MoveTowards(velocity, RB.velocity, settings.acceleration * Time.deltaTime);
                        transform.position += velocity * Time.deltaTime;
                    }
                }

            }


            //CAMERA ROTATION
            cameraAngle = cameraAngle - Input.GetAxis("Mouse Y") * settings.mouseSense;

            Quaternion NewRot = new Quaternion();
            NewRot.eulerAngles = new Vector3(cameraAngle, horizontalPivot.localRotation.y, 0);
            horizontalPivot.localRotation = NewRot;

            float rotationX = -Input.GetAxis("Mouse X") * settings.mouseSense;
            transform.localRotation *= Quaternion.AngleAxis(rotationX, Vector3.down);

            //CAMERA CLAMPING/COLLITIONS
        }

        [System.Serializable]
        public class CameraSettings
        {
            [Header("Camera Settings")]
            [Tooltip("If true the cameras position will always be the same as the players (invalidates all settings below).")]
            public bool lockToPlayer = false;
            [Tooltip("If true the camera will build up a speed, which will allow it to change directions quickly so long as it doesn't slow down beforehand.")]
            public bool useSpeedNotVelocity = false;
            [Tooltip("Rate at which the cameras speed will increase to keep up with the player.")]
            public float acceleration = 1;
            [Tooltip("Distance from the player at which the camera will forcefully keep up (ignoring acceleration).")]
            public float maxDistance = 1;
            public float mouseSense = 1;
        }
    }
}
