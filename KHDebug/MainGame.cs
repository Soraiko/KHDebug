#define playnAnims
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using kenuno;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace KHDebug
{
    public partial class MainGame : Game
    {
        public static bool france = System.Globalization.CultureInfo.CurrentCulture.EnglishName.ToLower().Contains("rance");
        public long ticks = -1;
        public long ticksAlways = -1;
        public static Stopwatch stp;
        public int Collision = 0;
        public bool Musique = true;

        public float FPS = 60;
        public float FPS_ = 15;
        public int CombatCountDown = 0;

        public static Vector3 MinVertex = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
        public static Vector3 MaxVertex = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);
        public static Model DoModel = null;
        public static int DoBone = -1;
        public static Vector3 DoVector = Vector3.Zero;

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
        public Microsoft.Xna.Framework.Color backgroundColor = new Color(0,0,0);


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
            /*int count = 0;

            for (int i=0;i< 27;i++)
            {

                    File.Copy(@"D:\Jeux\Kingdom Hearts\app_KH2Tools\export\map\jp\TT13-export\texture"+i.ToString("d3")+".png", @"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\TT08\texture"+(394+i).ToString
                        ("d3") + ".png");
                    count++;
                
            }
            return;*/


            /*byte[] arr = File.ReadAllBytes(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\DiveWind\move_001.frames");

            FileStream output = new FileStream(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Effects\Visual\DiveWind\MSET\move_001.frames", FileMode.Create);
            BinaryWriter wr = new BinaryWriter(output);
            int frame = 0;

            for (int i=4;i< arr.Length-4;i+=4)
            {
                int uz = BitConverter.ToInt32(arr,i - 4);
                ushort us = BitConverter.ToUInt16(arr,i);
                ushort us2 = BitConverter.ToUInt16(arr,i+2);
                if (uz==-1)
                    frame++;
                if (frame == 40)
                    break;
                    if (us == 5&& us2<2)
                    {
                        float val = 0.7f+(float)(Math.Sin(((frame % 20) / 20.0) * Math.PI)*0.15);
                        byte[] gb = BitConverter.GetBytes(val);
                        arr[i + 4] = gb[0];
                        arr[i + 5] = gb[1];
                        arr[i + 6] = gb[2];
                        arr[i + 7] = gb[3];
                    }
                wr.Write(arr,i,4);
            }
            wr.Close();
            output.Close();*/

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.75f),
                PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.75f)
            };
            Content.RootDirectory = "Content";
            Window.Title = title;
            LoadGraphicSettings();
        }

        public const string title = "Kingdom Hearts Debug - v1.0.1";
        List<Model> Players = new List<Model>(0);

        public void LoadCharacter(string modelName, string movesetName, ref Model target)
        {
            Model model = new DAE(@"Content\Models\"+ modelName);

            model.Parse();
            ResourceFiles.Add(model);
            model.Render = true;
            target = model;
            model.Opacity = 0;
            if (false&&modelName.Contains("P_EX100"))
            {
                MSET mset = new MSET(@"Content\Models\P_EX100\wait.mset");
                mset.Links.Add(model);
                mset.Parse();
                ResourceFiles.Add(mset);
                mset.Render = true;
                mset.ExportBinary();
                mset.PlayingIndex = mset.idle_;
                model.Links.Add(mset);

            }
            else
            {
                /*MSET m = new MSET(@"D:\Jeux\Kingdom Hearts\app_KH2Tools\export\anm_converted\hb\p_ex100.mset");
                m.Links.Add(model);
                m.Parse();
                m.ExportBinary();*/

                BinaryMoveset mset = new BinaryMoveset(@"Content\Models\" + movesetName + @"\MSET");
                mset.Links.Add(model);
                mset.Parse();
                ResourceFiles.Add(mset);
                mset.Render = true;
                mset.PlayingIndex = mset.idle_;
                model.Links.Add(mset);
            }
        }
        

        public void LoadKeyblade(string modelName, string movesetName, ref Model target)
        {
            Model keyblade = new DAE(@"Content\Models\" + modelName);
            keyblade.Parse();
            ResourceFiles.Add(keyblade);
            keyblade.Render = true;
            keyblade.Opacity = 0;


            BinaryMoveset keybladeMset = new BinaryMoveset(@"Content\Models\" + movesetName + @"\MSET");
            keybladeMset.Links.Add(keyblade);
            keybladeMset.Master = (target.Links[0] as Moveset);

            keybladeMset.Parse();
            ResourceFiles.Add(keybladeMset);
            keybladeMset.Render = true;

            keybladeMset.PlayingIndex = keybladeMset.idle_;

            keyblade.Links.Add(keybladeMset);
            keyblade.Master = target;
            target.Keyblade = keyblade;
        }

        public List<Cursor> cursors;
        public Model map1;
        public Model map2;
        public static int Hour = 0;
        public void SetMap(string name, bool bufferOnly)
        {
            if (!bufferOnly && this.Map != null)
            {
                this.Map.Render = false;
            }

            int index = BufferedMaps.IndexOf(name);
            Model map = null;
            if (index <0)
            {
                map = new BinaryModel(name);
                map.Parse();
                ResourceFiles.Insert(0,map);
                BufferedMaps.Add(name);
                Maps.Add(map);
            }
            else
            {
                map = Maps[index];
            }

            alphaTest.FogEnabled = map.FogStart > 0 || map.FogEnd>0;
            alphaTest.FogStart = map.FogStart;
            alphaTest.FogEnd = map.FogEnd;
            alphaTest.FogColor = map.FogColor;
            if (!bufferOnly)
            {
                map.Render = true;
                this.Map = map;
            }
        }

        protected override void Initialize()
        {
            cursors = new List<Cursor>();
            /*cursors.Add(new Cursor());
            cursors.Add(new Cursor());
            cursors.Add(new Cursor());
            cursors[0].Color = Color.Red;
            cursors[1].Color = new Color(0,255,0);
            cursors[2].Color = new Color(0,0,255);*/

            /*DAE dae = new DAE(@"Content\Models\TT08\TT08.dae");
            dae.Parse();
            dae.ExportBin();*/

            base.Initialize();
            InitializeGraphics();
            DefaultCamera();
            ResourceLoader.InitEmptyData();
            InitializeAll();

            /*Loading = LoadingState.PreparingToLoad;
            new Thread(() =>
            {
               Thread.CurrentThread.Priority = ThreadPriority.Highest;
                */
            SetMap(@"Content\Models\TT09\TT09.mdl", true);
            SetMap(@"Content\Models\TT08\TT08.mdl", false);
            this.Map.Render = true;

            LoadCharacter(@"P_EX100\P_EX100.dae", @"P_EX100", ref this.Player);
            LoadKeyblade(@"W_EX010\W_EX010.dae", @"W_EX010", ref this.Player);


            LoadCharacter(@"P_EX020\P_EX020.dae", @"P_EX020", ref this.Partner1);
            LoadKeyblade(@"W_EX020\W_EX020.dae", @"W_EX020", ref this.Partner1);


            this.Sora = this.Player;
            this.Donald = this.Partner1;



            this.Player.Patches[0].GetPatch(2);

            if (this.Player != null)
            {
                this.Player.Location = new Vector3(-2051.733f, 0.1857068f, -210.8058f);
                this.Player.DestRotate = 1.583f;
            }

            if (this.Partner1 != null)
            {
                this.Partner1.Location = new Vector3(-2042.231f, 0.20275f, -330.63077f);
                this.Partner1.DestRotate = 1.583f;
            }

            this.mainCamera.Target = this.Player;
            this.mainCamera.DestLookAt = this.mainCamera.Target.Location + this.mainCamera.Target.HeadHeight;

            this.mainCamera.LookAt = this.mainCamera.DestLookAt;
            this.mainCamera.DestYaw = this.mainCamera.Target.Rotate - PI / 2f;
            this.mainCamera.Yaw = this.mainCamera.DestYaw;

            /*Loading = LoadingState.NotLoading;

            Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            }).Start();*/

            
        }

        public static List<string> BufferedMaps = new List<string>(0);
        public static List<Model> Maps = new List<Model>(0);

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ResourceFiles = new List<Resource>(0);
        }

        protected override void UnloadContent()
        {

        }
        public const float PI = (float)Math.PI;
        double seconds = 0;

        public Camera mainCamera;

        public Model Player;
        public Model Partner1;

        public Model Sora;
        public Model Roxas;
        public Model Riku;
        public Model Donald;

        public Model Map;


        int FullScreenState = 0;

        Random rnd = new Random();

        public static int bufferWidth = 0;
        public static int bufferHeight = 0;
        

        protected override void Update(GameTime gameTime)
        {
            if (Window.ClientBounds.Width != bufferWidth || Window.ClientBounds.Height != bufferHeight)
            {
                mainCamera.AspectRatio = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;

                bufferWidth = Window.ClientBounds.Width;
                bufferHeight = Window.ClientBounds.Height;
            }

            ThreadCopy();
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.L) && oldKeyboardState.IsKeyUp(Keys.L))
            {
                if (Program.game.Loading < 0)
                    Program.game.Loading = MainGame.LoadingState.PreparingToLoad;
                else
                    Program.game.Loading = MainGame.LoadingState.NotLoading;
            }

            if (Loading != LoadingState.NotLoading)
            {
                oldKeyboardState = keyboardState;
                return;
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
                    Window.Position = new Point((int)((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width/2f)-
                       (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.375f)),(int)((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height/ 2f)-
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

            if (Program.game.CombatCountDown > 0)
                Program.game.CombatCountDown--;
            
            if (graphics.IsFullScreen && keyboardState.IsKeyDown(Keys.Escape) && oldKeyboardState.IsKeyUp(Keys.Escape))
                FullScreenState = -1;

            if (!graphics.IsFullScreen &&oldKeyboardState.IsKeyUp(Keys.Enter) && keyboardState.IsKeyDown(Keys.LeftAlt) && keyboardState.IsKeyDown(Keys.Enter))
                FullScreenState = 1;
            
            this.mainCamera.GamepadControls(Window);

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
                alphaTest.View = this.mainCamera.ViewMatrix;
                alphaTest.Projection = this.mainCamera.ProjectionMatrix;

                basicEffect.View = alphaTest.View;
                basicEffect.Projection = alphaTest.Projection;

                this.mainCamera.Update(Window);
            }

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

            PAXCaster.UpdatePaxes();


            oldKeyboardState = keyboardState;
            base.Update(gameTime);
        }


        RenderTarget2D renderTarget = null;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            if (Loading== LoadingState.PreparingToLoad)
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

            if (Loading == LoadingState.Loading)
            {
                DrawRenderTarget(gameTime);
                graphics.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
                graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                //DebugMenu.Update();
                //CharToScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                return;
            }
            if (FullScreenState !=0)
            {
                return;
            }
            
            if (ScenePlayer.ScenePlaying)
            for (int i = 0; i < ScenePlayer.Scenes.Count; i++)
            {
                ScenePlayer.Scenes[i].RenderNext();
            }

            for (int i = 0; i < ResourceFiles.Count; i++)
            {
                Model mdl = ResourceFiles[i] as Model;
                if (ResourceFiles[i].Render && mdl != null)
                {
                    mdl.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                }
            }

            if (this.Map!=null && mainCamera.Target != null)
            {
                MAPSpecies.ApplySpecies(this.Map);

                (this.Map.Links[0] as Collision).Draw(graphics, alphaTest, basicEffect, rasterSolid);

                this.Map.DrawObjects(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                this.Map.DrawEfects(graphics, basicEffect, rasterSolid);

                for (int i = 0; i < cursors.Count; i++)
                cursors[i].Draw(graphics, basicEffect);

                PAXCaster.DrawPaxes(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                //DebugMenu.Update();
                //CharToScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);
                Audio.UpdateAmbient();
            }

            base.Draw(gameTime);

            if (Loading == LoadingState.PreparingToLoad)
            {
                GraphicsDevice.SetRenderTarget(null);
                Loading = LoadingState.Loading;
                DrawRenderTarget(gameTime);
            }
            ticks++;
        }

        public void DrawRenderTarget(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);

            spriteBatch.End();
            LoadingScreen.Draw(graphics, alphaTest, basicEffect, rasterSolid, rasterSolidNoCull);

            base.Draw(gameTime);
        }

        public void PerformIACharacter(Model model)
        {
            if (mainCamera.Target == null ||
                this.Partner1 == null ||
                this.Player == null)
                return;

            if (model.Master == null && mainCamera.Target != model)
                model.DestOpacity = 1;

            if (mainCamera.Target != model && 
                (model.ResourceIndex == this.Partner1.ResourceIndex ||
                model.ResourceIndex == this.Player.ResourceIndex
                ))
            {
                Vector3 requestedLoc = model.Location;

                double angle = model.DestRotate - MainGame.PI / 2f;
                if (true)
                {
                    Vector3 oriPoint = 1f * requestedLoc;
                    Vector3 targetPoint = 1f * mainCamera.Target.Location;
                    float dist = Vector3.Distance(oriPoint, targetPoint);
                    float angl = MainGame.PI*1.5f-(float)Math.Atan2((oriPoint.Z - targetPoint.Z) / dist, (oriPoint.X - targetPoint.X) / dist);
                    
                    SrkBinary.MakePrincipal(ref angl);
                    angl = Math.Abs(angl-mainCamera.PrincipalYaw);
                    bool tropLoin = dist > 800 && angl>1.9f && angl < 4.1f;

                    if (tropLoin||model.SmoothJoystick > 0.4 && model.Speed > 0f && model.Speed < 5f)
                    {
                        model.iaBlockedCount++;
                    }
                    if (!tropLoin && FPS <10)
                    {
                        model.iaBlockedCount = 0;
                    }
                    if (model.iaBlockedCount > 50)
                    {
                        oriPoint.Y += model.HeadHeight.Y * 2f;
                        targetPoint.Y += mainCamera.Target.HeadHeight.Y * 2f;

                        for (int i = mainCamera.Target.PathHistory.Count - 1; i > 0; i--)
                        {
                            Vector3 point = 1f * mainCamera.Target.PathHistory[i];
                            if (tropLoin)
                            {
                                dist = Vector3.Distance(point, targetPoint);
                                angl = MainGame.PI * 1.5f - (float)Math.Atan2((point.Z - targetPoint.Z) / dist, (point.X - targetPoint.X) / dist);

                                SrkBinary.MakePrincipal(ref angl);
                                angl = Math.Abs(angl - mainCamera.PrincipalYaw);

                                if (angl > 1.9f && angl < 4.1f && Vector3.Distance(point, targetPoint) >= 800)
                                {
                                    model.iaBlockedCount = 0;
                                    model.Goto.X = Single.NaN;
                                    requestedLoc = point;
                                    break;
                                }
                            }
                            else
                            {
                                point.Y += mainCamera.Target.HeadHeight.Y * 2f;
                                if (!(this.Map.Links[0] as Collision).HasCol(oriPoint, point) /*&&
                                !(this.Map.Links[0] as Collision).HasCol(point, targetPoint)*/)
                                {

                                    if (model.cState == Model.ControlState.Jump)
                                        model.JumpPress = Math.Abs(model.JumpStart - requestedLoc.Y) < Math.Abs(model.JumpStart - point.Y);


                                    if (point.Y - mainCamera.Target.HeadHeight.Y * 2f > oriPoint.Y - model.HeadHeight.Y * 2f + 30)
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
                                    model.SmoothJoystick = 1f;
                                    point.Y -= mainCamera.Target.HeadHeight.Y * 2f;
                                    model.Goto = ((point - requestedLoc) * 1.3f) + requestedLoc;
                                    break;
                                }
                            }
                        }
                        if (!(this.Map.Links[0] as Collision).HasCol(oriPoint, targetPoint))
                        {
                            model.iaBlockedCount = 0;
                            model.Goto.X = Single.NaN;
                        }
                    }
                    else

                    if (model.Speed > 15f)
                    {
                        //model.iaBlockedCount = 0;
                        //model.Goto.X = Single.NaN;
                    }


                    Vector3 start = requestedLoc;
                    Vector3 end = this.Player.Location;
                    if (!Single.IsNaN(model.Goto.X))
                    {
                        end = model.Goto;
                        model.TurnStep = 0.1f;
                    }
                    else
                        model.TurnStep = 0.2f;

                    if (Math.Abs(start.Y - end.Y) < 700)
                    {
                        start.Y = 0;
                        end.Y = 0;
                        Vector3 diff_ = end - start;
                        float hypo = Vector3.Distance(start, end);
                        if (Single.IsNaN(model.Goto.X) && hypo > 300)
                        {
                            model.SmoothJoystick += (1f - model.SmoothJoystick) / 60f;
                            angle = (float)(Math.Atan2(diff_.X / hypo, diff_.Z / hypo) - MainGame.PI / 2);

                        }
                        else if (!Single.IsNaN(model.Goto.X) || hypo > 170)
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
                    }
                }
                if (model.SmoothJoystick > 0.3f)
                {
                    float step = model.SmoothJoystick < 0.75 ? 120 : 480;

                    if (model.cState != Model.ControlState.Land)
                    if (model.cState != Model.ControlState.Guard)
                    if (model.cState != Model.ControlState.UnGuard)
                    if (model.pState != Model.State.BlockAll || model.pState == Model.State.NoIdleWalkRun_ButRotate)
                    {
                        model.DestRotate = (float)(angle + Math.PI / 2);
                        if (model.pState != Model.State.BlockAll || model.pState == Model.State.NoIdleWalkRun_ButRotate)
                        {
                            requestedLoc.X += (float)((1 / 60f) * (step) * Math.Sin((float)(angle + Math.PI / 2f)));
                            requestedLoc.Z += (float)((1 / 60f) * (step) * Math.Cos((float)(angle + Math.PI / 2f)));

                            if (Program.game.Map != null && Program.game.Map.Links.Count > 0)
                            {
                                (Program.game.Map.Links[0] as Collision).MonitorCollision(model, ref requestedLoc);
                            }
                        }
                    }
                }
                (Program.game.Map.Links[0] as Collision).MonitorCollision(model, ref requestedLoc);
                model.Location = requestedLoc;
            }
        }

        public void RuleCharacter(Model model)
        {
            if (model.Links.Count == 0 || model.HasMaster)
                return;
            var modelMoveset = (model.Links[0] as Moveset);
            if (modelMoveset == null)
                return;
            Vector3 requestedLoc = model.Location;

            float playingFrame = modelMoveset.PlayingFrame;

            
            if (model.Speed>20)
                model.CliffCancel = false;


            if (model.SmoothJoystick > 0.3 && 
                    ((model.cState == Model.ControlState.Land && playingFrame > 15) ||
                    (model.cState == Model.ControlState.Roll && playingFrame > 33))
                )
            {
                modelMoveset.InterpolateFrameRate = 6;
                model.CliffCancel = false;
                modelMoveset.NextPlayingIndex = -1;
                model.pState = Model.State.GoToAtMove;
                model.cState = Model.ControlState.Idle;
                modelMoveset.PlayingIndex = modelMoveset.run;
            }

            float diffLowestFloor = model.LowestFloor - requestedLoc.Y;
            bool flyOk = model.Fly && model.Location.Y > model.LowestFloor + model.StairHeight;
            
            if (model.cState == Model.ControlState.Land)
                model.CurrentGravity2D *= 0.9f;
            else if (modelMoveset.PlayingIndex == modelMoveset.flyIdle_)
                model.CurrentGravity2D *= 0.99f;
            else
            {
                Vector2 diff2D = ((model.Speed2D) - model.CurrentGravity2D);

                model.CurrentGravity2D += diff2D / 25f;
            }
            
            if (model.cState == Model.ControlState.Fly||
                model.cState == Model.ControlState.Fall)
            {
                float multip = model.cState == Model.ControlState.Fly ? 0.1f : 0.0666f;

                requestedLoc += new Vector3(model.CurrentGravity2D.X, 0, model.CurrentGravity2D.Y) * multip;
            }


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
                            if ((this.Map.Links[0] as Collision).CanClimb(model))
                            {
                                modelMoveset.PlayingIndex = modelMoveset.cliffExit_;
                                modelMoveset.AtmospherePlayingIndex = modelMoveset.cliffExit_;
                                model.cState = Model.ControlState.UnCliff;
                            }
                            else
                            {
                                model.CliffCancel = true;
                            }
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
                                    model.JumpDelay = -1;
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
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.fall_;
                        modelMoveset.PlayingIndex = modelMoveset.fall_;
                        model.cState = Model.ControlState.Fall;
                    }
                    else if (model.cState != Model.ControlState.Fall)
                    {
                        if ((int)model.cState <3)
                            //requestedLoc.Y += (model.LowestFloor - requestedLoc.Y) * model.SmoothJoystick;
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
                            if (model.JumpDelay > 5f)
                            {
                                model.oldJumpPress = true;
                                model.cState = Model.ControlState.Jump;
                                modelMoveset.PlayingIndex = modelMoveset.jump_;
                                modelMoveset.AtmospherePlayingIndex = modelMoveset.jump_; /* <<----- a verifier*/
                            }
                        }
                    }
                }
                if (model.cState == Model.ControlState.Roll)
                {
                    if (modelMoveset.PlayingIndex == modelMoveset.roll_)
                    {
                        if (CombatCountDown > 0)
                            modelMoveset.NextPlayingIndex = modelMoveset.guard_;
                        else
                            modelMoveset.NextPlayingIndex = modelMoveset.idle_;
                    }
                    else
                    {
                        if (CombatCountDown > 0)
                            model.cState = Model.ControlState.Guard;
                        else
                            model.cState = Model.ControlState.Idle;
                    }
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
                if (model.pState != Model.State.BlockAll || model.pState != Model.State.GravityOnly)
                {
                    model.CurrentGravityY += (model.Gravity - model.CurrentGravityY) * (60f / 1200f);
                    requestedLoc.Y -= model.CurrentGravityY;
                }
                if (requestedLoc.Y <= model.LowestFloor)
                {
                    modelMoveset.PlayingIndex = modelMoveset.land_;
                    model.CliffCancel = false;
                    requestedLoc.Y = model.LowestFloor;
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

            if (model.pState != Model.State.NoIdleWalkRun 
                && model.cState != Model.ControlState.Jump 
                && model.cState != Model.ControlState.Fly
                && model.cState != Model.ControlState.Cliff)
            {
                if (model.SmoothJoystick < 0.3)
                {
                    if (Program.game.CombatCountDown > 0)
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.idleFight_;
                    else
                        modelMoveset.AtmospherePlayingIndex = modelMoveset.idle;
                }
                else
                {
                    if (model.SmoothJoystick < 0.75f)
                    {
                        if (Program.game.CombatCountDown > 0)
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.walkFight_;
                        else
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.walk;
                    }
                    else
                    {
                        if (Program.game.CombatCountDown > 0)
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.runFight_;
                        else
                            modelMoveset.AtmospherePlayingIndex = modelMoveset.run;
                    }
                }

            }

            (Program.game.Map.Links[0] as Collision).MonitorCollision(model, ref requestedLoc);
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


                if (modelMoveset.PlayingIndex == modelMoveset.idle_ &&  Program.game.CombatCountDown>0)
                {
                    modelMoveset.PlayingIndex = modelMoveset.guard_;
                    model.cState = Model.ControlState.Guard;
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

            if (Program.game.CombatCountDown == 1)
            {
                if (model.cState == Model.ControlState.Idle || model.cState == Model.ControlState.Guard)
                {
                    modelMoveset.PlayingIndex = modelMoveset.unguard_;
                    model.cState = Model.ControlState.UnGuard;
                    //model.pState = Model.State.GotoAtMove_WaitFinish;
                }
            }


            bool c1 = (model.cState == Model.ControlState.Land && modelMoveset.PlayingIndex != modelMoveset.land_);
            bool c2 = (model.cState == Model.ControlState.Guard && modelMoveset.PlayingIndex != modelMoveset.guard_);
            bool c3 = (model.cState == Model.ControlState.UnGuard && modelMoveset.PlayingIndex != modelMoveset.unguard_);

            if (c1 || c2 || c3)
            {
                model.cState = Model.ControlState.Idle;
            }
            if (c1)
            {

                if (Program.game.CombatCountDown > 0)
                {
                    modelMoveset.PlayingIndex = modelMoveset.guard_;
                    model.cState = Model.ControlState.Guard;
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
                    modelMoveset.InterpolateFrameRate = 6;
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
