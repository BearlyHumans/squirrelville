using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquirrelControllerSettings;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class SquirrelController : MonoBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SCReferences refs = new SCReferences();
        public SCChildren behaviourScripts = new SCChildren();

        private SCStoredValues vals = new SCStoredValues();

        /// <summary> Time and inputs are not simulated when this is true. </summary>
        private bool debugPause = false;

        //~~~~~~~~~~ PROPERTIES ~~~~~~~~~~

        public bool TouchingSomething
        {
            get { return vals.touchingSomething; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~

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

        void OnCollisionStay(Collision collision)
        {
            vals.touchingSomething = true;
        }

        void OnCollisionExit(Collision collision)
        {
            vals.touchingSomething = false;
        }

        //~~~~~~~~~~ FUNCTIONS ~~~~~~~~~~

        private void Initialize()
        {
            vals.jumpPressed = -10;
            vals.lastGrounded = -10;
            vals.lastJump = -10;
            vals.jumping = false;
            vals.lastOnSurface = -10;

            if (behaviourScripts.moveAndClimb == null)
                behaviourScripts.moveAndClimb = GetComponent<SquirrelMoveAndClimb>();
            if (behaviourScripts.ball == null)
                behaviourScripts.ball = GetComponent<SquirrelBall>();
            if (behaviourScripts.glide == null)
                behaviourScripts.glide = GetComponent<SquirrelGlide>();
            if (behaviourScripts.foodGrabber == null)
                behaviourScripts.foodGrabber = GetComponent<SquirrelFoodGrabber>();

            EnterRunState();
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

            refs.fCam.UpdateCamRotFromImput();

            //Debug State Changes
            if (Input.GetKeyDown(KeyCode.B))
                EnterBallState();
            if (Input.GetKeyDown(KeyCode.R))
                EnterRunState();
            if (Input.GetKeyDown(KeyCode.G))
                EnterGlideState();

            //State Machine
            if (vals.mState == MovementState.moveAndClimb)
            {
                behaviourScripts.moveAndClimb.ManualUpdate();
            }
            else if (vals.mState == MovementState.ball)
            {
                behaviourScripts.ball.ManualUpdate();
            }
            else if (vals.mState == MovementState.glide)
            {
                behaviourScripts.glide.ManualUpdate();
            }

            refs.fCam.UpdateCamPos();
            refs.fCam.UpdateDolly();
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
            refs.runBody.SetActive(false);
            refs.ballBody.SetActive(true);

            refs.RB.constraints = RigidbodyConstraints.None;
            refs.RB.useGravity = true;

            refs.fCam.UseRelativeAngles = false;
            refs.fCam.cameraTarget = gameObject;

            vals.mState = MovementState.ball;
        }

        private void EnterRunState()
        {
            refs.runBody.SetActive(true);
            refs.ballBody.SetActive(false);

            refs.RB.constraints = RigidbodyConstraints.FreezeRotation;

            refs.fCam.UseRelativeAngles = true;
            refs.fCam.cameraTarget = refs.model.gameObject;

            vals.mState = MovementState.moveAndClimb;
        }

        private void EnterGlideState()
        {

        }

        //~~~~~~~~~~ DATA STRUCTURES ~~~~~~~~~~

        [System.Serializable]
        public class SCReferences
        {
            public Rigidbody RB;
            public Transform body;
            public Transform model;
            public Camera camera;
            public CameraGimbal fCam;
            public Canvas pauseMenu;
            public GameObject runBody;
            public GameObject ballBody;
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
            public bool touchingSomething;
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