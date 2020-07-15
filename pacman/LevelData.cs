using System;
using System.IO;

namespace pacman
{
    public class LevelData
    {
        public int curLevel;
        public int chaseTimeSec;
        public int scatterTimeSec;
        public int frightenedTimeSec;

        public long levelDataOffset;

        public LevelData()
        {
            this.curLevel = 1;
            this.levelDataOffset = 0;
        }

        public void ReadLevelData(string pathToDataFile)
        {
            
        }
        
        
    }

    public class Reader
    {
        public static int Precti()
        {
            int z = Console.Read();
            while (((z < '0') && (z!= '-')) || (z > '9'))
                z = Console.Read();
            //ToDo: vyresit konec!!
            int x = 0;
            int sign = 1;
            if (z == '-')
            {
                sign = -1;
                z = Console.Read();
            }
            while ((z >= '0') && (z <= '9'))
            {
                x = 10 * x + z - '0';
                z = Console.Read();
            }
            return x * sign;
        }
    }
}