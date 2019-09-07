using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class BinaryModel:Model
    {
        public BinaryModel(string filename)
        {
            Console.WriteLine("Loading Resource " + filename);
            this.Type = ResourceType.Model;
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;
            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"))
            {
                this.Links.Add(new Collision(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"));
            }

            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "areas.obj"))
            {
                this.Area = new Area(Path.GetDirectoryName(this.FileName) + @"\" + "areas.obj");
            }

            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "actions.txt"))
            {
                varIDs = new List<int>(0);
                varValues = new List<bool>(0);
                string[] text = File.ReadAllLines(Path.GetDirectoryName(this.FileName) + @"\" + "actions.txt");
                for (int i=0;i<text.Length;i++)
                {
                    string[] spli = text[i].Split('|');

                    if (spli[0] == "IF")
                    {
                        Action action = new Action();
                        action.parent = this;
                        action.AddCondition(text[i]);
                        while (spli[0]!="ENDIF")
                        {
                            i++;
                            spli = text[i].Split('|');
                            if (spli[0] == "ELSE" || spli[0] == "AND" || spli[0] == "OR")
                                action.AddCondition(text[i]);
                            else
                                action.AddComand(text[i]);
                        }
                        this.Actions[this.ActionsCount] = action;
                        this.ActionsCount++;
                    }
                }
            }
        }

        public unsafe void Parse()
        {
            FileStream fs = new FileStream(this.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader reader = new BinaryReader(fs);
            this.ModelType = (MDType)reader.ReadSByte();
            this.NoCull = reader.ReadSByte()<0;
            this.ZIndex = (int)reader.ReadSByte();
            reader.ReadSByte();

            if (this.ModelType == MDType.Map)
            for (int i=0;i<10;i++)
            {
                    if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae"))
                    {
                        string fname = Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae";

                        Model model = new DAE(Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae");
                        model.Parse();

                        BinaryMoveset bm = null;
                        if (Directory.Exists(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname)))
                        {
                            bm = new BinaryMoveset(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname));
                            bm.Links.Add(model);
                            bm.Parse();
                            bm.PlayingIndex = 0;
                            bm.Render = true;
                            model.Links.Add(bm);
                        }
                        this.SuppMsets.Add(bm);
                        this.Supp.Add(model);
                    }
                    if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\OBJECTS\" + this.Name + "-sp" + i.ToString("d3") + ".dae"))
                    {
                        string fname = Path.GetDirectoryName(this.FileName) + @"\OBJECTS\" + this.Name + "-sp" + i.ToString("d3") + ".dae";
                        Model dae = new DAE(fname);

                        dae.Parse();
                        dae.ModelType = MDType.Specular;
                        //(dae as DAE).ExportBin();

                        BinaryMoveset bm = null;
                        if (Directory.Exists(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname)))
                        {
                            bm = new BinaryMoveset(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname));
                            bm.Links.Add(dae);
                            bm.Parse();
                            bm.PlayingIndex = 0;
                            bm.Render = true;
                            dae.Links.Add(bm);

                        }

                        this.SuppMsets.Add(bm);
                        this.Supp.Add(dae);
                    }
            }
            if (Directory.Exists(Path.GetDirectoryName(this.FileName) + @"\OBJECTS"))
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(this.FileName) + @"\OBJECTS", "*.dae");

                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains("-sp"))
                        continue;
                    Model dae = new DAE(files[i])
                    {
                        Render = true,
                        ModelType = MDType.Sky
                    };
                    dae.Parse();
                    //(dae as DAE).ExportBin();

                    BinaryMoveset bm = null;

                    if (Directory.Exists(Path.GetDirectoryName(files[i]) + "\\" + Path.GetFileNameWithoutExtension(files[i])))
                    {
                        bm = new BinaryMoveset(Path.GetDirectoryName(files[i]) + "\\" + Path.GetFileNameWithoutExtension(files[i]));
                        bm.Links.Add(dae);
                        bm.Parse();
                        bm.PlayingIndex = 0;
                        bm.ObjectMsetRender = bm.Name.Contains("-mdl-");
                        bm.Render = true;
                        dae.Links.Add(bm);
                    }

                    this.SuppMsets.Add(bm);
                    this.Supp.Add(dae);
                }
            }
            int textureCount = reader.ReadInt32();

            this.FogStart = reader.ReadInt16()*10f;
            this.FogEnd = reader.ReadInt16()*10f;
            this.FogColor = (new Microsoft.Xna.Framework.Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte())).ToVector3();


            for (int i = 0; i < textureCount; i++)
            {
                byte length = reader.ReadByte();
                string fname = "Content\\"+Encoding.ASCII.GetString(reader.ReadBytes(length));
                this.materialFileNames.Add(fname);
                if (File.Exists(fname))
                {
                    //this.Textures.Add(Texture2D.FromStream(Program.game.GraphicsDevice, new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                    this.Textures.Add(ResourceLoader.GetT2D(fname));
                }
                //this.Textures.Add(ResourceLoader.EmptyT2D);
                if (0x30 - (length + 1) > 0)
                    reader.ReadBytes(0x30 - (length + 1));
            }
            int meshCount = reader.ReadInt32();
            reader.ReadInt32(); reader.ReadInt32(); reader.ReadInt32();

            int vertexCount = 0;

            for (int i = 0; i < meshCount; i++)
            {
                this.MaterialIndices.Add(reader.ReadInt32());
                int[] off = new int[2];
                off[0] = reader.ReadInt32();
                off[1] = reader.ReadInt32();
                this.MeshesOffsets.Add(off);
                vertexCount += off[1];
            }

            if (meshCount % 4 > 0)
                for (int i = 0; i < (4 - (meshCount % 4)); i++)
                {
                    reader.ReadInt32();
                }


            this.VertexBuffer_c = new ComputedVertex[vertexCount];
            this.VertexBufferColor = new VertexPositionColorTexture[vertexCount];
            this.VertexBufferShadow = new VertexPositionColor[this.ModelType == MDType.Human?vertexCount:0];
            //this.VertexBufferShadow2 = new VertexPositionColor[this.ModelType == MDType.Human ? 3000 : 0];
            for (int i = 0; i < this.VertexBufferShadow.Length; i++)
                VertexBufferShadow[i].Color = new Color(0, 0, 0, 0);

            //for (int i = 0; i < this.VertexBufferShadow2.Length; i++)
            //    VertexBufferShadow2[i].Color = new Color(0, 0, 0, 0);


            for (int i = 0; i < vertexCount; i++)
            {
                int infs = reader.ReadInt32();


                VertexPositionColorTexture vpct = new VertexPositionColorTexture
                {
                    TextureCoordinate = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    Position = new Vector3(0, 0, 0),
                    Color = new Microsoft.Xna.Framework.Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte())
                };
                if (!this.HasColor && (vpct.Color.R < 1 || vpct.Color.G < 1 || vpct.Color.B < 1 || vpct.Color.A < 1))
                    this.HasColor = true;


                int ind = ((i / 3) * 3 + (2 - (i % 3)));

                this.VertexBuffer_c[ind] = new ComputedVertex
                {
                    Count = infs
                };
                int ind_ = 0;

                for (int j = 0; j < infs; j+=4)
                {
                    this.VertexBuffer_c[ind].Matis[ind_] = reader.ReadInt16();
                    reader.ReadInt16();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    this.VertexBuffer_c[ind].Vertices[j] = reader.ReadSingle();
                    this.VertexBuffer_c[ind].Vertices[j+1] = reader.ReadSingle();
                    this.VertexBuffer_c[ind].Vertices[j+2] = reader.ReadSingle();
                    this.VertexBuffer_c[ind].Vertices[j+3] = reader.ReadSingle();
                    ind_++;
                }
                this.VertexBufferColor[ind] = vpct;

            }
            reader.Close();
            fs.Close();

            this.vBuffer = new VertexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), this.VertexBufferColor.Length, BufferUsage.None);

            this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);

            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel"))
                fs = new FileStream(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel", FileMode.Open, FileAccess.Read, FileShare.Read);
            else
                fs = new FileStream(@"Content\default\skeleton.skel", FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new BinaryReader(fs);
            fs.Position = 4;
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                fs.Position = 16 + i * 16;
                if (reader.ReadInt32() == 4)
                {
                    reader.ReadInt32();
                    fs.Position = reader.ReadInt32() + 0xA0;
                    break;
                }
            }
            short boneCount = reader.ReadInt16();
            reader.ReadInt16();
            fs.Position = fs.Position - 0x14 + reader.ReadInt32();

            this.Skeleton = new Skeleton
            {
                Bones = new List<Bone>(0)
            };
            List<int> parents = new List<int>(0);

            for (int i = 0; i < boneCount; i++)
            {
                Bone currBone = new Bone("bone" + reader.ReadInt32().ToString("d3"));
                parents.Add(reader.ReadInt32());
                reader.ReadInt32(); reader.ReadInt32();
                currBone.ScaleX = reader.ReadSingle();
                currBone.ScaleY = reader.ReadSingle();
                currBone.ScaleZ = reader.ReadSingle();
                reader.ReadInt32();
                currBone.RotateX = reader.ReadSingle();
                currBone.RotateY = reader.ReadSingle();
                currBone.RotateZ = reader.ReadSingle();
                reader.ReadInt32();
                currBone.TranslateX = reader.ReadSingle();
                currBone.TranslateY = reader.ReadSingle();
                currBone.TranslateZ = reader.ReadSingle();
                reader.ReadInt32();
                currBone.GlobalMatrix = Matrix.CreateScale(new Vector3(currBone.ScaleX, currBone.ScaleY,currBone.ScaleZ));
                currBone.GlobalMatrix.Translation = new Vector3(currBone.TranslateX, currBone.TranslateY, currBone.TranslateZ);

                currBone.localMatrix = currBone.GlobalMatrix;
                this.Skeleton.Bones.Add(currBone);
            }
            for (int i = 0; i < parents.Count; i++)
            {
                if (parents[i] > -1)
                    this.Skeleton.Bones[i].Parent = this.Skeleton.Bones[parents[i]];
            }
            this.Skeleton.ComputeMatrices();
            this.HeadHeight = Vector3.Transform(Vector3.Zero, this.Skeleton.Bones[this.Skeleton.HeadBone].GlobalMatrix);

            for (int i=0;i< this.Skeleton.Bones.Count;i++)
            {
                if (this.Skeleton.Bones[i].Parent==null)
                {
                    this.Skeleton.RootBone = i;
                    break;
                }
            }
            if (Single.IsNaN(this.Skeleton.ZeroPosition.X))
            this.Skeleton.ZeroPosition = this.Skeleton.Bones[this.Skeleton.RootBone].GlobalMatrix.Translation;
            
            reader.Close();
            fs.Close();
            this.RecreateVertexBuffer(true);
            this.Skeleton.MaxVertex = this.MaxVertex;
            this.Skeleton.MinVertex = this.MinVertex;

            this.GetJoints();

            if (this.Name.Contains("-sp"))
            {
                this.SpecularBuffer = new Vector2[this.VertexBufferColor.Length];
                for (int i=0;i<this.VertexBufferColor.Length;i++)
                {
                    this.SpecularBuffer[i].X = this.VertexBufferColor[i].TextureCoordinate.X;
                    this.SpecularBuffer[i].Y = this.VertexBufferColor[i].TextureCoordinate.Y;
                    AverageVertex += this.VertexBufferColor[i].Position;
                }
                AverageVertex = AverageVertex / (float)this.VertexBufferColor.Length;
            }
        }
    }
}
