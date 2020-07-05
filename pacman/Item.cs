using System;
using System.IO;

namespace pacman
{
    public class GamePlan
    {
        private char[,] map;
        private int height;
        private int width;
        public Pacman pacman;
        

        public GamePlan(string pathToMap)
        {
            readMapFromFile(pathToMap);
        }

        private void readMapFromFile(string pathToFile)
        {
            System.IO.StreamReader sr = new StreamReader(pathToFile);
            height = int.Parse(sr.ReadLine());
            width = int.Parse(sr.ReadLine());
            map = new char[width, height];    // width is x, height is y

            for (int y = 0; y < height; y++)
            {
                string row = sr.ReadLine();
                for (int x = 0; x < width; x++)
                {
                    char mapChar = row[x];
                    map[x, y] = mapChar;
                    switch (mapChar)
                    {
                        case '>': 
                        case '<': 
                        case '^':
                        case 'v':
                            pacman = new Pacman(x, y, mapChar);
                            break;
                    }
                }
            }
        }
    }
    
    public class Item
    {
        public int x;
        public int y;

        public Item(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Dot : Item
    {
        public int points_worth;

        public Dot(int x, int y, int pointsWorth) : base(x, y)
        {
            points_worth = pointsWorth;
        }
    }

    public class BigDot : Dot
    {
        public BigDot(int x, int y, int pointsWorth) : base(x, y, pointsWorth)
        {
        }
    }

    public enum PacmanState
    {
        Run,
        Chase
    }

    public class Pacman : Item
    {
        public Tuple<int, int>  Direction; // direction in x and y, one of them always zero - no diagonal movement
        public PacmanState State;

        public Pacman(int x, int y, char directionChar) : base(x, y)
        {
            switch (directionChar)
            {
                case '>':
                    this.Direction = new Tuple<int, int>(1, 0);
                    break;
                case '<':
                    this.Direction = new Tuple<int, int>(-1, 0);
                    break;
                case '^':
                    this.Direction = new Tuple<int, int>(0, 1);
                    break;
                case 'v':
                    this.Direction = new Tuple<int, int>(0, -1);
                    break;
                default:
                    this.Direction = new Tuple<int, int>(1, 0);
                    break;
            }

            this.State = PacmanState.Run;
        }
    }
}