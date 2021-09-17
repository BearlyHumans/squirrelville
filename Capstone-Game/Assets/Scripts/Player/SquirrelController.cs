using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Stamina))]
    [RequireComponent(typeof(Rigidbody))]
    public class SquirrelController : MonoBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SCReferences refs = new SCReferences();
        public SCChildren behaviourScripts = new SCChildren();

        public bool debugMessages = false;

        [SerializeField]
        private List<ParameterChangeEvent> animationEvents = new List<ParameterChangeEvent>();

        private SCStoredValues vals = new SCStoredValues();

        //~~~~~~~~~~ PROPERTIES ~~~~~~~~~~

        public bool TouchingSomething
        {
            get { return vals.touchingSomething; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~

        void Awake()
        {
            refs.RB.useGravity = false;

            Initialize();
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
            if (behaviourScripts.foodGrabber == null)
                behaviourScripts.foodGrabber = GetComponent<SquirrelFoodGrabber>();

            if (refs.stamina == null)
                refs.stamina = GetComponent<Stamina>();

            EnterRunState();
        }

        /// <summary> Runs all updates for the squirrel character. This is done by calling ManualUpdate() in child scripts based on a statemachine.
        /// Also calls update in camera, and skips ALL calls if the game is paused. </summary>
        void Update()
        {
            if (PauseMenu.paused)
                return;

            if (vals.frozen)
                return;
			
            CallAnimationEvents(AnimationTrigger.frameStart);
			
            refs.fCam.UpdateCamRotFromInput();

            //Debug State Changes
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (vals.mState == MovementState.ball)
                    EnterRunState();
                else if (CanEnterBallState())
                    EnterBallState();
            }

            if (!CanEnterBallState())
                EnterRunState();

            //State Machine
            if (vals.mState == MovementState.moveAndClimb)
            {
                behaviourScripts.moveAndClimb.ManualUpdate();
            }
            else if (vals.mState == MovementState.ball)
            {
                behaviourScripts.ball.ManualUpdate();
            }

            refs.fCam.UpdateCamPos();
            refs.fCam.UpdateDolly();
        }

        private bool CanEnterBallState()
        {
            int foodCount = behaviourScripts.foodGrabber.GetFoodCount();
            int foodCountBallForm = behaviourScripts.foodGrabber.foodCountBallForm;
            return foodCount >= foodCountBallForm;
        }

        private void EnterBallState()
        {
            //Disable normal collider
            //Enable ball collider
            //Change model
            refs.animator.CrossFade("Idle", 0f);
            refs.animator.Update(1f);
            refs.runBody.SetActive(false);
            refs.ballBody.SetActive(true);
            refs.ballBody.transform.rotation = refs.runBody.transform.rotation;

            refs.RB.constraints = RigidbodyConstraints.None;
            refs.RB.useGravity = true;

            refs.fCam.UseRelativeAngles = false;
            refs.fCam.cameraTarget = refs.ballModel.gameObject;

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

        public void FreezeMovement()
        {
            vals.frozen = true;
            refs.RB.velocity = Vector3.zero;
        }

        public void UnfreezeMovement()
        {
            vals.frozen = false;
        }

        public void CallAnimationEvents(AnimationTrigger trigger)
        {
            if (debugMessages)
                print("Triggered Event: " + trigger.ToString());

            foreach (ParameterChangeEvent PCE in animationEvents)
            {
                if (PCE.trigger == trigger)
                    ChangeParameter(PCE);
            }
        }

        private void ChangeParameter(ParameterChangeEvent PCE)
        {
            if (PCE.parameterChange.type == ExposedAnimationParameter.ParamTypes.Bool)
                refs.animator.SetBool(PCE.parameterChange.paramName, (PCE.parameterChange.setToValue > 0) ? true : false);
            else if (PCE.parameterChange.type == ExposedAnimationParameter.ParamTypes.Int)
                refs.animator.SetInteger(PCE.parameterChange.paramName, (int)PCE.parameterChange.setToValue);
            else if (PCE.parameterChange.type == ExposedAnimationParameter.ParamTypes.Float)
                refs.animator.SetFloat(PCE.parameterChange.paramName, PCE.parameterChange.setToValue);
        }

        //~~ DATA STRUCTURES ~~

        [System.Serializable]
        public class SCReferences
        {
            public Rigidbody RB;
            public Transform body;
            public Transform ballModel;
            public Transform ballCollider;
            public Transform model;
            public Camera camera;
            public CameraGimbal fCam;
            public Stamina stamina;
            public Animator animator;
            public GameObject runBody;
            public GameObject ballBody;
        }

        [System.Serializable]
        public class SCChildren
        {
            public SquirrelMoveAndClimb moveAndClimb;
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
            public bool frozen;
            public float stamina;
            public bool usingStamina;
            public MovementState mState;
        }

        private enum MovementState
        {
            moveAndClimb,
            ball,
            glide
        }

        [System.Serializable]
        private class ParameterChangeEvent
        {
            [Tooltip("These events are called from the code when the trigger happens." +
                "Events endiing in 'ing' are called every frame that the trigger is true, and the others are called only on the first frame." +
                "Multiple Events can be made for each trigger and they will all be called.")]
            public AnimationTrigger trigger;
            public ExposedAnimationParameter parameterChange;
        }

        public enum AnimationTrigger
        {
            frameStart, //Done
            moving,
            notMoving,
            hop,
            becomeIdle,
            randomIdle1,
            randomIdle2,
            chargingJump,
            jump,
            landJump,
            dashing,
            sneaking,
            slipping,
            climbing,
            falling,
            rolling,
            barking,
            eating,
            collision,
            humanAttack
        }

        [System.Serializable]
        private class ExposedAnimationParameter
        {
            [Tooltip("Set this to the type of the parameter you want to change.")]
            public ParamTypes type;
            [Tooltip("Set this to the name of the parameter you want to change when the trigger occurs.")]
            public string paramName;
            [Tooltip("Use 0/1 for false/true.")]
            public float setToValue;

            public enum ParamTypes
            {
                Float,
                Int,
                Bool
            }
        }
    }
}