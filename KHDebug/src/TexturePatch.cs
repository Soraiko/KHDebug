using System;
using System.Drawing;
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
    public unsafe struct TexturePatch
    {
        public int TextureIndex;
        public fixed char Name[32];
        public int X;
        public int Y;
        public int Count;
        private Model model;
        public int Set;

        public TexturePatch(string filename, Model mdl)
        {
            this.f10Patching = false;
            this.Set = 0x584976;
            this.nextBlink = -1;
            this.currBlink = 0;
            this.blink_half = -1;
            this.blink = -1;
            this.original = null;
            this.lastIndex = -2;
            this.Width = 0;
            this.Height = 0;

            this.rnd = new Random(DateTime.Now.Millisecond);
            this.TextureIndex = -1;
            this.model = mdl;
            this.X = 0;
            this.Y = 0;
            this.Count = 1;
            char[] arr = Path.GetFileNameWithoutExtension(filename).ToArray();
            for (int i=0;i< arr.Length;i++)
            this.Name[i] = arr[i];

            string[] input = File.ReadAllLines(filename);
            for (int i=0;i<input.Length;i++)
            {
                string[] spli = input[i].Split('|');
                switch (spli[0].ToLower())
                {
                    case "index":
                        this.TextureIndex = int.Parse(spli[1]);
                    break;
                    case "x":
                        this.X = int.Parse(spli[1]);
                        break;
                    case "y":
                        this.Y = int.Parse(spli[1]);
                        break;
                    case "count":
                        this.Count = int.Parse(spli[1]);
                        break;
                    case "blink_half":
                        this.blink_half = int.Parse(spli[1]);
                        break;
                    case "blink":
                        this.blink = int.Parse(spli[1]);
                        break;
                }
            }
            patch = new Microsoft.Xna.Framework.Color[0];
            if (File.Exists(Path.GetDirectoryName(filename)+@"\"+ Path.GetFileNameWithoutExtension(filename)+".png"))
            {
                FileStream fs = new FileStream(Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".png",FileMode.Open);
                Texture2D t2d = Texture2D.FromStream(Program.game.graphics.GraphicsDevice, fs);
                patch = new Microsoft.Xna.Framework.Color[t2d.Width* t2d.Height];

                if (Program.game.Loading == MainGame.LoadingState.Loading)
                {
                    Program.game.sourceCopyT2D = t2d;
                    Program.game.OrderingThreadColorCopy = true;
                    System.Threading.Thread.Sleep(33);
                    Array.Copy(Program.game.sourceCopyColorArray, patch, patch.Length);
                }
                else
                {
                    t2d.GetData<Microsoft.Xna.Framework.Color>(patch);
                }

                this.Width = t2d.Width;
                this.Height = t2d.Height / this.Count;
            }
        }

        int lastIndex;
        private Microsoft.Xna.Framework.Color[] patch;
        private Texture2D original;
        private int Width;
        private int Height;

        public void GetPatch(int index)
        {
            if (original==null)
            {
                original = new Texture2D(Program.game.graphics.GraphicsDevice, this.model.Textures[this.TextureIndex].Width, this.model.Textures[this.TextureIndex].Height);
                Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[original.Width * original.Height];
                
                this.model.Textures[this.TextureIndex].GetData<Microsoft.Xna.Framework.Color>(data);
                original.SetData(data);
            }
            if (index != this.lastIndex)
            {
                if (index<0)
                {
                    Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[original.Width * original.Height];

                    original.GetData<Microsoft.Xna.Framework.Color>(data);

                    this.model.Textures[this.TextureIndex].SetData<Microsoft.Xna.Framework.Color>(data);
                }
                else
                {
                    Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[original.Width * original.Height];

                    this.model.Textures[this.TextureIndex].GetData<Microsoft.Xna.Framework.Color>(data);
                    for (int x = 0; x < this.Width; x++)
                        for (int y = 0; y < this.Height; y++)
                    {
                        data[(this.X + x) + (this.Y + y) * original.Width] = patch[x + ((index * this.Height) + y) * this.Width];
                    }
                    this.model.Textures[this.TextureIndex].SetData<Microsoft.Xna.Framework.Color>(data);
                }
            }
            this.lastIndex = index;
        }

        private int nextBlink;
        private int currBlink;
        private int blink_half;
        private int blink;
        private Random rnd;
        public bool f10Patching;



        public void Animate()
        {
            if (f10Patching)
                return;
            if (blink < 0 || blink_half < 0)
                return;
            if (nextBlink<0 || currBlink >= nextBlink)
            {
                nextBlink = this.rnd.Next(25, 250);
                currBlink = 0;
            }
            currBlink++;
            if (currBlink > nextBlink - 12)
            {
                GetPatch(this.blink);
            }
            else if (currBlink > nextBlink - 15)
            {
                GetPatch(this.blink_half);
            }
            else
                GetPatch(-1);
        }
    }
}
