using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace kenuno
{
    public class ReadBar {
        public class Barent {
            public int k;
            public string id;
            public int off, len;
            public byte[] bin;
        }
        public static Barent[] Explode(Stream si) {
            BinaryReader br = new BinaryReader(si);
            if (br.ReadByte() != 'B' || br.ReadByte() != 'A' || br.ReadByte() != 'R' || br.ReadByte() != 1)
                throw new NotSupportedException();
            int cx = br.ReadInt32();
            br.ReadBytes(8);
            List<Barent> al = new List<Barent>();
            for (int x = 0; x < cx; x++) {
                Barent ent = new Barent
                {
                    k = br.ReadInt32(),
                    id = Encoding.ASCII.GetString(br.ReadBytes(4)).TrimEnd((char)0),
                    off = br.ReadInt32(),
                    len = br.ReadInt32()
                };
                al.Add(ent);
            }
            for (int x = 0; x < cx; x++) {
                Barent ent = al[x];
                si.Position = ent.off;
                ent.bin = br.ReadBytes(ent.len);
                Debug.Assert(ent.bin.Length == ent.len);
            }
            return al.ToArray();
        }
        public static Barent[] Explode2(MemoryStream si) {
            BinaryReader br = new BinaryReader(si);
            if (br.ReadByte() != 'B' || br.ReadByte() != 'A' || br.ReadByte() != 'R' || br.ReadByte() != 1)
                throw new NotSupportedException();
            int cx = br.ReadInt32();
            br.ReadBytes(8);
            List<Barent> al = new List<Barent>();
            for (int x = 0; x < cx; x++) {
                Barent ent = new Barent
                {
                    k = br.ReadInt32(),
                    id = Encoding.ASCII.GetString(br.ReadBytes(4)).TrimEnd((char)0),
                    off = br.ReadInt32(),
                    len = br.ReadInt32()
                };
                if (((ent.off + ent.len) & 15) != 0) ent.len += 16 - ((ent.off + ent.len) & 15);
                al.Add(ent);
            }
            for (int x = 0; x < cx; x++) {
                Barent ent = al[x];
                si.Position = ent.off;
                ent.bin = new byte[ent.len];
                si.Read(ent.bin, 0, ent.len);
            }
            return al.ToArray();
        }
    }
}
