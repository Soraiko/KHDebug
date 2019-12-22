using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace KHDebug
{
    public static class Audio
    {
        public static string CurrentAmbient = "";
        public static int CurrentAmbientIndex = -1;
        public static List<SoundEffect> effects;
        public static List<SoundEffectInstance> effectInstances;

        public static List<float[]> DestVolumes;

        public static List<bool> loops;
        public static List<string> filenames;
        public static List<int> instaceCounts;
        public static List<int> playingMoments;
        public static List<string> names;
        public static List<byte[]> buffers;
        public static List<MemoryStream> streams;
        public static List<Model> emmiters;
		public static System.Threading.Thread loopThread;
		public static Random rnd;
        public static long oldTick = 0;
        public static void InitAudio()
        {
            if (loopThread == null)
            {
                loops = new List<bool>(0);
                effectInstances = new List<SoundEffectInstance>(0);
                DestVolumes = new List<float[]>(0);
                effects = new List<SoundEffect>(0);
                loops = new List<bool>(0);
                instaceCounts = new List<int>(0);
                filenames = new List<string>(0);
                playingMoments = new List<int>(0);
                names = new List<string>(0);
                buffers = new List<byte[]>(0);
                streams = new List<MemoryStream>(0);
                emmiters = new List<Model>(0);
				rnd = new Random(DateTime.Now.Millisecond);

                loopThread = new System.Threading.Thread(() => {

                    while (true)
                    {
						/*if (true||oldTick != Program.game.ticks)
                        {*/
						for (int i = 0; i < loops.Count; i++)
						{
							if (effectInstances[i] == null)
							{
								loops.RemoveAt(i);
								names.RemoveAt(i);
								playingMoments.RemoveAt(i);
								effectInstances.RemoveAt(i);
								DestVolumes.RemoveAt(i);
								emmiters.RemoveAt(i);
								CurrentAmbientIndex = names.IndexOf(CurrentAmbient);
								i--;
							}
						}
							for (int i = 0; i < loops.Count; i++)
                            {
								if (i == Audio.CurrentAmbientIndex)
								continue;
								var efi = effectInstances[i];
								if (efi == null)
									continue;
								Model target = Program.game.mainCamera.Target;


								if (emmiters[i] != null && target != null)
                                    {
                                        AudioEmitter emi = new AudioEmitter
                                        {
                                            Position = 
                                            (emmiters[i].Location) / 200f
                                        };
										if (!Single.IsNaN(emmiters[i].MinVertex.X))
											emi.Position += Vector3.Transform((emmiters[i].MinVertex + emmiters[i].MaxVertex) / 2f, emmiters[i].Rotate_matrix) / 200f;
                                        AudioListener reci = new AudioListener
                                        {
                                            //reci.Velocity = new Vector3((float)(Program.game.mainCamera.Target.Joystick * Math.Sin(Program.game.mainCamera.Target.Rotate)), 0, (float)(Program.game.mainCamera.Target.Joystick * Math.Cos(Program.game.mainCamera.Target.Rotate)));
                                            Position = target.Location / 200f
                                        };
										if (!Single.IsNaN(target.MinVertex.X))
											reci.Position += Vector3.Transform((target.MinVertex + target.MaxVertex) / 2f, target.Rotate_matrix) / 200f;

										emi.Position -= reci.Position;
                                        emi.Position = Vector3.Transform(emi.Position, Program.game.mainCamera.Yaw_backwards_matrix);
										

										//emi.Position += reci.Position;
										reci.Position = Vector3.Zero;


										efi.Apply3D(reci, emi);
                                }

								if (DestVolumes[i][0] < 0)
								{
									DestVolumes[i][0] *= -1f;
									efi.Volume = DestVolumes[i][0];
								}
						}
                        /*   oldTick = Program.game.ticks;
                       }*/
                    }
                });
                loopThread.Start();
            }
        }

        public static void UpdateAmbient()
        {
			if (Audio.NextBGM.Length > 0)
			{
				if (names.IndexOf(Audio.NextBGM)<0)
				Audio.Play(Audio.NextBGM, true, null, 100);
				Audio.BGM = Audio.NextBGM;
				Audio.NextBGM = "";
			}

			int ind = -1;
            for (int i=0;i<names.Count;i++)
			{
				var efi = effectInstances[i];
				var ff = DestVolumes[i];
				var emii = emmiters[i];
				string name = names[i];
				if (efi == null)
					continue;

				if (name.ToLower().Contains("bgm"))
				{
					if (Program.game.WaitMap && efi.Volume > 0)
					{
						efi.Volume += ((0f) - efi.Volume) / 50f;
						if (efi.Volume < 0.001)
							efi.Volume = 0;
					}
					else if (name!= Audio.BGM)
					{
						efi = null;
					}
				}
				else
				if (name.Length> 22 && name[22]=='A' && emii == null)
				{
					//Console.WriteLine(name+"   "+ efi.Volume);
					if (name == CurrentAmbient)
					{
						ind = i;
						if (ff[1] < 1.01f)
							ff[1] = 50f;
					}

					if (Program.game.WaitMap)
					{
						if (efi.Volume > 0)
						{
							efi.Volume += ((0f) - efi.Volume) / 50f;
						}
					}
					else if (!(ff[0] < 0))
					{
						efi.Volume += (ff[0] - efi.Volume) / ff[1];
					}
					if (!(ff[0]<0) && ff[0] < efi.Volume && efi.Volume < 0.000001)
						efi.Volume = 0;
				}
			}
			if (CurrentAmbient.Length < 1)
				return;
			if (ind<0)
            {
                Audio.Play(CurrentAmbient, true, null, 0);
				ind = names.IndexOf(CurrentAmbient);
            }
        }

        public static void SetVolume(string path, byte vol)
        {
            if (path.Contains(";"))
            {
                string[] spli = path.Split(';');
                for (int i = 0; i < spli.Length; i++)
                    SetVolume(spli[i], vol);
                return;
            }
            int index = filenames.IndexOf(path);
            if (index > -1)
            {
                effectInstances[index].Volume = vol / 400f;
            }
        }
        public static int moment = 0;
        public static string NextBGM = "";
        public static string BGM = "";

		public static void Play(string path, bool loop, Model emmiter, byte vol)
        {
            if (path.Contains(";"))
            {
                string[] spli = path.Split(';');
                path = spli[rnd.Next(0, spli.Length)];
            }

            bool wavLoop = false;
            if (path.Contains("wavloop"))
            {
                wavLoop = true;
                path = path.Remove(path.Length - 4, 4);
                loop = true;
            }

            int index = filenames.IndexOf(path);
            if (index < 0)
            {
                try
                {
                    var oupt = Console.Out;
                    Console.SetOut(TextWriter.Null);
                    index = filenames.Count;
                    filenames.Add(path);

                    var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    fs.Close();

                    buffers.Add(data);
                    MemoryStream ms = new MemoryStream(data);
                    streams.Add(ms);
                    var se_ = SoundEffect.FromStream(ms);
                    effects.Add(se_);
                    instaceCounts.Add(0);
                    Console.SetOut(oupt);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }


            try
            {
                int stoppedFound = -1;
                for (int i = 0; i < names.Count; i++)
                {
                    if (names[i] == path)
                    {
                        if (wavLoop)
                            return;
                        if (effectInstances[i].State != SoundState.Playing)
                        {
                            stoppedFound = i;
                            break;
                        }
                    }
                }
                instaceCounts[index]++;
                if (stoppedFound > -1)
                {
					Model target = Program.game.mainCamera.Target;
					if (emmiter != null && target!= null)
                    {
                        AudioEmitter emi = new AudioEmitter
                        {
                            Position = 
                            (emmiter.Location + Vector3.Transform((emmiter.MinVertex + emmiter.MaxVertex) / 2f, emmiter.Rotate_matrix)) / 200f
                        };
                        AudioListener reci = new AudioListener
                        {
                            Position = (target.Location + Vector3.Transform((target.MinVertex + target.MaxVertex) / 2f, target.Rotate_matrix)) / 200f
                        };
                        emi.Position -= reci.Position;
                        emi.Position = Vector3.Transform(emi.Position, Program.game.mainCamera.Yaw_backwards_matrix);
                        reci.Position = Vector3.Zero;

                        effectInstances[stoppedFound].Apply3D(reci, emi);
                    }

					DestVolumes[stoppedFound][0] = (-vol / 400f);
					DestVolumes[stoppedFound][1] = 1f;
					effectInstances[stoppedFound].Volume = 0f;
					effectInstances[stoppedFound].Play();
                    playingMoments[stoppedFound] = moment;
                }
                else


                if (instaceCounts[index] < 4)
                {
                    var se = effects[index].CreateInstance();

					Model target = Program.game.mainCamera.Target;
					if (emmiter != null && target != null)
					{
                        AudioEmitter emi = new AudioEmitter
                        {
                            Position = 
                            (emmiter.Location + Vector3.Transform((emmiter.MinVertex + emmiter.MaxVertex) / 2f, emmiter.Rotate_matrix)) / 200f
                        };
                        AudioListener reci = new AudioListener
                        {
                            Position = (target.Location + Vector3.Transform((target.MinVertex + target.MaxVertex) / 2f, target.Rotate_matrix)) / 200f
                        };
                        emi.Position -= reci.Position;
                        emi.Position = Vector3.Transform(emi.Position, Program.game.mainCamera.Yaw_backwards_matrix);
                        reci.Position = Vector3.Zero;

                        se.Apply3D(reci, emi);
                    }

                    se.IsLooped = loop;
					se.Volume = 0f;
					se.Play();
                    names.Add(path);
					CurrentAmbientIndex = names.IndexOf(CurrentAmbient);

					playingMoments.Add(moment);

                    effectInstances.Add(se);
                    DestVolumes.Add(new float[] { -vol / 400f, 1f });

					emmiters.Add(emmiter);
                    loops.Add(loop);
                }
                else
                {
                    int earliest = int.MaxValue;
                    int earlyIndex = -1;

                    for (int i=0;i< names.Count;i++)
                    {
						if (names[i] == path && playingMoments[i] < earliest)
                        {
                            earliest = playingMoments[i];
                            earlyIndex = i;
                        }
                    }
                    if (earlyIndex>-1)
                    {
                        effectInstances[earlyIndex].Stop();

						Model target = Program.game.mainCamera.Target;
						if (emmiter != null && target != null)
						{
                            AudioEmitter emi = new AudioEmitter
                            {
                                Position = 
                                (emmiter.Location + Vector3.Transform((emmiter.MinVertex + emmiter.MaxVertex) / 2f, emmiter.Rotate_matrix)) / 200f
                            };
                            AudioListener reci = new AudioListener
                            {
                                Position = (target.Location + Vector3.Transform((target.MinVertex + target.MaxVertex) / 2f, target.Rotate_matrix)) / 200f
                            };
                            emi.Position -= reci.Position;
                            emi.Position = Vector3.Transform(emi.Position, Program.game.mainCamera.Yaw_backwards_matrix);
                            reci.Position = Vector3.Zero;

                            effectInstances[earlyIndex].Apply3D(reci, emi);
                        }

                        DestVolumes[earlyIndex][0] = (-vol / 400f);
                        DestVolumes[earlyIndex][1] = 1f;
						effectInstances[earlyIndex].Volume = 0f;
						effectInstances[earlyIndex].Play();
                        playingMoments[earlyIndex] = moment;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
            moment++;
        }

    }
}
