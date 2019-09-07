#define playnAnims
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace KHDebug
{
    public class BinaryMoveset : Moveset
    {
        public List<byte[]> br;


        public BinaryMoveset(string filename)
        {
            Console.WriteLine("Loading Resource " + filename);
            this.br = new List<byte[]>(0);
            this.ecs = new List<EC>(0);

            this.Type = ResourceType.BinaryMoveset;

            if (filename.Contains("\\MSET"))
                this.Name = filename.Split('\\')[filename.Split('\\').Length - 2];
            else
            this.Name = Path.GetFileNameWithoutExtension(filename);

            this.FileName = filename;

            string[] moves = new string[0];
            if (Path.GetExtension(filename) == ".bin")
                moves = new string[] { filename };
            else
            {
                moves = Directory.GetFiles(filename, "*.bin");
            }

            Array.Sort(moves);

            for (int i = 0; i < moves.Length; i++)
            {
                FileStream fs = new FileStream(moves[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] array = new byte[fs.Length];
                fs.Read(array, 0, array.Length);
                fs.Close();
                this.br.Add(array);
            }
                
            if (Path.GetExtension(filename) == ".bin")
            {
                if (File.Exists(filename.Replace(".bin",".ec")))
                {
                    moves = new string[] { filename.Replace(".bin", ".ec") };
                }
            }
            else
                moves = Directory.GetFiles(filename, "*.ec");
            Array.Sort(moves, (x, y) => String.Compare(x, y));
            for (int i = 0; i < moves.Length; i++)
            {
                this.ecs.Add(new EC(moves[i]));
            }
            if (File.Exists(filename + @"\sound-indices.txt"))
            {
                this.Voices = File.ReadAllLines(filename + @"\sound-indices.txt");
            }
            if (File.Exists(filename + @"\moves.txt"))
            {
                moves = File.ReadAllLines(filename + @"\moves.txt");
                for (int i = 0; i < moves.Length; i++)
                {
                    string[] spli = moves[i].Split('|');
                    switch (spli[0])
                    {
                        case "idle":
                            idle_ = int.Parse(spli[1]);
                            break;
                        case "idleFight":
                            idleFight_ = int.Parse(spli[1]);
                            break;
                        case "idleRest":
                            idleRest_ = int.Parse(spli[1]);
                            idleRest1_ = int.Parse(spli[1]);
                            if (spli.Length > 2)
                            {
                                idleRest2_ = int.Parse(spli[2]);
                            }

                            break;
                        case "walk":
                            walk_ = int.Parse(spli[1]);
                            break;
                        case "walkFight":
                            walkFight_ = int.Parse(spli[1]);
                            break;
                        case "walkRest":
                            walkRest_ = int.Parse(spli[1]);
                            break;
                        case "run":
                            run_ = int.Parse(spli[1]);
                            break;
                        case "runFight":
                            runFight_ = int.Parse(spli[1]);
                            break;
                        case "runRest":
                            runRest_ = int.Parse(spli[1]);
                            break;
                        case "fall":
                            fall_ = int.Parse(spli[1]);
                            fall1_ = int.Parse(spli[1]);
                            if (spli.Length > 2)
                            {
                                fall2_ = int.Parse(spli[2]);
                            }
                            break;
                        case "land":
                            land_ = int.Parse(spli[1]);
                            land1_ = int.Parse(spli[1]);
                            if (spli.Length > 2)
                            {
                                land2_ = int.Parse(spli[2]);
                            }
                            break;
                        case "guard":
                            guard_ = int.Parse(spli[1]);
                            break;
                        case "unguard":
                            unguard_ = int.Parse(spli[1]);
                            break;
                        case "jump":
                            jump_ = int.Parse(spli[1]);
                            break;
                        case "flyForward":
                            flyForward_ = int.Parse(spli[1]);
                            break;
                        case "flyRight":
                            flyRight_ = int.Parse(spli[1]);
                            break;
                        case "flyLeft":
                            flyLeft_ = int.Parse(spli[1]);
                            break;
                        case "flyIdle":
                            flyIdle_ = int.Parse(spli[1]);
                            break;
                        case "cliff":
                            cliff_ = int.Parse(spli[1]);
                            break;
                        case "cliffExit":
                            cliffExit_ = int.Parse(spli[1]);
                            break;
                        case "roll":
                            roll_ = int.Parse(spli[1]);
                            break;
                        case "chat1":
                            chat1_ = int.Parse(spli[1]);
                            break;
                        case "chat2":
                            chat2_ = int.Parse(spli[1]);
                            break;
                        case "chat3":
                            chat3_ = int.Parse(spli[1]);
                            break;
                        case "chat4":
                            chat4_ = int.Parse(spli[1]);
                            break;
                        case "chat5":
                            chat5_ = int.Parse(spli[1]);
                            break;
                        case "chat6":
                            chat6_ = int.Parse(spli[1]);
                            break;
                        case "chat7":
                            chat7_ = int.Parse(spli[1]);
                            break;
                        case "chat8":
                            chat8_ = int.Parse(spli[1]);
                            break;
                        case "chat9":
                            chat9_ = int.Parse(spli[1]);
                            break;
                        case "chat10":
                            chat10_ = int.Parse(spli[1]);
                            break;
                        case "chat11":
                            chat11_ = int.Parse(spli[1]);
                            break;
                        case "chat12":
                            chat12_ = int.Parse(spli[1]);
                            break;
                        case "attack1":
                            attack1_ = int.Parse(spli[1]);
                            break;
                        case "attack1Air":
                            attack1Air_ = int.Parse(spli[1]);
                        break;
                            
                    }
                }
            }
            fly_ = flyForward_;
            idle = idleRest_;
            walk = walkRest_;
            run = runRest_;
            
        }


        //public static string names = File.ReadAllText("names.txt");
        
        public void Parse()
        {
            this.ResourceIndex = Array.IndexOf(Resource.ResourceIndices, this.FileName.Split('.')[0]);
            /*if (!names.Contains(this.FileName))
            {
                names += "\"" + this.FileName + "\"\r\n";
                File.WriteAllText("names.txt", names);
            }*/
            this.Skeleton = (this.Links[0] as Model).Skeleton;
        }
        
        Matrix m = Matrix.Identity;

        public void GetFrameData_()
        {
            if (this.Master!=null)
            {
                this.PlayingFrame = this.Master.PlayingFrame;
                this.PlayingIndex = this.Master.PlayingIndex;
                
                if ((this.PlayingIndex < 0))
                this.Links[0].Render = false;
                if (this.PlayingIndex < 0)
                {
                    this.PlayingFrame = 0;
                    return;
                }
            }
            if (this.PlayingIndex < 0 || this.PlayingIndex> this.br.Count-1)
                return;
            if (changingAnimation)
            {
                this.MaxTick = BitConverter.ToInt32(this.br[this.PlayingIndex],0);
                this.MinTick = BitConverter.ToInt32(this.br[this.PlayingIndex], 4);

                this.TransposeX = this.br[this.PlayingIndex][8] != 0;
                this.TransposeY = this.br[this.PlayingIndex][9] != 0;
                this.TransposeZ = this.br[this.PlayingIndex][10] != 0;
                byte b = this.br[this.PlayingIndex][11];
                if (b==255)
                this.InterpolateAnimation = false;
                else
                {
                    this.InterpolateAnimation = true;
                    if (b > 0)
                    {
                        this.InterpolateFrameRate = b;
                    }
                }

                this.Links[0].Render = this.br[this.PlayingIndex][12] == 0;

                this.Skeleton.RememberSRT();

                changingAnimation = false;
            }

            if (this.HasMaster)
            {
            }
            else
            {
                this.PlayingFrame += this.FrameStep;
                UpdateEC(this.PlayingIndex);

                #if (!playAnims)
                if (this.NextPlayingIndex > -1)
                {
                    if ((int)this.PlayingFrame > (int)this.MaxTick - 1)
                    {
                        if ((this.Links[0] as Model).cState != Model.ControlState.Jump)
                        {
                            this.PlayingIndex = this.NextPlayingIndex;
                        }
                        else
                            this.PlayingFrame = this.MinTick;
                        return;
                    }
                }
                #endif
            }

            if ((int)this.PlayingFrame < 0)
                this.PlayingFrame = 0;

            if (this.PlayingFrame > (int)this.MaxTick - 1)
                this.PlayingFrame = this.MinTick;

            int pos = 0x10 + (int)this.PlayingFrame * this.Skeleton.Bones.Count * 0x40;
            
            for (int t = 0; t < this.Skeleton.Bones.Count; t++)
            {
                m.M11 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M12 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M13 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M14 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                m.M21 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M22 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M23 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M24 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                m.M31 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M32 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M33 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M34 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                m.M41 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M42 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M43 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                m.M44 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                if ((this.TransposeX || this.TransposeY || this.TransposeZ) && this.Master == null && t == this.Skeleton.RootBone)
                {
                    if (!Single.IsNaN(this.TransportingZero.X))
                        this.TransportedZero = m.Translation - this.TransportingZero;
                    this.TransportingZero = m.Translation;
                    //if (!this.InterpolateAnimation)
                    Vector3 offset = Vector3.Transform(this.TransportedZero, Matrix.CreateRotationY((this.Links[0] as Model).Rotate));
                    Vector3 newLoc = (this.Links[0] as Model).Location;

                    Model mdl = this.Links[0] as Model;
                    Vector3 zerop = m.Translation;
                    if (this.TransposeX)
                    {
                        if (Program.game.CombatCountDown > 0)
                            zerop.X = this.Skeleton.ZeroPositionFight.X;
                        else
                        zerop.X = this.Skeleton.ZeroPosition.X;
                        newLoc.X += offset.X;
                    }
                    if (this.TransposeY)
                    {
                        if (Program.game.CombatCountDown > 0)
                            zerop.Y = this.Skeleton.ZeroPositionFight.Y;
                        else
                            zerop.Y = this.Skeleton.ZeroPosition.Y;
                        newLoc.Y += offset.Y;
                    }
                    if (this.TransposeZ)
                    {
                        if (Program.game.CombatCountDown > 0)
                            zerop.Z = this.Skeleton.ZeroPositionFight.Z;
                        else
                            zerop.Z = this.Skeleton.ZeroPosition.Z;
                        newLoc.Z += offset.Z;
                    }
                    if (Program.game.Map != null && Program.game.Map.Links.Count > 0&& !mdl.Cutscene)
                    {
                        (Program.game.Map.Links[0] as Collision).MonitorCollision(mdl, ref newLoc);
                    }
                    mdl.Location = newLoc;
                    m.Translation = zerop;
                    //Program.game.cursors[0].Position = (this.Links[0] as Model).Location;
                }

                if (this.InterpolateAnimation && this.ComputingFrame<=this.InterpolateFrameRate)
                {
                    float remember = (float)(this.InterpolateFrameRate - this.ComputingFrame)/(float)this.InterpolateFrameRate;
                    float current = (float)this.ComputingFrame / (float)this.InterpolateFrameRate;

                    Matrix m1 = this.Skeleton.Bones[t].RememberMatrix;

                    Matrix m2 = m;

                    Quaternion q = Quaternion.Slerp(m1.Rotation, m2.Rotation, current);
                    Vector3 tr = m1.Translation * remember + m2.Translation * current;
                    

                    Matrix m_ = Matrix.CreateScale(1f) * Matrix.CreateFromQuaternion(q) * Matrix.CreateTranslation(tr);

                    //this.Skeleton.Bones[t].GetSRT(this.Skeleton.Bones[t].RememberMatrix * remember +  m* current);
                    
                    this.Skeleton.Bones[t].localMatrix = m_;
                    //this.Skeleton.Bones[t].GlobalMatrix = m_;
                    //this.Skeleton.Bones[t].GetSRT(m_);
                }
                else
                {
                    this.Skeleton.Bones[t].localMatrix = m;
                    //this.Skeleton.Bones[t].GlobalMatrix = m;
                    //this.Skeleton.Bones[t].GetSRT(m);
                }

                this.Skeleton.Bones[t].DirtyMatrix = true;
            }

            /*if (this.ResourceIndex ==  "TT08-SKY0")
            {
                
                Console.WriteLine(this.FrameStep);
                Console.WriteLine(this.MinTick+"/"+this.MaxTick);
                Console.WriteLine(this.PlayingFrame);
                Console.WriteLine("");
            }*/
            if (Math.Floor(this.PlayingFrame)< this.PlayingFrame)
            {
                float percentageDiff = this.PlayingFrame - (float)Math.Floor(this.PlayingFrame);

                float currPercentage = 1 - percentageDiff;
                float nextPercentage = percentageDiff;
                
                int nextFrame = (int)this.PlayingFrame + 1;

                if (nextFrame > this.MaxTick - 1)
                    nextFrame = this.MinTick;


                pos = 0x10 + nextFrame * this.Skeleton.Bones.Count * 0x40;

                for (int t = 0; t < this.Skeleton.Bones.Count; t++)
                {
                    m.M11 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M12 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M13 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M14 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                    m.M21 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M22 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M23 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M24 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                    m.M31 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M32 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M33 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M34 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                    m.M41 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M42 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M43 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;
                    m.M44 = BitConverter.ToSingle(this.br[this.PlayingIndex], pos); pos += 4;

                    Matrix m1 = this.Skeleton.Bones[t].LocalMatrix;
                    Matrix m2 = m;

                    Quaternion q = Quaternion.Slerp(m1.Rotation, m2.Rotation, nextPercentage);
                    Vector3 tr = m1.Translation * currPercentage + m2.Translation * nextPercentage;
                    Vector3 sc = m1.Scale * currPercentage + m2.Scale * nextPercentage;

                    Matrix m_ = Matrix.CreateScale(sc) * Matrix.CreateFromQuaternion(q) * Matrix.CreateTranslation(tr);

                    //this.Skeleton.Bones[t].GetSRT(m_);
                    this.Skeleton.Bones[t].localMatrix = m_;

                    this.Skeleton.Bones[t].DirtyMatrix = true;
                }
            }
            Skeleton.Wrap(this.Skeleton);
        }
        
        
    }
}
