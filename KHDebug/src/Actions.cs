using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace KHDebug
{
    public class Action
    {
        public Model parent;

        public Condition[] conditions;
        public List<Command> commands;
        public List<Command> elseCommands;
        public int operator_;

        //0 = and
        //1 = or

        public static System.Threading.Thread thread;
        public static Action[] ac;
        public static int aAccount = 0;
                
        public Action()
        {
            if (Action.thread == null)
            {
                Action.thread = new System.Threading.Thread(() =>
                {
                    while (true)
                    {
                        int count = Action.aAccount * 1;
                        if (count > 0)
                        {
                            Action.aAccount = -1;
                            for (int i = 0; i < count; i++)
                            {
                                Action.ac[i].Verify();
                            }
                            Action.aAccount = 0;
                        }
                    }
                });
                Action.thread.Start();
            }
            this.conditions = new Condition[2];
            this.conditions[0] = new Condition();
            this.conditions[0].model_ = new Model[50];
            this.conditions[0].oldCondition = false;
            this.conditions[0].MatchCount = 0;

            this.conditions[1] = new Condition();
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

            string[] spli2 = new string[0];

            switch (spli[0])
            {
                case "SetPositionX":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionX;
                    break;
                case "SetPositionY":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionY;
                    break;
                case "SetPositionZ":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetPositionZ;
                    break;
                case "SetRotation":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.SetRotation;
                    break;
                case "SetMap":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.CommandType = Command.Type.SetMap;
                    break;
                case "GotoCutscene":
                case "Goto":
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
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.float_[0] = MainGame.SingleParse(spli[2]);
                    c.CommandType = Command.Type.GoForward;
                    break;
                case "SetAmbient":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                c.CommandType = Command.Type.SetAmbient;
                    break;
                    
                case "SetFiged":


                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.boolean_[0] = Boolean.Parse(spli[2]);
                    c.CommandType = Command.Type.SetFiged;
                    break;
                case "SetMatrix":

                    spli2 = spli[3].Split(',');
                    for (int m=0;m<16;m++)
                        c.matrix_[0 * 16 + m] = MainGame.SingleParse(spli2[m]);
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.int_[0] = int.Parse(spli[2]);
                    c.CommandType = Command.Type.SetMatrix;
                    break;
                   
                case "PlayMovesetMapObjects":
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.int_[0] = int.Parse(spli[2]);
                    c.float_[0] = MainGame.SingleParse(spli[3]);
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
                    for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];
                    c.int_[0] = int.Parse(spli[2]);
                    c.float_[0] = MainGame.SingleParse(spli[3]);
                    c.boolean_[0] = Boolean.Parse(spli[4]);
                    c.CommandType = Command.Type.PlayMovesetResources;
                break;
                case "ChangeDiffuseColor":
                    spli2 = spli[2].Split(',');
                    c.color_[0 * 4 + 0] = int.Parse(spli2[0]) / 255f;
                    c.color_[0 * 4 + 1] = int.Parse(spli2[1]) / 255f;
                    c.color_[0 * 4 + 2] = int.Parse(spli2[2]) / 255f;
                    c.color_[0 * 4 + 3] = int.Parse(spli2[3]) / 255f;

                    c.CommandType = Command.Type.ChangeDiffuseColor;
                break;
                case "SetReactionCommand":
                    string[] spli3 = line.Split('@');

                    if (spli3[1] == "null")
                    {
                        c.action_ = null;
                    }
                    else
                    {
                        for (int i = 0; i < spli[1].Length; i++) c.string_[0 * 32 + i] = spli[1][i];

                        c.int_[0] = int.Parse(spli[2]);
                        spli2 = spli[3].Split(',');

                        c.vector_[0 * 3 + 0] = MainGame.SingleParse(spli2[0]);
                        c.vector_[0 * 3 + 1] = MainGame.SingleParse(spli2[1]);
                        c.vector_[0 * 3 + 2] = MainGame.SingleParse(spli2[2]);

                        c.action_ = new Action();
                        c.action_.AddCondition("IF|true");
                        for (int a=1;a<spli3.Length;a++)
                        {
                            c.action_.AddComand(spli3[a]);
                        }
                    }
                    c.CommandType = Command.Type.SetReactionCommand;
                break;
                case "SetBoolean":
                    c.parent = this.parent;
                    c.int_[0] = int.Parse(spli[1]);
                    c.boolean_[0] = Boolean.Parse(spli[2]);
                    c.CommandType = Command.Type.SetBoolean;
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
            c.model_ = new Model[50];
            c.oldCondition = false;
            c.MatchCount = 0;

            for (int i = 0; i < 320; i++)
            {
                c.string_[i] = '\x0';
            }

            switch (spli[1])
            {
                case "InsideMapArea":
                    c.ConditionType = Condition.Type.InsideMapArea;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    for (int i = 0; i < spli[3].Length; i++) c.string_[1 * 32 + i] = spli[3][i];
                    break;
                case "JustEnter":
                    c.ConditionType = Condition.Type.JustEnter;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    for (int i = 0; i < spli[3].Length; i++) c.string_[1 * 32 + i] = spli[3][i];
                    break;
                case "JustLeave":
                    c.ConditionType = Condition.Type.JustLeave;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    for (int i = 0; i < spli[3].Length; i++) c.string_[1 * 32 + i] = spli[3][i];
                    break;
                case "BooleanEquals":
                    c.parent = this.parent;
                    c.ConditionType = Condition.Type.BooleanEquals;
                    c.int_[0] = int.Parse(spli[2]);
                    c.boolean_[0] = Boolean.Parse(spli[3]);
                    break;
                    
                case "InsideMapAreaGameplay":
                    c.ConditionType = Condition.Type.InsideMapAreaGameplay;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    for (int i = 0; i < spli[3].Length; i++) c.string_[1 * 32 + i] = spli[3][i];
                    break;
                case "AtOrigin":
                    c.ConditionType = Condition.Type.AtOrigin;
                    for (int i = 0; i < spli[2].Length; i++) c.string_[0 * 32 + i] = spli[2][i];
                    break;
                case "true":
                    c.ConditionType = Condition.Type.TRUE;
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
        public Type ConditionType;
        public Model parent;

        public enum Type
        {
            TRUE = 0,
            InsideMapArea = 1,
            AtOrigin = 2,
            InsideMapAreaGameplay = 3,
            JustEnter = 4,
            JustLeave = 5,
            BooleanEquals = 6
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
            this.MatchCount = 0;

            bool condition = false;
            switch (this.ConditionType)
            {
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
                    condition = true;
                break;
                case Type.AtOrigin:

                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }

                            if (model != null &&

                                Vector3.Distance(model.Location, Vector3.Zero) < 0.01f)
                            {
                                condition = true;
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            var mset = Program.game.Map.SuppMsets[i];
                            if (mset != null)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null &&

                                Vector3.Distance(model.Location, Vector3.Zero) < 0.01f)
                            {
                                condition = true;
                            }
                        }
                    }
                    break;
                case Type.BooleanEquals:
                    int valID = this.int_[0];
                    bool val = this.boolean_[0];
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
                    fixed (char* s = &string_[1 * 32])

                        if (Program.game.Map.Area.Set == 0x584976)
                        {
                            if (this.string_[0 * 32] == 't' &&
                                this.string_[0 * 32+1] == 'a' &&
                                this.string_[0 * 32+2] == 'r' &&
                                this.string_[0 * 32+3] == 'g' &&
                                this.string_[0 * 32+4] == 'e' &&
                                this.string_[0 * 32+5] == 't' && Program.game.mainCamera.Target != null && Program.game.Map.Area.IsInside(Program.game.mainCamera.Target.Location, s))
                            {
                                condition = true;
                            }
                            else
                            if (this.string_[0 * 32] == 'p' &&
                                this.string_[0 * 32 + 1] == 'a' &&
                                this.string_[0 * 32 + 2] == 'r' &&
                                this.string_[0 * 32 + 3] == 't' &&
                                this.string_[0 * 32 + 4] == 'n' &&
                                this.string_[0 * 32 + 5] == 'e' &&
                                this.string_[0 * 32 + 6] == 'r' &&
                                this.string_[0 * 32 + 7] == '1' && Program.game.Partner1 != null && Program.game.Map.Area.IsInside(Program.game.Partner1.Location, s))
                            {
                                    condition = true;
                            }
                            else
                                for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                                {
                                    currentCondition = 
                                        (this.string_[0 * 32] == 'a' &&
                                        this.string_[0 * 32 + 1] == 'n' &&
                                        this.string_[0 * 32 + 2] == 'y')
                                    || (this.string_[0 * 32] == 't' &&
                                        this.string_[0 * 32 + 1] == 'h' &&
                                        this.string_[0 * 32 + 2] == 'i' &&
                                        this.string_[0 * 32 + 3] == 's');
                                       if (!currentCondition)
                                        {
                                            currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                                            for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                                                if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                                    currentCondition = false;
                                        }

                                if (currentCondition)
                                {
                                        var model = MainGame.ResourceFiles[i] as Model;

                                        if (MainGame.ResourceFiles[i] is Moveset mset)
                                        {
                                            model = (mset.Links[0] as Model);
                                        }

                                        if (model != null &&

                                            Program.game.Map.Area.IsInside(model.Location, s))
                                        {
                                                condition = true;
                                            if (this.string_[0 * 32] == 't' &&
                                        this.string_[0 * 32 + 1] == 'h' &&
                                        this.string_[0 * 32 + 2] == 'i' &&
                                        this.string_[0 * 32 + 3] == 's')
                                            {
                                                this.model_[this.MatchCount] = model;
                                                this.MatchCount++;
                                            }
                                            else
                                                break;
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
            return condition;
        }









    }
    
    public unsafe struct Command
    {
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
            SetBoolean = 14,
            SetMatrix = 15,
            SetAmbient = 16,
            SetFiged = 17
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
        public Model parent;

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
            switch (this.CommandType)
            {
                case Type.ChangeDiffuseColor:
                    for (int i = 0; i < this.MatchCount; i++)
                    {
                        model_[i].DestDiffuseColor = new Color(this.color_[0 * 4 + 0], this.color_[0 * 4 + 1], this.color_[0 * 4 + 2], this.color_[0 * 4 + 3]).ToVector3();
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
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition &&j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;

                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                //Console.WriteLine(model.Name);
                                model.Location = new Vector3(this.float_[0], model.Location.Y, model.Location.Z);
                            }
                        }

                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                model.Location = new Vector3(this.float_[0], model.Location.Y, model.Location.Z);
                            }
                        }
                    }
                    break;
                case Type.SetPositionY:
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                model.Location = new Vector3(model.Location.X, this.float_[0], model.Location.Z);
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                model.Location = new Vector3(model.Location.X, this.float_[0], model.Location.Z);
                            }
                        }
                    }
                    break;
                case Type.SetPositionZ:
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                model.Location = new Vector3(model.Location.X, model.Location.Y, this.float_[0]);
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                model.Location = new Vector3(model.Location.X, model.Location.Y, this.float_[0]);
                            }
                        }
                    }
                    break;
                case Type.SetRotation:
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                model.DestRotate = this.float_[0];
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                model.DestRotate = this.float_[0];
                            }
                        }
                    }
                    break;
                case Type.GotoCutscene:
                case Type.Goto:
                    if (this.CommandType == Type.GotoCutscene && !ScenePlayer.ScenePlaying)
                        break;
                    if (this.CommandType == Type.Goto && ScenePlayer.ScenePlaying)
                        break;

                    if ((this.string_[0] == 'p' &&
                        this.string_[1] == 'a' &&
                        this.string_[2] == 'r' &&
                        this.string_[3] == 't' &&
                        this.string_[4] == 'n' &&
                        this.string_[5] == 'e' &&
                        this.string_[6] == 'r' &&
                        this.string_[7] == '1') && Program.game.Partner1 != null)
                    {
                        Program.game.Partner1.Goto.X = this.vector_[0 * 3];
                        Program.game.Partner1.Goto.Y = this.vector_[0 * 3 + 1];
                        Program.game.Partner1.Goto.Z = this.vector_[0 * 3 + 2];
                    }
                    else
                        for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                        {
                            currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                            for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                                if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                    currentCondition = false;
                            if (currentCondition)
                            {
                                var model = MainGame.ResourceFiles[i] as Model;
                                if (MainGame.ResourceFiles[i] is Moveset mset)
                                {
                                    model = (mset.Links[0] as Model);
                                }
                                if (model != null)
                                {
                                    model.Goto.X = this.vector_[0 * 3];
                                    model.Goto.Y = this.vector_[0 * 3 + 1];
                                    model.Goto.Z = this.vector_[0 * 3 + 2];
                                }
                            }
                        }
                    break;

                case Type.SetMap:
                    fixed (char* s = this.string_)
                        Program.game.SetMap(new string(s, 0 * 32, 32).TrimEnd('\x0'), false);
                    break;
                case Type.RunScene:
                    fixed (char* s = this.string_)
                        ScenePlayer.RunScene(new string(s, 0 * 32, 32).TrimEnd('\x0'));
                    break;
                case Type.SetMatrix:

                    Model modelNoNull = null;
                    string str = "";

                    fixed (char* s = this.string_)
                        str = new string(s, 0 * 32, 32).TrimEnd('\x0');

                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        if (MainGame.ResourceFiles[i].Name == str)
                        {
                            modelNoNull = MainGame.ResourceFiles[i] as Model;
                        }
                    }
                    if (modelNoNull == null)
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        if (Program.game.Map.Supp[i].Name == str)
                        {
                            modelNoNull = Program.game.Map.Supp[i] as Model;
                        }
                    }
                    if (modelNoNull != null)
                    {
                        Bone b = modelNoNull.Skeleton.Bones[this.int_[0]];
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
                        modelNoNull.RecreateVertexBuffer(true);
                    }

                    break;
                case Type.GoForward:
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                model.Location += new Vector3(
                                    (float)(this.float_[0] * Math.Sin((float)(model.Rotate))),
                                    0,
                                    (float)(this.float_[0] * Math.Cos((float)(model.Rotate))));
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                model.Location += new Vector3(
                                    (float)(this.float_[0] * Math.Sin((float)(model.Rotate))),
                                    0,
                                    (float)(this.float_[0] * Math.Cos((float)(model.Rotate))));
                            }
                        }
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
                                    mset.FrameStep = this.float_[0];
                                    if (mset.FrameStep < 0 && mset.PlayingFrame < 0.000001)
                                        break;
                                    mset.ComputeAnimation();
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
                            Audio.effectInstances[ind].Volume += ((this.float_[0] / 4f) - Audio.effectInstances[ind].Volume) / this.float_[1];
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
                    Model modelNoNull_ = null;
                    string str_ = "";

                    fixed (char* s = this.string_)
                        str_ = new string(s, 0 * 32, 32).TrimEnd('\x0');

                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        if (MainGame.ResourceFiles[i].Name == str_)
                        {
                            modelNoNull_ = MainGame.ResourceFiles[i] as Model;
                        }
                    }
                    if (modelNoNull_ == null)
                        for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                        {
                            if (Program.game.Map.Supp[i].Name == str_)
                            {
                                modelNoNull_ = Program.game.Map.Supp[i] as Model;
                            }
                        }
                    if (modelNoNull_ != null)
                    {
                        modelNoNull_.Figed = this.boolean_[0];
                        if (modelNoNull_.Links.Count>0)
                        {
                            var mset = modelNoNull_.Links[0] as Moveset;
                            mset.Figed = this.boolean_[0];
                        }
                    }
                    break;
                    
                case Type.SetReactionCommand:
                    MainGame.DoModel = null;
                    for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + MainGame.ResourceFiles[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < MainGame.ResourceFiles[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != MainGame.ResourceFiles[i].Name[j])
                                currentCondition = false;
                        if (currentCondition)
                        {
                            var model = MainGame.ResourceFiles[i] as Model;
                            if (MainGame.ResourceFiles[i] is Moveset mset)
                            {
                                model = (mset.Links[0] as Model);
                            }
                            if (model != null)
                            {
                                MainGame.DoModel = model;
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < Program.game.Map.Supp.Count; i++)
                    {
                        currentCondition = this.string_[0 * 32 + Program.game.Map.Supp[i].Name.Length] == 0; /* STRING COMPARE */
                        for (int j = 0; currentCondition && j < Program.game.Map.Supp[i].Name.Length; j++)
                            if (this.string_[0 * 32 + j] != Program.game.Map.Supp[i].Name[j])
                                currentCondition = false;

                        if (currentCondition)
                        {
                            var model = Program.game.Map.Supp[i];
                            if (model != null)
                            {
                                MainGame.DoModel = model;
                                break;
                            }
                        }
                    }
                    MainGame.DoBone = this.int_[0];
                    MainGame.DoVector.X = this.vector_[0*3];
                    MainGame.DoVector.Y = this.vector_[0*3+1];
                    MainGame.DoVector.Z = this.vector_[0*3+2];
                    MainGame.ReactionCommand = this.action_;
                    MainGame.UpdateReactionCommand = true;
                    break;
                case Type.SetBoolean:
                    int valID = this.int_[0];
                    bool val = this.boolean_[0];
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
