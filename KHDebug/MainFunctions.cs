using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
//using kenuno;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KHDebug
{
	public partial class MainGame : Game
	{
		public bool DebugCamera = false;

		public LoadingState Loading = LoadingState.NotLoading;
		public LoadingType Loading_Type = LoadingType.Block;

		public static int[] Transition = new int[2];
		public static int[] Transition_rc = new int[2];
		public static TransitionState TState = TransitionState.NotTransiting;
		public static VertexPositionColorTexture[] vertexPositionColorTextures = new VertexPositionColorTexture[]
		{
			new VertexPositionColorTexture(new Vector3(-50f,0,-50f),Color.White,new Vector2(0,0)),
			new VertexPositionColorTexture(new Vector3(50f,0,50f),Color.White,new Vector2(1,1)),
			new VertexPositionColorTexture(new Vector3(50f,0,-50f),Color.White,new Vector2(1,0)),

			new VertexPositionColorTexture(new Vector3(-50f,0,-50f),Color.White,new Vector2(0,0)),
			new VertexPositionColorTexture(new Vector3(-50f,0,50f),Color.White,new Vector2(0,1)),
			new VertexPositionColorTexture(new Vector3(50f,0,50f),Color.White,new Vector2(1,1))
		};

		public static Matrix ViewMatrixTransition = Matrix.CreateLookAt(new Vector3(0, 1f, 0.0001f), Vector3.Zero, Vector3.Up);
		public static Matrix ProjectionMatrixTransition = Matrix.CreateOrthographic(100f, 100f, 0f, 100f);


		/*public static void StartTransition()
		{
			Transition[0] = longueur;
			Transition[1] = longueur;
		}*/

		public static int triangleType = 0;


		public static string[] TriangleType = new string[]
		{
			"Triangle-Normal",
			"Triangle-Notice"
		};

		public static TransitionType transitionType = TransitionType.None;
		public static TransitionType transitionType_rc = TransitionType.None;

		public enum TransitionType
		{
			None = 0,
			FadeNormal = 1,
			FadeInBlack = 2,
			FadeOutBlack = 3,
			FadeInBlackBlock = 4
		}

		public enum TransitionState
		{
			NotTransiting = -1,
			PreparingToTransit = 0,
			Transiting = 1
		}

		public enum LoadingState
		{
			NotLoading = -1,
			PreparingToLoad = 0,
			Loading = 1
		}
		public enum LoadingType
		{
			Normal = 0,
			Block = 1
		}

		//public static char sp = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
		public static System.Globalization.CultureInfo en = new System.Globalization.CultureInfo("en-US");

		public static Single SingleParse(string input)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = en;
			System.Threading.Thread.CurrentThread.CurrentCulture = en;
			return Single.Parse(input);
		}


		public void InitializeAll()
		{
			stp = new Stopwatch();
			stp.Start();
			Audio.InitAudio();
			SrkFontDisplayer.InitFonts();
			InitializeGraphics();
			DefaultCamera();
			ResourceLoader.InitEmptyData();
			new System.Threading.Thread(() =>
			{
				while (ticks < 1)
				{

				}
				while (true)
				{
					if (stp.Elapsed.TotalSeconds >= seconds + 0.25)
					{
						FPS_ = (FPS_ + ticks) / 2f;
						//if (FPS_ * 4f>10)
						FPS = FPS_ * 4f;
						Window.Title = title + " - " + FPS + " FPS - " + Window.ClientBounds.Width + "x" + Window.ClientBounds.Height;
						//CharToScreen.WriteText((FPS_ * 4f) + " FPS\x0\x0\x0\x0\x0\x0", 5, 1, Color.White, Color.Black);
						//Debug.WriteLine(FPS);
						//Console.WriteLine(FPS);
						ticks = 0;
						seconds = stp.Elapsed.TotalSeconds;
					}
					System.Threading.Thread.Sleep(10);
				}
			}).Start();

			CharToScreen.Init(16, 16);
			LoadingScreen.Init();
		}
		public static SamplerState DefaultSmaplerState;


		public void LoadGraphicSettings()
		{
			if (File.Exists("graphicSettings.ini"))
			{
				string[] contenu = File.ReadAllLines("graphicSettings.ini");
				if (contenu.Length <= graphicSettings.Length)
				{
					Array.Copy(contenu, graphicSettings, contenu.Length);
				}
			}
			else
			{
				File.WriteAllLines("graphicSettings.ini", graphicSettings);
			}
			for (int i = 0; i < graphicSettings.Length; i++)
			{
				string[] spli = graphicSettings[i].Split('=');
				switch (spli[0])
				{
					case "MultisampleCount":
						MultisampleCount = int.Parse(spli[1]);
						break;
					case "EnableAntiAliasing":
						EnableAntiAliasing = bool.Parse(spli[1]);
						break;
					case "UseXboxController":
						UseXboxController = bool.Parse(spli[1]);
						break;

				}
			}

			if (EnableAntiAliasing)
				BufferAntialiasing();
			else
				MultisampleCount = 1;
		}

		public bool OrderingThreadColorCopy = false;
		public bool OrderingThreadTextureCopy = false;
		public Texture2D sourceCopyT2D;
		public Stream sourceCopyStream;
		public Color[] sourceCopyColorArray;

		public void ThreadCopy()
		{
			if (OrderingThreadColorCopy)
			{
				sourceCopyColorArray = null;
				sourceCopyColorArray = new Color[sourceCopyT2D.Width * sourceCopyT2D.Height];
				sourceCopyT2D.GetData<Microsoft.Xna.Framework.Color>(sourceCopyColorArray);
				OrderingThreadColorCopy = false;
			}
			if (OrderingThreadTextureCopy)
			{
				sourceCopyT2D = Texture2D.FromStream(graphics.GraphicsDevice, sourceCopyStream);
				OrderingThreadTextureCopy = false;
			}
		}

		public void BufferAntialiasing()
		{
			var window = new OpenTK.GameWindow(graphics.PreferredBackBufferWidth,
				graphics.PreferredBackBufferHeight, new OpenTK.Graphics.GraphicsMode(32, 0, 0, MultisampleCount));
			window.Exit();
			window.Dispose();
		}


		void InitializeGraphics()
		{
			graphics.PreferMultiSampling = EnableAntiAliasing;

			graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = MultisampleCount;
			if (EnableAntiAliasing)
				graphics.GraphicsProfile = GraphicsProfile.HiDef;
			else
				graphics.GraphicsProfile = GraphicsProfile.Reach;

			graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;


			rasterWireframe = new RasterizerState
			{
				FillMode = FillMode.WireFrame,
				MultiSampleAntiAlias = EnableAntiAliasing
			};

			rasterSolid = new RasterizerState
			{
				FillMode = FillMode.Solid,
				MultiSampleAntiAlias = EnableAntiAliasing
			};

			rasterSolidNoCull = new RasterizerState
			{
				FillMode = FillMode.Solid,
				MultiSampleAntiAlias = EnableAntiAliasing,
				CullMode = CullMode.None
			};

			alphaTest = new AlphaTestEffect(graphics.GraphicsDevice)
			{
				VertexColorEnabled = true,
				DiffuseColor = Color.White.ToVector3(),
				AlphaFunction = CompareFunction.Greater,
				ReferenceAlpha = 50
			};

			basicEffect = new BasicEffect(graphics.GraphicsDevice)
			{
				TextureEnabled = true,
				VertexColorEnabled = true,
				Alpha = 1f
			};

			/*basicEffect.EnableDefaultLighting();
            basicEffect.SpecularColor = (Color.White * 0.333f).ToVector3();
            basicEffect.LightingEnabled = true;
            basicEffect.PreferPerPixelLighting = true;*/

			/*basicEffect.FogEnabled = true;
            basicEffect.FogStart = 60000f;
            basicEffect.FogEnd = 70000f;
            basicEffect.FogColor = (new Microsoft.Xna.Framework.Color(194,96,28,255)).ToVector3();
            */



			IsMouseVisible = true;
			Window.AllowUserResizing = true;

			MainGame.DefaultSmaplerState = graphics.GraphicsDevice.SamplerStates[0];
		}

		public void WindowBoundsUpdate()
		{
			if (Window.ClientBounds.Width != bufferWidth || Window.ClientBounds.Height != bufferHeight)
			{
				mainCamera.AspectRatio = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;

				Bulle.Scale = new Vector3(Window.ClientBounds.Width / 3840f, 0, Window.ClientBounds.Height / 2160f);
				if ((Window.ClientBounds.Width * Window.ClientBounds.Height) < 2100000f)
				{
					Bulle.ScaleMatrix = Matrix.CreateScale((Window.ClientBounds.Width * Window.ClientBounds.Height) / 2592000f);
				}
				else
				{
					Bulle.ScaleMatrix = Matrix.CreateScale(0.8f);
				}

				PAX.ProjectionMatrixFullScreen = Matrix.CreateOrthographic(Window.ClientBounds.Width / 2f, Window.ClientBounds.Height / 2f, 0f, 120f);

				bufferWidth = Window.ClientBounds.Width;
				bufferHeight = Window.ClientBounds.Height;
			}
			if (FullScreenState != 0)
			{
				if (FullScreenState < 0)
					FullScreenState--;
				else
					FullScreenState++;

				if (FullScreenState == -30)
				{
					Window.BeginScreenDeviceChange(false);
					Window.Position = new Point((int)((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2f) -
					   (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.375f)), (int)((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2f) -
					   (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.375f)));

					graphics.IsFullScreen = false;
					graphics.PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.75f);
					graphics.PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.75f);
					Window.EndScreenDeviceChange(Window.Title, (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.75f), (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.75f));
				}

				if (FullScreenState == 30)
				{
					Window.BeginScreenDeviceChange(true);
					//Window.IsBorderless = true;
					Window.Position = new Point(0, 0);
					graphics.IsFullScreen = true;
					graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
					graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
					Window.EndScreenDeviceChange(Window.Title, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
				}
				if (FullScreenState < -60 || FullScreenState > 60)
					FullScreenState = 0;
				return;
			}


			if (graphics.IsFullScreen && keyboardState.IsKeyDown(Keys.Escape) && oldKeyboardState.IsKeyUp(Keys.Escape))
				FullScreenState = -1;

			if (!graphics.IsFullScreen && oldKeyboardState.IsKeyUp(Keys.Enter) && keyboardState.IsKeyDown(Keys.LeftAlt) && keyboardState.IsKeyDown(Keys.Enter))
				FullScreenState = 1;
		}


		public void DefaultCamera()
        {
            this.mainCamera = new Camera(16f / 9f, MathHelper.ToRadians(70f),
            Vector3.Zero, Microsoft.Xna.Framework.Vector3.Up, 10, 40000000);
            this.mainCamera.Zoom = this.mainCamera.MaxZoom;
            this.mainCamera.Yaw = MathHelper.ToRadians(0*180);
            this.mainCamera.DestYaw = MathHelper.ToRadians(0*180);

            this.mainCamera.Pitch = -0.21f;
            this.mainCamera.DestPitch = -0.21f;

            this.mainCamera.MoveCameraUp(100f);
		}

		public bool WaitMap = false;
		public MAP map = null;

		public void SetMap(string name, bool bufferOnly, bool thread)
		{
			if (!thread && !bufferOnly && this.MapSet)
				this.Map.Render = false;

			if (WaitMap)
			{
				WaitMap = false;

				Spawn.Load(@"Content\Scenes\Spawn\twilight_town_test.spawn");
				Loading = LoadingState.NotLoading;
			}
			else
			{
				int index = BufferedMaps.IndexOf(name);
				map = null;
				if (index < 0)
				{
					if (thread)
					{
						WaitMap = true;

						new System.Threading.Thread(() => {

							map = new MAP(name);
							map.Parse();
							ResourceFiles.Insert(0, map);
							BufferedMaps.Add(name);
							Maps.Add(map);


							while (Transition[0] > 0)
							{

							}
							SetMap("", bufferOnly, false);


						}).Start();
						return;
					}
					else
					{
						map = new MAP(name);
						for (int i=0;i<10;i++)
						{
							string plus = @"\texture";
							if (i > 0)
								plus = @"\texture"+ i.ToString();
							if (File.Exists(Path.GetDirectoryName(name) + plus + ".zip") &&
								!File.Exists(Path.GetDirectoryName(name) + plus))
							{
								DecompressFileLZMA(Path.GetDirectoryName(name) + plus + ".zip", Path.GetDirectoryName(name) + plus);
							}
						}
						map.Parse();
						ResourceFiles.Insert(0, map);
						BufferedMaps.Add(name);
						Maps.Add(map);

					}
				}
				else
				{
					map = Maps[index];
				}
			}

			if (!bufferOnly)
			{

				if (Action.thread != null)
				{
					Action.thread.Abort();
					Action.thread = null;
				}

				Action.thread = new System.Threading.Thread(() =>
				{
					while (true)
					{
						if (Action.oldTick != Program.game.ticks && (Loading == LoadingState.NotLoading || Loading_Type != LoadingType.Block))
						{
							int count = Action.aAccount * 1;
							if (count > 0)
							{
								Action.aAccount = -1;
								for (int i = 0; i < count; i++)
								{
									Action.ac[i].Verify();
								}
								Action.aAccount = 0;
							}
							count = Action.bAccount * 1;
							if (count > 0)
							{
								Action.bAccount = -1;
								for (int i = 0; i < count; i++)
								{
									Action.be[i].Verify();
								}
								Action.bAccount = 0;
							}

							Action.oldTick = Program.game.ticks;
						}
					}
				});
				Action.aAccount = 0;
				Action.bAccount = 0;
				Action.ac = null;
				Action.be = null;
				Action.thread.Start();
				

				if (map != null)
				{
					alphaTest.FogEnabled = map.FogStart > 0 || map.FogEnd > 0;
					alphaTest.FogStart = map.FogStart;
					alphaTest.FogEnd = map.FogEnd;
					alphaTest.FogColor = map.FogColor;
					map.Render = true;
					map.JustLoaded = true;
					if (File.Exists(@"Content\Effects\Audio\BGM\" + map.Name + ".wav"))
					{
						Audio.NextBGM = @"Content\Effects\Audio\BGM\" + map.Name + ".wav";
					}
				}
				this.Map = map;
			}
			graphics.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
			graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
		}
		private static void CompressFileLZMA(string inFile, string outFile)
		{
			SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
			FileStream input = new FileStream(inFile, FileMode.Open);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Write the encoder properties
			coder.WriteCoderProperties(output);

			// Write the decompressed file size.
			output.Write(BitConverter.GetBytes(input.Length), 0, 8);

			// Encode the file.
			coder.Code(input, output, input.Length, -1, null);
			output.Flush();
			output.Close();
		}

		private static void DecompressFileLZMA(string inFile, string outFile)
		{
			SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
			FileStream input = new FileStream(inFile, FileMode.Open, FileAccess.Read,FileShare.Read);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Read the decoder properties
			byte[] properties = new byte[5];
			input.Read(properties, 0, 5);

			// Read in the decompress file size.
			byte[] fileLengthBytes = new byte[8];
			input.Read(fileLengthBytes, 0, 8);
			long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

			coder.SetDecoderProperties(properties);
			coder.Code(input, output, input.Length, fileLength, null);
			output.Flush();
			output.Close();
		}

		public void DrawTransit()
		{
			graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			basicEffect.View = ViewMatrixTransition;
			basicEffect.Projection = ProjectionMatrixTransition;

			if (MainGame.transitionType == TransitionType.FadeNormal || MainGame.transitionType == TransitionType.FadeInBlackBlock)
				basicEffect.Texture = renderTarget;
			else
				basicEffect.Texture = ResourceLoader.EmptyT2D;

			basicEffect.CurrentTechnique.Passes[0].Apply();

			if (Transition[0] > 0)
				Transition[0]--;
			else if (!WaitMap)
				MainGame.TState = MainGame.TransitionState.NotTransiting;
			else
				Loading = LoadingState.PreparingToLoad;

			byte b = (byte)(255 * Transition[0] / (float)Transition[1]);

			switch (MainGame.transitionType)
			{
				case TransitionType.FadeNormal:
					for (int i = 0; i < 6; i++)
					{
						vertexPositionColorTextures[i].Color.R = 255;
						vertexPositionColorTextures[i].Color.G = 255;
						vertexPositionColorTextures[i].Color.B = 255;
						vertexPositionColorTextures[i].Color.A = b;
					}
					break;
				case TransitionType.FadeInBlack:
					for (int i = 0; i < 6; i++)
					{
						vertexPositionColorTextures[i].Color.R = 0;
						vertexPositionColorTextures[i].Color.G = 0;
						vertexPositionColorTextures[i].Color.B = 0;
						vertexPositionColorTextures[i].Color.A = (byte)(255-b);
					}
					break;
				case TransitionType.FadeOutBlack:
					for (int i = 0; i < 6; i++)
					{
						vertexPositionColorTextures[i].Color.R = 0;
						vertexPositionColorTextures[i].Color.G = 0;
						vertexPositionColorTextures[i].Color.B = 0;
						vertexPositionColorTextures[i].Color.A = b;
					}
					break;
				case TransitionType.FadeInBlackBlock:
					for (int i = 0; i < 6; i++)
					{
						vertexPositionColorTextures[i].Color.R = b;
						vertexPositionColorTextures[i].Color.G = b;
						vertexPositionColorTextures[i].Color.B = b;
						vertexPositionColorTextures[i].Color.A = 255;
					}
					break;
			}
			graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertexPositionColorTextures, 0, vertexPositionColorTextures.Length / 3);

			graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}

		public static Vector3 twoD = new Vector3(1, 0, 1);

		public static float Vector3Distance2D(Vector3 v_a, Vector3 v_b)
		{
			return Vector3.Distance(v_a * twoD, v_b * twoD);
		}

		public void DrawLoadingRenderTarget(GameTime gameTime)
		{
			spriteBatch.Begin();
			spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
			spriteBatch.End();
			if (Program.game.Loading != LoadingState.NotLoading)
			LoadingScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);

			base.Draw(gameTime);
			graphics.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
			graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
		}
	}
}
