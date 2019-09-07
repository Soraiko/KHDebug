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
    public class SunBeam
    {
        static VertexPositionColorTexture[] vpct_;
        VertexPositionColorTexture[] vpct;
        static Texture2D texture;
        public Collision coll;

        public SunBeam()
        {
            if (texture == null)
            {
                FileStream fs = new FileStream("Content\\Effects\\Visual\\Sun\\beam.png", FileMode.Open, FileAccess.Read, FileShare.Read);
                texture = Texture2D.FromStream(Program.game.graphics.GraphicsDevice, fs);

                vpct_ = new VertexPositionColorTexture[16];
                vpct_[0].TextureCoordinate = new Vector2(5f / 400f, 14f / 365f);
                vpct_[1].TextureCoordinate = new Vector2(341f / 400f, 14f / 365f);
                vpct_[2].TextureCoordinate = new Vector2(5f / 400f, 350f / 365f);
                vpct_[3].TextureCoordinate = new Vector2(341f / 400f, 350f / 365f);

                vpct_[0].Position = new Vector3(-0.5f, 0.5f, 0);
                vpct_[1].Position = new Vector3(0.5f, 0.5f, 0);
                vpct_[2].Position = new Vector3(-0.5f, -0.5f, 0);
                vpct_[3].Position = new Vector3(0.5f, -0.5f, 0);

                vpct_[4].TextureCoordinate = new Vector2(343f / 400f, 5f / 365f);
                vpct_[5].TextureCoordinate = new Vector2(396f / 400f, 5f / 365f);
                vpct_[6].TextureCoordinate = new Vector2(343f / 400f, 76f / 365f);
                vpct_[7].TextureCoordinate = new Vector2(396f / 400f, 76f / 365f);

                vpct_[4].Position = new Vector3(-0.5f, 0.5f, 0);
                vpct_[5].Position = new Vector3(0.5f, 0.5f, 0);
                vpct_[6].Position = new Vector3(-0.5f, -0.5f, 0);
                vpct_[7].Position = new Vector3(0.5f, -0.5f, 0);

                vpct_[8].TextureCoordinate = new Vector2(343f / 400f, 5f / 365f);
                vpct_[9].TextureCoordinate = new Vector2(396f / 400f, 5f / 365f);
                vpct_[10].TextureCoordinate = new Vector2(343f / 400f, 76f / 365f);
                vpct_[11].TextureCoordinate = new Vector2(396f / 400f, 76f / 365f);

                vpct_[8].Position = new Vector3(-0.5f, 0.5f, 0);
                vpct_[9].Position = new Vector3(0.5f, 0.5f, 0);
                vpct_[10].Position = new Vector3(-0.5f, -0.5f, 0);
                vpct_[11].Position = new Vector3(0.5f, -0.5f, 0);

                vpct_[12].TextureCoordinate = new Vector2(343f / 400f, 5f / 365f);
                vpct_[13].TextureCoordinate = new Vector2(396f / 400f, 5f / 365f);
                vpct_[14].TextureCoordinate = new Vector2(343f / 400f, 76f / 365f);
                vpct_[15].TextureCoordinate = new Vector2(396f / 400f, 76f / 365f);

                vpct_[12].Position = new Vector3(-0.5f, 0.5f, 0);
                vpct_[13].Position = new Vector3(0.5f, 0.5f, 0);
                vpct_[14].Position = new Vector3(-0.5f, -0.5f, 0);
                vpct_[15].Position = new Vector3(0.5f, -0.5f, 0);
            }
            this.vpct = new VertexPositionColorTexture[16];
            this.Scales = new float[] { 7000000, 80000, 100000, 30000 };
            this.Offsets = new float[] { 0.99f, -0.45f, -0.2f, -0.35f };
            this.Alphas = new Color[] { Color.White, new Color(255, 144, 98), new Color(255, 205, 149), new Color(255, 142, 54)};
            this.From = Vector3.Zero;
            this.To = Vector3.Zero;
            this.OffsetsV3 = new Vector3[4];

            short[] indices = new short[6*4];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;

            indices[6] = 4;
            indices[7] = 5;
            indices[8] = 6;
            indices[9] = 5;
            indices[10] = 7;
            indices[11] = 6;

            indices[12] = 8;
            indices[13] = 9;
            indices[14] = 10;
            indices[15] = 9;
            indices[16] = 11;
            indices[17] = 10;

            indices[18] = 12;
            indices[19] = 13;
            indices[20] = 14;
            indices[21] = 13;
            indices[22] = 15;
            indices[23] = 14;


            this.vBuffer = new VertexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), this.vpct.Length, BufferUsage.None);
            this.iBuffer = new IndexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(short), indices.Length, BufferUsage.None);
            this.iBuffer.SetData<short>(indices);

        }

        public float[] Scales;
        public float[] Offsets;
        public Vector3[] OffsetsV3;
        public Color[] Alphas;
        public Vector3 From;
        public Vector3 To;

        public VertexBuffer vBuffer;
        public IndexBuffer iBuffer;

        public void RecreateVertexBuffer()
        {
            float dist = Vector3.Distance(this.From,this.To);


            float angleFT = (float)Math.Atan2((this.From.Z - this.To.Z) / dist, (this.From.X - this.To.X) / dist);
            float angleTC = Program.game.mainCamera.PrincipalYaw;

            float angle = -Math.Abs(angleFT - angleTC);

            float alpha = Math.Abs(angle + 3.27f);
            if (alpha > 0.4)
                alpha = 0.4f;
            alpha = 1-(alpha/0.4f);

            float alpha2 = alpha - (float)(Math.Sin(((DateTime.Now.Millisecond % 200)/200f) * MainGame.PI) / 11f);

            /*byte v1 = 50;
            byte v2 = 255;
            byte v3 = 255;
            byte v4 = 255;*/
            if (alpha < 0)
                alpha = 0;
            if (alpha2 < 0)
                alpha2 = 0;

            /*try
            {
                string[] input = File.ReadAllLines("text.txt");
                v1 = byte.Parse(input[0]);
                v2 = byte.Parse(input[1]);
                v3 = byte.Parse(input[2]);
                v4 = byte.Parse(input[3]);

                this.Offsets[0] = MainGame.SingleParse(input[4]);
                this.Offsets[1] = MainGame.SingleParse(input[5]);
                this.Offsets[2] = MainGame.SingleParse(input[6]);
                this.Offsets[3] = MainGame.SingleParse(input[7]);

                this.Scales[0] = MainGame.SingleParse(input[8]);
                this.Scales[1] = MainGame.SingleParse(input[9]);
                this.Scales[2] = MainGame.SingleParse(input[10]);
                this.Scales[3] = MainGame.SingleParse(input[11]);
                //
            }
            catch
            {

            }*/
            
            this.Alphas[0].A = (byte)(alpha * 80);
            this.Alphas[1].A = (byte)(alpha2 * 50);
            this.Alphas[2].A = (byte)(alpha2 * 30);
            this.Alphas[3].A = (byte)(alpha2 * 80);
            
            Vector3 midd = (this.To - this.From) / 2f;


            Vector3 diffFrom = this.From - midd;
            diffFrom = Vector3.Transform(diffFrom, Matrix.CreateRotationY(angle));
            diffFrom += midd;

            Vector3 diffTo = this.To - midd;
            diffTo = Vector3.Transform(diffTo, Matrix.CreateRotationY(angle));
            diffTo += midd;

            Vector3 diff = this.To - diffFrom;

            this.OffsetsV3[0] = this.From + diff * this.Offsets[0];
            this.OffsetsV3[1] = this.From + diff * this.Offsets[1];
            this.OffsetsV3[2] = this.From + diff * this.Offsets[2];
            this.OffsetsV3[3] = this.From + diff * this.Offsets[3];

            Matrix m = Program.game.mainCamera.GetMatrix();
            for (int i = 0; i < vpct_.Length; i++)
            {
                Vector3 pos = vpct_[i].Position * this.Scales[i / 4];
                pos = Vector3.Transform(pos, m);
                if (i < 4)
                {
                    pos *= (float)(1 + Math.Sin(MainGame.PI * ((DateTime.Now.Millisecond % 280) /280f)) / 100f);
                }
                this.vpct[i].Position = this.OffsetsV3[i / 4]  + pos;
                this.vpct[i].TextureCoordinate = vpct_[i].TextureCoordinate;
                    this.vpct[i].Color = this.Alphas[i / 4];
            }
            this.vBuffer.SetData<VertexPositionColorTexture>(this.vpct);
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            if (this.coll.HasCol(Program.game.mainCamera.SunBeamPosition, this.From))
                return;
            gcm.GraphicsDevice.RasterizerState = rs;

            gcm.GraphicsDevice.SetVertexBuffer(this.vBuffer);
            gcm.GraphicsDevice.Indices = this.iBuffer;

            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            be.Texture = texture;
            be.CurrentTechnique.Passes[0].Apply();

            gcm.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 24 / 3);

            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
