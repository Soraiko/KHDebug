#define playnAnims
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BrandonPotter.XBox;

namespace KHDebug
{
    public class Camera
    {
        public Model Target;

        public Camera(float aspectRation, Vector3 lookAt)
            : this(aspectRation, MathHelper.PiOver4, lookAt, Vector3.Up, 0.1f, float.MaxValue) { }

        public Camera(float aspectRatio, float fieldOfView, Vector3 lookAt, Vector3 up, float nearPlane, float farPlane)
        {
            this.aspectRatio = aspectRatio;
            this.fieldOfView = fieldOfView;
            this.lookAt = lookAt;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
			this.ReCreateProjectionMatrix();
		}
		
        private void ReCreateViewMatrix()
        {
            position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(yaw, pitch, 0));
            position += lookAt;
			
			viewMatrix = Matrix.CreateLookAt(position, lookAt, Vector3.Up);

			viewMatrixDirty = false;
        }

		public int RollStep = 1;

		private void ReCreateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(this.fieldOfView, AspectRatio, nearPlane, farPlane);

            if (this.RollStep<0)
            projectionMatrix *= Matrix.CreateRotationZ(MainGame.PI);
            
			projectionMatrixDirty = false;
        }

        #region HelperMethods

        /// <summary>
        /// Moves the camera and lookAt at to the right,
        /// as seen from the camera, while keeping the same height
        /// </summary>        
        public void MoveCameraRight(float amount)
        {
            Vector3 right = Vector3.Normalize(LookAt - Position); //calculate forward
            right = Vector3.Cross(right, Vector3.Up); //calculate the real right

            right.Normalize();
            LookAt += right * amount;
        }

        public void MoveCameraUp(float amount)
        {
            Vector3 up = Vector3.Normalize(LookAt - Position);

            Vector3 output = up * amount;
            output = Vector3.Transform(output, Matrix.CreateRotationY(-yaw));
            output = Vector3.Transform(output, Matrix.CreateRotationX((float)((Math.PI / 2))));
            output = Vector3.Transform(output, Matrix.CreateRotationY(yaw));

            LookAt += output;
        }


        public void MoveCameraForward(float amount)
        {
            Vector3 forward = Vector3.Normalize(LookAt - Position);

            forward.Normalize();
            LookAt += forward * amount;
        }
        
		#endregion

		#region FieldsAndProperties
		//We don't need an update method because the camera only needs updating
		//when we change one of it's parameters.
		//We keep track if one of our matrices is dirty
		//and reacalculate that matrix when it is accesed.
		private bool viewMatrixDirty = true;
        private bool projectionMatrixDirty = true;
        
        public float MinPitch = -MathHelper.PiOver2 + 0.3f;
        public float MaxPitch = MathHelper.PiOver2 - 0.3f;
        private float pitch;
        public float Pitch
        {
            get { return pitch; }
            set
            {
                if (value < -MathHelper.PiOver2 + 0.3f || value > MathHelper.PiOver2 - 0.3f)
                {
                    return;
                }
                float newVal = value;

                
                SrkBinary.MakePrincipal(ref newVal);
                pitch = newVal;

                ReCreateProjectionMatrix();
			}
        }

        public Matrix GetMatrix()
        {
            return Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0);
        }

        private float destYaw;
        public float DestYaw
        {
            get { return destYaw; }
            set
            {
                destYaw = value;
            }
        }
        

        public float PrincipalYaw
        {
            get
            {
                float principal = destYaw;

                SrkBinary.MakePrincipal(ref principal);
                return principal;
            }
        }

        const float ov2m3 = -MathHelper.PiOver2 + 0.3f;
        const float ov2m3P = MathHelper.PiOver2 - 0.3f;
        
        private float destPitch;
        public float DestPitch
        {
            get { return destPitch; }
            set
            {
                if (value < ov2m3 || value > ov2m3P)
                {
                    return;
                }
                destPitch = value;
            }
        }

        private float yaw;
		public float Yaw
        {
            get { return yaw; }
            set
            {
                yaw = value;
            }
        }

        private float fieldOfView;
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                if (value > 0 && value < Math.PI)
                {
                    projectionMatrixDirty = true;
                    fieldOfView = value;
                }
            }
        }

        private float aspectRatio;
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                projectionMatrixDirty = true;
                aspectRatio = value;
            }
        }

        private float nearPlane;
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                projectionMatrixDirty = true;
                nearPlane = value;
            }
        }

        private float farPlane;
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                projectionMatrixDirty = true;
                farPlane = value;
            }
        }

        public float MinZoom = 1;
        public float MaxZoom = 500;
        private float zoom = 1;


        public float Zoom
        {
            get { return zoom; }
            set
            {
                if (Math.Abs(zoom-value)>0)
                    viewMatrixDirty = true;
                zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
            }
        }
        
        //

        public Microsoft.Xna.Framework.Input.GamePadState gamepadState;
        public Microsoft.Xna.Framework.Input.MouseState mouseState;
        public Microsoft.Xna.Framework.Input.MouseState oldMouseState;

        public KeyboardState keyboardState;
        public KeyboardState oldKeyboardState;



        public void Update(GameWindow win)
        {
            if (this.Target == null)
                return;
            float diff;

            /* Rotation horizontale douce de la camera */
            diff = this.DestYaw - this.Yaw;
            if (Math.Abs(diff) > 0.01f)
            {
                this.Yaw += diff / 10f;
                viewMatrixDirty = true;
            }
            
            /* Rotation verticale douce de la camera */
            diff = this.DestPitch - this.Pitch;
            if (Math.Abs(diff) > 0.01f)
            {
                this.Pitch += diff / 10f;
                viewMatrixDirty = true;
            }


            /* Ajustement vertical doux de la camera */
            diff = this.DestPitch + 0.21f;
            if (this.Target.Speed > 0f && Math.Abs(diff) > 0.001)
            {
                float vitesse = this.Target.Speed / (3200f);
                this.DestPitch -= diff * vitesse;
                viewMatrixDirty = true;
            }

            /* Déplacements de la camera */
            diff = (this.Target.PrincipalDestRotate - this.PrincipalYaw);

            SrkBinary.MakePrincipal(ref diff);

            /* Ajustement horizontal doux de la camera */
            if (this.Target.Speed > 0f && Math.Abs(diff) > 0.001 && Math.Abs(diff) < Math.PI * 0.75f)
            {
                float vitesse = this.Target.Speed / (6400f);
                this.DestYaw += diff * vitesse;
                viewMatrixDirty = true;
            }

            diff = Vector3.Distance(this.DestLookAt, this.LookAt);
            if (diff > 0.01f)
            {
                
                this.LookAt += ((this.DestLookAt - this.LookAt) / (this.Target.Cutscene ? 20f : 10f));
                viewMatrixDirty = true;
            }

            if (!this.Target.Cutscene && Program.game.Map != null && Program.game.Map.Links.Count > 0)
            {
                Vector3  end = this.LookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0)) * this.MaxZoom;
                
                Vector3 col = (Program.game.Map.Links[0] as Collision).GetCameraCollisionIntersect(this.LookAt, end);
                if (!Single.IsNaN(col.X))
                {
                    float newZoom = Vector3.Distance(this.LookAt, col);

                    if (newZoom < 10)
                        newZoom = 10;
                    
                    this.Zoom = newZoom;
                }
                else
                {
                    this.Zoom = this.MaxZoom;
                }

            }


            if (this.zoom < 100)
                this.Target.DestOpacity = 0;
            else
                this.Target.DestOpacity = 1;

        }

        float R2 = 0;
        bool R1 = false;
        float L2 = 0;
        bool L1 = false;

        float joyLX_ = 0;
        float joyLY_ = 0;
        float joyRX_ = 0;
        float joyRY_ = 0;

        float joyLX = 0;
        float joyLY = 0;
        float joyRX = 0;
        float joyRY = 0;
        


        /* Doit influener l'état du joueur MAIS PAS METTRE A JOUR SES PARAMATRES */
        public void GamepadControls(GameWindow win)
        {
            if (this.Target == null)
                return;
            float diff;
            this.gamepadState = GamePad.GetState(PlayerIndex.One);
            this.mouseState = Mouse.GetState(win);
            this.keyboardState = Keyboard.GetState();

            bool A = false;
            bool B = false;
            bool X = false;
            bool Y = false;

            joyLX_ = 0;
            joyLY_ = 0;
            joyRX_ = 0;
            joyRY_ = 0;

            R1 = false;
            L1 = false;
            R2 = 0;
            L2 = 0;

            if (!this.Target.Cutscene)
            {
                if (Program.game.UseXboxController)
                if (gamepadState.IsConnected)
                {
                    joyLX_ = gamepadState.ThumbSticks.Left.X;
                    joyLY_ = gamepadState.ThumbSticks.Left.Y;
                    joyRX_ = gamepadState.ThumbSticks.Right.X;
                    joyRY_ = gamepadState.ThumbSticks.Right.Y;
                    A = gamepadState.Buttons.A == ButtonState.Pressed;
                    B = gamepadState.Buttons.B == ButtonState.Pressed;
                    X = gamepadState.Buttons.X == ButtonState.Pressed;
                    Y = gamepadState.Buttons.Y == ButtonState.Pressed;
                    R1 = gamepadState.Buttons.RightShoulder == ButtonState.Pressed;
                    L1 = gamepadState.Buttons.LeftShoulder == ButtonState.Pressed;
                    R2 = gamepadState.Triggers.Right;
                    L2 = gamepadState.Triggers.Left;
                }
                else if (!Program.game.graphics.PreferMultiSampling)
                {
                    try
                    {
                        var connectedControllers = BrandonPotter.XBox.XBoxController.GetConnectedControllers();
                        if (connectedControllers.Count() > 0)
                        {
                            joyLX_ = (float)((connectedControllers.First().ThumbLeftX - 50d) / 50d);
                            joyLY_ = (float)((connectedControllers.First().ThumbLeftY - 50d) / 50d);
                            joyRX_ = (float)((connectedControllers.First().ThumbRightX - 50d) / 50d);
                            joyRY_ = (float)((connectedControllers.First().ThumbRightY - 50d) / 50d);
                            A = connectedControllers.First().ButtonAPressed;
                            B = connectedControllers.First().ButtonBPressed;
                            X = connectedControllers.First().ButtonXPressed;
                            Y = connectedControllers.First().ButtonYPressed;
                            R1 = connectedControllers.First().ButtonShoulderRightPressed;
                            L1 = connectedControllers.First().ButtonShoulderLeftPressed;
                            R2 = (float)connectedControllers.First().TriggerRightPosition;
                            L2 = (float)connectedControllers.First().TriggerLeftPosition;
                        }
                    }
                    catch
                    {

                    }
                }

                if (keyboardState.IsKeyDown(Keys.M) && oldKeyboardState.IsKeyUp(Keys.M))
                {
                    Program.game.Musique = !Program.game.Musique;
                    if (Program.game.Musique)
                        Audio.SetVolume(@"Content\Effects\Audio\BGM\TT08.wav",0);
                    else
                        Audio.SetVolume(@"Content\Effects\Audio\BGM\TT08.wav", 100);
                }

                if (keyboardState.IsKeyDown(Keys.F))
                {
                    Program.game.Collision = 0;
                    Program.game.Map.Render = true;
                }
                if (keyboardState.IsKeyDown(Keys.G))
                {
                    Program.game.Collision = 1;
                    Program.game.Map.Render = false;
                }
                if (keyboardState.IsKeyDown(Keys.H))
                {
                    Program.game.Collision = 2;
                    Program.game.Map.Render = true;
                }

                if (keyboardState.IsKeyDown(MainGame.france ? Keys.Z : Keys.W))
                {
                    joyLY = 1f;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    joyLY = -1f;
                }
                if (keyboardState.IsKeyDown(MainGame.france ? Keys.Q : Keys.A))
                {
                    joyLX = -1f;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    joyLX = 1f;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    B = true;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
#if !playAnims
                    Y = true;
#endif
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    A = true;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    X = true;
                }

                if (keyboardState.IsKeyDown(Keys.NumPad8))
                {
                    joyRY = 1f;
                }
                if (keyboardState.IsKeyDown(Keys.NumPad2))
                {
                    joyRY = -1f;
                }
                if (keyboardState.IsKeyDown(Keys.NumPad4))
                {
                    joyRX = -1f;
                }
                if (keyboardState.IsKeyDown(Keys.NumPad6))
                {
                    joyRX = 1f;
                }

                if (keyboardState.IsKeyDown(Keys.NumPad3))
                {
                    R2 = 1f;
                }

                if (keyboardState.IsKeyDown(Keys.NumPad1))
                {
                    L2 = 1f;
                }

                if (keyboardState.IsKeyDown(Keys.NumPad7))
                {
                    L1 = true;
                }

                if (keyboardState.IsKeyDown(Keys.NumPad9))
                {
                    R1 = true;
                }
            }


            diff = joyLX_ - joyLX;
            joyLX += diff * (10 / 60f);
            diff = joyLY_ - joyLY;
            joyLY += diff * (10 / 60f);
            diff = joyRX_ - joyRX;
            joyRX += diff * (10 / 60f);
            diff = joyRY_ - joyRY;
            joyRY += diff * (10 / 60f);
            

            this.DestLookAt = this.Target.Location + this.Target.HeadHeight;

            var targetMoveset = (this.Target.Links[0] as BinaryMoveset);
#if (playAnims)

            if (oldKeyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyDown(Keys.Down) && targetMoveset.PlayingIndex > 0)
            {
                targetMoveset.PlayingIndex--;
                Console.WriteLine(targetMoveset.PlayingIndex);

            }
            if (oldKeyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyDown(Keys.Up) && targetMoveset.PlayingIndex < 78)
            {
                targetMoveset.PlayingIndex++;
                Console.WriteLine(targetMoveset.PlayingIndex);
            }
            
            /*if ((keyboardState.IsKeyDown(Keys.NumPad1) || (oldKeyboardState.IsKeyUp(Keys.Left) && keyboardState.IsKeyDown(Keys.Left))))
            {
                targetMoveset.FrameStep = -1;
            }
            if ((joyRX>0 || keyboardState.IsKeyDown(Keys.NumPad3) || (oldKeyboardState.IsKeyUp(Keys.Right) && keyboardState.IsKeyDown(Keys.Right))))
            {
                targetMoveset.FrameStep = 1f;
            }*/
#endif



            float hypo = (float)Math.Sqrt(joyLX* joyLX + joyLY* joyLY);
            float realHypo = hypo*1f;
            this.Target.Joystick = realHypo;

            if (this.Target.cState == Model.ControlState.Fall)
                this.Target.SmoothJoystick += (hypo- this.Target.SmoothJoystick) /20f;
            else if (this.Target.cState != Model.ControlState.Fly || hypo>this.Target.SmoothJoystick)
                this.Target.SmoothJoystick = hypo;
            

            Vector3 requestedLoc = this.Target.Location;

            if (this.Target.cState == Model.ControlState.Cliff)
            {
                if (this.Target.SmoothJoystick < 0.1)
                    this.Target.AllowCliffCancel = true;
                if (this.Target.AllowCliffCancel)
                {
                    double angle_ = Math.Atan2(joyLY / hypo, joyLX / hypo);

                    float joyDir_ = (float)(this.yaw + angle_ + Math.PI / 2f);
                    float curDir_ = this.Target.DestRotate;

                    if (this.Target.CliffCancel || ((hypo > 0.6) && Math.Abs(Math.Cos(joyDir_)) > 0.3 &&
                        Math.Cos(joyDir_) < 0 ^ Math.Cos(curDir_) < 0

                        ))
                    {
                        this.Target.CliffCancel = true;
                        targetMoveset.AtmospherePlayingIndex = targetMoveset.fall_;
                        targetMoveset.PlayingIndex = targetMoveset.fall_;
                        this.Target.cState = Model.ControlState.Fall;
                    }
                }
            }
            else
                this.Target.AllowCliffCancel = false;

            if (hypo > 0.3)
            {
                if (this.Target.cState == Model.ControlState.Fly)
                    hypo = this.Target.SmoothJoystick;

                float step = hypo < 0.75 ? 120 : 480;

                if (this.Target.cState != Model.ControlState.Cliff)
                    if (this.Target.cState != Model.ControlState.Land)
                        if (this.Target.cState != Model.ControlState.Guard)
                        if (this.Target.cState != Model.ControlState.UnGuard)
                if (this.Target.pState != Model.State.BlockAll || this.Target.pState == Model.State.NoIdleWalkRun_ButRotate)
                {
                    double angle = Math.Atan2(joyLY / hypo, joyLX / hypo);
                    if ( this.Target.pState != Model.State.BlockAll)
                    {
                        this.Target.DestRotate = (float)(this.yaw + angle + Math.PI / 2f);
                        if (Math.Abs(angle-(Math.PI/2f)) <0.3f)
                        {
                            targetMoveset.fly_ = targetMoveset.flyForward_;
                        }
                        else
                        if (angle - (Math.PI / 2f)<0)
                        {
                            targetMoveset.fly_ = targetMoveset.flyRight_;
                        }
                        else
                        {
                            targetMoveset.fly_ = targetMoveset.flyLeft_;
                        }
                    }

                    if (this.Target.pState != Model.State.BlockAll || this.Target.pState == Model.State.NoIdleWalkRun_ButRotate)
                    {
                        requestedLoc.X += (float)((1 / 60f) * (step) * Math.Sin((this.yaw + angle + Math.PI / 2f)));
                        requestedLoc.Z += (float)((1 / 60f) * (step) * Math.Cos((this.yaw + angle + Math.PI / 2f)));
                                    
                        if (Program.game.Map != null && Program.game.Map.Links.Count > 0)
                        {
                            (Program.game.Map.Links[0] as Collision).MonitorCollision(this.Target, ref requestedLoc);
                        }
                    }
                }
            }
            if (joyRY<0 || joyRY>0)
                this.DestPitch += (joyRY / 15f);
            if (joyRX < 0 || joyRX > 0)
                this.DestYaw -= (joyRX / 15f);
            if (targetMoveset.fall1_ != targetMoveset.fall2_ && targetMoveset.PlayingIndex == targetMoveset.fall2_)
            {
                if (!Target.justDive)
                {
                    PAXCaster.RunPAX(@"Content\Effects\Visual\DiveWind\DiveWind.dae", null, -1, Vector3.Zero);
                    Audio.Play(@"Content\Effects\Audio\Ambient\Shared\windLoop.wav", true, null, 0);
                    Target.justDive = true;
                }
                for (int i = 0; i < Audio.names.Count; i++)
                {
                    if (Audio.names[i] == @"Content\Effects\Audio\Ambient\Shared\windLoop.wav")
                    {
                        Audio.effectInstances[i].Volume += (0.125f - Audio.effectInstances[i].Volume) / 10f;
                    }
                    if (Audio.names[i] == @"Content\Effects\Audio\Ambient\"+Program.game.Map.Name+".wav")
                    {
                        Audio.effectInstances[i].Volume += (0 - Audio.effectInstances[i].Volume) / 4f;
                    }
                }
                this.Target.Gravity = 20f;
                if (this.FieldOfView > MathHelper.ToRadians(45))
                {
                    if (targetMoveset.PlayingFrame < 70)
                    {
                        this.FieldOfView += (MathHelper.ToRadians(45) - this.FieldOfView) / 40;
                        this.DestPitch += (-1.5f - this.Pitch) / 30f;
                    }
                    else
                    {
                        this.FieldOfView += (MathHelper.ToRadians(45) - this.FieldOfView) / 5;
                        this.DestPitch += (-1.5f - this.Pitch) / 10f;
                    }
                }
            }
            else
            {
                if (Target.justDive)
                {
                    PAXCaster.FinishPAX(@"Content\Effects\Visual\DiveWind\DiveWind.dae");
                    int windCount = 0;
                    int zeroCount = 0;
                    for (int i = 0; i < Audio.names.Count; i++)
                    {
                        if (Audio.names[i] == @"Content\Effects\Audio\Ambient\Shared\windLoop.wav")
                        {
                            Audio.effectInstances[i].Volume += (0 - Audio.effectInstances[i].Volume) / 10f;
                            if (Audio.effectInstances[i].Volume < 0.0001)
                            {
                                zeroCount++;
                            }
                            windCount++;
                        }
                    }
                    if (windCount == zeroCount)
                    Target.justDive = false;
                }
                if (this.FieldOfView < MathHelper.ToRadians(70f))
                {
                    this.Target.Gravity = 16f;

                    this.FieldOfView *= 1.01f;
                }
            }

            if (keyboardState.IsKeyDown(Keys.NumPad7) && oldKeyboardState.IsKeyUp(Keys.NumPad7))
            {
                MainGame.Hour++;
                //Audio.Play((Program.game.Partner1.Links[0] as Moveset).Voices[7], false, Program.game.Partner1, 100);
            }

            if (X)
            {
                if (this.Target.Location.Y < this.Target.LowestFloor + 10 && targetMoveset.roll_ > -1 && realHypo > 0.5f)
                {
                    if (!this.Target.oldRollPress)
                    {
                        this.Target.rollPress = X;
                        if (targetMoveset.PlayingFrame > targetMoveset.MaxTick-5)
                        {
                            this.Target.pState = Model.State.GoToAtMove;
                            targetMoveset.PlayingIndex = targetMoveset.idle_;
                        }
                        targetMoveset.PlayingIndex = targetMoveset.roll_;
                        this.Target.cState = Model.ControlState.Roll;
                    }
                }
                else
                if (A&&this.Target.Location.Y > this.Target.LowestFloor + 800)
                {
                    if (targetMoveset.fall2_ > -1)
                    {
                        this.Target.Fly = false;
                        targetMoveset.fall_ = targetMoveset.fall2_;
                    }
                }
                else if (targetMoveset.fall_ == targetMoveset.fall1_)
                {
                    if (targetMoveset.fly_ > -1&&(this.Target.cState == Model.ControlState.Jump || this.Target.cState == Model.ControlState.Fall))
                    {
                        if (!this.Target.oldFlyPress)
                        {
                            this.Target.Fly = true;
                        }
                    }
                }
                this.Target.oldFlyPress = true;
                this.Target.oldRollPress = true;
            }
            else
            {
                this.Target.Fly = false;
                if (this.Target.cState == Model.ControlState.Jump || this.Target.cState == Model.ControlState.Fall)
                {
                    this.Target.oldFlyPress = false;
                }
                if (this.Target.cState == Model.ControlState.Land || (int)this.Target.cState < 3)
                {
                    this.Target.oldRollPress = false;
                }


                if (this.Target.cState != Model.ControlState.Fall)
                    targetMoveset.fall_ = targetMoveset.fall1_;
            }

            if (!B && (this.Target.cState == Model.ControlState.Cliff || this.Target.Location.Y < this.Target.LowestFloor+20))
            {
                this.Target.oldJumpPress = false;
            }

            if (B)
            {
                if (this.Target.cState == Model.ControlState.Cliff)
                {
                    if (!this.Target.oldJumpPress)
                        this.Target.CliffCancel = true;
                }
                else
                {
                    if (!this.Target.oldJumpPress)
                    {
                        this.Target.JumpCancel = false;
                        this.Target.JumpCollisionCancel = false;
                        this.Target.JumpDelay = this.Target.cState == Model.ControlState.Land ? 6: 0;
                        this.Target.JumpStart = this.Target.LowestFloor;
                    }
                }
                this.Target.oldJumpPress = true;
            }

            /*if (this.Target.cState == Model.ControlState.Land || Math.Abs(requestedLoc.Y - this.Target.LowestFloor) < 10)
            {

                if (!this.Target.oldJumpPress && B)
                {
                    if (this.Target.JumpDelay < 0)
                        this.Target.JumpDelay = 0;
                    this.Target.JumpStart = this.Target.LowestFloor;
                }

            }*/
            //Console.WriteLine(this.Target.Skeleton.Bones[this.Target.Skeleton.RootBone].GlobalMatrix.Translation.ToString());

            this.Target.JumpPress = B;
            bool chat = false;
            if (keyboardState.IsKeyDown(Keys.F1))
            {
                if (targetMoveset.chat1_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat1_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F2))
            {
                if (targetMoveset.chat2_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat2_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F3))
            {
                if (targetMoveset.chat3_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat3_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F4))
            {
                if (targetMoveset.chat4_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat4_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F5))
            {
                if (targetMoveset.chat5_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat5_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F6))
            {
                if (targetMoveset.chat6_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat6_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F7))
            {
                if (targetMoveset.chat7_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat7_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F8))
            {
                if (targetMoveset.chat8_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat8_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F9))
            {
                if (targetMoveset.chat9_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat9_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F10))
            {
                if (targetMoveset.chat10_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat10_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F11))
            {
                if (targetMoveset.chat11_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat11_;
                    chat = true;
                }
            }
            if (keyboardState.IsKeyDown(Keys.F12))
            {
                if (targetMoveset.chat12_ > -1)
                {
                    targetMoveset.PlayingIndex = targetMoveset.chat12_;
                    chat = true;
                }
            }
            if (chat)
            {
                targetMoveset.NextPlayingIndex = targetMoveset.idle;
                this.Target.cState = Model.ControlState.Chat;
            }
            if (keyboardState.IsKeyDown(Keys.O))
            {
                requestedLoc.Y += (5500 - requestedLoc.Y)/30f;
            }

            if (Y)
            {
                if (!this.Target.YPress)
                {
                    if (MainGame.ReactionCommand != null)
                    {
                        MainGame.ReactionCommand.Verify();
                        MainGame.ReactionCommand = null;
                        MainGame.UpdateReactionCommand = true;
                    }
                    this.Target.YPress = true;
                }
                //Program.game.px.Finish();
            }
            else
                this.Target.YPress = false;
            //Console.WriteLine(targetMoveset.Skeleton.Bones[0].LocalMatrix.Translation);
            if (A)
            {
                if (this.Target.Location.Y < this.Target.LowestFloor + 10)
                {
                    if (targetMoveset.attack1_ > -1)
                    {
                        targetMoveset.PlayingIndex = targetMoveset.attack1_;
                        targetMoveset.NextPlayingIndex = targetMoveset.idleFight_;
                        this.Target.cState = Model.ControlState.Attack1;
                        Program.game.CombatCountDown = 500;
                    }
                }
                else if (false)
                {
                    if (targetMoveset.attack1Air_ > -1 && this.Target.cState == Model.ControlState.Fall)
                    {
                        targetMoveset.PlayingIndex = targetMoveset.attack1Air_;
                        targetMoveset.NextPlayingIndex = targetMoveset.idleFight_;
                        this.Target.cState = Model.ControlState.Attack1Air;
                        Program.game.CombatCountDown = 500;
                    }
                }
            }
            //Console.WriteLine(Target.LowestFloor);

            //Program.game.cursors[0].Position = new Vector3(this.Target.Location.X, Target.LowestFloor,this.Target.Location.Z);

            this.Target.Location = requestedLoc;

            if (this.Target.Keyblade!=null)
            if (L2 > 0 || Program.game.CombatCountDown >0)
                this.Target.Keyblade.DestOpacity = 1;
            else
                this.Target.Keyblade.DestOpacity = 0;

            if (R1 && Program.game.CombatCountDown == 0)
                Program.game.CombatCountDown = 800;



            if (false)
            {
                if (oldKeyboardState.IsKeyDown(Keys.O))
                {
                    targetMoveset.NextPlayingIndex = 0;
                    targetMoveset.InterpolateAnimation = false;
                    targetMoveset.idleRest_=50;
                    targetMoveset.PlayingIndex = targetMoveset.idleRest_;
                    targetMoveset.AtmospherePlayingIndex = targetMoveset.idleRest_;
                }
                if (oldKeyboardState.IsKeyUp(Keys.I) && keyboardState.IsKeyDown(Keys.I) && targetMoveset.idle_ > 0)
                {
                    targetMoveset.idleRest_--;
                    targetMoveset.PlayingIndex = targetMoveset.idleRest_;
                    targetMoveset.AtmospherePlayingIndex = targetMoveset.idleRest_;
                    this.Target.pState = Model.State.BlockAll;
                    Console.WriteLine(targetMoveset.PlayingIndex);
                }
                if (oldKeyboardState.IsKeyUp(Keys.P) && keyboardState.IsKeyDown(Keys.P) || targetMoveset.idle_ < 0)
                {
                    targetMoveset.idleRest_++;
                    targetMoveset.PlayingIndex = targetMoveset.idleRest_;
                    targetMoveset.AtmospherePlayingIndex = targetMoveset.idleRest_;
                    this.Target.pState = Model.State.BlockAll;
                    Console.WriteLine(targetMoveset.PlayingIndex);
                }
            }
            if (keyboardState.IsKeyDown(Keys.P) && oldKeyboardState.IsKeyUp(Keys.P))
            {
                var player = Program.game.Player;
                var partn = Program.game.Partner1;
                Program.game.Player = partn;
                Program.game.Partner1 = player;
                Target = Program.game.Player;
            }

            if (keyboardState.IsKeyDown(Keys.D1) && oldKeyboardState.IsKeyUp(Keys.D1))
            {
                Program.game.Player = Program.game.Sora;
                Program.game.mainCamera.Target = Program.game.Sora;
                Program.game.Sora.Patches[0].GetPatch(2);
            }

            if (keyboardState.IsKeyDown(Keys.D2) && oldKeyboardState.IsKeyUp(Keys.D2))
            {
                if (Program.game.Roxas == null)
                {
                    Program.game.LoadCharacter(@"P_EX110\P_EX110.dae", @"P_EX110", ref Program.game.Roxas);
                    Program.game.Roxas.oldDist = Program.game.Player.Location + new Vector3(0,500,0);
                    Program.game.Roxas.Location = Program.game.Player.Location + new Vector3(0,500,0);
                    Program.game.Roxas.Patches[0].GetPatch(2);
                }
                Program.game.Player = Program.game.Roxas;
                Program.game.mainCamera.Target = Program.game.Roxas;
            }
            if (keyboardState.IsKeyDown(Keys.D3) && oldKeyboardState.IsKeyUp(Keys.D3))
            {
                if (Program.game.Riku == null)
                {
                    Program.game.LoadCharacter(@"P_EH000\P_EH000.dae", @"P_EH000", ref Program.game.Riku);
                    Program.game.Riku.oldDist = Program.game.Player.Location + new Vector3(0, 500, 0);
                    Program.game.Riku.Location = Program.game.Player.Location + new Vector3(0, 500, 0);
                }
                    Program.game.Player = Program.game.Riku;
                Program.game.mainCamera.Target = Program.game.Riku;
            }


            oldMouseState = mouseState;
            oldKeyboardState = keyboardState;
        }

        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                Vector3 camPos = this.lookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0)) * -this.zoom;
                return camPos;
            }
        }
        public Vector3 RealPosition
        {
            get
            {
                Vector3 camPos = this.lookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0)) * this.zoom;
                return camPos;
            }
        }
        public Vector3 SunBeamPosition
        {
            get
            {
                Vector3 camPos = this.lookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0)) * this.zoom*0.8f;
                return camPos;
            }
        }

        private Vector3 lookAt;
        public Vector3 LookAt
        {
            get { return lookAt; }
            set
            {
                lookAt = value;
            }
        }
        private Vector3 destLookAt;
        public Vector3 DestLookAt
        {
            get { return destLookAt; }
            set
            {

                destLookAt = value;
            }
        }
#endregion

#region ICamera Members        
        public Matrix ViewProjectionMatrix
        {
            get { return ViewMatrix * ProjectionMatrix; }
        }

        private Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (viewMatrixDirty)
                {
                    ReCreateViewMatrix();
                }

                Microsoft.Xna.Framework.Vector3 position = this.lookAt + Microsoft.Xna.Framework.Vector3.Normalize(this.lookAt - this.position) * -this.zoom;
                return Microsoft.Xna.Framework.Matrix.CreateLookAt(position, this.lookAt, Microsoft.Xna.Framework.Vector3.Up);
            }
        }
        
        private Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                if (projectionMatrixDirty)
                {
                    ReCreateProjectionMatrix();
                }
                return projectionMatrix;
            }
        }
#endregion
    }
}
