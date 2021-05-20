using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquirrelControllerSettings;
using Player;

namespace Player
{
    [RequireComponent(typeof(SquirrelController))]
    public class SquirrelMoveAndClimb : SquirrelBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SquirrelController PARENT;

        public MovementRefs refs = new MovementRefs();
        public SCRunModeSettings settings = new SCRunModeSettings();
        public SCTriggers triggers = new SCTriggers();

        public string debugString = "null";

        private SCRunStoredValues vals = new SCRunStoredValues();
        
        //~~~~~~~~~~ PROPERTIES ~~~~~~~~~~

        private SquirrelController.SCReferences ParentRefs
        {
            get { return PARENT.refs; }
        }

        private bool Grounded
        {
            get { return triggers.feet.triggered && PARENT.TouchingSomething; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~

        void Awake()
        {
            if (PARENT == null)
                GetComponentInParent<SquirrelController>();
        }

        //~~~~~~~~~~ FUNCTIONS ~~~~~~~~~~

        public override void ManualUpdate()
        {
            CheckGravity();
            UpdMove();
            UpdJump();
            JumpOnWall();
            RotateToWall();
            RotatePlayerBody();
        }

        private void UpdMove()
        {
            //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
            //INPUT
            vals.desiredDirection = new Vector3();
            Vector3 camForward = Vector3.Cross(transform.forward, ParentRefs.fCam.transform.right);
            vals.desiredDirection += camForward * Input.GetAxis("Vertical");
            vals.desiredDirection += ParentRefs.fCam.transform.right * Input.GetAxis("Horizontal");

            vals.desiredDirection.Normalize();

            if (Grounded)
            {
                vals.lastGrounded = Time.time;
                vals.jumping = false;
            }

            //Factor any modifiers like sneaking or slow effects into the max speed;
            float alteredMaxSpeed = settings.M.maxSpeed;

            //Calculate the ideal velocity from the input and the acceleration settings.
            Vector3 newVelocity;
            if (Grounded)
                newVelocity = ParentRefs.RB.velocity + (vals.desiredDirection * settings.M.acceleration * Time.deltaTime);
            else
                newVelocity = ParentRefs.RB.velocity + (vals.desiredDirection * settings.M.acceleration * settings.M.airControlFactor * Time.deltaTime);

            //Transform current and ideal velocity to local space so non-vertical (lateral) speed can be calculated and limited.
            Vector3 TransformedOldVelocity = transform.InverseTransformVector(ParentRefs.RB.velocity);
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
                    //If the player was not at max speed yet, set them to the max speed, otherwise revert to the old speed (direction changes allowed in both cases).
                    if (LateralVelocityOld.magnitude < alteredMaxSpeed)
                        LateralVelocityNew = LateralVelocityNew.normalized * alteredMaxSpeed;
                    else
                        LateralVelocityNew = LateralVelocityNew.normalized * LateralVelocityOld.magnitude;
                }

                //FRICTION
                //If the new lateral velocity is still greater than the max speed, reduce it by the relevant amount until it is AT the max speed.
                if (LateralVelocityNew.magnitude > settings.M.maxSpeed)
                {
                    if (Grounded && !vals.jumping)
                        LateralVelocityNew = LateralVelocityNew.normalized
                            * Mathf.Max(settings.M.maxSpeed, LateralVelocityNew.magnitude - settings.M.frictionForce * Time.deltaTime);
                    else
                        LateralVelocityNew = LateralVelocityNew.normalized
                            * Mathf.Max(settings.M.maxSpeed, LateralVelocityNew.magnitude - (settings.M.frictionForce * settings.M.airControlFactor) * Time.deltaTime);
                }
            }

            //If the player is not trying to move and not jumping, apply stopping force.
            if (vals.desiredDirection.magnitude < 0.01f && !vals.jumping)
            {
                //Jump to zero velocity when below max speed and on the ground to give more control and prevent gliding.
                if (Grounded && LateralVelocityNew.magnitude < alteredMaxSpeed * settings.M.haltAtFractionOfMaxSpeed)
                    LateralVelocityNew = new Vector3();
                else
                {
                    //Otherwise apply a 'friction' force to the player.
                    if (Grounded)
                        LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(0, LateralVelocityNew.magnitude - (settings.M.stoppingForce * Time.deltaTime));
                    else
                        LateralVelocityNew = LateralVelocityNew.normalized * Mathf.Max(0, LateralVelocityNew.magnitude - (settings.M.airStoppingForce * Time.deltaTime));
                }
                vals.moving = false;
            }
            else
                vals.moving = true;

            //Apply a small force towards (rotated) down when velocity is near 0, to make sure player sticks to walls. 
            float z = TransformedNewVelocity.z;
            if (TransformedNewVelocity.z.Inside(-settings.WC.wallStickTriggerRange, settings.WC.wallStickTriggerRange))
                TransformedNewVelocity.z += settings.WC.wallStickForce;

            //Rotate the character if the lateral velocity is above a threshold.
            if (LateralVelocityNew.magnitude > settings.M.maxSpeed * settings.M.turningThreshold)
            {
                //Quaternion bodyPreRotation = ParentRefs.model.rotation;
                ParentRefs.body.rotation = Quaternion.LookRotation(transform.TransformVector(LateralVelocityNew), -transform.forward);
                //ParentRefs.model.rotation = bodyPreRotation;
            }

            //Add the vertical component back, convert it to world-space, and set the new velocity to it.
            LateralVelocityNew += new Vector3(0, 0, TransformedNewVelocity.z);
            newVelocity = transform.TransformVector(LateralVelocityNew);

            Vector3 finalVelocityChange = newVelocity - ParentRefs.RB.velocity;
            ParentRefs.RB.velocity += finalVelocityChange;
        }

        private void UpdJump()
        {
            //Request a jump if the player presses the button.
            //This helps make jumping more consistent if conditions are false on intermittent frames.
            if (Input.GetButtonDown("Jump"))
                vals.jumpPressed = Time.time;

            //If the player wants to and is able to jump, apply a force and set the last jump time.
            bool tryingToJump = Time.time < vals.jumpPressed + settings.J.checkJumpTime;
            bool groundedOrCoyotee = Grounded || Time.time < vals.lastGrounded + settings.J.coyoteeTime;
            if (tryingToJump && groundedOrCoyotee)
            {
                vals.jumping = true;
                vals.lastJump = Time.time;
                vals.jumpPressed = -5;

                bool forwardJump = vals.moving && settings.J.allowForwardJumps;
                if (forwardJump)
                {
                    //Do a 'forward' jump relative to the character.
                    //Debug.Log("Forwards Jump");
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce * settings.J.forwardJumpVerticalFraction;
                    ParentRefs.RB.velocity += ParentRefs.body.forward * settings.J.forwardJumpForce;
                }
                else if (Vector3.Angle(transform.forward, Vector3.down) > settings.J.onWallAngle)
                { //If player is rotated to face the ground.
                  //Do a wall jump (biased towards up instead of out).
                    //Debug.Log("Wall Jump");
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce * (1 - settings.J.standingWallJumpVerticalRatio);
                    ParentRefs.RB.velocity += Vector3.up * settings.J.jumpForce * settings.J.standingWallJumpVerticalRatio;
                }
                else
                {
                    //Do a normal jump.
                    //Debug.Log("Normal Jump");
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce;
                }

            }
        }

        private void CheckGravity()
        {
            if (Grounded)
                ParentRefs.RB.useGravity = false;
            else
                ParentRefs.RB.useGravity = true;
        }

        private void RotateToWall()
        {
            //Raycasts:
            RaycastHit mainHit;
            bool Main = Physics.Raycast(transform.position, transform.forward, out mainHit, settings.WC.surfaceDetectRange);
            RaycastHit frontHit;
            RaycastHit backHit;
            
            //bool front = Physics.Raycast(refs.frontCheckRay.position, -refs.frontCheckRay.up, out frontHit, settings.WC.surfaceDetectRange);
            //bool back = Physics.Raycast(refs.backCheckRay.position, -refs.backCheckRay.up, out backHit, settings.WC.surfaceDetectRange);

            Vector3 dir = Vector3.down;

            /*
            if (front && back)
            {
                dir = frontHit.normal + backHit.normal;

                CustomIntuitiveSnapRotation(-dir);
                vals.lastOnSurface = Time.time;
            }
            */
            if (Main)
            {
                dir = mainHit.normal;

                CustomIntuitiveSnapRotation(-dir);
                vals.lastOnSurface = Time.time;
            }
            else if (Time.time > vals.lastOnSurface + settings.WC.noSurfResetTime)
            {
                CustomIntuitiveSnapRotation(Vector3.down);
                vals.lastOnSurface = Time.time;
            }
        }

        /*
        private void OLDRotateToWall()
        {
            //Raycasts:
            RaycastHit FRhit;
            RaycastHit FLhit;
            RaycastHit BRhit;
            RaycastHit BLhit;
            RaycastHit mainHit;
            bool FR = Physics.Raycast(refs.surfaceDetectorFR.position, refs.surfaceDetectorFR.up, out FRhit, settings.WC.surfaceDetectRange);
            bool FL = Physics.Raycast(refs.surfaceDetectorFL.position, refs.surfaceDetectorFL.up, out FLhit, settings.WC.surfaceDetectRange);
            bool BR = Physics.Raycast(refs.surfaceDetectorBR.position, refs.surfaceDetectorBR.up, out BRhit, settings.WC.surfaceDetectRange);
            bool BL = Physics.Raycast(refs.surfaceDetectorBL.position, refs.surfaceDetectorBL.up, out BLhit, settings.WC.surfaceDetectRange);
            bool Main = Physics.Raycast(transform.position, transform.forward, out mainHit, settings.WC.surfaceDetectRange);

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
            else if (Time.time > vals.lastOnSurface + settings.WC.noSurfResetTime)
            {
                CustomIntuitiveSnapRotation(Vector3.down);
                vals.lastOnSurface = Time.time;
            }
        }
        */

        private void RotatePlayerBody()
        {
            //Quaternion zero = new Quaternion();
            ParentRefs.model.rotation = Quaternion.RotateTowards(ParentRefs.model.rotation, ParentRefs.body.rotation, settings.WC.rotateDegreesPerSecond * Time.deltaTime);
            //ParentRefs.model.localRotation = Quaternion.Lerp(ParentRefs.model.localRotation, zero, 1f);
        }

        private void CustomIntuitiveSnapRotation(Vector3 direction)
        {
            Quaternion bodyPreRotation = ParentRefs.model.rotation;

            //Rotate the body 'holder' with the scripts attatched to it.
            //Remember that forward is rotated to face down (the feet of the player) to let LookRotation work.
            transform.rotation = Quaternion.LookRotation(direction, transform.right);
            Quaternion newRot = new Quaternion();
            newRot.eulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z + 90);
            transform.localRotation = newRot;
            
            ParentRefs.model.rotation = bodyPreRotation;
        }

        private void JumpOnWall()
        {
            if (Input.GetButtonDown("Jump"))
                StartCoroutine(JumpOnWallChecks());
        }

        //~~~~~~~~~~ COROUTINES ~~~~~~~~~~

        private IEnumerator JumpOnWallChecks()
        {
            int i = 0;
            while (i < settings.WC.jumpDetectRepeats)
            {
                if (Grounded && !vals.jumping)
                    break;

                RaycastHit mainHit;

                Vector3 sphereStart;
                Vector3 sphereDir;
                if (vals.desiredDirection != Vector3.zero)
                {
                    sphereStart = refs.climbCheckRay.position - (vals.desiredDirection * settings.WC.sphereDetectRadius);
                    sphereDir = vals.desiredDirection;
                }
                else
                {
                    sphereStart = refs.climbCheckRay.position - (ParentRefs.body.forward * settings.WC.sphereDetectRadius);
                    sphereDir = ParentRefs.body.forward;
                }

                bool foundTarget = false;
                if (Physics.SphereCast(sphereStart, settings.WC.sphereDetectRadius, sphereDir, out mainHit, 1, settings.WC.climableLayers.value))
                {
                    foundTarget = true;
                    debugString = mainHit.collider.name;
                    Debug.DrawLine(ParentRefs.body.position, mainHit.point);
                }

                if (foundTarget)
                {
                    Vector3 dir = mainHit.normal;
                    CustomIntuitiveSnapRotation(-dir);
                    vals.lastOnSurface = Time.time;
                    break;
                }

                yield return new WaitForSeconds(settings.WC.jumpDetectDelay);
                ++i;
            }
        }

        //~~~~~~~~~~ DATA STRUCTURES ~~~~~~~~~~

        [System.Serializable]
        public class SCTriggers
        {
            /// <summary> Trigger which is used to determine if the player is grounded and can therefore jump etc. </summary>
            public MovementTrigger feet;
        }

        private struct SCRunStoredValues
        {
            public float lastJump;
            public float lastGrounded;
            public float lastOnSurface; //Consider merging these two.
            public float jumpPressed;
            public bool jumping;
            public bool moving;
            public Vector3 desiredDirection;
            public Quaternion targetBodyRot;
        }

        [System.Serializable]
        public class MovementRefs
        {
            public Transform backCheckRay;
            public Transform frontCheckRay;
            public Transform acuteCheckRay;
            public Transform climbCheckRay;
        }

        [System.Serializable]
        public class SCRunModeSettings
        {
            public SCMoveSettings movement = new SCMoveSettings();
            public SCJumpSettings jump = new SCJumpSettings();
            public SCWallClimbSettings wallClimbing = new SCWallClimbSettings();

            public SCMoveSettings M { get { return movement; } }
            public SCJumpSettings J { get { return jump; } }
            public SCWallClimbSettings WC { get { return wallClimbing; } }

            [System.Serializable]
            public class SCMoveSettings
            {
                [Header("Movement Settings")]
                [Tooltip("Force applied when player holds movement input. Controlls how quickly max speed is reached and how much forces can be countered.")]
                public float acceleration = 20f;
                [Tooltip("The horizontal speed at which no new acceleration is allowed by the player.")]
                public float maxSpeed = 3f;
                [Tooltip("Multiplier for the amount of acceleration applied while in the air.")]
                public float airControlFactor = 0.5f;
                [Tooltip("Rate at which speed naturally decays back to max speed (used in case of external forces).")]
                public float frictionForce = 50f;
                [Tooltip("Rate at which speed falls to zero when not moving.")]
                public float stoppingForce = 50f;
                [Tooltip("Rate at which speed falls to zero when not moving and in the air.")]
                public float airStoppingForce = 2;
                [Tooltip("haltAtFractionOfMaxSpeed: Fraction of the max speed at which a grounded player will fully stop.")]
                [Range(0, 1)]
                public float haltAtFractionOfMaxSpeed = 0.9f;
                [Tooltip("Fraction of speed which the character model will rotate at.")]
                [Range(0, 1)]
                public float turningThreshold = 0.2f;
            }

            [System.Serializable]
            public class SCJumpSettings
            {
                [Header("Jump Settings")]
                [Tooltip("Force applied upwards (or outwards) when the player jumps.")]
                public float jumpForce = 3f;
                [Tooltip("Toggles if a burst of force is applied when jumping and moving.")]
                public bool allowForwardJumps = true;
                [Tooltip("Force applied in the direction of motion when the player jumps.")]
                public float forwardJumpForce = 5f;
                [Tooltip("forwardJumpVerticalFraction: Fraction of the normal jump force which is also applied in a forward jump.")]
                [Range(0, 1)]
                public float forwardJumpVerticalFraction = 0.5f;
                [Tooltip("Time after a jump before the player can jump again. Stops superjumps from pressing twice while trigger is still activated.")]
                public float jumpCooldown = 0.2f;
                [Tooltip("Time in which jumps will still be triggered if conditions are met after the key is pressed.")]
                public float checkJumpTime = 0.2f;
                [Tooltip("Time in which jump will still be allowed after the player leaves the ground. Should always be less than jumpCooldown.")]
                public float coyoteeTime = 0.2f;
                [Tooltip("Angle at which the player will be considered to be on a wall instead of on the ground (e.g. for special jumps).")]
                public float onWallAngle = 10f;
                [Tooltip("standingWallJumpVerticalRatio: Amount of the jump force which is applied upwards instead of outwards when a player jumps off a wall.")]
                [Range(0, 1)]
                public float standingWallJumpVerticalRatio = 0.5f;
            }

            [System.Serializable]
            public class SCWallClimbSettings
            {
                [Header("Wallclimb Settings")]
                [Tooltip("The layers of objects the player is allowed to climb on.")]
                public LayerMask climableLayers = new LayerMask();
                [Tooltip("Size of the sphere-cast that will detect surfaces to climb on. Larger means more forgiving controls, but also more likely to get objects behind the player.")]
                public float sphereDetectRadius = 0.3f;
                [Tooltip("Length of the sphere-cast that detects surfaces. Larger means the check will find objects further from the player.")]
                public float sphereDetectDistance = 0.5f;
                [Tooltip("Distance from the center of the character from which walls below will be detected.")]
                public float surfaceDetectRange = 0.15f;
                [Tooltip("Force applied when the character is near a wall to ensure they stick to it.")]
                public float wallStickForce = 0.2f;
                [Tooltip("Range of velocity (at normal to wall) within which sticking force is applied.")]
                public float wallStickTriggerRange = 0.5f;
                [Tooltip("Time away from a surface before the character rotates to face the ground.")]
                public float noSurfResetTime = 0.2f;
                [Tooltip("Time between checks for surfaces in front of the character.")]
                public float jumpDetectDelay = 0.05f;
                [Tooltip("Distance from the center of the character from which walls in front will be detected.")]
                public float jumpDetectRange = 0.3f;
                [Tooltip("Number of checks (with jumpDetectDelay time between) done after the character has jumped. Also stopped by being Grounded.")]
                public int jumpDetectRepeats = 20;
                [Tooltip("How quickly the squirrel model rotates to face the correct direction.")]
                public float rotateDegreesPerSecond = 360;
            }
        }
    }
}

//Put in seperate namespace since no other code should need to use this.
//Also seperated to sub-classes for ease of use in editor and autocomplete.
namespace SquirrelControllerSettings
{
}