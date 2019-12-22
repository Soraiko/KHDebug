using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using kenuno;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace KHDebug
{
	public static class Spawn
	{
		public static void Load(string filename)
		{
			for (int i=0;i<MainGame.ResourceFiles.Count;i++)
			{
				if (!(MainGame.ResourceFiles[i] is MAP))
				{
					MainGame.ResourceFiles[i].Render = false;
				}
			}
			string[] contenu = File.ReadAllLines(filename);
			string[] spli;

			if (Program.game.MapSet && File.Exists(@"Content\Scenes\Behaviours\" + contenu[0]))
			{
				string[] text = File.ReadAllLines(@"Content\Scenes\Behaviours\" + contenu[0]);
				for (int i = 0; i < text.Length; i++)
				{
					spli = text[i].Split('|');

					if (spli[0].Length > 1 && spli[0][0] == 'I' && spli[0][1] == 'F')
					{
						Action action = new Action();
						action.parent = Program.game.Map;
						action.AddCondition(text[i]);
						while (spli[0] != "ENDIF")
						{
							i++;
							spli = text[i].Split('|');
							if (spli[0] == "ELSE" || spli[0] == "AND" || spli[0] == "OR")
								action.AddCondition(text[i]);
							else
								action.AddComand(text[i]);
						}
						Program.game.Map.Behaviours[Program.game.Map.BehavioursCount] = action;
						Program.game.Map.BehavioursCount++;
					}
				}
			}

			spli = contenu[0+2].Split(',');
			LoadCharacter(spli[0], spli[1], ref Program.game.Player);
			spli = contenu[1 + 2].Split(',');
			LoadKeyblade(spli[0], spli[1], ref Program.game.Player);

			if (Program.game.Player!=null)
			{
				spli = contenu[2 + 2].Split(',');
				Program.game.Player.Location = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]));
				Program.game.Player.SpawnedLocation = Program.game.Player.Location;
				Program.game.Player.LowestFloor = Program.game.Player.Location.Y;
				Program.game.Player.DestRotate = MainGame.SingleParse(contenu[3 + 2]);
				Program.game.Player.Rotate = Program.game.Player.DestRotate;
			}

			spli = contenu[5 + 2].Split(',');
			LoadCharacter(spli[0], spli[1], ref Program.game.Partner1);
			spli = contenu[6 + 2].Split(',');
			LoadKeyblade(spli[0], spli[1], ref Program.game.Partner1);

			if (Program.game.Partner1 != null)
			{
				spli = contenu[7 + 2].Split(',');
				Program.game.Partner1.Location = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]));
				Program.game.Partner1.SpawnedLocation = Program.game.Partner1.Location;
				Program.game.Partner1.LowestFloor = Program.game.Partner1.Location.Y;
				Program.game.Partner1.DestRotate = MainGame.SingleParse(contenu[8 + 2]);
				Program.game.Partner1.Rotate = Program.game.Partner1.DestRotate;
			}

			spli = contenu[10 + 2].Split(',');
			LoadCharacter(spli[0], spli[1], ref Program.game.Partner2);
			spli = contenu[11 + 2].Split(',');
			LoadKeyblade(spli[0], spli[1], ref Program.game.Partner2);

			if (Program.game.Partner2 != null)
			{
				spli = contenu[12 + 2].Split(',');
				Program.game.Partner2.Location = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]));
				Program.game.Partner2.SpawnedLocation = Program.game.Partner2.Location;
				Program.game.Partner2.LowestFloor = Program.game.Partner2.Location.Y;
				Program.game.Partner2.DestRotate = MainGame.SingleParse(contenu[13 + 2]);
				Program.game.Partner2.Rotate = Program.game.Partner2.DestRotate;

			}


			for (int i=17;i< contenu.Length;i+=5)
			{
				spli = contenu[i].Split(',');
				LoadCharacter(spli[0], spli[1], ref Program.game.LastLoadedNotParty);
				if (spli[2] == "NPC")
				{
					Program.game.LastLoadedNotParty.NPC = true;
				}
				spli = contenu[i+1].Split(',');
				LoadKeyblade(spli[0], spli[1], ref Program.game.LastLoadedNotParty);

				if (Program.game.LastLoadedNotParty != null)
				{
					spli = contenu[i+2].Split(',');
					Program.game.LastLoadedNotParty.Location = new Vector3(MainGame.SingleParse(spli[0]), MainGame.SingleParse(spli[1]), MainGame.SingleParse(spli[2]));
					Program.game.LastLoadedNotParty.SpawnedLocation = Program.game.LastLoadedNotParty.Location;
					Program.game.LastLoadedNotParty.LowestFloor = Program.game.LastLoadedNotParty.Location.Y;
					Program.game.LastLoadedNotParty.DestRotate = MainGame.SingleParse(contenu[i+3]);
					Program.game.LastLoadedNotParty.Rotate = Program.game.LastLoadedNotParty.DestRotate;
				}
			}


			//LoadCharacter(@"P_EX020\P_EX020.dae", @"P_EX020", ref this.Partner1);
			//LoadKeyblade(@"W_EX020\W_EX020.dae", @"W_EX020", ref this.Partner1);
			
			Program.game.Sora = Program.game.Player;
			Program.game.Donald = Program.game.Partner1;
			//Program.game.Player.Patches[0].GetPatch(2);

			Program.game.mainCamera.Target = Program.game.Player;
			Model target = Program.game.mainCamera.Target;
			if (target != null)
			{
				Program.game.mainCamera.DestLookAt = target.Location + target.HeadHeight;
				Program.game.mainCamera.LookAt = Program.game.mainCamera.DestLookAt;
				Program.game.mainCamera.DestYaw = Program.game.Player.DestRotate - MainGame.PI;
				Program.game.mainCamera.Yaw = Program.game.mainCamera.DestYaw;
			}
			else
			{
				Program.game.mainCamera.DestLookAt = new Vector3(0,250,0);
				Program.game.mainCamera.LookAt = Program.game.mainCamera.DestLookAt;
				Program.game.mainCamera.DestYaw = 0f;
				Program.game.mainCamera.Yaw = Program.game.mainCamera.DestYaw;
			}
		}


		public static void LoadCharacter(string modelName, string movsesetName, ref Model target)
		{
			Model model = null;
			if (File.Exists(@"Content\Models\" + modelName + @"\" + modelName + ".dae"))
			{
				for (int i=0;i< MainGame.ResourceFiles.Count;i++)
				{
					if (MainGame.ResourceFiles[i] is Model && MainGame.ResourceFiles[i].Name == modelName)
					{
						model = MainGame.ResourceFiles[i] as Model;
						break;
					}
				}
				if (model==null)
				{
					model = new DAE(@"Content\Models\" + modelName + @"\" + modelName + ".dae");
					model.Parse();
					MainGame.ResourceFiles.Add(model);
				}

				model.Render = true;
				target = model;
				model.Opacity = 0;
			}

			if (model != null && Directory.Exists(@"Content\Models\" + movsesetName + @"\MSET"))
			{
				BinaryMoveset mset = null;
				for (int i = 0; i < MainGame.ResourceFiles.Count; i++)
				{
					if (MainGame.ResourceFiles[i] is BinaryMoveset && MainGame.ResourceFiles[i].Name == movsesetName)
					{
						mset = MainGame.ResourceFiles[i] as BinaryMoveset;
						break;
					}
				}
				if (mset ==null)
				{
					mset = new BinaryMoveset(@"Content\Models\" + movsesetName + @"\MSET");
					mset.Links.Add(model);
					mset.Parse();
					MainGame.ResourceFiles.Add(mset);
					if (Directory.Exists(@"Content\Models\" + movsesetName + @"\P_EX100_MSET"))
					{
						var mset2 = new BinaryMoveset(@"Content\Models\" + movsesetName + @"\P_EX100_MSET");
						mset2.Links.Add(Program.game.Player);
						mset2.Links.Add(mset);
						mset2.Parse();
						mset.Links.Add(mset2);
						mset2.Render = true;
						mset2.PlayingIndex = mset2.idle_;
					}
				}
				mset.Render = true;
				mset.PlayingIndex = mset.idle_;
				if (model.Links.Count == 0)
				model.Links.Insert(0,mset);
			}
		}


		public static void LoadKeyblade(string modelName, string movesetName, ref Model target)
		{
			Model keyblade = null;
			if (File.Exists(@"Content\Models\" + modelName + @"\" + modelName + ".dae"))
			{
				keyblade = new DAE(@"Content\Models\" + modelName + @"\" + modelName + ".dae");
				keyblade.Parse();
				MainGame.ResourceFiles.Add(keyblade);
				keyblade.Render = true;
				keyblade.Opacity = 0;
				keyblade.DestOpacity = 0;

			}

			if (keyblade != null && Directory.Exists(@"Content\Models\" + movesetName + @"\MSET"))
			{
				BinaryMoveset keybladeMset = new BinaryMoveset(@"Content\Models\" + movesetName + @"\MSET");
				keybladeMset.Links.Add(keyblade);
				keybladeMset.Master = (target.Links[0] as Moveset);

				keybladeMset.Parse();
				MainGame.ResourceFiles.Add(keybladeMset);
				keybladeMset.Render = true;

				keybladeMset.PlayingIndex = keybladeMset.idle_;

				keyblade.Links.Add(keybladeMset);
				keyblade.Master = target;
				target.Keyblade = keyblade;
			}
		}
	}
}
