using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public class GamePlan
    {
        public char[,] map;
        public int height;
        public int width;
        public Pacman pacman;
        public Ghost[] ghosts;

        public int dotPoints;
        public int curPoints;
        public int dotsLeft;
        public int livesLeft;
        

        public GamePlan(string pathToMap, int dotPoints, int[] redStart, int[] redHome, int[] pinkStart, int[] pinkHome,
            int[] blueStart, int[] blueHome, int[] yellowStart, int[] yellowHome)
        {
            readMapFromFile(pathToMap);
            this.dotPoints = dotPoints;
            this.curPoints = 0;
            livesLeft = 3;
            RedGhost red = new RedGhost(redStart[0], redStart[1], this, GhostMode.Chase, redHome[0], redHome[1]);
            PinkGhost pink = new PinkGhost(pinkStart[0], pinkStart[1], this, GhostMode.Chase, pinkHome[0], pinkHome[1]);

            ghosts = new Ghost[] {red, pink};
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
                        case 'd':
                        case 'D':
                            dotsLeft++;
                            break;
                    }
                }
            }
        }

        public bool IsFree(char mapChar)
        {
            if (mapChar == Global.FLOOR)
            {
                return true;
            }
            return false;
        }

        public bool IsDot(char mapChar)
        {
            if (mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }

        public bool IsBigDot(char mapChar)
        {
            if (mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }
        public bool IsFreeOrDot(char mapChar)
        {
            if (mapChar == Global.FLOOR || mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }

        public char what_is_on_position(int x, int y)
        {
            char mapChar = map[x, y];
            return mapChar;
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

        public int CountDistance(int x, int y, int a, int b)
        {
            return Math.Abs(x - a) + Math.Abs(y - b);
        }

        public List<int[]> FindFreeNeighbors(int x, int y)
        {
            int[][] potentialNeighbors = new[] {new[] {x - 1, y}, new[] {x + 1, y}, new[] {x, y - 1}, new[] {x, y + 1}};
            List<int[]> result = new List<int[]>();
            foreach (var neighbor in potentialNeighbors)
            {
                char mapChar = map[neighbor[0], neighbor[1]];
                if (IsFreeOrDot(mapChar))
                {
                    result.Add(neighbor);
                }
            }

            return result;
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

    public class Pacman : Item
    {
        public Tuple<int, int>  Direction; // direction in x and y, one of them always zero - no diagonal movement
        public char DirectionChar;
        private Tuple<int, int> WishedDirection;

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
                char mapChar = gamePlan.what_is_on_position(xInDir, yInDir);
                if (gamePlan.IsFreeOrDot(mapChar))
                {
                    changeDirection(WishedDirection);
                }
            }
            moveInDirection();
        }
        private void moveInDirection()
        {
            int newX = x + Direction.Item1;
            int newY = y + Direction.Item2;
            char mapChar = gamePlan.what_is_on_position(newX, newY);
            if (gamePlan.IsFreeOrDot(mapChar))
            {
                gamePlan.Move(x, y, newX, newY);
                x = newX;
                y = newY;
                if (gamePlan.IsDot(mapChar))
                {
                    eat_dot(mapChar);
                }
            }
        }

        private void eat_dot(char dotChar)
        {
            gamePlan.curPoints += gamePlan.dotPoints;
            gamePlan.dotsLeft--;
            Console.WriteLine("cur points: " + gamePlan.curPoints);
            if (gamePlan.IsBigDot(dotChar))
            {
                // ToDo: pacman can eat the ghosts
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

    }

    public enum GhostMode
    {
        Wait,
        Chase,
        Scatter,
        Frightened
    }
    public abstract class Ghost : Item
    {
        public GhostMode mode;
        public int homeTileX;
        public int homeTileY;

        public TimeSpan modeLastChanged;
        public const int chaseTimeSec = 20;
        public const int scatterTimeSec = 7;
        
        public Ghost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan)
        {
            this.mode = mode;
            this.homeTileX = homeTileX;
            this.homeTileY = homeTileY;
            this.modeLastChanged = TimeSpan.FromSeconds(0);
        }
        public abstract void Chase();
        
        public void Move(TimeSpan timeNow)
        {
            switch (mode)
            {
                case GhostMode.Chase:
                    if (CheckTime(timeNow, modeLastChanged, chaseTimeSec))
                    {
                        changeMode(GhostMode.Scatter, timeNow);
                    }
                    else
                    {
                        Chase();
                    }
                    break;
                case GhostMode.Scatter:
                    if (CheckTime(timeNow, modeLastChanged, scatterTimeSec))
                    {
                        changeMode(GhostMode.Chase, timeNow);
                    }
                    else
                    {
                        Scatter();
                    }
                    break;
                case GhostMode.Frightened:
                    break;
            }
        }
        public void Scatter()
        {
            ChaseTarget(homeTileX, homeTileY);
        }
        
        public void ChaseTarget(int targetX, int targetY)
        {
            /*int targetX = gamePlan.pacman.x;
            int targetY = gamePlan.pacman.y;*/
            List<int[]> posibleMoves = gamePlan.FindFreeNeighbors(x, y);
            int smallestDistance = Int32.MaxValue;
            int[] bestMove = new []{23, 23};     // ToDo: ??? 
            foreach (var tile in posibleMoves)
            {
                int distance = gamePlan.CountDistance(tile[0], tile[1], targetX, targetY);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    bestMove = tile;
                }
            }
            changeCoordinates(bestMove[0], bestMove[1]);
        }
        private void changeCoordinates(int newX, int newY)
        {
            x = newX;
            y = newY;
        }
        public bool CheckTime(TimeSpan timeNow, TimeSpan lastTime, int period)
        {
            if (timeNow.TotalSeconds > lastTime.TotalSeconds + period)
            {
                return true;
            }
            return false;
        }

        private void changeMode(GhostMode newMode, TimeSpan timeNow)
        {
            mode = newMode;
            modeLastChanged = timeNow;
        }
    }

    public class RedGhost : Ghost
    {
        public RedGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
        }

        public override void Chase()
        {
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            ChaseTarget(pacmanX, pacmanY);
        }

    }

    public class PinkGhost : Ghost
    {
        public PinkGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
        }

        public override void Chase()
        {
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            int targetX = pacmanX + gamePlan.pacman.Direction.Item1 * 4;  // 4 tiles before packman in direction of movement
            int targetY = pacmanY + gamePlan.pacman.Direction.Item2 * 4;
            ChaseTarget(targetX, targetY);
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
    }
}