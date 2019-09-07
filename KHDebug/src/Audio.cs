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
        public static List<SoundEffect> effects;
        public static List<SoundEffectInstance> effectInstances;
        public static List<bool> loops;
        public static List<string> filenames;
        public static List<int> instaceCounts;
        public static List<int> playingMoments;
        public static List<string> names;
        public static List<byte[]> buffers;
        public static List<MemoryStream> streams;
        public static List<Model> emmiters;
        public static System.Threading.Thread loopThread;
        static Random rnd;

        public static void InitAudio()
        {
            if (loopThread == null)
            {
                loops = new List<bool>(0);
                effectInstances = new List<SoundEffectInstance>(0);

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
                        for (int i = 0; i < loops.Count; i++)
                        {
                            if (effectInstances[i] != null && emmiters[i] != null && Program.game.mainCamera.Target != null)
                            {
                                AudioEmitter emi = new AudioEmitter
                                {
                                    Position = (emmiters[i].Location + Vector3.Transform((emmiters[i].MinVertex + emmiters[i].MaxVertex) / 2f, Matrix.CreateRotationY(emmiters[i].Rotate))) / 200f
                                };
                                AudioListener reci = new AudioListener
                                {
                                    //reci.Velocity = new Vector3((float)(Program.game.mainCamera.Target.Joystick * Math.Sin(Program.game.mainCamera.Target.Rotate)), 0, (float)(Program.game.mainCamera.Target.Joystick * Math.Cos(Program.game.mainCamera.Target.Rotate)));
                                    Position = (Program.game.mainCamera.Target.Location + Vector3.Transform((Program.game.mainCamera.Target.MinVertex + Program.game.mainCamera.Target.MaxVertex) / 2f, Matrix.CreateRotationY(Program.game.mainCamera.Target.Rotate))) / 200f
                                };
                                emi.Position -= reci.Position;
                                emi.Position = Vector3.Transform(emi.Position, Matrix.CreateRotationY(-Program.game.mainCamera.Yaw));
                                //emi.Position += reci.Position;
                                reci.Position = Vector3.Zero;
                                effectInstances[i].Apply3D(reci, emi);
                            }
                        }
                        /*for (int i = 0; i < loops.Count; i++)
                        {
                            if (effectInstances[i] !=null && effectInstances[i].State == SoundState.Stopped)
                            {
                                loops.RemoveAt(i);
                                if (effectInstances[i] !=null)
                                effectInstances[i].Dispose();
                                effectInstances[i] = null;
                                effectInstances.RemoveAt(i);
                                names.RemoveAt(i);
                                emmiters.RemoveAt(i);
                                i--;
                            }
                        }*/
                    }
                });
                loopThread.Start();
            }
        }

        public static void UpdateAmbient()
        {
            if (CurrentAmbient.Length<1)
                return;
            int ind = -1;
            for (int i=0;i<names.Count;i++)
            {
                if (names[i]== CurrentAmbient)
                {
                    ind = i;
                }
                else
                if (effectInstances[i].Volume>0 && !names[i].ToLower().Contains("bgm") && names[i].Contains(Program.game.Map.Name))
                {
                    effectInstances[i].Volume += ((0f) - effectInstances[i].Volume) / 30f;
                }
            }
            if (ind<0)
            {
                Audio.Play(CurrentAmbient, true, null, 0);
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

        public static void Play(string path, bool loop, Model emmiter, byte vol)
        {
            if (path.Contains(";"))
            {
                string[] spli = path.Split(';');
                path = spli[rnd.Next(0, spli.Length)];
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
            instaceCounts[index]++;

            try
            {
                if (instaceCounts[index] < 4)
                {
                    var se = effects[index].CreateInstance();
                    se.Volume = vol / 400f;
                    if (emmiter !=null)
                    {
                        AudioEmitter emi = new AudioEmitter
                        {
                            Position = new Vector3(Single.MaxValue, 0, 0)
                        };
                        AudioListener reci = new AudioListener
                        {
                            Position = Vector3.Zero
                        };
                        se.Apply3D(reci, emi);
                    }
                    se.Play();
                    se.IsLooped = loop;
                    names.Add(path);

                    playingMoments.Add(moment);

                    effectInstances.Add(se);
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
                        effectInstances[earlyIndex].Play();
                        playingMoments[earlyIndex] = moment;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            moment++;
        }

    }
}
