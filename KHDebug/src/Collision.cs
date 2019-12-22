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
		

        public Single MinX = Single.MaxValue;
        public Single MinY = Single.MaxValue;
        public Single MinZ = Single.MaxValue;

        public Single MaxX = Single.MinValue;
        public Single MaxY = Single.MinValue;
        public Single MaxZ = Single.MinValue;

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
                    Vector3 v = new Vector3(MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]), MainGame.SingleParse(spli[3]));
                    if (v.X < MinX) MinX = v.X;
                    if (v.Y < MinY) MinY = v.Y;
                    if (v.Z < MinZ) MinZ = v.Z;
                    if (v.X > MaxX) MaxX = v.X;
                    if (v.Y > MaxY) MaxY = v.Y;
                    if (v.Z > MaxZ) MaxZ = v.Z;

                    cols.Add(v);
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
				if (StartEnds[lastIndex + 500] == null)
				{
					StartEnds[lastIndex][1] = this.indices.Count;
				}
				else
				{
					StartEnds[lastIndex][1] = StartEnds[lastIndex + 500][0];
					StartEnds[lastIndex + 500][1] = indices.Count;
				}
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
            if (Program.game.Collision < 1)
            return;
            List<VertexPositionNormalTexture> vpnt1 = new List<VertexPositionNormalTexture>(0);

            for (int i = 0; i < MainGame.ResourceFiles.Count + Program.game.Map.Supp.Count; i++)
            {
                Model mdl = null;
                if (i >= MainGame.ResourceFiles.Count)
                    mdl = Program.game.Map.Supp[i - MainGame.ResourceFiles.Count] as Model;
                else
                    mdl = MainGame.ResourceFiles[i] as Model;
                    
                if (mdl == null) continue;
                if (mdl.Collision == null) continue;



                int start_ = mdl.Collision.StartEnds[0][0];
                int end_ = mdl.Collision.StartEnds[0][1];


                for (int j = start_; j < end_; j++)
                {
                    Matrix mt = mdl.Skeleton.Bones[mdl.Skeleton.RootBone].LocalMatrix;
                    if (Vector3.Distance(mdl.Location, Vector3.Zero) > 0)
                    {
                        mt = Matrix.Identity;
                    }
                    Vector3 v1 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][0][0]], mt)
                        , mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v2 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][1][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v3 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][2][0]], mt), mdl.Rotate_matrix) + mdl.Location;

					var mt_mat = Matrix.CreateFromQuaternion(mt.Rotation);

					Vector3 n1 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][0][1]], mt_mat);
                    Vector3 n2 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][1][1]], mt_mat);
                    Vector3 n3 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][2][1]], mt_mat);

                    VertexPositionNormalTexture vp1 = new VertexPositionNormalTexture();
                    VertexPositionNormalTexture vp2 = new VertexPositionNormalTexture();
                    VertexPositionNormalTexture vp3 = new VertexPositionNormalTexture();
                    vp1.Position = v1; vp2.Position = v2; vp3.Position = v3;
                    vp1.Normal = n1; vp2.Normal = n2; vp3.Normal = n3;
                    vp1.TextureCoordinate = new Vector2(0.5f, 0.5f);
                    vp2.TextureCoordinate = new Vector2(0.5f, 0.5f);
                    vp3.TextureCoordinate = new Vector2(0.5f, 0.5f);

                    vpnt1.Add(vp3);
                    vpnt1.Add(vp2);
                    vpnt1.Add(vp1);
                }
            }


            VertexPositionNormalTexture[] vpnt = new VertexPositionNormalTexture[this.indices.Count*3];
			Model target = Program.game.mainCamera.Target;
			if (target!=null)
			{
				UpdateIndex(target.Location);
				int start = this.StartEnds[this.CurrIndex][0];
				int end = this.StartEnds[this.CurrIndex][1];
				for (int i = start; i < end; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						vpnt[i * 3 + j] = new VertexPositionNormalTexture
						{
							Position = this.cols[this.indices[i][2 - j][0]],
							TextureCoordinate = new Vector2(0.5f, 0.5f),
							Normal = this.norms[this.indices[i][j][1]]
						};
					}
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
            be.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpnt, 0, vpnt.Length / 3);
            vpnt = vpnt1.ToArray();
            be.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpnt, 0, vpnt.Length / 3);


            be.LightingEnabled = false;
            be.PreferPerPixelLighting = false;
        }


		/*public bool CanClimb(Model mdl)
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
        }*/

		static Vector3 NaNoutput = new Vector3(Single.NaN, Single.NaN, Single.NaN);

		public Vector3 GetCameraCollisionIntersect(Vector3 old_, Vector3 new_, bool try2)
		{
			Vector3 output = NaNoutput;
            Vector3 inter = Vector3.Zero;
            float closest = Single.MaxValue;

            UpdateIndex(Program.game.mainCamera.RealPosition);
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

				if (try2)
				{
					Vector3 middleSolo = ((v1 + v2 + v3) / 3f);
					Vector3 middle = middleSolo - old_;
					float distMiddle = Vector3.Distance(Vector3.Zero, middle);
					Vector3 middlep1 = middle / distMiddle;
					Vector3 toNew = new_ - old_;
					float distToNew = Vector3.Distance(Vector3.Zero, toNew);
					Vector3 toNewp1 = toNew / distToNew;
					Vector3 movedMiddle = old_ + toNewp1 * distMiddle;
					movedMiddle = (movedMiddle - middleSolo) * 0.1f;

					if (Vector3.Distance(Vector3.Zero, movedMiddle) < 3)
					{
						v1 += movedMiddle;
						v2 += movedMiddle;
						v3 += movedMiddle;
					}
				}
				if (intersect3D_RayTriangle(old_, new_, v1, v2, v3, out inter) != 0)
                {
                    float dist = Vector3.Distance(old_, inter);
                    if (dist > 0 && dist < closest && dist < Vector3.Distance(old_, new_))
					{
						if (normal.Y > 0.5f)
							output = inter + normal * 60f;
						else
							output = inter + normal * 30f;
						closest = dist;
                    }
                }
            }
            for (int i = 0; i < /*MainGame.ResourceFiles.Count +*/ Program.game.Map.Supp.Count; i++)
            {
                Model mdl = 
                /*if (i >= MainGame.ResourceFiles.Count)
                    mdl = Program.game.Map.Supp[i - MainGame.ResourceFiles.Count] as Model;
                else
                    mdl = MainGame.ResourceFiles[i] as Model;*/
                Program.game.Map.Supp[i] as Model;

                if (mdl == null) continue;
                if (mdl.Collision == null) continue;
				Model target = Program.game.mainCamera.Target;

				if (target != null && target.ResourceIndex == mdl.ResourceIndex) continue;


                start_ = mdl.Collision.StartEnds[0][0];
                end_ = mdl.Collision.StartEnds[0][1];

                for (int j = start_; j < end_; j++)
                {
                    Matrix mt = mdl.Skeleton.Bones[mdl.Skeleton.RootBone].LocalMatrix;
                    if (Vector3.Distance(mdl.Location, Vector3.Zero) > 0)
                        mt = Matrix.Identity;
                    Vector3 v1 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][0][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v2 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][1][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v3 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][2][0]], mt), mdl.Rotate_matrix) + mdl.Location;


					var mt_mat = Matrix.CreateFromQuaternion(mt.Rotation);
					Vector3 n1 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][0][1]], mt_mat);
                    Vector3 n2 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][1][1]], mt_mat);
                    Vector3 n3 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][2][1]], mt_mat);
                    Vector3 normal = (n1 + n2 + n3) / 3f;

					if (try2)
					{
						Vector3 middleSolo = ((v1 + v2 + v3) / 3f);
						Vector3 middle = middleSolo - old_;
						float distMiddle = Vector3.Distance(Vector3.Zero, middle);
						Vector3 middlep1 = middle / distMiddle;
						Vector3 toNew = new_ - old_;
						float distToNew = Vector3.Distance(Vector3.Zero, toNew);
						Vector3 toNewp1 = toNew / distToNew;
						Vector3 movedMiddle = old_ + toNewp1 * distMiddle;
						movedMiddle = (movedMiddle - middleSolo) * 0.1f;

						if (Vector3.Distance(Vector3.Zero, movedMiddle) < 3)
						{
							v1 += movedMiddle;
							v2 += movedMiddle;
							v3 += movedMiddle;
						}
					}

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
            }
			if (!try2 && Single.IsNaN(output.X))
			{
				return GetCameraCollisionIntersect(old_, new_, true);
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

		public static Vector3 cliffBackwards = new Vector3(0, 0, -0.05f);
		public static Matrix ps4 = Matrix.CreateRotationY(MainGame.PI / 4f);

        public static void MonitorCollision(Model sujet, ref Vector3 pos)
        {
			var collision = Program.game.MapSet ? Program.game.Map.Links[0] as Collision : null;

			if (sujet.Epaisseur < 0.1)
				return;

            bool jumping = sujet.Location.Y> sujet.LowestFloor+sujet.StairHeight && pos.Y > sujet.Location.Y;

            Vector3 refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
            Vector3 refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);

            Vector3 inter;
            Vector3 shadowAngle = Vector3.Zero;
            Vector3 globalBone0 = sujet.GetGlobalBone(0, Vector3.Zero);
            globalBone0.Y = sujet.MinVertex.Y;

            float tallest = Single.MinValue;
            float smallest = Single.MaxValue;
            shadowAngle = Vector3.Zero;
			
            Vector3 forwardCliffStart = new Vector3(0, 10f, sujet.Epaisseur * 1.1f);
            Vector3 forwardCliffEnd = new Vector3(0, -10f, sujet.Epaisseur * 1.1f);


            forwardCliffStart = Vector3.Transform(forwardCliffStart, sujet.Rotate_matrix);
            forwardCliffEnd = Vector3.Transform(forwardCliffEnd, sujet.Rotate_matrix);
            cliffBackwards = Vector3.Transform(cliffBackwards, sujet.Rotate_matrix);






			bool pushed = false;


            sujet.Attached = false;
			int moreRes = 0;
			if (Program.game.MapSet)
				moreRes = Program.game.Map.Supp.Count;

			if ((ScenePlayer.ScenePlaying  && ScenePlayer.AllowContactCollision) ||
                !ScenePlayer.ScenePlaying)
            if (!sujet.NPC && sujet.cState != Model.ControlState.Cliff)
            for (int i = 0; i < MainGame.ResourceFiles.Count + moreRes; i++)
            {
                Model mdl = null;
                if (i >= MainGame.ResourceFiles.Count)
                    mdl = Program.game.Map.Supp[i- MainGame.ResourceFiles.Count] as Model;
                else
                    mdl = MainGame.ResourceFiles[i] as Model;

				if (mdl == null) continue;
                if (sujet.ResourceIndex == mdl.ResourceIndex) continue;
				if (mdl.Collision == null)
				{
					if (mdl.Epaisseur < 1) continue;
							if (mdl.NPC || sujet.Masse <= mdl.Masse)
							{
								Vector2 sujetPos = new Vector2(pos.X, pos.Z);
								Vector2 mdlPos = new Vector2(mdl.Location.X, mdl.Location.Z);

								if (sujet.Location.Y > mdl.Location.Y-mdl.StairHeight*2f && sujet.Location.Y<mdl.Location.Y+mdl.MaxVertex.Y)
								{
									float hypo = Vector2.Distance(sujetPos, mdlPos);
									sujetPos -= mdlPos;
									sujetPos /= hypo;
									if (hypo < (mdl.Epaisseur + sujet.Epaisseur))
									{
										sujetPos = mdlPos + sujetPos * (mdl.Epaisseur + sujet.Epaisseur) * 1.1f;
										pos.X = sujetPos.X;
										pos.Z = sujetPos.Y;
									}
									/*else
									if (sujet.ResourceIndex == Program.game.mainCamera.Target.ResourceIndex &&  hypo < (mdl.Epaisseur + sujet.Epaisseur)*2f)
									{
										float val1 = (float)Math.Atan2(sujetPos.X, sujetPos.Y);
										float val2 = sujet.DestRotate + MainGame.PI;
										SrkBinary.MakePrincipal(ref val1);
										SrkBinary.MakePrincipal(ref val2);


										if (Math.Abs(val1-val2) < 1)
										{
											if (Program.game.mainCamera.LXY_read==0)
											{
												Program.game.mainCamera.LXY_read = 10;
												float cam_mdl_diff = (float)(Math.Atan2(Program.game.mainCamera.joyLY_, Program.game.mainCamera.joyLX_) + MathHelper.ToRadians(33));

												Program.game.mainCamera.joyLY_ = (float)Math.Sin(cam_mdl_diff);
												Program.game.mainCamera.joyLX_ = (float)Math.Cos(cam_mdl_diff);
											}
										}
									}*/
								}
							}
				}
				else


				for (int p=0;p< mdl.Collision.PolyCount; p++)
				{
					int start_ = mdl.Collision.StartEnds[p][0];
					int end_ = mdl.Collision.StartEnds[p][1];
					for (int j = start_; j < end_; j++)
					{
						Matrix mt = mdl.Skeleton.Bones[p].LocalMatrix;

						if (Vector3.Distance(mdl.Location,Vector3.Zero) > 0)
							mt = Matrix.Identity;
						Vector3 v1 = Vector3.Transform(
							Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][0][0]], mt), mdl.Rotate_matrix) + mdl.Location;
						Vector3 v2 = Vector3.Transform(
							Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][1][0]], mt), mdl.Rotate_matrix) + mdl.Location;
						Vector3 v3 = Vector3.Transform(
							Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][2][0]], mt), mdl.Rotate_matrix) + mdl.Location;

									var mt_mat = Matrix.CreateFromQuaternion(mt.Rotation);
						Vector3 n1 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][0][1]], mt_mat);
						Vector3 n2 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][1][1]], mt_mat);
						Vector3 n3 = Vector3.Transform(mdl.Collision.norms[mdl.Collision.indices[j][2][1]], mt_mat);
						Vector3 normal = (n1 + n2 + n3) / 3f;
                        
						if (mdl.NPC || sujet.Masse <= mdl.Masse)
						{
							bool newAttach = false;
							if (!ScenePlayer.ScenePlaying && normal.Y < -0.5)
								if (Inside(refPosTop, v1 - 10f * n1, v2 - 10f * n2, v3 - 10f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
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
								if (Inside(refPosBottom, v1 - 10f * n1, v2 - 10f * n2, v3 - 10f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
								{
									Vector3 direction = refPosBottom + normal * sujet.Epaisseur * 10f;
									if (jumping && normal.Y < -0.5)
										sujet.JumpCollisionCancel = true;
									if (intersect3D_RayTriangle(refPosBottom, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
									{
										pos.X = inter.X;
										pos.Z = inter.Z;
										refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
									}
								}

								if (Inside(refPosTop, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
								{
									Vector3 direction = refPosTop + normal * sujet.Epaisseur * 10f;
									if (intersect3D_RayTriangle(refPosTop, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
									{
										pos.X = inter.X;
										pos.Z = inter.Z;
										refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);
									}
								}
							}
							else if (normal.Y > 0.5)
							{
								if (intersect3D_RayTriangle(pos + globalBone0 + new Vector3(0, 50000, 0), pos + globalBone0 + new Vector3(0, -50000, 0), v1, v2, v3, out inter) == 1)
								{
									if (Math.Abs(inter.Y-pos.Y)<sujet.StairHeight)
									{
										if (inter.Y < smallest)
										{
											smallest = inter.Y;
										}
										if (inter.Y <= sujet.Location.Y + sujet.StairHeight * 1.5f && inter.Y > tallest)
										{
											shadowAngle = new Vector3(
											0,
											(float)Math.Atan2(normal.Z, normal.Y),
											(float)Math.Atan2(-normal.X, normal.Y));
											tallest = inter.Y;
										}
									}
								}
							}
							if (tallest > Single.MinValue / 2f)
							{
								if (tallest < sujet.Location.Y + sujet.StairHeight * 1.5f)
								{
									sujet.LowestFloor = tallest;
									newAttach = true;
								}
							}
							else if (smallest < Single.MaxValue / 2f)
							{
								if (smallest < sujet.Location.Y + sujet.StairHeight * 1.5f)
								{
									sujet.LowestFloor = smallest;
									newAttach = true;
								}
							}
							if (newAttach && Math.Abs(pos.Y-sujet.LowestFloor)<1)
							{
											if (collision!=null)
												collision.AttachTo(mdl, sujet);
							}
                        
							/*if (!ScenePlayer.ScenePlaying && Math.Abs(normal.Y) < 0.5f)
							{
								if (Inside(refPosBottom, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
								{
									Vector3 direction = refPosBottom + normal * sujet.Epaisseur * 10f;

									if (intersect3D_RayTriangle(refPosBottom, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
									{
										pushed = true;
										pos.X = inter.X;
										pos.Z = inter.Z;
										refPosBottom = pos + new Vector3(0, sujet.StairHeight, 0);
									}
								}

								if (Inside(refPosTop, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
								{
									Vector3 direction = refPosTop + normal * sujet.Epaisseur * 10f;
									if (intersect3D_RayTriangle(refPosTop, direction, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3, out inter) == 1)
									{
										pushed = true;
										pos.X = inter.X;
										pos.Z = inter.Z;
										refPosTop = pos + new Vector3(0, sujet.MaxVertex.Y, 0);
									}
								}
							}*/
						}
                    

					}

				}
            }
            sujet.ShadowMatrixSurface = Matrix.CreateFromYawPitchRoll(shadowAngle.X, shadowAngle.Y, shadowAngle.Z);
			bool cliff = sujet.cState == Model.ControlState.Cliff || sujet.cState == Model.ControlState.UnCliff;

			if (collision == null)
				return;
			collision.UpdateIndex(sujet.Location);

			int start = collision.StartEnds[collision.CurrIndex][0];
			int end = collision.StartEnds[collision.CurrIndex][1];
			for (int i = start; i < end; i++)
            {
                Vector3 v1 = collision.cols[collision.indices[i][0][0]];
                Vector3 v2 = collision.cols[collision.indices[i][1][0]];
                Vector3 v3 = collision.cols[collision.indices[i][2][0]];

                Vector3 n1 = collision.norms[collision.indices[i][0][1]];
                Vector3 n2 = collision.norms[collision.indices[i][1][1]];
                Vector3 n3 = collision.norms[collision.indices[i][2][1]];
                Vector3 normal = (n1 + n2 + n3) / 3f;


                if (!cliff && !ScenePlayer.ScenePlaying && normal.Y < -0.5)
                if (Inside(refPosTop, v1 - 20f * n1, v2 - 20f * n2, v3 - 20f * n3, v1 + sujet.Epaisseur * n1, v2 + sujet.Epaisseur * n2, v3 + sujet.Epaisseur * n3))
                {
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
                if (!cliff && !ScenePlayer.ScenePlaying && Math.Abs(normal.Y) < 0.5f)
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
					if (!cliff && !ScenePlayer.ScenePlaying && sujet.cState == Model.ControlState.Fall && !sujet.CliffCancel && (sujet.Links.Count == 0 || sujet.Links[0].ResourceIndex != 39))
                    {
                        if (intersect3D_RayTriangle(refPosTop + forwardCliffStart, refPosTop + forwardCliffEnd, v1, v2, v3, out inter) == 1 && inter.Y > refPosTop.Y + forwardCliffEnd.Y)
                        {
                            Vector3 coin = inter;


							int ordre = 0;  /* V1V2  V2V3  V3V1*/
							float nearest = Single.MaxValue;

							Vector3 va_vb = v1 * 1f;
							Vector3 step_va_vb = (v2 - v1) / Vector3.Distance(v2, v1);
							
							while (Vector3.Distance(va_vb, v2) > sujet.Epaisseur * 2f)
							{
								va_vb += step_va_vb;
								float dist2d = MainGame.Vector3Distance2D(va_vb, refPosTop);
								if (dist2d < nearest)
								{
									coin = va_vb * 1f;
									nearest = dist2d;
									ordre = 0;
								}
							}

							va_vb = v2 * 1f;
							step_va_vb = (v3 - v2) / Vector3.Distance(v3, v2);

							while (Vector3.Distance(va_vb, v3) > sujet.Epaisseur * 2f)
							{
								va_vb += step_va_vb;
								float dist2d = MainGame.Vector3Distance2D(va_vb, refPosTop);
								if (dist2d < nearest)
								{
									coin = va_vb * 1f;
									nearest = dist2d;
									ordre = 1;
								}
							}

							va_vb = v3 * 1f;
							step_va_vb = (v1 - v3) / Vector3.Distance(v1, v3);

							while (Vector3.Distance(va_vb, v1) > sujet.Epaisseur * 2f)
							{
								va_vb += step_va_vb;
								float dist2d = MainGame.Vector3Distance2D(va_vb, refPosTop);
								if (dist2d < nearest)
								{
									coin = va_vb *1f;
									nearest = dist2d;
									ordre = 2;
								}
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
								float newDest = angle + MainGame.PI / 2f;
								SrkBinary.MakePrincipal(ref newDest);

								sujet.DestRotate = newDest;
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
								

								/*string[] text = File.ReadAllLines(@"D:\Desktop\KHDebug\KHDebug\bin\DesktopGL\AnyCPU\Debug\Content\Models\P_EX100\Joints.txt");
								text = text[9].Split(':')[1].Split(',');
								sujet.CliffPosition.X = MainGame.SingleParse(text[0]);
								sujet.CliffPosition.Y = MainGame.SingleParse(text[1]);
								sujet.CliffPosition.Z = MainGame.SingleParse(text[2]);*/

								cliffBackwards = Vector3.Transform(sujet.CliffPosition, sujet.Rotate_matrix);
                                pos = coin - cliffBackwards;

								sujet.locBlock = 10;
								sujet.loc = pos;
								sujet.locAction = sujet.loc;

								return;
                            }

                            
                        }

                    }

                    if (intersect3D_RayTriangle(pos + globalBone0+new Vector3(0, 50000, 0), pos + globalBone0 + new Vector3(0, -50000, 0), v1, v2, v3, out inter) ==1)
                    {
						if (Single.IsNaN(sujet.LastLand.X))
						{
							if (inter.Y < smallest)
							{
								smallest = inter.Y;
							}
							if (inter.Y <= sujet.Location.Y + sujet.StairHeight * 1.5f && inter.Y > tallest)
							{
								shadowAngle = new Vector3(
								0,
								(float)Math.Atan2(normal.Z, normal.Y),
								(float)Math.Atan2(-normal.X, normal.Y));
								tallest = inter.Y;
							}
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

        public void AttachTo(Model reci,Model emi)
        {
            emi.Attached = true;
            //sujet.Attachment = Vector3.Transform(sujet.Location, Matrix.Invert(mdl.Skeleton.Bones[mdl.Skeleton.RootBone].localMatrix));

            if (!reci.Attachments.Contains(emi.ResourceIndex))
            {
                reci.Attachments.Add(emi.ResourceIndex);
                reci.AttachmentsModels.Add(emi);
            }
        }


		public static Vector3 NaN_intersect = new Vector3(Single.NaN, 0, 0);
		public Vector3 HasCol(Vector3 start, Vector3 end)
		{
			Vector3 intersect = NaN_intersect;

			UpdateIndex(start);
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

				if (normal.Y > 0.5)
					continue;
                if (intersect3D_RayTriangle(start,end,v1,v2,v3, out intersect) >0)
				{
                    break;
                }
				else
				{
					intersect.X = Single.NaN;
				}
            }
			/*for (int i = 0; i < Program.game.Map.Supp.Count; i++)
            {
                Model mdl = Program.game.Map.Supp[i] as Model;


                if (mdl == null) continue;
                if (mdl.Collision == null) continue;
                if (Program.game.mainCamera.Target.ResourceIndex == mdl.ResourceIndex) continue;

                
                for (int j = 0; j < mdl.Collision.indices.Count; j++)
                {
                    Matrix mt = mdl.Skeleton.Bones[mdl.Skeleton.RootBone].LocalMatrix;
                    if (Vector3.Distance(mdl.Location, Vector3.Zero) > 0)
                        mt = Matrix.Identity;
                    Vector3 v1 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][0][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v2 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][1][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    Vector3 v3 = Vector3.Transform(
                        Vector3.Transform(mdl.Collision.cols[mdl.Collision.indices[j][2][0]], mt), mdl.Rotate_matrix) + mdl.Location;
                    
                    if (intersect3D_RayTriangle(start, end, v1, v2, v3, out intersect) > 0)
                    {
                        output = true;
                        break;

                    }
                }
            }*/
			return intersect;
        }

		static float dot(Vector3 u, Vector3 v)
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
		static int intersect3D_RayTriangle(Vector3 P0, Vector3 P1, Vector3 V0, Vector3 V1, Vector3 V2, out Vector3 I)
        {
            I = NaNoutput;
            
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

            if (Math.Abs(Vector3.Distance(P0,P1) - (Vector3.Distance(P0, I) + Vector3.Distance(I, P1))) > Vector3.Distance(P0, P1) /10f)
            {
                I = NaNoutput;
                return 0;
            }

            return 1;                       // I is in T
        }
    }
}
