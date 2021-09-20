using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Player
{
    [SelectionBase]
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
            get { return triggers.feet.triggered && Time.time < vals.lastOnSurface + settings.J.coyoteeTime; }
        }

        //~~~~~~~~~~ EVENTS ~~~~~~~~~~

        void Awake()
        {
            if (PARENT == null)
                PARENT = GetComponentInParent<SquirrelController>();

            vals.lastRotationDir = Vector3.down;
        }

        //~~~~~~~~~~ MAIN UPDATE FUNCTIONS ~~~~~~~~~~

        /// <summary> Call all the update steps for movement, climing and jumping. </summary>
        public override void ManualUpdate()
        {
            UpdInput();
            UpdMove();
            DoJumpChecks();
            FindAndRotateToSurface();
            RotateModel();
            UpdAnimator();
        }

        private void UpdAnimator()
        {
            if (vals.moving)
                PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.moving);
            else if (vals.jumping == false)
                PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.notMoving);
        }

        private void UpdInput()
        {
            //Joystick or WASD motion:
            vals.desiredDirection = new Vector3();
            Vector3 camForward = Vector3.Cross(transform.forward, ParentRefs.fCam.transform.right);
            vals.desiredDirection += camForward * Input.GetAxis("Vertical");
            Vector3 camright = Vector3.Cross(camForward, transform.forward);
            vals.desiredDirection += camright * Input.GetAxis("Horizontal");

            vals.desiredDirection.Normalize();

            //Dash button:
            vals.dashing = false;
            if (Input.GetButton("Dash") && ParentRefs.stamina.UseStamina(settings.S.dashStamPerSec * Time.deltaTime))
            {
                if (vals.dashing == false)
                    vals.startedDashing = Time.time;
                vals.dashing = true;
            }
            else if (vals.desiredDirection != Vector3.zero)
                ParentRefs.stamina.UseStamina(settings.S.walkStamPerSec * Time.deltaTime);

            //Request a jump if the player presses the button.
            //This helps make jumping more consistent if conditions are false on intermittent frames.
            if (Input.GetButtonDown("Jump"))
                vals.jumpPressed = Time.time;
        }
        
        /// <summary> Perform all the movement functions of the player, including applying forces such as friction and input relative to the players rotation. </summary>
        private void UpdMove()
        {
            //--------------------------MOVEMENT PHYSICS--------------------------//

            //-----PHASE ONE: GET AND ADJUST INPUT-----//

            //Factor any modifiers like sneaking or slow effects into the max speed;
            float alteredMaxSpeed = settings.M.maxSpeed;
            float alteredAcceleration = settings.M.acceleration;

            if (vals.dashing)
            {
                float value = settings.M.dashSpeedMultiplierCurve.Evaluate(Time.time - vals.startedDashing);
                alteredMaxSpeed *= value;
                alteredAcceleration *= value;
            }

            if (!Grounded)
                alteredAcceleration *= settings.M.airControlFactor;

            if (vals.onSlippery)
                vals.desiredDirection *= 0.1f;

            //Calculate the ideal velocity from the input and the acceleration settings.
            Vector3 newVelocity;
            newVelocity = ParentRefs.RB.velocity + (vals.desiredDirection * alteredAcceleration * Time.deltaTime);

            //-----PHASE TWO: SEPERATE VELOCITY TO VERTICAL AND LATERAL-----//

            //Transform current and ideal velocity to local space so non-vertical (lateral) speed can be calculated and limited.
            Vector3 TransformedOldVelocity = transform.InverseTransformVector(ParentRefs.RB.velocity);
            Vector3 TransformedNewVelocity = transform.InverseTransformVector(newVelocity);

            //Get lateral speed by cutting out local-z component (Must be z for LookRotation function to work. Would otherwise be y).
            Vector3 LateralVelocityOld = new Vector3(TransformedOldVelocity.x, TransformedOldVelocity.y, 0);
            Vector3 LateralVelocityNew = new Vector3(TransformedNewVelocity.x, TransformedNewVelocity.y, 0);

            //-----PHASE THREE: AVOID EDGES AND SLOW TO MAX SPEED-----//

            if (Grounded)
                LateralVelocityNew = AvoidEdgesLinear(LateralVelocityNew);

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

            //-----PHASE FOUR: STOPPING AND FRICTION-----//

            //If the player is not trying to move and not jumping, apply stopping force.
            if (!vals.jumping && !vals.onSlippery && vals.desiredDirection.magnitude < 0.01f)
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
            }
            
            //-----PHASE FIVE: CHECK OR EDIT RELATIVE AND LATERAL VELOCITY-----//

            //Delete the 'upwards' force (relative to player rotation), if requested by the climbing system.
            if (vals.eliminateUpForce)
            {
                vals.eliminateUpForce = false;
                TransformedNewVelocity.z = 0;
            }

            //Rotate the character model if the lateral velocity is above a threshold, and trigger movement animations.
            if (LateralVelocityNew.magnitude > settings.M.maxSpeed * settings.M.turningThreshold)
            {
                vals.moving = true;
                ParentRefs.body.rotation = Quaternion.LookRotation(transform.TransformVector(LateralVelocityNew), -transform.forward);
            }
            else if (vals.onSlippery && vals.desiredDirection != Vector3.zero)
            {
                vals.moving = true;
                //ParentRefs.body.rotation = Quaternion.LookRotation(transform.TransformVector(vals.desiredDirection), -transform.forward);
            }
            else
                vals.moving = false;

            //-----PHASE SIX: CONVERT TO WORLD AND APPLY-----//

            //Add the vertical component back, convert the new value back to world-space, and set the rigid bodys velocity to it.
            LateralVelocityNew += new Vector3(0, 0, TransformedNewVelocity.z);

            ParentRefs.RB.velocity = transform.TransformVector(LateralVelocityNew);
        }

        /// <summary> Check for jump input, and do the appropriate jump for the situation (needs work). </summary>
        private void Jump()
        {
            //If the player wants to and is able to jump, apply a force and set the last jump time.
            bool tryingToJump = Time.time < vals.jumpPressed + settings.J.checkJumpTime;
            bool offCooldown = Time.time > vals.lastJump + settings.J.jumpCooldown;
            bool groundedOrCoyotee = Grounded || Time.time < vals.lastOnSurface + settings.J.coyoteeTime;
            if (tryingToJump && groundedOrCoyotee && offCooldown)
            {
                vals.jumping = true;
                vals.lastJump = Time.time;
                vals.jumpPressed = -5;

                PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.jump);

                bool forwardJump = vals.moving && settings.J.allowForwardJumps;
                if (forwardJump)
                {
                    //Do a 'forward' jump relative to the character.
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce * settings.J.forwardJumpVerticalFraction;
                    ParentRefs.RB.velocity += ParentRefs.body.forward * settings.J.forwardJumpForce;
                }
                else if (Vector3.Angle(transform.forward, Vector3.down) > settings.J.onWallAngle)
                { //If player is rotated to face the ground.
                  //Do a wall jump (biased towards up instead of out).
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce * (1 - settings.J.standingWallJumpVerticalRatio);
                    ParentRefs.RB.velocity += Vector3.up * settings.J.jumpForce * settings.J.standingWallJumpVerticalRatio;
                }
                else
                {
                    //Do a normal jump.
                    ParentRefs.RB.velocity += -transform.forward * settings.J.jumpForce;
                }

            }
        }

        /// <summary> Rotate the player so their feet are aligned with the surface beneath them, based on a downwards raycast. </summary>
        private void FindAndRotateToSurface() // AKA Climb
        {
            if (Time.time < vals.lastJump + settings.J.jumpCooldown)
                return;
            //Raycasts:
            RaycastHit hitSurface;
            bool FoundSurface = Physics.Raycast(refs.climbRotateCheckRay.position, -refs.climbRotateCheckRay.up, out hitSurface, settings.WC.surfaceDetectRange, settings.WC.rotateToLayers);

            Vector3 dir = Vector3.down;

            ParentRefs.RB.useGravity = true;

            if (FoundSurface)
            {
                dir = hitSurface.normal;
                
                //Get the angle of this surface.
                float angle = Vector3.Angle(-dir, Vector3.down);

                //Get the type of this surface.
                SCRunModeSettings.SCStaminaSettings.SurfaceTypes surface = GetSurfaceType(hitSurface);
                
                vals.onSlippery = false;

                //Use stamina and set slippery status based on surface and angle.
                if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Climbable)
                {
                    if (angle > settings.S.climbMaxAngle)
                        vals.onSlippery = true;
                    else if (angle > settings.S.climbMinAngle)
                    {
                        if (!ParentRefs.stamina.UseStamina(settings.S.climbStamPerSec * Time.deltaTime))
                            vals.onSlippery = true;
                    }
                }
                else if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.NonClimbable)
                {
                    if (angle > settings.S.climbMinAngle)
                            vals.onSlippery = true;
                }
                else if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Slippery)
                {
                    vals.onSlippery = true;
                }

                //Do animations and behaviour based on if surface is slippery.
                if (vals.onSlippery)
                {
                    PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.slipping);

                    //Rotate to the surface but DON'T teleport to it.
                    //CustomIntuitiveSnapRotation(-hitSurface.normal);
                }
                else
                {
                    ParentRefs.RB.useGravity = false;
                    if (vals.falling)
                        PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.landJump);

                    //Teleport to the surface, and if its angle is too different eliminate the 'up force' to stop player flying off.
                    TeleportToSurface(hitSurface);
                    if (Vector3.Angle(vals.lastRotationDir, dir) > settings.WC.wallStickDangerAngle)
                        vals.eliminateUpForce = true;
                }

                //Reset falling, jumping and OnSurface values.
                vals.falling = false;
                vals.jumping = false;
                vals.lastOnSurface = Time.time;

                //Save the current normal so the difference can be checked next frame.
                vals.lastRotationDir = dir;
            }
            else if (Time.time > vals.lastOnSurface + settings.WC.noSurfResetTime)
            {
                //Point feet down and start falling if not on a surface for long enough.
                CustomIntuitiveSnapRotation(Vector3.down);
                PARENT.CallAnimationEvents(SquirrelController.AnimationTrigger.falling);
                vals.falling = true;
            }
        }

        //~~~~~~~~~~ HELPER/SUB-FUNCTIONS ~~~~~~~~~~

        /// <summary> Rotate the players model each frame so it aligns with the body (used to smooth jumping animations). </summary>
        private void RotateModel()
        {
            ParentRefs.model.rotation = Quaternion.RotateTowards(ParentRefs.model.rotation, ParentRefs.body.rotation, settings.WC.rotateDegreesPerSecond * Time.deltaTime);
            ParentRefs.model.localPosition = Vector3.MoveTowards(ParentRefs.model.localPosition, Vector3.zero, settings.WC.moveUnitsPerSecond * Time.deltaTime);
        }

        /// <summary> Rotate the player so its feet are pointed in the given direction, while maintaining facing as well as possible.
        /// Returns the model to the original rotation so it can be rotated smoothly. </summary>
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

        /// <summary> Call JumpToClimbWall when jump is pressed, or settings are correct for dash climbing.
        /// Also place the climb-point debug object. </summary>
        private void DoJumpChecks()
        {
            if (vals.dashing)
            {   //DASH MODE

                if (Input.GetButtonDown("Jump"))
                {
                    //Try to jump onto a wall, otherwise do a normal jump.
                    if (JumpToClimbWall(1f))
                        return;
                    
                    //Do normal Jump HERE
                    Jump();
                    return;
                }

                if (settings.M.dashForAutoclimb)
                {
                    if (Time.time > vals.lastJumpToWall + settings.WC.autoclimbCooldown)
                    {
                        if (JumpToClimbWall(0.5f, settings.WC.angleToFailAutoclimb))
                            return;
                    }

                    if (vals.stoppedAtEdge && JumpAroundCorners())
                    {
                        //Temporarily disable automatic corner jump HERE
                        return;
                    }
                }
            }
            else
            {   //CAREFUL MODE

                if (Input.GetButtonDown("Jump"))
                {
                    if (vals.stoppedAtEdge)
                    {
                        if (JumpAroundCorners())
                        {
                            //Temporarily disable automatic corner jump HERE
                            return;
                        }
                    }
                    
                    //Try to jump onto a wall, otherwise do a normal jump.
                    if (JumpToClimbWall(1f))
                        return;
                    
                    //Do normal Jump HERE
                    Jump();
                    return;
                }
            }
        }

        private bool JumpToClimbWall(float distMultiplier)
        {
            return JumpToClimbWall(distMultiplier, 0);
        }

        /// <summary> Use 'FindClimbableWall' to get a surface, and then jump to it while translating the squirrels model smoothly.
        /// Raycasting to check if the point is in LOS can be enabled here. </summary>
        private bool JumpToClimbWall(float distMultiplier, float relativeAngleToCancel)
        {
            RaycastHit mainHit;

            if (FindClimbableWall(out mainHit, distMultiplier))
            {
                if (!ValidClimb(mainHit))
                    return false;
                //Fail the teleport if there is not line-of-sight between the player and the new point.
                RaycastHit validityCheck;
                Vector3 checkDir = mainHit.point - refs.climbRotateCheckRay.position;
                float checkDist = Vector3.Distance(mainHit.point, refs.climbRotateCheckRay.position) - refs.mainCollider.radius * 0.5f;
                if (Physics.SphereCast(refs.climbRotateCheckRay.position, refs.mainCollider.radius * 0.1f, checkDir, out validityCheck, checkDist, settings.WC.rotateToLayers))
                {
                    return false;
                }

                if (relativeAngleToCancel > 0)
                {
                    if (Vector3.Angle(mainHit.normal, -transform.forward) < relativeAngleToCancel)
                    {
                        return false;
                    }
                }

                //Teleport to the point, while maintaining the models position so it moves smoothly.
                Quaternion oldRot = ParentRefs.model.rotation;
                TeleportToSurface(mainHit);
                ParentRefs.model.localRotation = oldRot;
                vals.lastJumpToWall = Time.time;
                vals.eliminateUpForce = true;

                //Set velocity to zero to mitigate weird physics.
                ParentRefs.RB.velocity = Vector3.zero;
                return true;
            }
            return false;
        }

        /// <summary> Sphere-cast in the input direction to find any nearby surfaces that the squirrel can climb on.
        /// Does two passes to make it more likely a wall will be found, while maintaining tight control. </summary>
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
            Debug.DrawLine(sphereStart, sphereStart + sphereDir * (modifiedDist + settings.WC.sphereDetectRadius));
            Debug.DrawLine((sphereStart - transform.forward * settings.WC.sphereDetectRadius), (sphereStart - transform.forward * settings.WC.sphereDetectRadius) + sphereDir * (modifiedDist + settings.WC.sphereDetectRadius));

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
        
        /// <summary> Check if there is a sharp edge in front of the player, and return a new movement vector to either stop, slide along an angled edge or move forward normally.
        /// (Uses 5 raycasts in a line in front of the direction of movement to check for surfaces). </summary>
        private Vector3 AvoidEdgesLinear(Vector3 lateralVelocity)
        {
            Vector3[] points = new Vector3[5];
            points[0] = lateralVelocity.normalized;
            vals.edgeRayStart = points[0];

            points[1] = points[0] + Vector3.Cross(points[0], Vector3.forward) * (settings.WC.edgeSlideCheckDist / 2);
            points[2] = points[0] - Vector3.Cross(points[0], Vector3.forward) * (settings.WC.edgeSlideCheckDist / 2);
            points[3] = points[0] + Vector3.Cross(points[0], Vector3.forward) * settings.WC.edgeSlideCheckDist;
            points[4] = points[0] - Vector3.Cross(points[0], Vector3.forward) * settings.WC.edgeSlideCheckDist;

            float[] multiplier = new float[5];
            multiplier[0] = 1;
            multiplier[1] = 1 / points[1].magnitude;
            multiplier[2] = multiplier[1];
            multiplier[3] = 1 / points[3].magnitude;
            multiplier[4] = multiplier[3];

            Vector3 pos;
            RaycastHit hit;
            for (int i = 0; i < points.Length; ++i)
            {
                float mag = settings.WC.edgeStopLatRadius;
                pos = refs.climbRotateCheckRay.position + transform.TransformVector(points[i]) * mag;
                if (Physics.Raycast(pos, transform.forward, out hit, settings.WC.edgeStopDownDist, settings.WC.rotateToLayers))
                {
                    if (Vector3.Angle(hit.normal, -transform.forward) < settings.J.EdgeDetectAngle)
                    {
                        vals.stoppedAtEdge = false;
                        return points[i].normalized * lateralVelocity.magnitude * multiplier[i];
                    }
                }
            }
            vals.stoppedAtEdge = true;
            return Vector3.zero;
        }

        /// <summary> Check for scenarios where there is a gap in front of the player, and  a surface at an angle below the player. </summary>
        private bool JumpAroundCorners()
        {
            //Cancel if on cooldown.
            if (Time.time < vals.lastCornerVault + settings.WC.cornerVaultCooldown)
                return false;

            //If the player is not trying to move, make the checks relative to the model rotation, otherwise make them relative to the desired direction.
            Vector3 moveDirection = ParentRefs.model.transform.forward;
            if (vals.desiredDirection != Vector3.zero)
                moveDirection = vals.desiredDirection;

            //Calculate the starting point for the checks, based on the moveDirection, the players orientation, and the settings.
            Vector3 firstCheckPoint = moveDirection.normalized;
            float size = settings.squirrelCenterToNoseDist / 2f;
            Vector3 firstCast = transform.position + (ParentRefs.model.transform.up * size * settings.J.SJCheckHeight) + moveDirection * size * 2f;
            RaycastHit hit;

            //Check if there is a normal surface for the player to climb to, in which case the corner-jump should not trigger.
            if (Physics.Raycast(transform.position, firstCast - transform.position, out hit, Vector3.Distance(firstCast, transform.position), settings.WC.rotateToLayers))
            {
                return false;
            }

            //Check if there is a normal surface for the player to climb to, in which case the corner-jump should not trigger.
            //If the surface found has a steep angle compared to the player (> EdgeDetectAngle) jump to it anyway. (For just over 90deg angles)
            if (Physics.Raycast(firstCast, -ParentRefs.model.transform.up, out hit, (size * settings.J.CornerJumpDepth) + (size * settings.J.SJCheckHeight), settings.WC.rotateToLayers))
            {
                refs.climbPointDisplay.position = hit.point;
                float angle = Vector3.Angle(hit.point, -transform.forward);
                if (angle > settings.J.EdgeDetectAngle)
                {
                    if (ValidClimb(hit))
                    {
                        Quaternion oldRot = ParentRefs.model.rotation;
                        TeleportToSurface(hit);
                        ParentRefs.model.localRotation = oldRot;
                        vals.lastCornerVault = Time.time;
                        vals.eliminateUpForce = true;
                        return true;
                    }
                }
                return false;
            }

            //Raycast back towards the player from the end point of the first cast down.
            Vector3 cornerCheckOrigin = firstCast - ParentRefs.model.transform.up * ((size * settings.J.CornerJumpDepth) + (size * settings.J.SJCheckHeight));
            if (Physics.Raycast(cornerCheckOrigin, -moveDirection, out hit, size * 2f, settings.WC.rotateToLayers))
            {
                if (ValidClimb(hit))
                {
                    Quaternion oldRot = ParentRefs.model.rotation;
                    TeleportToSurface(hit);
                    ParentRefs.model.localRotation = oldRot;
                    vals.lastCornerVault = Time.time;
                    vals.eliminateUpForce = true;
                    return true;
                }
            }

            return false;
        }

        private void TeleportToSurface(RaycastHit hit)
        {
            if (Time.time < vals.lastTeleported + settings.WC.teleportCooldown)
                return;

            vals.lastTeleported = Time.time;
            vals.lastTeleportDistance = hit.distance; //This is approximately right and a lot cheaper.

            Vector3 oldPos = ParentRefs.model.position;

            transform.position = hit.point;
            CustomIntuitiveSnapRotation(-hit.normal);

            ParentRefs.model.position = oldPos;
        }

        private bool ValidClimb(RaycastHit hit)
        {
            return ValidClimb(Vector3.Angle(hit.normal, Vector3.up), hit);
        }

        private bool ValidClimb(float angle, RaycastHit hit)
        {
            SCRunModeSettings.SCStaminaSettings.SurfaceTypes surface = GetSurfaceType(hit);
            if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Slippery)
                return false;
            if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.NonClimbable && angle > settings.S.climbMinAngle)
                return false;
            if (surface == SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Climbable && !ParentRefs.stamina.StaminaAvailable())
                return false;

            return true;
        }

        private SCRunModeSettings.SCStaminaSettings.SurfaceTypes GetSurfaceType(RaycastHit hitSurface)
        {
            SCRunModeSettings.SCStaminaSettings.SurfaceTypes surface = settings.S.defaultSurface;
            if (hitSurface.transform.tag == settings.S.EZClimbTag)
                surface = SCRunModeSettings.SCStaminaSettings.SurfaceTypes.EZClimb;
            else if (hitSurface.transform.tag == settings.S.ClimbableTag)
                surface = SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Climbable;
            else if (hitSurface.transform.tag == settings.S.nonClimbableTag)
                surface = SCRunModeSettings.SCStaminaSettings.SurfaceTypes.NonClimbable;
            else if (hitSurface.transform.tag == settings.S.slipperyTag)
                surface = SCRunModeSettings.SCStaminaSettings.SurfaceTypes.Slippery;
            return surface;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(refs.climbRotateCheckRay.position, settings.WC.sphereDetectRadius);
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
            //INPUT RELATED:
            /// <summary> A player-relative vector representing the movement inputs.
            /// Used to transfer the input to the movement update code so input can remain in a single function. </summary>
            public Vector3 desiredDirection;
            /// <summary> The time.time value of the last time the player pressed the jump button.
            /// Used to check for valid jump conditions over multiple frames to help prevent failures. </summary>
            public float jumpPressed;
            /// <summary> True when the player is holding the dash key.
            /// Used to check if a speed modifier should be evaluated from the dash animation curve. </summary>
            public bool dashing;
            /// <summary> The time.time value of the last time the player pressed the dash key down. 
            /// Used to evaluate a speed multiplier from the 'dashSpeedMultiplierCurve'. </summary>
            public float startedDashing;

            //ANIMATION RELATED:
            /// <summary> Value is true when the character is locked in a jumping state at the start of the jump.
            /// Used to play jump animations, and as a secondary check</summary>
            public bool jumping;
            /// <summary> True when the characters lateral velocity (i.e not up/down relative to rotation) is greater than 'M.turningThreshold'.
            /// Used to play running animations, and to change between jump modes. </summary>
            public bool moving;
            /// <summary> True when the characters lateral velocity (i.e not up/down relative to rotation) is greater than 'M.turningThreshold'.
            /// Used to play running animations, and to change between jump modes. </summary>
            public bool falling;

            //MESSAGES AND MULTIPLE-UPDATE VALUES:
            /// <summary> The time.time value of the last time the player started a jump.
            /// Used for jump cooldown and to prevent movement forces cancelling the jump.  </summary>
            public float lastJump;
            /// <summary> The time.time value of the last time the player was close enough to a surface to climb on it.
            /// Used to check if jumps are allowed, and if 'air-control' modifiers should be used. </summary>
            public float lastOnSurface;
            /// <summary> The time.time value of the last time the player teleported BECAUSE OF JumpToClimbWall.
            /// Used to prevent too rapid teleports specifically when autoclimbing. </summary>
            public float lastJumpToWall;
            /// <summary> The time.time value of the last time the player teleported (for any reason including rotate-to-surface).
            /// Used to prevent too rapid teleports. </summary>
            public float lastTeleported;
            /// <summary> The distance of the last teleport.
            /// Used to stun/slow the player relative to the distance moved, and adjust the cooldown time. </summary>
            public float lastTeleportDistance;
            /// <summary> The time.time value of the last time the player teleported around a (roughly) 90deg angle.
            /// Used to prevent too rapid vaulting. </summary>
            public float lastCornerVault;
            /// <summary> True if the player detected an edge (no surface in front of the player with a relative angle less than 90deg).
            /// Used to allow a corner jump if an appropriate button (dash or jump) is also pressed </summary>
            public bool stoppedAtEdge;
            /// <summary> True if the player is on a surface they are not allowed to climb.
            /// Disables control of movement but allows jumping still. </summary>
            public bool onSlippery;
            /// <summary> The start position of the main edge detect ray.
            /// Used as the starting point to detect corners for more efficiency and flexibility. </summary>
            public Vector3 edgeRayStart;
            /// <summary> Message bool telling the UpdMovement function to remove up/down (z axis) forces on that frame.
            /// Used because this action needs to be done at a different point to when the action is requested (FindAndRotateToSurface). </summary>
            public bool eliminateUpForce;
            /// <summary> The players down direction in the last frame.
            /// Used to check if the angle change is too great, and the up-force needs to be eliminated. </summary>
            public Vector3 lastRotationDir;
            /// <summary> [NOT IN USE] The desired rotation of the characters body or model.
            /// Previously used to create more smooth animation when quickly changing direction (may use again in future). </summary>
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
            public SphereCollider mainCollider;
            public UnityEngine.UI.Image staminaBar;
        }

        [System.Serializable]
        public class SCRunModeSettings
        {
            [Header("Generic Settings")]
            /// <summary> Used to calculate distances relative to the squirrels size. </summary>
            public float squirrelCenterToNoseDist = 0.16f;

            [Header("Settings Categories")]
            /// <summary> Contains variables which are exposed in the inspector, and are used as values for Stamina behaviours. </summary>
            public SCStaminaSettings stamina = new SCStaminaSettings();
            /// <summary> Contains variables which are exposed in the inspector, and are used as values for normal Movement behaviours. </summary>
            public SCMoveSettings movement = new SCMoveSettings();
            /// <summary> Contains variables which are exposed in the inspector, and are used as values for Jumping behaviours. </summary>
            public SCJumpSettings jump = new SCJumpSettings();
            /// <summary> Contains variables which are exposed in the inspector, and are used as values for Wall Climbing behaviours. </summary>
            public SCWallClimbSettings wallClimbing = new SCWallClimbSettings();

            /// <summary> Shorthand for Stamina settings class. </summary>
            public SCStaminaSettings S { get { return stamina; } }
            /// <summary> Shorthand for Movement settings class. </summary>
            public SCMoveSettings M { get { return movement; } }
            /// <summary> Shorthand for Jump settings class. </summary>
            public SCJumpSettings J { get { return jump; } }
            /// <summary> Shorthand for Wall Climbing settings class. </summary>
            public SCWallClimbSettings WC { get { return wallClimbing; } }

            [System.Serializable]
            public class SCStaminaSettings
            {
                [Tooltip("The maximum stamina value. Fairly arbitrary as charge rate etc can all be changed.")]
                public float maxStamina = 10f;
                [Tooltip("The amount of stamina needed before stamina-using actions can be performed after reaching zero. Necessary to prevent stuttering.")]
                public float minStaminaToStartUse = 10f;
                [Tooltip("The amount of stamina that regenerates each second.")]
                public float staminaRegenPerSecond = 1f;
                [Tooltip("The delay between stopping stamina-using actions (or running out) and the stamina recharging.")]
                public float staminaRegenDelay = 1f;
                [Tooltip("Control if a stamina check will fail when the value WOULD go below 0, or AFTER it does go below 0 (relevant for large consumptions like jumps).")]
                public bool allowNegativeStamina = true;
                [Tooltip("Control if actions which use zero stamina prevent it from regenerating.")]
                public bool zeroStopsRegen = false;
                [Tooltip("Control if trying to use stamina (e.g. holding dash button) prevents it regenerating even if the action fails.")]
                public bool failStopsRegen = false;

                [Space]
                [Tooltip("Amount of stamina used per second when dashing.")]
                public float dashStamPerSec = 1f;
                [Tooltip("Amount of stamina used per second when moving but not dashing.")]
                public float walkStamPerSec = 0f;
                [Tooltip("Amount of stamina used per jump.")]
                public float jumpStamPerUse = 1f;
                [Tooltip("Amount of stamina used per second when on a sufficiently steep surface.")]
                public float climbStamPerSec = 1f;
                [Tooltip("The angle of a surface for moving on it to be defined as climbing.")]
                public float climbMinAngle = 30f;
                [Tooltip("The angle of a surface where the player will immediately fall off (except EZ Climb).")]
                public float climbMaxAngle = 175f;

                [Space]
                public SurfaceTypes defaultSurface = SurfaceTypes.NonClimbable;
                public enum SurfaceTypes { EZClimb, Climbable, NonClimbable, Slippery }
                public string EZClimbTag = "EZClimb";
                public string ClimbableTag = "Climbable";
                public string nonClimbableTag = "HardClimb";
                public string slipperyTag = "NoClimb";
            }

            [System.Serializable]
            public class SCMoveSettings
            {
                [Header("Movement Settings")]
                [Tooltip("Force applied when player holds movement input. Controls how quickly max speed is reached and how much forces can be countered.")]
                public float acceleration = 20f;
                [Tooltip("The horizontal speed at which no new acceleration is allowed by the player.")]
                public float maxSpeed = 3f;
                //[Tooltip("Multiplier for the max speed when starting a dash.")]
                //public float dashSpeedMaxMult = 3f;
                //[Tooltip("Multiplier for the max speed after dashing for a long time.")]
                //public float dashSpeedMinMult = 3f;
                [Tooltip("Speed of the dash over time.")]
                public AnimationCurve dashSpeedMultiplierCurve = new AnimationCurve();
                [Tooltip("True allows climbing checks every frame while dashing.")]
                public bool dashForAutoclimb = false;
                [Tooltip("Multiplier for the amount of acceleration applied while in the air.")]
                public float airControlFactor = 0.5f;
                [Tooltip("Rate at which speed naturally decays back to max speed (used in case of external forces).")]
                public float frictionForce = 50f;
                [Tooltip("Rate at which speed falls to zero when not moving.")]
                public float stoppingForce = 50f;
                [Tooltip("Rate at which speed falls to zero when not moving and in the air.")]
                public float airStoppingForce = 2;
                [Tooltip("Fraction of the max speed at which a grounded player will fully stop.")]
                [Range(0, 1)]
                public float haltAtFractionOfMaxSpeed = 0.9f;
                [Tooltip("Fraction of max speed needed for the character model to rotate.")]
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
                [Tooltip("Time after a jump before the player can jump again. Stops superjumps from pressing twice while trigger is still activated. ALSO USED to stop player from teleporting to the ground.")]
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

                [Header("Special Jump Settings")]
                [Tooltip("Starting Height for edge-detection, corner detection and teleport-jumps, as a MULTIPLIER OF THE PLAYERS SIZE.")]
                public float SJCheckHeight = 2f;
                [Tooltip("Distance the edge detect will check below the player (in ADDITION to the starting height).")]
                public float EdgeDetectDepth = 2f;
                [Tooltip("The difference in angle between the current surface and the new surface which will still count as an edge.")]
                public float EdgeDetectAngle = 70f;
                [Tooltip("Distance the below the player that corner jump checks will occur (in ADDITION to the starting height)." +
                    "Effectively controls the minimum width of corners that can be jumped onto (e.g. top of fence).")]
                public float CornerJumpDepth = 2f;

                [HideInInspector]
                [Tooltip("Distance between teleport-jump checks.")]
                public float SJCheckInterval = 1f;
                [HideInInspector]
                [Tooltip("Number of teleport-jump checks.")]
                public int SJCheckCount = 4;

            }

            [System.Serializable]
            public class SCWallClimbSettings
            {
                [Header("Wallclimb Settings")]
                [Tooltip("The layers of objects the player is allowed to climb on.")]
                public LayerMask climableLayers = new LayerMask();
                [Tooltip("The layers of objects the player is allowed to climb on.")]
                public LayerMask rotateToLayers = new LayerMask();
                public float angleToFailAutoclimb = 30;
                [Tooltip("Size of the sphere-cast that will detect surfaces to climb on. Larger means more forgiving controls, but also more likely to get objects behind the player.")]
                public float sphereDetectRadius = 0.3f;
                [Tooltip("Multiplier to the sphere-cast radius for the second climable check pass.")]
                public float secondPassMultiplier = 5f;
                [Tooltip("Length of the sphere-cast that detects surfaces. Larger means the check will find objects further from the player.")]
                public float sphereDetectDistance = 0.5f;
                [Tooltip("Distance from the center of the character from which surfaces below will be detected.")]
                public float surfaceDetectRange = 0.15f;
                [Tooltip("Distance from the center of the character in the direction of movement from which edges will be detected.")]
                public float edgeStopLatRadius = 0.15f;
                [Tooltip("Maximum distance from the check point to a surface to count as not an edge.")]
                public float edgeStopDownDist = 0.15f;
                [Tooltip("Angle from the movement direction from which secondary edge checks are done (also checks half way between and on both sides).")]
                [Range(0.01f, 1f)]
                public float edgeSlideCheckDist = 0.1f;
                [Tooltip("If the angle between the current rotation and the new rotation when climbing is above this value, remove the vertical velocity to help the player stich to the wall.")]
                public float wallStickDangerAngle = 10f;
                [Tooltip("Force applied when the character is near a wall to ensure they stick to it.")]
                public float wallStickForce = 0.2f;
                [Tooltip("Range of velocity (at normal to wall) within which sticking force is applied.")]
                public float teleportDist =  0.1f;
                [Tooltip("Time away from a surface before the character rotates to face the ground.")]
                public float noSurfResetTime = 0.2f;
                [Tooltip("How quickly the squirrel model rotates to face the correct direction.")]
                public float rotateDegreesPerSecond = 360;
                [Tooltip("How quickly the squirrel model moves back to alignment when the physics object is teleported.")]
                public float moveUnitsPerSecond = 5f;

                [Tooltip("Cooldown autoclimb teleport. Can help reduce buginess and improve performance.")]
                public float autoclimbCooldown = 0.33f;
                [Tooltip("Cooldown corner vaulting. Should be roughly human reaction-time so players can control which side of branche/fence etc they want.")]
                public float cornerVaultCooldown = 0.33f;
                [Tooltip("Cooldown for ANY teleport. Should be VERY small (e.g. < 0.05s).")]
                public float teleportCooldown = 0.01f;
            }
        }
    }
}