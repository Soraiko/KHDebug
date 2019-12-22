using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
namespace KHDebug
{
	public static class TriangleStrip
	{
		/*TriangleStrip.VPC[0].Position = v1;
		TriangleStrip.VPC[1].Position = v2;
		TriangleStrip.VPC[2].Position = v3;*/
		//Program.game.cursors[1].Position = coin;

		public static VertexPositionColor[] VPC = new VertexPositionColor[] {
			new VertexPositionColor(Vector3.Zero,Color.Red),
			new VertexPositionColor(Vector3.Zero,Color.Red),
			new VertexPositionColor(Vector3.Zero,Color.Red)
		};

		public static void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
		{
			gcm.GraphicsDevice.RasterizerState = rs;
			be.DiffuseColor = Color.White.ToVector3();
			be.VertexColorEnabled = true;
			be.TextureEnabled = false;
			be.CurrentTechnique.Passes[0].Apply();
			gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VPC, 0, 1);
			be.TextureEnabled = true;
		}
	}

	public class Cursor
    {

        public static Vector3[] VECTORS = new Vector3[] {

            new Vector3(-0.500000f,0.500000f,0.500000f),
new Vector3(0.500000f,-0.500000f,0.500000f),
new Vector3(-0.500000f,-0.500000f,0.500000f),
new Vector3(0.500000f,0.500000f,0.500000f),
new Vector3(0.500000f,-0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,-0.500000f),
new Vector3(0.500000f,0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,0.500000f),
new Vector3(0.500000f,0.500000f,-0.500000f),
new Vector3(0.500000f,0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,-0.500000f),
new Vector3(0.500000f,0.500000f,-0.500000f),
new Vector3(-0.500000f,0.500000f,-0.500000f),
new Vector3(0.500000f,-0.500000f,-0.500000f),
new Vector3(0.500000f,0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,0.500000f),
new Vector3(0.500000f,-0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,-0.500000f),
new Vector3(0.500000f,-0.500000f,0.500000f),
new Vector3(0.500000f,-0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,0.500000f),
new Vector3(0.500000f,0.500000f,0.500000f),
new Vector3(0.500000f,-0.500000f,-0.500000f),
new Vector3(0.500000f,-0.500000f,0.500000f),
new Vector3(0.500000f,0.500000f,-0.500000f),
new Vector3(0.500000f,-0.500000f,-0.500000f),
new Vector3(0.500000f,0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,-0.500000f),
new Vector3(-0.500000f,-0.500000f,0.500000f),
new Vector3(-0.500000f,-0.500000f,-0.500000f),
new Vector3(-0.500000f,0.500000f,0.500000f),
new Vector3(-0.500000f,-0.500000f,0.500000f),
new Vector3(-0.500000f,0.500000f,-0.500000f)
        };
        public Vector3 Position;
        public Color Color;
        public VertexPositionColor[] VPC;

        public Cursor()
        {
            this.Color = Color.White;
            this.VPC = new VertexPositionColor[VECTORS.Length*3];
        }
        
        public void Draw(GraphicsDeviceManager gcm, BasicEffect be)
        {

            for (int i = 0; i < this.VPC.Length; i++)
            {
                this.VPC[i].Color = this.Color;
                this.VPC[i].Position = VECTORS[i% VECTORS.Length];
                if (i< this.VPC.Length/3)
                {
                    this.VPC[i].Position.Z *= 10000;
                }
                else if(i < (this.VPC.Length / 3)*2)
                {
                    this.VPC[i].Position.Y *= 100;
                }
                else
                {
                    this.VPC[i].Position.X *= 10000;
                }
                this.VPC[i].Position += this.Position;
            }

            //at.Texture = KHDebug.ResourceLoader.GetT2D(this.materialFileNames[this.MaterialIndices[ind]]);
            be.DiffuseColor = Color.White.ToVector3();
            be.VertexColorEnabled = true;
            be.TextureEnabled= false;
            be.CurrentTechnique.Passes[0].Apply();
            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.VPC, 0, this.VPC.Length / 3);

            be.TextureEnabled = true;
        }
    }
}
