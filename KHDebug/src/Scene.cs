using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class Scene
    {
        public string Name;
        public enum SceneCommand
        {
            SetCamera = 0,
            PlayAnim = 1,
            Gameplay = 2,
            ContactCollison = 3,
            Goto = 4
        }

        private List<SceneCommand> Commands;
        private List<object> Instructions;
        private List<int> StartFrames;
        private int Frame = 0;
        public void Reset()
        {
            for (int i = 0; i < this.Commands.Count; i++)
            {
                switch (this.Commands[i])
                {
                    case SceneCommand.PlayAnim:
                        (this.Instructions[i] as Animation).Init();
                    break;
                }
            }
            this.Frame = 0;
            ScenePlayer.CheckScenePlaying();
        }

        private int MaxFrame = 0;

        public bool Playing
        {
            get
            {
                return this.Frame <= this.MaxFrame;
            }
        }

        public void RenderNext()
        {
            if (!this.Playing)
                return;
            for (int i=0;i< this.StartFrames.Count;i++)
            {
                int currF = this.StartFrames[i];
                
                if (this.Frame == currF)
                {
                    switch (this.Commands[i])
                    {
                        case SceneCommand.PlayAnim:
                            (this.Instructions[i] as Animation).Init();
                        break;
                        case SceneCommand.Goto:
                            (this.Instructions[i] as GotoDest).Init();
                        break;
                    }
                }

                switch (this.Commands[i])
                {
                    case SceneCommand.Goto:

                        if (this.Frame == currF)
                        {
                            (this.Instructions[i] as GotoDest).Perform();
                        }
                        break;
                    case SceneCommand.PlayAnim:
                        if (this.Frame>= currF)
                        {
                            var anim = (this.Instructions[i] as Animation);

                            anim.RenderNext();
                        }
                    break;
                    case SceneCommand.SetCamera:

                        if (this.Frame == currF)
                        {
                            var v3 = (this.Instructions[i] as Vector3[]);
                            if (v3.Length == 1)
                            {
                                if (!Single.IsNaN(v3[0].X))
                                {
                                    float yaw = Program.game.mainCamera.Yaw;
                                    float then = v3[0].X;

                                    float nearest = Single.MaxValue;

                                    for (sbyte p=-50;p<50;p++)
                                    {
                                        float curr = v3[0].X + p * MainGame.PI * 2;
                                        float dist = Math.Abs(yaw - curr);
                                        if (dist< nearest)
                                        {
                                            then = curr;
                                            nearest = dist;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    Program.game.mainCamera.DestYaw = then;
                                    //Program.game.mainCamera.Yaw = v3[0].X;
                                }
                                if (!Single.IsNaN(v3[0].Y))
                                {
                                    Program.game.mainCamera.DestPitch = v3[0].Y;
                                    //Program.game.mainCamera.Pitch = v3[0].Y;
                                }
                                Program.game.mainCamera.Zoom = Program.game.mainCamera.MaxZoom;
                            }
                        }
                        break;
                    case SceneCommand.ContactCollison:

                        if (this.Frame == currF)
                        {
                            var v3 = (bool)this.Instructions[i];
                            ScenePlayer.AllowContactCollision = v3;
                        }
                        break;
                        
                    case SceneCommand.Gameplay:

                        if (this.Frame == currF)
                        {
                            for (int j = 0; j < this.Instructions.Count; j++)
                            {
                                var anm = (this.Instructions[j] as Animation);
                                if (anm != null)
                                {
                                    anm.AllIdle();
                                }
                                var gt = (this.Instructions[j] as GotoDest);
                                if (gt != null)
                                {
                                    gt.Idle();
                                }
                            }
                        }

                        break;
                }
            }

            this.Frame++;
            ScenePlayer.CheckScenePlaying();
        }

        public Scene(string fname)
        {
            this.Commands = new List<SceneCommand>(0);
            this.Instructions = new List<object>(0);
            this.StartFrames = new List<int>(0);

            string[] content = File.ReadAllLines(@"Content\Scenes\" + fname + ".txt");
            string[] spli1, spli2;
            for (int i=0;i<content.Length;i++)
            {
                spli1 = content[i].Split('|');
                spli2 = spli1[2].Split(';');
                int startFrame = int.Parse(spli1[0]);

                switch (spli1[1])
                {
                    case "Goto":
                        if (startFrame > this.MaxFrame)
                            this.MaxFrame = startFrame;

                        Model mdl_ = null;
                        for (int r = 0; r < MainGame.ResourceFiles.Count; r++)
                        {
                            if (MainGame.ResourceFiles[r].Name ==  spli2[0])
                            {
                                mdl_ = MainGame.ResourceFiles[r] as Model;
                                break;
                            }
                        }
                        Vector3 dest = new Vector3(MainGame.SingleParse(spli2[1]), MainGame.SingleParse(spli2[2]), MainGame.SingleParse(spli2[3]));

                        this.Instructions.Add(new GotoDest(mdl_,dest));
                        this.Commands.Add(SceneCommand.Goto);
                        break;
                    case "ContactCollison":
                        if (startFrame > this.MaxFrame)
                            this.MaxFrame = startFrame;

                        this.Instructions.Add(Boolean.Parse(spli2[0]));
                        this.Commands.Add(SceneCommand.ContactCollison);
                        break;
                    case "SetCamera":
                        if (startFrame>this.MaxFrame)
                        this.MaxFrame = startFrame;
                        Vector3[] insts = new Vector3[spli2.Length / 3];
                        for (int n = 0; n < spli2.Length; n++)
                        {
                            float val = Single.NaN;
                            Single.TryParse(spli2[n], out val);

                            if (n % 3 == 0)
                            {
                                insts[n / 3].X = val;
                            }
                            if (n % 3 == 1)
                            {
                                insts[n / 3].Y = val;
                            }
                            if (n % 3 == 2)
                            {
                                insts[n / 3].Z = val;
                            }
                        }
                        this.Instructions.Add(insts);
                        this.Commands.Add(SceneCommand.SetCamera);
                        break;
                    case "Gameplay":
                        if (startFrame > this.MaxFrame)
                            this.MaxFrame = startFrame;
                        this.Instructions.Add(null);
                        this.Commands.Add(SceneCommand.Gameplay);
                    break;
                    case "PlayAnim":
                        Animation anm = new Animation();

                        for (int n = 0; n < spli2.Length; n+=6)
                        {
                            spli2[n + 0] = spli2[n + 0].Replace("player", Program.game.Player.Name);
                            spli2[n + 1] = spli2[n + 1].Replace("player", Program.game.Player.Name);
                            Model mdl = null;
                            BinaryMoveset mset = null;
                            for (int r = 0; r < MainGame.ResourceFiles.Count; r++)
                            {
                                if (MainGame.ResourceFiles[r].Name ==  spli2[n + 0])
                                {
                                    mdl = MainGame.ResourceFiles[r] as Model;
                                    break;
                                }
                            }
                            if (mdl == null)
                            for (int r = 0; r < Program.game.Map.Supp.Count; r++)
                            {
                                if (Program.game.Map.Supp[r].Name ==  spli2[n + 0])
                                {
                                    mdl = Program.game.Map.Supp[r] as Model;
                                    break;
                                }
                            }

                            mdl.Cutscene = true;
                            int index = -1;
                            if (int.TryParse(spli2[n + 1], out index))
                            {
                                mset = (mdl.Links[0] as BinaryMoveset);
                            }
                            else
                            {
                                string name = Path.GetFileNameWithoutExtension(@"Content\Scenes\" + spli2[n + 1]);

                                for (int r=0;r< MainGame.ResourceFiles.Count;r++)
                                {
                                    if (MainGame.ResourceFiles[r].Name ==  name)
                                    {
                                        mset = MainGame.ResourceFiles[r] as BinaryMoveset;
                                    }
                                }
                                if (mset == null)
                                {
                                    mset = new BinaryMoveset(@"Content\Scenes\" + spli2[n + 1]);
                                    MainGame.ResourceFiles.Add(mset);
                                }
                                index = 0;
                            }
                            if (mset.Links.Count == 0)
                                mset.Links.Add(mdl);
                            
                            mset.Skeleton = mdl.Skeleton;
                            mset.Voices = (mdl.Links[0] as Moveset).Voices;
                            //mset.Parse();

                            Vector3 startPos = Vector3.Zero;
                            startPos.X = MainGame.SingleParse(spli2[n + 2]);
                            startPos.Y = MainGame.SingleParse(spli2[n + 3]);
                            startPos.Z = MainGame.SingleParse(spli2[n + 4]);
                            float rot = MainGame.SingleParse(spli2[n + 5]);
                            anm.Append(mdl, mset, index, startPos, rot);

                            if (startFrame+mset.MaxTick > this.MaxFrame)
                                this.MaxFrame = startFrame + mset.MaxTick;
                        }
                        this.Instructions.Add(anm);
                        this.Commands.Add(SceneCommand.PlayAnim);
                        break;
                }
                this.StartFrames.Add(startFrame);
            }
        }
    }

    public static class ScenePlayer
    {
        public static bool AllowContactCollision;
        public static List<string> SceneNames = new List<string>(0);
        public static List<Scene> Scenes = new List<Scene>(0);

        public static void RunScene(string name)
        {
            int index = SceneNames.IndexOf(name);
            if (index>-1)
            {
                Scenes[index].Reset();
            }
            else
            {
                SceneNames.Add(name);
                Scenes.Add(new Scene(name));
            }
            ScenePlaying = true;
        }

        public static bool ScenePlaying = false;

        public static void CheckScenePlaying()
        {
            bool playing = false;
            for (int i=0;i< Scenes.Count;i++)
            {
                if (Scenes[i].Playing)
                {
                    playing = true;
                    break;
                }
            }
            ScenePlaying = playing;
        }

    }

    public class GotoDest
    {
        private Model mdl;
        private Vector3 dest;

        public GotoDest(Model mdl, Vector3 dest)
        {
            this.mdl = mdl;
            this.dest= dest;
        }

        public void Perform()
        {
            this.mdl.Goto = this.dest;
        }

        public void Init()
        {
            this.mdl.Cutscene = true;
        }

        public void Idle()
        {
            this.mdl.Cutscene = false;
        }
    }

    public class Animation
    {
        private int Length;
        private int CurrFrame;
        private List<Vector3> StartPositions;
        private List<float> StartRotations;
        private List<Model> Models;
        private List<Moveset> Movesets;
        private List<int> PlayingIndices;

        public Animation()
        {
            this.Length = 0;
            this.CurrFrame = 0;
            this.Models = new List<Model>(0);
            this.Movesets = new List<Moveset>(0);
            this.PlayingIndices = new List<int>(0);
            this.StartPositions = new List<Vector3>(0);
            this.StartRotations = new List<float>(0);
        }
        public void Append(Model mdl, Moveset mset, int index, Vector3 startPos, float rot)
        {
            this.Models.Add(mdl);
            this.Movesets.Add(mset);
            this.PlayingIndices.Add(index);
            this.StartPositions.Add(startPos);
            this.StartRotations.Add(rot);
        }

        public void AllIdle()
        {
            for (int i = 0; i < this.Models.Count; i++)
            {
                for (int r = 0; r < MainGame.ResourceFiles.Count; r++)
                {
                    var mset = (MainGame.ResourceFiles[r] as Moveset);
                    if (mset != null && mset.ResourceIndex ==  this.Models[i].ResourceIndex)
                    {
                        mset.PlayingIndex = mset.idle;
                    }
                }
                this.Models[i].Cutscene = false;
            }
        }

        public void Init()
        {
            this.CurrFrame = 0;
            for (int i = 0; i < this.Models.Count; i++)
            {
                this.Models[i].Cutscene = true;
                this.Models[i].Location = this.StartPositions[i];
                this.Models[i].Rotate = this.StartRotations[i];
                this.Models[i].DestRotate = this.StartRotations[i];

                for (int r = 0; r < MainGame.ResourceFiles.Count; r++)
                {
                    if (MainGame.ResourceFiles[r] is Moveset && MainGame.ResourceFiles[r].ResourceIndex ==  this.Models[i].ResourceIndex)
                    {
                        (MainGame.ResourceFiles[r] as Moveset).PlayingIndex = -1;
                    }
                }
            }

            for (int i = 0; i < this.Movesets.Count; i++)
            {
                this.Movesets[i].PlayingIndex = -1;
                this.Movesets[i].PlayingIndex = this.PlayingIndices[i];
                this.Movesets[i].FrameStep = 0f;
                this.Movesets[i].PlayingFrame = -1;
                this.Movesets[i].ComputeAnimation();
                if ((int)this.Movesets[i].MaxTick > this.Length)
                {
                    this.Length = (int)this.Movesets[i].MaxTick;
                }
            }
        }

        public bool Finished
        {
            get
            {
                return this.CurrFrame == this.Length;
            }
        }

        public void RenderNext()
        {
            for (int i = 0; i < this.Models.Count; i++)
            {
                this.Models[i].DestRotate = this.StartRotations[i];
            }
            if (this.CurrFrame == this.Length)
                return;
            this.CurrFrame++;
            for (int i = 0; i < this.Movesets.Count; i++)
            {
                this.Movesets[i].PlayingFrame = this.CurrFrame;
                this.Movesets[i].ComputeAnimation();
            }
            for (int i = 0; i < this.Models.Count; i++)
            {
                this.Models[i].LowestFloor = this.Models[i].Location.Y;
                this.Models[i].RecreateVertexBuffer(true);
            }
        }
    }
}
