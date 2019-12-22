using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace KHDebug
{
    public class Action
    {
		public static void SetMset(Model mdl1, Model mdl2)
		{
			if (mdl1 != null && mdl2 != null)
			{
				if (mdl1.Links.Count == 1)
				{
					Moveset mset_sora2 = (mdl2.Links[0].Links[1] as Moveset);
					if (mdl1.Links[0].Links.Count == 1)
					{
						mdl2.Master = mdl1;
						(mdl2.Links[0] as Moveset).Master = mset_sora2;

						mdl1.Links[0].Links.Insert(0, null);
						mdl1.Links.Insert(0, mset_sora2);
						MainGame.ResourceFiles.Add(mset_sora2);
					}
					mdl1.WalkSpeed = mdl2.WalkSpeed;
					mdl1.RunSpeed = mdl2.RunSpeed;
				}
			}
			else if (mdl1 != null && mdl2 == null)
			{
				if (mdl1.Links.Count == 2)
				{
					Moveset mset = mdl1.Links[0] as Moveset;
					Moveset mset_sora2 = mdl1.Links[0] as Moveset;
					Moveset mset_skate = mset.Links[1] as Moveset;
					Model model_skate = mset_skate.Links[0] as Model;

					mdl1.Links.RemoveAt(0); //ok

					MainGame.ResourceFiles.Remove(mset_sora2);//ok
					model_skate.Master = null;//ok
					mset_skate.Master = null;//ok

					mdl1.Links[0].Links.RemoveAt(0);
					mset_skate.PlayingIndex = mset_skate.idle_;
					if (Program.game.MapSet)
					{
						int ind = Program.game.Map.varIDs.IndexOf(0);
						if (ind>-1)
						Program.game.Map.varValues[ind] = 0;
					}
				}
				mdl1.WalkSpeed = mdl1.InitialWalkSpeed;
				mdl1.RunSpeed = mdl1.InitialRunSpeed;
			}
		}

        public MAP parent;

        public Condition[] conditions;
        public List<Command> commands;
        public List<Command> elseCommands;
        public int operator_;

        //0 = and
        //1 = or

        public static System.Threading.Thread thread;
        public static long oldTick = 0;

		public static Action[] ac;
		public static int aAccount = 0;

		public static Action[] be;
		public static int bAccount = 0;

		public Action()
        {
            this.conditions = new Condition[2];
            this.conditions[0] = new Condition();
			this.conditions[0].is_not = false;

			this.conditions[0].model_ = new Model[50];
            this.conditions[0].oldCondition = false;
            this.conditions[0].MatchCount = 0;

            this.conditions[1] = new Condition();
			this.conditions[1].is_not = false;
			this.conditions[1].model_ = new Model[50];
            this.conditions[1].oldCondition = false;
            this.conditions[1].MatchCount = 0;

            this.commands = new List<Command>(0);
            this.elseCommands = new List<Command>(0);
            this.operator_ = -1;
        }

        public void Verify()
        {
            bool do_ = conditions[0].Test();

            if (this.operator_ == 1)
            {
                do_ = do_ || conditions[1].Test();
            }
            if (this.operator_ == 0)
            {
                do_ = do_ && conditions[1].Test();
            }
            if (do_)
            {
                for (int i = 0; i < this.commands.Count; i++)
                {
                    var v = this.commands[i];
                    v.MatchCount = conditions[0].MatchCount;
                    v.model_ = conditions[0].model_;
                    v.Perform();
                }
            }
            else
            {
                for (int i = 0; i < this.elseCommands.Count; i++)
                {
                    var v = this.elseCommands[i];
                    v.MatchCount = conditions[0].MatchCount;
                    v.model_ = conditions[0].model_;
                    v.Perform();
                }
            }
        }


		public unsafe void AddComand(string line)
        {
            string[] spli = line.Split('|');
            Command c = new Command();
            c.model_ = new Model[50];
            c.MatchCount = 0;
			c.ModelToFind1 = false;
			c.ModelToFind2 = false;

			string[] spli2 = new string[0];

            switch (spli[0])
            {
                case "SetPositionX":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
					c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionX;
                    break;
                case "SetPositionY":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionY;
                    break;
                case "SetPositionZ":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionZ;
                    break;
                case "SetRotation":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetRotation;
                    break;
                case "SetMap":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.boolean_[0] = Boolean.Parse(spli[2]);
                    c.boolean_[1] = Boolean.Parse(spli[3]);
					c.CommandType = Command.Type.SetMap;
                    break;
                case "GotoCutscene":
                case "Goto":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    spli2 = spli[2].Split(',');
                    if (spli2[0] == "NaN")
                        c.vector_[0 * 3 + 0] = Single.NaN;
                    else
                    {
                        c.vector_[0 * 3 + 0] = MainGame.SingleParse(spli2[0]);
                        c.vector_[0 * 3 + 1] = MainGame.SingleParse(spli2[1]);
                        c.vector_[0 * 3 + 2] = MainGame.SingleParse(spli2[2]);
                    }
                    c.CommandType = spli[0] == "Goto"? Command.Type.Goto : Command.Type.GotoCutscene;
                    break;
                case "RunScene":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.CommandType = Command.Type.RunScene;
                    break;
                case "GoForward":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.GoForward;
                    break;
                case "SetAmbient":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                c.CommandType = Command.Type.SetAmbient;
                    break;
                    
                case "SetFiged":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.boolean_[0] = Boolean.Parse(spli[2]);
                    c.CommandType = Command.Type.SetFiged;
                    break;
                case "SetMatrix":
					c.ModelToFind1 = true;
					spli2 = spli[3].Split(',');
                    for (int m=0;m<16;m++)
                        c.matrix_[0 * 16 + m] = MainGame.SingleParse(spli2[m]);
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.int_[0] = int.Parse(spli[2]);
                    c.CommandType = Command.Type.SetMatrix;
                    break;
                   
                case "PlayMovesetMapObjects":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.int_[0] = int.Parse(spli[2]);

					string[] ind_s = spli[3].Split('s');
					if (ind_s .Length > 1)
					{
						c.float_[0] = MainGame.SingleParse(ind_s[0]);
						c.float_[1] = MainGame.SingleParse(ind_s[1]);
						c.boolean_[1] = true;
					}
					else
					{
						c.float_[0] = MainGame.SingleParse(spli[3]);
						c.boolean_[1] = false;
					}
                    c.boolean_[0] = Boolean.Parse(spli[4]);
                    c.CommandType = Command.Type.PlayMovesetMapObjects;
                    break;
                case "AmbientVolumeTo":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.float_[1] = MainGame.SingleParse(spli[3]);
                    c.CommandType = Command.Type.AmbientVolumeTo;
                break;
				case "PlayMovesetResources":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
					c.int_[0] = int.Parse(spli[2]);
					c.float_[0] = MainGame.SingleParse(spli[3]);
					c.boolean_[0] = Boolean.Parse(spli[4]);
					c.CommandType = Command.Type.PlayMovesetResources;
					break;
				case "ShowBubble":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length && i < 288; i++) c.string_[1 * 32 + i] = spli[1][i];
					c.int_[0] = int.Parse(spli[2]);
					c.int_[1] = int.Parse(spli[3]);
					c.int_[2] = int.Parse(spli[4]);
					for (int i = 0; i < spli[5].Length && i < 32; i++) c.string_[0 * 32 + i] = spli[5][i];
					c.CommandType = Command.Type.ShowBubble;
					break;

				case "SetMoveset":
					c.ModelToFind1 = true;
					c.ModelToFind2 = true;
					for (int i = 0; i < spli[1].Length && i < 32; i++) c.string_[0 * 32 + i] = spli[1][i];
					for (int i = 0; i < spli[2].Length && i < 32; i++) c.string_[1 * 32 + i] = spli[2][i];
					c.CommandType = Command.Type.SetMoveset;
					break;
				case "AudioPlay":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[1].Length && i < 32; i++) c.string_[0 * 32 + i] = spli[1][i];
					for (int i = 0; i < spli[2].Length && i < 32; i++) c.string_[1 * 32 + i] = spli[2][i];
					c.boolean_[0] = Boolean.Parse(spli[3]);
					c.int_[0] = int.Parse(spli[4]);
					c.CommandType = Command.Type.AudioPlay;
					break;
					

				case "ChangeDiffuseColor":
                    spli2 = spli[2].Split(',');
					Vector3 v3 = new Color(int.Parse(spli2[0]) / 255f, int.Parse(spli2[1]) / 255f, int.Parse(spli2[2]) / 255f, int.Parse(spli2[3]) / 255f).ToVector3();
                    c.vector_[0 * 4 + 0] = v3.X;
                    c.vector_[0 * 4 + 1] = v3.Y;
					c.vector_[0 * 4 + 2] = v3.Z;

					c.CommandType = Command.Type.ChangeDiffuseColor;
                break;
                case "SetReactionCommand":
                    if (spli[1] == "null")
                    {
                        c.action_ = null;
                    }
                    else
					{
						c.ModelToFind1 = true;
						if (System.IO.File.Exists(@"Content\Scenes\ReactionCommands\"+spli[1]+".rc"))
						{
							string[] inpt = System.IO.File.ReadAllLines(@"Content\Scenes\ReactionCommands\" + spli[1] + ".rc");

							spli = inpt[0].Split('|');

							for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];

							spli = inpt[1].Split('|');
							c.int_[0] = int.Parse(spli[1]);
							spli2 = inpt[2].Split('|')[1].Split(',');

							c.vector_[0 * 3 + 0] = MainGame.SingleParse(spli2[0]);
							c.vector_[0 * 3 + 1] = MainGame.SingleParse(spli2[1]);
							c.vector_[0 * 3 + 2] = MainGame.SingleParse(spli2[2]);

							spli = inpt[3].Split('|');
							c.int_[1] = int.Parse(spli[1]);

							spli = inpt[4].Split('|');
							c.int_[2] = int.Parse(spli[1]);


							spli = inpt[5].Split('|');
							c.int_[3] = int.Parse(spli[1]);

							spli = inpt[6].Split('|');
							c.boolean_[0] = Boolean.Parse(spli[1]);


							int ind = 0;
							while (!inpt[ind].Contains("@"))
								ind++;
							ind++;

							c.action_ = new Action();
							c.parent = this.parent;
							c.action_.parent = this.parent;
							c.action_.AddCondition("IF|true");
							for (; ind < inpt.Length; ind++)
							{
								c.action_.AddComand(inpt[ind]);
							}

						}
                    }
                    c.CommandType = Command.Type.SetReactionCommand;
                break;
                case "SetInteger":
                    c.parent = this.parent;
                    c.int_[0] = int.Parse(spli[1]);
                    c.int_[1] = int.Parse(spli[2]);
                    c.CommandType = Command.Type.SetInteger;
                break;
            }
            if (elseMode)
            this.elseCommands.Add(c);
            else
            this.commands.Add(c);
        }
        bool elseMode = false;


        public unsafe void AddCondition(string line)
        {
            string[] spli = line.Split('|');
            if (spli[0] == "ELSE")
            {
                elseMode = true;
                return;
            }

            if (spli[0] == "AND")
            {
                this.operator_ = 0;
            }
            if (spli[0] == "OR")
            {
                this.operator_ = 1;
            }
            Condition c = new Condition();

			c.is_not = spli[0].Contains("NOT");
			c.model_ = new Model[50];
            c.oldCondition = false;
            c.MatchCount = 0;

			c.ModelToFind1 = false;
			c.ModelToFind2 = false;

			for (int i = 0; i < 320; i++)
            {
                c.string_[i] = '\x0';
            }

            switch (spli[1])
			{
				case "InsideMapAreaGameplay":
				case "JustLeave":
				case "JustEnter":
				case "InsideMapArea":
					c.ModelToFind1 = true;
					for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
					if (spli[3].Contains("#"))
					{
						string[] spli_sharp = spli[3].Split('#');
						c.ModelToFind2 = true;
						c.float_[0] = MainGame.SingleParse(spli_sharp[1]);
						if (spli_sharp.Length > 2)
						{
							spli_sharp = spli_sharp[2].Split(',');
							c.vector_[0] = MainGame.SingleParse(spli_sharp[0]);
							c.vector_[1] = MainGame.SingleParse(spli_sharp[1]);
							c.vector_[2] = MainGame.SingleParse(spli_sharp[2]);
						}
						else
						{
							c.vector_[0] = 0;
							c.vector_[1] = 0;
							c.vector_[2] = 0;
						}
						spli[3] = spli[3].Split('#')[0];
					}
					else
						c.float_[0] = Single.NaN;
					for (int i = 0; i < spli[3].Length; i++) c.string_[1 * 32 + i] = spli[3][i];

					if (spli[1] == "InsideMapArea")
						c.ConditionType = Condition.Type.InsideMapArea;
					else if (spli[1] == "JustEnter")
						c.ConditionType = Condition.Type.JustEnter;
					else if (spli[1] == "JustLeave")
						c.ConditionType = Condition.Type.JustLeave;
					else if (spli[1] == "InsideMapAreaGameplay")
						c.ConditionType = Condition.Type.InsideMapAreaGameplay;

					break;
                case "IntegerEquals":
                    c.parent = this.parent;
                    c.ConditionType = Condition.Type.IntegerEquals;
                    c.int_[0] = int.Parse(spli[2]);
                    c.int_[1] = int.Parse(spli[3]);
                    break;
                    
                case "AtOrigin":
					c.ModelToFind1 = true;
					c.ConditionType = Condition.Type.AtOrigin;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    break;
				case "true":
					c.ConditionType = Condition.Type.TRUE;
					break;
				case "start":
					c.ConditionType = Condition.Type.START;
					break;
					

			}
            if (this.operator_ < 0)
            {
                conditions[0] = c;
            }
            else
            {
                conditions[1] = c;
            }
        }
    }

    /*public unsafe struct ComputedVertex
    {
        public fixed float Vertices[16];
        public fixed short Matis[4];
        public int Count;
    }*/

    public unsafe struct Condition
	{
		public bool ModelToFind1;
		public bool ModelToFind2;
		public bool is_not;
        public Type ConditionType;
        public MAP parent;

        public enum Type
        {
            TRUE = 0,
            InsideMapArea = 1,
            AtOrigin = 2,
            InsideMapAreaGameplay = 3,
            JustEnter = 4,
            JustLeave = 5,
			IntegerEquals = 6,
			START = 7
		}
        
        public Model[] model_;

        public fixed float vector_[30];
        public fixed float color_[40];
        public fixed char string_[320];
        public fixed int int_[10];
        public fixed float float_[10];
        public fixed bool boolean_[10];
        
        public int MatchCount;
        public bool oldCondition;

        public bool currentCondition;


        public bool Test()
        {
			Model mdl1 = null;
			Model mdl2 = null;
            this.MatchCount = 0;

			int mdl_to_find_count = 0;
			if (this.ModelToFind1) mdl_to_find_count++;
			if (this.ModelToFind2) mdl_to_find_count++;

			bool any_1 = false;
			bool any_2 = false;
			bool this_1 = false;
			bool this_2 = false;
			
			for (int m=0;m< mdl_to_find_count;m++)
			{
				bool any_ = 
						(this.string_[m * 32] == 'a' &&
						this.string_[m * 32 + 1] == 'n' &&
						this.string_[m * 32 + 2] == 'y');
				bool this_ =
						this.string_[m * 32 + 1] == 'h' &&
						(this.string_[m * 32 + 0] == 't' &&
						this.string_[m * 32 + 2] == 'i' &&
						this.string_[m * 32 + 3] == 's');
				if (any_ || this_)
				{
					if (m == 0)
					{
						any_1 = any_;
						this_1 = this_;
					}
					if (m == 1)
					{
						any_2 = any_;
						this_2 = this_;
					}
					continue;
				}

				bool partner = this.string_[m * 32 + 0] == 'p' &&
						this.string_[m * 32 + 1] == 'a' &&
						this.string_[m * 32 + 2] == 'r' &&
						this.string_[m * 32 + 3] == 't' &&
						this.string_[m * 32 + 4] == 'n' &&
						this.string_[m * 32 + 5] == 'e' &&
						this.string_[m * 32 + 6] == 'r';

				bool partner1 = partner && this.string_[m * 32 + 7] == '1';
				bool partner2 = partner && this.string_[m * 32 + 7] == '2';

				bool target  = this.string_[m * 32 + 0] == 't' &&
						this.string_[m * 32 + 1] == 'a' &&
						this.string_[m * 32 + 2] == 'r' &&
						this.string_[m * 32 + 3] == 'g' &&
						this.string_[m * 32 + 4] == 'e' &&
						this.string_[m * 32 + 5] == 't';

				if (partner1)
				{
					if (m == 0)
						mdl1 = Program.game.Partner1;
					if (m == 1)
						mdl2 = Program.game.Partner1;
					continue;
				}
				if (partner2)
				{
					if (m == 0)
						mdl1 = Program.game.Partner2;
					if (m == 1)
						mdl2 = Program.game.Partner2;
					continue;
				}
				if (target)
				{
					if (m == 0)
						mdl1 = Program.game.mainCamera.Target;
					if (m == 1)
						mdl2 = Program.game.mainCamera.Target;
					continue;
				}

				if (any_ || this_)
				{
					continue;
				}

				bool modelCorresponds = false;
				for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
				{
					var model = MainGame.ResourceFiles[i] as Model;
					if (model == null)
						continue;

					if (any_ && model != null && model.ModelType == Model.MDType.Human)
					{
						this.model_[this.MatchCount] = model;
						this.MatchCount++;
					}

					modelCorresponds = this.string_[m * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
					for (int j = 0; modelCorresponds && j < MainGame.ResourceFiles[i].Name.Length; j++)
						if (this.string_[m * 32 + j] != MainGame.ResourceFiles[i].Name[j])
							modelCorresponds = false;
					if (modelCorresponds)
					{
						/*if (MainGame.ResourceFiles[i] is Moveset mset)
						 * model = (mset.Links[0] as Model);*/
						if (m == 0) mdl1 = model;
						if (m == 1) mdl2 = model;
						break;
					}
				}
				if ((m == 0 && mdl1 == null) || (m == 1 && mdl2 == null))
				for (int i = 0; i < Program.game.Map.Supp.Count; i++)
				{
					modelCorresponds = this.string_[m * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
					for (int j = 0; modelCorresponds && j < Program.game.Map.Supp[i].Name.Length; j++)
						if (this.string_[m * 32 + j] != Program.game.Map.Supp[i].Name[j])
								modelCorresponds = false;
					if (modelCorresponds)
					{
						var model = Program.game.Map.Supp[i];
						var mset = Program.game.Map.SuppMsets[i];
						if (mset != null)
							model = (mset.Links[0] as Model);
						if (m == 0) mdl1 = model;
						if (m == 1) mdl2 = model;
						break;
					}
				}
			}


			bool condition = false;
            switch (this.ConditionType)
			{
				case Type.START:

					condition = false;
					if (Program.game.MapSet)
					{
						if (Program.game.Map.JustLoaded)
						{
							condition = true;
							Program.game.Map.JustLoaded = false;
						}
					}
					break;
				case Type.TRUE:
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        var model = MainGame.ResourceFiles[i] as Model;
                        if (MainGame.ResourceFiles[i] is Moveset mset)
                        {
                            model = (mset.Links[0] as Model);
                        }
                        if (model != null && model.ModelType == Model.MDType.Human)
                        {
                            this.model_[this.MatchCount] = model;
                            this.MatchCount++;
                        }

                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
						var model = Program.game.Map.Supp[i] as Model;
                        if (model != null && model.ModelType == Model.MDType.Human)
                        {
                            this.model_[this.MatchCount] = model;
                            this.MatchCount++;
                        }
                    }
                    condition = true;
                break;
                case Type.AtOrigin:

					if (mdl1 != null && Vector3.Distance(mdl1.Location, Vector3.Zero) < 0.01f)
						condition = true;

					break;
                case Type.IntegerEquals:
                    int valID = this.int_[0];
                    int val = this.int_[1];
                    int ind_ = this.parent.varIDs.IndexOf(valID);
                    if (ind_ > -1)
                    {
                        condition = this.parent.varValues[ind_] == val;
                    }
                    break;

                case Type.JustEnter:
                case Type.JustLeave:
                case Type.InsideMapArea:
                case Type.InsideMapAreaGameplay:
                    if (this.ConditionType == Type.InsideMapAreaGameplay && ScenePlayer.ScenePlaying)
                    {
                        break;
                    }

					if (Single.IsNaN(float_[0]))
					{
						fixed (char* s = &string_[1 * 32])
						{
							if (Program.game.Map.Area.Set == 0x584976)
							{
								if (mdl1 != null && Program.game.Map.Area.IsInside(mdl1.Location, s))
								{
									condition = true;
								}
								else if (any_1 || this_1)
								{
									for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
									{
										var model = MainGame.ResourceFiles[i] as Model;

										if (MainGame.ResourceFiles[i] is Moveset mset)
											model = (mset.Links[0] as Model);

										if (model != null && Program.game.Map.Area.IsInside(model.Location, s))
										{
											condition = true;
											if (any_1)
												break;

											this.model_[this.MatchCount] = model;
											this.MatchCount++;
										}
									}

									for (int i = 0; i < Program.game.Map.Supp.Count; i++)
									{
										var model = Program.game.Map.Supp[i] as Model;
										if (model.ModelType != Model.MDType.Human) continue;

										if (model != null && Program.game.Map.Area.IsInside(model.Location + model.GetGlobalBone(model.Skeleton.RootBone, Vector3.Zero), s))
										{
											condition = true;
											if (any_1)
												break;

											this.model_[this.MatchCount] = model;
											this.MatchCount++;
										}
									}
								}

								
							}
						}
					}
					else//perimetre
					{
						if (mdl1 != null && mdl2 != null && mdl1.ResourceIndex != mdl2.ResourceIndex && Vector3.Distance(mdl1.Location + mdl1.GetGlobalBone(mdl1.Skeleton.RootBone, Vector3.Zero), mdl2.Location + mdl2.GetGlobalBone(mdl2.Skeleton.RootBone, new Vector3(this.vector_[0], this.vector_[1], this.vector_[2]))) < this.float_[0])
						{
							condition = true;
						}
						else if (mdl2 != null && (any_1 || this_1))
						{
							for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
							{
								var model = MainGame.ResourceFiles[i] as Model;

								if (MainGame.ResourceFiles[i] is Moveset mset)
									model = (mset.Links[0] as Model);

								if (model != null && Vector3.Distance(model.Location + model.GetGlobalBone(model.Skeleton.RootBone,Vector3.Zero), mdl2.Location + mdl2.GetGlobalBone(mdl2.Skeleton.RootBone, new Vector3(this.vector_[0], this.vector_[1], this.vector_[2]))) < this.float_[0])
								{
									if (any_1 && mdl2.ResourceIndex != model.ResourceIndex)
										condition = true;
									if (any_1 && condition)
										break;

									this.model_[this.MatchCount] = model;
									this.MatchCount++;
								}
							}

							for (int i = 0; i < Program.game.Map.Supp.Count; i++)
							{
								var model = Program.game.Map.Supp[i] as Model;
								if (model.ModelType != Model.MDType.Human) continue;

								if (model != null && Vector3.Distance(model.Location + model.GetGlobalBone(model.Skeleton.RootBone, Vector3.Zero), mdl2.Location + mdl2.GetGlobalBone(mdl2.Skeleton.RootBone, new Vector3(this.vector_[0], this.vector_[1], this.vector_[2]))) < this.float_[0])
								{
									if (any_1 && mdl2.ResourceIndex != model.ResourceIndex)
										condition = true;
									if (any_1 && condition)
										break;

									this.model_[this.MatchCount] = model;
									this.MatchCount++;
								}
							}
						}
					}
                       
                    break;
            }

            bool ancien = condition;

            if (this.ConditionType == Type.JustEnter)
            {
                condition = condition && !oldCondition;
            }
            if (this.ConditionType == Type.JustLeave)
            {
                condition = !condition && oldCondition;
            }

            oldCondition = ancien;
			if (is_not)
			{
				condition = !condition;
			}
            return condition;
        }









    }
    
    public unsafe struct Command
	{
		public bool ModelToFind1;
		public bool ModelToFind2;

		public enum Type
        {
                SetPositionX = 0,
                SetPositionY = 1,
                SetPositionZ = 2,
                GoForward = 3,
                PlayMovesetMapObjects = 4,
                AmbientVolumeTo = 5,
                PlayMovesetResources = 6,
            ChangeDiffuseColor = 7,
            SetRotation = 8,
            SetMap = 9,
            RunScene = 10,
            Goto = 11,
            GotoCutscene = 12,
            SetReactionCommand = 13,
            SetInteger = 14,
            SetMatrix = 15,
            SetAmbient = 16,
            SetFiged = 17,
			ShowBubble = 18,
			SetMoveset = 19,
			AudioPlay = 20
		}
        public Type CommandType;

        public Model[] model_;
        public fixed float vector_[30];
        public fixed float color_[40];
        public fixed float matrix_[160];

        public Action action_;

        public fixed char string_[320];
        public fixed int int_[10];
        public fixed float float_[10];
        public fixed bool boolean_[10];
        public MAP parent;

        /*public enum CommandType
        {
            PlayMovesetResources = 0,
            PlayMovesetMapObjects = 1,
            PlayVoice = 2,
            ChangeDiffuse = 3,
            ChangeOpacity = 4
        }*/

        public bool currentCondition;

        public int MatchCount;

        public void Perform()
		{
			Model mdl1 = null;
			Model mdl2 = null;

			int mdl_to_find_count = 0;
			if (this.ModelToFind1) mdl_to_find_count++;
			if (this.ModelToFind2) mdl_to_find_count++;

			bool any_1 = false;
			bool any_2 = false;
			bool this_1 = false;
			bool this_2 = false;

			for (int m = 0; m < mdl_to_find_count; m++)
			{
				if (this.string_[m * 32 + 0] == 'n' &&
						   this.string_[m * 32 + 1] == 'u' &&
						   this.string_[m * 32 + 2] == 'l' &&
						   this.string_[m * 32 + 3] == 'l')
				{
					continue;
				}

				bool any_ =
						(this.string_[m * 32] == 'a' &&
						this.string_[m * 32 + 1] == 'n' &&
						this.string_[m * 32 + 2] == 'y');
				bool this_ =
						this.string_[m * 32 + 1] == 'h' &&
						(this.string_[m * 32 + 0] == 't' &&
						this.string_[m * 32 + 2] == 'i' &&
						this.string_[m * 32 + 3] == 's');
				if (any_ || this_)
				{
					if (m == 0)
					{
						any_1 = any_;
						this_1 = this_;
					}
					if (m == 1)
					{
						any_2 = any_;
						this_2 = this_;
					}
					continue;
				}

				bool partner = this.string_[m * 32 + 0] == 'p' &&
						this.string_[m * 32 + 1] == 'a' &&
						this.string_[m * 32 + 2] == 'r' &&
						this.string_[m * 32 + 3] == 't' &&
						this.string_[m * 32 + 4] == 'n' &&
						this.string_[m * 32 + 5] == 'e' &&
						this.string_[m * 32 + 6] == 'r';

				bool partner1 = partner && this.string_[m * 32 + 7] == '1';
				bool partner2 = partner && this.string_[m * 32 + 7] == '2';

				bool target = this.string_[m * 32 + 0] == 't' &&
						this.string_[m * 32 + 1] == 'a' &&
						this.string_[m * 32 + 2] == 'r' &&
						this.string_[m * 32 + 3] == 'g' &&
						this.string_[m * 32 + 4] == 'e' &&
						this.string_[m * 32 + 5] == 't';

				if (partner1)
				{
					if (m == 0)
						mdl1 = Program.game.Partner1;
					if (m == 1)
						mdl2 = Program.game.Partner1;
					continue;
				}
				if (partner2)
				{
					if (m == 0)
						mdl1 = Program.game.Partner2;
					if (m == 1)
						mdl2 = Program.game.Partner2;
					continue;
				}
				if (target)
				{
					if (m == 0)
						mdl1 = Program.game.mainCamera.Target;
					if (m == 1)
						mdl2 = Program.game.mainCamera.Target;
					continue;
				}

				if (any_ || this_)
				{
					continue;
				}

				bool modelCorresponds = false;
				for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
				{
					var model = MainGame.ResourceFiles[i] as Model;
					if (model == null)
						continue;

					if (any_ && model != null && model.ModelType == Model.MDType.Human)
					{
						this.model_[this.MatchCount] = model;
						this.MatchCount++;
					}

					modelCorresponds = this.string_[m * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
					for (int j = 0; modelCorresponds && j < MainGame.ResourceFiles[i].Name.Length; j++)
						if (this.string_[m * 32 + j] != MainGame.ResourceFiles[i].Name[j])
							modelCorresponds = false;
					if (modelCorresponds)
					{
						/*if (MainGame.ResourceFiles[i] is Moveset mset)
						 * model = (mset.Links[0] as Model);*/
						if (m == 0) mdl1 = model;
						if (m == 1) mdl2 = model;
						break;
					}
				}
				if ((m == 0 && mdl1 == null) || (m == 1 && mdl2 == null))
					for (int i = 0; i < Program.game.Map.Supp.Count; i++)
					{
						modelCorresponds = this.string_[m * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
						for (int j = 0; modelCorresponds && j < Program.game.Map.Supp[i].Name.Length; j++)
							if (this.string_[m * 32 + j] != Program.game.Map.Supp[i].Name[j])
								modelCorresponds = false;
						if (modelCorresponds)
						{
							var model = Program.game.Map.Supp[i];
							var mset = Program.game.Map.SuppMsets[i];
							if (mset != null)
								model = (mset.Links[0] as Model);
							if (m == 0) mdl1 = model;
							if (m == 1) mdl2 = model;
							break;
						}
					}
			}



			//Model mdl = null;

			switch (this.CommandType)
			{
				case Type.ChangeDiffuseColor:
					for (int i = 0; i < this.MatchCount; i++)
					{
						model_[i].DestDiffuseColor.X = this.vector_[0 * 4 + 0];
						model_[i].DestDiffuseColor.Y = this.vector_[0 * 4 + 1];
						model_[i].DestDiffuseColor.Z = this.vector_[0 * 4 + 2];
					}
					break;
				case Type.PlayMovesetResources:
					for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
					{
						currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
						for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
							if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
								currentCondition = false;
						if (currentCondition)
						{
							if (MainGame.ResourceFiles[i] is Moveset mset)
							{
								mset.PlayingIndex = this.int_[0];
								mset.InterpolateAnimation = this.boolean_[0];
								mset.FrameStep = this.float_[0];
								mset.ComputeAnimation();
							}
						}
					}
					break;
				case Type.SetPositionX:
				case Type.SetPositionY:
				case Type.SetPositionZ:
				case Type.SetRotation:
					
					if (mdl1 != null)
					{
						if (this.CommandType == Type.SetPositionX)
						{
							mdl1.locBlock = 2;
							mdl1.loc.X = this.float_[0];
							mdl1.locAction = mdl1.loc;
						}
						else if (this.CommandType == Type.SetPositionY)
						{
							mdl1.locBlock = 2;
							mdl1.loc.Y = this.float_[0];
							mdl1.locAction = mdl1.loc;
						}
						else if (this.CommandType == Type.SetPositionZ)
						{
							mdl1.locBlock = 2;
							mdl1.loc.Z = this.float_[0];
							mdl1.locAction = mdl1.loc;
						}
						else if (this.CommandType == Type.SetRotation)
						{
							mdl1.DestRotate = this.float_[0];
						}
					}
					



				break;
				case Type.SetMoveset:
					Action.SetMset(mdl1, mdl2);
				break;
				case Type.AudioPlay:
					fixed (char* s = this.string_)
					{
						string str_bb = new string(s, 1*32, 32).TrimEnd('\x0');
						Audio.Play(@"Content\Effects\Audio\"+str_bb,this.boolean_[0],mdl1,(byte)this.int_[0]);
					}
				break;

				case Type.ShowBubble:


					fixed (char* s = this.string_)
					{
						string str_bb = new string(s, 32, 288).TrimEnd('\x0');
						BulleSpeecher.ShowBubble(str_bb, this.int_[0], (Bulle.BulleColor)this.int_[1], (Bulle.BulleType)this.int_[2], mdl1);
					}



					/*for (int i = 0; i < spli[1].Length && i < 288; i++) c.string_[32 + i] = spli[1][i];
					c.int_[0] = int.Parse(spli[2]);
					c.int_[1] = int.Parse(spli[3]);
					for (int i = 0; i < spli[4].Length && i < 32; i++) c.string_[i] = spli[4][i];
					c.CommandType = Command.Type.ShowBubble;*/
					break;

				case Type.GotoCutscene:
				case Type.Goto:
					if (this.CommandType == Type.GotoCutscene && !ScenePlayer.ScenePlaying)
						break;
					if (this.CommandType == Type.Goto && ScenePlayer.ScenePlaying)
						break;

					if (mdl1 != null)
					{
						mdl1.Goto.X = this.vector_[0 * 3];
						mdl1.Goto.Y = this.vector_[0 * 3 + 1];
						mdl1.Goto.Z = this.vector_[0 * 3 + 2];
					}
					break;

				case Type.SetMap:
					fixed (char* s = this.string_)
					{
						Program.game.SetMap(new string(s, 0 * 32, 32).TrimEnd('\x0'), this.boolean_[0], this.boolean_[1]);
					}
                    break;
                case Type.RunScene:
                    fixed (char* s = this.string_)
                        ScenePlayer.RunScene(new string(s, 0 * 32, 32).TrimEnd('\x0'));
                    break;
                case Type.SetMatrix:
					
                    if (mdl1 != null)
                    {
                        Bone b = mdl1.Skeleton.Bones[this.int_[0]];
                        b.GlobalMatrix.M11 = this.matrix_[0 * 16 + 0];
                        b.GlobalMatrix.M12 = this.matrix_[0 * 16 + 1];
                        b.GlobalMatrix.M13 = this.matrix_[0 * 16 + 2];
                        b.GlobalMatrix.M14 = this.matrix_[0 * 16 + 3];

                        b.GlobalMatrix.M21 = this.matrix_[0 * 16 + 4];
                        b.GlobalMatrix.M22 = this.matrix_[0 * 16 + 5];
                        b.GlobalMatrix.M23 = this.matrix_[0 * 16 + 6];
                        b.GlobalMatrix.M24 = this.matrix_[0 * 16 + 7];

                        b.GlobalMatrix.M31 = this.matrix_[0 * 16 + 8];
                        b.GlobalMatrix.M32 = this.matrix_[0 * 16 + 9];
                        b.GlobalMatrix.M33 = this.matrix_[0 * 16 + 10];
                        b.GlobalMatrix.M34 = this.matrix_[0 * 16 + 11];

                        b.GlobalMatrix.M41 = this.matrix_[0 * 16 + 12];
                        b.GlobalMatrix.M42 = this.matrix_[0 * 16 + 13];
                        b.GlobalMatrix.M43 = this.matrix_[0 * 16 + 14];
                        b.GlobalMatrix.M44 = this.matrix_[0 * 16 + 15];
                        mdl1.RecreateVertexBuffer(true);
                    }

                    break;
                case Type.GoForward:
					if (mdl1 != null)
					{
						mdl1.Location += new Vector3(
							(float)(this.float_[0] * Math.Sin((float)(mdl1.Rotate))),
							0,
							(float)(this.float_[0] * Math.Cos((float)(mdl1.Rotate))));
					}
					break;
                case Type.PlayMovesetMapObjects:
                    for (int i = 0; i < Program.game.Map.SuppMsets.Count; i++)
                    {
                        if (Program.game.Map.SuppMsets[i] != null)
                        {
                            currentCondition = this.string_[0 * 32 + Program.game.Map.SuppMsets[i].Name.Length] == 0; /* STRING COMPARE */
                            for (int j = 0; currentCondition && j < Program.game.Map.SuppMsets[i].Name.Length; j++)
                                if (this.string_[0 * 32 + j] != Program.game.Map.SuppMsets[i].Name[j])
                                    currentCondition = false;
                            if (currentCondition)
                            {
                                var mset = Program.game.Map.SuppMsets[i];
                                if (mset != null)
                                {
                                    mset.PlayingIndex = this.int_[0];
                                    mset.InterpolateAnimation = this.boolean_[0];
									if (this.boolean_[1])
										mset.FrameStep += (this.float_[0] - mset.FrameStep) / this.float_[1];
									else
										mset.FrameStep = this.float_[0];
								}
                            }
                        }
                    }
                    break;
                case Type.AmbientVolumeTo:
                    fixed (char* s = this.string_)
                    {
                        string name = "";
                        if (this.string_[0 * 30 + 0] == 0)
                        {
                            name = Audio.CurrentAmbient;
                        }
                        else
                        {
                            name = "Content\\Effects\\Audio\\Ambient\\" + new string(s, 0 * 32, 32).TrimEnd('\x0') + ".wav";
                        }

                        int ind = Audio.names.IndexOf(name);
                        
                        if (ind > -1)
                        {
							Audio.DestVolumes[ind][0] = (this.float_[0] / 4f);
							Audio.DestVolumes[ind][1] = this.float_[1];
                        }
                    }
                    break;
                case Type.SetAmbient:
                    fixed (char* s = this.string_)
                    {
                        Audio.CurrentAmbient = "Content\\Effects\\Audio\\Ambient\\" + new string(s, 0 * 32, 32).TrimEnd('\x0') + ".wav";
                    }
                    break;
                case Type.SetFiged:

					if (mdl1 != null)
                    {
						mdl1.Figed = this.boolean_[0];
                        if (mdl1.Links.Count>0)
                        {
                            var mset = mdl1.Links[0] as Moveset;
                            mset.Figed = this.boolean_[0];
                        }
                    }
                    break;
                    
                case Type.SetReactionCommand:
					if (this.action_ == null)
					{
						MainGame.UpdateReactionCommand = true;
						MainGame.ReactionCommand = null;
						break;
					}
					else if (MainGame.ReactionCommand == null && MainGame.DoModel != null && mdl1 != null)
					{
						Model target = Program.game.mainCamera.Target;
						if (target != null)
						{
							Vector3 curr_loc_rc = MainGame.DoModel.Location + MainGame.DoModel.GetGlobalBone(MainGame.DoBone,Vector3.Zero);
							Vector3 new_loc_rc = mdl1.Location + mdl1.GetGlobalBone(this.int_[0], Vector3.Zero);

							if (Vector3.Distance(target.Location, curr_loc_rc) < Vector3.Distance(target.Location, new_loc_rc))
							{
								break;
							}
						}
					}

					MainGame.UpdateReactionCommand = true;
					MainGame.ReactionCommand = null;
					PAXCaster.UpdatePaxes();

					MainGame.UpdateReactionCommand = true;
					MainGame.DoModel = mdl1;
                    MainGame.DoBone = this.int_[0];
                    MainGame.DoVector.X = this.vector_[0*3+0];
                    MainGame.DoVector.Y = this.vector_[0*3+1];
                    MainGame.DoVector.Z = this.vector_[0*3+2];
					BulleSpeecher.NoEmmiter = MainGame.DoVector * 1f;
					MainGame.ReactionCommand = this.action_;
                    MainGame.RotateAttract = this.boolean_[0];
					MainGame.triangleType = this.int_[1];
					MainGame.transitionType_rc = (MainGame.TransitionType)this.int_[2];
					MainGame.Transition_rc[0] = this.int_[3];
					MainGame.Transition_rc[1] = this.int_[3];
                    break;
                case Type.SetInteger:
                    int valID = this.int_[0];
                    int val = this.int_[1];
                    int ind_ = this.parent.varIDs.IndexOf(valID);
                    if (ind_ < 0)
                    {
                        this.parent.varIDs.Add(valID);
                        this.parent.varValues.Add(val);
                    }
                    else
                    {
                        this.parent.varValues[ind_] = val;
                    }
                break;
                    
            }
        }
    }
}
