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

        public static void ApplySpecies(MAP map)
        {

            switch (map.ResourceIndex)
            {
                case 8:
                    for (int i=0;i<map.Supp.Count;i++)
                    {
                        Moveset mset = map.SuppMsets[i];
                        Model mdl = map.Supp[i];

						/*if (map.JustLoaded && mdl.ResourceIndex == 14) //TT08-mdl-Bird
                        {
                            mset.PlayingIndex = 0;
                            mset.FrameStep = 1.5f;
                        }
                        if (map.JustLoaded && mdl.ResourceIndex == 17 ) //TT08-mdl-Trees
                        {
                            mset.PlayingIndex = 0;
                            mset.FrameStep = 1f;
                        }*/

						if (mdl.ResourceIndex == 13 /*.Name == "TT08-mdl-Bells"*/ && mset !=null)
                        {
                            float newRot = 1.05f - ((DateTime.Now.Minute) / 60f) * (MainGame.PI * 2f);
                            if (Math.Abs(mset.Skeleton.Bones[2].RotateZ - newRot) > 0.01)
							{
								if (!map.JustLoaded)
                                    mset.PlayingIndex = 0;
                                mset.Skeleton.Bones[2].RotateZ = newRot;
                            }
                            newRot = -1.05f - (DateTime.Now.Minute / 15) * (MainGame.PI * 2f);
                            if (Math.Abs(mset.Skeleton.Bones[1].RotateZ - newRot) > 0.01)
                            {
								if (!map.JustLoaded)
                                    mset.PlayingIndex = 1;
                                mset.Skeleton.Bones[1].RotateZ = newRot;
                            }
                        }


						if (mdl.ResourceIndex == 16 /*.Name=="TT08-mdl-Tram"*/)
						{
							/*if (map.JustLoaded)
								Audio.Play(@"Content\Effects\Audio\Sounds\Shared\tramMotor.wav", true, mdl, 0);*/

							if (mset.FrameStep > 0.05f)
								mset.FrameStep = 0f;

							if (!map.JustLoaded)
							{
								if (mset.FrameStep < 0.01f)
								{
									if (mdl.DestOpacity > 0.9999f)
									{
										Audio.Play(@"Content\Effects\Audio\Sounds\Shared\tramBrake.wav", false, mdl, 50);
										mdl.DestOpacity = 0.9999f;
									}
								}
								else
									mdl.DestOpacity = 1f;
								int ind = Audio.names.IndexOf(@"Content\Effects\Audio\Sounds\Shared\tramMotor.wav");
								if (ind > -1)
									Audio.effectInstances[ind].Volume += (mset.FrameStep - Audio.effectInstances[ind].Volume) / 5f;
							}
							
						}

						if (mdl.ResourceIndex == 11 /*.Name=="TT08-Clock"*/)
						{
                            /*try
                            {
                                string[] file = System.IO.File.ReadAllLines("fog.txt");*/
                            float newRot = -1.05f - ((DateTime.Now.Hour % 12) / 12f) * (MainGame.PI * 2f);
                            if (Math.Abs(mdl.Skeleton.Bones[2].RotateZ - newRot)>0.01)
							{
								if (!map.JustLoaded)
								{
                                    Audio.Play(@"Content\Effects\Audio\Sounds\Shared\tick.wav", false, mdl, 50);
                                }
                                mdl.Skeleton.Bones[2].RotateZ = newRot;
                            }
                            Matrix m = Matrix.CreateRotationZ(mdl.Skeleton.Bones[2].RotateZ) * Matrix.CreateTranslation(mdl.Skeleton.Bones[2].localMatrix.Translation);
                            mdl.Skeleton.Bones[2].localMatrix = m;
                            //
                            newRot = 1.05f - ((DateTime.Now.Minute) / 60f) * (MainGame.PI * 2f);
                            if (Math.Abs(mdl.Skeleton.Bones[3].RotateZ - newRot) > 0.01)
							{
								if (!map.JustLoaded)
									Audio.Play(@"Content\Effects\Audio\Sounds\Shared\tick.wav", false, mdl, 50);
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
		}
    }
}
