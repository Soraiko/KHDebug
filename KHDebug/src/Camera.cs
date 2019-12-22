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
            {yaw = value;}
        }

        public enum FieldsOfView
		{
			Default = 69,
			DiveWide = 45
		}

        private float fieldOfView = MathHelper.ToRadians((float)Camera.FieldsOfView.Default);
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
        public float MaxZoom = 450;
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
			Model target = this.Target;
            if (target == null)
                return;

			if (BulleSpeecher.CurrentBulle != null && BulleSpeecher.CurrentBulle.Type == Bulle.BulleType.Speech && MainGame.TState != MainGame.TransitionState.PreparingToTransit)
			{
				Model bulleEmmiter = BulleSpeecher.bulleEmmiters[BulleSpeecher.bulles.IndexOf(BulleSpeecher.CurrentBulle)];
				
				this.LookAt = bulleEmmiter.Location + bulleEmmiter.HeadHeight*0.92f;
				this.Zoom = bulleEmmiter.Epaisseur + 50f;
				target.DestOpacity = 0;
				target.Opacity = 0;


				Vector3 B_ = (bulleEmmiter.Location + bulleEmmiter.HeadHeight);
				Vector3 A_ = (target.Location + target.HeadHeight);
				float dist = Vector3.Distance(A_, B_);


				Vector3 diff_ = B_ - A_;
				float dist_ = Vector3.Distance((bulleEmmiter.Location + bulleEmmiter.HeadHeight), (target.Location + target.HeadHeight));

				float angle = (float)Math.Atan2((A_.X - B_.X) / dist, (A_.Z - B_.Z) / dist);

				bulleEmmiter.DestRotate = angle;
				bulleEmmiter.Rotate = angle;
				
				this.DestYaw = angle;
				this.Yaw = this.DestYaw;
				this.DestPitch = (float)Math.Atan2(diff_.Y/ dist_, 1);
				if (this.DestPitch > 0.2f)
					this.DestPitch = 0.2f;
				this.Pitch = this.DestPitch;
				return;
			}

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
            if (target.Speed > 0f && Math.Abs(diff) > 0.001)
            {
                float vitesse = target.Speed / (3200f);
                this.DestPitch -= diff * vitesse;
                viewMatrixDirty = true;
            }

            /* Déplacements de la camera */
            diff = (target.PrincipalDestRotate - this.PrincipalYaw);

            SrkBinary.MakePrincipal(ref diff);

            /* Ajustement horizontal doux de la camera */
            if (target.Speed > 0f && Math.Abs(diff) > 0.001 && Math.Abs(diff) < Math.PI * 0.75f)
            {
                float vitesse = target.Speed / (6400f);
                this.DestYaw += diff * vitesse;
                viewMatrixDirty = true;
            }

            diff = Vector3.Distance(this.DestLookAt, this.LookAt);
            if (diff > 0f)
            {
                this.LookAt += ((this.DestLookAt - this.LookAt) / (target.Cutscene ? 20f : 10f));
                viewMatrixDirty = true;
				YawPitch_matrix = Matrix.CreateFromYawPitchRoll(Program.game.mainCamera.Yaw, Program.game.mainCamera.Pitch, 0);
				Yaw_matrix = Matrix.CreateRotationY(Program.game.mainCamera.Yaw);
				Yaw_backwards_matrix = Matrix.CreateRotationY(-Program.game.mainCamera.Yaw);

			}

            if (!target.Cutscene && Program.game.MapSet && Program.game.Map.Links.Count > 0)
            {
                Vector3  end = this.LookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0)) * this.MaxZoom;
                
                Vector3 col = (Program.game.Map.Links[0] as Collision).GetCameraCollisionIntersect(this.LookAt, end, false);
                if (!Single.IsNaN(col.X))
                {
                    float newZoom = Vector3.Distance(this.LookAt, col);

                    if (newZoom < 10)
                        newZoom = 10;
                    
                    this.Zoom = newZoom;
                }
                else
                {
                    this.Zoom += (this.MaxZoom - this.Zoom)/3f;
                }
            }


            if (this.zoom < target.Epaisseur)
                target.DestOpacity = 0;
            else
                target.DestOpacity = 1;

        }
		public Matrix YawPitch_matrix = Matrix.Identity;
		public Matrix Yaw_matrix = Matrix.Identity;
		public Matrix Yaw_backwards_matrix = Matrix.Identity;


		float R2 = 0;
        bool R1 = false;
        float L2 = 0;
        bool L1 = false;

        public float joyLX_ = 0;
        public float joyLY_ = 0;
        float joyRX_ = 0;
        float joyRY_ = 0;

        float joyLX = 0;
        float joyLY = 0;
        float joyRX = 0;
        float joyRY = 0;

		public int LXY_read = 0;

		double oldAngle = 0;

		bool old_A = false;
		bool old_B = false;
		bool old_X = false;
		bool old_Y = false;

		/* Doit influener l'état du joueur MAIS PAS METTRE A JOUR SES PARAMATRES */
		public void GamepadControls(GameWindow win)
        {
			Model target = this.Target;

			if (target == null)
                return;
            float diff;
            this.gamepadState = GamePad.GetState(PlayerIndex.One);
            this.mouseState = Mouse.GetState(win);
            this.keyboardState = Keyboard.GetState();

            bool A = false;
            bool B = false;
            bool X = false;
            bool Y = false;

			if (LXY_read == 0)
			{
				joyLX_ = 0;
				joyLY_ = 0;
			}
            joyRX_ = 0;
            joyRY_ = 0;

            R1 = false;
            L1 = false;
            R2 = 0;
            L2 = 0;

			if (!target.Cutscene)
            {
                if (Program.game.UseXboxController)
                if (gamepadState.IsConnected)
            {
					if (LXY_read == 0)
					{
						joyLX_ = gamepadState.ThumbSticks.Left.X;
						joyLY_ = gamepadState.ThumbSticks.Left.Y;
					}
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
							if (LXY_read == 0)
							{
								joyLX_ = (float)((connectedControllers.First().ThumbLeftX - 50d) / 50d);
								joyLY_ = (float)((connectedControllers.First().ThumbLeftY - 50d) / 50d);
							}
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

				
				if (LXY_read == 0 && keyboardState.IsKeyDown(MainGame.france ? Keys.Z : Keys.W))
                {
                    joyLY = 1f;
                }
                if (LXY_read == 0 && keyboardState.IsKeyDown(Keys.S))
                {
                    joyLY = -1f;
                }
                if (LXY_read == 0 && keyboardState.IsKeyDown(MainGame.france ? Keys.Q : Keys.A))
                {
                    joyLX = -1f;
                }
                if (LXY_read == 0 && keyboardState.IsKeyDown(Keys.D))
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
            

            this.DestLookAt = target.Location + target.HeadHeight + new Vector3(0, 30, 0);



			var targetMoveset = (target.Links[0] as BinaryMoveset);
#if (playAnims)

            if (oldKeyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyDown(Keys.Down))
            {
                targetMoveset.PlayingIndex--;
                Console.WriteLine(targetMoveset.PlayingIndex);

            }
            if (oldKeyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyDown(Keys.Up))
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


			#region keyboardSets

			if (keyboardState.IsKeyDown(Keys.M) && oldKeyboardState.IsKeyUp(Keys.M))
			{

				Program.game.Musique = !Program.game.Musique;
				if (Program.game.Musique)
					Audio.SetVolume(Audio.BGM, 0);
				else
					Audio.SetVolume(Audio.BGM, 100);
			}

			if (keyboardState.IsKeyDown(Keys.F) && Program.game.MapSet)
			{
				Program.game.Collision = 0;
				Program.game.Map.Render = true;
			}
			if (keyboardState.IsKeyDown(Keys.G) && Program.game.MapSet)
			{
				Program.game.Collision = 1;
				Program.game.Map.Render = false;
			}
			if (keyboardState.IsKeyDown(Keys.H) && Program.game.MapSet)
			{
				Program.game.Collision = 2;
				Program.game.Map.Render = true;
			}
			if (keyboardState.IsKeyDown(Keys.J) && Program.game.MapSet)
			{
				Program.game.Collision = -1;
				Program.game.Map.Render = false;
			}
			#endregion


			bool allowControls = BulleSpeecher.CurrentBulle == null;

			if (BulleSpeecher.CurrentBulle != null && !old_A && A)
			{
				BulleSpeecher.CurrentBulle.NextText();
			}

			#region Controls
			if (allowControls)
			{
				if (joyRY < 0 || joyRY > 0)
					this.DestPitch += (joyRY / 15f);
				if (joyRX < 0 || joyRX > 0)
					this.DestYaw -= (joyRX / 15f);

				float hypo = (float)Math.Sqrt(joyLX * joyLX + joyLY * joyLY);
				if (LXY_read > 0)
					hypo = target.SmoothJoystick;
				float realHypo = hypo * 1f;
				target.Joystick = realHypo;

				if (target.cState == Model.ControlState.Fall)
					target.SmoothJoystick += (hypo - target.SmoothJoystick) / 20f;
				else if (target.cState != Model.ControlState.Fly || hypo > target.SmoothJoystick)
					target.SmoothJoystick = hypo;


				Vector3 requestedLoc = target.Location;

				if (target.cState == Model.ControlState.Cliff)
				{
					if (target.SmoothJoystick < 0.1)
						target.AllowCliffCancel = true;
					if (target.AllowCliffCancel)
					{
						double angle_ = Math.Atan2(joyLY / hypo, joyLX / hypo);

						float joyDir_ = (float)(this.yaw + angle_ + Math.PI / 2f);
						float curDir_ = target.DestRotate;

						if (target.CliffCancel || ((hypo > 0.6) && Math.Abs(Math.Cos(joyDir_)) > 0.3 &&
							Math.Cos(joyDir_) < 0 ^ Math.Cos(curDir_) < 0

							))
						{
							target.CliffCancel = true;
							targetMoveset.AtmospherePlayingIndex = targetMoveset.fall_;
							targetMoveset.PlayingIndex = targetMoveset.fall_;
							target.cState = Model.ControlState.Fall;
						}
					}
				}
				else
					target.AllowCliffCancel = false;

				bool skate = target.Links[0].ResourceIndex == 39;


				if (hypo > 0.3)
				{
					if (target.cState == Model.ControlState.Fly)
						hypo = target.SmoothJoystick;


					float step = 0;

					target.WalkSpeed_ += (target.WalkSpeed - target.WalkSpeed_)/4f;
					target.RunSpeed_ += (target.RunSpeed - target.RunSpeed_) / 4f;

					if (hypo < 0.75)
						step = target.WalkSpeed_;
					else
						step = target.RunSpeed_;


					if (target.cState != Model.ControlState.Cliff)
						if (target.cState != Model.ControlState.Land || target.pState == Model.State.Gravity_Slide)
							if (target.cState != Model.ControlState.Guarding)
								if (target.cState != Model.ControlState.UnGuarding)
									if (target.pState != Model.State.BlockAll || target.pState == Model.State.NoIdleWalkRun_ButRotate)
									{
										double angle = Math.Atan2(joyLY / hypo, joyLX / hypo);
										if (target.pState != Model.State.BlockAll)
										{
											target.DestRotate = (float)(this.yaw + angle + Math.PI / 2f);

											if (skate)
											{
												if (angle - oldAngle > 0.01)
													targetMoveset.run = targetMoveset.flyLeft_;
												else if (angle - oldAngle < -0.01)
													targetMoveset.run = targetMoveset.flyRight_;
												else
													targetMoveset.run = targetMoveset.flyIdle_;

											}
											else
											{
												if (Math.Abs(angle - (Math.PI / 2f)) < 0.3f)
												{
													targetMoveset.fly_ = targetMoveset.flyForward_;
												}
												else
												if (angle - (Math.PI / 2f) < 0)
													targetMoveset.fly_ = targetMoveset.flyRight_;
												else
													targetMoveset.fly_ = targetMoveset.flyLeft_;
											}
										}
										oldAngle = angle;
										if (target.pState != Model.State.BlockAll || target.pState == Model.State.NoIdleWalkRun_ButRotate)
										{
											requestedLoc.X += (float)((1 / 60f) * (step) * Math.Sin((this.yaw + angle + Math.PI / 2f)));
											requestedLoc.Z += (float)((1 / 60f) * (step) * Math.Cos((this.yaw + angle + Math.PI / 2f)));
											KHDebug.Collision.MonitorCollision(target, ref requestedLoc);
										}
									}
				}
				else
				{
					target.WalkSpeed_ = 0;
					target.RunSpeed_ = 0;
					if (skate && (targetMoveset.run == targetMoveset.flyLeft_ || targetMoveset.run == targetMoveset.flyRight_))
						targetMoveset.run = targetMoveset.flyIdle_;
				}
				

				bool chat = false;


				if (keyboardState.IsKeyDown(Keys.F1))
				{
					if (targetMoveset.chat1_ > -1)
					{
						if (skate)
						{
							if (target.Location.Y < target.LowestFloor + target.StairHeight)
							{
								A = true;
							}
						}
						else
						{
							targetMoveset.PlayingIndex = targetMoveset.chat1_;
							chat = true;
						}
					}
				}
				if (keyboardState.IsKeyDown(Keys.F2))
				{
					if (targetMoveset.chat2_ > -1)
					{
						if (skate)
						{
							target.JumpCancel = true;
							if (targetMoveset.PlayingFrame > 36)
								targetMoveset.PlayingFrame = 0;
							targetMoveset.PlayingIndex = targetMoveset.chat2_;
							targetMoveset.NextPlayingIndex = targetMoveset.idle;
							target.cState = Model.ControlState.Chat;
						}
						else
						{
							chat = true;
							targetMoveset.PlayingIndex = targetMoveset.chat2_;
						}
					}
				}
				if (keyboardState.IsKeyDown(Keys.F3))
				{
					if (skate)
					{
						A =  target.Location.Y > target.LowestFloor + target.StairHeight * 2f;
					}
					else
					if (targetMoveset.chat3_ > -1)
					{
						targetMoveset.PlayingIndex = targetMoveset.chat3_;
						chat = true;
					}
				}
				if (keyboardState.IsKeyDown(Keys.F4))
				{
					if (skate)
					{
						X = target.Location.Y > target.LowestFloor + target.StairHeight * 2f;
					}
					else
					if (targetMoveset.chat4_ > -1)
					{
						targetMoveset.PlayingIndex = targetMoveset.chat4_;
						chat = true;
					}
				}
				if (keyboardState.IsKeyDown(Keys.F5))
				{
					if (skate)
					{
						B = target.Location.Y > target.LowestFloor + target.StairHeight * 2f;
					}
					else
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
					target.cState = Model.ControlState.Chat;
				}

				if (keyboardState.IsKeyDown(Keys.O))
				{
					requestedLoc.Y += (5500 - requestedLoc.Y) / 30f;
				}

				if (X)
				{
					if (target.Location.Y < target.LowestFloor + target.StairHeight) /* au sol*/
					{
						if (!old_X) // 
						{
							if (target.Links.Count == 2)
							{
								int ind = Audio.names.IndexOf(@"Content\Effects\Audio\Sounds\F_TT010\skate_run.wav");
								if (ind > -1)
								{
									Audio.effectInstances[ind].Stop();
								}
								Action.SetMset(target, null);
								target.Gravity = 16f;
								targetMoveset = (target.Links[0] as BinaryMoveset);
								if (Program.game.CombatCountDown > 0)
								{
									targetMoveset.PlayingIndex = targetMoveset.guarding_;
									target.cState = Model.ControlState.Guarding;
								}
							}
							else
							if (targetMoveset.roll_ > -1 && realHypo > 0.5f)
							{
								if (!target.oldRollPress)
								{
									target.rollPress = X;
									if (targetMoveset.PlayingFrame > targetMoveset.MaxTick - 5)
									{
										target.pState = Model.State.GoToAtMove;
										targetMoveset.PlayingIndex = targetMoveset.idle_;
									}
									targetMoveset.PlayingIndex = targetMoveset.roll_;
									target.cState = Model.ControlState.Roll;
								}
							}
							else
							if (targetMoveset.guard_ > -1 && realHypo < 0.3f)
							{
								targetMoveset.PlayingIndex = targetMoveset.guard_;
								targetMoveset.NextPlayingIndex = targetMoveset.guarding_;
								target.cState = Model.ControlState.Guard;
								Program.game.CombatCountDown = MainGame.CombatcountDownMax;
							}
						}
					}
					else /* pas au sol*/
					{
						if (skate)
						{
							if (!old_X && target.Location.Y > target.LowestFloor + target.StairHeight * 2f)
							{
								target.JumpCancel = true;
								targetMoveset.fall_ = targetMoveset.chat4_;
								targetMoveset.PlayingIndex = targetMoveset.fall_;
								targetMoveset.NextPlayingIndex = targetMoveset.fall1_;
								target.cState = Model.ControlState.Fall;
							}
						}
						else
						if (A && target.Location.Y > target.LowestFloor + 800)
						{
							if (targetMoveset.fall2_ > -1)
							{
								target.Fly = false;
								targetMoveset.fall_ = targetMoveset.fall2_;
							}
						}
						else if (targetMoveset.fall_ == targetMoveset.fall1_)
						{
							if (targetMoveset.fly_ > -1 && (target.cState == Model.ControlState.Jump || target.cState == Model.ControlState.Fall))
							{
								if (!target.oldFlyPress)
								{
									target.Fly = true;
								}
							}
						}
					}
					target.oldFlyPress = true;
					target.oldRollPress = true;
				}
				else
				{
					target.Fly = false;
					if (target.cState == Model.ControlState.Jump || target.cState == Model.ControlState.Fall)
					{
						target.oldFlyPress = false;
					}
					if (target.cState == Model.ControlState.Land || (int)target.cState < 3)
					{
						target.oldRollPress = false;
					}


					if (target.cState != Model.ControlState.Fall)
						targetMoveset.fall_ = targetMoveset.fall1_;
				}

				if (!B && (target.cState == Model.ControlState.Cliff || target.Location.Y < target.LowestFloor + 20))
				{
					target.oldJumpPress = false;
				}

				if (B)
				{

					if (!old_B && skate && target.Location.Y > target.LowestFloor + target.StairHeight * 2f &&
						(targetMoveset.PlayingIndex != targetMoveset.chat3_ || targetMoveset.PlayingFrame > 28))
					{
						target.JumpCancel = true;
						if (targetMoveset.PlayingFrame > 30)
							targetMoveset.PlayingFrame = 0;
						targetMoveset.fall_ = targetMoveset.chat5_;
						targetMoveset.PlayingIndex = targetMoveset.fall_;
						targetMoveset.NextPlayingIndex = targetMoveset.fall1_;
						target.cState = Model.ControlState.Fall;
					}
					else
					if (target.cState == Model.ControlState.Cliff)
					{
						if (!target.oldJumpPress)
							target.CliffCancel = true;
					}
					else
					{
						if (target.pState != Model.State.BlockAll && !target.oldJumpPress)
						{
							target.JumpPress = true;
							target.JumpCancel = false;
							target.JumpCollisionCancel = false;
							target.JumpDelay = target.cState == Model.ControlState.Land ? 6 : 0;
							target.JumpStart = target.LowestFloor;
							target.oldJumpPress = true;
						}
					}
				}
				else
					target.JumpPress = false;


				if (Y)
				{
					if (!target.YPress)
					{
						if (MainGame.ReactionCommand != null)
						{
							MainGame.ReactionCommand.Verify();
							MainGame.transitionType = MainGame.transitionType_rc;
							MainGame.Transition = MainGame.Transition_rc;

							if (MainGame.transitionType != MainGame.TransitionType.None)
								MainGame.TState = MainGame.TransitionState.PreparingToTransit;
							
							if (MainGame.RotateAttract)
							{
								target.Joystick = 0;
								target.SmoothJoystick = 0;

								var targetMset = (target.Links[0] as Moveset);
								if (targetMset != null)
								{
									targetMset.PlayingIndex = 0;
									target.pState = Model.State.BlockAll;
								}

								Vector3 A_ = BulleSpeecher.NoEmmiter;

								if (MainGame.DoModel != null)
									A_ = (MainGame.DoModel.Location + MainGame.DoModel.Skeleton.Bones[MainGame.DoBone].GlobalMatrix.Translation);

								Vector3 B_ = target.Location;
								float dist = Vector3.Distance(A_, B_);
								float angle = (float)Math.Atan2((A_.X - B_.X) / dist, (A_.Z - B_.Z) / dist);
								target.DestRotate = angle;
							}

							MainGame.ReactionCommand = null;
							MainGame.UpdateReactionCommand = true;
						}
						target.YPress = true;
					}
					//Program.game.px.Finish();
				}
				else
					target.YPress = false;
				//Console.WriteLine(targetMoveset.Skeleton.Bones[0].LocalMatrix.Translation);
				if (!old_A && A)
				{
					if (target.Location.Y < target.LowestFloor + 10)
					{
						if (skate)
						{
							if (targetMoveset.PlayingFrame > 25)
							{
								targetMoveset.PlayingFrame = 0;
							}
							targetMoveset.PlayingIndex = targetMoveset.chat1_;
							targetMoveset.NextPlayingIndex = targetMoveset.land1_;
							target.cState = Model.ControlState.Chat;
							target.JumpCancel = true;
						}
						else if (targetMoveset.attack1_ > -1)
						{
							targetMoveset.PlayingIndex = targetMoveset.attack1_;
							targetMoveset.NextPlayingIndex = targetMoveset.idleFight_;
							target.cState = Model.ControlState.Attack1;
							Program.game.CombatCountDown = MainGame.CombatcountDownMax;
						}
					}
					else if (skate && target.Location.Y > target.LowestFloor + target.StairHeight * 2f && 
						(targetMoveset.PlayingIndex != targetMoveset.chat5_ ||targetMoveset.PlayingFrame > 30))
					{
						target.JumpCancel = true;
						if (targetMoveset.PlayingFrame > 28)
							targetMoveset.PlayingFrame = 0;
						targetMoveset.fall_ = targetMoveset.chat3_;
						targetMoveset.PlayingIndex = targetMoveset.fall_;
						targetMoveset.NextPlayingIndex = targetMoveset.fall1_;
						target.cState = Model.ControlState.Fall;
					}
					else if (false)
					{
						if (targetMoveset.attack1Air_ > -1 && target.cState == Model.ControlState.Fall)
						{
							targetMoveset.PlayingIndex = targetMoveset.attack1Air_;
							targetMoveset.NextPlayingIndex = targetMoveset.idleFight_;
							target.cState = Model.ControlState.Attack1Air;
							Program.game.CombatCountDown = MainGame.CombatcountDownMax;
						}
					}
				}

				//Program.game.cursors[0].Position = new Vector3(target.Location.X, target.LowestFloor,target.Location.Z);

				target.Location = requestedLoc;


				if (target.Keyblade != null)
					if (L2 > 0 || Program.game.CombatCountDown > 0)
						target.Keyblade.DestOpacity = 1;
					else
						target.Keyblade.DestOpacity = 0;


				if (keyboardState.IsKeyDown(Keys.W) && oldKeyboardState.IsKeyUp(Keys.W))
				{
					PAXCaster.RunPAX(@"Content\Effects\Visual\SavePoint", null, -1, new Vector3(-1911.05f, 1.141877f, -685.3207f));
					//PAXCaster.RunPAX(@"Content\Effects\Visual\SavePoint", null, -1, new Vector3(MainGame.rnd.Next(-1000, 1000), 1.141877f, MainGame.rnd.Next(-1000, 1000)));
					//Program.game.ticksAlways = 81;
				}

				if (keyboardState.IsKeyDown(Keys.P) && oldKeyboardState.IsKeyUp(Keys.P))
				{
					var player = Program.game.Player;
					var partn = Program.game.Partner2;
					Program.game.Player = partn;
					Program.game.Partner2 = player;
					this.Target = Program.game.Player;
				}

				if (keyboardState.IsKeyDown(Keys.D1) && oldKeyboardState.IsKeyUp(Keys.D1))
				{
					Program.game.Player.SpawnedLocation = Program.game.Player.Location;
					Program.game.Player.NPC = true;
					Program.game.Player = Program.game.Sora;
					Program.game.Player.NPC = false;
					this.Target = Program.game.Sora;
					Program.game.Sora.Patches[0].GetPatch(2);
				}

				if (keyboardState.IsKeyDown(Keys.D2) && oldKeyboardState.IsKeyUp(Keys.D2))
				{
					if (Program.game.Roxas == null)
					{
						Spawn.LoadCharacter(@"P_EX110", @"P_EX110", ref Program.game.Roxas);
						Program.game.Roxas.oldDist = Program.game.Player.Location + new Vector3(0, 500, 0);
						Program.game.Roxas.Location = Program.game.Player.Location + new Vector3(0, 500, 0);
						Program.game.Roxas.Patches[0].GetPatch(2);
						Program.game.Roxas.LowestFloor = Program.game.Roxas.Location.Y;
					}
					Program.game.Player.SpawnedLocation = Program.game.Player.Location;
					Program.game.Player.NPC = true;
					Program.game.Player = Program.game.Roxas;
					Program.game.Player.NPC = false;
					this.Target = Program.game.Roxas;
				}
				if (keyboardState.IsKeyDown(Keys.D3) && oldKeyboardState.IsKeyUp(Keys.D3))
				{
					if (Program.game.Riku == null)
					{
						Spawn.LoadCharacter(@"P_EH000", @"P_EH000", ref Program.game.Riku);
						Program.game.Riku.oldDist = Program.game.Player.Location + new Vector3(0, 500, 0);
						Program.game.Riku.Location = Program.game.Player.Location + new Vector3(0, 500, 0);
						Program.game.Riku.LowestFloor = Program.game.Riku.Location.Y;
					}
					Program.game.Player.SpawnedLocation = Program.game.Player.Location;
					Program.game.Player.NPC = true;
					Program.game.Player = Program.game.Riku;
					Program.game.Player.NPC = false;
					this.Target = Program.game.Riku;
				}


			}
			#endregion



			if (targetMoveset.fall2_ > -1)
			if (targetMoveset.fall1_ != targetMoveset.fall2_ && targetMoveset.PlayingIndex == targetMoveset.fall2_)
            {
                if (!target.justDive)
                {
                    PAXCaster.RunPAX(@"Content\Effects\Visual\DiveWind\DiveWind.dae", null, -1, Vector3.Zero);
                    Audio.Play(@"Content\Effects\Audio\Sounds\Shared\windLoop.wav", true, null, 0);
                    target.justDive = true;
                }
                for (int i = 0; i < Audio.names.Count; i++)
                {
                    if (Audio.names[i] == @"Content\Effects\Audio\Sounds\Shared\windLoop.wav")
                    {
                        Audio.effectInstances[i].Volume += (0.125f - Audio.effectInstances[i].Volume) / 10f;
                    }
                    if (Program.game.MapSet && Audio.names[i] == @"Content\Effects\Audio\Ambient\"+Program.game.Map.Name+".wav")
                    {
                        Audio.effectInstances[i].Volume += (0 - Audio.effectInstances[i].Volume) / 4f;
                    }
                }
                target.Gravity = 20f;
                if (this.FieldOfView > MathHelper.ToRadians((float)FieldsOfView.DiveWide))
                {
                    if (targetMoveset.PlayingFrame < 70)
                    {
                        this.FieldOfView += (MathHelper.ToRadians((float)FieldsOfView.DiveWide) - this.FieldOfView) / 40;
                        this.DestPitch += (-1.5f - this.Pitch) / 30f;
                    }
                    else
                    {
                        this.FieldOfView += (MathHelper.ToRadians((float)FieldsOfView.DiveWide) - this.FieldOfView) / 5;
                        this.DestPitch += (-1.5f - this.Pitch) / 10f;
					}
						projectionMatrixDirty = true;
				}
            }
            else
            {
                if (target.justDive)
                {
                    PAXCaster.FinishPAX(@"Content\Effects\Visual\DiveWind\DiveWind.dae");
                    int windCount = 0;
                    int zeroCount = 0;
                    for (int i = 0; i < Audio.names.Count; i++)
                    {
                        if (Audio.names[i] == @"Content\Effects\Audio\Sounds\Shared\windLoop.wav")
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
                    target.justDive = false;
					target.Gravity = 16f;
				}
				diff = MathHelper.ToRadians((float)FieldsOfView.Default) - this.FieldOfView;

				if (diff > 0 || diff < 0)
				{
					this.FieldOfView += diff/10f;
					projectionMatrixDirty = true;
				}
            }





			old_A = A;
			old_B = B;
			old_X = X;
			old_Y = Y;
			oldMouseState = mouseState;
            oldKeyboardState = keyboardState;
			if (LXY_read > 0)
			LXY_read--;
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
