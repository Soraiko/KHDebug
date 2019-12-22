using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KHDebug
{
    public class BinaryModel:Model
    {
        public BinaryModel(string filename)
        {
            Console.WriteLine("Loading Resource " + filename);
            this.Type = ResourceType.Model;
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.FileName = filename;
        }

        public new unsafe void Parse()
        {
			byte[] array = File.ReadAllBytes(this.FileName);
			int address = 0;

			this.ModelType = (MDType)(sbyte)array[address]; address++;
            this.NoCull = (sbyte)array[address] <0; address++;
			this.ZIndex = (int)(sbyte)array[address]; address++;
            address++;





			int textureCount = BitConverter.ToInt32(array, address); address+=4;

			address += 8;

			//int addressCopy = address;
			//address = 16 + textureCount * 0x30;
			/*new System.Threading.Thread(() =>
			{
				System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
				*/
				for (int i = 0; i < textureCount; i++)
				{
					byte length = array[address]; address++;
					string fname = "Content\\" + Encoding.ASCII.GetString(array, address, length); address += length;

					this.materialFileNames.Add(fname);
						//this.Textures.Add(Texture2D.FromStream(Program.game.GraphicsDevice, new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Open)));
					this.Textures.Add(ResourceLoader.GetT2D(fname));
					
					//this.Textures.Add(ResourceLoader.EmptyT2D);
					if (0x30 - (length + 1) > 0)
					address += 0x30 - (length + 1);
				}
			/*	while (this.vBuffer.VertexCount == 0) { }
				array = null;
			}).Start();*/

            int meshCount = BitConverter.ToInt32(array, address); address+=4;
            BitConverter.ToInt32(array, address); address+=4; BitConverter.ToInt32(array, address); address+=4; BitConverter.ToInt32(array, address); address+=4;

            int vertexCount = 0;

            for (int i = 0; i < meshCount; i++)
            {
                this.MaterialIndices.Add(BitConverter.ToInt32(array,address)); address += 4;
                int[] off = new int[2];
                off[0] = BitConverter.ToInt32(array, address); address+=4;
                off[1] = BitConverter.ToInt32(array, address); address+=4;
                this.MeshesOffsets.Add(off);
                vertexCount += off[1];
            }

            if (meshCount % 4 > 0)
                for (int i = 0; i < (4 - (meshCount % 4)); i++)
                {
                    BitConverter.ToInt32(array, address); address+=4;
                }


            this.VertexBuffer_c = new ComputedVertex[vertexCount];
            this.VertexBufferColor = new VertexPositionColorTexture[vertexCount];

			this.ShadowBuffer = new VertexPositionColorTexture[this.ModelType == MDType.Human ? 6:0];

			//for (int i = 0; i < this.VertexBufferShadow2.Length; i++)
			//    VertexBufferShadow2[i].Color = new Color(0, 0, 0, 0);


			for (int i = 0; i < vertexCount; i++)
            {
                int infs = BitConverter.ToInt32(array, address); address+=4;


                VertexPositionColorTexture vpct = new VertexPositionColorTexture
                {
                    TextureCoordinate = new Vector2(BitConverter.ToSingle(array,address), BitConverter.ToSingle(array, address+4)),
                    Position = new Vector3(0, 0, 0),
                    Color = new Microsoft.Xna.Framework.Color(array[address+8], array[address+9], array[address+10], array[address+11])
                };
				address += 12;
                if (!this.HasColor && (vpct.Color.R < 1 || vpct.Color.G < 1 || vpct.Color.B < 1 || vpct.Color.A < 1))
                    this.HasColor = true;


                int ind = ((i / 3) * 3 + (2 - (i % 3)));

                this.VertexBuffer_c[ind] = new ComputedVertex
                {
                    Count = infs
                };
                int ind_ = 0;

                for (int j = 0; j < infs; j+=4)
                {
					this.VertexBuffer_c[ind].Matis[ind_] = BitConverter.ToInt16(array, address);
					address +=16;
                    this.VertexBuffer_c[ind].Vertices[j] = BitConverter.ToSingle(array, address);
                    this.VertexBuffer_c[ind].Vertices[j+1] = BitConverter.ToSingle(array, address+4);
                    this.VertexBuffer_c[ind].Vertices[j+2] = BitConverter.ToSingle(array, address + 8);
					this.VertexBuffer_c[ind].Vertices[j+3] = BitConverter.ToSingle(array, address + 12);
					address += 16;
					ind_++;
                }
                this.VertexBufferColor[ind] = vpct;

            }


			//array = null;

			byte[] array2;

			this.vBuffer = new VertexBuffer(KHDebug.Program.game.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), this.VertexBufferColor.Length, BufferUsage.None);

            this.vBuffer.SetData<VertexPositionColorTexture>(this.VertexBufferColor);


			if (File.Exists(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel"))
				array2 = File.ReadAllBytes(Path.GetDirectoryName(this.FileName) + @"\" + this.Name + ".skel");
            else
				array2 = File.ReadAllBytes(@"Content\default\skeleton.skel");


            address = 4;

            int count = BitConverter.ToInt32(array2, address); address+=4;
            for (int i = 0; i < count; i++)
            {
                address = 16 + i * 16;
                if (BitConverter.ToInt32(array2, address) == 4)
                {
					address += 4;
					BitConverter.ToInt32(array2, address); address+=4;
                    address = BitConverter.ToInt32(array2, address) + 0xA0;
                    break;
                }
				else
				address += 4;
			}
            short boneCount = BitConverter.ToInt16(array2, address);
			address += 4;
			address = address - 0x14 + BitConverter.ToInt32(array2, address);

            this.Skeleton = new Skeleton
            {
                Bones = new List<Bone>(0)
            };
            List<int> parents = new List<int>(0);

            for (int i = 0; i < boneCount; i++)
            {
                Bone currBone = new Bone("bone" + BitConverter.ToInt32(array2, address).ToString("d3"));
				address += 4;
                parents.Add(BitConverter.ToInt32(array2, address)); address += 12;
                currBone.ScaleX = BitConverter.ToSingle(array2, address); address += 4;
                currBone.ScaleY = BitConverter.ToSingle(array2, address); address += 4;
				currBone.ScaleZ = BitConverter.ToSingle(array2, address); address += 4;
				address += 4;
				currBone.RotateX = BitConverter.ToSingle(array2, address); address += 4;
				currBone.RotateY = BitConverter.ToSingle(array2, address); address += 4;
				currBone.RotateZ = BitConverter.ToSingle(array2, address); address += 4;
				address+=4;
                currBone.TranslateX = BitConverter.ToSingle(array2, address); address += 4;
				currBone.TranslateY = BitConverter.ToSingle(array2, address); address += 4;
				currBone.TranslateZ = BitConverter.ToSingle(array2, address); address += 4;
				address += 4;
				currBone.GlobalMatrix = Matrix.CreateScale(new Vector3(currBone.ScaleX, currBone.ScaleY,currBone.ScaleZ));
                currBone.GlobalMatrix.Translation = new Vector3(currBone.TranslateX, currBone.TranslateY, currBone.TranslateZ);

                currBone.localMatrix = currBone.GlobalMatrix;
                this.Skeleton.Bones.Add(currBone);
            }
			array2 = null;

			for (int i = 0; i < parents.Count; i++)
            {
                if (parents[i] > -1)
                    this.Skeleton.Bones[i].Parent = this.Skeleton.Bones[parents[i]];
            }
            this.Skeleton.ComputeMatrices();
			
            this.RecreateVertexBuffer(true);

        }
    }
}
