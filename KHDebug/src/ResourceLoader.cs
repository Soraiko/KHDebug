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
    public static class ResourceLoader
    {

        public static List<DateTime> filesUniqueIDs = new List<DateTime>(0);
        public static List<string> filesFname = new List<string>(0);

        public static List<byte[]> filesBytes = new List<byte[]>(0);
        public static List<Texture2D> filesT2D = new List<Texture2D>(0);


        public static byte[] GetBytesArray(string fname)
        {
            byte[] output = EmptyFile;
            if (File.Exists(fname))
            {
                DateTime currTD = File.GetLastWriteTime(fname);
                int index = filesFname.IndexOf(fname);
                if (index > -1)
                {
                    DateTime lastTD = filesUniqueIDs[index];
                    if ((lastTD - currTD).ToString()[0] == '-')
                    {
                        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        output = new byte[fs.Length];
                        fs.Read(output, 0, output.Length);
                        fs.Close();
                    }
                    else
                    {
                        output = filesBytes[index];
                    }
                }
                else
                {
                    FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    output = new byte[fs.Length];
                    fs.Read(output, 0, output.Length);
                    filesFname.Add(fname);
                    filesUniqueIDs.Add(currTD);
                    filesBytes.Add(output);
                    filesT2D.Add(EmptyT2D);
                    fs.Close();
                }
            }
            return output;
        }
        public static System.Drawing.Bitmap EmptyBMP;
        public static Texture2D EmptyT2D;
        public static byte[] EmptyFile;

        public static void InitEmptyData()
        {
            EmptyBMP = new System.Drawing.Bitmap(1, 1);
            EmptyBMP.SetPixel(0, 0, System.Drawing.Color.FromArgb(255, 255, 255));
            EmptyT2D = new Texture2D(KHDebug.Program.game.graphics.GraphicsDevice, 1, 1);
            EmptyT2D.SetData<Color>(new Color[] { new Color(255, 255, 255,255) });
            EmptyFile = new byte[1024];
        }

        public static Texture2D GetT2D(string fname)
        {
            Texture2D output = EmptyT2D;
            if (File.Exists(fname))
            {
                DateTime currTD = File.GetLastWriteTime(fname);
                int index = filesFname.IndexOf(fname);
                if (index > -1)
                {
                    DateTime lastTD = filesUniqueIDs[index];
                    if ((lastTD - currTD).ToString()[0] == '-')
                    {
                        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (GetImageFormat(fs) != ImageFormat.unknown)
                        {
                            filesUniqueIDs[index] = currTD;


                            if (Program.game.Loading == MainGame.LoadingState.Loading)
                            {
                                Program.game.sourceCopyStream = fs;
                                Program.game.OrderingThreadTextureCopy = true;
                                System.Threading.Thread.Sleep(33);
                                output = Program.game.sourceCopyT2D;
                            }
                            else
                            {
                                output = Texture2D.FromStream(KHDebug.Program.game.graphics.GraphicsDevice, fs);
                            }

                            //output = Texture2D.FromStream(KHDebug.Program.game.graphics.GraphicsDevice, fs);
                            filesT2D[index] = output;
                        }
                        fs.Close();
                    }
                    else
                    {
                        output = filesT2D[index];
                    }
                }
                else
                {
                    FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (GetImageFormat(fs) != ImageFormat.unknown)
                    {
                        /*if (Program.game.Loading == MainGame.LoadingState.Loading)
                        {
                            Program.game.sourceCopyStream = fs;
                            Program.game.OrderingThreadTextureCopy = true;
                            System.Threading.Thread.Sleep(33);
                            output = Program.game.sourceCopyT2D;
                        }*/
                            output = Texture2D.FromStream(KHDebug.Program.game.graphics.GraphicsDevice, fs);
                        
                        //output = Texture2D.FromStream(KHDebug.Program.game.graphics.GraphicsDevice, fs);
                        filesFname.Add(fname);
                        filesUniqueIDs.Add(currTD);
                        filesBytes.Add(EmptyFile);
                        filesT2D.Add(output);
                    }
                    fs.Close();
                }
            }
            return output;
        }

        public enum ImageFormat
        {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            unknown
        }

        public static ImageFormat GetImageFormat(FileStream fs)
        {
            byte[] bytes = new byte[32];

            if (fs.Length >= 32)
            {
                fs.Read(bytes, 0, 32);
            }

            int notZeroCount = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                {
                    notZeroCount++;
                }
            }
            if (notZeroCount == 32)
                return ImageFormat.unknown;
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            return ImageFormat.unknown;
        }

    }
}
