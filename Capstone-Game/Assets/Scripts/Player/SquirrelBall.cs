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

        public Transform ballModel;
        
        public SCBallModeSettings settings = new SCBallModeSettings();

        [HideInInspector]
        public bool isGiant = false;
        
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

        void Awake()
        {
            vals.normalScale = ballModel.localScale;
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
                bool feetTouching = Physics.CheckSphere(ParentRefs.ballCollider.position + (Vector3.down * settings.J.JumpTriggerOffset),
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
                if ((Input.GetButton("Dash") || Input.GetAxis("Dash") > 0) && ParentRefs.stamina.UseStamina(settings.M.boostStaminaUse * Time.deltaTime))
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

            if (Grounded && ParentRefs.RB.velocity.magnitude > 0.3f)
            {
                if (isGiant)
                    PARENT.CallEvents(SquirrelController.EventTrigger.giantRolling);
                else
                    PARENT.CallEvents(SquirrelController.EventTrigger.rolling);
            }
            else
            {
                if (isGiant)
                    PARENT.CallEvents(SquirrelController.EventTrigger.giantStopRolling);
                else
                    PARENT.CallEvents(SquirrelController.EventTrigger.stopRolling);
            }
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
                
                ParentRefs.RB.velocity += Vector3.up * settings.J.jumpForce;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (settings.J.JumpTriggerGizmo)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(ParentRefs.ballCollider.position + (Vector3.down * settings.J.JumpTriggerOffset), settings.J.JumpTriggerRadius);
            }
        }

        public void BallCollision(Vector3 impulse)
        {
            float force = impulse.magnitude;
            if (force > vals.lastSquishForce * 0.5f)
            {
                if (vals.squishAnimation != null)
                    StopCoroutine(vals.squishAnimation);
                vals.squishAnimation = StartCoroutine(SquishBall(force));
            }
            if (force > settings.S.squishForceRange.x)
            {
                if (isGiant)
                    PARENT.CallEvents(SquirrelController.EventTrigger.giantBounce);
                else
                    PARENT.CallEvents(SquirrelController.EventTrigger.ballBounce);
            }
        }

        private IEnumerator SquishBall(float force)
        {
            vals.lastSquishForce = force;
            ballModel.localScale = vals.normalScale;
            float startTime = Time.time;
            while (Time.time < startTime + settings.S.squishCurve.length)
            {
                float forceMultiplier = 1;
                if (force < settings.S.squishForceRange.x)
                    forceMultiplier = settings.S.squishMinForceMultiplier;
                else if (force > settings.S.squishForceRange.y)
                    forceMultiplier = 1;
                else
                {
                    float a = settings.S.squishForceRange.y;
                    float b = settings.S.squishForceRange.x;
                    float m = settings.S.squishMinForceMultiplier;
                    forceMultiplier = (force - b) * ((1 - m) / (a - b)) + m;
                }
                float scaling = (settings.S.squishCurve.Evaluate(Time.time - startTime) * forceMultiplier * settings.S.squishAmount) + 1;

                ballModel.localScale = vals.normalScale * scaling;
                yield return new WaitForEndOfFrame();
            }
            ballModel.localScale = vals.normalScale;
            vals.lastSquishForce = 0;
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
            public Vector3 normalScale;
            public Coroutine squishAnimation;
            public float lastSquishForce;
        }

        [System.Serializable]
        public class SCBallModeSettings
        {
            public SCMoveSettings movement = new SCMoveSettings();
            public SCSquishinessSettings squishiness = new SCSquishinessSettings();
            public SCJumpSettings jump = new SCJumpSettings();

            public SCMoveSettings M { get { return movement; } }
            public SCJumpSettings J { get { return jump; } }
            public SCSquishinessSettings S { get { return squishiness; } }

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
            public class SCSquishinessSettings
            {
                [Header("Squishiness Settings")]
                [Tooltip("Controls the ball models size during the squish animation. Value is a fraction of the normal size that should be added to the size during anmation, so value should start and end on 0." +
                    "The curve will be smaller for smaller forces based on settings below, but what is shown here is the maximum change.")]
                public AnimationCurve squishCurve;
                [Tooltip("Minimum (x) and Maximum (y) range of forces that change how much squishing happens. If the force is >= y it will do the curve exactly," +
                    "if it is <= x it will shrink the curve by the multiplier below, and if it is in between it will shrink it proportionally.")]
                public Vector2 squishForceRange = new Vector2(0f, 5f);
                [Tooltip("The multiplier on the squish effect that happens when the LOWEST force is encountered, and the bottom of the range controlled by the force.")]
                [Range(0f, 1f)]
                public float squishMinForceMultiplier = 0.2f;
                [Tooltip("A pure multiplier on the squish effect. 0 is off, 0.5 is half, 1 is normal.")]
                [Range(0f, 1f)]
                public float squishAmount = 1;
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
                private DownOrVel JumpTriggerRelativeTo = DownOrVel.down; //Don't mind this just trying something
                public bool JumpTriggerGizmo = false;
            }
        }

    }
}
