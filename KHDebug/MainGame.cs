#define playnAnims
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KHDebug
{
	public partial class MainGame : Game
    {
        public static bool france = System.Globalization.CultureInfo.CurrentCulture.EnglishName.ToLower().Contains("rance");
        public long ticks = -1;
        public long ticksAlways = -1;
        public static Stopwatch stp;
        public int Collision = 0;
        public bool Musique = false;

        public float FPS = 60;
        public float FPS_ = 15;
        public int CombatCountDown = 0;
        public static int CombatcountDownMax = 50000000;

        public static Vector3 MinVertex = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        public static Vector3 MaxVertex = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
        public static Model DoModel = null;
        public static int DoBone = -1;
        public static Vector3 DoVector = Vector3.Zero;

        public static bool RotateAttract;
        public static Action ReactionCommand;
        public static bool UpdateReactionCommand = false;




        public BasicEffect basicEffect;
        AlphaTestEffect alphaTest;
        KeyboardState keyboardState;
        KeyboardState oldKeyboardState;
        public SpriteBatch spriteBatch;
        public GraphicsDeviceManager graphics;
        public static RasterizerState rasterSolid;
        public static RasterizerState rasterWireframe;
        public static List<Resource> ResourceFiles;
        public static RasterizerState rasterSolidNoCull;
        public Microsoft.Xna.Framework.Color backgroundColor = new Color(0, 0, 0);


        int MultisampleCount = 1;
        public bool UseXboxController = true;
        bool EnableAntiAliasing = false;

        string[] graphicSettings = new string[] {
            "MultisampleCount=1",
            "EnableAntiAliasing=True",
            "UseXboxController=True"
        };


        public MainGame()
		{
			graphics = new GraphicsDeviceManager(this)
		   {
			   /*PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.75f),
			   PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.75f)*/

				PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.454166f),
				PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.605555f)
		   };
            Content.RootDirectory = "Content";
            Window.Title = title;
            LoadGraphicSettings();
        }

		public void GetData(Vector3 a, Vector3 b, double part, ref Vector3 c)
        {

            Vector3 module = (b - a) / Vector3.Distance(b, a);
            c = a + module * (float)part;
        }

        public const string title = "Kingdom Hearts Debug - v1.0.4";


        public List<Cursor> cursors;
        public Model map1;
        public Model map2;


		protected override void Initialize()
		{

			cursors = new List<Cursor>(0);
			cursors.Add(new Cursor());
			cursors[0].Color = Color.Red;
			cursors.Add(new Cursor());
            cursors.Add(new Cursor());
			cursors[1].Color = new Color(0,255,0);
            cursors[2].Color = new Color(0,0,255);


			/*FileStream fs = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08\texture",FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);
			bw.Write(632);
			bw.Write(0);
			bw.Write(0);
			bw.Write(0);
			int nextfile_start = 16 + 632 * 8;
			while (nextfile_start % 16 > 0) nextfile_start++;

			bw.Write(new byte[nextfile_start-16]);

			for (int i=0;i<632;i++)
			{
				fs.Position = fs.Length;
				byte[] data2 = File.ReadAllBytes(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08\texture_" + i.ToString("d3") + ".png");
				Texture2D t2 = Texture2D.FromStream(graphics.GraphicsDevice, new MemoryStream(data2));
				byte[] data = new byte[t2.Width * t2.Height * 4];
				t2.GetData<byte>(data);

				int datalength = data.Length;
				bw.Write(data);
				while (datalength % 16 >0)
				{
					bw.Write((byte)0);
					datalength++;
				}
				fs.Position = 16 + i * 8;

				bw.Write(nextfile_start);
				bw.Write((short)t2.Width);
				bw.Write((short)t2.Height);


				nextfile_start += datalength;
			}
			bw.Close();
			fs.Close();*/

			base.Initialize();
			InitializeAll();

			/*DAE d = new DAE(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\SavePoint\p1\p1.dae");
			d.Parse();
			//d.ExportBin();
			int index = 0;

			FileStream fs1 = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\SavePoint\p1\MSET\move_00" + index.ToString() + ".bin", FileMode.Create);
			BinaryWriter wr1 = new BinaryWriter(fs1);

			FileStream fs2 = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\SavePoint\p1\MSET\move_00" + index.ToString() + ".frames", FileMode.Create);
			BinaryWriter wr2 = new BinaryWriter(fs2);

			FileStream fs1_2 = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\SavePoint\p2\MSET\move_00" + index.ToString() + ".bin", FileMode.Create);
			BinaryWriter wr1_2 = new BinaryWriter(fs1_2);

			FileStream fs2_2 = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\SavePoint\p2\MSET\move_00" + index.ToString() + ".frames", FileMode.Create);
			BinaryWriter wr2_2 = new BinaryWriter(fs2_2);


			float frames = 500;

			wr1.Write((int)frames);
			wr1.Write(0);
			wr1.Write(0xFF000000);
			wr1.Write(0);

			wr1_2.Write((int)frames);
			wr1_2.Write(0);
			wr1_2.Write(0xFF000000);
			wr1_2.Write(0);
			


			for (int i = 0; i < frames; i++)
			{
				float v1 = (float)(Math.Sin((i / 250f) * Math.PI) + 1) / 2f;
				float v2 = 1 - v1;

				wr2.Write((ushort)11);
					wr2_2.Write((ushort)11);
				wr2.Write((ushort)0);
					wr2_2.Write((ushort)0);
				wr2.Write(1.000f * v1 + 0.745f * v2);
					wr2_2.Write(1.000f * v1 + 0.745f * v2);

				wr2.Write((ushort)12);
					wr2_2.Write((ushort)12);
				wr2.Write((ushort)0);
					wr2_2.Write((ushort)0);
				wr2.Write(0.999f * v1 + 1.000f * v2);
					wr2_2.Write(0.999f * v1 + 1.000f * v2);

				wr2.Write((ushort)13);
					wr2_2.Write((ushort)13);
				wr2.Write((ushort)0);
					wr2_2.Write((ushort)0);
				wr2.Write(0.745f * v1 + 0.982f * v2);
					wr2_2.Write(0.745f * v1 + 0.982f * v2);

				wr2.Write((ushort)11);
					wr2_2.Write((ushort)11);
				wr2.Write((ushort)1);
					wr2_2.Write((ushort)1);
				wr2.Write(1.000f * v1 + 0.745f * v2);
					wr2_2.Write(1.000f * v1 + 0.745f * v2);

				wr2.Write((ushort)12);
					wr2_2.Write((ushort)12);
				wr2.Write((ushort)1);
					wr2_2.Write((ushort)1);
				wr2.Write(0.999f * v1 + 1.000f * v2);
					wr2_2.Write(0.999f * v1 + 1.000f * v2);

				wr2.Write((ushort)13);
					wr2_2.Write((ushort)13);
				wr2.Write((ushort)1);
					wr2_2.Write((ushort)1);
				wr2.Write(0.745f * v1 + 0.982f * v2);
					wr2_2.Write(0.745f * v1 + 0.982f * v2);

				wr2.Write(-1);
					wr2_2.Write(-1);



				for (int j = 0; j < d.Skeleton.Bones.Count; j++)
				{
					Matrix m1 = d.Skeleton.Bones[j].LocalMatrix*Matrix.CreateScale(1.05f,1f,1.05f);
					Matrix m2 = d.Skeleton.Bones[j].LocalMatrix * Matrix.CreateScale(1.05f, 1f, 1.05f);

					if (j == 0)
					{
						m2 *= Matrix.CreateRotationY(MainGame.PI);
						//m1 *= Matrix.CreateRotationY(-(i / 250f) * MainGame.PI * 6f);
						//m2 *= Matrix.CreateRotationY(-(i / 250f) * MainGame.PI * 6f);
					}
					else
					{
						m1 *= Matrix.CreateRotationY(-(i / 250f) * MainGame.PI * 6f);
						m2 *= Matrix.CreateRotationY(-(i / 250f) * MainGame.PI * 6f);


						int ind = (i + ((17-j)) * 4) % (int)frames;

						
						if ((ind%250) <= 62.5f)
							m1 *= Matrix.CreateScale(1 - (float)Math.Sin(((ind % 250) / 62.5f) * Math.PI) * 0.1f) * Matrix.CreateTranslation(new Vector3(0, -50 + ((ind % 250) / 62.5f) * 80f, 0)); //monte
						else if ((ind % 250) > 62 && (ind % 250) <= 125)
							m1 *= Matrix.CreateTranslation(new Vector3(0, 30, 0)); //haut
						else if ((ind % 250) > 125 && (ind % 250) <= 187.5f)
							m1 *= Matrix.CreateScale(1 - (float)Math.Sin((((ind % 250) - 125) / 62.5f) * Math.PI) * 0.1f) * Matrix.CreateTranslation(new Vector3(0, 30 + (((ind % 250) - 125) / 62.5f) * -80f, 0)); //descend
						else if ((ind % 250) > 187.5f)
							m1 *= Matrix.CreateTranslation(new Vector3(0, -50f, 0)); //bas

						
						if ((ind % 250) <= 62.5f)
							m2 *= Matrix.CreateScale(1-(float)Math.Sin(((ind % 250) / 62.5f)*Math.PI) *0.1f) * Matrix.CreateTranslation(new Vector3(0, 30 + ((ind % 250) / 62.5f) * -80f, 0)); //descend
						else if ((ind % 250) > 62 && (ind % 250) <= 125)
							m2 *= Matrix.CreateTranslation(new Vector3(0, -50f, 0)); //bas
						else if ((ind % 250) > 125 && (ind % 250) <= 187.5f)
							m2 *= Matrix.CreateScale(1 - (float)Math.Sin((((ind % 250) - 125) / 62.5f) * Math.PI) * 0.1f) * Matrix.CreateTranslation(new Vector3(0, -50 + (((ind % 250) - 125) / 62.5f) * 80f, 0)); //monte
						else if ((ind % 250) > 187.5f)
							m2 *= Matrix.CreateTranslation(new Vector3(0, 30, 0)); //haut
						
						Vector3 t1 = m1.Translation;
						Vector3 t2 = m2.Translation;

						int ind2 = (i + ((17 - j)) * 4) % (int)frames;

						m1 *= Matrix.CreateTranslation(-t1);
						m1 *= Matrix.CreateScale(1f+(float)Math.Sin(ind2/5f)*0.25f);
						m1 *= Matrix.CreateTranslation(t1);

						m2 *= Matrix.CreateTranslation(-t2);
						m2 *= Matrix.CreateScale(1f + (float)Math.Sin(ind2 / 5f) * 0.25f);
						m2 *= Matrix.CreateTranslation(t2);

						if (i<64)
						{
							Vector3 modulo = t1 / Vector3.Distance(Vector3.Zero, t1);
							double angle = Math.Atan2(modulo.Z, modulo.X);
							while (angle < 0)
								angle += Math.PI * 2;
							
							if (angle < 0.6)
							{
								m1 *= Matrix.CreateTranslation(-t1);
								m1 *= Matrix.CreateScale(0f);

								m2 *= Matrix.CreateTranslation(-t2);
								m2 *= Matrix.CreateScale(0f);

								m1 *= Matrix.CreateTranslation(t1);
								m2 *= Matrix.CreateTranslation(t2);
							}
						}
					}
					wr1.Write(m1.M11);
						wr1_2.Write(m2.M11);
					wr1.Write(m1.M12);
						wr1_2.Write(m2.M12);
					wr1.Write(m1.M13);
						wr1_2.Write(m2.M13);
					wr1.Write(m1.M14);
						wr1_2.Write(m2.M14);
					wr1.Write(m1.M21);
						wr1_2.Write(m2.M21);
					wr1.Write(m1.M22);
						wr1_2.Write(m2.M22);
					wr1.Write(m1.M23);
						wr1_2.Write(m2.M23);
					wr1.Write(m1.M24);
						wr1_2.Write(m2.M24);
					wr1.Write(m1.M31);
						wr1_2.Write(m2.M31);
					wr1.Write(m1.M32);
						wr1_2.Write(m2.M32);
					wr1.Write(m1.M33);
						wr1_2.Write(m2.M33);
					wr1.Write(m1.M34);
						wr1_2.Write(m2.M34);
					wr1.Write(m1.M41);
						wr1_2.Write(m2.M41);
					wr1.Write(m1.M42);
						wr1_2.Write(m2.M42);
					wr1.Write(m1.M43);
						wr1_2.Write(m2.M43);
					wr1.Write(m1.M44);
						wr1_2.Write(m2.M44);
				}
			}

			wr1.Close();
			fs1.Close();
			wr2.Close();
			fs2.Close();

			wr1_2.Close();
			fs1_2.Close();
			wr2_2.Close();
			fs2_2.Close();*/



			/*Loading = LoadingState.PreparingToLoad;

            new Thread(() =>
            {
               Thread.CurrentThread.Priority = ThreadPriority.Highest;*/


			/*SetMap(@"Content\Models\TT09\TT09.mdl", true);
			SetMap(@"Content\Models\TT08\TT08.mdl", false);*/



			/*Loading = LoadingState.NotLoading;

            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            }).Start();*/


			/*Model partner2 = new DAE(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\P_EX030\P_EX030.dae");
			partner2.Parse();
			MSET mset = new MSET(@"Content\Models\P_EX030\P_EX030.mset");
			mset.Links.Add(partner2);
			mset.Parse();
			mset.ExportBinary();
			mset.ExportBinary();*/

		}

		public static List<string> BufferedMaps = new List<string>(0);
        public static List<MAP> Maps = new List<MAP>(0);

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ResourceFiles = new List<Resource>(0);
			Resource.ResourceIndices = File.ReadAllLines(@"Content\Models\objentry.txt");
		}

        protected override void UnloadContent()
        {
		}
        public const float PI = (float)Math.PI;
        double seconds = 0;

        public Camera mainCamera;

        public Vector3 camPos;
        public Vector3 lookAt;

		public Model Player;
		public Model Partner1;
		public Model Partner2;

		public Model Sora;
        public Model Roxas;
        public Model Riku;
        public Model Donald;

        public Model LastLoadedNotParty;

		public bool MapSet = false;
		public MAP map_ = null;

		public MAP Map
		{
			get
			{
				return this.map_;
			}
			set
			{
				MapSet = value != null;
				this.map_ = value;
			}
		}


        int FullScreenState = 0;

		public static Random rnd = new Random();

        public static int bufferWidth = 0;
        public static int bufferHeight = 0;
        

        protected override void Update(GameTime gameTime)
		{
            keyboardState = Keyboard.GetState();

			/* Update everything */
			ThreadCopy();
			WindowBoundsUpdate();
			SrkFontDisplayer.Clear();

			/* Fake loading (beta only) */
			if (keyboardState.IsKeyDown(Keys.L) && oldKeyboardState.IsKeyUp(Keys.L))
				if (Program.game.Loading < 0) Program.game.Loading = MainGame.LoadingState.PreparingToLoad; else Program.game.Loading = MainGame.LoadingState.NotLoading;
			if (Loading_Type == LoadingType.Block && Loading != LoadingState.NotLoading)
				{oldKeyboardState = keyboardState; return;}

			if (this.MapSet) this.Map.UpdateObjects();
			if (Program.game.CombatCountDown > 0) Program.game.CombatCountDown--;
			PAXCaster.UpdatePaxes();

			/* Controls */
			this.mainCamera.GamepadControls(Window);
			

			/* Camera */
			if (DebugCamera)
            {
                alphaTest.View = ArcBallCamera.mainCamera.ViewMatrix;
                alphaTest.Projection = ArcBallCamera.mainCamera.ProjectionMatrix;

                basicEffect.View = alphaTest.View;
                basicEffect.Projection = alphaTest.Projection;

                ArcBallCamera.MouseControls(Window);
            }
            else
            {
				if (this.mainCamera.Target == null)
					alphaTest.View = Matrix.CreateLookAt(this.camPos,this.lookAt,Vector3.Up);
				else
					alphaTest.View = this.mainCamera.ViewMatrix;

				alphaTest.Projection = this.mainCamera.ProjectionMatrix;
                basicEffect.View = alphaTest.View;
                basicEffect.Projection = alphaTest.Projection;
                this.mainCamera.Update(Window);
            }

			/* Update Animations and physics */
			foreach (Resource rs in ResourceFiles)
            {
                var mdl = rs as Model;
                #if !playAnims
                if (rs.Render && mdl != null)
                {
                    PerformIACharacter(mdl);
                    if (!mdl.Cutscene)
                    RuleCharacter(mdl);
                }
                #endif

                var mset = rs as Moveset;
                if (rs.Render && mset !=null && !mset.Cutscene && ticks >= 0)
                {
					mset.ComputeAnimation();
				}
			}

				if (Program.game.t != null)
					Program.game.t.Monitor(Program.game.mainCamera.Target, 80, 40, false, Vector3.Zero, Vector3.Zero, Vector3.Zero);
			


			oldKeyboardState = keyboardState;
            base.Update(gameTime);
        }


        RenderTarget2D renderTarget = null;

		public Trail t;

		protected override void Draw(GameTime gameTime)
		{
			if (ticksAlways == 0)
			{
				t = new Trail("W_EX010");

				/*DAE d = new DAE(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08\TT08.dae");
				d.Parse();
				return;*/
				if (File.Exists(@"Content\startup.txt"))
				{
					MainGame.transitionType = MainGame.TransitionType.FadeOutBlack;
					MainGame.TState = MainGame.TransitionState.PreparingToTransit;
					MainGame.Transition[0] = 20;
					MainGame.Transition[1] = 20;
					string[] startup_info = File.ReadAllLines(@"Content\startup.txt");
					string[] spli = startup_info[0].Split(',');
					if (spli.Length > 0)
					{
						for (int i = 0; i < spli.Length - 1; i++)
							SetMap(@"Content\Models\" + spli[i] + @"\" + spli[i] + ".mdl", true, false);
						SetMap(@"Content\Models\" + spli[spli.Length - 1] + @"\" + spli[spli.Length - 1] + ".mdl", false, false);
						this.Map.Render = true;

					}

					if (File.Exists(startup_info[1]))
					{
						Spawn.Load(startup_info[1]);
					}
				}
				ticksAlways = 1;
				Console.WriteLine("Game loaded in " + stp.Elapsed.Seconds +"s "+stp.Elapsed.Milliseconds +"ms");
			}
			Audio.UpdateAmbient();
			ticks++;
            ticksAlways++;



			for (int i = 0; i < ResourceFiles.Count; i++)
			{
				Model mdl = ResourceFiles[i] as Model;
				if (ResourceFiles[i].Render && mdl != null)
					mdl.GetShadow(graphics, basicEffect, alphaTest, rasterSolid, rasterSolidNoCull);
			}
			graphics.GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(backgroundColor);

			if (MainGame.TState == MainGame.TransitionState.PreparingToTransit || (Loading == LoadingState.PreparingToLoad && Loading_Type == LoadingType.Block))
			{
				if (renderTarget!=null)
                renderTarget.Dispose();
				renderTarget = new RenderTarget2D(
				GraphicsDevice,
				GraphicsDevice.PresentationParameters.BackBufferWidth,
				GraphicsDevice.PresentationParameters.BackBufferHeight,
				false,
				GraphicsDevice.PresentationParameters.BackBufferFormat,
				DepthFormat.Depth24Stencil8);
				GraphicsDevice.SetRenderTarget(renderTarget);
            }

            if (Loading == LoadingState.Loading && Loading_Type == LoadingType.Block)
            {
                DrawLoadingRenderTarget(gameTime);
                DebugMenu.Update();
                CharToScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                return;
			}

			if (FullScreenState !=0)
                return;
            
            if (ScenePlayer.ScenePlaying)
            for (int i = 0; i < ScenePlayer.Scenes.Count; i++)
            {
                ScenePlayer.Scenes[i].RenderNext();
			}
			if (MainGame.TState != MainGame.TransitionState.Transiting || MainGame.transitionType != TransitionType.FadeInBlackBlock)
			{
				for (int i = 0; i < ResourceFiles.Count; i++)
				{
					Model mdl = ResourceFiles[i] as Model;
					if (ResourceFiles[i].Render && mdl != null)
					{
						mdl.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
					}
				}
				if (this.MapSet)
				{
					MAPSpecies.ApplySpecies(this.Map);

					(this.Map.Links[0] as Collision).Draw(graphics, alphaTest, basicEffect, rasterSolid);

					this.Map.DrawObjects(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
					this.Map.DrawEfects(graphics, basicEffect, rasterSolid);

					for (int i = 0; i < cursors.Count; i++)
						cursors[i].Draw(graphics, basicEffect);

					TriangleStrip.Draw(graphics, basicEffect, rasterSolidNoCull);

					PAXCaster.DrawPaxes(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
					DebugMenu.Update();

					CharToScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
				}
			}
			if (t != null)
				t.Draw(graphics, basicEffect, rasterSolidNoCull);
			SrkFontDisplayer.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);


			for (int i = 0; i < ResourceFiles.Count; i++)
			{
				Model mdl = ResourceFiles[i] as Model;
				if (ResourceFiles[i].Render && mdl != null)
					mdl.DrawShadow(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
			}

			base.Draw(gameTime);

			if (Loading == LoadingState.PreparingToLoad)
			{
				if (Loading_Type == LoadingType.Block)
				{
					GraphicsDevice.SetRenderTarget(null);
					DrawLoadingRenderTarget(gameTime);
				}
				Loading = LoadingState.Loading;
			}

			if (Loading == LoadingState.Loading && Loading_Type == LoadingType.Normal)
			{
				LoadingScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
			}

			if (MainGame.TState == MainGame.TransitionState.PreparingToTransit)
			{
				GraphicsDevice.SetRenderTarget(null);
				MainGame.TState = MainGame.TransitionState.Transiting;
				if (MainGame.transitionType == TransitionType.FadeNormal)
				DrawLoadingRenderTarget(gameTime);
			}

			if (MainGame.TState == MainGame.TransitionState.Transiting)
				DrawTransit();
		}


		public void PerformIACharacter(Model model)
		{
			Model target = this.mainCamera.Target;

			if (target == null)
				return;

			if (target == model)
				return;

			if (model.Links.Count == 0)
				return;
			if (model.Master == null && target != model)
                model.DestOpacity = 1;
			var mset = (model.Links[0] as Moveset);
			Vector3 requestedLoc = model.Location;
			double angle = model.DestRotate - MainGame.PI / 2f;
			Vector3 start = requestedLoc;
			Vector3 end = requestedLoc;
			Vector3 diff_ = Vector3.Zero;
			bool travers = false;

			bool setLowestFloor = false;
			int pindex = 0;
			if (this.Partner1 != null && model.ResourceIndex == this.Partner1.ResourceIndex)
				pindex = 1;
			if (this.Partner2 != null && model.ResourceIndex == this.Partner2.ResourceIndex)
				pindex = 2;

			if (pindex == 1 ||
				pindex == 2)
			{
				end = target.Location;

				if (pindex == 1)
				end += Vector3.Transform(new Vector3(100,0,0),Matrix.CreateRotationY(target.Rotate));
				if (pindex == 2)
					end += Vector3.Transform(new Vector3(-100, 0, 0), Matrix.CreateRotationY(target.Rotate));

				angle = model.DestRotate - MainGame.PI / 2f;

				if (this.MapSet && !ScenePlayer.ScenePlaying)
				{
					Vector3 oriPoint = 1f * requestedLoc;
                    Vector3 targetPoint = 1f * target.Location;

                    float oriHeight = (model.MinVertex.Y + model.MaxVertex.Y) / 1f;
                    float targetHeight = (target.MinVertex.Y + target.MaxVertex.Y) / 1f;

                    oriPoint.Y += oriHeight;
                    targetPoint.Y += targetHeight;


					float dist = Vector3Distance2D(oriPoint, targetPoint);
					float angl = MainGame.PI * 1.5f - (float)Math.Atan2((oriPoint.Z - targetPoint.Z) / dist, (oriPoint.X - targetPoint.X) / dist);

					SrkBinary.MakePrincipal(ref angl);
					angl = Math.Abs(angl - mainCamera.PrincipalYaw);
					if (dist > 1000 && angl > 1.9f && angl < 4.1f && mainCamera.Pitch>-0.7)
					{
						for (int i = 0; i < target.PathHistory.Count; i++)
						{
							Vector3 point = 1f * target.PathHistory[i];
							point.Y += oriHeight;
							dist = Vector3Distance2D(point, targetPoint);
							if (dist > 800 && dist < 900)
							{
								dist = Vector3Distance2D(target.PathHistory[i], targetPoint);
								angl = MainGame.PI * 1.5f - (float)Math.Atan2((target.PathHistory[i].Z - targetPoint.Z) / dist, (target.PathHistory[i].X - targetPoint.X) / dist);

								SrkBinary.MakePrincipal(ref angl);
								angl = Math.Abs(angl - mainCamera.PrincipalYaw);
								if (angl > 2.3f && angl < 3.7f)
								{
									setLowestFloor = true;
									requestedLoc = target.PathHistory[i];
									break;
								}
							}
						}
					}
					
					Vector3 intersect = (this.Map.Links[0] as Collision).HasCol(oriPoint, targetPoint);
					if (!Single.IsNaN(intersect.X))
					{
						travers = true;
						if (model.iaBlockedCount > 100)
						{
							model.Goto = 1f * requestedLoc;
						}
						else
						if (!Single.IsNaN(model.Goto.X))
						{
							if (Vector2.Distance(model.Speed2D,Vector2.Zero) < model.RunSpeed*0.1f && model.cState != Model.ControlState.Idle)
							{
								model.iaBlockedCount++;
								if (model.iaBlockedCount > 100)
								{
									model.JumpPress = false;
									if (mset.sora_doko_ > -1 && requestedLoc.Y < model.LowestFloor + model.StairHeight)
									{
										model.SmoothJoystick = 0;
										model.Joystick = 0;
										model.DestRotate += (float)Math.PI;
										mset.PlayingIndex = mset.sora_doko_;
										model.cState = Model.ControlState.Chat;
										return;
									}
								}
							}
							else
							{
								model.iaBlockedCount = 0;
							}
							if (model.iaBlockedCount > 10)
							{
								if (model.cState == Model.ControlState.Jump)
									model.JumpPress = Math.Abs(model.JumpStart - requestedLoc.Y) < Math.Abs(model.JumpStart - model.Goto.Y) - model.StairHeight;

								if (requestedLoc.Y < model.LowestFloor + model.StairHeight && model.Goto.Y > requestedLoc.Y + model.StairHeight)
								{
									if (model.cState != Model.ControlState.Jump)
									{
										model.JumpPress = true;
										model.JumpCancel = false;
										model.JumpCollisionCancel = false;
										if (model.JumpDelay < 0)
											model.JumpDelay = model.cState == Model.ControlState.Land ? 6 : 0;
										model.JumpStart = model.LowestFloor;
									}
								}
							}

							if (Vector3Distance2D(requestedLoc, model.Goto) < model.RunSpeed * 0.1f)
							{
								model.Goto.X = Single.NaN;
							}

						}
						else
						{
							float closest = Single.MaxValue;
							for (int i = 0; i < target.PathHistory.Count; i++)
							{
								Vector3 point = 1f * target.PathHistory[i];
								point.Y += oriHeight;


								dist = Vector3Distance2D(point, targetPoint);
								intersect = (this.Map.Links[0] as Collision).HasCol(oriPoint, point);
								if (Single.IsNaN(intersect.X) && dist < closest)
								{
									if (Vector3Distance2D(oriPoint, point)  > model.RunSpeed * 0.11f)
									{
										closest = dist;
										model.Goto = 1f * target.PathHistory[i];
									}
								}
							}
							if (closest > Single.MaxValue / 2f)
							{
								model.iaBlockedCount = 101;
							}
						}

					}
					else if (Math.Abs(start.Y - end.Y) < 700)
					{
						if (mset.playingIndex == mset.sora_doko_)
						{
							mset.PlayingIndex = mset.idle_;
							model.cState = Model.ControlState.Idle;
						}
						model.iaBlockedCount = 0;
						model.Goto.X = Single.NaN;
					}


                    /* model.SmoothJoystick = 1f;
                    point.Y -= target.HeadHeight.Y * 2f;
                    model.Goto = ((point - requestedLoc) * 1.3f) + requestedLoc;
                    cursors[2].Position = model.Goto; */
                }

				bool force = false;
				if (!Single.IsNaN(model.Goto.X))
				{
					force = ScenePlayer.ScenePlaying && Vector3Distance2D(requestedLoc, model.Goto) > model.WalkSpeed * 0.25f;
					end = model.Goto * 1f;
					model.TurnStep = 0.1f;
				}
				else
				{
					model.TurnStep = 0.2f;
				}


				if (force || Math.Abs(start.Y - end.Y) < 700)
                {
                    start.Y = 0;
                    end.Y = 0;
                    diff_ = end - start;
					float hypo = Vector3.Distance(start, end);
					
                    if (travers || hypo > 300)
                    {
                        model.SmoothJoystick += (1f - model.SmoothJoystick) / 60f;
                        angle = (float)(Math.Atan2(diff_.X / hypo, diff_.Z / hypo) - MainGame.PI / 2);

                    }
                    else if (force || hypo > 170)
                    {
                        float slow = this.Player.Speed < 30 ? 0.35f : 1f;
                        model.SmoothJoystick += (slow - model.SmoothJoystick) / 60f;
                        angle = (float)(Math.Atan2(diff_.X / hypo, diff_.Z / hypo) - MainGame.PI / 2);
                    }
                    else if (this.Player.Speed < 50)
                    {
                        model.SmoothJoystick *= 0.9f;
                    }
                }
                else 
                {
                    model.SmoothJoystick *= 0.9f;
					if (this.MapSet && !ScenePlayer.ScenePlaying)
						if (mset.sora_doko_ > -1 && model.cState != Model.ControlState.Chat)
					{
						mset.PlayingIndex = mset.sora_doko_;
						model.cState = Model.ControlState.Chat;
						model.SmoothJoystick = 0f;
					}
				}
			}
			start.Y = 0;

			if (model.NPC)
			{
				if (BulleSpeecher.CurrentBulle != null && BulleSpeecher.CurrentBulle.displayType != Bulle.DisplayType.None)
				{
					if (BulleSpeecher.bulleEmmiters[BulleSpeecher.bulles.IndexOf(BulleSpeecher.CurrentBulle)] == model)
					{
						model.SmoothJoystick = 0f;
						model.Skeleton.NeckBoneSet = false;
						model.Rotate = model.DestRotate;
					}
				}
				else
				{
					if (mset.PlayingIndex != mset.idle && mset.PlayingIndex != mset.walk && mset.PlayingIndex != mset.run)
					{
						mset.PlayingIndex = mset.idle;
					}
					float dist = Vector3Distance2D(model.Location, target.Location);
					bool reach = false;
					bool closeSora = dist < 100 + model.Epaisseur + target.Epaisseur;
					bool veryCloseSora = dist < 50 + model.Epaisseur + target.Epaisseur;

					if (target.Speed > 1 && model.NPCNewReach == -1)
					{
						model.NPCNewReach = 0;
					}

						Vector3 targetHead = target.Location + target.HeadHeight;
						Vector3 modelNeck = model.Location + model.GetGlobalBone(model.Skeleton.NeckBone,Vector3.Zero);

						end = target.Location * 1f;
						end.Y = 0;
						diff_ = end - start;
						float hypo = Vector3.Distance(start, end);
						angle = Math.Atan2(diff_.X / hypo, diff_.Z / hypo);
						if (Math.Abs(model.PrincipalRotate - (float)angle) < 1)
						{
							float dest = ((float)(modelNeck.Y - targetHead.Y) / 250f);
							if (dest < -0.6f)
								dest = -0.6f;
							if (model.Skeleton.NeckBoneSet)
							{
								if (Math.Abs(model.Skeleton.NeckBoneDest - dest) > 0.02f)
								{
									if (model.Skeleton.NeckBoneDest < dest)
										model.Skeleton.NeckBoneDest += 0.02f;
									if (model.Skeleton.NeckBoneDest > dest)
										model.Skeleton.NeckBoneDest -= 0.02f;
								}
							}
							else
								model.Skeleton.NeckBoneDest = 0f;
							model.Skeleton.NeckBoneSet = true;
						}
						else
						{
							if (model.Skeleton.NeckBoneDest > 0)
								model.Skeleton.NeckBoneDest *= 0.98f;
							else
								model.Skeleton.NeckBoneSet = false;
						}
						

					if (/*target.Speed < 0.1 &&*/ model.NPCNewReach >= 0 && closeSora)
					{
						reach = true;
						if (veryCloseSora || !model.NPCReached)
						{
							model.NPCNewReach = -500;
							model.Goto.X = Single.NaN;
							end = target.Location * 1f;
							end.Y = 0;
							diff_ = end - start;
							hypo = Vector3.Distance(start, end);
							angle = Math.Atan2(diff_.X / hypo, diff_.Z / hypo);

							model.DestRotate = (float)angle;
						}
						model.SmoothJoystick = 0f;
					}
					else
					{
						if (Single.IsNaN(model.Goto.X))
						{
							Vector3 add = new Vector3(rnd.Next(-500, 500), model.LowestFloor, rnd.Next(-500, 500));
							model.Goto = model.SpawnedLocation + add;
							if (this.MapSet && this.Map.Links.Count > 0)
							{
								Vector3 inter = (this.Map.Links[0] as Collision).HasCol(model.SpawnedLocation + new Vector3(0, model.StairHeight, 0), model.Goto + new Vector3(0, model.StairHeight, 0));
								if (!Single.IsNaN(inter.X))
								model.Goto.X = Single.NaN;// model.Location;
								else
									model.Goto = model.SpawnedLocation + add*0.95f;
							}
							if (!Single.IsNaN(model.Goto.X) && Vector3.Distance(model.Goto, model.Location)< model.Epaisseur * 4f)
								model.Goto.X = Single.NaN;

							model.SmoothJoystick = 0f;
						}
						else
						{
							end = model.Goto * 1f;
							end.Y = 0;
							diff_ = end - start;

							dist = Vector3.Distance(start, end);
							if (dist > model.Epaisseur * 2f)
							{
								if (model.NPCNewReach > 0)
								{
									model.NPCNewReach--;
								}
								else
								if (model.NPCNewReach < -1)
								{
									model.NPCNewReach++;
								}
								else
								{
									model.SmoothJoystick = 0.5f;
									angle = Math.Atan2(diff_.X / dist, diff_.Z / dist) - MainGame.PI / 2;
								}
							}
							else
							{
								model.SmoothJoystick = 0f;
								if (!model.NPCReached)
								{
									model.Goto.X = Single.NaN;
									model.NPCNewReach = 250;
								}
							}
						}

					}
					model.NPCReached = reach;
				}
			}

			if (model.SmoothJoystick > 0.3f)
			{
				float step = model.SmoothJoystick < 0.75 ? model.WalkSpeed : model.RunSpeed;

				if (model.cState != Model.ControlState.Land || model.pState == Model.State.Gravity_Slide)
					if (model.cState != Model.ControlState.Guarding)
						if (model.cState != Model.ControlState.UnGuarding)
							if (model.pState != Model.State.BlockAll || model.pState == Model.State.NoIdleWalkRun_ButRotate)
							{
								model.DestRotate = (float)(angle + Math.PI / 2);
								if (model.pState != Model.State.BlockAll || model.pState == Model.State.NoIdleWalkRun_ButRotate)
								{
									requestedLoc.X += (float)((1 / 60f) * (step) * Math.Sin((float)(angle + Math.PI / 2f)));
									requestedLoc.Z += (float)((1 / 60f) * (step) * Math.Cos((float)(angle + Math.PI / 2f)));
									
									KHDebug.Collision.MonitorCollision(model, ref requestedLoc);
								}
							}
			}
			if (setLowestFloor)
				requestedLoc.Y = model.lowestFloor;
			model.Location = requestedLoc;
		}

        public void RuleCharacter(Model model)
		{
			if (model.Links.Count == 0 || model.HasMaster)
                return;
            var modelMoveset = (model.Links[0] as Moveset);
            if (modelMoveset == null)
                return;


			if (model.Keyblade != null)
				if (Program.game.CombatCountDown > 0)
					model.Keyblade.DestOpacity = 1;
				else
					model.Keyblade.DestOpacity = 0;

			Vector3 requestedLoc = model.Location;

            float playingFrame = modelMoveset.PlayingFrame;


			bool skate = model.Links[0].ResourceIndex == 39;

			if (model.Speed>20)
                model.CliffCancel = false;


			if (model.pState != Model.State.Gravity_Slide && 
				model.SmoothJoystick > 0.3 && 
                    ((model.cState == Model.ControlState.Land && playingFrame > 15) ||
					(model.cState == Model.ControlState.Roll && playingFrame > 33) ||
					(model.cState == Model.ControlState.Guard && playingFrame > 50)))
            {
				model.CliffCancel = false;
				modelMoveset.NextPlayingIndex = -1;
				model.pState = Model.State.GoToAtMove;
				model.cState = Model.ControlState.Idle;
				modelMoveset.PlayingIndex = modelMoveset.run;
				modelMoveset.InterpolateFrameRate = 6;
				modelMoveset.InterpolateAnimation = true;
			}

            float diffLowestFloor = model.LowestFloor - requestedLoc.Y;
            bool flyOk = model.Fly && model.Location.Y > model.LowestFloor + model.StairHeight;
            


            if (flyOk)
            {
                if (model.cState != Model.ControlState.Fly)
                {
                    modelMoveset.AtmospherePlayingIndex = modelMoveset.flyIdle_;
                    modelMoveset.PlayingIndex = modelMoveset.flyIdle_;
                }
                model.cState = Model.ControlState.Fly;
                modelMoveset.AtmospherePlayingIndex = modelMoveset.flyIdle_;
            }
            else if (!model.Fly && model.cState == Model.ControlState.Fly)
            {
                modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
                modelMoveset.PlayingIndex = modelMoveset.fall_;
                model.cState = Model.ControlState.Fall;
            }

            if (model.JumpDelay > -1)
                model.JumpDelay ++;
            if (model.cState == Model.ControlState.UnCliff)
            {
                modelMoveset.NextPlayingIndex = modelMoveset.fall_;
                modelMoveset.AtmospherePlayingIndex = modelMoveset.cliffExit_;
                if (modelMoveset.PlayingIndex != modelMoveset.cliffExit_)
                {
					model.CurrentGravity2D *= 3f;
					model.CurrentGravityY = 5f;
                    model.cState = Model.ControlState.Fall;
                    //cursors[1].Position = model.Location + Vector3.Transform(model.Skeleton.Bones[0].GlobalMatrix.Translation, Matrix.CreateRotationY(model.DestRotate));
                }
                else
                {
                    //cursors[0].Position = model.Location + Vector3.Transform(model.Skeleton.Bones[0].GlobalMatrix.Translation, Matrix.CreateRotationY(model.DestRotate));
                }
            }
            else if (model.cState == Model.ControlState.Cliff)
            {
                modelMoveset.InterpolateFrameRate =5;
                modelMoveset.AtmospherePlayingIndex = modelMoveset.cliff_;
                modelMoveset.NextPlayingIndex = -1;
                model.pState = Model.State.BlockAll;
                if (modelMoveset.PlayingIndex == modelMoveset.cliff_)
                {
                    if (playingFrame > 10)
                    {
                        if (model.SmoothJoystick > 0.6)
                        {
                            modelMoveset.PlayingIndex = modelMoveset.cliffExit_;
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.cliffExit_;
                            model.cState = Model.ControlState.UnCliff;
                        }
                    }
                }
            }
            else 
            if (!flyOk && model.cState != Model.ControlState.Fly)
            {
                if (model.cState == Model.ControlState.Jump)
                {
                    if (model.JumpCollisionCancel)
                    {
                        model.JumpDelay = -1;
                        modelMoveset.PlayingIndex = modelMoveset.fall_;
                        modelMoveset.InterpolateFrameRate = 15;
                        model.cState = Model.ControlState.Fall;
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
                        model.JumpCancel = false;
                        model.JumpCollisionCancel = false;
                    }
                    else
                    {
                        float push = (float)Math.Abs(model.CurrentGravityY) * (60f / 480f);
                        model.CurrentGravityY -= (push);
                        if (model.CurrentGravityY < 0.00001)
                            model.CurrentGravityY = 0.00001f;

                        /*if (model.CurrentGravity < 0.1f)
                            model.CurrentGravity = 0.1f;*/

                    if (((model.JumpCancel || !model.JumpPress) && requestedLoc.Y < model.JumpStart + model.JumpMin))
                        {
                            model.CurrentGravityY *= 0.0001f;
                            model.JumpCancel = true;
                            model.JumpDelay = -1;
                            requestedLoc.Y += ((model.JumpStart + model.JumpMin) - requestedLoc.Y) / 6f;


                            if (playingFrame > 12)
                                if (requestedLoc.Y > model.JumpStart + model.JumpMin * 0.9f)
								{
									model.CurrentGravityY = 0f;
                                    modelMoveset.PlayingIndex = modelMoveset.fall_;
                                    modelMoveset.InterpolateFrameRate = 15;
                                    model.cState = Model.ControlState.Fall;
                                    modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
                                    model.JumpCancel = false;
                                    model.JumpCollisionCancel = false;
                                }

                        }
                        else
                        if (!model.JumpPress || requestedLoc.Y > (model.JumpStart + model.JumpMax * 0.98f))
                        {
                            if (playingFrame > 12)
							{
								model.CurrentGravityY = 0f;
								model.JumpDelay = -1;
                                modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
                                modelMoveset.PlayingIndex = modelMoveset.fall_;
                                modelMoveset.InterpolateFrameRate = 15;
                                model.cState = Model.ControlState.Fall;
                            }
                        }
                        else
                        if (modelMoveset.PlayingIndex == modelMoveset.jump_ && playingFrame > 5)
                        {
                            requestedLoc.Y += ((model.JumpStart + model.JumpMax) - requestedLoc.Y) / (15f * 60f / 80f);
                        }
                    }
                }
                else
                {

                    if (Math.Abs(requestedLoc.Y - model.LowestFloor) > model.StairHeight*1.5f)
					{
						if (Single.IsNaN(model.LastLand.X))
						{
							modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
							modelMoveset.PlayingIndex = modelMoveset.fall_;
							model.cState = Model.ControlState.Fall;
						}
                    }
                    else if (model.cState != Model.ControlState.Fall)
                    {
                        if (!skate && (int)model.cState <3)
                        requestedLoc.Y += (model.LowestFloor - requestedLoc.Y) * (0.033f + (model.SmoothJoystick / 4f));
                        else
                            requestedLoc.Y += (model.LowestFloor - requestedLoc.Y) * 0.85f;
                        bool JumpFast = (modelMoveset.PlayingIndex == modelMoveset.land_ && playingFrame > 10 && model.JumpPress);

                        if (model.cState != Model.ControlState.Land && Math.Abs(requestedLoc.Y - model.LowestFloor) > 0 && modelMoveset.PlayingIndex == modelMoveset.fall_)
                        {
                            model.cState = Model.ControlState.Fall;
                        }
                        if (model.pState == Model.State.GoToAtMove || JumpFast)
                        {
                            if ((!JumpFast && model.JumpDelay > -1) || model.JumpDelay > 5f)
                            {
                                model.oldJumpPress = true;
                                model.cState = Model.ControlState.Jump;
                                modelMoveset.PlayingIndex = modelMoveset.jump_;

                                modelMoveset.AtmospherePlayingIndex = modelMoveset.jump_; /* <<----- a verifier*/
                            }
                        }
                    }
                }
                if (!model.NPC && model.cState == Model.ControlState.Roll)
                {
                    if (modelMoveset.PlayingIndex == modelMoveset.roll_)
                    {
                        if (CombatCountDown > 0)
                            modelMoveset.NextPlayingIndex = modelMoveset.guarding_;
                        else
                            modelMoveset.NextPlayingIndex = modelMoveset.idle_;
                    }
                    else
                    {
                        if (CombatCountDown > 0)
                            model.cState = Model.ControlState.Guarding;
                        else
                            model.cState = Model.ControlState.Idle;
                    }
                }
            }
            if (!Single.IsNaN(model.LastLand.X))
			{
				if (Vector3.Distance(model.Location, model.LastLand)>=10f)
				{
					model.LastLand.X = Single.NaN;
				}
			}
            //Console.WriteLine(modelMoveset.interpolateAnimation);
            if (model.cState == Model.ControlState.Fall)
            {
                if (modelMoveset.PlayingIndex == modelMoveset.fall2_)
                    modelMoveset.land_ = modelMoveset.land2_;
                else
                    modelMoveset.land_ = modelMoveset.land1_;

                if (modelMoveset.ComputingFrame > modelMoveset.InterpolateFrameRate)
                    modelMoveset.InterpolateFrameRate = 6;

                model.JumpDelay = -1;
				if (skate)
				{
					if ((modelMoveset.PlayingIndex == modelMoveset.chat3_ || modelMoveset.PlayingIndex == modelMoveset.chat5_) && modelMoveset.PlayingFrame<20)
						model.Gravity = -8f;
					else
					if (modelMoveset.PlayingIndex == modelMoveset.chat4_ && modelMoveset.PlayingFrame < 20)
						model.Gravity = 8f;
					else
						model.Gravity = 16f;
					if (modelMoveset.PlayingFrame >= modelMoveset.MaxTick-1)
					{
						modelMoveset.fall_ = modelMoveset.fall1_;
					}
				}

				if (model.pState != Model.State.BlockAll || model.pState == Model.State.GravityOnly)
                {
                    model.CurrentGravityY += (model.Gravity - model.CurrentGravityY) * 0.04f;
                    requestedLoc.Y -= model.CurrentGravityY;
                }
                if (requestedLoc.Y <= model.LowestFloor)
				{
					modelMoveset.PlayingIndex = modelMoveset.land_;
                    model.CliffCancel = false;
                    requestedLoc.Y = model.LowestFloor;
					model.LastLand = requestedLoc;
					model.cState = Model.ControlState.Land;
                }
            }
            else
            if (model.cState == Model.ControlState.Fly)
            {
                if (model.JumpDelay>-1)
                model.TurnStep = 0.1f;
                model.JumpDelay = -1;
                model.JumpCancel = false;
                model.JumpCollisionCancel = false;

                model.Gravity = 16f;
                model.CurrentGravityY = 1f;

                if (requestedLoc.Y > model.LowestFloor + model.StairHeight)
                if (model.pState != Model.State.BlockAll || model.pState != Model.State.GravityOnly)
                {
                    requestedLoc.Y--;
                }
                if (requestedLoc.Y < model.LowestFloor + 10f)
                {
                    modelMoveset.PlayingIndex = modelMoveset.land_;
                    model.cState = Model.ControlState.Land;
                }
                if (requestedLoc.Y <= model.LowestFloor)
                {
                    requestedLoc.Y = model.LowestFloor;
                    modelMoveset.PlayingIndex = modelMoveset.land_;
                    model.cState = Model.ControlState.Land;
                }
                /*if (requestedLoc.Y < model.LowestFloor + 10f)
                {
                    modelMoveset.PlayingIndex = modelMoveset.land_;
                }
                if (requestedLoc.Y <= model.LowestFloor)
                {
                    requestedLoc.Y = model.LowestFloor;
                    //modelMoveset.PlayingIndex = modelMoveset.land_;
                    model.cState = Model.ControlState.Land;
                }*/

                if (model.Joystick > 0.3f)
                {
                    modelMoveset.AtmospherePlayingIndex = modelMoveset.fly_;
                }
                else
                {
                    modelMoveset.AtmospherePlayingIndex = modelMoveset.flyIdle_;
                }
            }
			if (skate) /*skate*/
			{
				model.Keyblade.DestOpacity = 0;
				model.Keyblade.Opacity = 0;

				if (modelMoveset.PlayingIndex == modelMoveset.walk && model.SkateCount == 0 && model.Speed < 5 && modelMoveset.PlayingFrame < 5)
				{
					modelMoveset.PlayingFrame = 5;
				}


				/*if (model.Speed < 10)
				{
					model.CurrentGravity2D.X = 0;
					model.CurrentGravity2D.Y = 0;
				}*/


				model.TurnStep = 0.15f;


				if (model.Joystick > 0.3)
				{
					Vector2 diff2D = ((model.Speed2D) - model.CurrentGravity2D);
					model.CurrentGravity2D += diff2D / 25f;
					if (model.SkateCount == 0)
					{
						int ind = Audio.names.IndexOf(@"Content\Effects\Audio\Sounds\F_TT010\skate_launch.wav");
							Audio.Play(@"Content\Effects\Audio\Sounds\F_TT010\skate_launch.wav", false, model, 50);
					}
					if (model.SkateCount == 0 || model.Speed > 50)
					{
						model.SkateCount++;
					}


				}
				else
				{
					model.SkateCount = 0;
					model.CurrentGravity2D *= 0.99f;
				}


				if (model.Speed < 50)
				{
					if (model.Joystick < 0.3)
						model.SmoothJoystick = 0f;
					else
					{
						if (model.SmoothJoystick < 0.3f)
							model.SmoothJoystick = 0.3f;
					}
				}

				if (model.cState != Model.ControlState.Run && model.cState != Model.ControlState.Land)
				{
					int ind = Audio.names.IndexOf(@"Content\Effects\Audio\Sounds\F_TT010\skate_run.wav");
					if (ind > -1)
					{
						Audio.effectInstances[ind].Volume += (0- Audio.effectInstances[ind].Volume)/10f;
					}
				}

				if (model.cState == Model.ControlState.Run || model.cState == Model.ControlState.Land)
				{
					int ind = Audio.names.IndexOf(@"Content\Effects\Audio\Sounds\F_TT010\skate_run.wav");
					if (ind == -1)
						Audio.Play(@"Content\Effects\Audio\Sounds\F_TT010\skate_run.wav", true, model, 0);
					else
					{
						if (Audio.effectInstances[ind].State != Microsoft.Xna.Framework.Audio.SoundState.Playing)
						{
							Audio.effectInstances[ind].Play();
						}
						if (model.cState == Model.ControlState.Land && Vector2.Distance(Vector2.Zero, model.Speed2D) < 30)
						{
							Audio.effectInstances[ind].Volume += (0 - Audio.effectInstances[ind].Volume) / 10f;
						}
						else
						{
							Audio.effectInstances[ind].Volume += ((50f / 400f) - Audio.effectInstances[ind].Volume) / 10f;
						}
					}
				}

				if (model.SkateCount < 80 && model.SmoothJoystick > 0.7f)
				{
					model.SmoothJoystick = 0.7f;
				}
				else
				if (model.cState == Model.ControlState.Run)
				{
					model.SmoothJoystick = 1f;
					if (model.Joystick < 0.3)
					{
						requestedLoc += new Vector3(model.CurrentGravity2D.X, 0, model.CurrentGravity2D.Y) * 0.15f;
						if (model.Speed < 10)
							model.SmoothJoystick = Vector2.Distance(Vector2.Zero, model.CurrentGravity2D / 20f);
						else
							model.SmoothJoystick = Vector2.Distance(Vector2.Zero, model.CurrentGravity2D);
					}
					else if (model.Speed < 5)
					{
						model.SkateCount = 0;
						model.SmoothJoystick = 0;
					}
				}




				if (model.pState == Model.State.Gravity_Slide)
				{
					float multip = model.Joystick>0.3f ? 0.025f : 0.1f;
					requestedLoc += new Vector3(model.CurrentGravity2D.X, 0, model.CurrentGravity2D.Y) * multip;
				}
			}
			else
			{
				model.SkateCount = 0;
				if (model.cState == Model.ControlState.Land)
					model.CurrentGravity2D *= 0.9f;
				else if (modelMoveset.PlayingIndex == modelMoveset.flyIdle_)
					model.CurrentGravity2D *= 0.99f;
				else
				{
					Vector2 diff2D = ((model.Speed2D) - model.CurrentGravity2D);
					model.CurrentGravity2D += diff2D / 25f;
				}


				if (model.pState == Model.State.Gravity_Slide ||
					model.cState == Model.ControlState.Fly ||
					model.cState == Model.ControlState.Fall)
				{
					float multip = model.cState == Model.ControlState.Fly ? 0.1f : 0.0666f;
					requestedLoc += new Vector3(model.CurrentGravity2D.X, 0, model.CurrentGravity2D.Y) * multip;
				}
			}

			if (model.pState != Model.State.NoIdleWalkRun 
                && model.cState != Model.ControlState.Jump 
                && model.cState != Model.ControlState.Fly
                && model.cState != Model.ControlState.Cliff)
			{
				if (model.NPC && (Math.Abs(Math.Cos(model.Rotate) - Math.Cos(model.DestRotate)) > 0.1f ||
						Math.Abs(Math.Sin(model.Rotate) - Math.Sin(model.DestRotate)) > 0.1f))
				{
					modelMoveset.AtmospherePlayingIndex = modelMoveset.walk;
				}
				else
				if (model.SmoothJoystick < 0.3)
                {
                    if (!model.NPC && Program.game.CombatCountDown > 0)
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.idleFight_;
                    else
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.idle;
                }
                else
				{
					if (model.SmoothJoystick < 0.75f)
                    {
                        if (!model.NPC && (Program.game.CombatCountDown > 0 || (model.Keyblade != null && model.Keyblade.DestOpacity > 0)))
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.walkFight_;
                        else
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.walk;
                    }
                    else
                    {
                        if (!model.NPC && (Program.game.CombatCountDown > 0 || (model.Keyblade != null && model.Keyblade.DestOpacity > 0)))
							modelMoveset.AtmospherePlayingIndex = modelMoveset.runFight_;
                        else
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.run;
                    }
                }

			}

			KHDebug.Collision.MonitorCollision(model, ref requestedLoc);

            model.Location = requestedLoc;

            if (model.cState == Model.ControlState.Idle)
            {
                model.InactiveCount++;
                if (modelMoveset.idle == modelMoveset.idleRest2_ && modelMoveset.PlayingFrame > modelMoveset.MaxTick-2)
                {
                    modelMoveset.idle = modelMoveset.idleRest1_;
                }
                
                if (modelMoveset.idleRest2_ > -1 && model.InactiveCount > 3000)
                {
                    modelMoveset.idle = modelMoveset.idleRest2_;
                    modelMoveset.NextPlayingIndex = modelMoveset.idleRest1_;
                    model.InactiveCount = 0;
                }


                if (!model.NPC && modelMoveset.guarding_ > -1 && modelMoveset.PlayingIndex == modelMoveset.idle_ &&  Program.game.CombatCountDown>0)
                {
                    modelMoveset.PlayingIndex = modelMoveset.guarding_;
                    model.cState = Model.ControlState.Guarding;
                    //model.pState = Model.State.GotoAtMove_WaitFinish;
                }
            }
            else
            {
                if (modelMoveset.PlayingIndex != modelMoveset.idleRest2_)
                {
                    modelMoveset.idle = modelMoveset.idleRest1_;
                    model.InactiveCount = 0;
                }
            }

            if (!model.NPC && Program.game.CombatCountDown == 1 && modelMoveset.unguarding_ >-1)
            {
                if (model.cState == Model.ControlState.Idle || model.cState == Model.ControlState.Guarding)
                {
                    modelMoveset.PlayingIndex = modelMoveset.unguarding_;
                    model.cState = Model.ControlState.UnGuarding;
                    //model.pState = Model.State.GotoAtMove_WaitFinish;
                }
            }


            bool c1 = (model.cState == Model.ControlState.Land && modelMoveset.PlayingIndex != modelMoveset.land_);
            bool c2 = (model.cState == Model.ControlState.Guarding && modelMoveset.PlayingIndex != modelMoveset.guarding_);
            bool c3 = (model.cState == Model.ControlState.UnGuarding && modelMoveset.PlayingIndex != modelMoveset.unguarding_);

            if (c1 || c2 || c3)
            {
                model.cState = Model.ControlState.Idle;
            }
            if (!model.NPC && c1 && modelMoveset.guarding_ > -1)
            {

                if (Program.game.CombatCountDown > 0)
                {
                    modelMoveset.PlayingIndex = modelMoveset.guarding_;
                    model.cState = Model.ControlState.Guarding;
                }
            }
            
            if (model.cState == Model.ControlState.Cliff || model.cState == Model.ControlState.Fly)
            {
                modelMoveset.PlayingIndex = modelMoveset.AtmospherePlayingIndex;
            }
            else
            if (model.pState == Model.State.GoToAtMove)
            {
                if (modelMoveset.PlayingIndex!=modelMoveset.AtmospherePlayingIndex)
                model.TurnStep = 0.2f;
                modelMoveset.PlayingIndex = modelMoveset.AtmospherePlayingIndex;
                if (Math.Abs(requestedLoc.Y - model.LowestFloor) > model.StairHeight)
                {

                }
                else
                {
					/*if (!model.NPC && !skate)
                    modelMoveset.InterpolateFrameRate = 6;*/
                    model.CurrentGravityY += ((model.Gravity * 0.3f) - model.CurrentGravityY) / 10f;
                    if (model.SmoothJoystick < 0.3)
                    {
                        model.cState = Model.ControlState.Idle;
                    }
                    else
                    {
                        if (model.SmoothJoystick < 0.75f)
                        {
                            model.cState = Model.ControlState.Walk;
                        }
                        else
                        {
                            model.cState = Model.ControlState.Run;
                        }
                    }
                }
            }
        }
    }
}
