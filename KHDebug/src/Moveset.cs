using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace KHDebug
{
    public class Moveset : Resource
    {
        public bool Figed = false;
        public string[] Voices = new string[0];
        public int idle = 1;

        public int roll_ = -1;

        public int walk = 2;
        public int run = 5;

        public int cliff_ = 13;
        public int cliffExit_ = 14;

        public int idle_ = 1;
        public int idleFight_ = 0;

        public int idleRest_ = 1;
        public int idleRest1_ = 1;
        public int idleRest2_ = -1;

        public int walk_ = 2;
        public int walkFight_ = 4;
        public int walkRest_ = 3;
        public int run_ = 5;
        public int runFight_ = 7;
        public int runRest_ = 6;
        public int fall_ = 9;

        public int fly_ = -1;
        public int flyForward_ = -1;
        public int flyRight_ = -1;
        public int flyLeft_ = -1;
        public int flyIdle_ = -1;

        public int fall1_ = 9;
        public int fall2_ = -1;

        public int land_ = 10;
        public int land1_ = 10;
        public int land2_ = -1;
        public int guarding_ = -1;
        public int guard_ = -1;
		public int unguarding_ = -1;
        public int jump_ = -1;

        public int sora_doko_ = -1;
        public int sora_matu_ = -1;
        public int sora_mikke_ = -1;

        public int chat1_ = -1;
        public int chat2_ = -1;
        public int chat3_ = -1;
        public int chat4_ = -1;
        public int chat5_ = -1;
        public int chat6_ = -1;
        public int chat7_ = -1;
        public int chat8_ = -1;
        public int chat9_ = -1;
        public int chat10_ = -1;
        public int chat11_ = -1;
        public int chat12_ = -1;

		public int attack1_ = -1;
		public int attack1Air_ = -1;



		Moveset master;
        public bool HasMaster = false;

        public Moveset Master
        {
            get
            {
                return this.master;
            }
            set
            {
                HasMaster = (value != null);
                this.master = value;
            }
        }

        public Skeleton Skeleton;
        public float PlayingFrame = 0;

        public int MinTick;
        public int MaxTick;
        public bool LoopAnimation = true;

        public int playingIndex = -1;

        public int NextPlayingIndex = -1;

        public Vector3 TransportingZero;
        public Vector3 TransportedZero;
        


        public int PlayingIndex
        {
            get
            {
                return this.playingIndex;
            }
            set
            {
                if (this.playingIndex == value)
                    return;
				Model mdl = (this.Links[0] as Model);
				if (mdl == null)
					return;
				mdl.InactiveCount = 0;
				lastECFrame = -1;

				this.NextPlayingIndex = -1;

                if (this.playingIndex != value && value > -1)
                {
                    this.ComputingFrame = 0;
                    //this.InterpolateAnimation = true;
                    this.PlayingFrame = 0;
                    this.TransportedZero = Vector3.Zero;
                    this.TransportingZero.X = Single.NaN;
                    changingAnimation = true;
                    UpdateEC(value);
                }
                
                this.playingIndex = value;
                if (false&& mdl != null && Program.game.mainCamera.Target != null && mdl.ResourceIndex ==  Program.game.mainCamera.Target.ResourceIndex)
                {

                        Console.Clear();
                        Console.WriteLine("ANIM index = " + this.playingIndex);
                        Console.WriteLine("0x10 file data:");
                        Console.WriteLine("");
                        Console.WriteLine("___________________");
                        Console.WriteLine("___________________");
                        Console.WriteLine("Group 1 :");
                        Console.WriteLine("___________________");
                        Console.WriteLine("___________________");
                        for (int i = 0; i < this.ecs[this.playingIndex].Group1.Count; i++)
                        {
                            Console.WriteLine("Start frame:" + this.ecs[this.playingIndex].Group1[i].Start);
                            Console.WriteLine("End frame:" + this.ecs[this.playingIndex].Group1[i].End);
                            Console.WriteLine("Data type:" + this.ecs[this.playingIndex].Group1[i].ID);
                            Console.WriteLine("Data (" + this.ecs[this.playingIndex].Group1[i].Data.Length + " bytes):");
                            if (this.ecs[this.playingIndex].Group1[i].Data.Length>0)
                            Console.WriteLine(BitConverter.ToString(this.ecs[this.playingIndex].Group1[i].Data));
                            Console.WriteLine("___________________");
                        }
                        Console.WriteLine("___________________");
                        Console.WriteLine("___________________");
                        Console.WriteLine("Group 2 :");
                        Console.WriteLine("___________________");
                        Console.WriteLine("___________________");
                        for (int i = 0; i < this.ecs[this.playingIndex].Group2.Count; i++)
                        {
                            Console.WriteLine("Start frame:" + this.ecs[this.playingIndex].Group2[i].Start);
                            Console.WriteLine("Data type:" + this.ecs[this.playingIndex].Group2[i].ID);
                            Console.WriteLine("Data (" + this.ecs[this.playingIndex].Group2[i].Data.Length + " bytes):");
                            if (this.ecs[this.playingIndex].Group2[i].Data.Length > 0)
                                Console.WriteLine(BitConverter.ToString(this.ecs[this.playingIndex].Group2[i].Data));
                            Console.WriteLine("___________________");
                        }
                        Console.WriteLine("___________________");
                        Console.WriteLine("___________________");

                }
            }
        }

        public bool changingAnimation = false;

        public bool InterpolateAnimation
        {
            get
            {
                return this.interpolateAnimation;
            }
            set
            {
                this.interpolateAnimation = value;
            }
        }

        public int InterpolateFrameRate = 6;
        public bool interpolateAnimation = true;
        public int ComputingFrame = 0;

        public List<EC> ecs;
        public bool TransposeX = false;
        public bool TransposeY = false;
        public bool TransposeZ = false;

        int lastECIndex = -1;
        int lastECFrame = -1;

        public void UpdateEC(int index)
		{
			Model mdl = (this.Links[0] as Model);
			if (mdl == null)
				return;

			mdl.Mouth.f10Patching = false;
            mdl.Eyes.f10Patching = false;
            for (int i=0;i<mdl.Patches.Count;i++)
            {
                TexturePatch tp = mdl.Patches[i];
                tp.f10Patching = false;
            }
            int currFrame = (int)this.PlayingFrame;
            mdl.pState = Model.State.BlockAll;
            if (index > -1 && index < this.ecs.Count)
            {
                var Ecs = this.ecs[index].GetG1(currFrame);
                for (int i = 0; i < Ecs.Count; i++)
                {
                    //Console.WriteLine(Ecs[i].ID);
                    if (Ecs[i].ID == 0)
                    {
                        mdl.pState = (Model.State)0;
                    }
                    if (Ecs[i].ID == 1)
                    {
                        mdl.pState = (Model.State)1;
                    }
                    if (Ecs[i].ID == 2)
                    {
                        mdl.pState = (Model.State)2;
                        this.NextPlayingIndex = this.AtmospherePlayingIndex;
					}
					if (Ecs[i].ID == 3)
					{
						mdl.pState = (Model.State)3;
					}
					if (Ecs[i].ID == 26)
					{
						mdl.pState = (Model.State)26;
					}
					if (Ecs[i].ID == 247)
                    {
                        for (int p=0;p<Ecs[i].Data.Length/2;p++)
                        {
                            short indPatch = BitConverter.ToInt16(Ecs[i].Data, p * 2);
                            if (p == 0)
                            {
                                mdl.Eyes.GetPatch(indPatch);
                                //mdl.Eyes.f10Patching = (indPatch > -1);
                                mdl.Eyes.f10Patching = true;
                            }
                            else if (p == 1)
                            {
                                mdl.Mouth.GetPatch(indPatch);
                                //mdl.Mouth.f10Patching = (indPatch > -1);
                                mdl.Mouth.f10Patching = true;
                            }
                            else
                            {
                                TexturePatch tp = mdl.Patches[p];
                                tp.GetPatch(indPatch);
                                //tp.f10Patching = (indPatch > -1);
                                tp.f10Patching = true;
                            }
                        }
                    }
                }
                Ecs = this.ecs[index].GetG2(currFrame);
                for (int i = 0; i < Ecs.Count; i++)
                {
                    if (Ecs[i].ID == 2)
                    {
                        if (Ecs[i].Data.Length > 3 && (currFrame != lastECFrame || i != lastECIndex))
                        {
                            short ind = BitConverter.ToInt16(Ecs[i].Data,0);
                            if (100+ ind < this.Voices.Length)
                            Audio.Play(this.Voices[100+ ind], false, mdl,80);
							lastECIndex = i;
						}
                    }
                    if (Ecs[i].ID == 8)
                    {
                        if (Ecs[i].Data.Length > 3)
                        {
                            ushort ind_ui2 = BitConverter.ToUInt16(Ecs[i].Data, 2);
                            string fname = @"Content\Effects\Audio\Sounds\0x08\" + ind_ui2.ToString("X4") + ".wav";
                            if (System.IO.File.Exists(fname) && (currFrame != lastECFrame || i != lastECIndex))
                            {
                                Audio.Play(fname, false, mdl, 50);
                                lastECIndex = i;
                            }
                        }
                    }
                    if (Ecs[i].ID == 14)
                    {
                        if (Ecs[i].Data.Length > 0 && Ecs[i].Data[0] < this.Voices.Length)
                        {
                            if (this.FrameStep >= 0 && (currFrame != lastECFrame || i != lastECIndex))
                            {
                                Audio.Play(this.Voices[Ecs[i].Data[0]], false, mdl, 50);
                                lastECIndex = i;
                            }
                        }
                    }
                    if (Ecs[i].ID == 252) //1 voice interrupt others
                    {
                        if (Ecs[i].Data.Length > 0 && Ecs[i].Data[0] < this.Voices.Length)
                        {
                            if (this.FrameStep >= 0 && (currFrame != lastECFrame || i != lastECIndex))
                            {
                                for (int k = 0; k < this.Voices.Length; k++)
                                {
                                    for (int l = 0; l < Audio.names.Count; l++)
                                        if (Audio.names[l] == this.Voices[k])
                                        {
                                            Audio.effectInstances[l].Stop();
                                        }
                                }

                                Audio.Play(this.Voices[Ecs[i].Data[0]], false, mdl, 50);
                                lastECIndex = i;
                            }
                        }
                    }
                    if (Ecs[i].ID == 253 && currFrame != lastECFrame) //SetInteger
                    {
                        int valID = Program.game.Map.varIDs.IndexOf(BitConverter.ToUInt16(Ecs[i].Data, 0));
                        int val = BitConverter.ToUInt16(Ecs[i].Data, 2);
                        
                        int ind_ = Program.game.Map.varIDs.IndexOf(valID);
                        if (ind_ < 0)
                        {
                            Program.game.Map.varIDs.Add(valID);
                            Program.game.Map.varValues.Add(val);
                        }
                        else
                        {
                            Program.game.Map.varValues[ind_] = val;
                        }
                    }
                    if (Ecs[i].ID == 254)
                    {
                        if (Ecs[i].Data.Length > 0 && Ecs[i].Data[0] < this.Voices.Length)
                        {
                            if (this.FrameStep<-0.00000001 && (currFrame != lastECFrame || i != lastECIndex))
                            {
                                for (int k = 0; k < this.Voices.Length; k++)
                                {
                                    for (int l = 0; l < Audio.names.Count; l++)
                                        if (Audio.names[l] == this.Voices[k])
                                        {
                                            Audio.effectInstances[l].Stop();
                                        }
                                }

                                Audio.Play(this.Voices[Ecs[i].Data[0]], false, mdl, 50);
                                lastECIndex = i;
                            }
                        }
                    }
                }
            }
            lastECFrame = currFrame;
        }
        public bool IK = false;

        public void PerformIK()
        {
            
        }
        

        public void ComputeAnimation()
		{
			Model mdl = (this.Links[0] as Model);
			if (mdl == null)
				return;

			if (this.HasMaster)
            {
                this.interpolateAnimation = this.Master.interpolateAnimation;
                this.InterpolateFrameRate = this.Master.InterpolateFrameRate;

                this.ComputingFrame = this.Master.ComputingFrame;
			}


			if (this.IK)
                this.PerformIK();

            this.GetFrameData();

			mdl.RecreateVertexBuffer(true);
            this.ComputingFrame++;
        }

        public float FrameStep = 1f;

        public int AtmospherePlayingIndex = 1;

        public void GetFrameData()
        {
            if (this.Figed)
                return;
            if (this.Type == ResourceType.BinaryMoveset)
            {
               (this as BinaryMoveset).GetFrameData_();
            }
            if (this.Type == ResourceType.Moveset)
            {
                (this as MSET).GetFrameData_();
            }
        }


        
    }
}
