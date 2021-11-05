using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Player
{
    [RequireComponent(typeof(Stamina))]
    [RequireComponent(typeof(Rigidbody))]
    public class SquirrelController : MonoBehaviour
    {
        //~~~~~~~~~~ CLASS VARIABLES ~~~~~~~~~~

        public SCReferences refs = new SCReferences();
        public SCChildren behaviourScripts = new SCChildren();

        public SCGiantBallSettings giantSettings = new SCGiantBallSettings();
        public SCShadowProjectionSettings shadowSettings = new SCShadowProjectionSettings();

        public bool debugMessages = false;

        [SerializeField]
#if UNITY_EDITOR
        [AnimationEvent.CustomListTitles("trigger", "paramName", "setToValue", "type")]
#endif
        private List<AnimationEvent> animationEvents = new List<AnimationEvent>();

        [SerializeField]
#if UNITY_EDITOR
        [SoundEvent.CustomListTitles("trigger", "action", "soundName")]
#endif
        private List<SoundEvent> soundEvents = new List<SoundEvent>();

        [SerializeField]
#if UNITY_EDITOR
        [ParticleEvent.CustomListTitles("trigger", "action", "particleName")]
#endif
        private List<ParticleEvent> particleEvents = new List<ParticleEvent>();

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

        void OnCollisionEnter(Collision collision)
        {
            if (vals.mState == MovementState.ball || vals.mState == MovementState.giantBall)
                behaviourScripts.ball.BallCollision(collision.impulse);
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
            vals.preFreezeConstraints = refs.RB.constraints;

            refs.fCam.SetSensitivity(PlayerPrefs.GetFloat("cameraSensitivity", 150));

            if (behaviourScripts.moveAndClimb == null)
                behaviourScripts.moveAndClimb = GetComponent<SquirrelMoveAndClimb>();
            if (behaviourScripts.ball == null)
                behaviourScripts.ball = GetComponent<SquirrelBall>();
            if (behaviourScripts.foodGrabber == null)
                behaviourScripts.foodGrabber = GetComponent<SquirrelFoodGrabber>();

            if (refs.stamina == null)
                refs.stamina = GetComponent<Stamina>();

            if (refs.runCameraTarget == null)
                refs.runCameraTarget = refs.model;

            EnterRunState();

            CallEvents(EventTrigger.gameStart);
        }

        /// <summary> Runs all updates for the squirrel character. This is done by calling ManualUpdate() in child scripts based on a statemachine.
        /// Also calls update in camera, and skips ALL calls if the game is paused. </summary>
        void Update()
        {
            if (PauseMenu.paused)
            {
                if (vals.wasPaused == false)
                {
                    refs.SFXControl.Pause();
                    OnPause();
                }
                vals.wasPaused = true;
                return;
            }
            else
            {
                if (vals.wasPaused)
                {
                    refs.SFXControl.Resume();
                    OnResume();
                }
                vals.wasPaused = false;
            }

            if (vals.frozen)
                return;

            CallEvents(EventTrigger.frameStart);

            refs.fCam.UpdateCamRotFromInput();

            if (vals.mState != MovementState.giantBall)
            {
                if (Input.GetButtonDown("BallToggle"))
                {
                    if (vals.mState == MovementState.ball)
                        EnterRunState();
                    else if (CanEnterBallState())
                        EnterBallState();
                }

                if (!behaviourScripts.foodGrabber.CanBeBall())
                    EnterRunState();
            }

            //State Machine
            if (vals.mState == MovementState.moveAndClimb)
            {
                behaviourScripts.moveAndClimb.ManualUpdate();
            }
            else if (vals.mState == MovementState.ball || vals.mState == MovementState.giantBall)
            {
                behaviourScripts.ball.ManualUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (PauseMenu.paused || vals.frozen)
                return;
            
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
            refs.fCam.cameraTarget = refs.ballModel;

            refs.shadowProjector.fieldOfView = shadowSettings.ballShadowFOV;
            
            CallEvents(EventTrigger.stopRunning);
            CallEvents(EventTrigger.startBeingBall);

            vals.mState = MovementState.ball;
        }

        public void EnterGiantBallState()
        {
            //Disable normal collider
            //Enable ball collider
            //Change model
            
            refs.animator.CrossFade("Idle", 0f);
            refs.animator.Update(1f);
            refs.runBody.SetActive(false);
            refs.ballBody.SetActive(true);
            refs.ballBody.transform.rotation = refs.runBody.transform.rotation;
            
            //Change ball so it is giant:
            refs.ballBody.transform.localScale *= giantSettings.sizeMultiplier;
            refs.fCam.programmerSettings.minDistance *= giantSettings.cameraMultiplier;
            refs.fCam.programmerSettings.maxDistance *= giantSettings.cameraMultiplier;
            behaviourScripts.ball.settings.squishiness.squishAmount *= giantSettings.squishinessMultiplier;
            if (giantSettings.allowBoosting)
                behaviourScripts.ball.settings.jump.JumpTriggerRadius *= giantSettings.sizeMultiplier;
            behaviourScripts.ball.isGiant = true;

            refs.RB.constraints = RigidbodyConstraints.None;
            refs.RB.useGravity = true;

            refs.fCam.UseRelativeAngles = false;
            refs.fCam.cameraTarget = refs.ballModel;

            refs.shadowProjector.fieldOfView = shadowSettings.giantShadowFOV;

            CallEvents(EventTrigger.stopBeingBall);
            CallEvents(EventTrigger.stopRunning);
            CallEvents(EventTrigger.startGiantBall);

            vals.mState = MovementState.giantBall;

            giantSettings.inGiantMode = true;
        }

        public void LeaveGiantBallState()
        {
            
            //Change ball so it is NOT giant:
            refs.ballBody.transform.localScale /= giantSettings.sizeMultiplier;
            refs.fCam.programmerSettings.minDistance /= giantSettings.cameraMultiplier;
            refs.fCam.programmerSettings.maxDistance /= giantSettings.cameraMultiplier;
            behaviourScripts.ball.settings.squishiness.squishAmount /= giantSettings.squishinessMultiplier;
            if (giantSettings.allowBoosting)
                behaviourScripts.ball.settings.jump.JumpTriggerRadius /= giantSettings.sizeMultiplier;
            behaviourScripts.ball.isGiant = false;

            refs.runBody.SetActive(true);
            refs.ballBody.SetActive(false);

            refs.RB.constraints = RigidbodyConstraints.FreezeRotation;

            refs.fCam.UseRelativeAngles = true;
            refs.fCam.cameraTarget = refs.runCameraTarget;

            refs.shadowProjector.fieldOfView = shadowSettings.runShadowFOV;

            CallEvents(EventTrigger.stopGiantBall);
            CallEvents(EventTrigger.stopBeingBall);
            CallEvents(EventTrigger.startRunning);

            vals.mState = MovementState.moveAndClimb;

            giantSettings.inGiantMode = false;
        }

        private void EnterRunState()
        {
            refs.runBody.SetActive(true);
            refs.ballBody.SetActive(false);

            refs.RB.constraints = RigidbodyConstraints.FreezeRotation;

            refs.fCam.UseRelativeAngles = true;
            refs.fCam.cameraTarget = refs.runCameraTarget;

            refs.shadowProjector.fieldOfView = shadowSettings.runShadowFOV;
            
            CallEvents(EventTrigger.stopBeingBall);
            CallEvents(EventTrigger.startRunning);

            vals.mState = MovementState.moveAndClimb;
        }

        /// <summary> Freeze the squirrel and play the stunned animation and/or particle effects (call UnfreezeMovement to reverse). </summary>
        public void FreezeAndStun()
        {
            if (vals.frozen == false)
            {
                vals.frozen = true;
                vals.preFreezeConstraints = refs.RB.constraints;
                refs.RB.velocity = Vector3.zero;
                refs.RB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                refs.RB.useGravity = true;
                CallEvents(EventTrigger.notMoving);
                CallEvents(EventTrigger.landJump);
                CallEvents(EventTrigger.humanAttack);
            }
        }

        /// <summary> Freeze the squirrel and play the idle animation - for talking to NPCs (call UnfreezeMovement to reverse). </summary>
        public void FreezeMovement()
        {
            if (vals.frozen == false)
            {
                vals.frozen = true;
                vals.preFreezeConstraints = refs.RB.constraints;
                refs.RB.velocity = Vector3.zero;
                refs.RB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                refs.RB.useGravity = true;
                CallEvents(EventTrigger.notMoving);
                CallEvents(EventTrigger.landJump);
                refs.animator.CrossFade("Idle", 0f);
                refs.animator.Update(1f);
            }
        }

        /// <summary> Freeze the squirrel and play the stunned animation and/or particle effects (call UnfreezeMovement to reverse). </summary>
        public void UnfreezeMovement()
        {
            vals.frozen = false;
            refs.RB.constraints = vals.preFreezeConstraints;
        }

        private void OnPause()
        {

        }

        private void OnResume()
        {
            refs.fCam.SetSensitivity(PlayerPrefs.GetFloat("cameraSensitivity", 150));
        }

        public void CallEvents(EventTrigger trigger)
        {
            foreach (AnimationEvent AE in animationEvents)
            {
                if (AE.trigger == trigger)
                    ChangeAnimationParameter(AE);
            }
            foreach (SoundEvent SE in soundEvents)
            {
                if (SE.trigger == trigger)
                    DoSoundEvent(SE);
            }
            foreach (ParticleEvent PE in particleEvents)
            {
                if (PE.trigger == trigger)
                    DoParticleEvent(PE);
            }
        }

        private void ChangeAnimationParameter(AnimationEvent AE)
        {
            if (AE.type == AnimationEvent.ParamTypes.Bool)
                refs.animator.SetBool(AE.paramName, (AE.setToValue > 0) ? true : false);
            else if (AE.type == AnimationEvent.ParamTypes.Int)
                refs.animator.SetInteger(AE.paramName, (int)AE.setToValue);
            else if (AE.type == AnimationEvent.ParamTypes.Float)
                refs.animator.SetFloat(AE.paramName, AE.setToValue);
            else if (AE.type == AnimationEvent.ParamTypes.PlayOnce)
                refs.animator.Play(AE.paramName);
        }

        private void DoSoundEvent(SoundEvent SE)
        {
            if (SE.action == SoundEvent.Action.PlayOrRestart)
                refs.SFXControl.PlaySound(SE.soundName);
            else if (SE.action == SoundEvent.Action.PlayOrContinue)
                refs.SFXControl.PlayOrContinueSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.PlayIfQuiet)
                refs.SFXControl.PlayIfQuiet(SE.soundName);
            else if (SE.action == SoundEvent.Action.PlayIfSilent)
                refs.SFXControl.PlayIfSilent(SE.soundName);
            else if (SE.action == SoundEvent.Action.PlayAndLoop)
                refs.SFXControl.LoopSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.StopAfterLoop)
                refs.SFXControl.StopLoopingSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.StopImmediately)
                refs.SFXControl.StopSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.PlayAswell)
                refs.SFXControl.PlayAswell(SE.soundName);
            else if (SE.action == SoundEvent.Action.Block)
                refs.SFXControl.BlockSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.Unblock)
                refs.SFXControl.UnBlockSound(SE.soundName);
            else if (SE.action == SoundEvent.Action.Pause)
                refs.SFXControl.PauseSound(SE.soundName);
        }

        private void DoParticleEvent(ParticleEvent PE)
        {
            if (PE.action == ParticleEvent.Action.Play)
                refs.particlesController.PlayParticle(PE.particleName);
            else if (PE.action == ParticleEvent.Action.PlayOrContinue)
                refs.particlesController.PlayOrContinueParticle(PE.particleName);
            else if (PE.action == ParticleEvent.Action.Stop)
                refs.particlesController.StopParticle(PE.particleName);
            else if (PE.action == ParticleEvent.Action.Enable)
                refs.particlesController.EnableParticle(PE.particleName);
            else if (PE.action == ParticleEvent.Action.Disable)
                refs.particlesController.DisableParticle(PE.particleName);
            else if (PE.action == ParticleEvent.Action.Pause)
                refs.particlesController.PauseParticle(PE.particleName);
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
            public Transform runCameraTarget;
            public SFXController SFXControl;
            public ParticlesController particlesController;
            public Projector shadowProjector;
        }

        [System.Serializable]
        public class SCGiantBallSettings
        {
            [Header("Warning: these only apply when transforming - changing in play mode will break things.")]
            public bool allowBoosting = true;
            public float sizeMultiplier = 8f;
            public float cameraMultiplier = 3f;
            public float squishinessMultiplier = 0.5f;
            [HideInInspector]
            public bool inGiantMode = false;
        }

        [System.Serializable]
        public class SCShadowProjectionSettings
        {
            public float runShadowFOV = 20f;
            public float ballShadowFOV = 30f;
            public float giantShadowFOV = 100f;
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
            public RigidbodyConstraints preFreezeConstraints;
            public float stamina;
            public bool usingStamina;
            public MovementState mState;
            public bool wasPaused;
        }

        private enum MovementState
        {
            moveAndClimb,
            ball,
            glide,
            giantBall
        }

        public enum EventTrigger
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
            notClimbing,
            falling,
            rolling,
            barking,
            eat,
            collision,
            humanAttack,
            startDashing,
            spit,
            stopRolling,
            gameStart,
            stopDashing,
            giantRolling,
            giantStopRolling,
            ballBounce,
            giantBounce,
            startBeingBall,
            stopBeingBall,
            startRunning,
            stopRunning,
            startGiantBall,
            stopGiantBall
        }

        [System.Serializable]
        private class AnimationEvent
        {
            [Tooltip("These events are called from the code when the trigger happens." +
                "Events endiing in 'ing' are called every frame that the trigger is true, and the others are called only on the first frame." +
                "Multiple Events can be made for each trigger and they will all be called.")]
            public EventTrigger trigger;
            [Header("Parameter Change:")]
            [Tooltip("Set this to the type of the parameter you want to change. PlayOnce is instead used to directly start an animation - put the animations name in the parameter slot.")]
            public ParamTypes type;
            [Tooltip("Set this to the name of the parameter you want to change when the trigger occurs. If using 'PlayOnce' make this the name of the animation instead.")]
            public string paramName;
            [Tooltip("Use 0/1 for false/true.")]
            public float setToValue;

            public enum ParamTypes
            {
                Float,
                Int,
                Bool,
                PlayOnce
            }

#if UNITY_EDITOR
            public class CustomListTitlesAttribute : PropertyAttribute
            {
                public string TriggerName;
                public string ParameterName;
                public string ParamType;
                public string ValueName;
                public CustomListTitlesAttribute(string Trigger, string Parameter, string Value, string PType)
                {
                    TriggerName = Trigger;
                    ParameterName = Parameter;
                    ValueName = Value;
                    ParamType = PType;
                }

                public CustomListTitlesAttribute(string Trigger, string Parameter, string Value)
                {
                    TriggerName = Trigger;
                    ParameterName = Parameter;
                    ValueName = Value;
                    ParamType = "Int";
                }
            }

            [CustomPropertyDrawer(typeof(CustomListTitlesAttribute))]
            public class CustomListTitlesDrawer : PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return EditorGUI.GetPropertyHeight(property, label, true);
                }

                protected virtual CustomListTitlesAttribute Atribute
                {
                    get { return (CustomListTitlesAttribute)attribute; }
                }

                SerializedProperty TitleNameProp;

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    string FullPathName = property.propertyPath + "." + Atribute.TriggerName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string trigger = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.ParameterName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string parameter = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.ValueName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string value = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.ParamType;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string paramType = GetTitle();

                    string combinedName;
                    if (paramType == "PlayOnce")
                        combinedName = "After " + trigger + " start playing " + parameter;
                    else
                        combinedName = "After " + trigger + " set " + parameter + " to " + value;

                    EditorGUI.PropertyField(position, property, new GUIContent(combinedName, label.tooltip), true);
                }

                private string GetTitle()
                {
                    switch (TitleNameProp.propertyType)
                    {
                        case SerializedPropertyType.Generic:
                            break;
                        case SerializedPropertyType.Integer:
                            return TitleNameProp.intValue.ToString();
                        case SerializedPropertyType.Boolean:
                            return TitleNameProp.boolValue.ToString();
                        case SerializedPropertyType.Float:
                            return TitleNameProp.floatValue.ToString();
                        case SerializedPropertyType.String:
                            return TitleNameProp.stringValue;
                        case SerializedPropertyType.Color:
                            return TitleNameProp.colorValue.ToString();
                        case SerializedPropertyType.ObjectReference:
                            return TitleNameProp.objectReferenceValue.ToString();
                        case SerializedPropertyType.Enum:
                            return TitleNameProp.enumNames[TitleNameProp.enumValueIndex];
                        case SerializedPropertyType.Vector2:
                            return TitleNameProp.vector2Value.ToString();
                        case SerializedPropertyType.Vector3:
                        default:
                            break;
                    }
                    return "";
                }
            }
#endif
        }

        [System.Serializable]
        private class SoundEvent
        {
            [Tooltip("These events are called from the code when the trigger happens." +
                "Events endiing in 'ing' are called every frame that the trigger is true, and the others are called only on the first frame." +
                "Multiple Events can be made for each trigger and they will all be called.")]
            public EventTrigger trigger;
            [Tooltip("Action to perform with the sound.")]
            public Action action;
            [Tooltip("Sound to effect.")]
            public string soundName;

            public enum Action
            {
                PlayOrRestart,
                PlayOrContinue,
                PlayIfQuiet,
                PlayIfSilent,
                PlayAndLoop,
                StopAfterLoop,
                StopImmediately,
                PlayAswell,
                Block,
                Unblock,
                Pause
            }

#if UNITY_EDITOR
            public class CustomListTitlesAttribute : PropertyAttribute
            {
                public string TriggerName;
                public string StateName;
                public string SoundName;
                public CustomListTitlesAttribute(string Trigger, string State, string Sound)
                {
                    TriggerName = Trigger;
                    StateName = State;
                    SoundName = Sound;
                }
            }

            [CustomPropertyDrawer(typeof(CustomListTitlesAttribute))]
            public class CustomListTitlesDrawer : PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return EditorGUI.GetPropertyHeight(property, label, true);
                }

                protected virtual CustomListTitlesAttribute Atribute
                {
                    get { return (CustomListTitlesAttribute)attribute; }
                }

                SerializedProperty TitleNameProp;

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    string FullPathName = property.propertyPath + "." + Atribute.TriggerName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string trigger = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.StateName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string state = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.SoundName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string sound = GetTitle();

                    string combinedName = "After " + trigger + " " + state + " " + sound;

                    EditorGUI.PropertyField(position, property, new GUIContent(combinedName, label.tooltip), true);
                }

                private string GetTitle()
                {
                    switch (TitleNameProp.propertyType)
                    {
                        case SerializedPropertyType.Generic:
                            break;
                        case SerializedPropertyType.Integer:
                            return TitleNameProp.intValue.ToString();
                        case SerializedPropertyType.Boolean:
                            return TitleNameProp.boolValue.ToString();
                        case SerializedPropertyType.Float:
                            return TitleNameProp.floatValue.ToString();
                        case SerializedPropertyType.String:
                            return TitleNameProp.stringValue;
                        case SerializedPropertyType.Color:
                            return TitleNameProp.colorValue.ToString();
                        case SerializedPropertyType.ObjectReference:
                            return TitleNameProp.objectReferenceValue.ToString();
                        case SerializedPropertyType.Enum:
                            return TitleNameProp.enumNames[TitleNameProp.enumValueIndex];
                        case SerializedPropertyType.Vector2:
                            return TitleNameProp.vector2Value.ToString();
                        case SerializedPropertyType.Vector3:
                        default:
                            break;
                    }
                    return "";
                }
            }
#endif
        }

        [System.Serializable]
        private class ParticleEvent
        {
            [Tooltip("These events are called from the code when the trigger happens." +
                "Events endiing in 'ing' are called every frame that the trigger is true, and the others are called only on the first frame." +
                "Multiple Events can be made for each trigger and they will all be called.")]
            public EventTrigger trigger;
            [Tooltip("Action to do with the particle.")]
            public Action action;
            [Tooltip("Particle to play/stop.")]
            public string particleName;

            public enum Action
            {
                Play,
                PlayOrContinue,
                Stop,
                Enable,
                Disable,
                Pause,
                PauseEmission,
                ResumeEmission
            }

#if UNITY_EDITOR
            public class CustomListTitlesAttribute : PropertyAttribute
            {
                public string TriggerName;
                public string StateName;
                public string ParticleName;
                public CustomListTitlesAttribute(string Trigger, string State, string Particle)
                {
                    TriggerName = Trigger;
                    StateName = State;
                    ParticleName = Particle;
                }
            }

            [CustomPropertyDrawer(typeof(CustomListTitlesAttribute))]
            public class CustomListTitlesDrawer : PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return EditorGUI.GetPropertyHeight(property, label, true);
                }

                protected virtual CustomListTitlesAttribute Atribute
                {
                    get { return (CustomListTitlesAttribute)attribute; }
                }

                SerializedProperty TitleNameProp;

                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    string FullPathName = property.propertyPath + "." + Atribute.TriggerName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string trigger = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.StateName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string state = GetTitle();

                    FullPathName = property.propertyPath + "." + Atribute.ParticleName;
                    TitleNameProp = property.serializedObject.FindProperty(FullPathName);
                    string particle = GetTitle();

                    string combinedName = "After " + trigger + " " + state + " " + particle;

                    EditorGUI.PropertyField(position, property, new GUIContent(combinedName, label.tooltip), true);
                }

                private string GetTitle()
                {
                    switch (TitleNameProp.propertyType)
                    {
                        case SerializedPropertyType.Generic:
                            break;
                        case SerializedPropertyType.Integer:
                            return TitleNameProp.intValue.ToString();
                        case SerializedPropertyType.Boolean:
                            return TitleNameProp.boolValue.ToString();
                        case SerializedPropertyType.Float:
                            return TitleNameProp.floatValue.ToString();
                        case SerializedPropertyType.String:
                            return TitleNameProp.stringValue;
                        case SerializedPropertyType.Color:
                            return TitleNameProp.colorValue.ToString();
                        case SerializedPropertyType.ObjectReference:
                            return TitleNameProp.objectReferenceValue.ToString();
                        case SerializedPropertyType.Enum:
                            return TitleNameProp.enumNames[TitleNameProp.enumValueIndex];
                        case SerializedPropertyType.Vector2:
                            return TitleNameProp.vector2Value.ToString();
                        case SerializedPropertyType.Vector3:
                        default:
                            break;
                    }
                    return "";
                }
            }
#endif
        }
    }


}