using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IsThisDarkSouls
{
    public class CameraManager : MonoBehaviour
    {
        #region Variables
        public static CameraManager instance; // Singleton
        
        public bool lockedOn; // Possibly used later for a 'lockon' feature?

        [SerializeField] private float followSpeed = 10;
        [SerializeField] private float mouseSpeed = 4;
        // [SerializeField] private float controllerSpeed = 2;
        [SerializeField] private Transform target;
        public Transform lockOnTarget;

        public Transform pivotPoint; // Point in world space that the camera rotates from
        public Transform cameraTransform;

        private float turnSmoothing = 0.1f;
        public float minAngle = -35; // Minimum rotation on the Y axis
        public float maxAngle = 35; // Maximum rotation on the Y axis
        
        private float smoothX;
        private float smoothY;
        private float smoothXVelocity;
        private float smoothYVelocity;
        private float lookAngle; // Rotation on the X axis
        private float tiltAngle; // Rotation on the Y axis
        #endregion

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Performs set up references for variables needed for class methods.
        /// </summary>
        public void Initialse(Transform _target)
        {
            target = _target;
            cameraTransform = Camera.main.transform;
            pivotPoint = cameraTransform.parent;
        }

        /// <summary>
        /// Records user input necessary for camera controls
        /// </summary>
        public void Tick(float deltaTime)
        {
            // All of the below may change if a controller speed is introduced
            float horizontalAzis = Input.GetAxis("Mouse X");
            float verticalAxis = Input.GetAxis("Mouse Y");
            float speed = mouseSpeed; // Will definitely need to change if controller speed is introduced

            FollowTarget(deltaTime);
            HandleRotation(deltaTime, verticalAxis, horizontalAzis, speed);
        }

        /// <summary>
        /// Follows the target of the camera via lerping from the old position to the new, based on 'followSpeed'.
        /// </summary>
        private void FollowTarget(float deltaTime)
        {
            float speed = deltaTime * followSpeed;
            Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
            transform.position = targetPosition;
        }

        /// <summary>
        /// Controls the rotation of the camera based on the input of the player, smoothing the result before rotating the camera on the X & Y axis.
        /// </summary>
        private void HandleRotation(float deltaTime, float vertical, float horizontal, float speed)
        {
            if (turnSmoothing > 0) // If any smoothing has been set...
            {
                smoothX = Mathf.SmoothDamp(smoothX, horizontal, ref smoothXVelocity, turnSmoothing); // Smooths the transition between two values (velocitys)
                smoothY = Mathf.SmoothDamp(smoothY, vertical, ref smoothYVelocity, turnSmoothing);
            }
            else // Otherwise use raw input...
            {
                smoothX = horizontal;
                smoothY = vertical;
            }

            tiltAngle -= smoothY * speed; // Calculate the new Y axis
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle); // Clamp the Y axis between the minimum and maximum values
            pivotPoint.localRotation = Quaternion.Euler(tiltAngle, 0, 0); // Assign Y axis rotation to the camera

            if (lockedOn && lockOnTarget != null)
            {
                minAngle = 40;
                Vector3 targetDirection = lockOnTarget.position - transform.position;
                targetDirection.Normalize();

                if (targetDirection == Vector3.zero)
                {
                    targetDirection = transform.forward;
                }

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 10);
                lookAngle = transform.eulerAngles.y;

                return;
            }
            minAngle = -10;
            lookAngle += smoothX * speed; // Calculate the new X axis
            transform.rotation = Quaternion.Euler(0, lookAngle, 0); // Assign X axis rotation to the camera

            
        }
    }
}

