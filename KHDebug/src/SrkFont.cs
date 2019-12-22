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

	public class SrkFont
	{
		public Texture2D Texture;
		public float[] Spacing;

		public int Width = 30;
		public int Height = 30;

		public float U = 1f;
		public float V = 1f;

		public int StrideW = 30;
		public int StrideH = 30;
		public Color Color;

		public SrkFont(string fontName, Color color)
		{
			this.Color = color;
			FileStream fs = new FileStream(@"Content\Fonts\"+ fontName+@"\texture.png", FileMode.Open);
			Texture = Texture2D.FromStream(Program.game.graphics.GraphicsDevice, fs);
			fs.Close();
			byte[] input = File.ReadAllBytes(@"Content\Fonts\" + fontName + @"\fontSpacing.bin");
			this.Width = (int)BitConverter.ToUInt16(input, 0);
			this.Height = (int)BitConverter.ToUInt16(input, 2);
			this.U = this.Width / (float)Texture.Width;
			this.V = this.Height / (float)Texture.Height;
			
			this.StrideW = this.Texture.Width / this.Width;
			this.StrideH = this.Texture.Height / this.Height;
			this.Spacing = new float[input.Length - 4];
			for (int i = 0; i < input.Length-4; i++)
			{
				this.Spacing[i] = input[4 + i] / (float)this.Width;
			}
		}
	}

	public static class SrkFontDisplayer
	{
		public static SrkFont Speech;

		public static void InitFonts()
		{
			Speech = new SrkFont("Speech", new Color(84, 34, 20));
		}

		public static void Clear()
		{
			Offsets.Clear();
			for (int j = 0; j < Length+24; j++)
				VertexBuffer[j].Color.A = 0;
			SrkFontDisplayer.WordCount = 0;
			SrkFontDisplayer.Length = 0;
		}

		public static VertexPositionColorTexture[] VertexBuffer = new VertexPositionColorTexture[6*1000];


		public static int Length = 0;
		public static int WordCount = 0;
		public static SrkFont currentFont = Speech;

		public static float totalWidth = 0;
		public static float yOffset = 0;
		

		public static Vector2 MinBounds = Vector2.Zero;
		public static Vector2 MaxBounds = Vector2.Zero;


		public static void Write(SrkFont font, string text, float scale, float limit, Color foreColor)
		{
			if (text.Length == 0)
				return;
			currentFont = font;

			currentFont = font;
			yOffset = 0;
			totalWidth = 0;

			MinBounds.X = Single.MaxValue;
			MinBounds.Y = Single.MaxValue;

			MaxBounds.X = Single.MinValue;
			MaxBounds.Y = Single.MinValue;

			string[] spli = text.Split(' ');
			for (int i = 0; i < spli.Length; i++)
			{
				string txt = spli[i];

				if (i < spli.Length - 1)
					txt += " ";
				WriteLine(txt, scale, foreColor);



				if (!Single.IsNaN(limit) && totalWidth > limit)
				{
					totalWidth = 0;
					if (i < spli.Length - 1)
						yOffset -= scale * currentFont.Height * Bulle.Scale.X * 0.4f;
				}
			}
			WordCount++;
			Length += 6;
		}

		public static void ApplyOffsets(Vector2 off)
		{
			Offsets.Add(off);
		}

		public static List<Vector2> Offsets = new List<Vector2>(0);

		public static void WriteLine(string text, float scale, Color foreColor)
		{
			float fontSpacing = 0;


			Vector3 posLeft = VertexBuffer[Length].Position;

			for (int i=0;i<text.Length;i++)
			{
					
				VertexBuffer[Length + i * 6 + 0].Position.X = 0;
				VertexBuffer[Length + i * 6 + 0].Position.Z = 0f;

				VertexBuffer[Length + i * 6 + 1].Position.X = 1;
				VertexBuffer[Length + i * 6 + 1].Position.Z = 0f;

				VertexBuffer[Length + i * 6 + 2].Position.X = 1;
				VertexBuffer[Length + i * 6 + 2].Position.Z = 1f;

				VertexBuffer[Length + i * 6 + 3].Position.X = 0;
				VertexBuffer[Length + i * 6 + 3].Position.Z = 0f;

				VertexBuffer[Length + i * 6 + 4].Position.X = 1;
				VertexBuffer[Length + i * 6 + 4].Position.Z = 1f;

				VertexBuffer[Length + i * 6 + 5].Position.X = 0;
				VertexBuffer[Length + i * 6 + 5].Position.Z = 1f;



				for (int j = 0; j < 6; j++)
					VertexBuffer[Length + i * 6 + j].Position.X += fontSpacing;

				fontSpacing += currentFont.Spacing[(byte)text[i]]*0.8f;

				for (int j = 0; j < 6; j++)
				{
					VertexBuffer[Length + i * 6 + j].Position.X *= scale * currentFont.Width * Bulle.Scale.X * 0.5f;
					VertexBuffer[Length + i * 6 + j].Position.Z *= scale * currentFont.Height * Bulle.Scale.X * 0.5f;
					
				}

				for (int j = 0; j < 6; j++)
				{
					VertexBuffer[Length + i * 6 + j].Position.X += totalWidth;
					VertexBuffer[Length + i * 6 + j].Position.Z -= yOffset;
				}

				float x = (text[i] % currentFont.StrideW) * currentFont.U;
				float y = (text[i] / currentFont.StrideW) * currentFont.V;

				VertexBuffer[Length + i * 6 + 0].TextureCoordinate = new Vector2(x, y);
				VertexBuffer[Length + i * 6 + 1].TextureCoordinate = new Vector2(x + currentFont.U, y);
				VertexBuffer[Length + i * 6 + 2].TextureCoordinate = new Vector2(x + currentFont.U, y + currentFont.V);
				VertexBuffer[Length + i * 6 + 3].TextureCoordinate = new Vector2(x, y);
				VertexBuffer[Length + i * 6 + 4].TextureCoordinate = new Vector2(x + currentFont.U, y + currentFont.V);
				VertexBuffer[Length + i * 6 + 5].TextureCoordinate = new Vector2(x, y + currentFont.V);

				for (int j = 0; j < 6; j++)
					VertexBuffer[Length + i * 6 + j].Color = foreColor;
			}

			Vector3 posRight = VertexBuffer[Length + (text.Length - 1) * 6 + 4].Position;
			totalWidth = VertexBuffer[Length + (text.Length-1) * 6 + 4].Position.X - scale * currentFont.Width * Bulle.Scale.X * 0.25f;

			if (posRight.X > MaxBounds.X) MaxBounds.X = posRight.X;
			if (posRight.Z > MaxBounds.Y) MaxBounds.Y = posRight.Z;
			if (posLeft.X > MaxBounds.X) MaxBounds.X = posLeft.X;
			if (posLeft.Z > MaxBounds.Y) MaxBounds.Y = posLeft.Z;

			if (posRight.X < MinBounds.X) MinBounds.X = posRight.X;
			if (posRight.Z < MinBounds.Y) MinBounds.Y = posRight.Z;
			if (posLeft.X < MinBounds.X) MinBounds.X = posLeft.X;
			if (posLeft.Z < MinBounds.Y) MinBounds.Y = posLeft.Z;


			Length += text.Length * 6;
		}


		public static void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			if (WordCount == 0)
				return;
			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			gcm.GraphicsDevice.RasterizerState = rsNoCull;
			be.Projection = PAX.ProjectionMatrixFullScreen;


			int pos = 0;

			for (int i = 0; i < WordCount; i++)
			{
				
				be.View = Matrix.CreateLookAt(
					new Vector3(Offsets[i].X, 1, Offsets[i].Y), 
					new Vector3(Offsets[i].X, 0, Offsets[i].Y), Vector3.Forward);

				int j = 0;
				while (j < Length && VertexBuffer[j].Color.A > 0)
					j += 6;

				be.Texture = currentFont.Texture;
				be.CurrentTechnique.Passes[0].Apply();
				gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBuffer, pos, j/3);
				pos += j;
			}

			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			gcm.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

			//gcm.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
			be.View = at.View;
			be.Projection = at.Projection;

		}

	}
}
