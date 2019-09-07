using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHDebug
{
    public class Resource
    {
        public int ResourceIndex = -1;

        public static string[] ResourceIndices = new string[]
        {   @"Content\Models\TT09\TT09",
            @"Content\Models\TT09\OBJECTS\TT09-Doors",
            @"Content\Models\TT08\TT08",
            @"Content\Models\TT08\SKY\TT08-SKY0",
            @"Content\Models\TT08\OBJECTS\TT08-sp001",
            @"Content\Models\TT08\OBJECTS\TT08-Clock",
            @"Content\Models\TT08\OBJECTS\TT08-Doors",
            @"Content\Models\TT08\OBJECTS\TT08-mdl-Bells",
            @"Content\Models\TT08\OBJECTS\TT08-mdl-Bird",
            @"Content\Models\TT08\OBJECTS\TT08-mdl-Trees",
            @"Content\Models\TT08\OBJECTS\TT08-Poster",
            @"Content\Models\TT08\OBJECTS\TT08-Window",
            @"Content\Models\TT08\OBJECTS\TT08-Windows",
            @"Content\Models\P_EX110\P_EX110",
            @"Content\Models\P_EX100\P_EX100",
            @"Content\Models\W_EX110\W_EX110",
            @"Content\Models\P_EX020\P_EX020",
            @"Content\Models\W_EX020\W_EX020",
            @"Content\Models\P_EX110\MSET",
            @"Content\Models\W_EX110\MSET",
            @"Content\Models\P_EX020\MSET",
            @"Content\Models\W_EX020\MSET"
        };

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
