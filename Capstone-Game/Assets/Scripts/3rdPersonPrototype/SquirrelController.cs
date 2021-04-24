using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquirrelControllerSettings;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class SquirrelController : MonoBehaviour
    {
        public SCReferences refs = new SCReferences();
        public SCSettings settings = new SCSettings();
        public SCTriggers triggers = new SCTriggers();
        public SCChildren behaviourScripts = new SCChildren();

        private SCStoredValues vals = new SCStoredValues();

        /// <summary> Time and inputs are not simulated when this is true. </summary>
        private bool debugPause = false;

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

        void Start()
        {
            GetOrMakePause();
        }

        private void Initialize()
        {
            vals.jumpPressed = -10;
            vals.lastGrounded = -10;
            vals.lastJump = -10;
            vals.jumping = false;
            vals.lastOnSurface = -10;
        }

        /// <summary> Makes sure the controller has a reference to the singleton pause-menu object, possibly by creating a new one.
        /// Call in Start() and NOT Awake() for best results. </summary>
        private void GetOrMakePause()
        {
            if (PauseMenu.singleton == null)
            {
                Canvas pre = Resources.Load<Canvas>("Prefabs/PauseCanvas(Dummy)");
                refs.pauseMenu = Instantiate(pre);
            }
            else
            {
                refs.pauseMenu = PauseMenu.singleton;
            }
            Pause();
        }

        /// <summary> Runs all updates for the squirrel character. This is done by calling ManualUpdate() in child scripts based on a statemachine.
        /// Also calls update in camera, and skips ALL calls if the game is paused. </summary>
        void Update()
        {
            if (CheckPause())
                return;

            refs.fCam.UpdCamera(transform, refs.RB);

            if (vals.mState == MovementState.moveAndClimb)
            {
                behaviourScripts.moveAndClimb.ManualUpdate();
            }
            else if (vals.mState == MovementState.ball)
            {
                behaviourScripts.moveAndClimb.ManualUpdate();
            }
        }

        /// <summary> Checks if escape has been pressed (change to include controller buttons etc later),
        /// then swaps between paused/not if pressed, and returns true if the game is paused so that update functions can be skipped. </summary>
        private bool CheckPause()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (debugPause)
                    UnPause();
                else
                    Pause();
            }

            return debugPause;
        }

        /// <summary> Changes settings and (should) run animations required for pausing the game. </summary>
        private void Pause()
        {
            debugPause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            refs.pauseMenu.enabled = true;
        }

        /// <summary> Changes settings and (should) run animations required for UNpausing the game. </summary>
        private void UnPause()
        {
            debugPause = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            refs.pauseMenu.enabled = false;
        }

        private void EnterBallState()
        {
            //Disable normal collider
            //Enable ball collider
            //Change model

            vals.mState = MovementState.ball;
        }

        private void EnterRunState()
        {

        }

        private void EnterGlideState()
        {

        }

        [System.Serializable]
        public class SCReferences
        {
            public Rigidbody RB;
            public Transform head;
            public Transform body;
            public Camera camera;
            public FloatingCamera fCam;
            public Canvas pauseMenu;
        }

        [System.Serializable]
        public class SCTriggers
        {
            /// <summary> Trigger which is used to determine if the player is grounded and can therefore jump etc. </summary>
            public MovementTrigger feet;
            /// <summary> Trigger which is used to determine if the player is running into a wall, to trigger wall climbing. </summary>
            public MovementTrigger wallClimb;
        }

        [System.Serializable]
        public class SCChildren
        {
            public SquirrelMoveAndClimb moveAndClimb;
            public SquirrelGlide glide;
            public SquirrelBall ball;
            public SquirrelFoodGrabber foodGrabber;
        }

        private struct SCStoredValues
        {
            public float lastJump;
            public float lastGrounded;
            public float jumpPressed;
            public bool jumping;
            public float cameraAngle;
            public float lastOnSurface;
            public bool moving;
            public MovementState mState;
        }

        private enum MovementState
        {
            moveAndClimb,
            ball,
            glide
        }
    }
}

//Put in seperate namespace since no other code should need to use this.
//Also seperated to sub-classes for ease of use in editor and autocomplete.
/*
namespace SquirrelControllerSettings
{
    [System.Serializable]
    public class SCSettings
    {
        public SCMoveSettings movement = new SCMoveSettings();
        public SCJumpSettings jump = new SCJumpSettings();
        public SCCameraSettings camera = new SCCameraSettings();
        public SCWallClimbSettings wallClimbing = new SCWallClimbSettings();

        public SCMoveSettings M { get { return movement; } }
        public SCJumpSettings J { get { return jump; } }
        public SCCameraSettings C { get { return camera; } }
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
        public class SCCameraSettings
        {
            [Header("Camera Settings")]
            [Tooltip("Multiplier for converting mouse (or joystick) motion to camera movement.")]
            public float mouseSense = 1;
            [Tooltip("Maximum angle for the vertical motion of the camera.")]
            public float cameraClampMax = 50;
            [Tooltip("Minimum angle for the vertical motion of the camera.")]
            public float cameraClampMin = 0;
        }

        [System.Serializable]
        public class SCWallClimbSettings
        {
            [Header("Wallclimb Settings")]
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
        }
    }
}
*/