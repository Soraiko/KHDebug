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
    public static class LoadingScreen
    {
        private static VertexPositionNormalTexture[] vpnt;
        private static Texture2D texture;

        public static void Init()
        {
            string[] input = File.ReadAllLines(@"Content\Effects\Visual\LoadingScreen\heart.obj");
            List<Vector3> pos = new List<Vector3>(0);
            List<Vector3> normal = new List<Vector3>(0);
            List<Vector2> texcoord = new List<Vector2>(0);
            List<VertexPositionNormalTexture> output = new List<VertexPositionNormalTexture>(0);
            string[] spli,spli2;
            for (int i=0;i<input.Length;i++)
            {
                spli = input[i].Split(' ');
                if (spli[0] == "v")
                {
                    pos.Add(new Vector3(
                        MainGame.SingleParse(spli[1]),
                        MainGame.SingleParse(spli[2]),
                        MainGame.SingleParse(spli[3])
                        ));
                }
                if (spli[0] == "vt")
                {
                    texcoord.Add(new Vector2(
                        MainGame.SingleParse(spli[1]),
                        MainGame.SingleParse(spli[2])
                        ));
                }
                if (spli[0] == "vn")
                {
                    normal.Add(new Vector3(
                        MainGame.SingleParse(spli[1]),
                        MainGame.SingleParse(spli[2]),
                        MainGame.SingleParse(spli[3])
                        ));
                }
                if (spli[0] == "f")
                {
                    spli2 = spli[1].Split('/');
                    int iv = int.Parse(spli2[0]);
                    int ivt = int.Parse(spli2[1]);
                    int ivn = int.Parse(spli2[2]);
                    VertexPositionNormalTexture vpn1 = new VertexPositionNormalTexture();
                    vpn1.Position = pos[iv - 1];
                    vpn1.TextureCoordinate = texcoord[ivt - 1];
                    vpn1.Normal = normal[ivn - 1];

                    spli2 = spli[2].Split('/');
                    iv = int.Parse(spli2[0]);
                    ivt = int.Parse(spli2[1]);
                    ivn = int.Parse(spli2[2]);
                    VertexPositionNormalTexture vpn2 = new VertexPositionNormalTexture();
                    vpn2.Position = pos[iv - 1];
                    vpn2.TextureCoordinate = texcoord[ivt - 1];
                    vpn2.Normal = normal[ivn - 1];

                    spli2 = spli[3].Split('/');
                    iv = int.Parse(spli2[0]);
                    ivt = int.Parse(spli2[1]);
                    ivn = int.Parse(spli2[2]);
                    VertexPositionNormalTexture vpn3 = new VertexPositionNormalTexture();
                    vpn3.Position = pos[iv - 1];
                    vpn3.TextureCoordinate = texcoord[ivt - 1];
                    vpn3.Normal = normal[ivn - 1];

                    output.Add(vpn3);
                    output.Add(vpn2);
                    output.Add(vpn1);
                }
            }
            vpnt = output.ToArray();
            output.Clear();
            output = null;
            pos.Clear();
            pos = null;
            normal.Clear();
            normal = null;
            texcoord.Clear();
            texcoord = null;
            
            texture = ResourceLoader.GetT2D(@"Content\Effects\Visual\LoadingScreen\texture.png"); 
            for (int i=0;i< vpnt.Length;i++)
            {
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, Matrix.CreateScale(new Vector3(5f, 5f, 5f)));
            }

            dnewV = new Viewport();
            dnewV.Bounds = Program.game.graphics.GraphicsDevice.Viewport.Bounds;
            dnewV.X = 100;
        }
        static double angle = 0;
        
        static Viewport dnewV;

        public static void Draw(GraphicsDeviceManager gcm, AlphaTestEffect at, BasicEffect be, RasterizerState rs, RasterizerState rsNoCull)
        {
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            be.EnableDefaultLighting();
            be.DiffuseColor = (Color.White).ToVector3();
            be.SpecularColor = (Color.White * 0.5f).ToVector3();
            be.LightingEnabled = true;
            be.DirectionalLight0.Direction = new Vector3(0,0,-1);
            //be.PreferPerPixelLighting = true;

            be.View = Matrix.CreateLookAt(new Vector3(0,0,100), Vector3.Zero, Vector3.Up);
            be.Projection = Matrix.CreateOrthographic(1920f, 1080f, 0f, 1000f);

            Matrix rot = Matrix.CreateRotationY(-0.1f);
            float ratio = 1.6f/Program.game.mainCamera.AspectRatio;

            Matrix scale = Matrix.CreateScale(new Vector3(ratio, 1, 1));
            Matrix scaleReverse = Matrix.CreateScale(new Vector3(1f/ ratio, 1, 1));

            Matrix tranFo = Matrix.CreateTranslation(850f, -450f, 0);
            Matrix tranBa = Matrix.CreateTranslation(-850f, 450f, 0);
            for (int i = 0; i < vpnt.Length; i++)
            {
                vpnt[i].Normal = Vector3.Transform(vpnt[i].Normal, rot);
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, rot);
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, scale);
            }

            for (int i = 0; i < vpnt.Length; i++)
            {
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, tranFo);
            }


            be.Texture = texture;
            be.CurrentTechnique.Passes[0].Apply();

            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vpnt, 0, vpnt.Length / 3);
            
            for (int i = 0; i < vpnt.Length; i++)
            {
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, tranBa);
                vpnt[i].Position = Vector3.Transform(vpnt[i].Position, scaleReverse);
            }

            be.LightingEnabled = false;
            

            be.View = at.View;
            be.Projection = at.Projection;
        }
    }
}
