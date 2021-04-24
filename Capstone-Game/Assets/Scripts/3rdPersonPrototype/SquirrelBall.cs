using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquirrelControllerSettings;
using Player;

namespace Player
{
    [RequireComponent(typeof(SquirrelController))]
    public class SquirrelBall : SquirrelBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SquirrelController PARENT;
        
        public SCBallModeSettings settings = new SCBallModeSettings();
        public SCTriggers triggers = new SCTriggers();

        private SCBallStoredValues vals = new SCBallStoredValues();

        //~~~~~~~~~~ PROPERTIES ~~~~~~~~~~

        private SquirrelController.SCReferences ParentRefs
        {
            get { return PARENT.refs; }
        }

        private bool Grounded
        {
            get { return vals.lastOnSurface < 1; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~
        public override void ManualUpdate()
        {
            UpdMove();
            UpdJump();
        }

        private void UpdMove()
        {
            //--------------------------MOVEMENT PHYSICS + INPUTS--------------------------//
            //INPUT
            Vector3 desiredDirection = new Vector3();
            Vector3 camForward = Vector3.Cross(Vector3.down, ParentRefs.fCam.transform.right);
            desiredDirection += camForward * Input.GetAxis("Vertical");
            desiredDirection += ParentRefs.fCam.transform.right * Input.GetAxis("Horizontal");

            desiredDirection.Normalize();

            if (Grounded)
            {
                vals.lastGrounded = Time.time;
                if (Time.time > vals.lastJump + settings.J.jumpCooldown)
                    vals.jumping = false;
            }

            //Factor any modifiers like sneaking or slow effects into the max speed;
            float alteredMaxSpeed = settings.M.maxSpeed;

            //Calculate the ideal velocity from the input and the acceleration settings.
            Vector3 newVelocity;
            if (Grounded)
                newVelocity = ParentRefs.RB.velocity + (desiredDirection * settings.M.acceleration * Time.deltaTime);
            else
                newVelocity = ParentRefs.RB.velocity + (desiredDirection * settings.M.acceleration * settings.M.airControlFactor * Time.deltaTime);

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
            if (desiredDirection.magnitude < 0.01f && !vals.jumping)
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

            //Rotate the character if the lateral velocity is above a threshold.
            if (LateralVelocityNew.magnitude > settings.M.maxSpeed * settings.M.turningThreshold)
                ParentRefs.body.rotation = Quaternion.LookRotation(transform.TransformVector(LateralVelocityNew), -transform.forward);

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
            bool jumpOffCooldown = Time.time > vals.lastJump + settings.J.jumpCooldown;
            if (tryingToJump && groundedOrCoyotee && jumpOffCooldown)
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
        
        //~~~~~~~~~~ DATA STRUCTURES ~~~~~~~~~~

        [System.Serializable]
        public class SCTriggers
        {
            /// <summary> Trigger which is used to determine if the player is grounded and can therefore jump etc. </summary>
            public MovementTrigger feet;
        }

        private struct SCBallStoredValues
        {
            public float lastJump;
            public float lastGrounded;
            public float jumpPressed;
            public bool jumping;
            public float lastOnSurface;
            public bool moving;
        }

        [System.Serializable]
        public class SCBallModeSettings
        {
            public SCMoveSettings movement = new SCMoveSettings();
            public SCJumpSettings jump = new SCJumpSettings();

            public SCMoveSettings M { get { return movement; } }
            public SCJumpSettings J { get { return jump; } }

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
        }

    }
}
