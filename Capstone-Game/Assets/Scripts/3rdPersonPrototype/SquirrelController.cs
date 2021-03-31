using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class SquirrelController : MonoBehaviour
    {
        public SCReferences refs = new SCReferences();
        public SCSettings settings = new SCSettings();
        public SCTriggers triggers = new SCTriggers();

        private SCStoredValues vals = new SCStoredValues();

        public bool disableMovement = false;
        /// <summary> Time and inputs are not simulated when this is true. </summary>
        public bool debugPause = false;

        private bool Grounded
        {
            get { return triggers.feet.triggered; }
        }

        void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            refs.RB.useGravity = false;

            Initialize();
        }

        private void Initialize()
        {
            vals.jumpPressed = -10;
            vals.lastGrounded = -10;
            vals.lastJump = -10;
            vals.lastOnSurface = -10;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (CheckPause())
                return;
            CheckGravity();
            UpdCamera();
            UpdMove();
            JumpOnWall();
            RotateToWall();
        }

        private bool CheckPause()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (debugPause)
                {
                    debugPause = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1;
                }
                else
                {
                    debugPause = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 0;
                }
            }

            return debugPause;
        }

        private void UpdCamera()
        {
            refs.fCam.UpdCamera(transform, refs.RB);
        }

        private void UpdMove()
        {
            //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
            //INPUT
            Vector3 desiredDirection = new Vector3();
            Vector3 camForward = Vector3.Cross(transform.forward, refs.fCam.transform.right);
            desiredDirection += camForward * Input.GetAxis("Vertical");
            desiredDirection += refs.fCam.transform.right * Input.GetAxis("Horizontal");

            desiredDirection.Normalize();

            if (Grounded)
                vals.lastGrounded = Time.time;

            //Request a jump if the player has been on the ground recently, but has NOT jumped recently (prevents inconsistent jump forces).
            if (Input.GetButtonDown("Jump"))
            {
                if ((Grounded || Time.time < vals.lastGrounded + settings.coyoteeTime) && Time.time > vals.lastJump + settings.jumpCooldown)
                {
                    //vals.lastJump = Time.time;
                    vals.jumpPressed = Time.time;
                }
            }

            //Factor any modifiers like sneaking or slow effects into the max speed;
            float alteredMaxSpeed = settings.maxSpeed;

            //Calculate the ideal velocity from the input and the acceleration settings.
            Vector3 newVelocity;
            if (Grounded)
                newVelocity = refs.RB.velocity + (desiredDirection * settings.acceleration * Time.deltaTime);
            else
                newVelocity = refs.RB.velocity + (desiredDirection * settings.acceleration * settings.airControlFactor * Time.deltaTime);

            //Transform current and ideal velocity to local space so non-vertical (lateral) speed can be calculated and limited.
            Vector3 TransformedOldVelocity = transform.InverseTransformVector(refs.RB.velocity);
            Vector3 TransformedNewVelocity = transform.InverseTransformVector(newVelocity);

            //Get lateral speed by cutting out local-z component (Must be z for LookRotation function to work. Would otherwise be y).
            Vector3 LateralVelocityOld = new Vector3(TransformedOldVelocity.x, TransformedOldVelocity.y, 0);
            Vector3 LateralVelocityNew = new Vector3(TransformedNewVelocity.x, TransformedNewVelocity.y, 0);

            //If the local lateral velocity of the player is above the max speed, do not allow any increases in speed due to input.
            if (LateralVelocityNew.magnitude > alteredMaxSpeed)
            {
                //If the new movement would speed up the player.
                if (LateralVelocityNew.magnitude > LateralVelocityOld.magnitude)
                {
                    //If the player was not at max speed yet, set them to the max speed, otherwise revert to the old speed (but with direction changes).
                    if (LateralVelocityOld.magnitude < alteredMaxSpeed)
                        LateralVelocityNew = LateralVelocityNew.normalized * alteredMaxSpeed;
                    else
                        LateralVelocityNew = LateralVelocityNew.normalized * LateralVelocityOld.magnitude;
                }

                //FRICTION
                //If the new lateral velocity is still greater than the max speed, reduce it by the relevant amount until it is AT the max speed.
                if (LateralVelocityNew.magnitude > settings.maxSpeed)
                {
                    if (Grounded)
                        LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(settings.maxSpeed, LateralVelocityNew.magnitude - settings.frictionForce);
                    //else
                    //	LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max (MaxSpeed, LateralVelocityNew.magnitude - (FrictionForce * AirControlFactor));
                }
            }

            //If the player is not trying to move, apply stopping force.
            if ((desiredDirection.magnitude < 0.01f)) //&& !(Time.time < vals.jumpPressed + settings.checkJumpTime)) || disableMovement)
            {

                Vector3 NewVelocity = refs.RB.velocity;

                //Jump to zero velocity when below max speed and on the ground to give more control and prevent gliding.
                if (Grounded && LateralVelocityNew.magnitude < alteredMaxSpeed / 2)
                    LateralVelocityNew = new Vector3();
                else
                {
                    //Otherwise apply a 'friction' force to the player.
                    if (Grounded)
                        LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(0, LateralVelocityNew.magnitude - (settings.stoppingForce * Time.deltaTime));
                    else
                        LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(0, LateralVelocityNew.magnitude - (settings.airStoppingForce * Time.deltaTime));
                }
                vals.moving = false;
            }
            else
                vals.moving = true;

            float z = TransformedNewVelocity.z;
            if (TransformedNewVelocity.z.Inside(-settings.wallStickTriggerRange, settings.wallStickTriggerRange))
                TransformedNewVelocity.z += settings.wallStickForce;


            if (LateralVelocityNew.magnitude > settings.maxSpeed / 5)
                refs.body.rotation = Quaternion.LookRotation(transform.TransformVector(LateralVelocityNew), -transform.forward);

            //Add the vertical component back, convert it to world-space, and set the new velocity to it.
            LateralVelocityNew += new Vector3(0, 0, TransformedNewVelocity.z);
            newVelocity = transform.TransformVector(LateralVelocityNew);

            Vector3 finalVelocityChange = newVelocity - refs.RB.velocity;

            //If the player has requested a jump recently, apply a force and set the last jump time.
            if (Time.time < vals.jumpPressed + settings.checkJumpTime)
            {
                refs.RB.velocity += -transform.forward * settings.jumpForce;
                vals.lastJump = Time.time;
                vals.jumpPressed = -5;
            }

            refs.RB.velocity += finalVelocityChange;
        }

        private void CheckGravity()
        {
            if (triggers.feet.triggered)
                refs.RB.useGravity = false;
            else
                refs.RB.useGravity = true;
        }

        public float ClampCam(float val)
        {
            if (val > settings.cameraClampMax)
                return settings.cameraClampMax;
            else if (val < settings.cameraClampMin)
                return settings.cameraClampMin;

            return val;
        }

        private void RotateToWall()
        {
            //Raycasts:
            RaycastHit FRhit;
            RaycastHit FLhit;
            RaycastHit BRhit;
            RaycastHit BLhit;
            RaycastHit mainHit;
            bool FR = Physics.Raycast(refs.surfaceDetectorFR.position, refs.surfaceDetectorFR.up, out FRhit, settings.surfaceDetectRange);
            bool FL = Physics.Raycast(refs.surfaceDetectorFL.position, refs.surfaceDetectorFL.up, out FLhit, settings.surfaceDetectRange);
            bool BR = Physics.Raycast(refs.surfaceDetectorBR.position, refs.surfaceDetectorBR.up, out BRhit, settings.surfaceDetectRange);
            bool BL = Physics.Raycast(refs.surfaceDetectorBL.position, refs.surfaceDetectorBL.up, out BLhit, settings.surfaceDetectRange);
            bool Main = Physics.Raycast(transform.position, transform.forward, out mainHit, settings.surfaceDetectRange);

            //Diagnose:

            if (FR || FL || Main)
            {
                Vector3 dir = mainHit.normal;
                if (FR && FL)
                {
                    if (Main && BR && BL)
                    {
                        dir = (FRhit.normal + FLhit.normal + BLhit.normal + BRhit.normal + mainHit.normal) / 5;
                    }
                }

                CustomIntuitiveSnapRotation(-dir);
                vals.lastOnSurface = Time.time;
            }
            else if (Time.time > vals.lastOnSurface + settings.noSurfResetTime)
            {
                CustomIntuitiveSnapRotation(Vector3.down);
                vals.lastOnSurface = Time.time;
            }
        }

        private void CustomIntuitiveSnapRotation(Vector3 direction)
        {
            Quaternion CameraPreRotation = refs.head.transform.rotation;
            Vector3 OriginalFacing = refs.head.transform.forward; //Remember that forward is down (the feet of the player) to let LookRotation work.

            //Rotate the players 'body'.
            transform.rotation = Quaternion.LookRotation(direction, transform.right);
            Quaternion NewRot = new Quaternion();
            NewRot.eulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
            transform.localRotation = NewRot;

            //Calculate the angle difference between the two rotations, then save the 'number of full rotations' it represents.
            float Signed = Vector3.SignedAngle(OriginalFacing, refs.head.transform.forward, transform.right);
            vals.cameraAngle -= Signed;
            refs.head.transform.rotation = CameraPreRotation;
        }

        private void JumpOnWall()
        {
            if (Input.GetButtonDown("Jump"))
                StartCoroutine(JumpOnWallChecks());
        }

        private IEnumerator JumpOnWallChecks()
        {
            int i = 0;
            while (i < settings.jumpDetectRepeats)
            {
                yield return new WaitForSeconds(settings.jumpDetectDelay);
                ++i;

                RaycastHit mainHit;
                bool Main = Physics.Raycast(refs.body.position, refs.body.forward, out mainHit, settings.surfaceDetectRange);
                if (Main)
                {
                    Vector3 dir = mainHit.normal;
                    CustomIntuitiveSnapRotation(-dir);
                    vals.lastOnSurface = Time.time;
                    break;
                }
            }
        }

        [System.Serializable]
        public class SCReferences
        {
            public Rigidbody RB;
            public Transform head;
            public Transform body;
            public Camera camera;
            public FloatingCamera fCam;
            public Transform surfaceDetectorFR;
            public Transform surfaceDetectorFL;
            public Transform surfaceDetectorBR;
            public Transform surfaceDetectorBL;
        }

        [System.Serializable]
        public class SCSettings
        {
            [Header("Movement Settings")]
            [Tooltip("Force applied when player holds movement input. Controlls how quickly max speed is reached and how much forces can be countered.")]
            public float acceleration = 1;
            [Tooltip("The horizontal speed at which no new acceleration is allowed by the player.")]
            public float maxSpeed = 1;
            [Tooltip("Multiplier for the amount of acceleration applied while in the air.")]
            public float airControlFactor = 1;
            [Tooltip("Rate at which speed naturally decays back to max speed (used in case of external forces).")]
            public float frictionForce = 1;
            [Tooltip("Rate at which speed falls to zero when not moving.")]
            public float stoppingForce = 1;
            [Tooltip("Rate at which speed falls to zero when not moving and in the air.")]
            public float airStoppingForce = 1;

            [Header("Jump Settings")]
            [Tooltip("Force applied when the player jumps.")]
            public float jumpForce = 1;
            [Tooltip("Time after a jump before the player can jump again. Stops superjumps from pressing twice while trigger is still activated.")]
            public float jumpCooldown = 0.2f;
            [Tooltip("Time in which jumps will be triggered if conditions are met after the key is pressed.")]
            public float checkJumpTime = 0.2f;
            [Tooltip("Time in which jump will still be allowed after the player leaves the ground. Should always be less than jumpCooldown.")]
            public float coyoteeTime = 0.2f;

            [Header("Camera Settings")]
            [Tooltip("Multiplier for converting mouse (or joystick) motion to camera movement.")]
            public float mouseSense = 1;
            [Tooltip("Maximum angle for the vertical motion of the camera.")]
            public float cameraClampMax = 50;
            [Tooltip("Minimum angle for the vertical motion of the camera.")]
            public float cameraClampMin = 0;

            [Header("Wallclimb Settings")]
            [Tooltip("Distance from the center of the character from which walls below will be detected.")]
            public float surfaceDetectRange = 2;
            [Tooltip("Force applied when the character is near a wall to ensure they stick to it.")]
            public float wallStickForce = 0.1f;
            [Tooltip("Range of velocity (at normal to wall) within which sticking force is applied.")]
            public float wallStickTriggerRange = 0.3f;
            [Tooltip("Time away from a surface before the character rotates to face the ground.")]
            public float noSurfResetTime = 0.2f;
            [Tooltip("Time between checks for surfaces in front of the character.")]
            public float jumpDetectDelay = 0.2f;
            [Tooltip("Distance from the center of the character from which walls in front will be detected.")]
            public float jumpDetectRange = 0.2f;
            [Tooltip("Number of checks (with jumpDetectDelay time between) done after the character has jumped.")]
            public int jumpDetectRepeats = 5;

            //[Header("Misc")]
        }

        [System.Serializable]
        public class SCTriggers
        {
            /// <summary> Trigger which is used to determine if the player is grounded and can therefore jump etc. </summary>
            public MovementTrigger feet;
            /// <summary> Trigger which is used to determine if the player is running into a wall, to trigger wall climbing. </summary>
            public MovementTrigger wallClimb;
        }

        private struct SCStoredValues
        {
            public float lastJump;
            public float lastGrounded;
            public float jumpPressed;
            public float cameraAngle;
            public float lastOnSurface;
            public bool moving;
        }
    }
}
