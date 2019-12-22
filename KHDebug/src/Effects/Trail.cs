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
	public class Trail
	{
		public Texture2D T2D;
		public VertexPositionColorTexture[] vpct;

		
		public Trail(string texture)
		{
			this.T2D = ResourceLoader.GetT2D(@"Content\Effects\Visual\KeybladeTrails\"+texture+@"\texture.png");
			vpct = new VertexPositionColorTexture[6 * 1000];
			positionsCount = 0;
			for (int i = 0; i < vpct.Length; i += 6)
			{
				vpct[i + 0].Color = Color.White;
				vpct[i + 1].Color = Color.White;
				vpct[i + 2].Color = Color.White;
				vpct[i + 3].Color = Color.White;
				vpct[i + 4].Color = Color.White;
				vpct[i + 5].Color = Color.White;
			}
		}
		Vector3 old_haut;
		Vector3 old_bas;
		Vector3 old_point;

		int positionsCount;

		public void Monitor(Model mdl, float height, int limit, bool calculated_path, Vector3 point, Vector3 haut, Vector3 bas)
		{
			if (mdl == null || mdl.Keyblade == null)
				return;

			if (!calculated_path)
			{
				point = Vector3.Transform(new Vector3(-height / 2f,0, 0), mdl.Keyblade.Skeleton.Bones[mdl.Keyblade.Skeleton.RootBone+2].GlobalMatrix * mdl.Skeleton.Bones[mdl.Skeleton.LeftHandBone].GlobalMatrix);
				point = mdl.Location + Vector3.Transform(point, Matrix.CreateRotationY(mdl.Rotate));

				haut = Vector3.Transform(new Vector3(-height,0, 0), mdl.Keyblade.Skeleton.Bones[mdl.Keyblade.Skeleton.RootBone + 2].GlobalMatrix * mdl.Skeleton.Bones[mdl.Skeleton.LeftHandBone].GlobalMatrix);
				haut = mdl.Location + Vector3.Transform(haut, Matrix.CreateRotationY(mdl.Rotate));

				bas = Vector3.Transform(new Vector3(0, 0, 0), mdl.Keyblade.Skeleton.Bones[mdl.Keyblade.Skeleton.RootBone + 2].GlobalMatrix * mdl.Skeleton.Bones[mdl.Skeleton.LeftHandBone].GlobalMatrix);
				bas = mdl.Location + Vector3.Transform(bas, Matrix.CreateRotationY(mdl.Rotate));

				if (positionsCount > 0)
				{
					float dist = Vector3.Distance(old_point, point);
					if (dist > height / 10f)
					{
						float count = 1;
						do
						{
							Vector3 point_ = old_point + ((point - old_point)/ Vector3.Distance(point , old_point))* count*(height / 10f);
							Vector3 haut_ = old_haut + ((haut - old_haut) / Vector3.Distance(haut, old_haut)) * count * (height / 10f);
							Vector3 bas_ = old_bas + ((bas - old_bas) / Vector3.Distance(bas, old_bas)) * count * (height / 10f);

							if (Vector3.Distance(old_bas, bas_) > Vector3.Distance(old_bas, bas))
								bas_ = bas;

							if (Vector3.Distance(old_haut, haut_) > Vector3.Distance(old_haut, haut))
								haut_ = haut;

							count++;
							Monitor(mdl, height, limit, true, point_, haut_, bas_);

							dist -= height / 10f;
						}
						while (dist > height / 10f);
						
					}
				}
				old_point = point;
				old_haut = haut;
				old_bas = bas;
			}


			if (positionsCount == limit)
			{
				for (int i = 0; i < positionsCount - 1; i++)
				{
					vpct[i * 6 + 0].Position = vpct[(i + 1) * 6 + 0].Position;
					vpct[i * 6 + 2].Position = vpct[(i + 1) * 6 + 2].Position;
					vpct[i * 6 + 3].Position = vpct[(i + 1) * 6 + 3].Position;

					vpct[i * 6 + 0].Color = vpct[(i + 1) * 6 + 0].Color;
					vpct[i * 6 + 2].Color = vpct[(i + 1) * 6 + 2].Color;
					vpct[i * 6 + 3].Color = vpct[(i + 1) * 6 + 3].Color;

					vpct[i * 6 + 0].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 0);
					vpct[i * 6 + 2].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 1);
					vpct[i * 6 + 3].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 0);

					if (i > 0)
					{
						vpct[(i - 1) * 6 + 1].Position = vpct[(i) * 6 + 1].Position;
						vpct[(i - 1) * 6 + 4].Position = vpct[(i) * 6 + 4].Position;
						vpct[(i - 1) * 6 + 5].Position = vpct[(i) * 6 + 5].Position;

						vpct[(i - 1) * 6 + 1].Color = vpct[(i) * 6 + 1].Color;
						vpct[(i - 1) * 6 + 4].Color = vpct[(i) * 6 + 4].Color;
						vpct[(i - 1) * 6 + 5].Color = vpct[(i) * 6 + 5].Color;

						vpct[(i - 1) * 6 + 1].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 1);
						vpct[(i - 1) * 6 + 4].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 0);
						vpct[(i - 1) * 6 + 5].TextureCoordinate = new Vector2((i + 0) * (1 / (float)limit), 1);
					}
				}
			}
			if (positionsCount == 0 || calculated_path)
			{

				if (positionsCount > 0)
				{
					vpct[(positionsCount - 1) * 6 + 1].Position = bas;
					vpct[(positionsCount - 1) * 6 + 4].Position = haut;
					vpct[(positionsCount - 1) * 6 + 5].Position = bas;

					vpct[(positionsCount - 1) * 6 + 1].TextureCoordinate = new Vector2((positionsCount + 1) * (1 / (float)limit), 1);
					vpct[(positionsCount - 1) * 6 + 4].TextureCoordinate = new Vector2((positionsCount + 1) * (1 / (float)limit), 0);
					vpct[(positionsCount - 1) * 6 + 5].TextureCoordinate = new Vector2((positionsCount + 1) * (1 / (float)limit), 1);

					vpct[(positionsCount - 1) * 6 + 1].Color.A = 255;
					vpct[(positionsCount - 1) * 6 + 4].Color.A = 255;
					vpct[(positionsCount - 1) * 6 + 5].Color.A = 255;
				}


				vpct[positionsCount * 6 + 0].Position = haut;
				vpct[positionsCount * 6 + 2].Position = bas;
				vpct[positionsCount * 6 + 3].Position = haut;

				vpct[positionsCount * 6 + 0].TextureCoordinate = new Vector2((positionsCount + 0) * (1 / (float)limit), 0);
				vpct[positionsCount * 6 + 2].TextureCoordinate = new Vector2((positionsCount + 0) * (1 / (float)limit), 1);
				vpct[positionsCount * 6 + 3].TextureCoordinate = new Vector2((positionsCount + 0) * (1 / (float)limit), 0);

				vpct[positionsCount * 6 + 0].Color.A = 255;
				vpct[positionsCount * 6 + 2].Color.A = 255;
				vpct[positionsCount * 6 + 3].Color.A = 255;
				

				
				if (positionsCount < limit)
					positionsCount++;


				for (int h = 0; h < 10; h++)
				for (int i = 0; i < positionsCount - 1; i++)
				{
					if (i> 0 && (i+h) % 3 == 1)
					{
						Vector3 beforeHaut = vpct[(i - 1) * 6 + 0].Position;
						Vector3 beforeBas = vpct[(i - 1) * 6 + 2].Position;

						Vector3 currHaut = vpct[(i + 0) * 6 + 0].Position;
						Vector3 currBas = vpct[(i + 0) * 6 + 2].Position;

						Vector3 nextHaut = vpct[(i + 1) * 6 + 0].Position;
						Vector3 nextBas = vpct[(i + 1) * 6 + 2].Position;

						currHaut = (beforeHaut + currHaut + nextHaut) / 3f;
						currBas = (beforeBas + currBas + nextBas) / 3f;

						vpct[i * 6 + 0].Position = currHaut;
						vpct[i * 6 + 2].Position = currBas;
						vpct[i * 6 + 3].Position = currHaut;

						vpct[(i - 1) * 6 + 1].Position = currBas;
						vpct[(i - 1) * 6 + 4].Position = currHaut;
						vpct[(i - 1) * 6 + 5].Position = currBas;

					}
				}


			}

			if (positionsCount > 1)
			{
				vpct[(positionsCount - 2) * 6 + 1].Position = bas;
				vpct[(positionsCount - 2) * 6 + 4].Position = haut;
				vpct[(positionsCount - 2) * 6 + 5].Position = bas;

				vpct[(positionsCount - 1) * 6 + 0].Position = haut;
				vpct[(positionsCount - 1) * 6 + 2].Position = bas;
				vpct[(positionsCount - 1) * 6 + 3].Position = haut;
			}

			for (int i = 0; i < positionsCount - 1; i++)
			{
				if (vpct[i * 6 + 0].Color.A>0)
				{
					vpct[i * 6 + 0].Color.A -= 10;
					if (vpct[i * 6 + 0].Color.A < 10)
						vpct[i * 6 + 0].Color.A = 0;
					vpct[i * 6 + 2].Color.A = vpct[i * 6 + 0].Color.A;
					vpct[i * 6 + 3].Color.A = vpct[i * 6 + 0].Color.A;

					if (i > 0)
					{
						vpct[(i - 1) * 6 + 1].Color.A = vpct[i * 6 + 0].Color.A;
						vpct[(i - 1) * 6 + 4].Color.A = vpct[i * 6 + 0].Color.A;
						vpct[(i - 1) * 6 + 5].Color.A = vpct[i * 6 + 0].Color.A;
					}
				}
			}
		}

		public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
		{
			if (positionsCount < 2)
				return;

			var old_depth = gcm.GraphicsDevice.DepthStencilState;
			var old_rast = gcm.GraphicsDevice.RasterizerState;

			gcm.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			gcm.GraphicsDevice.RasterizerState = rs;

			be.VertexColorEnabled = true;
			be.Texture = this.T2D;
			be.CurrentTechnique.Passes[0].Apply();
			gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.vpct, 0, (positionsCount-1)*2);

			gcm.GraphicsDevice.DepthStencilState = old_depth;
			gcm.GraphicsDevice.RasterizerState = old_rast;
		}
	}
}
