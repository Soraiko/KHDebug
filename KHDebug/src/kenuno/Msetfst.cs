using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace kenuno {

    public class AxBone
    {
        public int cur, parent, v08, v0c;
        public float x1, y1, z1, w1; // S
        public float x2, y2, z2, w2; // R
        public float x3, y3, z3, w3; // T

        public AxBone Clone()
        {
            return (AxBone)MemberwiseClone();
        }
    }

    public class Mt1 {
        public uint off, len;
        public string id;
        public byte[] bin;
        public int k1;
        public FacMod fm;
        public bool isRaw;

        public override string ToString() {
            return id;
        }
    }
    public class FacMod {
        public List<Fac1> alf1 = new List<Fac1>();

        public FacMod(MemoryStream si) {
            if (si.Length < 2) return;
            BinaryReader br = new BinaryReader(si);
            byte cnt1 = br.ReadByte(); // @0x00
            byte cnt2 = br.ReadByte(); // @0x01
            br.ReadUInt16(); // @0x02
            for (int t1 = 0; t1 < cnt1; t1++) {
                Fac1 f1 = new Fac1();
                try {
                    f1.v0 = br.ReadInt16();
                    f1.v2 = br.ReadInt16();
                    if (f1.v0 == 0 && f1.v2 == -1 && t1 != 0) {
                        f1.v4 = 0;
                        f1.v6 = 0;
                    }
                    else {
                        f1.v4 = br.ReadInt16();
                        if (f1.v2 != -1) {
                            f1.v6 = br.ReadInt16();
                        }
                        else {
                            f1.v6 = 0;
                        }
                    }
                }
                catch (EndOfStreamException) { }
                alf1.Add(f1);
            }
        }
    }
    public class Fac1 {
        public short v0, v2, v4, v6;
    }
    public class Msetblk {
        public To to = new To();
        public int cntb1, cntb2;

        public Msetblk(Stream si) {
            PosTbl p = new PosTbl(si);
            int baseoff = 0;
            int tbloff = p.tbloff;

            cntb1 = p.va0;
            cntb2 = p.va2;

            BinaryReader br = new BinaryReader(si);

            int cnt9 = 0, cnt10 = 0, cnt12 = 0;
            if (true) { // cnt9
                si.Position = baseoff + tbloff + p.vc0 - baseoff; // t2
                for (int i2 = 0; i2 < p.vc4; i2++) {
                    br.ReadByte();
                    br.ReadByte();
                    br.ReadByte();
                    int tcx = br.ReadByte();
                    int tx = br.ReadUInt16();
                    cnt9 = Math.Max(cnt9, tx + tcx);
                }
                si.Position = baseoff + tbloff + p.vc8 - baseoff; // t2x
                for (int i2 = 0; i2 < p.vcc; i2++) {
                    br.ReadByte();
                    br.ReadByte();
                    br.ReadByte();
                    int tcx = br.ReadByte();
                    int tx = br.ReadUInt16();
                    cnt9 = Math.Max(cnt9, tx + tcx);
                }

                if (true) { // cnt10, cnt12
                    si.Position = baseoff + tbloff + p.vd0 - baseoff; // t9
                    for (int i9 = 0; i9 < cnt9; i9++) {
                        br.ReadUInt16();
                        int ti10 = br.ReadUInt16(); cnt10 = Math.Max(cnt10, ti10 + 1);
                        int ti12a = br.ReadUInt16(); cnt12 = Math.Max(cnt12, ti12a + 1);
                        int ti12b = br.ReadUInt16(); cnt12 = Math.Max(cnt12, ti12b + 1);
                    }
                }
            }
            int cntt8 = 0;
            if (true) {
                si.Position = baseoff + tbloff + p.ve0 - baseoff; // t3
                for (int i3 = 0; i3 < p.ve4; i3++) {
                    br.ReadUInt16();
                    br.ReadUInt16();
                    br.ReadUInt16();
                    int ti8 = br.ReadInt16(); cntt8 = Math.Max(cntt8, ti8 + 1);
                    br.ReadUInt16();
                    br.ReadUInt16();
                }
            }

            int off1 = tbloff + p.vb4; int cnt1 = p.vb8;
            si.Position = off1;
            for (int a1 = 0; a1 < cnt1; a1++) {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                float c04 = br.ReadSingle();
                to.al1.Add(new T1(c00, c02, c04));
            }

            int off10 = tbloff + p.vd8;
            si.Position = off10;
            to.al10 = new float[cnt10];
            for (int a = 0; a < cnt10; a++) {
                to.al10[a] = br.ReadSingle();
            }

            int off11 = tbloff + p.vd4; int cnt11 = p.vb0;
            si.Position = off11;
            to.al11 = new float[cnt11];
            for (int a = 0; a < cnt11; a++) {
                to.al11[a] = br.ReadSingle();
            }

            int off12 = tbloff + p.vdc;
            si.Position = off12;
            to.al12 = new float[cnt12];
            for (int a = 0; a < cnt12; a++) {
                to.al12[a] = br.ReadSingle();
            }

            int off9 = tbloff + p.vd0;
            si.Position = off9;
            for (int a = 0; a < cnt9; a++) {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                int c04 = br.ReadUInt16();
                int c06 = br.ReadUInt16();
                to.al9.Add(new T9(c00, c02, c04, c06));
            }

            int off2 = tbloff + p.vc0; int cnt2 = p.vc4;
            si.Position = off2;
            for (int a = 0; a < cnt2; a++) {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadByte();
                int c03 = br.ReadByte();
                int c04 = br.ReadUInt16(); // t9_xxxx
                T2 o2 = new T2(c00, c01, c02, c03, c04);
                to.al2.Add(o2);
            }
            for (int a = 0; a < cnt2; a++) {
                T2 o2 = to.al2[a];
                for (int b = 0; b < o2.c03; b++) {
                    T9 o9 = to.al9[o2.c04 + b];

                    int t9c00 = o9.c00; // t11_xxxx
                    int t9c02 = o9.c02; // t10_xxxx
                    int t9c04 = o9.c04; // t12_xxxx
                    int t9c06 = o9.c06; // t12_xxxx

                    o2.al9f.Add(new T9f(o2.c04 + b, to.al11[t9c00 >> 2], to.al10[t9c02], to.al12[t9c04], to.al12[t9c06]));
                }
            }

            int off2x = tbloff + p.vc8; int cnt2x = p.vcc;
            si.Position = off2x;
            for (int a = 0; a < cnt2x; a++) {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadByte();
                int c03 = br.ReadByte();
                int c04 = br.ReadUInt16(); // t9_xxxx
                T2 o2 = new T2(c00, c01, c02, c03, c04);
                to.al2x.Add(o2);
            }
            for (int a = 0; a < cnt2x; a++) {
                T2 o2 = to.al2x[a];
                for (int b = 0; b < o2.c03; b++) {
                    T9 o9 = to.al9[o2.c04 + b];

                    int t9c00 = o9.c00; // t11_xxxx
                    int t9c02 = o9.c02; // t10_xxxx
                    int t9c04 = o9.c04; // t12_xxxx
                    int t9c06 = o9.c06; // t12_xxxx

                    o2.al9f.Add(new T9f(o2.c04 + b, to.al11[t9c00 >> 2], to.al10[t9c02], to.al12[t9c04], to.al12[t9c06]));
                }
            }

            int off3 = tbloff + p.ve0; int cnt3 = p.ve4;
            si.Position = off3;
            for (int a3 = 0; a3 < cnt3; a3++) {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadUInt16();
                int c04 = br.ReadUInt16();
                int c06 = br.ReadUInt16();
                uint c08 = br.ReadUInt32();
                to.al3.Add(new T3(c00, c01, c02, c04, c06, c08));
            }

            int off4 = tbloff + p.vac; int cnt4 = p.va2;
            si.Position = off4;
            for (int a4 = 0; a4 < cnt4; a4++) {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                to.al4.Add(new T4(c00, c02));
            }

            to.off5 = tbloff + p.va8; to.cnt5 = (p.va2 - p.va0);
            si.Position = to.off5;
            for (int a5 = 0; a5 < to.cnt5; a5++) {
                AxBone o = new AxBone
                {
                    cur = br.ReadUInt16(),
                    parent = br.ReadUInt16(),
                    v08 = br.ReadUInt16(),
                    v0c = br.ReadUInt16()
                };
                br.ReadUInt64();
                o.x1 = br.ReadSingle();
                o.y1 = br.ReadSingle();
                o.z1 = br.ReadSingle();
                o.w1 = br.ReadSingle();
                o.x2 = br.ReadSingle();
                o.y2 = br.ReadSingle();
                o.z2 = br.ReadSingle();
                o.w2 = br.ReadSingle();
                o.x3 = br.ReadSingle();
                o.y3 = br.ReadSingle();
                o.z3 = br.ReadSingle();
                o.w3 = br.ReadSingle();
                to.al5.Add(o);
            }

            //listBoxJtbl.Items.Add(new Jtbl("t1", p.vb4, 8));
            //listBoxJtbl.Items.Add(new Jtbl("t2", p.vc0, 6));
            //listBoxJtbl.Items.Add(new Jtbl("t2x", p.vc8, 6));
            //listBoxJtbl.Items.Add(new Jtbl("t3", p.ve0, 12));
            //listBoxJtbl.Items.Add(new Jtbl("t4", p.vac, 4));
            //listBoxJtbl.Items.Add(new Jtbl("t5", p.va8, 16)); // 64
            //listBoxJtbl.Items.Add(new Jtbl("t6", p.vf8, 12));
            //listBoxJtbl.Items.Add(new Jtbl("t7", p.vf0, 8));
            //listBoxJtbl.Items.Add(new Jtbl("t8", p.vec, 16)); // 48
            //listBoxJtbl.Items.Add(new Jtbl("t9", p.vd0, 8));
            //listBoxJtbl.Items.Add(new Jtbl("t10", p.vd8, 4));
            //listBoxJtbl.Items.Add(new Jtbl("t11", p.vd4, 4));
            //listBoxJtbl.Items.Add(new Jtbl("t12", p.vdc, 4));
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vb4, p.vb8 * 8, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vc0, p.vc4 * 6, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vc8, p.vcc * 6, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.ve0, p.ve4 * 12, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vac, p.va2 * 4, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.va8, (p.va2 - p.va0) * 64, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vf8, p.vfc * 12, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vf0, p.vf4 * 8, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vec, cntt8 * 48, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vd0, cnt9 * 8, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vd8, cnt10 * 4, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vd4, p.vb0 * 4, clr, clrb);
            //hexVwer1.AddRangeMarked(baseoff + tbloff + p.vdc, cnt12 * 4, clr, clrb);
        }
    }
    internal class PosTbl {
        public int tbloff = 0x90;

        public int va0;
        public int va2; // cnt t4
        public int va8; // off t5 (each 64 bytes)  { cnt_t5 = va2 -va0 }
        public int vac; // off t4 (each 4 bytes)
        public int vb0; // cnt t11
        public int vb4; // off t1 (each 8 bytes)
        public int vb8; // cnt t1
        public int vc0; // off t2 (each 6 bytes)
        public int vc4; // cnt t2
        public int vc8; // off t2` (each 6 bytes)
        public int vcc; // cnt t2`
        public int vd0; // off t9 (each 8 bytes)
        public int vd4; // off t11 (each 4 bytes)
        public int vd8; // off t10 (each 4 bytes)
        public int vdc; // off t12 (each 4 bytes)
        public int ve0; // off t3 (each 12 bytes)
        public int ve4; // cnt t3
        public int ve8;
        public int vec; // off t8 (each 48 bytes)  { cnt_t8 = cnt_t2` }
        public int vf0; // off t7 (each 8 bytes)
        public int vf4; // cnt t7
        public int vf8; // off t6 (each 12 bytes)
        public int vfc; // cnt t6

        public PosTbl(Stream si) {
            BinaryReader br = new BinaryReader(si);
            int off = tbloff - 0x90;

            // Clone¨
            si.Position = off + 0xA0;
            va0 = br.ReadUInt16();
            va2 = br.ReadUInt16(); // cnt t4
            si.Position = off + 0xA8;
            va8 = br.ReadInt32(); // off t5 (each 64 bytes)  { cnt_t5 = va2 -va0 }
            vac = br.ReadInt32(); // off t4 (each 4 bytes)

            si.Position = off + 0xB0;
            vb0 = br.ReadInt32(); // cnt t11
            vb4 = br.ReadInt32(); // off t1 (each 8 bytes)
            vb8 = br.ReadInt32(); // cnt t1
            si.Position = off + 0xC0;
            vc0 = br.ReadInt32(); // off t2 (each 6 bytes)
            vc4 = br.ReadInt32(); // cnt t2
            vc8 = br.ReadInt32(); // off t2` (each 6 bytes)
            vcc = br.ReadInt32(); // cnt t2`
            si.Position = off + 0xD0;
            vd0 = br.ReadInt32(); // off t9 (each 8 bytes)
            vd4 = br.ReadInt32(); // off t11 (each 4 bytes)
            vd8 = br.ReadInt32(); // off t10 (each 4 bytes)
            vdc = br.ReadInt32(); // off t12 (each 4 bytes)
            si.Position = off + 0xE0;
            ve0 = br.ReadInt32(); // off t3 (each 12 bytes)
            ve4 = br.ReadInt32(); // cnt t3
            ve8 = br.ReadInt32();
            vec = br.ReadInt32(); // off t8 (each 48 bytes)  { cnt_t8 = cnt_t2` }
            si.Position = off + 0xF0;
            vf0 = br.ReadInt32(); // off t7 (each 8 bytes)
            vf4 = br.ReadInt32(); // cnt t7
            vf8 = br.ReadInt32(); // off t6 (each 12 bytes)
            vfc = br.ReadInt32(); // cnt t6
            // ©Clone
        }
    }

    public class Msetfst {
        public List<Mt1> al1 = new List<Mt1>();

        public Msetfst(Stream fs, string motionId) {
            ReadBar.Barent[] ents1 = ReadBar.Explode(fs);
            int k1 = 0;
            foreach (ReadBar.Barent ent1 in ents1) {
                switch (ent1.k) {
                    case 17:
                        if (ent1.len != 0) {
                            Mt1 m1 = new Mt1();
                            ReadBar.Barent[] ents2 = ReadBar.Explode2(new MemoryStream(ent1.bin, false));
                            int k2 = 0;
                            foreach (ReadBar.Barent ent2 in ents2) {
                                switch (ent2.k) {
                                    case 9:
                                        m1.off = (uint)ent1.off + (uint)ent2.off;
                                        m1.len = (uint)ent1.len;
                                        m1.id = ent1.id + "#" + ent2.id;
                                        m1.bin = ent2.bin;
                                        m1.k1 = k1;
                                        m1.isRaw = ent2.id.Equals("raw");
                                        break;
                                    case 16:
                                        m1.fm = new FacMod(new MemoryStream(ent2.bin, false));
                                        break;
                                }
                                k2++;
                            }
                            al1.Add(m1);
                        }
                        break;
                }
                k1++;
            }
        }
    }

    public class To {
        public List<T1> al1 = new List<T1>();
        public List<T2> al2 = new List<T2>();
        public List<T2> al2x = new List<T2>();
        public List<T3> al3 = new List<T3>();
        public List<T4> al4 = new List<T4>();
        public List<AxBone> al5 = new List<AxBone>();
        public List<T9> al9 = new List<T9>();
        public float[] al10 = null;
        public float[] al11 = null;
        public float[] al12 = null;

        public int off5 = 0, cnt5 = 0;
    }
    public class T3 {
        public int c00, c01, c02, c04, c06;
        public uint c08;

        public T3(int c00, int c01, int c02, int c04, int c06, uint c08) {
            this.c00 = c00;
            this.c01 = c01;
            this.c02 = c02;
            this.c04 = c04;
            this.c06 = c06;
            this.c08 = c08;
        }

        public override string ToString() {
            return string.Format("{0:X2} {1:X2} {2:X4} {3:X4} {4:X4} {5:X8}", c00, c01, c02, c04, c06, c08);
        }
    }
    public class T4 {
        public int c00;
        public int c02;

        public T4(int c00, int c02) {
            this.c00 = c00;
            this.c02 = c02;
        }

        public override string ToString() {
            return string.Format("{0:X4} {1:X4}", c00, c02);
        }
    }
    public class T1 {
        public int c00, c02;
        public float c04;

        public T1(int c00, int c02, float c04) {
            this.c00 = c00;
            this.c02 = c02;
            this.c04 = c04;
        }

        public override string ToString() {
            return string.Format("{0:X4} {1:X4} {2}", c00, c02, c04);
        }
    }
    public class T2 {
        public int c00, c01, c02, c03, c04;
        public List<T9f> al9f = new List<T9f>();

        public T2(int c00, int c01, int c02, int c03, int c04) {
            this.c00 = c00;
            this.c01 = c01;
            this.c02 = c02;
            this.c03 = c03;
            this.c04 = c04;
        }

        public override string ToString() {
            return string.Format("{0:X2} {1:X2} {2:X2} {3:X2} {4:X4}", c00, c01, c02, c03, c04);
        }
    }
    public class T9 {
        public int c00, c02, c04, c06;

        public T9(int c00, int c02, int c04, int c06) {
            this.c00 = c00;
            this.c02 = c02;
            this.c04 = c04;
            this.c06 = c06;
        }

        public override string ToString() {
            return string.Format("{0:X4} {1:X4} {2:X4} {3:X4}", c00, c02, c04, c06);
        }
    }
    public class T9f {
        public int c00;
        public float v0, v1, v2, v3;

        public T9f(int c00, float v0, float v1, float v2, float v3) {
            this.c00 = c00;
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}
