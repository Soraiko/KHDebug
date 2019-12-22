using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHDebug
{
    public class Resource
	{
		public bool JustLoaded = false;

		int resourceIndex = -1;
		public int ResourceIndex
		{
			get
			{
				return this.resourceIndex;
			}
			set
			{
				if (value < 0 && this.FileName.Contains("Models"))
				{
					string[] newRI = new string[Resource.ResourceIndices.Length+1];
					Array.Copy(Resource.ResourceIndices, newRI, Resource.ResourceIndices.Length);
					newRI[newRI.Length - 1] = this.FileName.Split('.')[0] + ".";
					Resource.ResourceIndices = null;
					Resource.ResourceIndices = newRI;

					System.IO.File.WriteAllLines(@"Content\Models\objentry.txt", newRI);
					value = newRI.Length - 1;
				}
				this.resourceIndex = value;
			}
		}

		public static string[] ResourceIndices;

        public bool ObjectMsetRender = false;
        public bool Cutscene = false;
        public bool Render;

        static int IDs = 0;
        int UniqueID = 0;

        public string Name;
        public string FileName;
        public ResourceType Type;
        public List<Resource> Links;

        public enum ResourceType
        {
            DaeModel = 0,
            Model = 1,
            Moveset = 2,
            BinaryMoveset = 3,
            Collision = 4
        }

        public Resource()
        {
            this.Links = new List<Resource>(0);
            this.Render = false;
            this.UniqueID = IDs;
            IDs++;
        }

        
    }
}
