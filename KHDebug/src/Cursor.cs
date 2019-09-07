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
