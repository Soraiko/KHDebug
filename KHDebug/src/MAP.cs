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
    public class MAP:BinaryModel
	{
		public List<int> varIDs;
		public List<int> varValues;
		public SunBeam sunbeam;
		public static string[] sunbeamList;
		public float FogStart = 0f;
		public float FogEnd = 0f;
		public Microsoft.Xna.Framework.Vector3 FogColor = Color.Transparent.ToVector3();

		public MAP(string filename):base(filename)
		{
			this.Actions = new Action[5000];
			this.Behaviours = new Action[5000];
			this.Supp = new List<Model>(0);
			this.SuppMsets = new List<Moveset>(0);
			varIDs = new List<int>(0);
			varValues = new List<int>(0);

			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"))
            {
                this.Links.Add(new Collision(Path.GetDirectoryName(this.FileName) + @"\" + "collision.obj"));
            }

            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "areas.obj"))
            {
                this.Area = new Area(Path.GetDirectoryName(this.FileName) + @"\" + "areas.obj");
            }

            if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + "actions.txt"))
            {
                string[] text = File.ReadAllLines(Path.GetDirectoryName(this.FileName) + @"\" + "actions.txt");
                for (int i=0;i<text.Length;i++)
                {
                    string[] spli = text[i].Split('|');

                    if (spli[0].Length> 1 && spli[0][0] == 'I' && spli[0][1] == 'F')
                    {
                        Action action = new Action();
                        action.parent = this;
                        action.AddCondition(text[i]);
                        while (spli[0]!="ENDIF")
                        {
                            i++;
                            spli = text[i].Split('|');
                            if (spli[0] == "ELSE" || spli[0] == "AND" || spli[0] == "OR")
                                action.AddCondition(text[i]);
                            else
                                action.AddComand(text[i]);
                        }
                        this.Actions[this.ActionsCount] = action;
                        this.ActionsCount++;
                    }
                }
            }
        }

		public new void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{

			if (Action.bAccount == 0)
			{
				Action.be = this.Behaviours;
				Action.bAccount = this.BehavioursCount;
			}

			if (Action.aAccount == 0)
			{
				Action.ac = this.Actions;
				Action.aAccount = this.ActionsCount;
			}

			if (this.Supp.Count > 0)
			{
				gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
				for (int i = 0; i < this.Supp.Count; i++)
				{
					if (this.Supp[i].IsSky)
						this.Supp[i].Draw(gcm, at, be, rs, rsNoCull);
				}
				gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			}
		}


		public void Parse()
		{


			this.ResourceIndex = Array.IndexOf(Resource.ResourceIndices, this.FileName.Split('.')[0] + ".");
			this.IsSky = this.Name.Contains("SKY");


			FileStream fs = new FileStream(this.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReader reader = new BinaryReader(fs);
			this.ModelType = (MDType)reader.ReadSByte();
			this.NoCull = reader.ReadSByte() < 0;
			this.ZIndex = (int)reader.ReadSByte();
			reader.ReadSByte();
			int textureCount = reader.ReadInt32();

			if (this.ModelType == MDType.Map)
				for (int i = 0; i < 10; i++)
				{
					if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae"))
					{
						string fname = Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae";

						Model model = new DAE(Path.GetDirectoryName(this.FileName) + @"\SKY\" + this.Name + "-SKY" + i + ".dae");
						model.Parse();

						BinaryMoveset bm = null;
						if (Directory.Exists(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname)))
						{
							bm = new BinaryMoveset(Path.GetDirectoryName(fname) + "\\" + Path.GetFileNameWithoutExtension(fname));
							bm.Links.Add(model);
							bm.Parse();
							bm.PlayingIndex = 0;
							bm.Render = true;
							model.Links.Add(bm);
						}
						this.SuppMsets.Add(bm);
						this.Supp.Add(model);
					}
				}
			if (Directory.Exists(Path.GetDirectoryName(this.FileName) + @"\OBJECTS"))
			{
				string[] files = Directory.GetFiles(Path.GetDirectoryName(this.FileName) + @"\OBJECTS", "*.dae");

				for (int i = 0; i < files.Length; i++)
				{
					Model dae = new DAE(files[i])
					{
						Render = true,
						ModelType = files[i].Contains("-sp") ? MDType.Specular : MDType.Sky
					};
					dae.Parse();
					//(dae as DAE).ExportBin();

					BinaryMoveset bm = null;

					if (Directory.Exists(Path.GetDirectoryName(files[i]) + "\\" + Path.GetFileNameWithoutExtension(files[i])))
					{
						bm = new BinaryMoveset(Path.GetDirectoryName(files[i]) + "\\" + Path.GetFileNameWithoutExtension(files[i]));
						bm.Links.Add(dae);
						bm.Parse();
						bm.PlayingIndex = 0;
						bm.ObjectMsetRender = bm.Name.Contains("-mdl-");
						bm.Render = true;
						dae.Links.Add(bm);
					}

					this.SuppMsets.Add(bm);
					this.Supp.Add(dae);
				}
			}
			this.FogStart = reader.ReadInt16() * 10f;
			this.FogEnd = reader.ReadInt16() * 10f;
			this.FogColor = (new Microsoft.Xna.Framework.Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte())).ToVector3();

			reader.Close();
			fs.Close();

			if (sunbeamList == null)
			{
				sunbeamList = File.ReadAllLines("Content\\Effects\\Visual\\Sun\\sunpositions.txt");
			}
			for (int i = 0; i < sunbeamList.Length; i++)
			{
				string[] spli = sunbeamList[i].Split('|');
				if (spli[0] == this.Name)
				{
					spli = spli[1].Split(';');
					this.sunbeam = new SunBeam
					{
						From = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]))
					};
					this.sunbeam.coll = new Collision("Content\\Effects\\Visual\\Sun\\" + this.Name + ".obj");
				}
			}


			for (int i = 0; i < this.Supp.Count; i++)
				for (int j = i + 1; j < this.Supp.Count; j++)
				{
					if (this.Supp[j].ZIndex < this.Supp[i].ZIndex)
					{
						var curr = this.Supp[j];
						this.Supp.RemoveAt(j);
						this.Supp.Insert(0, curr);
						var currM = this.SuppMsets[j];
						this.SuppMsets.RemoveAt(j);
						this.SuppMsets.Insert(0, currM);
					}
				}


			base.Parse();
		}

		public void DrawEfects(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
		{
			if (this.sunbeam != null)
			{
				this.sunbeam.To = Program.game.mainCamera.Position;
				this.sunbeam.RecreateVertexBuffer();
				this.sunbeam.Draw(gcm, be, rs);
			}
		}

		public List<Model> Supp;
		public List<Moveset> SuppMsets;
		public Action[] Actions;
		public int ActionsCount;

		public Action[] Behaviours;
		public int BehavioursCount;

		public void UpdateObjects()
		{
			for (int j = 0; j < this.Supp.Count; j++)
			{
				if (this.SuppMsets[j] != null)
				{
					if (this.Supp[j].IsSky || this.SuppMsets[j].ObjectMsetRender)
					{
						(this.SuppMsets[j] as Moveset).ComputeAnimation();
					}
				}
			}
		}

		public void DrawObjects(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			for (int j = 0; j < this.Supp.Count; j++)
			{
				if (!this.Supp[j].IsSky)
				{
					this.Supp[j].Draw(gcm, at, be, rs, rsNoCull);
				}
			}
		}
	}
}
