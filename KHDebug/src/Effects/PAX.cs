using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public static class PAXCaster
    {
        public static List<PAX> PAX_Instances = new List<PAX>(0);
        public static List<string> PAX_InstancesNames = new List<string>(0);

        public static void FinishPAX(string name)
        {
            for (int i = 0; i < PAX_InstancesNames.Count; i++)
            {
                if (PAX_InstancesNames[i] == name && !PAX_Instances[i].Finished)
                {
                    PAX_Instances[i].Finish();
                }
            }
        }
        /*public static void ResetPAX(string name)
        {
            for (int i = 0; i < PAX_InstancesNames.Count; i++)
            {
                if (PAX_InstancesNames[i] == name && !PAX_Instances[i].Finising)
                {
                    PAX_Instances[i].Reset();
                }
            }
        }*/

        public static void RunPAX(string name, Model on, int bone,Vector3 offset)
        {
            PAX px = null;
            int index = -1;
            for (int i = 0; i < PAX_InstancesNames.Count; i++)
            {
                if (PAX_InstancesNames[i]== name && PAX_Instances[i].Finished)
                {
                    index = i;
                    PAX_Instances[i].Reset();
                    px = PAX_Instances[i];
                    break;
                }
            }
            if (index<0)
            {
                px = new PAX(name);
                PAX_Instances.Add(px);
                PAX_InstancesNames.Add(name);
            }
            px.Finished = false;
            px.On = on;
            px.OnBone = bone;
            px.Offset = offset;
            
        }

        public static void UpdatePaxes()
        {
            if (MainGame.UpdateReactionCommand)
            {
                if (MainGame.ReactionCommand == null)
                    PAXCaster.FinishPAX(@"Content\Effects\Visual\Triangle-Normal\Triangle.dae");
                else
                    PAXCaster.RunPAX(@"Content\Effects\Visual\Triangle-Normal\Triangle.dae", MainGame.DoModel, MainGame.DoBone, MainGame.DoVector);
                    
                MainGame.UpdateReactionCommand = false;
            }
            for (int i = 0; i < PAX_Instances.Count; i++)
            {
                PAX px = PAX_Instances[i];
                px.Output();
                if (!px.Finished)
                {
                    Vector3 base_ = Vector3.Zero;
                    float rot = 0;
                    Vector3 off = px.Offset;

                    if (px.On !=null)
                    {
                        base_ = px.On.Location;
                        rot = px.On.Rotate;
                        off+= px.On.Skeleton.Bones[px.OnBone].GlobalMatrix.Translation;
                    }
                    
                    px.Location = base_ + Vector3.Transform(off, Matrix.CreateRotationY(rot));
                    px.Update();
                }
            }
        }
        public static void DrawPaxes(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            for (int i = 0; i < PAX_Instances.Count; i++)
            {
                PAX px = PAX_Instances[i];
                if (!px.Finished)
                {
                    px.Draw(gcm, at, be, rs, rsNoCull);
                }
            }
        }

    }

    public class PAX
    {
        public string Output()
        {
            return this.Finished + "             "+this.moveFile.playingIndex + "       "+ this.moveFile.PlayingFrame;
        }
        public enum DrawType
        {
            ToScreen2D = 0,
            FullScreen = 1
        }
        public DrawType DType;
        public bool Cull = true;
        public bool Depth = true;
        private DAE daeFile;
        private BinaryMoveset moveFile;
        private byte[][] Frames;
        private List<int>[] FramesIndices;

        public Model On;
        public int OnBone = -1;
        public Vector3 Offset = Vector3.Zero;


        public Vector3 Location
        {
            get
            {
                return this.daeFile.Location;
            }
            set
            {
               this.daeFile.Location = value;
            }
        }

        public PAX(string filename)
        {
            /*FileStream fs = new FileStream(@"Content\Effects\Visual\Triangle-Normal\MSET\move_001.frames", FileMode.Create);
            BinaryWriter wr = new BinaryWriter(fs);

            for (int i=0;i<200;i++)
            {

                wr.Write((ushort)14);
                wr.Write((ushort)7);
                wr.Write(0f);

                wr.Write((ushort)0);
                wr.Write((ushort)1);
                wr.Write(-0.5f);

                wr.Write((ushort)3);
                wr.Write((ushort)1);
                wr.Write(2f);

                wr.Write((ushort)0);
                wr.Write((ushort)1);
                wr.Write(0.5f);

                wr.Write(-1);
            }

            wr.Close();
            fs.Close();*/

            /*FileStream fs = new FileStream(@"Content\Effects\Visual\FakeHUD\MSET\move_002.frames", FileMode.OpenOrCreate);
            BinaryWriter wr = new BinaryWriter(fs);
            for (int i = 0; i < 1; i++)
            {
                wr.Write((ushort)14);
                wr.Write((ushort)0);
                wr.Write(0f);

                wr.Write(-1);
            }

            wr.Close();
            fs.Close();
            fs.Close();*/

            /*FileStream fs = new FileStream(@"Content\Effects\Visual\FakeHUD\MSET\move_002.bin", FileMode.OpenOrCreate);
            BinaryWriter wr = new BinaryWriter(fs);
            int fCount = 1;
            wr.Write(fCount);
            wr.Write(0);
            wr.Write(0);
            wr.Write(0);

            for (int i = 0; i < fCount; i++)
            {
                Matrix m = Matrix.CreateScale(1f);
                wr.Write(m.M11);
                wr.Write(m.M12);
                wr.Write(m.M13);
                wr.Write(m.M14);
                wr.Write(m.M21);
                wr.Write(m.M22);
                wr.Write(m.M23);
                wr.Write(m.M24);
                wr.Write(m.M31);
                wr.Write(m.M32);
                wr.Write(m.M33);
                wr.Write(m.M34);
                wr.Write(m.M41);
                wr.Write(m.M42);
                wr.Write(m.M43);
                wr.Write(m.M44);

            }

            wr.Close();
            fs.Close();*/
            if (File.Exists(filename.Replace(".dae", ".info")))
            {
                string[] input = File.ReadAllLines(filename.Replace(".dae", ".info"));
                switch (input[0])
                {
                    case "ToScreen2D":
                        this.DType = DrawType.ToScreen2D;
                        break;
                    case "FullScreen":
                        this.DType = DrawType.FullScreen;
                        break;
                }
                if (input[1] == "CullNone")
                {
                    this.Cull = false;
                }
                if (input[2] == "DepthNone")
                {
                    this.Depth = false;
                }
            }
            daeFile = new DAE(filename);
            daeFile.Parse();
            RecreateVertexBuffer(true);
            //daeFile.ModelType = Model.MDType.Sky;
            //daeFile.Location = new Vector3(0, 0, 0);

            if (Directory.Exists(filename.Replace(Path.GetFileName(filename), "MSET")))
            {
                moveFile = new BinaryMoveset(filename.Replace(Path.GetFileName(filename), "MSET"));
                moveFile.Links.Add(daeFile);
                moveFile.Parse();
                moveFile.PlayingIndex = 0;
                moveFile.InterpolateAnimation = false;
                string[] frameFiles = Directory.GetFiles(filename.Replace(Path.GetFileName(filename), "MSET"), "*.frames");
                Array.Sort(frameFiles);

                Frames = new byte[frameFiles.Length][];
                FramesIndices = new List<int>[frameFiles.Length];

                for (int i=0;i< Frames.Length;i++)
                {
                    Frames[i] = File.ReadAllBytes(frameFiles[i]);
                    FramesIndices[i] = new List<int>(0);
                    FramesIndices[i].Add(0);
                    for (int j = 0; j < Frames[i].Length-4; j+=4)
                    {
                        int read = BitConverter.ToInt32(Frames[i], j);
                        if (read==-1)
                        {
                            FramesIndices[i].Add(j+4);
                        }
                    }
                }
            }
        }

        private Vector2[] UvsDefault;
        private Color[] ColorsDefault;
        public unsafe void RecreateVertexBuffer(bool first)
        {
            if (first)
            {
                UvsDefault = new Vector2[this.daeFile.VertexBufferColor.Length];
                ColorsDefault = new Color[this.daeFile.VertexBufferColor.Length];
                for (int i = 0; i < this.daeFile.VertexBufferColor.Length; i++)
                {
                    UvsDefault[i] = this.daeFile.VertexBufferColor[i].TextureCoordinate * 1f;
                    ColorsDefault[i] = this.daeFile.VertexBufferColor[i].Color * 1f;
                }
            }

            bool checkMaximums = Single.IsNaN(this.daeFile.MaxVertex.X);

            if (checkMaximums)
            {
                this.daeFile.MaxVertex.X = Single.MinValue;
                this.daeFile.MaxVertex.Y = Single.MinValue;
                this.daeFile.MaxVertex.Z = Single.MinValue;

                this.daeFile.MinVertex.X = Single.MaxValue;
                this.daeFile.MinVertex.Y = Single.MaxValue;
                this.daeFile.MinVertex.Z = Single.MaxValue;
            }
            
            Vector3 v3 = Vector3.Zero;
            Vector3 ComputingBuffer = Vector3.Zero;

            int jo4Ind = 0;

            for (int i = 0; i < this.daeFile.VertexBuffer_c.Length; i++)
            {
                jo4Ind = 0;
                v3 = Vector3.Zero;
                for (int j = 0; j < this.daeFile.VertexBuffer_c[i].Count; j += 4)
                {
                    ComputingBuffer.X = this.daeFile.VertexBuffer_c[i].Vertices[j];
                    ComputingBuffer.Y = this.daeFile.VertexBuffer_c[i].Vertices[j + 1];
                    ComputingBuffer.Z = this.daeFile.VertexBuffer_c[i].Vertices[j + 2];

                    Matrix mat = this.daeFile.Skeleton.Bones[this.daeFile.VertexBuffer_c[i].Matis[jo4Ind]].GlobalMatrix;
                    /*if (hasMaster)
                    {
                        mat *= this.Master.Skeleton.Bones[this.Master.Skeleton.LeftHandBone].GlobalMatrix;
                    }*/
                    ComputingBuffer = Vector3.Transform(ComputingBuffer, mat);
                    
                    v3 += ComputingBuffer * this.daeFile.VertexBuffer_c[i].Vertices[j + 3];
                    jo4Ind++;
                }

                if (checkMaximums)
                {
                    if (v3.X > daeFile.MaxVertex.X) daeFile.MaxVertex.X = v3.X;
                    if (v3.Y > daeFile.MaxVertex.Y) daeFile.MaxVertex.Y = v3.Y;
                    if (v3.Z > daeFile.MaxVertex.Z) daeFile.MaxVertex.Z = v3.Z;
                    if (v3.X < daeFile.MinVertex.X) daeFile.MinVertex.X = v3.X;
                    if (v3.Y < daeFile.MinVertex.Y) daeFile.MinVertex.Y = v3.Y;
                    if (v3.Z < daeFile.MinVertex.Z) daeFile.MinVertex.Z = v3.Z;
                }

                this.daeFile.VertexBufferColor[i].Position = v3;
            }
        }


        int lastFrame = -1;

        public void Finish()
        {
            lastFrame = -1;
            moveFile.PlayingIndex = 2;
        }

        public void Reset()
        {
            lastFrame = -1;
            moveFile.PlayingIndex = 0;
        }
        public bool Finishing
        {
            get
            {
                return this.moveFile.playingIndex == 2;
            }
        }
        public bool Finished = true;

        public void Update()
        {
            float read = 0;
            int unSurDeux = 0;
            ushort inst = 0;
            ushort msh = 0;
            int cursor = FramesIndices[moveFile.PlayingIndex][(int)moveFile.PlayingFrame];

            while (cursor < Frames[moveFile.PlayingIndex].Length)
            {
                read = BitConverter.ToSingle(Frames[moveFile.PlayingIndex], cursor);
                if (Single.IsNaN(read))
                    break;
                if (unSurDeux % 2 == 0)
                {
                    inst = BitConverter.ToUInt16(Frames[moveFile.PlayingIndex], cursor);
                    msh = BitConverter.ToUInt16(Frames[moveFile.PlayingIndex], cursor+2);
                }
                else
                {
                    int start = msh;
                    int end = msh+1;
                    if (msh > daeFile.MeshesOffsets.Count-1)
                    {
                        start = 0;
                        end = daeFile.MeshesOffsets.Count;
                    }
                    for (int h = start; h < end; h++)
                        for (int j = daeFile.MeshesOffsets[h][0]; j < daeFile.MeshesOffsets[h][0] + daeFile.MeshesOffsets[h][1]; j++)
                        {
                        switch (inst)
                        {
                            case 0:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate.X += read;
                            break;
                            case 1:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate.Y += read;
                            break;
                            case 2:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate =
                                    Vector2.Transform(this.daeFile.VertexBufferColor[j].TextureCoordinate, Matrix.CreateRotationZ(read));
                            break;
                            case 3:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate.X *= read;
                            break;
                            case 4:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate.Y *= read;
                            break;
                            case 5:
                                this.daeFile.VertexBufferColor[j].TextureCoordinate *= read;
                                break;
                            case 6:
                                float col = this.daeFile.VertexBufferColor[j].Color.A / 255f;
                                col *= read;
                                this.daeFile.VertexBufferColor[j].Color.A = (byte)(col * 255);
                            break;
                            case 7:
                                col = this.daeFile.VertexBufferColor[j].Color.R / 255f;
                                col *= read;
                                this.daeFile.VertexBufferColor[j].Color.R = (byte)(col * 255);
                            break;
                            case 8:
                                col = this.daeFile.VertexBufferColor[j].Color.G / 255f;
                                col *= read;
                                this.daeFile.VertexBufferColor[j].Color.G = (byte)(col * 255);
                            break;
                            case 9:
                                col = this.daeFile.VertexBufferColor[j].Color.B / 255f;
                                col *= read;
                                this.daeFile.VertexBufferColor[j].Color.B = (byte)(col * 255);
                            break;
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                                if (read < 0) read = 0;
                                if (read > 1) read = 1;

                                if (inst == 10)
                                    this.daeFile.VertexBufferColor[j].Color.A = (byte)(read * 255);
                                else if (inst == 11)
                                    this.daeFile.VertexBufferColor[j].Color.R = (byte)(read * 255);
                                else if (inst == 12)
                                    this.daeFile.VertexBufferColor[j].Color.G = (byte)(read * 255);
                                else if (inst == 13)
                                    this.daeFile.VertexBufferColor[j].Color.B = (byte)(read * 255);
                            break;
                            case 14:
                                    this.daeFile.VertexBufferColor[j].TextureCoordinate = UvsDefault[j] * 1f;
                                    this.daeFile.VertexBufferColor[j].Color = ColorsDefault[j] * 1f;
                            break;
                        }
                    }
                }
                unSurDeux++;
                cursor += 4;
            }
            

            if (lastFrame >= (int)moveFile.PlayingFrame)
            {
                if (moveFile.PlayingIndex == 0)
                    moveFile.PlayingIndex = 1;

                if (moveFile.PlayingIndex == 2)
                {
                    this.Finished = true;

                }
            }

            lastFrame = (int)moveFile.PlayingFrame;
            moveFile.GetFrameData_();

            moveFile.ComputingFrame++;
            RecreateVertexBuffer(false);

            Matrix rot = Matrix.CreateFromYawPitchRoll(Program.game.mainCamera.Yaw, Program.game.mainCamera.Pitch + MainGame.PI / 2f, 0);
            switch (this.DType)
            {
                case DrawType.ToScreen2D:
                    float scale = Vector3.Distance(this.Location + ((daeFile.MinVertex + daeFile.MaxVertex) / 2f), Program.game.mainCamera.RealPosition) / 1000f;
                    for (int i = 0; i < daeFile.VertexBufferColor.Length; i++)
                    {
                        daeFile.VertexBufferColor[i].Position = Vector3.Transform(daeFile.VertexBufferColor[i].Position, Matrix.CreateScale(scale));
                    }
                    for (int i = 0; i < daeFile.VertexBufferColor.Length; i++)
                    {
                        daeFile.VertexBufferColor[i].Position = Vector3.Transform(daeFile.VertexBufferColor[i].Position, rot);
                    }
                    break;
            }
            for (int i = 0; i < this.daeFile.VertexBuffer_c.Length; i++)
                this.daeFile.VertexBufferColor[i].Position += this.Location;


            daeFile.vBuffer.SetData<VertexPositionColorTexture>(daeFile.VertexBufferColor);
        }


        public new void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at,  BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            if (this.Depth)
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            else
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            if (this.Cull)
                gcm.GraphicsDevice.RasterizerState = rs;
            else
                gcm.GraphicsDevice.RasterizerState = rsNoCull;



            gcm.GraphicsDevice.SetVertexBuffer(daeFile.vBuffer);

            switch (this.DType)
            {
                case DrawType.FullScreen:

                    /*for (int i = 0; i < daeFile.VertexBufferColor.Length; i++)
                    {
                        daeFile.VertexBufferColor[i].Position = Vector3.Transform(daeFile.VertexBufferColor[i].Position, Matrix.CreateScale(100f));
                        daeFile.VertexBufferColor[i].Position = Vector3.Transform(daeFile.VertexBufferColor[i].Position, rot);
                        daeFile.VertexBufferColor[i].Position += Program.game.mainCamera.LookAt + Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(Program.game.mainCamera.Yaw, Program.game.mainCamera.Pitch, 0)) * -Program.game.mainCamera.Zoom * 0.99999f;
                    }*/


                    be.View = Matrix.CreateLookAt(new Vector3(0, 1, 0), Vector3.Zero, Vector3.Forward);
                    be.Projection = Matrix.CreateOrthographic(16f, 9f, 0f, 10f);

                    break;
            }

            for (int i = 0; i < daeFile.MeshesOffsets.Count; i++)
            {
                be.DiffuseColor = Color.White.ToVector3();
                be.Texture = daeFile.Textures[daeFile.MaterialIndices[i]];
                be.CurrentTechnique.Passes[0].Apply();

                gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, daeFile.MeshesOffsets[i][0], daeFile.MeshesOffsets[i][1] / 3);
            }
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gcm.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            be.View = at.View;
            be.Projection = at.Projection;
        }
    }
}
