using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace KHDebug
{
    public class Model : Resource
    {
        public bool Figed = false;
        public Area Area;
        public bool IsSky = false;
        public List<int> varIDs;
        public List<bool> varValues;
        public Model Keyblade;
        public Vector3 Goto = new Vector3(Single.NaN,0,0);
        public Vector3 BlockedGoto = new Vector3(Single.NaN,0,0);
        public int[] MeshGroups = new int[0];

        public Vector3 MinVertex = new Vector3(Single.NaN, 0, 0);
        public Vector3 MaxVertex = new Vector3(Single.NaN, 0, 0);
        public float Epaisseur = 50f;

        public float JumpMax = 380f;
        public float JumpMin = 160f;
        public float JumpStart = Single.NaN;
        public float JumpStep =0;

        public float StairHeight = 60f;

        public SunBeam sunbeam;
        public static string[] sunbeamList;
        public float FogStart = 0f;
        public float FogEnd = 0f;
        public Microsoft.Xna.Framework.Vector3 FogColor = Color.Transparent.ToVector3();
        public void Parse()
        {
            this.ResourceIndex = Array.IndexOf(Resource.ResourceIndices, this.FileName.Split('.')[0]);
            this.IsSky =  this.Name.Contains("SKY");

            if (this is DAE)
                (this as DAE).Parse();
            if (this is BinaryModel)
                (this as BinaryModel).Parse();

            for (int i = 0; i < this.Supp.Count; i++)
                for (int j = i+1; j < this.Supp.Count; j++)
                {
                    if (this.Supp[j].ZIndex < this.Supp[i].ZIndex)
                    {
                        var curr = this.Supp[j];
                        this.Supp.RemoveAt(j);
                        this.Supp.Insert(0, curr);
                        var currM = this.SuppMsets[j];
                        this.SuppMsets.RemoveAt(j);
                        this.SuppMsets.Insert(0, currM);
                    }
                }

            if (sunbeamList == null)
            {
                sunbeamList = File.ReadAllLines("Content\\Effects\\Visual\\Sun\\sunpositions.txt");
            }
            for (int i = 0; i < sunbeamList.Length; i++)
            {
                string[] spli = sunbeamList[i].Split('|');
                if (spli[0] == this.Name)
                {
                    spli = spli[1].Split(';');
                    this.sunbeam = new SunBeam
                    {
                        From = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]))
                    };
                    this.sunbeam.coll = new Collision("Content\\Effects\\Visual\\Sun\\" + this.Name + ".obj");
                }
            }
        }

        public TexturePatch Eyes;
        public TexturePatch Mouth;
        public Skeleton Skeleton;
        public bool HasColor = false;

        public Model()
        {
            this.Name = "";
            this.Actions = new Action[5000];
            //this.stp = new System.Diagnostics.Stopwatch();
            this.Rotate = 0.001f;
            this.destRotate = 0;

            this.Location = Vector3.Zero;
            this.VertexBufferColor = new VertexPositionColorTexture[0];
            this.VertexBufferShadow = new VertexPositionColor[0];
            
            this.VertexBuffer_c = new ComputedVertex[0];
            this.SpecularBuffer = new Vector2[0];
            this.vBuffer = new VertexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), 0, BufferUsage.None);

            this.Supp = new List<Model>(0);
            this.SuppMsets = new List<Moveset>(0);
            
            this.MeshesOffsets = new List<int[]>(0);
            this.MaterialIndices = new List<int>(0);
            this.materialFileNames = new List<string>(0);
            this.Textures = new List<Texture2D>(0);
            this.Patches = new List<TexturePatch>(0);
        }
        public List<TexturePatch> Patches;
        public bool AllZeroOpacity = false;
        public Vector3 CliffPosition = Vector3.Zero;
        public void GetJoints()
        {
            string[] joints = new string[]
            {
                "HeadBone:0",
                "LeftHandBone: 0",
                "LeftLeg: -1:0,0,0:0",
                "LeftKnee: -1:0,0,0:0",
                "LeftFoot: -1:0,0,0:0",
                "RightLeg: -1:0,0,0:0",
                "RightKnee: -1:0,0,0:0",
                "RightFoot: -1:0,0,0:0",
                "CliffHeight: 0,0,0",
                "ZeroPosition: 0,0,0",
                "ZeroPositionFight: 0,0,0"
            };
                if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\Joints.txt"))
            {
                string[] text = File.ReadAllLines(Path.GetDirectoryName(this.FileName) + @"\Joints.txt");
                if (text.Length<=joints.Length)
                Array.Copy(text, joints, text.Length);
            }
            this.Skeleton.HeadBone = int.Parse(joints[0].Split(':')[1]);
            this.Skeleton.LeftHandBone = int.Parse(joints[1].Split(':')[1]);

            this.Skeleton.LeftLeg = int.Parse(joints[2].Split(':')[1]);
            this.Skeleton.LeftKnee = int.Parse(joints[3].Split(':')[1]);
            this.Skeleton.LeftFoot = int.Parse(joints[4].Split(':')[1]);

            this.Skeleton.RightLeg = int.Parse(joints[5].Split(':')[1]);
            this.Skeleton.RightKnee = int.Parse(joints[6].Split(':')[1]);
            this.Skeleton.RightFoot = int.Parse(joints[7].Split(':')[1]);

            this.CliffPosition.X = MainGame.SingleParse(joints[8].Split(':')[1].Split(',')[0]);
            this.CliffPosition.Y = MainGame.SingleParse(joints[8].Split(':')[1].Split(',')[1]);
            this.CliffPosition.Z = MainGame.SingleParse(joints[8].Split(':')[1].Split(',')[2]);

            this.Skeleton.ZeroPosition.X = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[0]);
            this.Skeleton.ZeroPosition.Y = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[1]);
            this.Skeleton.ZeroPosition.Z = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[2]);

            this.Skeleton.ZeroPositionFight.X = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[0]);
            this.Skeleton.ZeroPositionFight.Y = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[1]);
            this.Skeleton.ZeroPositionFight.Z = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[2]);
        }

        public unsafe struct ComputedVertex
        {
            public fixed float Vertices[16];
            public fixed short Matis[4];
            public int Count;
        }


        public List<Vector3> PathHistory = new List<Vector3>(0);
        


        public float PrincipalDestRotate
        {
            get
            {
                float principal = this.destRotate + MainGame.PI;

                SrkBinary.MakePrincipal(ref principal);
                return principal;
            }
        }

        public float SmoothJoystick = 0;
        public float Joystick = 0;
        public bool Fly = false;

        public float DestRotate
        {
            get
            {
                return this.destRotate;
            }
            set
            {
                float newVal = value;

                while (newVal < this.Rotate - Math.PI)
                {
                    newVal += MainGame.PI*2f;
                }
                while (newVal > this.Rotate + Math.PI)
                {
                    newVal -= MainGame.PI * 2f;
                }
                this.destRotate = newVal;
            }
        }



        public Vector3 oldDist;
        public float Speed = 0;
        public Vector2 Speed2D = Vector2.Zero;

        Model master;
        public bool HasMaster = false;

        public Model Master
        {
            get
            {
                return this.master;
            }
            set
            {
                HasMaster = (value != null);
                this.master = value;
            }
        }
        

        //System.Diagnostics.Stopwatch stp = new System.Diagnostics.Stopwatch();
        long lastElapsed = 0;

        public unsafe void RecreateVertexBuffer(bool force)
        {
            if (this.Figed)
                return;
            float diff = 0;

            diff = this.DestOpacity - this.Opacity;
            if (Math.Abs(diff) > 0.01)
            {
                this.Opacity += (diff * (4 / 60f));
                if (this.Opacity > 1)
                    this.Opacity = 1;
                if (this.Opacity < 0)
                    this.Opacity = 0;
            }
            else
            {
                this.Opacity = this.DestOpacity;
            }

            float OutOpacity = this.Opacity;
            if (this.Master !=null)
            {
                OutOpacity *= this.Master.Opacity;
                this.DestDiffuseColor = this.Master.DestDiffuseColor;
            }
            if (this.ModelType == MDType.Human)
            this.DiffuseColor += (this.DestDiffuseColor - this.DiffuseColor) / 4f;

            byte OutOpacity_ = (byte)(OutOpacity * 255);
            
            if (MainGame.stp.ElapsedMilliseconds > lastElapsed + 100)
            {
                this.Speed = Vector3.Distance(this.Location, oldDist);
                this.Speed2D = new Vector2(this.Location.X, this.Location.Z)- new Vector2(oldDist.X, oldDist.Z);
                oldDist = this.Location;
                lastElapsed = MainGame.stp.ElapsedMilliseconds;
            }

            if (this.HasMaster)
            {
                this.Location = this.Master.Location;
                this.Rotate = this.Master.Rotate;
            }
            else
            {
                diff = this.DestRotate - this.Rotate;
                if (Math.Abs(diff) > 0.001f)
                    this.Rotate += (diff * this.TurnStep);
            }

            this.AllZeroOpacity = OutOpacity_ == 0;
            if (this.AllZeroOpacity)
                return;

            bool checkMaximums = Program.game.ticks % 8 == 0;

            if (checkMaximums)
            {
                this.MaxVertex.X = Single.MinValue;
                this.MaxVertex.Y = Single.MinValue;
                this.MaxVertex.Z = Single.MinValue;

                this.MinVertex.X = Single.MaxValue;
                this.MinVertex.Y = Single.MaxValue;
                this.MinVertex.Z = Single.MaxValue;
            }

            bool hasShadow = this.VertexBufferShadow.Length > 0;


            float lowestFloor = this.Master == null ? this.LowestFloor : this.Master.LowestFloor;

            double distlowest = Math.Abs(lowestFloor - this.loc.Y);
            if (distlowest > 200)
            {
                distlowest = 200;
            }
            distlowest = 200 - distlowest;
            byte alpha = (byte)(80 * OutOpacity * (distlowest / 200f));

            Vector3 floorPos = new Vector3(this.Location.X, lowestFloor + 1.5f, this.Location.Z);

            Matrix shadowMatrix = this.Master == null ? this.ShadowMatrixSurface : this.Master.ShadowMatrixSurface;
            Matrix rotY = Matrix.CreateRotationY(this.Rotate);
            bool mset = force || (this.Links.Count > 0 && this.Links[0] as Moveset != null);
            //Console.WriteLine(this.Name + (this is DAE ? "DAE" : "Model"));
            Vector3 v3 = Vector3.Zero;
            Vector3 ComputingBuffer = Vector3.Zero;
            bool hasMaster = this.HasMaster;
            bool isHuman = this.ModelType == MDType.Human;
            int jo4Ind = 0;

            for (int i = 0; i < this.VertexBuffer_c.Length; i++)
            {
                jo4Ind = 0;
                v3 = Vector3.Zero;
                for (int j = 0; j < this.VertexBuffer_c[i].Count; j+=4)
                {
                    ComputingBuffer.X = this.VertexBuffer_c[i].Vertices[j];
                    ComputingBuffer.Y = this.VertexBuffer_c[i].Vertices[j + 1];
                    ComputingBuffer.Z = this.VertexBuffer_c[i].Vertices[j + 2];
                    if (mset)
                    {
                        Matrix mat = this.Skeleton.Bones[this.VertexBuffer_c[i].Matis[jo4Ind]].GlobalMatrix;
                        if (hasMaster)
                        {
                            mat *= this.Master.Skeleton.Bones[this.Master.Skeleton.LeftHandBone].GlobalMatrix;
                        }
                        ComputingBuffer = Vector3.Transform(ComputingBuffer, mat);
                    }
                    v3 += ComputingBuffer * this.VertexBuffer_c[i].Vertices[j + 3];
                    jo4Ind++;
                }
                v3 = Vector3.Transform(v3, rotY);

                if (checkMaximums)
                {
                    if (v3.X > MaxVertex.X) MaxVertex.X = v3.X;
                    if (v3.Y > MaxVertex.Y) MaxVertex.Y = v3.Y;
                    if (v3.Z > MaxVertex.Z) MaxVertex.Z = v3.Z;
                    if (v3.X < MinVertex.X) MinVertex.X = v3.X;
                    if (v3.Y < MinVertex.Y) MinVertex.Y = v3.Y;
                    if (v3.Z < MinVertex.Z) MinVertex.Z = v3.Z;
                }

                if (hasShadow)
                {
                    /*Vector3 absV3 = new Vector3(v3.X, 0, v3.Z);
                    if (absV3.X < 0) absV3.X *= -1f;
                    if (absV3.Z < 0) absV3.Z *= -1f;
                    float dist = Vector3.Distance(Vector3.Zero,absV3);

                    double angle = Math.Atan2(absV3.Z / dist, absV3.X / dist);
                    int index = (int)(((angle / (2 * MainGame.PI)) + 1f) * 50f);

                    if (dist > Vector3.Distance(Vector3.Zero,this.hundredVects[index]))
                    {
                        this.hundredVects[index] = absV3;
                    }*/
                    this.VertexBufferShadow[i].Position = v3;
                    this.VertexBufferShadow[i].Position.Y = 0;

                    this.VertexBufferShadow[i].Position = Vector3.Transform(this.VertexBufferShadow[i].Position, shadowMatrix);
                    this.VertexBufferShadow[i].Position += floorPos;
                    

                    this.VertexBufferShadow[i].Color.A = alpha;
                }

                this.VertexBufferColor[i].Position = this.Location + v3;

                if (isHuman)
                    this.VertexBufferColor[i].Color.A = OutOpacity_;
            }
            /*for (int i=0;i<100;i++)
            {
                this.VertexBufferShadow2[i * 3].Position = Vector3.Zero;
                this.VertexBufferShadow2[i * 3+1].Position = this.hundredVects[i];
                this.VertexBufferShadow2[i * 3+1].Position = this.hundredVects[(i+1)%100];
            }*/
        }


        float opacity = 1f;
        public float Opacity
        {
            get
            {
                return this.opacity;
            }
            set
            {
                if (Math.Abs(value-this.opacity) >0)
                {
                    if (this.HasMaster)
                    {
                        Moveset master = ((this.Master as Model).Links[0] as Moveset);
                        if (value < 0.0001)
                        {
                            master.idle = master.idleRest_;
                            master.walk = master.walkRest_;
                            master.run = master.runRest_;
                        }
                        else
                        {
                            master.idle = master.idle_;
                            master.walk = master.walk_;
                            master.run = master.run_;
                        }
                    }
                }
                this.opacity = value;
            }
        }
        public float DestOpacity = 1f;
        public Vector3 DiffuseColor = Microsoft.Xna.Framework.Color.White.ToVector3();

        public Vector3 DestDiffuseColor = Microsoft.Xna.Framework.Color.White.ToVector3();
        
        public VertexPositionColorTexture[] VertexBufferColor;
        public VertexPositionColor[] VertexBufferShadow;
        //public VertexPositionColor[] VertexBufferShadow2;
        //public Vector3[] hundredVects = new Vector3[100];
        //public VertexPositionNormalTexture[] VertexBuffer;
        public Vector2[] SpecularBuffer;

        public Vector3 AverageVertex = Vector3.Zero;
        public ComputedVertex[] VertexBuffer_c;
        public List<int[]> MeshesOffsets;
        public List<string> materialFileNames;
        public List<Texture2D> Textures;
        public List<int> MaterialIndices;
        public VertexBuffer vBuffer;

        Vector3 loc;
        public int iaBlockedCount = 0;
        public Vector3 Location
        {
            get
            {
                return this.loc;
            }
            set
            {
                if (Vector3.Distance(lastLoc,value)>50)
                {
                    if (PathHistory.Count < 300)
                    {
                        PathHistory.Add(lastLoc);
                    }
                    else
                    {
                        PathHistory.RemoveAt(0);
                        PathHistory.Add(lastLoc);
                    }

                    lastLoc = value;
                }
                this.loc = value;
            }
        }
        Vector3 lastLoc;

        public int InactiveCount = 0;

        public bool JumpCancel = false;
        public bool CliffCancel = false;
        public bool AllowCliffCancel = false;

        public bool JumpCollisionCancel = false;
        public bool JumpPress = false;
        public bool YPress = false;
        public float JumpDelay = -1;
        public bool oldJumpPress = false;
        public bool justDive = false;
        public bool oldRollPress = false;
        public bool rollPress = false;
        public bool oldFlyPress = false;
        public float Gravity = 16f;
        public float CurrentGravityY = 10f;

        public Vector2 CurrentGravity2D = Vector2.Zero;

        public float TurnStep = 0.2f;

        public ControlState cState = ControlState.Idle;
        public enum ControlState
        {
            Idle = 0,
            Walk = 1,
            Run = 2,
            Fall = 3,
            Land = 4,
            Jump = 5,
            Guard = 6,
            UnGuard = 7,
            Fly = 8,
            Cliff = 9,
            UnCliff = 10,
            Roll = 11,
            Chat = 12,
            Attack1 = 13,
            Attack1Air = 14
        }

        public State pState = State.GoToAtMove;
        public enum State
        {
            GoToAtMove = 0,
            NoIdleWalkRun = 1,
            GotoAtMove_WaitFinish = 2,
            BlockAll = 3,
            GravityOnly = 4,
            NoIdleWalkRun_ButRotate = 29
        }
        public Matrix ShadowMatrixSurface = Matrix.Identity;

        public float LowestFloor
        {
            get
            {
                return this.lowestFloor;
            }
            set
            {
                if (this.Speed>3 || Math.Abs(this.lowestFloor-value)>3f)
                {
                    this.lowestFloor = value;
                }
            }
        }
        public float lowestFloor = 0;
        
        public float Rotate = 0;
        float destRotate = 0;
        public List<Model> Supp;
        public List<Moveset> SuppMsets;
        public Vector3 HeadHeight;

        public Action[] Actions;
        public int ActionsCount;
        public int ZIndex = -1;

        public enum MDType
        {
            Human = 0,
            Map = 1,
            Sky = 2,
            Specular = 3,
            ShadowLessHuman = 4
        }

        public MDType ModelType = MDType.Human;

        public void DrawObjects(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            for (int j = 0; j < this.Supp.Count; j++)
            {
                if (!this.Supp[j].IsSky)
                {
                    if (this.SuppMsets[j] != null && this.SuppMsets[j].ObjectMsetRender)
                    {
                        (this.SuppMsets[j] as Moveset).ComputeAnimation();
                    }
                    this.Supp[j].Draw(gcm, at, be, rs, rsNoCull);
                }
            }
        }


        public void DrawEfects(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            if (this.sunbeam != null)
            {
                this.sunbeam.To = Program.game.mainCamera.Position;
                this.sunbeam.RecreateVertexBuffer();
                this.sunbeam.Draw(gcm, be, rs);
            }
        }

        public Vector3 GetGlobalBone(int index)
        {
            return Vector3.Transform(this.Skeleton.Bones[index].GlobalMatrix.Translation, Matrix.CreateRotationY(this.Rotate));
        }
        public bool NoCull = false;

        public void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            if (this.AllZeroOpacity)
            {
                return;
            }
            if (this.Eyes.Set == 0x584976)
            {
                this.Eyes.Animate();
            }
            if (Action.aAccount == 0)
            {
                Action.ac = this.Actions;
                Action.aAccount = this.ActionsCount;
            }

            if (this.VertexBufferShadow.Length>0)
            {
                be.Texture = ResourceLoader.EmptyT2D;
                be.CurrentTechnique.Passes[0].Apply();
                gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.VertexBufferShadow,0, this.VertexBufferShadow.Length/3);
            }

            if (this.Supp.Count>0)
            {
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                for (int i = 0; i < this.Supp.Count; i++)
                {
                    if (this.Supp[i].IsSky)
                        this.Supp[i].Draw(gcm, at, be, rs, rsNoCull);
                }
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            
            if (rs != null)
            {
                if (this.NoCull)
                    gcm.GraphicsDevice.RasterizerState = rsNoCull;
                else
                    gcm.GraphicsDevice.RasterizerState = rs;
            }

            
            if (this.SpecularBuffer.Length>0)
            {
                Vector3 camPos = Program.game.mainCamera.LookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(Program.game.mainCamera.Yaw, Program.game.mainCamera.Pitch, 0)) * Program.game.mainCamera.Zoom;

                double hypo = Vector3.Distance(camPos + new Vector3(0, this.AverageVertex.Y- camPos.Y, 0), this.AverageVertex);
                double angle = Math.Atan2((camPos.Z - this.AverageVertex.Z) / hypo, ((camPos.X - this.AverageVertex.X) / hypo));
                
                float scale =600f/(float)hypo;

                float xOff = (float)((camPos.X - this.AverageVertex.X) / hypo)*0.3f;
                float yOff = (float)((camPos.Y - this.AverageVertex.Y) / hypo) * 0.3f;
                if (xOff > 0.5)
                    xOff = 0.5f;
                if (yOff > 0.5)
                    yOff = 0.5f;
                Matrix scaleMat = Matrix.CreateScale(scale);
                for (int i = 0; i < this.SpecularBuffer.Length; i++)
                {
                    this.VertexBufferColor[i].TextureCoordinate.X = this.SpecularBuffer[i].X - 0.5f;
                    this.VertexBufferColor[i].TextureCoordinate.Y = this.SpecularBuffer[i].Y - 0.5f;
                    this.VertexBufferColor[i].TextureCoordinate = Vector2.Transform(this.VertexBufferColor[i].TextureCoordinate, scaleMat);
                    this.VertexBufferColor[i].TextureCoordinate.X += 0.5f;
                    this.VertexBufferColor[i].TextureCoordinate.Y += 0.5f;

                    this.VertexBufferColor[i].TextureCoordinate.X += -xOff;
                    this.VertexBufferColor[i].TextureCoordinate.Y += yOff;
                    
                    this.VertexBufferColor[i].Color.A = 128;
                }
            }
            
            this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);
            gcm.GraphicsDevice.SetVertexBuffer(this.vBuffer);

            for (int i = 0; i<this.MeshesOffsets.Count; i++)
            {
                if (this.ModelType == MDType.Sky || this.ModelType == MDType.Specular || (this.Opacity>0.001f&&this.Opacity<1))
                {
                    if (be != null)
                    {
                        be.VertexColorEnabled = true;// this.HasColor;
                        be.DiffuseColor = this.DiffuseColor;
                        be.Texture = this.Textures[this.MaterialIndices[i]];
                        be.CurrentTechnique.Passes[0].Apply();
                        //be.Texture = KHDebug.ResourceLoader.GetT2D(this.materialFileNames[this.MaterialIndices[ind]]);
                    }
                }
                else
                {
                    if (at != null)
                    {
                        at.VertexColorEnabled = this.HasColor;
                        at.DiffuseColor = this.DiffuseColor;
                        at.Texture = this.Textures[this.MaterialIndices[i]];
                        at.CurrentTechnique.Passes[0].Apply();
                        //at.Texture = KHDebug.ResourceLoader.GetT2D(this.materialFileNames[this.MaterialIndices[ind]]);
                    }
                }
                gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.MeshesOffsets[i][0], this.MeshesOffsets[i][1] / 3);
                //gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBufferColor,this.MeshesOffsets[i][0], this.MeshesOffsets[i][1] / 3);
            }
        }

    }
}
