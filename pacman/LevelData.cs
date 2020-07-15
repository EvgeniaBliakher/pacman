using System;
using System.IO;

namespace pacman
{
    public class LevelData
    {
        // data about timing of the ghosts in levels
        
        public int curLevel;
        public int curChaseTimeSec;
        public int curScatterTimeSec;
        public int curFrightenedTimeSec;

        public int levelsCount;
        public int[] chaseTime;
        public int[] scatterTime;
        public int[] frightenedTime;
        
        public LevelData(string pathToDataFile)
        {
            this.curLevel = 0;
            ReadLevelData(pathToDataFile);
        }

        public void ReadLevelData(string pathToDataFile)
        {
            // Structure of level data file:
            // first line - number of levels
            // then 3 lines for each level
            // first contains chase time in seconds, second scatter time, third frightened time
            // there should be no gaps between levels
            
            System.IO.StreamReader sr = new StreamReader(pathToDataFile);
            levelsCount = int.Parse(sr.ReadLine());
            chaseTime = new int[levelsCount + 1];    // level info starts at index 1
            scatterTime = new int[levelsCount + 1];
            frightenedTime = new int[levelsCount + 1];
            for (int i = 1; i <= levelsCount ; i++)
            {
                chaseTime[i] = int.Parse(sr.ReadLine());
                scatterTime[i] = int.Parse(sr.ReadLine());
                frightenedTime[i] = int.Parse(sr.ReadLine());
            }
        }

        public void GetNextLevelData()
        {
            // increments current level until max level is reached
            // then repeatedly returns level data of the last level
            
            if (curLevel < levelsCount)
            {
                curLevel++;
            }

            curChaseTimeSec = chaseTime[curLevel];
            curScatterTimeSec = scatterTime[curLevel];
            curFrightenedTimeSec = frightenedTime[curLevel];
            
            loadLevelDataToGlobal();
        }

        private void loadLevelDataToGlobal()
        {
            Global.chaseTimeSec = curChaseTimeSec;
            Global.scatterTimeSec = curScatterTimeSec;
            Global.frightenedTimeSec = curFrightenedTimeSec;
        }
    }
    
}