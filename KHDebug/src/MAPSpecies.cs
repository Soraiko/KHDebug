using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public static class MAPSpecies
    {
        static List<string> starts = new List<string>(0);
        static List<bool> FirstPass = new List<bool>(0);

        public static void ApplySpecies(Model map)
        {
            int index = starts.IndexOf(map.Name);
            if (index<0)
            {
                index = starts.Count;
                starts.Add(map.Name);
                FirstPass.Add(true);
            }
            switch (map.ResourceIndex)
            {
                case 2:
                    for (int i=0;i<map.Supp.Count;i++)
                    {
                        Moveset mset = map.SuppMsets[i];
                        Model mdl = map.Supp[i];
                        
                        if (FirstPass[index] && mdl.ResourceIndex == 8 /*.Name == "TT08-mdl-Bird"*/)
                        {
                            mset.PlayingIndex = 0;
                            mset.FrameStep = 1.5f;
                        }
                        if (FirstPass[index] && mdl.ResourceIndex == 9 /*.Name == "TT08-mdl-Trees"*/)
                        {
                            mset.PlayingIndex = 0;
                            mset.FrameStep = 1f;
                        }
                        if (mdl.ResourceIndex == 7 /*.Name == "TT08-mdl-Bells"*/ && mset !=null)
                        {
                            float newRot = 1.05f - ((DateTime.Now.Minute) / 60f) * (MainGame.PI * 2f);
                            if (Math.Abs(mset.Skeleton.Bones[2].RotateZ - newRot) > 0.01)
                            {
                                if (!FirstPass[index])
                                {
                                    mset.PlayingIndex = 0;
                                }
                                mset.Skeleton.Bones[2].RotateZ = newRot;
                            }
                             newRot = -1.05f - (DateTime.Now.Minute / 5) * (MainGame.PI * 2f);
                            if (Math.Abs(mset.Skeleton.Bones[1].RotateZ - newRot) > 0.01)
                            {
                                if (!FirstPass[index])
                                {
                                    mset.PlayingIndex = 1;
                                }
                                mset.Skeleton.Bones[1].RotateZ = newRot;
                            }
                        }

                        if (mdl.ResourceIndex == 5 /*.Name=="TT08-Clock"*/)
                        {
                            if (FirstPass[index])
                            {
                                Audio.Play(@"Content\Effects\Audio\Ambient\Shared\gearsLoop.wav", true, mdl, 50);
                            }
                            /*try
                            {
                                string[] file = System.IO.File.ReadAllLines("fog.txt");*/
                            float newRot = -1.05f - ((DateTime.Now.Hour % 12) / 12f) * (MainGame.PI * 2f);
                            if (Math.Abs(mdl.Skeleton.Bones[2].RotateZ - newRot)>0.01)
                            {
                                if (!FirstPass[index])
                                {
                                    Audio.Play(@"Content\Effects\Audio\Ambient\Shared\tick.wav", false, mdl, 50);
                                }
                                mdl.Skeleton.Bones[2].RotateZ = newRot;
                            }
                            Matrix m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[2].RotateZ) * Matrix.CreateTranslation(mdl.Skeleton.Bones[2].localMatrix.Translation);
                            mdl.Skeleton.Bones[2].localMatrix = m;
                            //
                            newRot = 1.05f - ((DateTime.Now.Minute) / 60f) * (MainGame.PI * 2f);
                            if (Math.Abs(mdl.Skeleton.Bones[3].RotateZ - newRot) > 0.01)
                            {
                                if (!FirstPass[index])
                                    Audio.Play(@"Content\Effects\Audio\Ambient\Shared\tick.wav", false, mdl, 50);
                                mdl.Skeleton.Bones[3].RotateZ = newRot;
                            }
                            m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[3].RotateZ) * Matrix.CreateTranslation(mdl.Skeleton.Bones[3].localMatrix.Translation);
                                mdl.Skeleton.Bones[3].localMatrix = m;

                                m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[1].RotateZ += 0.001f) * Matrix.CreateTranslation(mdl.Skeleton.Bones[1].localMatrix.Translation);
                                mdl.Skeleton.Bones[1].localMatrix = m;

                                m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[5].RotateZ += 0.01f) * Matrix.CreateTranslation(mdl.Skeleton.Bones[5].localMatrix.Translation);
                                mdl.Skeleton.Bones[5].localMatrix = m;

                                m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[4].RotateZ -= 0.001f) * Matrix.CreateTranslation(mdl.Skeleton.Bones[4].localMatrix.Translation);
                                mdl.Skeleton.Bones[4].localMatrix = m;

                                m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[6].RotateZ -= 0.01f) * Matrix.CreateTranslation(mdl.Skeleton.Bones[6].localMatrix.Translation);
                                mdl.Skeleton.Bones[6].localMatrix = m;


                                Skeleton.Wrap(mdl.Skeleton);
                            mdl.RecreateVertexBuffer(true);
                            /*}
                            catch
                            {

                            }*/
                        }
                    }
                break;
            }
            FirstPass[index] = false;
        }
    }
}
