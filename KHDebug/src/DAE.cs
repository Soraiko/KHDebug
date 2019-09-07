using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class DAE : Model
    {
        public XmlNodeList images;
        public XmlNodeList materials;
        public XmlNodeList effects;
        public XmlNodeList geometries;
        public XmlNodeList controllers;
        public XmlNodeList visual_scenes;
        public XmlNodeList[] surfaces;
        public XmlNodeList[] joints;

        public static DAE SampleDAE;
        public static bool SampleLoaded;

        XmlDocument Document;
        public string Directory;

        static XmlNode sampleImage;
        static XmlNode sampleMaterial;
        static XmlNode sampleEffect;
        static XmlNode sampleGeomerty;
        static XmlNode sampleController;
        static XmlNode sampleScene;
        static XmlNode sampleSurface;
        static XmlNode sampleJoint;



        public DAE(string filename)
        {
            Console.WriteLine("Loading Resource " + filename);
            this.Skeleton = new Skeleton
            {
                Bones = new List<Bone>(0)
            };
            this.Type = ResourceType.DaeModel;
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;
            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"))
            {
                this.Links.Add(new Collision(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"));
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

            filename = filename.Replace("/", "\\");
            if (!filename.Contains(":\\") && File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\" + filename))
            {
                filename = System.IO.Directory.GetCurrentDirectory() + "\\" + filename;
            }

            if (!SampleLoaded)
            {
                SampleLoaded = true;
                if (!File.Exists("Content\\sample.dae"))
                    throw new Exception("The sample file does not exist. Make sure that the file sample.dae is present near the executable.");
                SampleDAE = new DAE("Content\\sample.dae");
                SampleDAE.GiveUntitledName();

                sampleImage = SampleDAE.images[0];
                sampleImage.ParentNode.RemoveChild(sampleImage);

                sampleMaterial = SampleDAE.materials[0];
                sampleMaterial.ParentNode.RemoveChild(sampleMaterial);

                sampleEffect = SampleDAE.effects[0];
                sampleEffect.ParentNode.RemoveChild(sampleEffect);

                sampleGeomerty = SampleDAE.geometries[0];
                sampleGeomerty.ParentNode.RemoveChild(sampleGeomerty);

                sampleController = SampleDAE.controllers[0];
                sampleController.ParentNode.RemoveChild(sampleController);

                sampleScene = SampleDAE.visual_scenes[0];
                sampleScene.ParentNode.RemoveChild(sampleScene);

                sampleJoint = sampleScene.SelectNodes("//node[translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']")[0];
                sampleJoint.ParentNode.RemoveChild(sampleJoint);

                sampleSurface = sampleScene.SelectNodes("//node")[0];
                sampleSurface.ParentNode.RemoveChild(sampleSurface);

                //SampleDAE.Export(SampleDAE.Name+".dae");
            }

            if (filename.Length == 0)
            {
                this.GiveUntitledName();
                this.Document = new XmlDocument();
                this.Document.LoadXml(SampleDAE.Document.OuterXml);
                return;
            }

            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.Directory = Path.GetDirectoryName(filename);

            byte[] fileData = File.ReadAllBytes(filename);

            for (int i = 0; i < 500; i++)
            {
                if (fileData[i + 0] != 0x78) continue;
                if (fileData[i + 1] != 0x6D) continue;
                if (fileData[i + 2] != 0x6C) continue;
                if (fileData[i + 3] != 0x6E) continue;
                if (fileData[i + 4] != 0x73) continue;
                fileData[i + 0] = 0x77;
            }

            this.Document = new XmlDocument();
            this.Document.LoadXml(Encoding.ASCII.GetString(fileData));

            this.images = this.Document.SelectNodes("//library_images/image");
            this.materials = this.Document.SelectNodes("//library_materials/material");
            this.effects = this.Document.SelectNodes("//library_effects/effect");
            this.geometries = this.Document.SelectNodes("//library_geometries/geometry");
            this.controllers = this.Document.SelectNodes("//library_controllers/controller");
            this.visual_scenes = this.Document.SelectNodes("//library_visual_scenes/visual_scene");
            this.surfaces = new XmlNodeList[this.visual_scenes.Count];
            this.joints = new XmlNodeList[this.visual_scenes.Count];

            for (int i = 0; i < this.visual_scenes.Count; i++)
            {
                this.joints[i] = this.visual_scenes[i].SelectNodes("descendant::node[translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");
                this.surfaces[i] = this.visual_scenes[i].SelectNodes("node[not(@type)]");
            }

            this.ImagesIDs = new List<string>(0);
            this.ImagesFilenames = new List<string>(0);

            this.PerGeometryMaterials = new List<string>(0);
            this.MaterialsIDs = new List<string>(0);
            this.MaterialsEffectIDs = new List<string>(0);
            this.EffectsIDs = new List<string>(0);
            this.EffectsImageIDs = new List<string>(0);
            this.GeometryIDs = new List<string>(0);

            this.GeometryDataVertex = new List<Vector3[]>(0);
            this.GeometryDataTexcoordinates = new List<Vector2[]>(0);
            this.GeometryDataNormals = new List<Vector3[]>(0);
            this.GeometryDataColors = new List<Color[]>(0);

            this.GeometryDataVertex_i = new List<List<int>>(0);
            this.GeometryDataTexcoordinates_i = new List<List<int>>(0);
            this.GeometryDataNormals_i = new List<List<int>>(0);
            this.GeometryDataColors_i = new List<List<int>>(0);

            this.ControllerDataJoints_i = new List<List<List<int>>>(0);
            this.ControllerDataMatrices_i = new List<List<List<int>>>(0);
            this.ControllerDataWeights_i = new List<List<List<int>>>(0);

            this.ControllersIDs = new List<string>(0);
            this.PerControllerGeometry = new List<string>(0);

            this.ShapeMatrices = new List<Matrix>(0);
            this.PerGeometryTexturesFNames = new List<string>(0);
            this.ControllerDataJoints = new List<string[]>(0);
            this.ControllerDataMatrices = new List<Matrix[]>(0);
            this.ControllerDataWeights = new List<float[]>(0);
            this.VisualScenesIDs = new List<string>(0);
            this.Joints = new List<Joint>(0);
            this.JointsIDs = new List<List<string>>(0);
            this.JointsMatrices = new List<List<Matrix>>(0);
            this.SurfacesIDs = new List<List<string>>(0);
            this.SurfacesMaterialsID = new List<List<string>>(0);
        }
        
        /*public void Draw()
        {
            if (this.RendererGame == null)
                throw new InvalidOperationException("You must set a renderer for this DAE file before to be able to display it.");

            this.RendererGame.GraphicsDevice.RasterizerState = this.RendererGame.rasterizerState;
            for (int i = 0; i < this.GeometryIDs.Count; i++)
            {
                this.RendererGame.basicEffect.Texture = PerGeometryTextures[i];
                this.RendererGame.basicEffect.CurrentTechnique.Passes[0].Apply();

                this.RendererGame.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, this.vBufferOffsets[i], this.vBufferCounts[i]);
                //this.RendererGame.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.VertexBufferColor, this.vBufferOffsets[i], this.vBufferCounts[i] / 3);
            }
        }*/
        
        unsafe void GetBuffer()
        {
            
            int totalVertexCount = 0;
            this.MeshesOffsets = new List<int[]>(0);


            for (int i = 0; i < this.GeometryDataVertex_i.Count; i++)
            {
                this.MeshesOffsets.Add(new int[] { totalVertexCount, this.GeometryDataVertex_i[i].Count });
                totalVertexCount += this.GeometryDataVertex_i[i].Count;
            }
            this.VertexBufferColor = new VertexPositionColorTexture[totalVertexCount];
            this.VertexBuffer_c = new ComputedVertex[totalVertexCount];




            this.VertexBufferShadow = new VertexPositionColor[this.ModelType == MDType.Human ? totalVertexCount : 0];
            //this.VertexBufferShadow2 = new VertexPositionColor[this.ModelType == MDType.Human ? 3000 : 0];

            for (int i = 0; i < this.VertexBufferShadow.Length; i++)
            {
                VertexBufferShadow[i].Color = new Microsoft.Xna.Framework.Color(0, 0, 0, 0);
            }

            //for (int i = 0; i < this.VertexBufferShadow2.Length; i++)
            //    VertexBufferShadow2[i].Color = new Color(0, 0, 0, 0);

            int vIndex = 0;




            for (int i = 0; i < this.GeometryDataVertex_i.Count; i++)
            {
                int controllerIndex = this.PerControllerGeometry.IndexOf(this.GeometryIDs[i]);
                for (int j = 0; j < this.GeometryDataVertex_i[i].Count; j++)
                {

                    int vertexIndex = this.GeometryDataVertex_i[i][j];
                    int texcoordIndex = -1;
                    int colorIndex = -1;
                    int normalIndex = -1;

                    if (i < this.GeometryDataTexcoordinates_i.Count && j < this.GeometryDataTexcoordinates_i[i].Count)
                        texcoordIndex = this.GeometryDataTexcoordinates_i[i][j];
                    if (i < this.GeometryDataColors_i.Count && j < this.GeometryDataColors_i[i].Count)
                        colorIndex = this.GeometryDataColors_i[i][j];
                    if (i < this.GeometryDataNormals_i.Count && j < this.GeometryDataNormals_i[i].Count)
                        normalIndex = this.GeometryDataNormals_i[i][j];

                    Vector3 position = this.GeometryDataVertex[i][vertexIndex];


                    ComputedVertex cv = new ComputedVertex
                    {
                        Count = this.ControllerDataWeights_i[controllerIndex][vertexIndex].Count * 4
                    };

                    for (int m = 0; m < this.ControllerDataJoints_i[controllerIndex][vertexIndex].Count; m++)
                    {
                        string joint = this.ControllerDataJoints[controllerIndex][this.ControllerDataJoints_i[controllerIndex][vertexIndex][m]];
                        float influence = this.ControllerDataWeights[controllerIndex][this.ControllerDataWeights_i[controllerIndex][vertexIndex][m]];
                        Matrix matrix = this.ControllerDataMatrices[controllerIndex][this.ControllerDataMatrices_i[controllerIndex][vertexIndex][m]];



                        Vector3 v3 = Vector3.Transform(position, matrix);
                        cv.Vertices[m * 4] = v3.X;
                        cv.Vertices[m * 4 + 1] = v3.Y;
                        cv.Vertices[m * 4 + 2] = v3.Z;
                        cv.Vertices[m * 4 + 3] = influence;
                        for (short n = 0; n < this.Joints.Count; n++)
                        {
                            if (this.Joints[n].Name == joint)
                            {
                                cv.Matis[m] = n;
                                break;
                            }
                        }
                    }

                    int ind = ((vIndex / 3) * 3 + (2 - (vIndex % 3)));

                    this.VertexBuffer_c[ind] = cv;
                    this.VertexBufferColor[ind].Position = position;
                    //this.VertexBufferNormal[vIndex].Position = position;



                    if (texcoordIndex > -1)
                    {
                        this.VertexBufferColor[ind].TextureCoordinate = this.GeometryDataTexcoordinates[i][texcoordIndex];
                        //this.VertexBufferNormal[vIndex].TextureCoordinate = this.GeometryDataTexcoordinates[i][texcoordIndex];
                    }
                    else
                    {
                        this.VertexBufferColor[ind].TextureCoordinate = Vector2.Zero;
                        //this.VertexBufferNormal[vIndex].TextureCoordinate = Vector2.Zero;
                    }

                    if (colorIndex > -1)
                        this.VertexBufferColor[ind].Color = this.GeometryDataColors[i][colorIndex];
                    else
                        this.VertexBufferColor[ind].Color = Color.White;

                    /*if (normalIndex > -1)
                        this.VertexBufferNormal[vIndex].Normal = this.GeometryDataNormals[i][normalIndex];
                    else
                        this.VertexBufferNormal[vIndex].Normal = Vector3.Zero;*/

                    vIndex++;
                }
            }

            this.vBuffer = new VertexBuffer(Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), this.VertexBufferColor.Length, BufferUsage.None);
            this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);

            if (this.Name.Contains("-sp"))
            {
                this.SpecularBuffer = new Vector2[this.VertexBufferColor.Length];
                for (int i = 0; i < this.VertexBufferColor.Length; i++)
                {
                    this.SpecularBuffer[i].X = this.VertexBufferColor[i].TextureCoordinate.X;
                    this.SpecularBuffer[i].Y = this.VertexBufferColor[i].TextureCoordinate.Y;
                    AverageVertex += this.VertexBufferColor[i].Position;
                }
                AverageVertex = AverageVertex / (float)this.VertexBufferColor.Length;
            }



            this.GetJoints();

            this.Skeleton.MaxVertex = this.MaxVertex;
            this.Skeleton.MinVertex = this.MinVertex;
            this.HeadHeight = Vector3.Transform(Vector3.Zero, this.Skeleton.Bones[this.Skeleton.HeadBone].GlobalMatrix) /*+ new Vector3(0, 40, 0)*/;

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
        }

        public enum DisplayMode
        {
            Normal = 0,
            Color = 1
        }

        public class Joint
        {
            public Matrix ComputedMatrix;
            public Matrix Matrix;
            public Joint Parent;
            public List<Joint> Children;
            public bool Computed;

            public string Name;
            public Joint(string name)
            {
                this.Name = name;
                this.Children = new List<Joint>(0);
                this.Computed = false;
            }
        }

        public void Compute(Joint joint)
        {
            if (joint.Parent != null)
                joint.ComputedMatrix *= joint.Parent.ComputedMatrix;
            for (int i = 0; i < joint.Children.Count; i++)
            {
                Compute(joint.Children[i]);
            }
        }

        public List<Joint> Joints;
        public new void Parse()
        {
            /*List<string> filenames = new List<string>(0);
            List<string> imageIDs = new List<string>(0);

            List<string> fromImageID = new List<string>(0);
            List<string> toImageID = new List<string>(0);

            var cols = Document.SelectNodes("//library_images//image[@id]");

            for (int i = 0; i < cols.Count; i++)
            {
                try
                {
                    string filename = Path.GetFileName(cols[i].ChildNodes[0].InnerText);
                    string imageID = cols[i].Attributes["id"].InnerText;

                    int index = filenames.IndexOf(filename);
                    if (index < 0)
                    {
                        filenames.Add(filename);
                        imageIDs.Add(imageID);
                    }
                    else
                    {
                        cols[i].ParentNode.RemoveChild(cols[i]);
                        fromImageID.Add(imageID);
                        toImageID.Add(imageIDs[index]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("");
                }

            }
            List<string> toeIDs = new List<string>(0);





            for (int i = 0; i < toImageID.Count; i++)
            {
                var imagd = Document.SelectNodes("//library_effects/effect[@id]//*[@texture='" + toImageID[i] + "']");

                if (imagd.Count > 0)
                {
                    var node = imagd[0];
                    while (node.Name != "effect")
                        node = node.ParentNode;
                    toeIDs.Add(node.Attributes["id"].InnerText);
                }
            }
            List<string> fromeIDs = new List<string>(0);


            List<string> matsFrom = new List<string>(0);
            List<string> matsTo = new List<string>(0);


            for (int i = 0; i < fromImageID.Count; i++)
            {
                var imagd = Document.SelectNodes("//library_effects/effect[@id]//*[@texture='" + fromImageID[i] + "']");

                if (imagd.Count > 0)
                {
                    var node = imagd[0];
                    while (node.Name != "effect")
                        node = node.ParentNode;

                    XmlNodeList mat = Document.SelectNodes("//library_materials/material/instance_effect[@url='#" + node.Attributes["id"].InnerText + "']");
                    if (mat.Count > 0)
                    {
                        matsFrom.Add(mat[0].ParentNode.Attributes["id"].InnerText);
                        mat[0].ParentNode.ParentNode.RemoveChild(mat[0].ParentNode);
                    }
                    XmlNodeList mat2 = Document.SelectNodes("//library_materials/material/instance_effect[@url='#" + toeIDs[fromeIDs.Count] + "']");
                    if (mat2.Count > 0)
                    {
                        matsTo.Add(mat2[0].ParentNode.Attributes["id"].InnerText);
                    }
                    fromeIDs.Add(node.Attributes["id"].InnerText);
                    node.ParentNode.RemoveChild(node);
                }
            }

            string outpt = Document.InnerXml;
            for (int i = 0; i < matsFrom.Count; i++)
            {
                outpt = outpt.Replace("\"" + matsFrom[i] + "\"", "\"" + matsTo[i] + "\"");
                outpt = outpt.Replace("\"#" + matsFrom[i] + "\"", "\"#" + matsTo[i] + "\"");
            }*/
            /*FileStream mStream = new FileStream(@"F:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\combined\TT082.dae", FileMode.OpenOrCreate);
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.ASCII);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = '	';
            this.Document.InnerXml = outpt;
            this.Document.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();

            writer.Close();
            mStream.Close();*/

            /*List<System.Drawing.Bitmap> bmps = new List<System.Drawing.Bitmap>(0);
            var cols_ = Document.SelectNodes("//image/init_from");

            for (int j = 0; j < cols_.Count; j++)
            {
                string textureName = cols_[j].InnerText.Remove(0,7);
                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(textureName);
                bool create = true;
                for (int k=0;k< bmps.Count;k++)
                {
                    if (bmps[k].Width == bmp.Width && bmps[k].Height== bmp.Height)
                    {
                        bool same = true;
                        for (int x=0;x< bmps[k].Width;x++)
                        for (int y=0;y< bmps[k].Height;y++)
                            {
                                System.Drawing.Color currCol = bmps[k].GetPixel(x, y);
                                System.Drawing.Color bmpCol = bmp.GetPixel(x, y);
                                if (currCol.R != bmpCol.R || currCol.G != bmpCol.G || currCol.B != bmpCol.B)
                                {
                                    same = false;
                                    break;
                                }

                            }
                        if (same)
                        {
                            create = false;
                            cols_[j].InnerText = @"texture" + k.ToString("d3") + ".png";
                            break;
                        }
                    }
                }
                if (create)
                {
                    cols_[j].InnerText = @"texture" + bmps.Count.ToString("d3") + ".png";
                    bmp.Save(@"F:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\new2\texture" + bmps.Count.ToString("d3") + ".png");
                    bmps.Add(bmp);
                }
            }*/

            /*var cols = Document.SelectNodes("//source[contains(@id,'oly') and contains(@id,'COLOR')]/float_array");
            for (int j = 0; j < cols.Count; j++)
            {
                string[] split_ = cols[j].InnerText.Split('\n');
                string output = "";
                int count1 = 0;
                int count2 = 0;
                for (int i = 0; i < split_.Length; i++)
                {
                    string[] spli = split_[i].Split(' ');
                    if (spli.Length > 3)
                    {
                        Single val1 = MainGame.SingleParse(spli[0]);// * 0.92841754927434920048713788903984f;
                        Single val2 = MainGame.SingleParse(spli[1]);// * 0.77952510261049787454527205006687f;
                        Single val3 = MainGame.SingleParse(spli[2]);// * 0.6891708342657306265189402415733f;
                        if (val1 < 0.5f / 255f && val2 < 0.5f / 255f && val3 < 0.5f / 255f)
                        {
                            count1++;
                        }
                        count2++;
                    }
                }
                
                for (int i = 0; i < split_.Length; i++)
                {
                    string[] spli = split_[i].Split(' ');
                    if (spli.Length > 3)
                    {
                        Single val1 = MainGame.SingleParse(spli[0]) * 0.92841754927434920048713788903984f;
                        Single val2 = MainGame.SingleParse(spli[1]) * 0.77952510261049787454527205006687f;
                        Single val3 = MainGame.SingleParse(spli[2]) * 0.6891708342657306265189402415733f;
                        Single val4 = MainGame.SingleParse(spli[3]);
                        val4 = (count1 == count2)  ? 0.333333f : 1f;
                        output += val1.ToString("0.000000") + " " + val2.ToString("0.000000") + " " + val3.ToString("0.000000") + " " + val4.ToString("0.000000") + "\r\n";
                    }
                }

                cols[j].InnerText = output;
            }
            FileStream mStream = new FileStream(@"Content\Models\TT08\tokamkeRed2.dae", FileMode.OpenOrCreate);
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.ASCII);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = '	';
            this.Document.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();

            writer.Close();
            mStream.Close();*/


            if (File.Exists(this.FileName.Replace(Path.GetExtension(this.FileName), "-type.txt")))
            {
                string[] str = File.ReadAllLines(this.FileName.Replace(Path.GetExtension(this.FileName), "-type.txt"));
                if (str.Length > 0)
                    this.ModelType = (MDType)int.Parse(str[0]);
                if (str.Length > 1)
                    this.NoCull = Boolean.Parse(str[1]);
                if (str.Length > 2)
                    this.ZIndex = int.Parse(str[2]);
            }
            List<string> toFixGeo = new List<string>(0);
            List<Vector4> toFixGeoRects = new List<Vector4>(0);

            int maxOffset = -1;
            float currVal = 0;
            int valCount = 0;
            int valIndex = 0;
            char separator = ' ';

            #region Parsing Joints
            XmlNodeList joints = this.Document.SelectNodes("//library_visual_scenes//node[translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");

            for (int i = 0; i < joints.Count; i++)
            {
                string jointName = joints[i].Attributes["name"].Value;
                Joint joint = new Joint(jointName)
                {
                    Matrix = this.ParseMatrices(joints[i].SelectNodes("matrix")[0].InnerText, 1)[0]
                };
                joint.ComputedMatrix = joint.Matrix * 1f;
                this.Joints.Add(joint);
            }
            for (int i = 0; i < joints.Count; i++)
            {
                XmlNode parent = joints[i].ParentNode;
                if (parent.Attributes["type"] != null && parent.Attributes["type"].Value.ToLower() == "joint")
                {
                    string jointName = joints[i].Attributes["name"].Value;
                    string parentJointName = parent.Attributes["name"].Value;
                    for (int c = 0; c < this.Joints.Count; c++)
                    {
                        if (this.Joints[c].Name ==  jointName)
                        {
                            for (int p = 0; p < this.Joints.Count; p++)
                            {
                                if (this.Joints[p].Name ==  parentJointName)
                                {
                                    this.Joints[p].Children.Add(this.Joints[c]);
                                    this.Joints[c].Parent = this.Joints[p];
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            /* Computing Joints */

            for (int i = 0; i < this.Joints.Count; i++)
            {
                if (this.Joints[i].Parent == null)
                {
                    Compute(this.Joints[i]);
                }
            }

            for (int i = 0; i < this.Joints.Count; i++)
            {
                Bone b = new Bone(this.Joints[i].Name)
                {
                    localMatrix = (this.Joints[i].Matrix),
                    rememberMatrix = (this.Joints[i].Matrix),

                    GlobalMatrix = this.Joints[i].ComputedMatrix
                };
                this.Skeleton.Bones.Add(b);
            }
            for (int i = 0; i < this.Joints.Count; i++)
            {
                if (this.Joints[i].Parent != null)
                    for (int j = 0; j < this.Joints.Count; j++)
                    {
                        if (this.Joints[j] == this.Joints[i].Parent)
                        {
                            this.Skeleton.Bones[i].Parent = this.Skeleton.Bones[j];
                        }
                    }
            }

            #endregion

            #region Parsing Images
            for (int i = 0; i < this.images.Count; i++)
            {
                var initFromNode = this.images[i].SelectNodes("init_from");
                if (initFromNode.Count > 0)
                {
                    Uri uri;
                    string path = initFromNode[0].InnerText;
                    string test = this.Directory + "\\" + path.Replace("/", "\\");
                    if (File.Exists(test))
                    {
                        path = "file://" + test;
                    }
                    if (Uri.TryCreate(path, UriKind.Absolute, out uri))
                    {
                        path = uri.LocalPath;
                    }
                    path = path.Replace(this.Directory + "\\", "");

                    if (!File.Exists(this.Directory + "\\" + path))
                        path = Path.GetFileName(path);
                    this.ImagesIDs.Add(this.images[i].Attributes["id"].Value);
                    this.ImagesFilenames.Add(path);
                }
            }
            #endregion

            #region Parsing Materials
            for (int i = 0; i < this.materials.Count; i++)
            {
                var instanceEffectNode = this.materials[i].SelectNodes("instance_effect");
                if (instanceEffectNode.Count > 0)
                {
                    string url = instanceEffectNode[0].Attributes["url"].Value;
                    if (url.Length > 0 && url[0] == '#')
                        url = url.Remove(0, 1);

                    this.MaterialsEffectIDs.Add(url);
                }
                else
                {
                    this.MaterialsEffectIDs.Add("");
                }
                this.MaterialsIDs.Add(this.materials[i].Attributes["id"].Value);
            }
            #endregion

            #region Parsing Effects
            for (int i = 0; i < this.effects.Count; i++)
            {
                var textureNode = this.effects[i].SelectNodes("descendant::texture");
                if (textureNode.Count > 0)
                {
                    this.EffectsImageIDs.Add(textureNode[0].Attributes["texture"].Value);
                }
                else
                {
                    this.EffectsImageIDs.Add("");
                }
                this.EffectsIDs.Add(this.effects[i].Attributes["id"].Value);
            }
            #endregion

            #region Parsing Geometries
            for (int i = 0; i < this.geometries.Count; i++)
            {
                string position_SourceID = "";
                string normal_SourceID = "";
                string texcoord_SourceID = "";
                string color_SourceID = "";

                int position_SourceOffset = 0;
                int normal_SourceOffset = -1;
                int texcoord_SourceOffset = -1;
                int color_SourceOffset = -1;

                var verticesNode = this.geometries[i].SelectNodes("descendant::vertices");
                var trianglesNode = this.geometries[i].SelectNodes("descendant::triangles");
                int countTri = int.Parse(trianglesNode[0].Attributes["count"].Value);

                if (trianglesNode.Count>0)
                {
                    var rootPNode = trianglesNode[0].SelectNodes("descendant::p")[0];
                    for (int t=1;t< trianglesNode.Count;t++)
                    {
                        int count_ = int.Parse(trianglesNode[t].Attributes["count"].Value);
                        string inner = trianglesNode[t].SelectNodes("descendant::p")[0].InnerText;
                        while (inner[0]==separator)
                        {
                            inner = inner.Remove(0, 1);
                        }
                        inner = " " + inner;
                        rootPNode.InnerText += inner;
                        countTri += count_;
                    }
                }
                this.PerGeometryMaterials.Add(trianglesNode[0].Attributes["material"].Value);

                var vertexSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='vertex']");
                if (vertexSemanticNode.Count > 0)
                {
                    var offsetAttribute = vertexSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        position_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vertexSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        position_SourceID = sourceID_Attribute.Value;
                }
                if (verticesNode.Count > 0)
                {
                    position_SourceID = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='position']")[0].Attributes["source"].Value;

                    var normal_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='normal']");
                    if (normal_SourceNode.Count > 0)
                    {
                        normal_SourceID = normal_SourceNode[0].Attributes["source"].Value;
                        normal_SourceOffset = position_SourceOffset;
                    }

                    var texcoord_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='texcoord']");
                    if (texcoord_SourceNode.Count > 0)
                    {
                        texcoord_SourceID = texcoord_SourceNode[0].Attributes["source"].Value;
                        texcoord_SourceOffset = position_SourceOffset;
                    }

                    var color_SourceNode = verticesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='color']");
                    if (color_SourceNode.Count > 0)
                    {
                        color_SourceID = color_SourceNode[0].Attributes["source"].Value;
                        color_SourceOffset = position_SourceOffset;
                    }
                }

                var texcoordSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='texcoord']");
                if (texcoordSemanticNode.Count > 0)
                {
                    var offsetAttribute = texcoordSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        texcoord_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = texcoordSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        texcoord_SourceID = sourceID_Attribute.Value;
                }

                var normalSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='normal']");
                if (normalSemanticNode.Count > 0)
                {
                    var offsetAttribute = normalSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        normal_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = normalSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        normal_SourceID = sourceID_Attribute.Value;
                }

                var colorSemanticNode = trianglesNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='color']");
                if (colorSemanticNode.Count > 0)
                {
                    var offsetAttribute = colorSemanticNode[0].Attributes["offset"];
                    if (offsetAttribute != null)
                        color_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = colorSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        color_SourceID = sourceID_Attribute.Value;
                }


                if (position_SourceID.Length > 0 && position_SourceID[0] == '#')
                    position_SourceID = position_SourceID.Remove(0, 1);
                if (normal_SourceID.Length > 0 && normal_SourceID[0] == '#')
                    normal_SourceID = normal_SourceID.Remove(0, 1);
                if (texcoord_SourceID.Length > 0 && texcoord_SourceID[0] == '#')
                    texcoord_SourceID = texcoord_SourceID.Remove(0, 1);
                if (color_SourceID.Length > 0 && color_SourceID[0] == '#')
                    color_SourceID = color_SourceID.Remove(0, 1);

                Vector3[] Vertices = new Vector3[0];
                Vector2[] TexCoordinates = new Vector2[0];
                Vector3[] Normals = new Vector3[0];
                Color[] Colors = new Color[0];

                #region Parsing POSITION-Array
                XmlNode source = this.geometries[i].SelectNodes("descendant::source[@id='" + position_SourceID + "']")[0];
                XmlNode accessor = source.SelectNodes("descendant::accessor")[0];
                int count = int.Parse(accessor.Attributes["count"].Value);
                string floatArray = source.SelectNodes("float_array")[0].InnerText;

                for (int j = 2; j < floatArray.Length && j < 20; j++)
                {
                    if (floatArray[j] == 9 ||
                        floatArray[j] == 32 ||
                        floatArray[j] == 160)
                    {
                        separator = floatArray[j];
                        break;
                    }
                }

                Vertices = new Vector3[count];
                string[] split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                valIndex = 0;

                for (int j = 0; j < split.Length; j++)
                {
                    if (Single.TryParse(split[j], out currVal))
                    {
                        if (valCount % 3 == 0)
                        {
                            Vertices[valIndex].X = currVal;
                        }
                        if (valCount % 3 == 1)
                        {
                            Vertices[valIndex].Y = currVal;
                        }
                        if (valCount % 3 == 2)
                        {
                            Vertices[valIndex].Z = currVal;
                            valIndex++;
                        }
                        valCount++;
                    }
                }
                #endregion

                #region Parsing TEXCOORD-Array
                if (texcoord_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + texcoord_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    TexCoordinates = new Vector2[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 2 == 0)
                            {
                                TexCoordinates[valIndex].X = currVal;
                            }
                            if (valCount % 2 == 1)
                            {
                                TexCoordinates[valIndex].Y = 1 -currVal;
                                //currVal = 1 - currVal;
                                valIndex++;
                            }
                            valCount++;
                        }
                    }
                }
                #endregion

                #region Parsing NORMAL-Array
                if (normal_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + normal_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Normals = new Vector3[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 3 == 0)
                            {
                                Normals[valIndex].X = currVal;
                            }
                            if (valCount % 3 == 1)
                            {
                                Normals[valIndex].Y = currVal;
                            }
                            if (valCount % 3 == 2)
                            {
                                Normals[valIndex].Z = currVal;
                                valIndex++;
                            }
                            valCount++;
                        }
                    }
                }
                #endregion

                Vector4 currV4 = Vector4.Zero;

                #region Parsing COLOR-Array
                if (color_SourceOffset > -1)
                {
                    source = this.geometries[i].SelectNodes("descendant::source[@id='" + color_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);
                    floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Colors = new Color[count];
                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                    currVal = 0;
                    valCount = 0;
                    valIndex = 0;

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            if (valCount % 4 == 0)
                            {
                                currV4.X = currVal;
                            }
                            if (valCount % 4 == 1)
                            {
                                currV4.Y = currVal;
                            }
                            if (valCount % 4 == 2)
                            {
                                currV4.Z = currVal;
                            }
                            if (valCount % 4 == 3)
                            {
                                currV4.W = currVal;
                                Colors[valIndex] = new Color(currV4);
                                valIndex++;
                            }
                            if (!this.HasColor && (currVal < 1 || currVal < 1 || currVal < 1 || currVal < 1))
                                this.HasColor = true;
                            valCount++;
                        }
                    }
                }
                #endregion

                #region Parsing Triangles-Array
                string TriangleIndices = trianglesNode[0].SelectNodes("p")[0].InnerText;
                split = TriangleIndices.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                maxOffset = -1;
                if (position_SourceOffset > maxOffset)
                    maxOffset = position_SourceOffset;
                if (texcoord_SourceOffset > maxOffset)
                    maxOffset = texcoord_SourceOffset;
                if (normal_SourceOffset > maxOffset)
                    maxOffset = normal_SourceOffset;
                if (color_SourceOffset > maxOffset)
                    maxOffset = color_SourceOffset;
                maxOffset++;
                valCount = 0;
                int currInt = 0;
                int[] indicesOrdered = new int[maxOffset];


                this.GeometryDataVertex.Add(Vertices);
                this.GeometryDataTexcoordinates.Add(TexCoordinates);
                this.GeometryDataNormals.Add(Normals);
                this.GeometryDataColors.Add(Colors);

                this.GeometryDataVertex_i.Add(new List<int>(0));
                this.GeometryDataTexcoordinates_i.Add(new List<int>(0));
                this.GeometryDataNormals_i.Add(new List<int>(0));
                this.GeometryDataColors_i.Add(new List<int>(0));

                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        if (valCount % maxOffset == position_SourceOffset)
                        {
                            this.GeometryDataVertex_i[this.GeometryDataVertex.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == texcoord_SourceOffset)
                        {
                            this.GeometryDataTexcoordinates_i[this.GeometryDataTexcoordinates.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == normal_SourceOffset)
                        {
                            this.GeometryDataNormals_i[this.GeometryDataNormals.Count - 1].Add(currInt);
                        }
                        if (valCount % maxOffset == color_SourceOffset)
                        {
                            this.GeometryDataColors_i[this.GeometryDataColors.Count - 1].Add(currInt);
                        }
                        valCount++;
                    }
                }
                #endregion

                this.GeometryIDs.Add(this.geometries[i].Attributes["id"].Value);
            }
            #endregion

            #region Parse Controllers

            for (int i = 0; i < this.controllers.Count; i++)
            {
                Matrix shapeMatrix = Matrix.Identity;

                string joints_SourceID = "";
                string matrices_SourceID = "";
                string weights_SourceID = "";

                int joints_SourceOffset = -1;
                int matrices_SourceOffset = -1;
                int weights_SourceOffset = -1;

                var shapeMatrixNode = this.controllers[i].SelectNodes("descendant::skin/bind_shape_matrix");
                if (shapeMatrixNode.Count > 0)
                    shapeMatrix = Matrix.Identity;// ParseMatrices(shapeMatrixNode[0].InnerText, 1)[0];

                var jointsNode = this.controllers[i].SelectNodes("descendant::joints");
                var vertexWeightsNode = this.controllers[i].SelectNodes("descendant::vertex_weights");

                var vwJointSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");
                if (vwJointSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwJointSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        joints_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwJointSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        joints_SourceID = sourceID_Attribute.Value;
                }

                if (jointsNode.Count > 0)
                {
                    var jointSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='joint']");
                    if (jointSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = jointSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                            joints_SourceID = sourceID_Attribute.Value;
                    }

                    var weightSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='weight']");
                    if (weightSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = weightSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                        {
                            weights_SourceID = sourceID_Attribute.Value;
                            weights_SourceOffset = joints_SourceOffset;
                        }
                    }

                    var matriceSemanticNode = jointsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='inv_bind_matrix']");
                    if (matriceSemanticNode.Count > 0)
                    {
                        var sourceID_Attribute = matriceSemanticNode[0].Attributes["source"];
                        if (sourceID_Attribute != null)
                        {
                            matrices_SourceID = sourceID_Attribute.Value;
                            matrices_SourceOffset = joints_SourceOffset;
                        }
                    }
                }

                var vwWeightSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='weight']");
                if (vwWeightSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwWeightSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        weights_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwWeightSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        weights_SourceID = sourceID_Attribute.Value;
                }

                var vwMatrixSemanticNode = vertexWeightsNode[0].SelectNodes("input[translate(@semantic, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='inv_bind_matrix']");
                if (vwMatrixSemanticNode.Count > 0)
                {
                    var offsetAttribute = vwMatrixSemanticNode[0].Attributes["offset"];

                    if (offsetAttribute != null)
                        matrices_SourceOffset = int.Parse(offsetAttribute.Value);

                    var sourceID_Attribute = vwMatrixSemanticNode[0].Attributes["source"];
                    if (sourceID_Attribute != null)
                        matrices_SourceID = sourceID_Attribute.Value;
                }

                if (joints_SourceID.Length > 0 && joints_SourceID[0] == '#')
                    joints_SourceID = joints_SourceID.Remove(0, 1);
                if (matrices_SourceID.Length > 0 && matrices_SourceID[0] == '#')
                    matrices_SourceID = matrices_SourceID.Remove(0, 1);
                if (weights_SourceID.Length > 0 && weights_SourceID[0] == '#')
                    weights_SourceID = weights_SourceID.Remove(0, 1);

                #region Parsing JOINT-Array
                XmlNode source = this.controllers[i].SelectNodes("descendant::source[@id='" + joints_SourceID + "']")[0];
                XmlNode accessor = source.SelectNodes("descendant::accessor")[0];
                int count = int.Parse(accessor.Attributes["count"].Value);
                string nameArray = source.SelectNodes("Name_array")[0].InnerText;
                valIndex = 0;

                string[] Joints = new string[count];
                Matrix[] Matrices = new Matrix[0];
                float[] Weights = new float[0];

                string[] split = nameArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                for (int j = 0; j < split.Length; j++)
                {
                    for (int k = 0; k < this.Joints.Count; k++)
                        if (this.Joints[k].Name ==  split[j])
                        {
                            Joints[valIndex] = split[j];
                            valIndex++;
                            break;
                        }
                }
                #endregion

                #region Parsing Matrices-Array

                if (matrices_SourceOffset > -1)
                {
                    source = this.controllers[i].SelectNodes("descendant::source[@id='" + matrices_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);

                    string floatArray = source.SelectNodes("float_array")[0].InnerText;

                    Matrices = ParseMatrices(floatArray, count);
                }
                #endregion

                #region Parsing Weights-Array

                if (weights_SourceOffset > -1)
                {
                    source = this.controllers[i].SelectNodes("descendant::source[@id='" + weights_SourceID + "']")[0];
                    accessor = source.SelectNodes("descendant::accessor")[0];
                    count = int.Parse(accessor.Attributes["count"].Value);

                    string floatArray = source.SelectNodes("float_array")[0].InnerText;

                    valIndex = 0;
                    Weights = new float[count];

                    split = floatArray.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });

                    for (int j = 0; j < split.Length; j++)
                    {
                        if (Single.TryParse(split[j], out currVal))
                        {
                            Weights[valIndex] = currVal;
                            valIndex++;
                        }
                    }
                }
                #endregion


                #region Parsing Vertex-Weights
                string vcount = vertexWeightsNode[0].SelectNodes("vcount")[0].InnerText;
                string v = vertexWeightsNode[0].SelectNodes("v")[0].InnerText;


                maxOffset = -1;
                if (joints_SourceOffset > maxOffset)
                    maxOffset = joints_SourceOffset;
                if (matrices_SourceOffset > maxOffset)
                    maxOffset = matrices_SourceOffset;
                if (weights_SourceOffset > maxOffset)
                    maxOffset = weights_SourceOffset;
                maxOffset++;

                int[] indicesOrdered = new int[maxOffset];

                this.ControllerDataJoints.Add(Joints);
                this.ControllerDataMatrices.Add(Matrices);
                this.ControllerDataWeights.Add(Weights);

                this.ControllerDataJoints_i.Add(new List<List<int>>(0));
                this.ControllerDataMatrices_i.Add(new List<List<int>>(0));
                this.ControllerDataWeights_i.Add(new List<List<int>>(0));


                split = vcount.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                List<int> vcountInt = new List<int>(0);

                int currInt = 0;

                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        this.ControllerDataJoints_i[this.ControllerDataJoints_i.Count - 1].Add(new List<int>(0));
                        this.ControllerDataMatrices_i[this.ControllerDataMatrices_i.Count - 1].Add(new List<int>(0));
                        this.ControllerDataWeights_i[this.ControllerDataWeights_i.Count - 1].Add(new List<int>(0));
                        vcountInt.Add(currInt);
                    }
                }

                valCount = 0;
                currInt = 0;
                int currWeightIndex = -1;
                int currvCountIndex = 0;

                split = v.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
                for (int j = 0; j < split.Length; j++)
                {
                    if (int.TryParse(split[j], out currInt))
                    {
                        if (valCount % maxOffset == joints_SourceOffset)
                        {
                            this.ControllerDataJoints_i[this.ControllerDataJoints_i.Count - 1][currvCountIndex].Add(currInt);
                        }
                        if (valCount % maxOffset == matrices_SourceOffset)
                        {
                            this.ControllerDataMatrices_i[this.ControllerDataMatrices_i.Count - 1][currvCountIndex].Add(currInt);
                        }
                        if (valCount % maxOffset == weights_SourceOffset)
                        {
                            this.ControllerDataWeights_i[this.ControllerDataWeights_i.Count - 1][currvCountIndex].Add(currInt);
                            currWeightIndex++;
                            if (currWeightIndex == vcountInt[currvCountIndex] - 1)
                            {
                                currWeightIndex = -1;
                                currvCountIndex++;
                            }
                        }
                        valCount++;
                    }
                }
                #endregion

                this.ShapeMatrices.Add(shapeMatrix);
                this.ControllersIDs.Add(this.controllers[i].Attributes["id"].Value);
                this.PerControllerGeometry.Add(this.controllers[i].SelectSingleNode("skin").Attributes["source"].Value.Remove(0, 1));
            }

            #endregion
            #region Get Per-Geometry Textures
            for (int i = 0; i < this.PerGeometryMaterials.Count; i++)
            {
                string currEffectID = this.MaterialsEffectIDs[MaterialsIDs.IndexOf(this.PerGeometryMaterials[i])];
                string currImageID = this.EffectsImageIDs[this.EffectsIDs.IndexOf(currEffectID)];

                string currImageFileName = this.ImagesFilenames[this.ImagesIDs.IndexOf(currImageID)];

                int index = this.materialFileNames.IndexOf(currImageFileName);
                if (index < 0)
                {
                    /*FileStream fs = new FileStream(this.Directory + "\\" + currImageFileName, FileMode.Open);
                    Texture2D tx = Texture2D.FromStream(Program.game.GraphicsDevice, fs);
                    fs.Close();*/
                    this.MaterialIndices.Add(this.Textures.Count);
                    //this.Textures.Add(tx);
                    this.Textures.Add(ResourceLoader.GetT2D(this.Directory + "\\" + currImageFileName));
                    this.materialFileNames.Add(currImageFileName);
                }
                else
                {
                    this.MaterialIndices.Add(index);
                }
            }

            #endregion

            if (this.ControllerDataJoints.Count == 0)
            {
                Bone b = new Bone("bone000");
                b.GlobalMatrix = Matrix.CreateScale(1f);
                b.localMatrix = Matrix.CreateScale(1f);
                this.Skeleton.Bones.Add(b);


                for (int i=0;i<this.GeometryIDs.Count;i++)
                {
                    this.PerControllerGeometry.Add(this.GeometryIDs[i]);
                    this.ControllersIDs.Add("id" + i.ToString("d3"));
                    this.ControllerDataJoints.Add(new string[] { "bone000" });
                    this.ControllerDataWeights.Add(new float[] { 1f });
                    this.ControllerDataMatrices.Add(new Matrix[] { Matrix.CreateScale(1f) });


                    this.ControllerDataJoints_i.Add(new List<List<int>>(0));
                    this.ControllerDataMatrices_i.Add(new List<List<int>>(0));
                    this.ControllerDataWeights_i.Add(new List<List<int>>(0));

                    for (int j = 0; j < this.GeometryDataVertex[i].Length; j++)
                    {
                        this.ControllerDataJoints_i[this.ControllerDataJoints_i.Count - 1].Add(new List<int>(new int[] {0}));
                        this.ControllerDataMatrices_i[this.ControllerDataMatrices_i.Count - 1].Add(new List<int>(new int[] {0 }));
                        this.ControllerDataWeights_i[this.ControllerDataWeights_i.Count - 1].Add(new List<int>(new int[] {0 }));
                    }
                }
            }
            //this.Document.Save


            this.GetBuffer();
        }

        readonly List<string> ImagesIDs;
        readonly List<string> ImagesFilenames;
        readonly List<string> PerGeometryMaterials;
        List<string> MaterialsIDs;
        readonly List<string> MaterialsEffectIDs; /* Data is corresponding effect ID (an URL, with #) */
        readonly List<string> EffectsIDs;
        readonly List<string> EffectsImageIDs; /* Data is corresponding image ID */

        readonly List<string> GeometryIDs;
        readonly List<Vector3[]> GeometryDataVertex;
        readonly List<Vector2[]> GeometryDataTexcoordinates;
        readonly List<Vector3[]> GeometryDataNormals;
        readonly List<Color[]> GeometryDataColors;
        readonly List<List<int>> GeometryDataVertex_i;
        readonly List<List<int>> GeometryDataTexcoordinates_i;
        readonly List<List<int>> GeometryDataNormals_i;
        readonly List<List<int>> GeometryDataColors_i;

        readonly List<string> PerGeometryTexturesFNames;
        readonly List<Matrix> ShapeMatrices;
        readonly List<string> ControllersIDs;
        readonly List<string> PerControllerGeometry;
        readonly List<string[]> ControllerDataJoints;
        readonly List<Matrix[]> ControllerDataMatrices;
        readonly List<float[]> ControllerDataWeights;
        readonly List<List<List<int>>> ControllerDataJoints_i;
        readonly List<List<List<int>>> ControllerDataMatrices_i;
        readonly List<List<List<int>>> ControllerDataWeights_i;
        readonly List<string> VisualScenesIDs;
        readonly List<List<string>> JointsIDs;
        readonly List<List<Matrix>> JointsMatrices;
        readonly List<List<string>> SurfacesIDs;
        readonly List<List<string>> SurfacesMaterialsID;

        public void GiveUntitledName()
        {
            this.Name = "Untitled";
            this.Directory = System.IO.Directory.GetCurrentDirectory();
        }

        public void Export(string filename)
        {
            FileStream mStream = new FileStream(filename, FileMode.OpenOrCreate);
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '	'
            };

            XmlNode collada = this.Document.SelectNodes("//COLLADA")[0];
            XmlAttribute xmlns = this.Document.CreateAttribute("xmlns");
            xmlns.InnerText = "http://www.collada.org/2005/11/COLLADASchema";
            collada.Attributes.Remove(collada.Attributes["wmlns"]);
            collada.Attributes.InsertBefore(xmlns, collada.Attributes[0]);


            this.Document.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();

            writer.Close();
            mStream.Close();
        }

        public Matrix[] ParseMatrices(string content, int count)
        {
            Matrix[] output = new Matrix[count];
            char separator = ' ';

            for (int j = 2; j < content.Length && j < 20; j++)
            {
                if (content[j] == 9 ||
                    content[j] == 32 ||
                    content[j] == 160)
                {
                    separator = content[j];
                    break;
                }
            }

            string[] split = content.Split(new char[] { separator, '\x09', '\x20', '\xA0', '\r', '\n' });
            float currVal = 0;
            int valCount = 0;
            int valIndex = 0;

            for (int j = 0; j < split.Length; j++)
            {
                if (Single.TryParse(split[j], out currVal))
                {
                    if (valCount % 16 == 0)
                        output[valIndex].M11 = currVal;
                    if (valCount % 16 == 1)
                        output[valIndex].M21 = currVal;
                    if (valCount % 16 == 2)
                        output[valIndex].M31 = currVal;
                    if (valCount % 16 == 3)
                        output[valIndex].M41 = currVal;

                    if (valCount % 16 == 4)
                        output[valIndex].M12 = currVal;
                    if (valCount % 16 == 5)
                        output[valIndex].M22 = currVal;
                    if (valCount % 16 == 6)
                        output[valIndex].M32 = currVal;
                    if (valCount % 16 == 7)
                        output[valIndex].M42 = currVal;

                    if (valCount % 16 == 8)
                        output[valIndex].M13 = currVal;
                    if (valCount % 16 == 9)
                        output[valIndex].M23 = currVal;
                    if (valCount % 16 == 10)
                        output[valIndex].M33 = currVal;
                    if (valCount % 16 == 11)
                        output[valIndex].M43 = currVal;

                    if (valCount % 16 == 12)
                        output[valIndex].M14 = currVal;
                    if (valCount % 16 == 13)
                        output[valIndex].M24 = currVal;
                    if (valCount % 16 == 14)
                        output[valIndex].M34 = currVal;
                    if (valCount % 16 == 15)
                    {
                        output[valIndex].M44 = currVal;
                        valIndex++;
                    }
                    valCount++;
                }
            }
            return output;
        }

        /*public void RecreateVertexBuffer()
        {
            for (int i = 0; i < this.VertexBuffer_c.Length; i++)
            {
                Vector3 v3 = Vector3.Zero;

                for (int j = 0; j < this.VertexBuffer_c[i].Vertices.Length; j++)
                {
                    Vector3 ComputingVertex = Vector3.Zero;
                    ComputingVertex.X = this.VertexBuffer_c[i].Vertices[j].X;
                    ComputingVertex.Y = this.VertexBuffer_c[i].Vertices[j].Y;
                    ComputingVertex.Z = this.VertexBuffer_c[i].Vertices[j].Z;

                    Matrix mat = this.Joints[this.VertexBuffer_c[i].Matis[j]].ComputedMatrix;

                    ComputingVertex = Vector3.Transform(ComputingVertex, mat);
                    v3 += ComputingVertex * this.VertexBuffer_c[i].Vertices[j].W;
                }

                this.VertexBufferColor[i].Position.X = v3.X;
                this.VertexBufferColor[i].Position.Y = v3.Y;
                this.VertexBufferColor[i].Position.Z = v3.Z;

                this.VertexBufferNormal[i].Position.X = v3.X;
                this.VertexBufferNormal[i].Position.Y = v3.Y;
                this.VertexBufferNormal[i].Position.Z = v3.Z;
            }
            if (this.display == DisplayMode.Normal)
            {
                this.vBuffer.SetData<VertexPositionNormalTexture>(this.VertexBufferNormal);
            }
            else
            {
                this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);
            }
        }*/
        
        public static string ToString(Microsoft.Xna.Framework.Matrix m)
        {
            string s = "";
            s += m.M11.ToString("0.000000") + " " + m.M21.ToString("0.000000") + " " + m.M31.ToString("0.000000") + " " + m.M41.ToString("0.000000") + "\r\n";
            s += m.M12.ToString("0.000000") + " " + m.M22.ToString("0.000000") + " " + m.M32.ToString("0.000000") + " " + m.M42.ToString("0.000000") + "\r\n";
            s += m.M13.ToString("0.000000") + " " + m.M23.ToString("0.000000") + " " + m.M33.ToString("0.000000") + " " + m.M43.ToString("0.000000") + "\r\n";
            s += m.M14.ToString("0.000000") + " " + m.M24.ToString("0.000000") + " " + m.M34.ToString("0.000000") + " " + m.M44.ToString("0.000000") + "\r\n";
            return s;
        }
        public static string ToStringAccurate(Microsoft.Xna.Framework.Matrix m)
        {
            string s = "";
            s += ((Decimal)m.M11) + " " + ((Decimal)m.M21) + " " + ((Decimal)m.M31) + " " + ((Decimal)m.M41) + "\r\n";
            s += ((Decimal)m.M12) + " " + ((Decimal)m.M22) + " " + ((Decimal)m.M32) + " " + ((Decimal)m.M42) + "\r\n";
            s += ((Decimal)m.M13) + " " + ((Decimal)m.M23) + " " + ((Decimal)m.M33) + " " + ((Decimal)m.M43) + "\r\n";
            s += ((Decimal)m.M14) + " " + ((Decimal)m.M24) + " " + ((Decimal)m.M34) + " " + ((Decimal)m.M44) + "\r\n";
            return s;
        }
        
        

        public byte[] mdlxBytes;
        public MemoryStream mdlxStream;

        public unsafe void ExportBin()
        {
            FileStream fs = new FileStream(Path.GetDirectoryName(this.FileName)+ @"\" + this.Name + ".mdl", FileMode.OpenOrCreate);
            BinaryWriter write = new BinaryWriter(fs);

            int textureCount = this.materialFileNames.Count;

            write.Write((sbyte)this.ModelType);
            write.Write(this.NoCull ? (sbyte)-1 : (sbyte)0);
            write.Write((sbyte)this.ZIndex);
            write.Write((sbyte)0);

            write.Write(textureCount);
            write.Write(0x11300096);
            write.Write(0x000e317B);

            byte[] textureNameBuffer = new byte[0x30];
            for (int i=0;i < textureCount;i++)
            {
                string name = "";
                string[] spli = (this.Directory + "\\"+this.materialFileNames[i]).Replace("//", "\\").Split('\\');
                bool start = false;
                for (int j=0;j<spli.Length;j++)
                {
                    if (spli[j]=="Models")
                    {
                        start = true;
                    }
                    if (start)
                    {
                        name += spli[j];
                        if (j<spli.Length-1)
                        {
                            name += "\\";
                        }
                    }
                }

                textureNameBuffer[0] = (byte)name.Length;
                Array.Copy(Encoding.ASCII.GetBytes(name), 0, textureNameBuffer, 1, name.Length);
                write.Write(textureNameBuffer);
            }

            int meshCount = this.MeshesOffsets.Count;
            write.Write(meshCount);
            write.Write(0);
            write.Write(0);
            write.Write(0);
            for (int i = 0; i < meshCount; i++)
            {
                write.Write(this.MaterialIndices[i]);
                write.Write(this.MeshesOffsets[i][0]);
                write.Write(this.MeshesOffsets[i][1]);
            }

            if (meshCount % 4 > 0)
                for (int i = 0; i < (4 - (meshCount % 4)); i++)
                write.Write(0);


            int jo4Ind = 0;


            for (int i = 0; i < this.VertexBuffer_c.Length; i++)
            {
                int ind = ((i / 3) * 3 + (2 - (i % 3)));
                jo4Ind = 0;

                write.Write(this.VertexBuffer_c[ind].Count);
                write.Write(this.VertexBufferColor[ind].TextureCoordinate.X);
                write.Write(this.VertexBufferColor[ind].TextureCoordinate.Y);
                write.Write((byte)this.VertexBufferColor[ind].Color.R);
                write.Write((byte)this.VertexBufferColor[ind].Color.G);
                write.Write((byte)this.VertexBufferColor[ind].Color.B);
                write.Write((byte)this.VertexBufferColor[ind].Color.A);
                
                for (int j = 0; j < this.VertexBuffer_c[ind].Count; j+=4)
                {
                    write.Write((int)this.VertexBuffer_c[ind].Matis[jo4Ind]);
                    write.Write(0);
                    write.Write(0);
                    write.Write(0);
                    write.Write(this.VertexBuffer_c[ind].Vertices[j]);
                    write.Write(this.VertexBuffer_c[ind].Vertices[j+1]);
                    write.Write(this.VertexBuffer_c[ind].Vertices[j+2]);
                    write.Write(this.VertexBuffer_c[ind].Vertices[j+3]);
                    jo4Ind++;
                }
            }

            write.Close();
            fs.Close();
            MSET m = new MSET();
            m.Skeleton = this.Skeleton;
            m.CreateSkeleton();
            //File.WriteAllBytes(Path.GetDirectoryName(this.FileName) + @"\"+this.Name + ".skel", m.skelBytes);
        }
    }
}
