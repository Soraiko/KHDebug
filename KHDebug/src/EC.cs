using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHDebug
{
    public struct EC
    {
        public struct Eff
        {
            public short Start;
            public short End;
            public short ID;
            public byte[] Data;
        }
        public List<Eff> Group1;
        public List<Eff> Group2;
        int oldFrame1;
        int oldFrame2;

        public List<Eff> GetG1(int frame)
        {
            List<Eff> output = new List<Eff>(0);
            if (frame != oldFrame1)
            {
                for (int i = 0; i < this.Group1.Count; i++)
                {
                    if (frame >= this.Group1[i].Start && (frame < this.Group1[i].End || this.Group1[i].End < 0))
                    {
                        output.Add(this.Group1[i]);
                    }
                }
            }
            oldFrame1 = frame;
            return output;
        }
        public List<Eff> GetG2(int frame)
        {
            List<Eff> output = new List<Eff>(0);
            if (frame != oldFrame2)
            {
                for (int i = 0; i < this.Group2.Count; i++)
                {
                    if (frame == this.Group2[i].Start)
                    {
                        output.Add(this.Group2[i]);
                    }
                }
            }
            oldFrame2 = frame;
            return output;
        }

        public EC(string fname)
        {
            this.oldFrame1 = -1;
            this.oldFrame2 = -1;
            this.Group1 = new List<Eff>(0);
            this.Group2 = new List<Eff>(0);
            if (File.Exists(fname))
            {
                byte[] data = File.ReadAllBytes(fname);
                if (data.Length>0)
                {
                    byte cnt1 = data[0];
                    byte cnt2 = data[1];
                    byte start2 = data[2];
                    int pos = 4;

                    /*for (int j = 0; j < data.Length; j++)
                    {
                        Console.Write(data[j].ToString("X2") + " ");
                        if (j % 16 == 15)
                            Console.WriteLine("");
                    }*/

                    for (int i = 0; i < cnt1; i++)
                    {
                        Eff e_ = new Eff
                        {
                            Start = BitConverter.ToInt16(data, pos),
                            End = BitConverter.ToInt16(data, pos + 2),
                            ID = data[pos + 4]
                        };
                        int additionnalBytes = data[pos + 5] * 2;
                        e_.Data = new byte[additionnalBytes];
                        if (additionnalBytes > 0)
                        {
                            Array.Copy(data, pos + 6, e_.Data, 0, additionnalBytes);
                        }
                        this.Group1.Add(e_);
                        pos += 6 + additionnalBytes;
                    }
                    pos = start2;

                    for (int i = 0; i < cnt2; i++)
                    {
                        Eff e_ = new Eff
                        {
                            Start = BitConverter.ToInt16(data, pos)
                        };
                        e_.End = e_.Start;
                        e_.ID = data[pos + 2];

                        int additionnalBytes = data[pos + 3] * 2;
                        e_.Data = new byte[additionnalBytes];
                        if (additionnalBytes > 0)
                        {
                            Array.Copy(data, pos + 4, e_.Data, 0, additionnalBytes);
                        }
                        this.Group2.Add(e_);
                        pos += 4 + additionnalBytes;
                        /*Console.WriteLine("");
                        Console.WriteLine(e_.Start.ToString("X2"));
                        Console.WriteLine(e_.End.ToString("X2"));
                        Console.WriteLine(e_.ID.ToString("X2"));
                        for (int j = 0; j < e_.Data.Length; j++)
                        {
                            Console.Write(e_.Data[j].ToString("X2") + " ");
                            if (j % 16 == 15)
                                Console.WriteLine("");
                        }
                        Console.WriteLine("");*/
                    }
                }
            }
        }
    }
}
