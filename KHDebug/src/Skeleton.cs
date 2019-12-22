using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace KHDebug
{
    public class Skeleton
    {
        public void RememberSRT()
        {
            for (int i = 0; i <this.Bones.Count;i++)
            {
                /*this.Bones[i].RememberRotateX = this.Bones[i].RotateX;
                this.Bones[i].RememberRotateY = this.Bones[i].RotateY;
                this.Bones[i].RememberRotateZ = this.Bones[i].RotateZ;
                this.Bones[i].RememberTranslateX = this.Bones[i].TranslateX;
                this.Bones[i].RememberTranslateY = this.Bones[i].TranslateY;
                this.Bones[i].RememberTranslateZ = this.Bones[i].TranslateZ;
                this.Bones[i].RememberScaleX = this.Bones[i].ScaleX;
                this.Bones[i].RememberScaleY = this.Bones[i].ScaleY;
                this.Bones[i].RememberScaleZ = this.Bones[i].ScaleZ;
                this.Bones[i].DirtyRememberMatrix = true;*/

                this.Bones[i].rememberMatrix = this.Bones[i].localMatrix;
            }
        }

		public int RootBone = 0;
        public int HeadBone = -1;
        public int NeckBone = -1;
		public bool NeckBoneSet = false;
		public float NeckBoneDest = 0;

		public int LeftHandBone = -1;

        public int LeftLeg = -1;
        public int LeftKnee = -1;
        public int LeftFoot = -1;

        public int RightLeg = -1;
        public int RightKnee = -1;
        public int RightFoot = -1;

        public Vector3 LeftLegV3;
        public Vector3 LeftKneeV3;
        public Vector3 LeftFootV3;

        public Vector3 ZeroPosition = new Vector3(Single.NaN,0,0);
        public Vector3 ZeroPositionFight = new Vector3(Single.NaN,0,0);
        public Vector3 MaxVertex;
        public Vector3 MinVertex;
        
        
        public List<Bone> Bones;

        public Skeleton()
		{
            this.Bones = new List<Bone>(0);
            this.HeadBone = 0;
            this.LeftHandBone = 0;
            this.LeftLeg = 0;
            this.LeftKnee = 0;
            this.LeftKnee = 0;
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < this.Bones.Count; i++)
                if (this.Bones[i].Name == name)
                    return i;
            return -1;
        }


        public void ComputeMatrices()
        {
            for (int u = 0; u < this.Bones.Count; u++)
                this.Bones[u].GlobalMatrix = this.Bones[u].LocalMatrix;
            for (int u = 0; u < this.Bones.Count; u++)
            {
                Matrix mat = this.Bones[u].GlobalMatrix;
                for (int v = 0; v < this.Bones.Count; v++)
                {
                    if (u == v) continue;
                    if (this.Bones[v].Parent != null && this.Bones[v].Parent.Name == this.Bones[u].Name)
                    {
                        this.Bones[v].GlobalMatrix *= mat;
                    }
                }
            }
            for (int u = 0; u < this.Bones.Count; u++)
                this.Bones[u].rememberMatrix = this.Bones[u].localMatrix;
        }

        /*public void ComputeMatrices2(Bone bone)
        {
            bool dirty = bone.DirtyMatrix;
            if (bone.DirtyMatrix)
            {
                if (bone.Parent != null)
                    bone.Matrix *= bone.Parent.GlobalMatrix;
                bone.DirtyMatrix = false;
            }

            for (int i = 0; i < this.Bones.Count; i++)
            {
                if (this.Bones[i].Parent != null && this.Bones[i].Parent.Name == bone.Name)
                {
                    if (dirty)
                        this.Bones[i].DirtyMatrix = true;
                    ComputeMatrices(this.Bones[i]);
                }
            }
        }*/


        public static void Wrap(Skeleton sk)
        {
            for (int u = 0; u < sk.Bones.Count; u++)
                sk.Bones[u].GlobalMatrix = sk.Bones[u].LocalMatrix;
            for (int u = 0; u < sk.Bones.Count; u++)
            {
                for (int v = 0; v < sk.Bones.Count; v++)
                {
                    if (u == v) continue;
                    if (sk.Bones[v].Parent != null && sk.Bones[v].Parent == sk.Bones[u])
                    {
                        sk.Bones[v].GlobalMatrix *= sk.Bones[u].GlobalMatrix;
                    }
                }
            }
        }

        public static Quaternion FromRotationMatrix(Matrix m)
        {
            float t = 1.0f;
            float s = 0.5f;
            if (m[0] + m[5] + m[10] > 0.0f)
            {
                t += m[0] + m[5] + m[10];
                s *= 1f / (float)Math.Sqrt(t);
                return new Quaternion(m[6] - m[9], m[8] - m[2], m[1] - m[4], t) * s;
            }
            if (m[0] > m[5] && m[0] > m[10])
            {
                t += m[0] - m[5] - m[10];
                s *= 1f / (float)Math.Sqrt(t);
                return new Quaternion(t, m[1] + m[4], m[8] + m[2], m[6] - m[9]) * s;
            }
            if (m[5] > m[10])
            {
                t += -m[0] + m[5] - m[10];
                s *= 1f / (float)Math.Sqrt(t);
                return new Quaternion(m[1] + m[4], t, m[6] + m[9], m[8] - m[2]) * s;
            }
            t += -m[0] - m[5] + m[10];
            s *= 1f / (float)Math.Sqrt(t);
            return new Quaternion(m[8] + m[2], m[6] + m[9], t, m[1] - m[4]) * s;
        }

        public static Quaternion FromAxisAngle(Vector3 axis, float angle)
        {
            Vector3 vectorPart = (float)Math.Sin(angle * 0.5f) * axis;
            return new Quaternion(vectorPart.X, vectorPart.Y, vectorPart.Z,
                (float)Math.Cos(angle * 0.5f));
        }
    }
}
