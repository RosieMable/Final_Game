using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZaldensGambit
{
    public class CameraManager : MonoBehaviour
    {
        #region Variables
        public static CameraManager instance; // Singleton
        
        public bool lockedOn;

        [SerializeField] private float followSpeed = 10;
        [SerializeField] private float mouseSpeed = 4;
        // [SerializeField] private float controllerSpeed = 2;
        [SerializeField] private Transform target;
        public Transform lockOnTarget;
        public GameObject lockOnPrefab;

        public Transform pivotPoint; // Point in world space that the camera rotates from
        public Transform cameraTransform;

        private float turnSmoothing = 0.1f;
        private float originalMinAngle, originalMaxAngle;
        public float minAngle = -20; // Minimum rotation on the Y axis
        public float maxAngle = 60; // Maximum rotation on the Y axis
        public float lockOnMinAngle = 60;
        public float lockOnMaxAngle = 70;
        public float defaultZDistance = -4;
        private float currentZ;
        public float zSpeed = 5;
        public float minimumZDistance = -1.5f;
        
        private float smoothX;
        private float smoothY;
        private float smoothXVelocity;
        private float smoothYVelocity;
        private float lookAngle; // Rotation on the X axis
        private float tiltAngle; // Rotation on the Y axis
        #endregion

        private void Awake()
        {
            // Initialise Singleton
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
        /// Performs initial set up references for variables needed for class methods.
        /// </summary>
        public void Initialse(Transform _target)
        {
            ToggleCursorVisibleState(false);
            target = _target;
            cameraTransform = Camera.main.transform;
            pivotPoint = cameraTransform.parent;
            originalMaxAngle = maxAngle;
            originalMinAngle = minAngle;
            currentZ = defaultZDistance;
            lockOnPrefab.transform.SetParent(this.transform);
            lockOnPrefab.transform.position = Vector3.zero;
            lockOnPrefab.SetActive(false);

        }

        /// <summary>
        /// Records user input necessary for camera controls.
        /// </summary>
        public void Tick(float deltaTime)
        {
            // All of the below may change if a controller speed is introduced
            float horizontalAzis = Input.GetAxis("Mouse X");
            float verticalAxis = Input.GetAxis("Mouse Y");
            float speed = mouseSpeed; // Will definitely need to change if controller speed is introduced

            FollowTarget(deltaTime);
            HandleRotation(deltaTime, verticalAxis, horizontalAzis, speed);
            HandlePivotPosition();
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

            if(!lockedOn || lockOnTarget == null)
            {
                lockOnPrefab.transform.SetParent(this.transform);
                lockOnPrefab.transform.position = Vector3.zero;
                lockOnPrefab.SetActive(false);
            }

            if (lockedOn && lockOnTarget != null)
            {
                minAngle = lockOnMinAngle;
                maxAngle = lockOnMaxAngle;
                Vector3 targetDirection = lockOnTarget.position - transform.position;
                lockOnPrefab.transform.SetParent(lockOnTarget);
                lockOnPrefab.transform.position = new Vector3(lockOnTarget.position.x, lockOnTarget.position.y + 2.5f, lockOnTarget.position.z);
                lockOnPrefab.transform.LookAt(new Vector3(transform.position.x, lockOnPrefab.transform.position.y, transform.position.z), Vector3.up);
                lockOnPrefab.SetActive(true);
                targetDirection.Normalize();

                if (targetDirection == Vector3.zero)
                {
                    targetDirection = transform.forward;
                }

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                Mathf.Clamp(targetRotation.x, minAngle, maxAngle);
                targetRotation.x = transform.rotation.x;
                targetRotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * 10);
                lookAngle = transform.eulerAngles.y;
                return;
            }            

            minAngle = originalMinAngle;
            maxAngle = originalMaxAngle;
            lookAngle += smoothX * speed; // Calculate the new X axis
            transform.rotation = Quaternion.Euler(0, lookAngle, 0); // Assign X axis rotation to the camera            
        }

        /// <summary>
        /// Sets the Z axis of the camera between the default and minimum based on the results of any collisions detected between the camera and the target.
        /// </summary>
        private void HandlePivotPosition()
        {
            float targetZ = defaultZDistance;

            CameraCollision(defaultZDistance, ref targetZ); // Check for any objects obstructing camera view to the target.

            currentZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * zSpeed); // Lerp current Z axis to the new target Z axis
            Vector3 targetPosition = Vector3.zero;
            targetPosition.z = currentZ;

            if (targetPosition.z > minimumZDistance) // If Z value returned is greater than the minimum (-1 is greater than -2 for example as the Z is intended to be negative always)
            {
                targetPosition.z = minimumZDistance; // Set Z to minimum
            }

            cameraTransform.localPosition = targetPosition; // Update camera position
        }

        /// <summary>
        /// Checks for any collisions between the camera and the target, calculating a new Z axis point that will not be obstructed by the cause of the collision.
        /// </summary>
        private void CameraCollision(float targetZ, ref float actualZ)
        {
            StateManager states = FindObjectOfType<StateManager>(); // Player reference
            float step = Mathf.Abs(targetZ);
            int stepCount = 1;
            float stepIncrement = step / stepCount;

            RaycastHit hit;
            Vector3 origin = pivotPoint.position;
            Vector3 direction = -pivotPoint.forward;

            //Debug.DrawRay(origin, direction * step, Color.blue);

            if (Physics.Raycast(origin, direction, out hit, step, states.ignoredLayers)) // Raycast, ignoring the same layers as the player
            {
                if (!hit.transform.GetComponent<StateManager>() && !hit.transform.GetComponent<Enemy>()) // If the raycast returns an object which is not a player or enemy
                {
                    //Debug.Log(hit.transform.root.name);
                    float distance = Vector3.Distance(hit.point, origin); // Calculate distance from the point hit and the origin
                    actualZ = -(distance / 2); // Halve the distance and convert to negative value
                }               
            }
            else // If nothing is hit by the raycast...
            {
                for (int s = 0; s < stepCount + 1; s++)
                {
                    for (int i = 0; i < 4; i++) // Loop 4 times to raycast out in 4 different directions to check for additional obstacles to the camera
                    {
                        Vector3 dir = Vector3.zero;
                        Vector3 secondOrigin = origin + (direction * s) * stepIncrement;

                        switch (i)
                        {
                            case 0:
                                dir = cameraTransform.right;
                                break;
                            case 1:
                                dir = -cameraTransform.right;
                                break;
                            case 2:
                                dir = cameraTransform.up;
                                break;
                            case 3:
                                dir = -cameraTransform.up;
                                break;
                        }

                        //Debug.DrawRay(secondOrigin, dir * 0.5f, Color.red);

                        if (Physics.Raycast(secondOrigin, dir, out hit, 0.5f, states.ignoredLayers)) // Raycast, ignoring the same layers as the player
                        {
                            if (!hit.transform.GetComponent<StateManager>() && !hit.transform.GetComponent<Enemy>()) // If the raycast returns an object which is not a player or enemy
                            {
                                //Debug.Log(hit.transform.root.name);
                                float distance = Vector3.Distance(secondOrigin, origin); // Calculate distance from both origins
                                actualZ = -(distance / 2); // Halve the distance and convert to negative value

                                if (actualZ < 0.2f)
                                {
                                    actualZ = 0;
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the visible and lockstate of the Cursor. False for locked & not visible, true for unlocked & visible.
        /// </summary>
        public void ToggleCursorVisibleState(bool stateToChangeTo)
        {
            if (!stateToChangeTo)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}

