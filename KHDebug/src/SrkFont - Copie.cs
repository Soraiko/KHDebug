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
	public static class SrkFont
	{
		public static Texture2D Sprite;
		public static VertexPositionColorTexture[] VertexBuffer = new VertexPositionColorTexture[6*5000];

		public static List<string> chaines = new List<string>(0);
		public static List<int> positions = new List<int>(0);

		public static int[] Focused = new int[50];
		public static int FocusedIndex = 0;

		public static void WriteLine(string text, int size, Color foreColor)
		{
			if (FocusedIndex < 0)
				return;
			int index = -1;
			for (int i=0;i < chaines.Count;i++)
			{
				if (text.Length == chaines[i].Length)
				{
					bool same = true;
					for (int j = 0; j < text.Length; j++)
					{
						if (chaines[i][j] != text[j])
						{
							same = false;
							break;
						}
					}
					if (same)
					{
						index = i;
						break;
					}
				}
			}
			int whiteCount = 0;

			if (index<0)
			{
				int start = 0;
				for (int i = 0; i < VertexBuffer.Length-24; i += 12)
				{
					whiteCount+=2;
					if (VertexBuffer[i].Color.A > 0)
					{
						whiteCount = 0;
						start = i+24;
					}
					if (whiteCount>= text.Length)
					{
						chaines.Add(text);
						positions.Add(start);
						break;
					}
				}
				index = chaines.Count - 1;
				for (int i=0;i<text.Length;i++)
				{
					
					VertexBuffer[start + i * 6 + 0].Position.X = 0;
					VertexBuffer[start + i * 6 + 0].Position.Y = 0.5f;

					VertexBuffer[start + i * 6 + 1].Position.X = 1;
					VertexBuffer[start + i * 6 + 1].Position.Y = 0.5f;

					VertexBuffer[start + i * 6 + 2].Position.X = 1;
					VertexBuffer[start + i * 6 + 2].Position.Y = -0.5f;

					VertexBuffer[start + i * 6 + 3].Position.X = 0;
					VertexBuffer[start + i * 6 + 3].Position.Y = 0.5f;

					VertexBuffer[start + i * 6 + 4].Position.X = 1;
					VertexBuffer[start + i * 6 + 4].Position.Y = -0.5f;

					VertexBuffer[start + i * 6 + 5].Position.X = 0;
					VertexBuffer[start + i * 6 + 5].Position.Y = -0.5f;

					for (int j = 0; j < 6; j++)
						VertexBuffer[start + i * 6 + j].Position.X += i;

					for (int j = 0; j < 6; j++)
					{
						VertexBuffer[start + i * 6 + j].Position.X *= 100f;
						VertexBuffer[start + i * 6 + j].Position.Y *= 100f;
					}


					float x = (text[i] % 16) * 0.0625f;
					float y = (text[i] / 16) * 0.0625f;

					VertexBuffer[start + i * 6 + 0].TextureCoordinate = new Vector2(x, y);
					VertexBuffer[start + i * 6 + 1].TextureCoordinate = new Vector2(x + 0.0625f, y);
					VertexBuffer[start + i * 6 + 2].TextureCoordinate = new Vector2(x + 0.0625f, y + 0.0625f);
					VertexBuffer[start + i * 6 + 3].TextureCoordinate = new Vector2(x, y);
					VertexBuffer[start + i * 6 + 4].TextureCoordinate = new Vector2(x + 0.0625f, y + 0.0625f);
					VertexBuffer[start + i * 6 + 5].TextureCoordinate = new Vector2(x, y + 0.0625f);

					for (int j = 0; j < 6; j++)
						VertexBuffer[start + i * 6 + j].Color = foreColor;
				}
			}
			bool nott = true;
			for (int i=0;i< FocusedIndex;i++)
			{
				if (Focused[i] == index)
				{
					nott = false;
					break;
				}
			}
			if (nott)
			{
				Focused[FocusedIndex] = index;
				FocusedIndex++;
			}
		}


		public static void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
		{
			if (FocusedIndex<0)
				return;
			int localFI = FocusedIndex;
			FocusedIndex = -1;


			for (int i = 0; i < localFI; i++)
			{
				int localPos = Focused[i];
				gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
				gcm.GraphicsDevice.RasterizerState = rs;


				be.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
				be.Projection = Matrix.CreateOrthographic(1920f, 1080f, 0f, 10f);

				Console.WriteLine(chaines[localPos]);
				Console.WriteLine(positions[localPos]);
				Console.WriteLine("");
				int length = (int)(Math.Ceiling(chaines[localPos].Length / 2f) * 2);

				be.Texture = CharToScreen.Sprite;
				be.CurrentTechnique.Passes[0].Apply();
				gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBuffer, positions[localPos], length * 2);

				gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
				gcm.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

				//gcm.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
				be.View = at.View;
				be.Projection = at.Projection;
			}
			
			FocusedIndex = 0;
		}

	}
}
