using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public unsafe struct Area
    {
        private List<string> Names;
        private List<Vector3[]> Bases;
        private List<Vector3> Heights;
        private List<Vector3> verts;

        public unsafe bool IsInside(Vector3 pos, char* areaName)
        {
            bool inside = false;
            for (int i=0;i<this.Names.Count;i++)
            {
                bool sameName = areaName[this.Names[i].Length] == 0;
                for (int j=0; sameName && j < this.Names[i].Length; j++)
                    if (areaName[j] != this.Names[i][j])
                        sameName = false;
                if (!sameName)
                    continue;

                inside = Collision.Inside(pos, 
                    this.Bases[i][0], 
                    this.Bases[i][1], 
                    this.Bases[i][2], 
                    this.Bases[i][0] + this.Heights[i], 
                    this.Bases[i][1] + this.Heights[i],
                    this.Bases[i][2] + this.Heights[i]);
                /*Console.Clear();
                Console.WriteLine(this.Bases[i][0]);
                Console.WriteLine(this.Bases[i][1]);
                Console.WriteLine(this.Bases[i][2]);
                Console.WriteLine(this.Bases[i][3]);*/
                if (!inside)
                {
                    inside = Collision.Inside(pos,
                        this.Bases[i][0],
                        this.Bases[i][2],
                        this.Bases[i][3],
                        this.Bases[i][0] + this.Heights[i],
                        this.Bases[i][2] + this.Heights[i],
                        this.Bases[i][3] + this.Heights[i]);
                }
                break;
            }
            return inside;
        }

        public string Name;
        public string FileName;
        public Resource.ResourceType Type;
        public List<Resource> Links;
        public int Set;

        public Area(string filename)
        {
            this.Set = 0x584976;
            this.verts = new List<Vector3>(0);
            this.Names = new List<string>(0);
            this.Bases = new List<Vector3[]>(0);
            this.Heights = new List<Vector3>(0);
            this.Links = new List<Resource>(0);

            this.Type = Resource.ResourceType.Collision;
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;

            string[] obj = File.ReadAllLines(filename);
            string[] spli = new string[0];
            Vector3[] plane1 = new Vector3[4];
            Vector3[] plane2 = new Vector3[4];
            plane1[0].X = Single.NaN;
            plane2[0].X = Single.NaN;

            for (int i = 0; i < obj.Length; i++)
            {
                spli = obj[i].Split(' ');
                if (spli[0] == "v")
                {
                    this.verts.Add(new Vector3(MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]), MainGame.SingleParse(spli[3])));
                }

                if (spli[0] == "g")
                {
                    this.Names.Add(spli[1]);
                }
                if (spli[0] == "f")
                {
                    Vector3 v1 = this.verts[int.Parse(spli[1]) - 1];
                    Vector3 v2 = this.verts[int.Parse(spli[2]) - 1];
                    Vector3 v3 = this.verts[int.Parse(spli[3]) - 1];
                    Vector3 v4 = this.verts[int.Parse(spli[4]) - 1];

                    if (Math.Abs(((v1.Y+ v2.Y+ v3.Y+ v4.Y)/4f)-v1.Y)<1)
                    {
                        if (Single.IsNaN(plane1[0].X))
                        {
                            plane1[0] = v1;
                            plane1[1] = v2;
                            plane1[2] = v3;
                            plane1[3] = v4;
                        }
                        else if (Single.IsNaN(plane2[0].X))
                        {
                            plane2[0] = v1;
                            plane2[1] = v2;
                            plane2[2] = v3;
                            plane2[3] = v4;

                            if (plane2[0].Y>plane1[0].Y)
                            {
                                this.Bases.Add(plane1);
                                this.Heights.Add(new Vector3(0,plane2[0].Y- plane1[0].Y,0));
                            }
                            else
                            {
                                this.Bases.Add(plane2);
                                this.Heights.Add(new Vector3(0, plane1[0].Y - plane2[0].Y, 0));
                            }
                            plane1 = new Vector3[4];
                            plane2 = new Vector3[4];
                            plane1[0].X = Single.NaN;
                            plane2[0].X = Single.NaN;
                        }
                    }
                }
            }
        }
    }
}
