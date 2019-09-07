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
    public static class DebugMenu
    {
        public static int MaxLineCount = 15;
        public static int MaxLineSize = 80;
        public static int Count = -1;
        public static string[] Lines;

        public static void WriteLine(int text)
        {
            WriteLine(text.ToString());
        }

        public static void WriteLine(string text)
        {
            if (Lines == null)
            {
                Lines = new string[MaxLineCount];
                for (int i=0;i< MaxLineCount;i++)
                {
                    for (int j = 0; j < MaxLineSize; j++)
                    {
                        Lines[i] += "\x0";
                    }
                }
            }
            if (Count< MaxLineCount-1)
            {
                Count++;
            }
            else
            {
                for (int i = 0; i < Lines.Length-1; i++)
                {
                    Lines[i] = Lines[i + 1];
                }
            }

            if (text.Length > MaxLineSize)
                text = text.Substring(0, MaxLineSize);
            while (text.Length < MaxLineSize)
                text += "\x0";
            Lines[Count] = text;
            White.A = 255;
            Black.A = 210;
            OpacityCountDown = 150;
        }

       public static int OpacityCountDown = 0;

        public static void Update()
        {
            if (Lines == null)
                return;

            /*var mdl = Program.game.mainCamera.Target;
            var mset = Program.game.mainCamera.Target.Links[0] as Moveset;
            CharToScreen.WriteText("Target              :" + mdl.Name, 5, 5, Color.White, Color.Black);
            CharToScreen.WriteText("_____________________", 5, 6, Color.White, Color.Black);
            CharToScreen.WriteText("X                   :" + mdl.Location.X.ToString(), 5, 7, Color.White, Color.Black);
            CharToScreen.WriteText("Y                   :" + mdl.Location.Y.ToString(), 5, 8, Color.White, Color.Black);
            CharToScreen.WriteText("Z                   :" + mdl.Location.Z.ToString(), 5, 9, Color.White, Color.Black);
            CharToScreen.WriteText("_____________________", 5, 10, Color.White, Color.Black);

            CharToScreen.WriteText("\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0", 5, 11, Color.White, Color.Transparent);
            CharToScreen.WriteText("Playing index       :"+ mset.PlayingIndex, 5, 11, Color.White, Color.Black);
            CharToScreen.WriteText("\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0", 5, 12, Color.White, Color.Transparent);
            CharToScreen.WriteText("Playing frame       :" + mset.PlayingFrame.ToString() + " / " + mset.MaxTick.ToString(), 5, 12, Color.White, Color.Black);
            CharToScreen.WriteText("\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0", 5, 13, Color.White, Color.Transparent);
            CharToScreen.WriteText("Control State       :" + mdl.cState.ToString(), 5, 13, Color.White, Color.Black);
            CharToScreen.WriteText("\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0\x0", 5, 14, Color.White, Color.Transparent);
            CharToScreen.WriteText("Animation State       :" + mdl.pState.ToString(), 5, 14, Color.White, Color.Black);*/
            for (int i=0;i<MaxLineCount;i++)
            {
                CharToScreen.WriteText(Lines[i],5,5+i, White, Black);
            }
            if (OpacityCountDown > 0)
                OpacityCountDown--;
            else
            {
                if (White.A > 0)
                    White.A--;
                if (Black.A > 1)
                    Black.A -= 2;
                else
                    Black.A = 0;
            }
        }
        public static Color White = Color.White;
        public static Color Black = Color.Black;
        //
    }
}
