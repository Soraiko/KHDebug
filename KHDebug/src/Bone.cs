using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace KHDebug
{
    public class Bone
    {
        public float Visibility { get; set; }
        public int Flag1 { get; set; }
        public int Flag2 { get; set; }

        public bool DirtyMatrix = true;
        public bool DirtyRememberMatrix = true;

        public float RememberScaleX;
        public float RememberScaleY;
        public float RememberScaleZ;

        public float RememberRotateX;
        public float RememberRotateY;
        public float RememberRotateZ;

        public float RememberTranslateX;
        public float RememberTranslateY;
        public float RememberTranslateZ;

        public float ScaleX { get { return this.sX; } set { this.sX = value; this.DirtyMatrix = true; } }
        public float ScaleY { get { return this.sY; } set { this.sY = value; this.DirtyMatrix = true; } }
        public float ScaleZ { get { return this.sZ; } set { this.sZ = value; this.DirtyMatrix = true; } }

        public float sX;
        public float sY;
        public float sZ;

        public float RotateX { get { return this.rX; } set { this.rX = value; this.DirtyMatrix = true; } }
        public float RotateY { get { return this.rY; } set { this.rY = value; this.DirtyMatrix = true; } }
        public float RotateZ { get { return this.rZ; } set { this.rZ = value; this.DirtyMatrix = true; } }

        public float rX;
        public float rY;
        public float rZ;

        public float TranslateX { get { return this.tX; } set { this.tX = value; this.DirtyMatrix = true; } }
        public float TranslateY { get { return this.tY; } set { this.tY = value; this.DirtyMatrix = true; } }
        public float TranslateZ { get { return this.tZ; } set { this.tZ = value; this.DirtyMatrix = true; } }

        public float tX;
        public float tY;
        public float tZ;
        
        public void GetSRT(Microsoft.Xna.Framework.Matrix m)
        {
            Microsoft.Xna.Framework.Vector3 scale = Microsoft.Xna.Framework.Vector3.Zero;
            Microsoft.Xna.Framework.Vector3 translate = Microsoft.Xna.Framework.Vector3.Zero;
            Microsoft.Xna.Framework.Quaternion q = Microsoft.Xna.Framework.Quaternion.Identity;


            m.Decompose(out scale, out q, out translate);
            
            this.tX = translate.X;
            this.tY = translate.Y;
            this.tZ = translate.Z;

            this.sX = scale.X;
            this.sY = scale.Y;
            this.sZ = scale.Z;

            float Singularity = 0.499f;

            float ww = q.W * q.W;
            float xx = q.X * q.X;
            float yy = q.Y * q.Y;
            float zz = q.Z * q.Z;
            float lengthSqd = xx + yy + zz + ww;
            float singularityTest = q.Y * q.W - q.X * q.Z;
            float singularityValue = Singularity * lengthSqd;
            if (singularityTest > singularityValue)
            {
                this.rX = (-2f * (float)Math.Atan2(q.Z, q.W));
                this.rY = (90.0f) * (float)(Math.PI / 180);
                this.rZ = (0f) * (float)(Math.PI / 180);
            }
            else
            {
                if (singularityTest < -singularityValue)
                {
                    this.rX = (2 * (float)Math.Atan2(q.Z, q.W));
                    this.rY = (-90.0f) * (float)(Math.PI / 180);
                    this.rZ = (0f) * (float)(Math.PI / 180);
                }
                else
                {
                    this.rX = ((float)Math.Atan2(2.0f * (q.Y * q.Z + q.X * q.W), 1.0f - 2.0f * (xx + yy)));
                    this.rY = ((float)Math.Asin(2.0f * singularityTest / lengthSqd));
                    this.rZ = ((float)Math.Atan2(2.0f * (q.X * q.Y + q.Z * q.W), 1.0f - 2.0f * (yy + zz)));
                }
            }

            while (this.rX > Math.PI) this.rX -= (float)(2 * Math.PI);
            while (this.rX < -Math.PI) this.rX += (float)(2 * Math.PI);
            while (this.rY > Math.PI) this.rY -= (float)(2 * Math.PI);
            while (this.rY < -Math.PI) this.rY += (float)(2 * Math.PI);
            while (this.rZ > Math.PI) this.rZ -= (float)(2 * Math.PI);
            while (this.rZ < -Math.PI) this.rZ += (float)(2 * Math.PI);
        }


        public Bone Parent
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        

        public static string ToString(Microsoft.Xna.Framework.Matrix m)
        {
            string s = "";
            s += m.M11.ToString("0.000000") + " " + m.M12.ToString("0.000000") + " " + m.M13.ToString("0.000000") + " " + m.M14.ToString("0.000000") + "\r\n";
            s += m.M12.ToString("0.000000") + " " + m.M22.ToString("0.000000") + " " + m.M23.ToString("0.000000") + " " + m.M24.ToString("0.000000") + "\r\n";
            s += m.M13.ToString("0.000000") + " " + m.M32.ToString("0.000000") + " " + m.M33.ToString("0.000000") + " " + m.M34.ToString("0.000000") + "\r\n";
            s += m.M14.ToString("0.000000") + " " + m.M42.ToString("0.000000") + " " + m.M43.ToString("0.000000") + " " + m.M44.ToString("0.000000") + "\r\n";
            return s;
        }

        public Matrix rememberMatrix;
        public Matrix localMatrix;

        public Matrix GlobalMatrix;

        public Matrix RememberMatrix
        {
            get
            {
                if (false&&this.DirtyRememberMatrix)
                {
                    Quaternion q2 = ScaRoTra.FromAxisAngle(new Vector3(0, 0, 1), this.RememberRotateZ) *
                        ScaRoTra.FromAxisAngle(new Vector3(0, 1, 0), this.RememberRotateY) *
                        ScaRoTra.FromAxisAngle(new Vector3(1, 0, 0), this.RememberRotateX);

                    this.rememberMatrix = Matrix.CreateFromQuaternion(q2) * Matrix.CreateTranslation(new Vector3(this.RememberTranslateX, this.RememberTranslateY, this.RememberTranslateZ));
                    this.DirtyRememberMatrix = false;
                }
                return this.rememberMatrix;
            }
        }

        public Matrix LocalMatrix
        {
            get
            {
                if (false && this.DirtyMatrix)
                {
                    Quaternion q2 = ScaRoTra.FromAxisAngle(new Vector3(0, 0, 1), this.RotateZ) *
                        ScaRoTra.FromAxisAngle(new Vector3(0, 1, 0), this.RotateY) *
                        ScaRoTra.FromAxisAngle(new Vector3(1, 0, 0), this.RotateX);

                    this.localMatrix = Matrix.CreateFromQuaternion(q2) * Matrix.CreateTranslation(new Vector3(this.TranslateX, this.TranslateY, this.TranslateZ));
                    this.DirtyMatrix = false;
                }
                return this.localMatrix;
            }
        }
        
        

        public Bone(string name_)
        {
            this.Visibility = 1f;
            this.Flag1 = 0;
            this.Flag2 = 0;
            this.Name = name_;
            this.DirtyMatrix = true;
        }
    }
}
