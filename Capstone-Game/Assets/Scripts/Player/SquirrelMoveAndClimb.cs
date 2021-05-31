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

            vals.lastRotationDir = Vector3.down;
        }

        //~~~~~~~~~~ FUNCTIONS ~~~~~~~~~~

        /// <summary> Call all the update steps for movement, climing and jumping. </summary>
        public override void ManualUpdate()
        {
            UpdInput();
            CheckGravity();
            UpdMove();
            UpdJump();
            JumpOnWall();
            FindAndRotateToWall();
            RotateModel();
            UpdAnimator();
        }

        private void UpdAnimator()
        {
            if (vals.moving)
                ParentRefs.animator.SetInteger("MoveState", 1);
            else if (vals.jumping)
                ParentRefs.animator.SetInteger("MoveState", 2);
            else
                ParentRefs.animator.SetInteger("MoveState", 0);
        }

        private void UpdInput()
        {
            //Joystick or WASD motion:
            vals.desiredDirection = new Vector3();
            Vector3 camForward = Vector3.Cross(transform.forward, ParentRefs.fCam.transform.right);
            vals.desiredDirection += camForward * Input.GetAxis("Vertical");
            vals.desiredDirection += ParentRefs.fCam.transform.right * Input.GetAxis("Horizontal");

            vals.desiredDirection.Normalize();

            //Dash button:
            if (Input.GetButton("Dash"))
            {
                if (vals.dashing == false)
                    vals.startedDashing = Time.time;
                vals.dashing = true;
            }
            else
                vals.dashing = false;
        }
    

        /// <summary> Perform all the movement functions of the player, including making all forces including friction and input relative to the players rotation. </summary>
        private void UpdMove()
        {
            //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
            if (Grounded)
            {
                vals.lastGrounded = Time.time;
                vals.jumping = false;
            }

            //Factor any modifiers like sneaking or slow effects into the max speed;
            float alteredMaxSpeed = settings.M.maxSpeed;
            float alteredAcceleration = settings.M.acceleration;

            if (vals.dashing)
            {
                float value = settings.M.dashSpeedMultiplierCurve.Evaluate(Time.time - vals.startedDashing);
                alteredMaxSpeed *= value;
                alteredAcceleration *= value;
            }

            if (Grounded)
                alteredAcceleration *= settings.M.airControlFactor;

            //Calculate the ideal velocity from the input and the acceleration settings.
            Vector3 newVelocity;
            newVelocity = ParentRefs.RB.velocity + (vals.desiredDirection * alteredAcceleration * Time.deltaTime);

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

            //Delete the 'upwards' force (relative to player rotation), if requested by the climbing system.
            if (vals.eliminateUpForce)
            {
                vals.eliminateUpForce = false;
                TransformedNewVelocity.z = 0;
            }

            //Apply a small force towards (relative) down when velocity is near 0, to make sure player sticks to walls. 
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

        /// <summary> Check for jump input, and do the appropriate jump for the situation (needs work). </summary>
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

        /// <summary> Turn gravity off when the player is touching something so climbing works. </summary>
        private void CheckGravity()
        {
            if (Grounded)
                ParentRefs.RB.useGravity = false;
            else
                ParentRefs.RB.useGravity = true;
        }

        /// <summary> Rotate the player so their feet are aligned with the surface beneath them, based on a downwards raycast (need to add better checks for 90deg angles). </summary>
        private void FindAndRotateToWall()
        {
            //Raycasts:
            RaycastHit mainHit;
            bool Main = Physics.Raycast(refs.climbRotateCheckRay.position, -refs.climbRotateCheckRay.up, out mainHit, settings.WC.surfaceDetectRange, settings.WC.rotateToLayers);

            Vector3 dir = Vector3.down;
            if (Main)
            {
                dir = mainHit.normal;

                CustomIntuitiveSnapRotation(-dir);

                if (Vector3.Angle(vals.lastRotationDir, dir) > settings.WC.wallStickDangerAngle)
                {
                    vals.eliminateUpForce = true;
                }
                vals.lastRotationDir = dir;

                vals.lastOnSurface = Time.time;
            }
            else if (Time.time > vals.lastOnSurface + settings.WC.noSurfResetTime)
            {
                CustomIntuitiveSnapRotation(Vector3.down);
                vals.lastOnSurface = Time.time;
            }
        }

        private void RotateModel()
        {
            ParentRefs.model.rotation = Quaternion.RotateTowards(ParentRefs.model.rotation, ParentRefs.body.rotation, settings.WC.rotateDegreesPerSecond * Time.deltaTime);
            ParentRefs.model.localPosition = Vector3.MoveTowards(ParentRefs.model.localPosition, Vector3.zero, settings.WC.moveUnitsPerSecond * Time.deltaTime);
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
            {
                JumpToClimbWall(1f);
            }
            else if (vals.dashing)
            {
                JumpToClimbWall(0.5f);
            }
            else
            {
                RaycastHit hit;
                if (FindClimbableWall(out hit, 0.5f))
                {
                    refs.climbPointDisplay.position = hit.point;
                    refs.climbPointDisplay.gameObject.SetActive(true);
                }
                else
                    refs.climbPointDisplay.gameObject.SetActive(false);
            }
        }

        private bool JumpToClimbWall(float distMultiplier)
        {
            RaycastHit mainHit;

            if (FindClimbableWall(out mainHit, distMultiplier))
            {
                if (Physics.Raycast(transform.position, mainHit.point - transform.position, Vector3.Distance(transform.position, mainHit.point) - 0.01f, settings.WC.rotateToLayers))
                    return false; //Aborts if there is no line of sight between the player and the chosen point.
                //Rotate so feet are on new surface.
                Vector3 dir = mainHit.normal;
                CustomIntuitiveSnapRotation(-dir);
                //Teleport to the point, while maintaining the models position so it moves smoothly.
                Vector3 oldPos = ParentRefs.model.position;
                transform.position = mainHit.point;
                ParentRefs.model.position = oldPos;
                //Set relevant variables.
                vals.lastOnSurface = Time.time;
                //Set velocity to zero to mitigate weird physics.
                ParentRefs.RB.velocity = Vector3.zero;
                return true;
            }
            return false;
        }

        private bool FindClimbableWall(out RaycastHit hit, float distanceMultiplier)
        {
            RaycastHit mainHit;

            Vector3 sphereStart;
            Vector3 sphereDir;
            float modifiedDist = settings.WC.sphereDetectDistance * distanceMultiplier;
            if (vals.desiredDirection != Vector3.zero)
            {
                sphereStart = refs.startClimbCheckRay.position - (vals.desiredDirection * settings.WC.sphereDetectRadius);
                sphereDir = vals.desiredDirection;
            }
            else
            {
                sphereStart = refs.startClimbCheckRay.position - (ParentRefs.body.forward * settings.WC.sphereDetectRadius);
                sphereDir = ParentRefs.body.forward;
            }

            bool found = Physics.SphereCast(sphereStart, settings.WC.sphereDetectRadius, sphereDir, out mainHit, modifiedDist, settings.WC.climableLayers.value);

            //Do a second, larger pass if no target was found with the small forwards check.
            if (!found)
            {
                float secondPassRadius = settings.WC.sphereDetectRadius * settings.WC.secondPassMultiplier;
                if (vals.desiredDirection != Vector3.zero)
                {
                    sphereStart = refs.startClimbCheckRay.position - (vals.desiredDirection * secondPassRadius);
                    sphereDir = vals.desiredDirection;
                }
                else
                {
                    sphereStart = refs.startClimbCheckRay.position - (ParentRefs.body.forward * secondPassRadius);
                    sphereDir = ParentRefs.body.forward;
                }

                found = Physics.SphereCast(sphereStart, secondPassRadius, sphereDir, out mainHit, modifiedDist, settings.WC.climableLayers.value);
            }

            hit = mainHit;
            return found;
        }

        //~~~~~~~~~~ COROUTINES ~~~~~~~~~~

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
            public bool dashing;
            public float startedDashing;
            public bool eliminateUpForce;
            public Vector3 lastRotationDir;
            public Vector3 desiredDirection;
            public Quaternion targetBodyRot;
        }

        [System.Serializable]
        public class MovementRefs
        {
            public Transform climbPointDisplay;
            public Transform backCheckRay;
            public Transform frontCheckRay;
            public Transform acuteCheckRay;
            public Transform startClimbCheckRay;
            public Transform climbRotateCheckRay;
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
                //[Tooltip("Multiplier for the max speed when starting a dash.")]
                //public float dashSpeedMaxMult = 3f;
                //[Tooltip("Multiplier for the max speed after dashing for a long time.")]
                //public float dashSpeedMinMult = 3f;
                [Tooltip("Speed of the dash over time.")]
                public AnimationCurve dashSpeedMultiplierCurve = new AnimationCurve();
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
                [Tooltip("The layers of objects the player is allowed to climb on.")]
                public LayerMask rotateToLayers = new LayerMask();
                [Tooltip("Size of the sphere-cast that will detect surfaces to climb on. Larger means more forgiving controls, but also more likely to get objects behind the player.")]
                public float sphereDetectRadius = 0.3f;
                [Tooltip("Multiplier to the sphere-cast radius for the second climable check pass.")]
                public float secondPassMultiplier = 5f;
                [Tooltip("Length of the sphere-cast that detects surfaces. Larger means the check will find objects further from the player.")]
                public float sphereDetectDistance = 0.5f;
                [Tooltip("Distance from the center of the character from which walls below will be detected.")]
                public float surfaceDetectRange = 0.15f;
                [Tooltip("If the angle between the current rotation and the new rotation when climbing is above this value, remove the vertical velocity to help the player stich to the wall.")]
                public float wallStickDangerAngle = 10f;
                [Tooltip("Force applied when the character is near a wall to ensure they stick to it.")]
                public float wallStickForce = 0.2f;
                [Tooltip("Range of velocity (at normal to wall) within which sticking force is applied.")]
                public float wallStickTriggerRange = 0.5f;
                [Tooltip("Time away from a surface before the character rotates to face the ground.")]
                public float noSurfResetTime = 0.2f;
                [Tooltip("How quickly the squirrel model rotates to face the correct direction.")]
                public float rotateDegreesPerSecond = 360;
                [Tooltip("How quickly the squirrel model moves back to alignment when the physics object is teleported.")]
                public float moveUnitsPerSecond = 5f;
            }
        }
    }
}

//Put in seperate namespace since no other code should need to use this.
//Also seperated to sub-classes for ease of use in editor and autocomplete.
namespace SquirrelControllerSettings
{
}