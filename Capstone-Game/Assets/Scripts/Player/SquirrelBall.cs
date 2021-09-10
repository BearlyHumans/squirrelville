using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Player
{
    [RequireComponent(typeof(SquirrelController))]
    public class SquirrelBall : SquirrelBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SquirrelController PARENT;
        
        public SCBallModeSettings settings = new SCBallModeSettings();
        
        private SCBallStoredValues vals = new SCBallStoredValues();

        //~~~~~~~~~~ PROPERTIES ~~~~~~~~~~

        private SquirrelController.SCReferences ParentRefs
        {
            get { return PARENT.refs; }
        }

        private bool Grounded
        {
            get { return PARENT.TouchingSomething && vals.lastGrounded == Time.time; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~
        public override void ManualUpdate()
        {
            CheckGrounded();
            UpdMove();
            UpdJump();
        }

        private void CheckGrounded()
        {
            if (PARENT.TouchingSomething)
            {
                bool feetTouching = false;
                if (settings.J.JumpTriggerRelativeTo == SCBallModeSettings.SCJumpSettings.DownOrVel.down || ParentRefs.RB.velocity == Vector3.zero)
                    feetTouching = Physics.CheckSphere(ParentRefs.ballCollider.position + Vector3.down * settings.J.JumpTriggerOffset,
                        settings.J.JumpTriggerRadius, settings.J.JumpableLayers);
                else
                    feetTouching = Physics.CheckSphere(ParentRefs.ballCollider.position + ParentRefs.RB.velocity.normalized * settings.J.JumpTriggerOffset,
                        settings.J.JumpTriggerRadius, settings.J.JumpableLayers);

                if (feetTouching)
                    vals.lastGrounded = Time.time;
            }
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

            Vector3 addedVelocity;
            if (Grounded)
            {
                if (Input.GetButton("Dash") && ParentRefs.stamina.UseStamina(settings.M.boostStaminaUse * Time.deltaTime))
                    addedVelocity = (desiredDirection * settings.M.acceleration * settings.M.boostMultiplier * Time.deltaTime);
                else
                    addedVelocity = (desiredDirection * settings.M.acceleration * Time.deltaTime);
            }
            else
                addedVelocity = (desiredDirection * settings.M.acceleration * settings.M.airControlFactor * Time.deltaTime);

            Vector3 newVel = ParentRefs.RB.velocity + addedVelocity;
            float oldMag = ParentRefs.RB.velocity.magnitude;
            float addedMag = addedVelocity.magnitude;
            //If the new movement would speed up the player.
            if (oldMag != 0 && newVel.magnitude > (oldMag + (addedMag / 2f)))
                addedVelocity = addedVelocity * Mathf.Min(1, (settings.M.maxSpeed / oldMag));
            else if (oldMag != 0 && newVel.magnitude < (oldMag - (addedMag / 2f)))
                addedVelocity = addedVelocity * 1.2f;


            ParentRefs.RB.velocity += addedVelocity;
        }

        private void UpdJump()
        {
            //Request a jump if the player presses the button.
            //This helps make jumping more consistent if conditions are false on intermittent frames.
            if (Input.GetButtonDown("Jump"))
                vals.jumpPressed = Time.time;

            //If the player wants to and is able to jump, apply a force and set the last jump time.
            bool tryingToJump = Time.time < vals.jumpPressed + settings.J.checkJumpTime;
            bool groundedOrCoyotee = Time.time < vals.lastGrounded + settings.J.coyoteeTime;
            bool jumpOffCooldown = Time.time > vals.lastJump + settings.J.jumpCooldown;
            if (tryingToJump && groundedOrCoyotee && jumpOffCooldown && ParentRefs.stamina.UseStamina(settings.J.staminaUsed))
            {
                vals.jumping = true;
                vals.lastJump = Time.time;
                vals.jumpPressed = -5;

                if (ParentRefs.RB.velocity.y > settings.J.jumpForce * 2f)
                    return;

                if (settings.J.JumpTriggerRelativeTo == SCBallModeSettings.SCJumpSettings.DownOrVel.down)
                    ParentRefs.RB.velocity += Vector3.up * settings.J.jumpForce;
                else
                    ParentRefs.RB.velocity = -ParentRefs.RB.velocity.normalized * settings.J.jumpForce;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (settings.J.JumpTriggerGizmo)
            {
                Gizmos.color = Color.blue;
                if (settings.J.JumpTriggerRelativeTo == SCBallModeSettings.SCJumpSettings.DownOrVel.down || ParentRefs.RB.velocity == Vector3.zero)
                    Gizmos.DrawWireSphere(ParentRefs.ballCollider.position + Vector3.down * settings.J.JumpTriggerOffset, settings.J.JumpTriggerRadius);
                else
                    Gizmos.DrawWireSphere(ParentRefs.ballCollider.position + ParentRefs.RB.velocity.normalized * settings.J.JumpTriggerOffset, settings.J.JumpTriggerRadius);
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
                [Tooltip("Multiplier when holding dash/boost key (shift).")]
                public float boostMultiplier = 2f;
                [Tooltip("Boost stamina use per second.")]
                public float boostStaminaUse = 10f;
                [Tooltip("The horizontal speed at which no new acceleration is allowed by the player.")]
                public float maxSpeed = 3f;
                [Tooltip("Multiplier for the amount of acceleration applied while in the air.")]
                public float airControlFactor = 0.5f;
            }

            [System.Serializable]
            public class SCJumpSettings
            {
                [Header("Jump Settings")]
                [Tooltip("Force applied upwards (or outwards) when the player jumps.")]
                public float jumpForce = 3f;
                [Tooltip("Time after a jump before the player can jump again. Stops superjumps from pressing twice while trigger is still activated.")]
                public float jumpCooldown = 0.2f;
                [Tooltip("Time in which jumps will still be triggered if conditions are met after the key is pressed.")]
                public float checkJumpTime = 0.2f;
                [Tooltip("Time in which jump will still be allowed after the player leaves the ground. Should always be less than jumpCooldown.")]
                public float coyoteeTime = 0.2f;
                [Tooltip("Amount of stamina used by a jump.")]
                public float staminaUsed = 5f;

                [Space]
                public LayerMask JumpableLayers = new LayerMask();
                public float JumpTriggerRadius = 0.3f;
                public float JumpTriggerOffset = 0.1f;
                public enum DownOrVel { down, velocity }
                public DownOrVel JumpTriggerRelativeTo = DownOrVel.down;
                public bool JumpTriggerGizmo = false;
            }
        }

    }
}
