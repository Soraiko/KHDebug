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
	public static class BulleSpeecher
	{
		public static List<Bulle> bulles = new List<Bulle>(0);
		public static List<Model> bulleEmmiters = new List<Model>(0);

		public static Bulle CurrentBulle = null;
		public static Vector3 NoEmmiter = Vector3.Zero;

		public static void ShowBubble(string text, int textIndex, Bulle.BulleColor color, Bulle.BulleType type, Model emmiter)
		{
			Bulle bulle = null;
			for (int i=0;i< bulles.Count;i++)
			{
				if (bulles[i].TextFileName == text)
				{
					bulle = bulles[i];
					break;
				}
			}
			if (bulle == null)
			{
				bulle = new Bulle(text, textIndex);
				bulles.Add(bulle);
				bulleEmmiters.Add(emmiter);
			}
			else
			{
				bulle.SetText(text, textIndex);
				bulle.NextText();
			}

			bulle.Color = color;
			bulle.Type = type;

			if (bulle.Type == Bulle.BulleType.Speech)
			{
				bulle.BranchPosition = 0.1f;
				bulle.Position.X = -0.1f;
				bulle.Position.Y = -0.3f;
			}
			if (bulle.Type == Bulle.BulleType.Information)
			{
				bulle.Position.X = 0f;
				bulle.Position.Y = 0.9f;
			}

			if (bulle.displayType == Bulle.DisplayType.None || bulle.displayType == Bulle.DisplayType.Disapear)
			{
				bulle.displayType = Bulle.DisplayType.None;
				bulle.displayType = Bulle.DisplayType.Appear;
			}
		}
		public static void UpdateBubbles()
		{
			for (int i = 0; i < bulles.Count; i++)
				bulles[i].Update();
		}

		public static void DrawBubbles(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			for (int i=0;i<bulles.Count;i++)
				bulles[i].Draw(gcm, at, be, rs, rsNoCull);
		}
	}

	public class Bulle
	{
		public static DAE bulle;
		public string TextFileName = "";

		float PlayingPath = 1f;

		DisplayType displayType_ = DisplayType.None;

		public DisplayType displayType
		{
			get
			{
				return this.displayType_;
			}
			set
			{
				SrkFontDisplayer.Speech.Color.A = 255;
				if (value == DisplayType.Appear || value == DisplayType.None)
					SrkFontDisplayer.Speech.Color.A = 1;

				if (this.displayType_ == DisplayType.WaitPrompt && value == DisplayType.Appear)
				{
					return;
				}

				if (value == DisplayType.None)
				{
					bulle.Skeleton.Bones[1].GlobalMatrix = Matrix.CreateScale(0f);
					bulle.Skeleton.Bones[2].GlobalMatrix = Matrix.CreateScale(0f);
					bulle.Skeleton.Bones[3].GlobalMatrix = Matrix.CreateScale(0f);
					bulle.Skeleton.Bones[4].GlobalMatrix = Matrix.CreateScale(0f);
					bulle.Skeleton.Bones[5].GlobalMatrix = Matrix.CreateScale(0f);
					bulle.Skeleton.Bones[6].GlobalMatrix = Matrix.CreateScale(0f);

					for (int i = 0; i < bulle.VertexBufferColor.Length; i++)
						bulle.VertexBufferColor[i].Color.A = 0;

					xScale = 0;
					bulle.Opacity = 0;
					this.PlayingPath = 1f;
					RecreateVertexBuffer();
				}
				if (this.displayType_ == DisplayType.WaitPrompt)
				{
					if (value == DisplayType.Disapear)
					{

						bulle.Skeleton.Bones[5].GlobalMatrix = Matrix.CreateScale(0f);
						bulle.Skeleton.Bones[6].GlobalMatrix = Matrix.CreateScale(0f);

						for (int i = bulle.VertexBufferColor.Length - 6; i < bulle.VertexBufferColor.Length; i++)
							bulle.VertexBufferColor[i].Color.A = 0;
					}
				}
				if (value == DisplayType.WaitPrompt)
				{
					this.PlayingPath = 100f;
				}

				this.displayType_ = value;
			}
		}

		public enum DisplayType
		{
			Appear = 0,
			WaitPrompt = 1,
			Disapear = 2,
			None = 3
		}

		float width = 262f;
		float height = 131f;

		

		public float Width
		{
			get
			{
				return this.width;
			}
			set
			{
				if (value < 262f)
					value = 262f;
				this.width = value;
			}
		}
		public float Height
		{
			get
			{
				return this.height;
			}
			set
			{
				if (value < 131f)
					value = 131f;
				this.height = value;
			}
		}
		public static Vector3 ShadowOffset = new Vector3(-2.5f, -0.333f, -2.5f);
		int color = 0;

		public BulleColor Color
		{
			get
			{
				return (BulleColor)color;
			}
			set
			{
				Vector2 diff = new Vector2(0.25f * ((int)value - (int)color), 0);
				for (int i = 0; i < bulle.VertexBufferColor.Length; i++)
				{
					bulle.VertexBufferColor[i].TextureCoordinate += diff;
				}
				RecreateVertexBuffer();
				this.color = (int)value;
			}
		}

		public enum BulleColor
		{
			Yellow = 0,
			Green = 1,
			Red = 2,
			Blue = 3
		}

		int type = 0;

		public BulleType Type
		{
			get
			{
				return (BulleType)type;
			}
			set
			{
				if ((int)value != type)
				{
					Vector2 off = new Vector2(0, 0.5f);
					if (type == 1)
						off.Y = -0.5f;

					for (int i = 0; i < bulle.VertexBufferColor.Length; i++)
					{
						bulle.VertexBufferColor[i].TextureCoordinate += off;
					}
					RecreateVertexBuffer();


					this.type = (int)value;
				}

			}
		}

		public enum BulleType
		{
			Speech = 0,
			Information = 1
		}

		public VertexBuffer vBuffer;
		public IndexBuffer iBuffer;

		string[] Texts = new string[] {""};
		string Text = "";
		int TextIndex = -1;

		public void SetText(string textFilename, int textIndex)
		{
			Texts = File.ReadAllLines(@"Content\Scenes\Dialogues\"+textFilename+".txt")[textIndex].Split('|');
			this.TextIndex = -1;
		}

		public void NextText()
		{
			this.TextIndex++;
			if (this.TextIndex> this.Texts.Length-1)
			{
				this.Text = "";
				MainGame.TState = MainGame.TransitionState.NotTransiting;
				BulleSpeecher.CurrentBulle = null;
				if (Type == BulleType.Speech)
				{
					Program.game.mainCamera.DestPitch = -0.21f;
					Program.game.mainCamera.Pitch = -0.21f;
				}
				this.displayType = DisplayType.Disapear;
				if (Program.game.MapSet)
				{
					int ind = Program.game.Map.varIDs.IndexOf(0);
					if (ind > -1)
						Program.game.Map.varValues[ind] = 0;
				}
				Audio.Play(@"Content\Effects\Audio\Sounds\0x08\0018.wav", false, null, 50);
			}
			else
			{
				BulleSpeecher.CurrentBulle = this;
				this.Text = this.Texts[this.TextIndex];
				//DebugMenu.WriteLine(this.Text);
				if (this.TextIndex>0)
				{
					if (this.displayType == DisplayType.Appear)
					{
						PlayingPath = 100f;
					}
					Audio.Play(@"Content\Effects\Audio\Sounds\0x08\0018.wav", false, null, 50);
				}
				else
				{
					//MainGame.TState = MainGame.TransitionState.PreparingToTransit;
				}
				xScale = 0;
			}
		}

		public Bulle(string textFilename, int textIndex)
        {
			if (bulle==null)
			{
				bulle = new DAE("Content\\Effects\\Visual\\Bulle\\bulle.dae");
				bulle.Parse();
				VertexPositionColorTexture[] replace = new VertexPositionColorTexture[bulle.VertexBufferColor.Length*2];
				Array.Copy(bulle.VertexBufferColor, 0, replace, 0, bulle.VertexBufferColor.Length);
				Array.Copy(bulle.VertexBufferColor, 0, replace, bulle.VertexBufferColor.Length, bulle.VertexBufferColor.Length);
				for (int i = 0; i < bulle.VertexBufferColor.Length; i++)
				{
					replace[i].TextureCoordinate.Y += 0.25f;
				}
				
				/*bulle.VertexBufferColor = new VertexPositionColorTexture[bulle.VertexBufferColor.Length * 2];
				Array.Copy(replace, 0, bulle.VertexBufferColor, 0, bulle.VertexBufferColor.Length);*/
				bulle.VertexBufferColor = replace;
				bulle.MeshesOffsets[0][1] *= 2;
				bulle.vBuffer = new VertexBuffer(Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), bulle.VertexBufferColor.Length, BufferUsage.None);
			}
			this.TextFileName = textFilename;
			this.SetText(textFilename, textIndex);
			this.NextText();
			/*this.Color = BulleColor.Red;
			this.Type= BulleType.Information;*/


			/*this.Width = SpeechWidth;
			this.Height = SpeechHeight;*/
			this.displayType = DisplayType.None;
		}

		public Vector2 Position = new Vector2(0,0f);
		public float BranchPosition = 0.5f;
		float xScale = 0;

		public static float SpeechWidth = 1000f;
		public static float SpeechHeight = 100f;

		public static float InformationWidth = 2000f;
		public static float InformationHeight = 100f;

		public void Update()
		{
			if (this.Type == Bulle.BulleType.Speech)
			{
				this.Width = SpeechWidth;
				this.Height = SpeechHeight;
			}
			if (this.Type == Bulle.BulleType.Information)
			{
				this.Width = InformationWidth;
				this.Height = InformationHeight;
			}

			SrkFontDisplayer.Write(SrkFontDisplayer.Speech, this.Text, 0.5f, (Scale.X * (this.Width / 3.5f)), SrkFontDisplayer.Speech.Color);
			float new_ = (((SrkFontDisplayer.MaxBounds.X - SrkFontDisplayer.MinBounds.X) / Scale.X) / 1.1f) * 3.5f;
			if (new_ < this.Width)
				this.Width = new_;

			Vector2 off = new Vector2(-bulle.Location.X + (SrkFontDisplayer.MaxBounds.X - SrkFontDisplayer.MinBounds.X) / 2f,
				-bulle.Location.Z + (SrkFontDisplayer.MaxBounds.Y - SrkFontDisplayer.MinBounds.Y) / 2f + 3.5f);
			SrkFontDisplayer.ApplyOffsets(off);
			this.Height -= SrkFontDisplayer.yOffset * 4f;
			



			float branchOffset = (Scale.X * this.Width * 0.28f) * (this.BranchPosition - 0.5f);

			bulle.Location = new Vector3(Position.X * (960f * Scale.X) - (branchOffset),
				0f, -Position.Y* (540 * Scale.Z) + (Scale.Z * this.Height * 0.190839f) + (Scale.Z * 50f));



			switch (this.displayType)
			{
				case DisplayType.Appear:
					PlayingPath *= 1.15f;

					if (PlayingPath >= 100f)
					{
						PlayingPath = 100f;
						this.displayType = DisplayType.WaitPrompt;
					}

					if (PlayingPath > 25f)
					{
						bulle.Opacity = (this.PlayingPath - 25f) / 75f;
					}
					bulle.Skeleton.Bones[2].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale*new Vector3((this.PlayingPath / 100f) * this.Width * 0.190839f, 0, -(this.PlayingPath / 100f) * this.Height * 0.190839f));
					bulle.Skeleton.Bones[4].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * this.Height * 0.190839f));

					bulle.Skeleton.Bones[1].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * -this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * -this.Height * 0.190839f));
					bulle.Skeleton.Bones[3].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * -this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * this.Height * 0.190839f));

					xScale = 0f;
				break;



				case DisplayType.WaitPrompt:
					if (xScale<1f)
					xScale += 0.2f;
					bulle.Skeleton.Bones[2].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3(this.Width * 0.190839f, 0, -this.Height * 0.190839f));
					bulle.Skeleton.Bones[4].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3(this.Width * 0.190839f, 0, this.Height * 0.190839f));

					bulle.Skeleton.Bones[1].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3(-this.Width * 0.190839f, 0, -this.Height * 0.190839f));
					bulle.Skeleton.Bones[3].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3(-this.Width * 0.190839f, 0, this.Height * 0.190839f));

					bulle.Skeleton.Bones[5].GlobalMatrix = ScaleMatrix * Matrix.CreateScale(xScale, 1,1) * Matrix.CreateTranslation(Scale * new Vector3(0, 0, -this.Height * 0.190839f));
					bulle.Skeleton.Bones[6].GlobalMatrix = ScaleMatrix * Matrix.CreateScale(xScale, 1, 1) * Matrix.CreateTranslation(Scale * new Vector3(this.Width * 0.190839f - 20f, 0, 4f * (float)Math.Sin((DateTime.Now.Millisecond / 500f) * MainGame.PI) + this.Height * 0.190839f));

				break;


				case DisplayType.Disapear:
					if (xScale > 1f)
						xScale *= 0.95f;
					PlayingPath /= 1.13f;

					if (PlayingPath > 25f)
						bulle.Opacity = (this.PlayingPath - 25f) / 75f;
					else
						bulle.Opacity = 0;

					bulle.Skeleton.Bones[2].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * this.Width * 0.190839f, 0, -(this.PlayingPath / 100f) * this.Height * 0.190839f));
					bulle.Skeleton.Bones[4].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * this.Height * 0.190839f));

					bulle.Skeleton.Bones[1].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * -this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * -this.Height * 0.190839f));
					bulle.Skeleton.Bones[3].GlobalMatrix = ScaleMatrix * Matrix.CreateTranslation(Scale * new Vector3((this.PlayingPath / 100f) * -this.Width * 0.190839f, 0, (this.PlayingPath / 100f) * this.Height * 0.190839f));

					bulle.Skeleton.Bones[6].GlobalMatrix = ScaleMatrix * Matrix.CreateScale(xScale, 1, 1) * Matrix.CreateTranslation(Scale * new Vector3(this.Width * 0.190839f - 20f, 0, 4f * (float)Math.Sin((DateTime.Now.Millisecond / 500f) * MainGame.PI) + this.Height * 0.190839f));



				if (PlayingPath <= 1f)
				{
					this.displayType = DisplayType.None;
				}
				break;
			}


			/*if (PlayingPath >= 100f)
				return;*/

			RecreateVertexBuffer();
		}

		public unsafe void RecreateVertexBuffer()
		{
			Vector3 v3 = Vector3.Zero;
			Vector3 ComputingBuffer = Vector3.Zero;
			int jo4Ind = 0;

			byte opacite = (byte)(255 * bulle.Opacity);


			for (int i = 0; i < bulle.VertexBuffer_c.Length; i++)
			{
				jo4Ind = 0;
				v3 = Vector3.Zero;
				for (int j = 0; j < bulle.VertexBuffer_c[i].Count; j += 4)
				{
					ComputingBuffer.X = bulle.VertexBuffer_c[i].Vertices[j];
					ComputingBuffer.Y = bulle.VertexBuffer_c[i].Vertices[j + 1];
					ComputingBuffer.Z = bulle.VertexBuffer_c[i].Vertices[j + 2];

					Matrix mat = bulle.Skeleton.Bones[bulle.VertexBuffer_c[i].Matis[jo4Ind]].GlobalMatrix;

					ComputingBuffer = Vector3.Transform(ComputingBuffer, mat);

					v3 += ComputingBuffer * bulle.VertexBuffer_c[i].Vertices[j + 3];
					jo4Ind++;
				}

				bulle.VertexBufferColor[i].Color.A = opacite;
				bulle.VertexBufferColor[i].Position = bulle.Location + v3;

				bulle.VertexBufferColor[bulle.VertexBuffer_c.Length+i].Color.A = opacite;
				bulle.VertexBufferColor[bulle.VertexBuffer_c.Length + i].Position = bulle.Location + v3 + ShadowOffset;
			}

			float branchOffset = -Scale.X * this.Width * 0.14f + (Scale.X * this.Width * 0.28f) * this.BranchPosition;

			bulle.VertexBufferColor[120].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[121].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[122].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[123].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[124].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[125].Position += new Vector3(branchOffset, 0, 0);


			bulle.VertexBufferColor[54].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[55].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[56].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[57].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[58].Position += new Vector3(branchOffset, 0, 0);
			bulle.VertexBufferColor[59].Position += new Vector3(branchOffset, 0, 0);
		}

		public static Vector3 Scale = new Vector3(1f,1f,1f);
		public static Matrix ScaleMatrix = Matrix.CreateScale(1f);
		
		public void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			if (this.displayType == DisplayType.None)
				return;
			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;

			be.View = PAX.ViewMatrixFullScreen;
			be.Projection = PAX.ProjectionMatrixFullScreen;
			bulle.vBuffer.SetData<VertexPositionColorTexture>(bulle.VertexBufferColor);
			gcm.GraphicsDevice.SetVertexBuffer(bulle.vBuffer);
			be.DiffuseColor = Model.DefaultDiffuseColor;

			for (int i = 0; i < bulle.MeshesOffsets.Count; i++)
			{
				be.Texture = bulle.Textures[bulle.MaterialIndices[i]];
				be.CurrentTechnique.Passes[0].Apply();
				gcm.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, bulle.MeshesOffsets[i][0], bulle.MeshesOffsets[i][1] / 3);
			}

			be.View = at.View;
			be.Projection = at.Projection;

		}
    }
}
