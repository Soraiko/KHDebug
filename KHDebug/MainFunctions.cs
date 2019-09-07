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

        public enum LoadingState
        {
            NotLoading = -1,
            PreparingToLoad = 0,
            Loading = 1
        }
        public enum LoadingType
        {
           Normal = 0,
            FadeBlack = 1,
            FadeWhite = 2
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
                        Window.Title = title+" - " + FPS + " FPS";
                        //CharToScreen.WriteText((FPS_ * 4f) + " FPS\x0\x0\x0\x0\x0\x0", 5, 1, Color.White, Color.Black);
                        //Debug.WriteLine(FPS);
                        //Console.WriteLine(FPS);
                        ticks = 0;
                        seconds = stp.Elapsed.TotalSeconds;
                    }
                }
            }).Start();
            Audio.Play(@"Content\Effects\Audio\BGM\TT08.wav", true, null, 100);

            //CharToScreen.Init(16, 16);
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
                sourceCopyColorArray = new Color[sourceCopyT2D.Width* sourceCopyT2D.Height];
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

        public BlendState ShadowBlend;

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
    }
}
