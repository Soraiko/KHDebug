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
		public float Masse = 0;
		public bool MasseSet = false;

		bool npc = false;
		public bool NPC
		{
			get
			{
				return this.npc;
			}
			set
			{
				if (value)
					this.TurnStepScale = 0.333f;
				else
					this.TurnStepScale = 1f;
				this.npc = value;
			}
		}
		public bool NPCReached = false;
		public int NPCNewReach = 0;

		public List<int> Attachments = new List<int>(0);
        public bool Attached = false;
        public List<Model> AttachmentsModels = new List<Model>(0);

		public float WalkSpeed_ = 0;
		public float RunSpeed_ = 0;

		public float WalkSpeed = 120;
		public float RunSpeed = 480;
		public float InitialWalkSpeed = 120;
		public float InitialRunSpeed = 480;

		public bool Figed = false;
        public Area Area;
        public bool IsSky = false;
        public Model Keyblade;
        public Vector3 Goto = new Vector3(Single.NaN,0,0);
        public int[] MeshGroups = new int[0];

        public Vector3 MinVertex = new Vector3(Single.NaN, 0, 0);
        public Vector3 MaxVertex = new Vector3(Single.NaN, 1000, 0);
        public float Epaisseur = 0f;

        public float JumpMax = 380f;
        public float JumpMin = 160f;
        public float JumpStart = Single.NaN;
        public float JumpStep =0;

        public float StairHeight = 60f;

        public void Parse()
		{
			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"))
			{
				this.Links.Add(new Collision(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"));
			}

			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + Path.GetFileNameWithoutExtension(this.FileName) + ".obj"))
			{
				this.Collision = new Collision(Path.GetDirectoryName(this.FileName) + @"\" + Path.GetFileNameWithoutExtension(this.FileName) + ".obj");
			}

			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "eyes.txt"))
			{
				this.Eyes = new TexturePatch(Path.GetDirectoryName(this.FileName) + @"\" + "eyes.txt", this);
			}
			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "mouth.txt"))
			{
				this.Mouth = new TexturePatch(Path.GetDirectoryName(this.FileName) + @"\" + "mouth.txt", this);
			}
			for (int i = 0; i < 10; i++)
			{
				if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "patch" + i + ".txt"))
				{
					this.Patches.Add(new TexturePatch(Path.GetDirectoryName(this.FileName) + @"\" + "patch" + i + ".txt", this));
				}
			}
			this.ResourceIndex = Array.IndexOf(Resource.ResourceIndices, this.FileName.Split('.')[0]+".");
            this.IsSky =  this.Name.Contains("SKY");

			if (this is DAE)
				(this as DAE).Parse();
			else if (this is MAP)
			{
				(this as MAP).Parse();
				return;
			}
			else if (this is BinaryModel)
				(this as BinaryModel).Parse();

			if (this.ModelType == MDType.Specular)
			{
				for (int i = 0; i < this.VertexBufferColor.Length; i++)
					AverageVertex += this.VertexBufferColor[i].Position;
				AverageVertex = AverageVertex / (float)this.VertexBufferColor.Length;
			}
			this.GetJoints();

			this.Skeleton.MaxVertex = this.MaxVertex;
			this.Skeleton.MinVertex = this.MinVertex;
			if (this.Skeleton.HeadBone>-1)
			this.HeadHeight = Vector3.Transform(Vector3.Zero, this.Skeleton.Bones[this.Skeleton.HeadBone].GlobalMatrix);

			for (int i = 0; i < this.Skeleton.Bones.Count; i++)
			{
				if (this.Skeleton.Bones[i].Parent == null)
				{
					this.Skeleton.RootBone = i;
					break;
				}
			}
			if (Single.IsNaN(this.Skeleton.ZeroPosition.X))
				this.Skeleton.ZeroPosition = this.Skeleton.Bones[this.Skeleton.RootBone].GlobalMatrix.Translation;
			
			if (this.ModelType == MDType.Human)
				this.ShadowT2D = new RenderTarget2D(KHDebug.Program.game.graphics.GraphicsDevice, 1000, 1000);
		}

        public TexturePatch Eyes;
        public TexturePatch Mouth;
        public Skeleton Skeleton;
		public bool HasColor = false;

        public Model()
        {
            this.Name = "";
            //this.stp = new System.Diagnostics.Stopwatch();
            this.destRotate = 0;

            this.loc = Vector3.Zero;
            this.VertexBufferColor = new VertexPositionColorTexture[0];
			this.ShadowBuffer = new VertexPositionColorTexture[0];
			
			this.VertexBuffer_c = new ComputedVertex[0];
            this.vBuffer = new VertexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), 0, BufferUsage.None);

            
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
				"NeckBone:0",
				"LeftHandBone: 0",
                "LeftLeg: -1:0,0,0:0",
                "LeftKnee: -1:0,0,0:0",
                "LeftFoot: -1:0,0,0:0",
                "RightLeg: -1:0,0,0:0",
                "RightKnee: -1:0,0,0:0",
                "RightFoot: -1:0,0,0:0",
				"CliffPosition: 0,0,0",
                "ZeroPosition: 0,0,0",
				"ZeroPositionFight: 0,0,0",
				"WalkSpeed: 120",
				"RunSpeed: 240",
				"Epaisseur: 0"
			};
            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\Joints.txt"))
            {
                string[] text = File.ReadAllLines(Path.GetDirectoryName(this.FileName) + @"\Joints.txt");
                if (text.Length<=joints.Length)
                Array.Copy(text, joints, text.Length);
            }
            this.Skeleton.HeadBone = int.Parse(joints[0].Split(':')[1]);
            this.Skeleton.NeckBone = int.Parse(joints[1].Split(':')[1]);
			this.Skeleton.LeftHandBone = int.Parse(joints[2].Split(':')[1]);

            this.Skeleton.LeftLeg = int.Parse(joints[3].Split(':')[1]);
            this.Skeleton.LeftKnee = int.Parse(joints[4].Split(':')[1]);
            this.Skeleton.LeftFoot = int.Parse(joints[5].Split(':')[1]);

            this.Skeleton.RightLeg = int.Parse(joints[6].Split(':')[1]);
            this.Skeleton.RightKnee = int.Parse(joints[7].Split(':')[1]);
            this.Skeleton.RightFoot = int.Parse(joints[8].Split(':')[1]);

            this.CliffPosition.X = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[0]);
            this.CliffPosition.Y = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[1]);
            this.CliffPosition.Z = MainGame.SingleParse(joints[9].Split(':')[1].Split(',')[2]);

            this.Skeleton.ZeroPosition.X = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[0]);
            this.Skeleton.ZeroPosition.Y = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[1]);
            this.Skeleton.ZeroPosition.Z = MainGame.SingleParse(joints[10].Split(':')[1].Split(',')[2]);

			this.Skeleton.ZeroPositionFight.X = MainGame.SingleParse(joints[11].Split(':')[1].Split(',')[0]);
			this.Skeleton.ZeroPositionFight.Y = MainGame.SingleParse(joints[11].Split(':')[1].Split(',')[1]);
			this.Skeleton.ZeroPositionFight.Z = MainGame.SingleParse(joints[11].Split(':')[1].Split(',')[2]);

			this.WalkSpeed = MainGame.SingleParse(joints[12].Split(':')[1]);
			this.RunSpeed = MainGame.SingleParse(joints[13].Split(':')[1]);
			this.InitialWalkSpeed = this.WalkSpeed;
			this.InitialRunSpeed = this.RunSpeed;
			this.Epaisseur = MainGame.SingleParse(joints[14].Split(':')[1]);
			
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

		public float PrincipalRotate
		{
			get
			{
				float principal = this.Rotate;

				SrkBinary.MakePrincipal(ref principal);
				return principal;
			}
		}

		public float SmoothJoystick = 0;
        public float Joystick = 0;
		public bool Fly = false;

		private bool rotate_dirty = true;
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
				rotate_dirty = true;

			}
        }



        public Vector3 oldDist;
        public float Speed = 0;

        public int SkateCount = 0;

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
		public static Matrix scale500_matrix = Matrix.CreateScale(500f);

        public unsafe void RecreateVertexBuffer(bool force)
		{
			if (this.Figed)
                return;
            float diff;

			
			if (opacity_dirty)
			{
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
					opacity_dirty = false;
					this.Opacity = this.DestOpacity;
				}
			}

            float OutOpacity = this.Opacity;
            if (this.Master !=null)
			{
				OutOpacity *= this.Master.Opacity;
                this.DestDiffuseColor = this.Master.DestDiffuseColor;
            }
            if (this.ModelType == MDType.Human && Action.aAccount == 0)
            this.DiffuseColor += (this.DestDiffuseColor - this.DiffuseColor) / 4f;

            byte OutOpacity_ = (byte)(OutOpacity * 255);
            
            if (MainGame.stp.ElapsedMilliseconds > lastElapsed + 100)
            {
                this.Speed = Vector3.Distance(this.Location, oldDist);
                this.Speed2D = new Vector2(this.Location.X, this.Location.Z)- new Vector2(oldDist.X, oldDist.Z);
                if (Vector2.Distance(this.Speed2D, Vector2.Zero)>1000)
                {
                    this.Speed2D = Vector2.Zero;
                    this.Speed = 0f;
                }
                oldDist = this.Location;
                lastElapsed = MainGame.stp.ElapsedMilliseconds;
            }

            if (this.HasMaster)
            {
                this.Location = this.Master.Location;
                this.Rotate = this.Master.Rotate;
				this.DestRotate = this.Master.Rotate;
				this.LowestFloor = this.Master.LowestFloor;
			}
            else
            {
				if (rotate_dirty)
				{
					diff = this.DestRotate - this.Rotate;
					if (Math.Abs(diff) > 0.001f)
					{
						this.Rotate += (diff * this.TurnStep * this.TurnStepScale);
					}
					else
						rotate_dirty = false;
				}
            }

            this.AllZeroOpacity = OutOpacity_ == 0;

            if (this.AllZeroOpacity)
                return;

            bool checkMaximums = Program.game.ticksAlways % 8 == 0;

            if (checkMaximums)
            {
                this.MaxVertex.X = Single.MinValue;
                this.MaxVertex.Y = Single.MinValue;
                this.MaxVertex.Z = Single.MinValue;

                this.MinVertex.X = Single.MaxValue;
                this.MinVertex.Y = Single.MaxValue;
                this.MinVertex.Z = Single.MaxValue;
            }



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

            bool mset = force || (this.Links.Count > 0 && this.Links[0] as Moveset != null);
            //Console.WriteLine(this.Name + (this is DAE ? "DAE" : "Model"));
            Vector3 v3 = Vector3.Zero;
            Vector3 ComputingBuffer = Vector3.Zero;
            bool hasMaster = this.HasMaster;
            bool isHuman = this.ModelType == MDType.Human;
            int jo4Ind = 0;

			hasMaster = hasMaster && this.Master.Keyblade !=null && this.ResourceIndex == this.Master.Keyblade.ResourceIndex;

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
                v3 = Vector3.Transform(v3, this.Rotate_matrix);

                if (checkMaximums)
                {
                    if (v3.X > MaxVertex.X) MaxVertex.X = v3.X;
                    if (v3.Y > MaxVertex.Y) MaxVertex.Y = v3.Y;
                    if (v3.Z > MaxVertex.Z) MaxVertex.Z = v3.Z;
                    if (v3.X < MinVertex.X) MinVertex.X = v3.X;
                    if (v3.Y < MinVertex.Y) MinVertex.Y = v3.Y;
                    if (v3.Z < MinVertex.Z) MinVertex.Z = v3.Z;
                }


                this.VertexBufferColor[i].Position = this.Location + v3;

                if (isHuman)
                    this.VertexBufferColor[i].Color.A = OutOpacity_;
			}


			if (this.ShadowBuffer.Length>0)
			{
				this.ShadowBuffer[0].Position = new Vector3(-0.5f, 0, -0.5f);
				this.ShadowBuffer[1].Position = new Vector3(0.5f, 0, -0.5f);
				this.ShadowBuffer[2].Position = new Vector3(0.5f, 0, 0.5f);
				this.ShadowBuffer[3].Position = new Vector3(-0.5f, 0, -0.5f);
				this.ShadowBuffer[4].Position = new Vector3(0.5f, 0, 0.5f);
				this.ShadowBuffer[5].Position = new Vector3(-0.5f, 0, 0.5f);
				
				for (int i = 0; i < 6; i++)
				{
					this.ShadowBuffer[i].Color.A = alpha;
					this.ShadowBuffer[i].Position = Vector3.Transform(this.ShadowBuffer[i].Position, scale500_matrix);
					this.ShadowBuffer[i].Position = Vector3.Transform(this.ShadowBuffer[i].Position, shadowMatrix);
					this.ShadowBuffer[i].Position += new Vector3(this.Location.X, this.LowestFloor+0.05f, this.Location.Z);
				}
			}

			if (!this.MasseSet)
			{
				for (int i=0;i< this.VertexBufferColor.Length;i+=3)
				{
					this.Masse+= (float)Collision.AreaOfTriangle(
						this.VertexBufferColor[i].Position,
						this.VertexBufferColor[i+1].Position,
						this.VertexBufferColor[i+2].Position);
				}
				this.MasseSet = true;
			}


		}



		float opacity = 1f;
		private bool opacity_dirty = true;

        public float Opacity
        {
            get
            {
                return this.opacity;
            }
            set
			{
				if (value > 1)
					value = 1;
				if (value < 0)
					value = 0;

                if (false&&this.HasMaster)
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
                this.opacity = value;
            }
        }

        public Collision Collision;

        float destOpacity = 1f;
        public float DestOpacity
		{
			get
			{
				return this.destOpacity;
			}
			set
			{
				opacity_dirty = true;
				this.destOpacity = value;
			}
		}

        public Vector3 DiffuseColor = Microsoft.Xna.Framework.Color.White.ToVector3();
        public static Vector3 DefaultDiffuseColor = Microsoft.Xna.Framework.Color.White.ToVector3();
        public Vector3 DestDiffuseColor = Microsoft.Xna.Framework.Color.White.ToVector3();

        public VertexPositionColorTexture[] VertexBufferColor;

		public VertexPositionColorTexture[] ShadowBuffer;
		public RenderTarget2D ShadowT2D;

		public Vector3 AverageVertex = Vector3.Zero;
        public ComputedVertex[] VertexBuffer_c;
        public List<int[]> MeshesOffsets;
        public List<string> materialFileNames;
        public List<Texture2D> Textures;
        public List<int> MaterialIndices;
        public VertexBuffer vBuffer;

		public long locBlock = -1;

		public Vector3 loc;
		public Vector3 locAction;
		public int iaBlockedCount = 0;


		public Vector3 SpawnedLocation;


		public Vector3 Location
        {
            get
            {
                return this.loc;
            }
            set
			{
				if (Single.IsNaN(value.X))
				{
					return;
				}
				locBlock--;
				if (locBlock > 0)
				{
					this.loc = this.locAction;
					return;
				}

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
        public Vector3 LastLand = Vector3.Zero;
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
        public float TurnStepScale = 1f;

		public ControlState cState = ControlState.Idle;
        public enum ControlState
        {
            Idle = 0,
            Walk = 1,
            Run = 2,
            Fall = 3,
            Land = 4,
            Jump = 5,
            Guarding = 6,
            UnGuarding = 7,
            Fly = 8,
            Cliff = 9,
            UnCliff = 10,
            Roll = 11,
            Chat = 12,
            Attack1 = 13,
			Attack1Air = 14,
			Guard = 15
		}

        public State pState = State.GoToAtMove;
        public enum State
        {
            GoToAtMove = 0,
            NoIdleWalkRun = 1,
            GotoAtMove_WaitFinish = 2,
            BlockAll = 3,
            GravityOnly = 4,
			Gravity_Slide = 26,
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
        public float lowestFloor = Single.MinValue;

		private float rotate = 0;

        public float Rotate
		{
			get
			{
				return this.rotate;
			}
			set
			{
				this.rotate = value;
				this.Rotate_matrix = Matrix.CreateRotationY(value);
			}
		}
        public Matrix Rotate_matrix = Matrix.Identity;

        float destRotate = 0;
		public Vector3 HeadHeight;

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




        public Vector3 GetGlobalBone(int index, Vector3 offset)
        {
            return Vector3.Transform(Vector3.Transform(offset, this.Skeleton.Bones[index].GlobalMatrix) , this.Rotate_matrix);
        }
        public bool NoCull = false;
		public static Matrix Ortho = Matrix.CreateOrthographic(500f, 500f, 0, Single.MaxValue);

		public void GetShadow(GraphicsDeviceManager gcm, BasicEffect be, AlphaTestEffect at, RasterizerState rs, RasterizerState rsNoCull)
		{
			if (this.ShadowT2D != null && this.Master == null)
			{
				gcm.GraphicsDevice.SetRenderTarget(this.ShadowT2D);
				gcm.GraphicsDevice.Clear(Color.Transparent);


				at.View = Matrix.CreateLookAt(
					this.Location + new Vector3(0, this.MaxVertex.Y, 1f),
					this.Location, Vector3.Up);
				
				at.Projection = Ortho;

				this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);
				gcm.GraphicsDevice.SetVertexBuffer(this.vBuffer);


				for (int j = 0; j < this.MeshesOffsets.Count; j++)
				{
					at.Texture = this.Textures[this.MaterialIndices[j]];
					at.CurrentTechnique.Passes[0].Apply();
					gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.MeshesOffsets[j][0], this.MeshesOffsets[j][1] / 3);
				}

				at.View = be.View;
				at.Projection = be.Projection;
			}
			else
			if (this.Master != null && this.Master.ShadowT2D != null)
			{
				gcm.GraphicsDevice.SetRenderTarget(this.Master.ShadowT2D);
				gcm.GraphicsDevice.Clear(Color.Transparent);


				at.View = Matrix.CreateLookAt(
					this.Master.Location + new Vector3(0, this.Master.MaxVertex.Y, 1f),
					this.Master.Location, Vector3.Up);


				at.Projection = Ortho;

				this.Master.vBuffer.SetData<VertexPositionColorTexture>(this.Master.VertexBufferColor);
				gcm.GraphicsDevice.SetVertexBuffer(this.Master.vBuffer);

				//at.Texture = ResourceLoader.EmptyT2D;
				//at.CurrentTechnique.Passes[0].Apply();

				for (int j = 0; j < this.Master.MeshesOffsets.Count; j++)
				{
					at.Texture = this.Master.Textures[this.Master.MaterialIndices[j]];
					at.CurrentTechnique.Passes[0].Apply();
					gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.Master.MeshesOffsets[j][0], this.Master.MeshesOffsets[j][1] / 3);
				}


				this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);
				gcm.GraphicsDevice.SetVertexBuffer(this.vBuffer);
				for (int j = 0; j < this.MeshesOffsets.Count; j++)
				{
					at.Texture = this.Textures[this.MaterialIndices[j]];
					at.CurrentTechnique.Passes[0].Apply();
					gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.MeshesOffsets[j][0], this.MeshesOffsets[j][1] / 3);
				}
				

				at.View = be.View;
				at.Projection = be.Projection;
			}
		}

		public void DrawShadow(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			if (this.Master != null)
				return;

			var rs_old = gcm.GraphicsDevice.RasterizerState;
			var depth_old = gcm.GraphicsDevice.DepthStencilState;

			gcm.GraphicsDevice.RasterizerState = rs;
			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			if (this.ShadowT2D != null)
			{
				be.Texture = this.ShadowT2D;
				be.CurrentTechnique.Passes[0].Apply();
				gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.ShadowBuffer, 0, this.ShadowBuffer.Length / 3);
			}

			gcm.GraphicsDevice.RasterizerState = rs_old;
			gcm.GraphicsDevice.DepthStencilState = depth_old;
		}

        public void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
			if (this is MAP)
			{
				(this as MAP).Draw(gcm, at, be, rs, rsNoCull);
			}
            if (this.AllZeroOpacity)
            {
                return;
            }
            if (this.Eyes.Set == 0x584976)
            {
                this.Eyes.AnimateEye();
            }

            if (this.Mouth.Set == 0x584976 && this.NPC && this.Links.Count>0)
            {
				var mset = (this.Links[0] as Moveset);
				//if (mset.PlayingIndex == mset.chat1_)
				int ind = -1;
				if (BulleSpeecher.CurrentBulle != null)
				{
					ind = BulleSpeecher.bulles.IndexOf(BulleSpeecher.CurrentBulle);
					if (ind>-1 && BulleSpeecher.bulleEmmiters[ind]!=null && BulleSpeecher.bulleEmmiters[ind].ResourceIndex == this.ResourceIndex)
					this.Mouth.AnimateMouth();
				}
				if (ind<0 && this.Mouth.lastIndex > -1)
					this.Mouth.GetPatch(-1);
			}



			if (rs != null)
            {
                if (this.NoCull)
                    gcm.GraphicsDevice.RasterizerState = rsNoCull;
                else
                    gcm.GraphicsDevice.RasterizerState = rs;
            }


			if (this.ModelType == MDType.Specular)
			{
                Vector3 camPos = Program.game.mainCamera.LookAt + Vector3.Transform(Vector3.Backward, Program.game.mainCamera.YawPitch_matrix) * Program.game.mainCamera.Zoom;

                double hypo = Vector3.Distance(camPos + new Vector3(0, this.AverageVertex.Y- camPos.Y, 0), this.AverageVertex);
                double angle = Math.Atan2((camPos.Z - this.AverageVertex.Z) / hypo, ((camPos.X - this.AverageVertex.X) / hypo));
                
                float scale =600f/(float)hypo;

                float xOff = (float)((camPos.X - this.AverageVertex.X) / hypo)*0.3f;
                float yOff = (float)((camPos.Y - this.AverageVertex.Y) / hypo) * 0.3f;
                if (xOff > 0.5)
                    xOff = 0.5f;
                if (yOff > 0.5)
                    yOff = 0.5f;

				Matrix scaleMat = Matrix.CreateScale(0.73f*scale);
				float width = (this.MaxVertex.X - this.MinVertex.X);
				float height = (this.MaxVertex.Y - this.MinVertex.Y);
				float ratio = height / width;

				for (int i = 0; i < this.VertexBufferColor.Length; i++)
				{
					this.VertexBufferColor[i].TextureCoordinate.X = this.VertexBufferColor[i].Position.X;
					this.VertexBufferColor[i].TextureCoordinate.Y = this.VertexBufferColor[i].Position.Y;

					this.VertexBufferColor[i].TextureCoordinate.X -= this.MinVertex.X;
					this.VertexBufferColor[i].TextureCoordinate.Y -= this.MinVertex.Y;

					this.VertexBufferColor[i].TextureCoordinate.X /= width;
					this.VertexBufferColor[i].TextureCoordinate.Y /= height;


					this.VertexBufferColor[i].TextureCoordinate.Y = 1 - this.VertexBufferColor[i].TextureCoordinate.Y;

					this.VertexBufferColor[i].TextureCoordinate.X -= 0.5f;
					this.VertexBufferColor[i].TextureCoordinate.Y -= 0.5f;

					this.VertexBufferColor[i].TextureCoordinate.Y *= ratio;

					this.VertexBufferColor[i].TextureCoordinate = Vector2.Transform(this.VertexBufferColor[i].TextureCoordinate, scaleMat);
                    this.VertexBufferColor[i].TextureCoordinate.X += 0.5f;
                    this.VertexBufferColor[i].TextureCoordinate.Y += 0.5f;

                    this.VertexBufferColor[i].TextureCoordinate.X += -xOff;
                    this.VertexBufferColor[i].TextureCoordinate.Y += yOff;
                    
                }
            }
            
            this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);
            gcm.GraphicsDevice.SetVertexBuffer(this.vBuffer);
			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			if (this.ModelType == MDType.Sky || this.ModelType == MDType.Specular || (this.Opacity>0.001f&&this.Opacity<1))
			{
				if (be != null)
                    {
                        be.VertexColorEnabled = true;// this.HasColor;
                        if (this.HasColor)
                            be.DiffuseColor = DefaultDiffuseColor;
                        else
							be.DiffuseColor = this.DiffuseColor;

					//be.Texture = ResourceLoader.EmptyT2D;
					//be.CurrentTechnique.Passes[0].Apply();

					for (int i = 0; i < this.MeshesOffsets.Count; i++)
					{
						if (this.MaterialIndices[i] < this.Textures.Count)
							be.Texture = this.Textures[this.MaterialIndices[i]];
						else
							be.Texture = ResourceLoader.EmptyT2D;
						be.CurrentTechnique.Passes[0].Apply();
                        gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.MeshesOffsets[i][0], this.MeshesOffsets[i][1] / 3);
                    }
					//be.Texture = KHDebug.ResourceLoader.GetT2D(this.materialFileNames[this.MaterialIndices[ind]]);
				}
			}
                else
                {
                    if (at != null)
                    {
                        at.VertexColorEnabled = this.HasColor;
                        if (this.HasColor)
                            at.DiffuseColor = DefaultDiffuseColor;
                        else
                            at.DiffuseColor = this.DiffuseColor;

					//at.Texture = ResourceLoader.EmptyT2D;
					//at.CurrentTechnique.Passes[0].Apply();
					for (int i = 0; i < this.MeshesOffsets.Count; i++)
					{
						if (this.MaterialIndices[i] < this.Textures.Count)
							at.Texture = this.Textures[this.MaterialIndices[i]];
						else
							at.Texture = ResourceLoader.EmptyT2D;
						at.CurrentTechnique.Passes[0].Apply();
                        gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.MeshesOffsets[i][0], this.MeshesOffsets[i][1] / 3);
                    }
                        //at.Texture = KHDebug.ResourceLoader.GetT2D(this.materialFileNames[this.MaterialIndices[ind]]);
                    }
                }
                //gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBufferColor,this.MeshesOffsets[i][0], this.MeshesOffsets[i][1] / 3);
            
        }

    }
}
