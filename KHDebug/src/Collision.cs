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
    public class Collision : Resource
    {
        private List<Vector3> cols;
        private List<Vector3> norms;
        private List<int[][]> indices;
        int[][] StartEnds;
        int PolyCount;

        public Collision(string filename)
        {
            this.Type = ResourceType.Collision;
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;
            this.cols = new List<Vector3>(0);
            this.norms = new List<Vector3>(0);
            this.indices = new List<int[][]>(0);
            this.StartEnds = new int[1000][];
            this.PolyCount = 0;

            string[] obj = File.ReadAllLines(filename);
            string[] spli = obj[0].Split('#');
            if (spli.Length == 3)
            {
                Program.game.backgroundColor.R = byte.Parse(spli[0]);
                Program.game.backgroundColor.G = byte.Parse(spli[1]);
                Program.game.backgroundColor.B = byte.Parse(spli[2]);
            }

            int lastIndex = -1;

            for (int i=0;i<obj.Length;i++)
            {
                spli = obj[i].Split(' ');
                if (spli[0] == "v")
                {
                    cols.Add(new Vector3(MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]), MainGame.SingleParse(spli[3])));
                }
                if (spli[0] == "vn")
                {
                    norms.Add(new Vector3(MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]), MainGame.SingleParse(spli[3])));
                }
                if (spli[0] == "g")
                {
                    if (spli[1].Contains("poly"))
                    {
                        int index = int.Parse(spli[1].Substring(4, spli[1].Length-4));

                        if (lastIndex>-1)
                        {
                            StartEnds[lastIndex][1] = indices.Count;
                        }
                        StartEnds[index] = new int[] { indices.Count, 0 };
                        this.PolyCount++;
                        lastIndex = index;
                    }
                    if (spli[1].Contains("perimetre"))
                    {
                        int index = 500+int.Parse(spli[1].Substring(9, spli[1].Length - 9));

                        if (lastIndex > -1)
                        {
                            StartEnds[lastIndex][1] = indices.Count;
                        }
                        StartEnds[index] = new int[] { indices.Count ,0};

                        lastIndex = index;
                    }
                }
                if (spli[0] == "f")
                {
                    int[][] vals = new int[][] { new int[2] , new int[2] , new int[2] };

                    string[] spli_ = spli[1].Split('/');
                    vals[0][0] = int.Parse(spli_[0]) - 1;
                    vals[0][1] = int.Parse(spli_[2]) - 1;

                    spli_ = spli[2].Split('/');
                    vals[1][0] = int.Parse(spli_[0]) - 1;
                    vals[1][1] = int.Parse(spli_[2]) - 1;

                    spli_ = spli[3].Split('/');
                    vals[2][0] = int.Parse(spli_[0]) - 1;
                    vals[2][1] = int.Parse(spli_[2]) - 1;

                    this.indices.Add(vals);
                }
            }
            if (PolyCount == 0)
            {
                this.PolyCount = 1;
                StartEnds[0] = new int[] { 0, this.indices.Count };
            }
            if (lastIndex>-1)
            {
                if (lastIndex >= 500)
                {
                    lastIndex -= 500;
                }
                StartEnds[lastIndex][1] = StartEnds[lastIndex + 500][0];
                StartEnds[lastIndex + 500][1] = indices.Count;
            }
        }

        private static double area(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            return Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
        }
        public static double areaD(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
        }

        private static bool isInside(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int x1 = (int)v1.X;
            int y1 = (int)v1.Z;
            int x2 = (int)v2.X;
            int y2 = (int)v2.Z;
            int x3 = (int)v3.X;
            int y3 = (int)v3.Z;
            int x = (int)point.X;
            int y = (int)point.Z;

            double A = area(x1, y1, x2, y2, x3, y3);
            double A1 = area(x, y, x2, y2, x3, y3);
            double A2 = area(x1, y1, x, y, x3, y3);
            double A3 = area(x1, y1, x2, y2, x, y);

            return (A == A1 + A2 + A3);
        }

        

        int CurrIndex = 0;

        public void UpdateIndex(Vector3 who)
        {
            if (this.PolyCount == 1)
            {
                this.CurrIndex = 0;
                return;
            }
            double closest = double.MaxValue;
            for (int i = 0 ; i < this.PolyCount ; i++)
            {
                for (int j = StartEnds[500 + i][0]; j < StartEnds[500 + i][1]; j++)
                {

                    Vector3 v1 = cols[this.indices[j][0][0]];
                    Vector3 v2 = cols[this.indices[j][1][0]];
                    Vector3 v3 = cols[this.indices[j][2][0]];
                    if (Math.Abs(v1.Y - v2.Y) < 1)
                    {
                        if (Math.Abs(v2.Y - v3.Y) < 1)
                        {
                            Vector3 flat = new Vector3(who.X,v1.Y, who.Z);

                            double totalArea = Collision.AreaOfTriangle(v1, v2, v3);

                            double area1 = Collision.AreaOfTriangle(flat, v2, v3);
                            double area2 = Collision.AreaOfTriangle(v1, flat, v3);
                            double area3 = Collision.AreaOfTriangle(v1, v2, flat);
                            double somme = Math.Abs((area1 + area2 + area3) - totalArea);
                            if (somme < closest)
                            {
                                closest = somme;
                                this.CurrIndex = i;
                            }
                        }
                    }
                }
            }
        }

        public static double AreaOfTriangle(Vector3 pt1, Vector3 pt2, Vector3 pt3)
        {
            double a = Vector3.Distance(pt1, pt2);
            double b = Vector3.Distance(pt2, pt3);
            double c = Vector3.Distance(pt3, pt1);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public const double oneOver144 = 1d / 144d;

        public static double GetVolume(double a1, double a2, double a3, double a4, double a5, double a6)
        {
            double a1_squ = a1 * a1;
            double a2_squ = a2 * a2;
            double a3_squ = a3 * a3;
            double a4_squ = a4 * a4;
            double a5_squ = a5 * a5;
            double a6_squ = a6 * a6;

            double output =
                oneOver144 * (
                (a1_squ * a5_squ) * (a2_squ + a3_squ + a4_squ + a6_squ - a1_squ - a5_squ) +
                (a2_squ * a6_squ) * (a1_squ + a3_squ + a4_squ + a5_squ - a2_squ - a6_squ) +
                (a3_squ * a4_squ) * (a1_squ + a2_squ + a5_squ + a6_squ - a3_squ - a4_squ)
                - (a1_squ * a2_squ * a4_squ)
                - (a2_squ * a3_squ * a5_squ)
                - (a1_squ * a3_squ * a6_squ)
                - (a4_squ * a5_squ * a6_squ)
                );

            return Math.Sqrt(output);
        }

        public static bool Inside(Vector3 which, Vector3 a, Vector3 b, Vector3 c, Vector3 aN, Vector3 bN, Vector3 cN)
        {
            Vector3 middle = (a + b + c + aN + bN + cN) / 6f;
            double reference = TotalVolume(middle, a, b, c, aN, bN, cN);
            double test = TotalVolume(which, a, b, c, aN, bN, cN);

            return Math.Abs(reference - test) < Math.Abs(reference) / 1000d;
        }

        public static double TotalVolume(Vector3 middle, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, Vector3 f)
        {
            double a_b = Vector3.Distance(a, b);
            double a_c = Vector3.Distance(a, c);
            double a_d = Vector3.Distance(a, d);
            double a_e = Vector3.Distance(a, e);
            double a_f = Vector3.Distance(a, f);

            double b_c = Vector3.Distance(b, c);
            double b_d = Vector3.Distance(b, d);
            double b_e = Vector3.Distance(b, e);
            double b_f = Vector3.Distance(b, f);

            double c_d = Vector3.Distance(c, d);
            double c_e = Vector3.Distance(c, e);
            double c_f = Vector3.Distance(c, f);

            double d_e = Vector3.Distance(d, e);
            double d_f = Vector3.Distance(d, f);

            double e_f = Vector3.Distance(e, f);

            double a_middle = Vector3.Distance(a, middle);
            double b_middle = Vector3.Distance(b, middle);
            double c_middle = Vector3.Distance(c, middle);
            double d_middle = Vector3.Distance(d, middle);
            double e_middle = Vector3.Distance(e, middle);
            double f_middle = Vector3.Distance(f, middle);

            double v1 = GetVolume(a_middle, b_middle, d_middle, a_b, b_d, a_d);
            double v2 = GetVolume(b_middle, d_middle, e_middle, b_d, d_e, b_e);
            double v3 = GetVolume(a_middle, c_middle, f_middle, a_c, c_f, a_f);
            double v4 = GetVolume(a_middle, d_middle, f_middle, a_d, d_f, a_f);
            double v5 = GetVolume(b_middle, c_middle, e_middle, b_c, c_e, b_e);
            double v6 = GetVolume(c_middle, e_middle, f_middle, c_e, e_f, c_f);
            double v7 = GetVolume(a_middle, b_middle, c_middle, a_b, b_c, a_c);
            double v8 = GetVolume(d_middle, e_middle, f_middle, d_e, e_f, d_f);

            return v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8;
        }

        //public static Vector3[] iruTori = new Vector3[3];
        public static float LastAccessedY;
        
        public void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs)
        {
            if (Program.game.Collision == 0)
            return;
            VertexPositionNormalTexture[] vpnt = new VertexPositionNormalTexture[this.indices.Count*3];
            UpdateIndex(Program.game.mainCamera.Target.Location);
            int start = this.StartEnds[this.CurrIndex][0];
            int end = this.StartEnds[this.CurrIndex][1];
            for (int i= start; i< end; i++)
            {
                for (int j=0;j<3;j++)
                {
                    vpnt[i * 3 + j] = new VertexPositionNormalTexture
                    {
                        Position = this.cols[this.indices[i][2 - j][0]],
                        TextureCoordinate = new Vector2(0.5f, 0.5f),
                        Normal = this.norms[this.indices[i][j][1]]
                    };
                }
            }


            be.EnableDefaultLighting();
            be.LightingEnabled = true;
            be.PreferPerPixelLighting = true;

            //be.AmbientLightColor = Color.White.ToVector3();
            be.DiffuseColor = new Color(100, 100, 100,255).ToVector3();
            //be.EmissiveColor = Color.White.ToVector3();
            
            be.Texture = ResourceLoader.EmptyT2D;
            be.CurrentTechnique.Passes[0].Apply();
            be.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpnt, 0, vpnt.Length/3);


            be.LightingEnabled = false;
            be.PreferPerPixelLighting = false;
        }


        public bool CanClimb(Model mdl)
        {
            bool can = true;
            Vector3 start = mdl.Location + mdl.MaxVertex;
            start.Y += 30f;
            Vector3 end = start + Vector3.Transform(new Vector3(0, 0, 100f), Matrix.CreateRotationY(mdl.DestRotate));
            Vector3 inter;

            UpdateIndex(mdl.Location);
            int start_ = this.StartEnds[this.CurrIndex][0];
            int end_ = this.StartEnds[this.CurrIndex][1];
            for (int i = start_; i < end_; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                if (intersect3D_RayTriangle(start, end, v1, v2, v3, out inter) != 0)
                {
                    if (Vector3.Distance(start, inter)<90f)
                    {
                        can = false;
                        break;
                    }
                }
            }

            return can;
        }

        public Vector3 GetCameraCollisionIntersect(Vector3 old_, Vector3 new_)
        {
            Vector3 output = new Vector3(Single.NaN, Single.NaN, Single.NaN);
            Vector3 inter = Vector3.Zero;
            float closest = Single.MaxValue;

            UpdateIndex(old_);
            int start_ = this.StartEnds[this.CurrIndex][0];
            int end_ = this.StartEnds[this.CurrIndex][1];
            for (int i = start_; i < end_; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                Vector3 n1 = norms[this.indices[i][0][1]];
                Vector3 n2 = norms[this.indices[i][1][1]];
                Vector3 n3 = norms[this.indices[i][2][1]];
                Vector3 normal = (n1 + n2 + n3) / 3f;

                if (intersect3D_RayTriangle(old_, new_, v1, v2, v3, out inter) != 0)
                {

                    float dist = Vector3.Distance(old_, inter);
                    if (dist > 0 && dist < closest && dist < Vector3.Distance(old_, new_))
                    {
                        output = inter + normal * 30f;
                        closest = dist;
                    }
                }
            }
            return output;
        }

        public bool GetCameraCollisionVolume(Vector3 point)
        {
            Vector3 output = new Vector3(Single.NaN, Single.NaN, Single.NaN);
            Vector3 inter = Vector3.Zero;

            UpdateIndex(point);
            int start_ = this.StartEnds[this.CurrIndex][0];
            int end_ = this.StartEnds[this.CurrIndex][1];
            for (int i = start_; i < end_; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                Vector3 n1 = norms[this.indices[i][0][1]];
                Vector3 n2 = norms[this.indices[i][1][1]];
                Vector3 n3 = norms[this.indices[i][2][1]];

                if (Inside(point, v1 - n1*50f, v2 - n2 * 50f, v3 - n3 * 50f, v1 + n1*20f * n1, v2 + n2 * 20f, v3 + n3 * 20f))
                {
                    return false;
                }
            }
            return true;
        }

        public float GetIKDistance(Vector3 pos)
        {
            Vector3 inter;
            float distance = 0;
            for (int i = 0; i < this.indices.Count; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                Vector3 n1 = norms[this.indices[i][0][1]];
                Vector3 n2 = norms[this.indices[i][1][1]];
                Vector3 n3 = norms[this.indices[i][2][1]];
                Vector3 normal = (n1 + n2 + n3) / 3f;
                if (normal.Y > 0.5)
                {
                    if (intersect3D_RayTriangle(pos, pos+new Vector3(0,40,0), v1, v2,v3, out inter) == 1)
                    {
                        distance = inter.Y;
                        break;
                    }
                }

            }
             return distance;
        }

        public void MonitorCollision(Model sujet, ref Vector3 pos)
        {
            UpdateIndex(sujet.Location);

            bool jumping = sujet.Location.Y> sujet.LowestFloor+sujet.StairHeight && pos.Y > sujet.Location.Y;

            Vector3 refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
            Vector3 refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);

            Vector3 inter;
            Vector3 shadowAngle = Vector3.Zero;
            Vector3 globalBone0 = sujet.GetGlobalBone(0);
            globalBone0.Y = sujet.MinVertex.Y;

            float tallest = Single.MinValue;
            float smallest = Single.MaxValue;
            shadowAngle = Vector3.Zero;

            float epaisseur4 = sujet.Epaisseur;
            bool pushed = false;

            Matrix ps4 = Matrix.CreateRotationY(MainGame.PI / 4f);

            if ((ScenePlayer.ScenePlaying  && ScenePlayer.AllowContactCollision) ||
                !ScenePlayer.ScenePlaying)
            if (sujet.cState != Model.ControlState.Cliff)
            for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
            {
                        if (!(MainGame.ResourceFiles[i] is Model mdl)) continue;
                        if (mdl.ModelType != Model.MDType.Human) continue;
                        if (mdl.HasMaster) continue;
                if (sujet.ResourceIndex == mdl.ResourceIndex) continue;

                for (int h = 0; h < 2; h++)
                for (int j = 0; j < Cursor.VECTORS.Length; j+=3)
                {
                    Vector3 v1 = Cursor.VECTORS[j]*2f;
                    Vector3 v2 = Cursor.VECTORS[j+1] * 2f;
                    Vector3 v3 = Cursor.VECTORS[j+2] * 2f;
                    if (h==1)
                    {
                        v1 = Vector3.Transform(v1, ps4);
                        v2 = Vector3.Transform(v2, ps4);
                        v3 = Vector3.Transform(v3, ps4);
                    }
                    /*if (v1.Y < 0 && v2.Y < 0 && v3.Y < 0)
                        continue;
                    if (v1.Y > 0 && v2.Y >0 && v3.Y> 0)
                        continue;*/

                    Vector3 n1 = v1 / Vector3.Distance(Vector3.Zero, v1);
                    Vector3 n2 = v2 / Vector3.Distance(Vector3.Zero, v2);
                    Vector3 n3 = v3 / Vector3.Distance(Vector3.Zero, v3);

                    v1 += new Vector3(0, 1f, 0);
                    v2 += new Vector3(0, 1f, 0);
                    v3 += new Vector3(0, 1f, 0);

                    v1.Y *= mdl.MaxVertex.Y;
                    v1.X *= mdl.Epaisseur / 2f;
                    v1.Z *= mdl.Epaisseur / 2f;
                    v1 += mdl.Location - new Vector3(0, mdl.MaxVertex.Y, 0);

                    v2.Y *= mdl.MaxVertex.Y;
                    v2.X *= mdl.Epaisseur / 2f;
                    v2.Z *= mdl.Epaisseur / 2f;
                    v2 += mdl.Location - new Vector3(0, mdl.MaxVertex.Y, 0);

                    v3.Y *= mdl.MaxVertex.Y;
                    v3.X *= mdl.Epaisseur / 2f;
                    v3.Z *= mdl.Epaisseur / 2f;
                    v3 += mdl.Location - new Vector3(0, mdl.MaxVertex.Y, 0);

                    Vector3 normal = (n1 + n2 + n3) / 3f;
                    
                    if (Math.Abs(normal.Y) < 0.5f)
                    {
                        if (sujet.Name != Program.game.mainCamera.Target.Name)
                        {
                            /*if (Inside(refPosBottom, v1, v2, v3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
                            {
                                Vector3 direction = refPosBottom + normal * sujet.Epaisseur * 10f;
                                if (jumping && normal.Y < -0.5)
                                    sujet.JumpCollisionCancel = true;
                                if (intersect3D_RayTriangle(refPosBottom, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
                                {
                                    //pos += ((inter + new Vector3(0, -height, 0))- pos)/10f;
                                    pos = (inter + new Vector3(0, -sujet.StairHeight, 0));
                                    refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
                                }
                            }*/
                            if (Inside(refPosBottom, mdl.Location, mdl.Location, mdl.Location, v1 + epaisseur4 * n1, v2 + epaisseur4 * n2, v3 + epaisseur4 * n3))
                            {
                                Vector3 direction = refPosBottom + normal * epaisseur4 * 10f;

                                if (intersect3D_RayTriangle(refPosBottom, direction, v1 + epaisseur4 * n1, v2 + epaisseur4 * n2, v3 + epaisseur4 * n3, out inter) == 1)
                                {
                                    //pos += ((inter + new Vector3(0, -height, 0))- pos)/10f;
                                    pos.Z = inter.Z;
                                    pos.X = inter.X;
                                    pushed = true;
                                    //pos.Y = sujet.Location.Y;
                                    refPosBottom = pos;
                                }
                            }
                            if (Inside(refPosTop, mdl.Location, mdl.Location, mdl.Location, v1 + epaisseur4 * n1, v2 + epaisseur4 * n2, v3 + epaisseur4 * n3))
                            {
                                Vector3 direction = refPosTop + normal * epaisseur4 * 10f;

                                if (intersect3D_RayTriangle(refPosTop, direction, v1 + epaisseur4 * n1, v2 + epaisseur4 * n2, v3 + epaisseur4 * n3, out inter) == 1)
                                {
                                    //pos += ((inter + new Vector3(0, -height, 0))- pos)/10f;
                                    pos.X = inter.X;
                                    pos.Z = inter.Z;
                                    pushed = true;

                                    //pos.Y = sujet.Location.Y;
                                    refPosTop = pos;
                                }
                            }
                        }
                    }

                }
            }
            sujet.ShadowMatrixSurface = Matrix.CreateFromYawPitchRoll(shadowAngle.X, shadowAngle.Y, shadowAngle.Z);
            if (sujet.cState == Model.ControlState.Cliff)
            {
                return;
            }
            Vector3 forwardCliffStart = new Vector3(0, 10f, sujet.Epaisseur*1.1f);
            Vector3 forwardCliffEnd = new Vector3(0, - 10f, sujet.Epaisseur*1.1f);
            Vector3 cliffBackwards = new Vector3(0, 0, -0.05f);
            Matrix rotYMat = Matrix.CreateRotationY(sujet.Rotate);

            forwardCliffStart = Vector3.Transform(forwardCliffStart, rotYMat);
            forwardCliffEnd = Vector3.Transform(forwardCliffEnd, rotYMat);
            cliffBackwards = Vector3.Transform(cliffBackwards, rotYMat);
            int start = this.StartEnds[this.CurrIndex][0];
            int end = this.StartEnds[this.CurrIndex][1];

            for (int i = start; i < end; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                Vector3 n1 = norms[this.indices[i][0][1]];
                Vector3 n2 = norms[this.indices[i][1][1]];
                Vector3 n3 = norms[this.indices[i][2][1]];
                Vector3 normal = (n1 + n2 + n3) / 3f;


                if (!ScenePlayer.ScenePlaying && normal.Y < -0.5)
                if (Inside(refPosTop, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
                {
                    if (normal.Y < -0.5)
                        sujet.JumpCollisionCancel = true;
                    Vector3 direction = refPosTop + normal * sujet.Epaisseur;
                    if (intersect3D_RayTriangle(refPosTop, direction, v1, v2, v3, out inter) == 1)
                    {
                        //pos += ((inter + new Vector3(0, -height, 0))- pos)/10f;
                        pos = (inter + new Vector3(0, -(sujet.MaxVertex.Y), 0));
                        if (pos.Y < sujet.LowestFloor)
                            pos.Y = sujet.LowestFloor;

                        refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);
                    }
                }
                if (!ScenePlayer.ScenePlaying && Math.Abs(normal.Y) < 0.5f)
                {
                    if (Inside(refPosBottom, v1-20f*n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
                    {
                        Vector3 direction = refPosBottom + normal * sujet.Epaisseur * 10f;
                        if (jumping && normal.Y < -0.5)
                            sujet.JumpCollisionCancel = true;
                        if (intersect3D_RayTriangle(refPosBottom, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
                        {
                            pos = (inter + new Vector3(0, -sujet.StairHeight, 0));
                            refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
                        }
                    }

                    if (Inside(refPosTop, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
                    {
                        Vector3 direction = refPosTop + normal * sujet.Epaisseur * 10f;
                        if (intersect3D_RayTriangle(refPosTop, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
                        {
                            pos = (inter + new Vector3(0, -(sujet.MaxVertex.Y), 0));
                            if (pos.Y < sujet.LowestFloor)
                                pos.Y = sujet.LowestFloor;

                            refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);
                        }
                    }
                }
                else if (normal.Y>0.5)
                {
                    if (!ScenePlayer.ScenePlaying && sujet.cState == Model.ControlState.Fall && !sujet.CliffCancel)
                    {
                        if (intersect3D_RayTriangle(refPosTop + forwardCliffStart, refPosTop + forwardCliffEnd, v1, v2, v3, out inter) == 1 && inter.Y > refPosTop.Y + forwardCliffEnd.Y)
                        {
                            Vector3 coin = Vector3.Zero;
                            for (int k=0;k<100;k++)
                            {
                                if (intersect3D_RayTriangle(inter + cliffBackwards*k + new Vector3(0, 10, 0), inter + cliffBackwards * k + new Vector3(0, -10, 0), v1, v2, v3, out inter) == 1)
                                {
                                    coin = inter + cliffBackwards * k;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            float distv1v2 = Vector3.Distance(v1, v2);
                            float distv2v3 = Vector3.Distance(v2, v3);
                            float distv3v1 = Vector3.Distance(v3, v1);

                            float distv1coin = Vector3.Distance(v1, coin);
                            float distv2coin = Vector3.Distance(v2, coin);
                            float distv3coin = Vector3.Distance(v3, coin);

                            int ordre = 0;  /* V1V2  V2V3  V3V1*/
                           double diff = double.MaxValue;
                            double currDiff = Math.Abs(distv1v2 - (distv1coin+ distv2coin));
                            if (currDiff < diff)
                            {
                                ordre = 0;
                                diff = currDiff;
                            }
                            currDiff = Math.Abs(distv2v3 - (distv2coin + distv3coin));
                            if (currDiff < diff)
                            {
                                ordre = 1;
                                diff = currDiff;
                            }
                            currDiff = Math.Abs(distv3v1 - (distv3coin + distv1coin));
                            if (currDiff < diff)
                            {
                                ordre = 2;
                                diff = currDiff;
                            }
                            float angle = 0;

                            if (ordre == 0)
                            {
                                Vector3 v1v2Base = v2 - v1;
                                v1v2Base /= Vector3.Distance(Vector3.Zero, v1v2Base);
                                angle = (float)Math.Atan2(v1v2Base.X, v1v2Base.Z);
                            }
                            if (ordre == 1)
                            {
                                Vector3 v2v3Base = v3 - v2;
                                v2v3Base /= Vector3.Distance(Vector3.Zero, v2v3Base);
                                angle = (float)Math.Atan2(v2v3Base.X, v2v3Base.Z);
                            }
                            if (ordre == 2)
                            {
                                Vector3 v3v1Base = v1 - v3;
                                v3v1Base /= Vector3.Distance(Vector3.Zero, v3v1Base);
                                angle = (float)Math.Atan2(v3v1Base.X, v3v1Base.Z);
                            }
                            if (coin.Y > sujet.LowestFloor + sujet.MaxVertex.Y)
                            {
                                sujet.DestRotate = angle + MainGame.PI / 2f;
                                sujet.Rotate = sujet.DestRotate;
                                sujet.cState = Model.ControlState.Cliff;
                                
                                //Program.game.cursors[0].Position = coin;
                                /*try
                                {
                                    string[] input = File.ReadAllLines("data.txt");
                                    cliffBackwards.X = MainGame.SingleParse(input[0]);
                                    cliffBackwards.Y = MainGame.SingleParse(input[1]);
                                    cliffBackwards.Z = MainGame.SingleParse(input[2]);
                                    cliffBackwards = Vector3.Transform(cliffBackwards, rotYMat);

                                }
                                catch
                                {

                                }*/
                                cliffBackwards = Vector3.Transform(sujet.CliffPosition, rotYMat);
                                pos = coin - cliffBackwards;
                                sujet.Location = pos;
                                return;
                            }

                            
                        }

                    }

                    if (intersect3D_RayTriangle(pos + globalBone0+new Vector3(0, 50000, 0), pos + globalBone0 + new Vector3(0, -50000, 0), v1, v2, v3, out inter) ==1)
                    {
                        if (inter.Y < smallest)
                        {
                            smallest = inter.Y;
                        }
                        if (inter.Y <= sujet.Location.Y + sujet.StairHeight*1.5f && inter.Y > tallest)
                        {
                            shadowAngle = new Vector3(
                            0,
                            (float)Math.Atan2(normal.Z, normal.Y),
                            (float)Math.Atan2(-normal.X, normal.Y));
                            tallest = inter.Y;
                        }
                    }
                }
                if (tallest > Single.MinValue / 2f)
                {
                    if (tallest <sujet.Location.Y + sujet.StairHeight * 1.5f)
                    sujet.LowestFloor = tallest;
                }
                else if (smallest < Single.MaxValue / 2f)
                {
                    if (smallest < sujet.Location.Y + sujet.StairHeight * 1.5f)
                        sujet.LowestFloor = smallest;
                }
            }
            sujet.ShadowMatrixSurface = Matrix.CreateFromYawPitchRoll(shadowAngle.X, shadowAngle.Y, shadowAngle.Z);
            if (pushed && pos.Y<sujet.LowestFloor)
            {
                pos.Y = sujet.LowestFloor;
            }
        }

        public bool HasCol(Vector3 start, Vector3 end)
        {
            bool output = false;
            Vector3 intersect = Vector3.Zero;
            
            for (int i = 0; i < this.indices.Count; i++)
            {
                Vector3 v1 = cols[this.indices[i][0][0]];
                Vector3 v2 = cols[this.indices[i][1][0]];
                Vector3 v3 = cols[this.indices[i][2][0]];

                if (intersect3D_RayTriangle(start,end,v1,v2,v3, out intersect) >0)
                {
                    output = true;
                        break;

                }
            }
            return output;
        }

        private float dot(Vector3 u, Vector3 v)
        {
            return ((u).X * (v).X + (u).Y * (v).Y + (u).Z * (v).Z);
        }

        // intersect3D_RayTriangle(): find the 3D intersection of a ray with a triangle
        //    Input:  a ray R, and a triangle T
        //    Output: *I = intersection point (when it exists)
        //    Return: -1 = triangle is degenerate (a segment or point)
        //             0 =  disjoint (no intersect)
        //             1 =  intersect in unique point I1
        //             2 =  are in the same plane
        private int intersect3D_RayTriangle(Vector3 P0, Vector3 P1, Vector3 V0, Vector3 V1, Vector3 V2, out Vector3 I)
        {
            I = new Vector3(Single.NaN, Single.NaN, Single.NaN);
            
            Vector3 u, v, n;              // triangle vectors
            Vector3 dir, w0, w;           // ray vectors
            float r, a, b;              // params to calc ray-plane intersect

            // get triangle edge vectors and plane normal
            u = V1 - V0;
            v = V2 - V0;

            n = Vector3.Cross(u,v);              // cross product
            if (n == Vector3.Zero)             // triangle is degenerate
                return -1;                  // do not deal with this case

            dir = P1 - P0;              // ray direction vector
            w0 = P0 - V0;
            a = -dot(n, w0);
            b = dot(n, dir);
            if (Math.Abs(b) < 0.00001)
            {     // ray is  parallel to triangle plane
                if (a == 0)                 // ray lies in triangle plane
                    return 2;
                else return 0;              // ray disjoint from plane
            }

            // get intersect point of ray with triangle plane
            r = a / b;
            if (r < 0.0)                    // ray goes away from triangle
                return 0;                   // => no intersect
                                            // for a segment, also test if (r > 1.0) => no intersect

            I = P0 + r * dir;            // intersect point of ray and plane

            // is I inside T?
            float uu, uv, vv, wu, wv, D;
            uu = dot(u, u);
            uv = dot(u, v);
            vv = dot(v, v);
            w = I - V0;
            wu = dot(w, u);
            wv = dot(w, v);
            D = uv * uv - uu * vv;

            // get and test parametric coords
            float s, t;
            s = (uv * wv - vv * wu) / D;
            if (s < 0.0 || s > 1.0)         // I is outside T
                return 0;
            t = (uv * wu - uu * wv) / D;
            if (t < 0.0 || (s + t) > 1.0)  // I is outside T
                return 0;

            if (Math.Abs(Vector3.Distance(P0,P1) - (Vector3.Distance(P0, I) + Vector3.Distance(I, P1))) >10)
            {
                I = new Vector3(Single.NaN, Single.NaN, Single.NaN);
                return 0;
            }

            return 1;                       // I is in T
        }
    }
}
