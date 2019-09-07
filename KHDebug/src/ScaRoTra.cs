using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KHDebug
{
    public class ScaRoTra
    {
        float rX = 0;
        float rY = 0;
        float rZ = 0;

        float tX = 0;
        float tY = 0;
        float tZ = 0;

        float sX = 0;
        float sY = 0;
        float sZ = 0;

        public bool DirtyMatrix = true;
        public float ScaleX { get { return this.sX; } set { this.sX = value; this.DirtyMatrix = true; } }
        public float ScaleY { get { return this.sY; } set { this.sY = value; this.DirtyMatrix = true; } }
        public float ScaleZ { get { return this.sZ; } set { this.sZ = value; this.DirtyMatrix = true; } }
        
        public float RotateX { get { return this.rX; } set { this.rX = value; this.DirtyMatrix = true; } }
        public float RotateY { get { return this.rY; } set { this.rY = value; this.DirtyMatrix = true; } }
        public float RotateZ { get { return this.rZ; } set { this.rZ = value; this.DirtyMatrix = true; } }
        
        public float TranslateX { get { return this.tX; } set { this.tX = value; this.DirtyMatrix = true; } }
        public float TranslateY { get { return this.tY; } set { this.tY = value; this.DirtyMatrix = true; } }
        public float TranslateZ { get { return this.tZ; } set { this.tZ = value; this.DirtyMatrix = true; } }

        public Matrix matrix;

        public Matrix Matrix
        {
            get
            {
                if (this.DirtyMatrix)
                {
                    Quaternion q2 = FromAxisAngle(new Vector3(0, 0, 1), this.RotateZ) *
                        FromAxisAngle(new Vector3(0, 1, 0), this.RotateY) *
                        FromAxisAngle(new Vector3(1, 0, 0), this.RotateX);

                    this.matrix = Matrix.CreateFromQuaternion(q2) * Matrix.CreateTranslation(new Vector3(this.TranslateX, this.TranslateY, this.TranslateZ));
                    this.DirtyMatrix = false;
                }
                return this.matrix;
            }
        }

        public static Quaternion FromAxisAngle(Vector3 axis, float angle)
        {
            Vector3 vectorPart = (float)Math.Sin(angle * 0.5f) * axis;
            return new Quaternion(vectorPart.X, vectorPart.Y, vectorPart.Z,
                (float)Math.Cos(angle * 0.5f));
        }

        public ScaRoTra(Matrix mat)
        {
            this.matrix = Matrix.Identity;
            Microsoft.Xna.Framework.Vector3 scale = Microsoft.Xna.Framework.Vector3.Zero;
            Microsoft.Xna.Framework.Vector3 translate = Microsoft.Xna.Framework.Vector3.Zero;
            Microsoft.Xna.Framework.Quaternion q = Microsoft.Xna.Framework.Quaternion.Identity;


            mat.Decompose(out scale, out q, out translate);

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
    }
}
