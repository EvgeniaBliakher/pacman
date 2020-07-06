using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public class GamePlan
    {
        public char[,] map;
        public int height;
        public int width;
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
                            pacman = new Pacman(x, y, this, mapChar);
                            break;
                    }
                }
            }
        }

        public bool IsFree(int x, int y)
        {
            char mapChar = map[x, y];
            if (mapChar == Global.FLOOR)
            {
                return true;
            }
            return false;
        }

        public bool IsDot(int x, int y)
        {
            char mapChar = map[x, y];
            if (mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }
        public bool IsFreeOrDot(int x, int y)
        {
            char mapChar = map[x, y];
            if (mapChar == Global.FLOOR || mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }

        public void Move(int fromX, int fromY, int toX, int toY)
        {
            char mapChar = map[fromX, fromY];
            map[fromX, fromY] = Global.FLOOR;
            map[toX, toY] = mapChar;
        }

        public void ChangeOnPosition(int x, int y, char newChar)
        {
            map[x, y] = newChar;
        }
        
    }
    
    public class Item
    {
        public int x;
        public int y;
        public GamePlan gamePlan;

        public Item(int x, int y, GamePlan gamePlan)
        {
            this.x = x;
            this.y = y;
            this.gamePlan = gamePlan;
        }
    }

    public class Dot : Item
    {
        public int points_worth;

        public Dot(int x, int y, GamePlan gamePlan, int pointsWorth) : base(x, y, gamePlan)
        {
            points_worth = pointsWorth;
        }
    }

    public class BigDot : Dot
    { 
        public BigDot(int x, int y, GamePlan gamePlan, int pointsWorth) : base(x, y, gamePlan, pointsWorth)
        {
        }
    }
    

    public class Pacman : Item
    {
        public Tuple<int, int>  Direction; // direction in x and y, one of them always zero - no diagonal movement
        public char DirectionChar;
        public Tuple<int, int> WishedDirection;

        public Pacman(int x, int y, GamePlan gamePlan, char directionChar) : base(x, y, gamePlan)
        {
            this.DirectionChar = directionChar;
            switch (directionChar)
            {
                case '>':
                    this.Direction = DirectionGlobal.CharToDirection['>'];
                    break;
                case '<':
                    this.Direction = DirectionGlobal.CharToDirection['<'];
                    break;
                case '^':
                    this.Direction = DirectionGlobal.CharToDirection['^'];
                    break;
                case 'v':
                    this.Direction = DirectionGlobal.CharToDirection['v'];
                    break;
                default:
                    this.Direction = DirectionGlobal.CharToDirection['>'];
                    break;
            }

            this.WishedDirection = Direction;
            // this.WishedDirection = DirectionGlobal.CharToDirection['^'];
        }
        
        public void Move()
        {
            if (!Direction.Equals(WishedDirection))
            {
                int xInDir = x + WishedDirection.Item1;
                int yInDir = y + WishedDirection.Item2;
                if (gamePlan.IsFreeOrDot(xInDir, yInDir))
                {
                    changeDirection(WishedDirection);
                }
            }
            MoveInDirection();
        }
        public void MoveInDirection()
        {
            int newX = x + Direction.Item1;
            int newY = y + Direction.Item2;
            if (gamePlan.IsFreeOrDot(newX, newY))
            {
                gamePlan.Move(x, y, newX, newY);
                x = newX;
                y = newY;
            }
        }

        private void changeDirection(Tuple<int, int> newDirection)
        {
            Direction = newDirection;
            char newDirectionChar = DirectionGlobal.DirectionToChar[newDirection];
            DirectionChar = newDirectionChar;
            gamePlan.ChangeOnPosition(x, y, DirectionChar);
        }

        public void ChangeWishedDirection(Tuple<int, int> newWished)
        {
            WishedDirection = newWished;
        }

        public void TurnRight()
        {
            char newDirChar = DirectionGlobal.RightTurn(DirectionChar);
            Tuple<int, int> newDirection = DirectionGlobal.CharToDirection[newDirChar];
            gamePlan.ChangeOnPosition(x, y, newDirChar);
            DirectionChar = newDirChar;
            Direction = newDirection;
        }
        public void TurnLeft()
        {
            char newDirChar = DirectionGlobal.LeftTurn(DirectionChar);
            Tuple<int, int> newDirection = DirectionGlobal.CharToDirection[newDirChar];
            gamePlan.ChangeOnPosition(x, y, newDirChar);
            DirectionChar = newDirChar;
            Direction = newDirection;
        }
      
    }

    public static class DirectionGlobal
    {
        public static Dictionary<char, Tuple<int, int>> CharToDirection;
        public static Dictionary<Tuple<int, int>, char> DirectionToChar;

        public static Dictionary<Keys, Tuple<int, int>> KeyToDirection;

        static DirectionGlobal()
        {
            CharToDirection = new Dictionary<char, Tuple<int, int>>();
            CharToDirection.Add('>', new Tuple<int, int>(1, 0));
            CharToDirection.Add('<', new Tuple<int, int>(-1, 0));
            CharToDirection.Add('^', new Tuple<int, int>(0, -1));
            CharToDirection.Add('v', new Tuple<int, int>(0, 1));
            
            DirectionToChar = new Dictionary<Tuple<int, int>, char>();
            DirectionToChar.Add(new Tuple<int, int>(1, 0), '>');
            DirectionToChar.Add(new Tuple<int, int>(-1, 0), '<');
            DirectionToChar.Add(new Tuple<int, int>(0, -1), '^');
            DirectionToChar.Add(new Tuple<int, int>(0, 1), 'v');
            
            KeyToDirection = new Dictionary<Keys, Tuple<int, int>>();
            KeyToDirection.Add(Keys.Right, new Tuple<int, int>(1, 0));
            KeyToDirection.Add(Keys.Left, new Tuple<int, int>(-1, 0));
            KeyToDirection.Add(Keys.Up, new Tuple<int, int>(0, -1));
            KeyToDirection.Add(Keys.Down, new Tuple<int, int>(0, 1));
            
        }

        public static char RightTurn(char directionChar)
        {
            switch (directionChar)
            {
                case '>':
                    return 'v';
                    break;
                case 'v':
                    return '<';
                    break;
                case '<':
                    return '^';
                    break;
                case '^':
                    return '>';
                    break;
                default:
                    return directionChar;
            }
        }
        public static char LeftTurn(char directionChar)
        {
            switch (directionChar)
            {
                case '>':
                    return '^';
                    break;
                case '^':
                    return '<';
                    break;
                case '<':
                    return 'v';
                    break;
                case 'v':
                    return '>';
                    break;
                default:
                    return directionChar;
            }
        }
    }
}