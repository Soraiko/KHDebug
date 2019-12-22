using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kenuno;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class MSET : Moveset
    {
        public class MotInf
        {
            public Mt1 mt1;
            public float maxtick;
            public float mintick;
            public bool isRaw = false;
        }

        class Szexp
        {
            public static void Decode(byte[] eeramx, byte[] eeram, Int64 outSize)
            {
                MemoryStream sic = new MemoryStream(eeramx, false);
                BinaryReader br = new BinaryReader(sic);
                sic.Position = sic.Length - 8 * 3;
                Int64 pos0 = br.ReadInt64();
                Int64 pos1 = br.ReadInt64();
                Int64 pos2 = br.ReadInt64();
                sic.Position = pos1; byte[] prop = br.ReadBytes((int)(pos2 - pos1));
                sic.Position = pos0;

                SevenZip.Compression.LZMA.Decoder dec = new SevenZip.Compression.LZMA.Decoder();

                dec.SetDecoderProperties(prop);
                dec.Code(sic, new MemoryStream(eeram, true), pos1 - pos0, outSize, null);
            }
        }
		public static byte[] eeramx = File.ReadAllBytes("Content/eeramx.lzma");


        public MSET()
        {

        }

        public MSET(string filename)
        {
            Console.WriteLine("Loading Resource " + filename);
            this.Type = ResourceType.Moveset;
            this.msetBytes = new byte[0];
            this.skelBytes = new byte[0];
            this.blks = new Msetblk[0];
            this.blksRaw = new MsetRawblk[0];
            this.tags = new List<MotInf>(0);
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;

            if (File.Exists(this.FileName))
            {
                using (FileStream fs = File.OpenRead(this.FileName))
                {
                    this.Moveset = new Msetfst(fs, Path.GetFileName(this.FileName));

                }

                foreach (Mt1 mt1 in this.Moveset.al1)
                {
                    MotInf mi = new MotInf
                    {
                        mt1 = mt1
                    };
                    if (mt1.isRaw)
                    {
                        MsetRawblk blk = new MsetRawblk(new MemoryStream(mt1.bin, false));
                        mi.maxtick = blk.cntFrames;
                        mi.mintick = 0;
                    }
                    else
                    {
                        Msetblk blk = new Msetblk(new MemoryStream(mt1.bin, false));
                        mi.maxtick = (blk.to.al11.Length != 0) ? blk.to.al11[blk.to.al11.Length - 1] : 0;
                        mi.mintick = (blk.to.al11.Length != 0) ? blk.to.al11[0] : 0;
                    }
                    this.tags.Add(mi);
                }
                this.msetBytes = File.ReadAllBytes(this.FileName);
                this.msetStream = new MemoryStream(this.msetBytes, false);

            }
        }
        public List<MotInf> tags;

        public Msetfst Moveset;

        public byte[] msetBytes;
        MemoryStream msetStream;

        public byte[] skelBytes;
        MemoryStream skelStream;

        public void Parse()
        {
            this.Skeleton = (this.Links[0] as Model).Skeleton;
            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel"))
            {
                this.skelBytes = File.ReadAllBytes(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel");
            }
            else
            {
                CreateSkeleton();
                if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".mdlx"))
                {
                    FileStream fs = new FileStream(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".mdlx", FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryReader reader = new BinaryReader(fs);
                    fs.Position = 4;
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        fs.Position = 16 + i * 16;
                        if (reader.ReadInt32() == 4)
                        {
                            reader.ReadInt32();
                            fs.Position = reader.ReadInt32() + 0xA0;
                            break;
                        }
                    }
                    short boneCount = reader.ReadInt16();
                    reader.ReadInt16();
                    fs.Position = fs.Position - 0x14 + reader.ReadInt32();

                    reader.Read(this.skelBytes, 0x1F0, boneCount * 0x40);
                    reader.Close();
                    fs.Close();
                }
                File.WriteAllBytes(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel",this.skelBytes);
            }


            if (this.skelBytes.Length > 0)
            {
                this.skelStream = new MemoryStream(this.skelBytes);
                this.LoadMoves();
            }
        }

        public void CreateSkeleton()
        {
            this.skelBytes = new byte[0x01F0 + this.Skeleton.Bones.Count * 0x40];
            Array.Copy(Encoding.ASCII.GetBytes("BAR"), this.skelBytes, 3);
            this.skelBytes[3] = 1;
            this.skelBytes[4] = 1;
            this.skelBytes[0x10] = 4;
            this.skelBytes[0x18] = 0x20;
            Array.Copy(BitConverter.GetBytes(0x01D0 + this.Skeleton.Bones.Count * 0x40), 0, this.skelBytes, 0x1C, 3);
            this.skelBytes[0xB0] = 0x03;

            Array.Copy(BitConverter.GetBytes((short)this.Skeleton.Bones.Count), 0, this.skelBytes, 0xC0, 2);
            this.skelBytes[0xC2] = 3;
            Array.Copy(BitConverter.GetBytes(0x140), 0, this.skelBytes, 0xC4, 4);
            Array.Copy(BitConverter.GetBytes(0x20), 0, this.skelBytes, 0xC8, 4);

            for (int i = 0; i < this.Skeleton.Bones.Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(i), 0, this.skelBytes, 0x1F0 + i * 0x40, 4);
                if (this.Skeleton.Bones[i].Parent != null)
                    Array.Copy(BitConverter.GetBytes(this.Skeleton.IndexOf(this.Skeleton.Bones[i].Parent.Name)), 0, this.skelBytes, 0x1F0 + i * 0x40 + 4, 4);
                else
                    Array.Copy(BitConverter.GetBytes(-1), 0, this.skelBytes, 0x1F0 + i * 0x40 + 4, 4);

				/*Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleX), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x10, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleY), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x14, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleZ), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x18, 4);
*/
				this.Skeleton.Bones[i].GetSRT(this.Skeleton.Bones[i].LocalMatrix);

				Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleX), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x10, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleY), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x14, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].ScaleZ), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x18, 4);

                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].Flag1), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x08, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].Flag2), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x0C, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].Visibility), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x1C, 4);

                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].RotateX), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x20, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].RotateY), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x24, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].RotateZ), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x28, 4);

                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].TranslateX), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x30, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].TranslateY), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x34, 4);
                Array.Copy(BitConverter.GetBytes(this.Skeleton.Bones[i].TranslateZ), 0, this.skelBytes, 0x1F0 + i * 0x40 + 0x38, 4);
            }
        }


        Msetblk[] blks;
        MsetRawblk[] blksRaw;
        Mlink olS;

        public int Count;
        public void LoadMoves()
        {
            if (this.Moveset != null)
            {
                this.Count = this.Moveset.al1.Count;
                this.blks = new Msetblk[this.Count];
                this.blksRaw = new MsetRawblk[this.Count];
                this.olS = new Mlink();

                for (int i = 0; i < this.Count; i++)
                {
                    if (this.Moveset.al1[i].isRaw)
                    {
                        this.blksRaw[i] = new MsetRawblk(new MemoryStream(this.Moveset.al1[i].bin, false));
                    }
                    else
                    {
                        this.blks[i] = new Msetblk(new MemoryStream(this.Moveset.al1[i].bin, false));
                    }
                }
            }
        }


        public void Unwrap()
        {
            int totalClean = 0;
            while (totalClean < this.Skeleton.Bones.Count - 1)
            {
                totalClean = 0;
                for (int u = 0; u < this.Skeleton.Bones.Count; u++)
                {
                    Bone currBone = this.Skeleton.Bones[u];
                    Matrix mat = currBone.localMatrix;
                    List<Bone> children = new List<Bone>(0);

                    for (int v = 0; v < this.Skeleton.Bones.Count; v++)
                    {
                        if (u == v) continue;
                        Bone currChild = this.Skeleton.Bones[v];
                        if (currChild.Parent != null && currChild.Parent.Name == currBone.Name)
                        {
                            children.Add(currChild);
                        }
                    }
                    int dirtyCount = 0;
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (!children[i].DirtyMatrix)
                            dirtyCount++;
                    }
                    if (currBone.DirtyMatrix && dirtyCount == children.Count && currBone.Parent != null)
                    {
                        currBone.localMatrix *= Matrix.Invert(currBone.Parent.localMatrix);
                        currBone.DirtyMatrix = false;
                    }
                    if (!currBone.DirtyMatrix)
                        totalClean++;
                }

            }
        }

        public void ExportBinary()
        {
            if (!Directory.Exists(Path.GetDirectoryName(this.FileName)+@"\exportedMSET"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.FileName) + @"\exportedMSET");
            }
            for (int i=0;i<this.blksRaw.Length;i++)
            {
                FileStream outputStream = new FileStream(Path.GetDirectoryName(this.FileName) + @"\exportedMSET\move_" + i.ToString("d3") + ".bin",FileMode.OpenOrCreate);
                BinaryWriter bw = new BinaryWriter(outputStream);


                if (false&&File.Exists(Path.GetDirectoryName(this.FileName) + @"\MSET\move_" + i.ToString("d3") + ".bin"))
                {
                    FileStream inputStream = new FileStream(Path.GetDirectoryName(this.FileName) + @"\MSET\move_" + i.ToString("d3") + ".bin", FileMode.Open);
                    BinaryReader br = new BinaryReader(inputStream);
                    bw.Write(br.ReadInt32());
                    bw.Write(br.ReadInt32());
                    bw.Write(br.ReadInt32());
                    bw.Write(br.ReadInt32());
                    br.Close();
                    inputStream.Close();
                }
                else
                {
                    bw.Write((int)this.tags[i].maxtick);
                    bw.Write(0);
                    bw.Write(0);
                    bw.Write(0);
                }
                for (int j=0;j< (int)this.tags[i].maxtick; j++)
                {
                    if (this.blksRaw[i] != null)
                    {
                        int t0 = Math.Max(0, Math.Min(this.blksRaw[i].cntFrames - 1, (int)Math.Floor((double)j)));
                        int t1 = Math.Max(0, Math.Min(this.blksRaw[i].cntFrames - 1, (int)Math.Ceiling((double)j)));
                        if (t0 == t1)
                        {
                            MsetRM rm = this.blksRaw[i].alrm[t0];
                        }
                        else
                        {
                            MsetRM rm0 = this.blksRaw[i].alrm[t0]; float f1 = j % 1.0f;
                            MsetRM rm1 = this.blksRaw[i].alrm[t1]; float f0 = 1.0f - f1;
                            for (int t = 0; t < this.blksRaw[i].cntJoints; t++)
                            {
                                Matrix m = Matrix.Identity;
                                m.M11 = (rm0.al[t].M11 * f0 + rm1.al[t].M11 * f1);
                                m.M12 = (rm0.al[t].M12 * f0 + rm1.al[t].M12 * f1);
                                m.M13 = (rm0.al[t].M13 * f0 + rm1.al[t].M13 * f1);
                                m.M14 = (rm0.al[t].M14 * f0 + rm1.al[t].M14 * f1);

                                m.M21 = (rm0.al[t].M21 * f0 + rm1.al[t].M21 * f1);
                                m.M22 = (rm0.al[t].M22 * f0 + rm1.al[t].M22 * f1);
                                m.M23 = (rm0.al[t].M23 * f0 + rm1.al[t].M23 * f1);
                                m.M24 = (rm0.al[t].M24 * f0 + rm1.al[t].M24 * f1);

                                m.M31 = (rm0.al[t].M31 * f0 + rm1.al[t].M31 * f1);
                                m.M32 = (rm0.al[t].M32 * f0 + rm1.al[t].M32 * f1);
                                m.M33 = (rm0.al[t].M33 * f0 + rm1.al[t].M33 * f1);
                                m.M34 = (rm0.al[t].M34 * f0 + rm1.al[t].M34 * f1);

                                m.M41 = (rm0.al[t].M41 * f0 + rm1.al[t].M41 * f1);
                                m.M42 = (rm0.al[t].M42 * f0 + rm1.al[t].M42 * f1);
                                m.M43 = (rm0.al[t].M43 * f0 + rm1.al[t].M43 * f1);
                                m.M44 = (rm0.al[t].M44 * f0 + rm1.al[t].M44 * f1);
                                this.Skeleton.Bones[t].localMatrix = m;
                                this.Skeleton.Bones[t].DirtyMatrix = true;
                            }
                        }
                    }
                    else
                    {
                        this.olS.getMats(this.skelStream, this.blks[i].cntb1, this.msetStream, this.blks[i].cntb2, this.Moveset.al1[i].off, j);
                        
                        for (int t = 0; t < this.blks[i].cntb1; t++)
                        {
                            Matrix m = Matrix.Identity;
                            m.M11 = (Mlink.MatrixBuffer[t].M11);
                            m.M12 = (Mlink.MatrixBuffer[t].M12);
                            m.M13 = (Mlink.MatrixBuffer[t].M13);
                            m.M14 = (Mlink.MatrixBuffer[t].M14);

                            m.M21 = (Mlink.MatrixBuffer[t].M21);
                            m.M22 = (Mlink.MatrixBuffer[t].M22);
                            m.M23 = (Mlink.MatrixBuffer[t].M23);
                            m.M24 = (Mlink.MatrixBuffer[t].M24);

                            m.M31 = (Mlink.MatrixBuffer[t].M31);
                            m.M32 = (Mlink.MatrixBuffer[t].M32);
                            m.M33 = (Mlink.MatrixBuffer[t].M33);
                            m.M34 = (Mlink.MatrixBuffer[t].M34);

                            m.M41 = (Mlink.MatrixBuffer[t].M41);
                            m.M42 = (Mlink.MatrixBuffer[t].M42);
                            m.M43 = (Mlink.MatrixBuffer[t].M43);
                            m.M44 = (Mlink.MatrixBuffer[t].M44);
                            this.Skeleton.Bones[t].localMatrix = m;
                            this.Skeleton.Bones[t].DirtyMatrix = true;
                        }
                    }
                    Unwrap();

                    for (int k=0;k<this.Skeleton.Bones.Count;k++)
                    {
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M11);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M12);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M13);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M14);

                        bw.Write(this.Skeleton.Bones[k].localMatrix.M21);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M22);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M23);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M24);

                        bw.Write(this.Skeleton.Bones[k].localMatrix.M31);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M32);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M33);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M34);

                        bw.Write(this.Skeleton.Bones[k].localMatrix.M41);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M42);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M43);
                        bw.Write(this.Skeleton.Bones[k].localMatrix.M44);
                    }
                }
                bw.Close();
                outputStream.Close();
            }
        }
    
        public void GetFrameData_()
        {
            this.PlayingFrame += this.FrameStep;

            if (LoopAnimation && this.PlayingFrame > this.tags[this.playingIndex].maxtick - 1)
            {
                this.PlayingFrame = this.MinTick;
            }
        

            if (this.blksRaw[this.playingIndex] != null)
            {
                int t0 = Math.Max(0, Math.Min(this.blksRaw[this.playingIndex].cntFrames - 1, (int)Math.Floor(PlayingFrame)));
                int t1 = Math.Max(0, Math.Min(this.blksRaw[this.playingIndex].cntFrames - 1, (int)Math.Ceiling(PlayingFrame)));
                if (t0 == t1)
                {
                    MsetRM rm = this.blksRaw[this.playingIndex].alrm[t0];
                }
                else
                {
                    MsetRM rm0 = this.blksRaw[this.playingIndex].alrm[t0]; float f1 = PlayingFrame % 1.0f;
                    MsetRM rm1 = this.blksRaw[this.playingIndex].alrm[t1]; float f0 = 1.0f - f1;
                    for (int t = 0; t < this.blksRaw[this.playingIndex].cntJoints; t++)
                    {
                        this.Skeleton.Bones[t].GlobalMatrix.M11 = rm0.al[t].M11 * f0 + rm1.al[t].M11 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M21 = rm0.al[t].M21 * f0 + rm1.al[t].M21 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M31 = rm0.al[t].M31 * f0 + rm1.al[t].M31 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M41 = rm0.al[t].M41 * f0 + rm1.al[t].M41 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M12 = rm0.al[t].M12 * f0 + rm1.al[t].M12 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M22 = rm0.al[t].M22 * f0 + rm1.al[t].M22 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M32 = rm0.al[t].M32 * f0 + rm1.al[t].M32 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M42 = rm0.al[t].M42 * f0 + rm1.al[t].M42 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M13 = rm0.al[t].M13 * f0 + rm1.al[t].M13 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M23 = rm0.al[t].M23 * f0 + rm1.al[t].M23 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M33 = rm0.al[t].M33 * f0 + rm1.al[t].M33 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M43 = rm0.al[t].M43 * f0 + rm1.al[t].M43 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M14 = rm0.al[t].M14 * f0 + rm1.al[t].M14 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M24 = rm0.al[t].M24 * f0 + rm1.al[t].M24 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M34 = rm0.al[t].M34 * f0 + rm1.al[t].M34 * f1;
                        this.Skeleton.Bones[t].GlobalMatrix.M44 = rm0.al[t].M44 * f0 + rm1.al[t].M44 * f1;
                    }
                }
            }
            else
            {
                this.olS.getMats(this.skelStream, this.blks[this.playingIndex].cntb1, this.msetStream, this.blks[this.playingIndex].cntb2, this.Moveset.al1[this.playingIndex].off, PlayingFrame);

                //if (this.Master!=null)
                //    Mlink.MatrixBuffer[0] += this.Master.Skeleton.Bones[this.Master.Skeleton.LeftHandBone].Matrix;
                //Console.WriteLine(this.blks[this.playingIndex].cntb1);
                for (int t = 0; t < this.blks[this.playingIndex].cntb1; t++)
                {
                    this.Skeleton.Bones[t].GetSRT(Mlink.MatrixBuffer[t]);
                    this.Skeleton.Bones[t].DirtyMatrix = true;
                    this.Skeleton.Bones[t].GlobalMatrix = this.Skeleton.Bones[t].LocalMatrix;
                }
            }
        }

        

    }
}