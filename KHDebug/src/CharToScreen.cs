using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace KHDebug
{
    public static class CharToScreen
    {
        public static Texture2D Sprite;
        public static int BufferWidth;
        public static int BufferHeight;

        static VertexPositionColorTexture[] VertexBuffer;
        static VertexPositionColorTexture[] VertexBufferBackground;

        private static byte[] ASCIIBuffer;
        private static Color[] ASCIIBufferColors;
        private static Color[] ASCIIBufferBackgroundColors;

        private static bool[] ASCIIBufferDirty;
        static float unitH;
        static float unitV;

        public static void Init(int pixelsPerCharH, int pixelsPerCharV)
        {
            if (Sprite==null)
            {
                Sprite = ResourceLoader.GetT2D(@"Content\Effects\Visual\DebugMenu\texture.png");
            }
            BufferWidth = 1920/ pixelsPerCharH;
            BufferHeight = 1080/pixelsPerCharV;
            ASCIIBuffer = new byte[BufferWidth * BufferHeight];
            ASCIIBufferColors = new Color[BufferWidth * BufferHeight];
            ASCIIBufferBackgroundColors = new Color[BufferWidth * BufferHeight];
            ASCIIBufferDirty = new bool[BufferWidth * BufferHeight];
            VertexBuffer = new VertexPositionColorTexture[BufferWidth * BufferHeight * 6];
            VertexBufferBackground = new VertexPositionColorTexture[BufferWidth * BufferHeight * 6];



            unitH = (float)pixelsPerCharH;
            unitV = (float)pixelsPerCharV;

            for (int i = 0; i < ASCIIBuffer.Length;i++)
            {
                ASCIIBuffer[i] = 0;
                ASCIIBufferDirty[i] = true;
                ASCIIBufferColors[i] = Color.TransparentBlack;
                ASCIIBufferBackgroundColors[i] = Color.TransparentBlack;
                float x = (i % BufferWidth) * unitH;
                float y = (i / BufferWidth) * unitV;

                VertexBuffer[i * 6 + 0].Position = new Vector3((-1920f / 2f) + x, (1080f / 2f) - y, 0);
                VertexBuffer[i * 6 + 1].Position = new Vector3((-1920f / 2f) + x + unitH, (1080f / 2f) - y, 0);
                VertexBuffer[i * 6 + 2].Position = new Vector3((-1920f / 2f) + x + unitH, (1080f / 2f) - y - unitV, 0);
                VertexBuffer[i * 6 + 3].Position = new Vector3((-1920f / 2f) + x, (1080f / 2f) - y, 0);
                VertexBuffer[i * 6 + 4].Position = new Vector3((-1920f / 2f) + x + unitH, (1080f / 2f) - y - unitV, 0);
                VertexBuffer[i * 6 + 5].Position = new Vector3((-1920f / 2f) + x, (1080f / 2f) - y - unitV, 0);

                VertexBufferBackground[i * 6 + 0].Position = VertexBuffer[i * 6 + 0].Position;
                VertexBufferBackground[i * 6 + 1].Position = VertexBuffer[i * 6 + 1].Position;
                VertexBufferBackground[i * 6 + 2].Position = VertexBuffer[i * 6 + 2].Position;
                VertexBufferBackground[i * 6 + 3].Position = VertexBuffer[i * 6 + 3].Position;
                VertexBufferBackground[i * 6 + 4].Position = VertexBuffer[i * 6 + 4].Position;
                VertexBufferBackground[i * 6 + 5].Position = VertexBuffer[i * 6 + 5].Position;


                VertexBuffer[i * 6 + 0].Color = Color.Transparent;
                VertexBuffer[i * 6 + 1].Color = Color.Transparent;
                VertexBuffer[i * 6 + 2].Color = Color.Transparent;
                VertexBuffer[i * 6 + 3].Color = Color.Transparent;
                VertexBuffer[i * 6 + 4].Color = Color.Transparent;
                VertexBuffer[i * 6 + 5].Color = Color.Transparent;

                VertexBufferBackground[i * 6 + 0].Color = Color.Transparent;
                VertexBufferBackground[i * 6 + 1].Color = Color.Transparent;
                VertexBufferBackground[i * 6 + 2].Color = Color.Transparent;
                VertexBufferBackground[i * 6 + 3].Color = Color.Transparent;
                VertexBufferBackground[i * 6 + 4].Color = Color.Transparent;
                VertexBufferBackground[i * 6 + 5].Color = Color.Transparent;
            }
        }

        public static void WriteText(string text,int posX,int posY, Color color, Color backgroundColor)
        {
            int pos = posY * BufferWidth + posX;

            Array.Copy(System.Text.Encoding.ASCII.GetBytes(text), 0,ASCIIBuffer, pos, text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                ASCIIBufferColors[pos+i] = color;
                ASCIIBufferBackgroundColors[pos + i] = backgroundColor;
                ASCIIBufferDirty[pos + i] = true;
            }

            RecreateVertexBuffer();
        }

        static void RecreateVertexBuffer()
        {
            for (int i = 0; i < ASCIIBuffer.Length; i++)
            {
                if (!ASCIIBufferDirty[i])
                    continue;
                
                VertexBuffer[i * 6 + 0].Color = ASCIIBufferColors[i];
                VertexBuffer[i * 6 + 1].Color = ASCIIBufferColors[i];
                VertexBuffer[i * 6 + 2].Color = ASCIIBufferColors[i];
                VertexBuffer[i * 6 + 3].Color = ASCIIBufferColors[i];
                VertexBuffer[i * 6 + 4].Color = ASCIIBufferColors[i];
                VertexBuffer[i * 6 + 5].Color = ASCIIBufferColors[i];

                VertexBufferBackground[i * 6 + 0].Color = ASCIIBufferBackgroundColors[i];
                VertexBufferBackground[i * 6 + 1].Color = ASCIIBufferBackgroundColors[i];
                VertexBufferBackground[i * 6 + 2].Color = ASCIIBufferBackgroundColors[i];
                VertexBufferBackground[i * 6 + 3].Color = ASCIIBufferBackgroundColors[i];
                VertexBufferBackground[i * 6 + 4].Color = ASCIIBufferBackgroundColors[i];
                VertexBufferBackground[i * 6 + 5].Color = ASCIIBufferBackgroundColors[i];

                byte chr = ASCIIBuffer[i];

                float x = (chr % 16) * 0.0625f;
                float y = (chr / 16) * 0.0625f;

                VertexBuffer[i * 6 + 0].TextureCoordinate = new Vector2(x, y);
                VertexBuffer[i * 6 + 1].TextureCoordinate = new Vector2(x + 0.0625f, y);
                VertexBuffer[i * 6 + 2].TextureCoordinate = new Vector2(x + 0.0625f, y + 0.0625f);
                VertexBuffer[i * 6 + 3].TextureCoordinate = new Vector2(x, y);
                VertexBuffer[i * 6 + 4].TextureCoordinate = new Vector2(x + 0.0625f, y + 0.0625f);
                VertexBuffer[i * 6 + 5].TextureCoordinate = new Vector2(x , y + 0.0625f);

                ASCIIBufferDirty[i] = false;
            }
        }


        public static void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            gcm.GraphicsDevice.RasterizerState = rs;
            

            //gcm.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            be.View = Matrix.CreateLookAt(new Vector3(0,0, 1), Vector3.Zero, Vector3.Up);
            be.Projection = Matrix.CreateOrthographic(1920f, 1080f, 0f, 10f);


            be.DiffuseColor = Color.White.ToVector3();
            be.Texture = ResourceLoader.EmptyT2D;
            be.CurrentTechnique.Passes[0].Apply();

            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBufferBackground, 0, VertexBufferBackground.Length / 6);
            be.Texture = Sprite;
            be.CurrentTechnique.Passes[0].Apply();
            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBuffer, 0, VertexBuffer.Length / 6);
            
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gcm.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            //gcm.GraphicsDevice.SamplerStates[0] = MainGame.DefaultSmaplerState;
            be.View = at.View;
            be.Projection = at.Projection;
        }
    }
}
