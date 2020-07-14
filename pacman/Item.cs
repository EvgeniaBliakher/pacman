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
        
        public int curPoints;
        public int dotsLeft;
        public int dotsEaten;
        public int livesLeft;
        
        public int dotPoints;
        public int ghostPoints;

        public int[] door;  

        public GamePlan(string pathToMap, int dotPoints, int ghostPoints, int[] redStart, int[] redHome, int[] pinkStart, int[] pinkHome,
            int[] blueStart, int[] blueHome, int[] yellowStart, int[] yellowHome)
        {
            readMapFromFile(pathToMap);
            this.dotPoints = dotPoints;
            this.ghostPoints = ghostPoints;
            this.curPoints = 0;
            dotsEaten = 0;
            livesLeft = 3;
            RedGhost red = new RedGhost(redStart[0], redStart[1], this, GhostMode.Chase, redHome[0], redHome[1]);
            PinkGhost pink = new PinkGhost(pinkStart[0], pinkStart[1], this, GhostMode.Wait, pinkHome[0], pinkHome[1]);
            
            YellowGhost yellow = new YellowGhost(yellowStart[0], yellowStart[1], this, GhostMode.Wait, yellowHome[0], yellowHome[1]);

            ghosts = new Ghost[] {red, pink, yellow};
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
                        case '-':
                            door = new int[2];
                            door[0] = x;
                            door[1] = y;
                            map[x, y] = Global.FLOOR;
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

        public bool IsPacman(char mapChar)
        {
            if (mapChar == Global.RIGHT || mapChar == Global.LEFT || mapChar == Global.UP || mapChar == Global.DOWN)
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
                if (IsFreeOrDot(mapChar) || IsPacman(mapChar))
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
        
        public void Move(TimeSpan timeNow)
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
            moveInDirection(timeNow);
        }
        private void moveInDirection(TimeSpan timeNow)
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
                    eat_dot(mapChar, timeNow);
                }
            }
        }

        private void eat_dot(char dotChar, TimeSpan timeNow)
        {
            gamePlan.curPoints += gamePlan.dotPoints;
            gamePlan.dotsLeft--;
            gamePlan.dotsEaten++;
            Console.WriteLine("cur points: " + gamePlan.curPoints);
            if (gamePlan.IsBigDot(dotChar))
            {
                // ToDo: pacman can eat the ghosts
                foreach (var ghost in gamePlan.ghosts)
                {
                    ghost.ChangeMode(GhostMode.Frightened, timeNow);
                }
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
        public Tuple<int, int> direction;
        public GhostMode startMode;
        public int startTileX;
        public int startTileY;
        public int homeTileX;
        public int homeTileY;

        public TimeSpan modeLastChanged;
        public const int chaseTimeSec = 20;
        public const int scatterTimeSec = 7;
        public const int frightenedTimeSec = 10;
        public int dotsEatenToStart;
        
        Random random = new Random();
        
        public Ghost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan)
        {
            this.mode = mode;
            this.startMode = mode;
            this.startTileX = x;
            this.startTileY = y;
            this.homeTileX = homeTileX;
            this.homeTileY = homeTileY;
            this.modeLastChanged = TimeSpan.FromSeconds(0);
            this.direction = new Tuple<int, int>(0, 0);
        }
        public abstract void Chase(TimeSpan timeNow);
        
        public void Move(TimeSpan timeNow)
        {
            switch (mode)
            {
                case GhostMode.Wait:
                    Wait(timeNow);
                    break;
                case GhostMode.Chase:
                    if (CheckTime(timeNow, modeLastChanged, chaseTimeSec))
                    {
                        ChangeMode(GhostMode.Scatter, timeNow);
                    }
                    else
                    {
                        Chase(timeNow);
                    }
                    break;
                case GhostMode.Scatter:
                    if (CheckTime(timeNow, modeLastChanged, scatterTimeSec))
                    {
                        ChangeMode(GhostMode.Chase, timeNow);
                    }
                    else
                    {
                        Scatter(timeNow);
                    }
                    break;
                case GhostMode.Frightened:
                    if (CheckTime(timeNow, modeLastChanged, frightenedTimeSec))
                    {
                        ChangeMode(GhostMode.Scatter, timeNow);    //ToDo: change back to mode before frightened
                    }
                    else
                    {
                        int a = random.Next(0, gamePlan.width);
                        int b = random.Next(0, gamePlan.height);
                        ChaseTarget(a, b, timeNow); 
                    }
                    break;
            }
        }
        public void Scatter(TimeSpan timeNow)
        {
            ChaseTarget(homeTileX, homeTileY, timeNow);
        }
        
        public void ChaseTarget(int targetX, int targetY, TimeSpan timeNow)
        {
            List<int[]> posibleMoves = gamePlan.FindFreeNeighbors(x, y);
            int smallestDistance = Int32.MaxValue;
            int[] bestMove = new []{0, 0};     // ToDo: ??? 
            Tuple<int, int> finalDir = new Tuple<int, int>(0, 0);
            if (posibleMoves.Count == 1)    // if ghost meets a dead end it can turn 180 degrees
            {
                bestMove = posibleMoves[0];
            }
            foreach (var tile in posibleMoves)
            {
                int distance = gamePlan.CountDistance(tile[0], tile[1], targetX, targetY);
                Tuple<int, int> potentialDir = new Tuple<int, int>(tile[0] - x, tile[1] - y);
                if (distance < smallestDistance && !IsOppositeDirection(potentialDir))
                {
                    smallestDistance = distance;
                    bestMove = tile;
                    finalDir = potentialDir;
                }
            }
            changeCoordinates(bestMove[0], bestMove[1], finalDir, timeNow);
        }

        public void GoToDoor(TimeSpan timeNow)
        {
            int targetX = gamePlan.door[0];
            int targetY = gamePlan.door[1] - 1;    // one tile above the door
            if (x != targetX || y != targetY)
            {
                ChaseTarget(targetX, targetY, timeNow);
            }
            else
            {
                ChangeMode(GhostMode.Chase, timeNow);
            }
        }

        public void Wait(TimeSpan timeNow)
        {
            if (gamePlan.dotsEaten >= dotsEatenToStart)
            {
                GoToDoor(timeNow);
            }
            else { /* wait */ }
        }
            
        private void changeCoordinates(int newX, int newY, Tuple<int, int> newDir, TimeSpan timeNow)
        {
            x = newX;
            y = newY;
            direction = newDir;
            if (newX == gamePlan.pacman.x && newY == gamePlan.pacman.y)
            {
                resolveGhostAndPacman(timeNow);
            }
        }

        private void resolveGhostAndPacman(TimeSpan timeNow)
        {
            if (mode == GhostMode.Frightened)
            {
                killGhost(timeNow);
            }
            else
            {
                killPacman(timeNow);
            }
        }
        private void killPacman(TimeSpan timeNow)
        {
            gamePlan.livesLeft--;
            resetGhosts(timeNow);
        }

        private void killGhost(TimeSpan timeNow)
        {
            resetGhost(this, timeNow);
            ChangeMode(GhostMode.Frightened, timeNow);
            gamePlan.curPoints += gamePlan.ghostPoints;
            Console.WriteLine("ghost eaten + " + Global.GHOSTPOINTS);
        }
        private void resetGhosts(TimeSpan timeNow)
        {
            foreach (var ghost in gamePlan.ghosts)
            {
                resetGhost(ghost, timeNow);
            }
            Global.ResetToPrepareFlag = true;
        }

        private void resetGhost(Ghost ghost, TimeSpan timeNow)
        {
            ghost.x = ghost.startTileX;
            ghost.y = ghost.startTileY;
            ghost.ChangeMode(ghost.startMode, timeNow);
        }

        public void FrightenGhosts(TimeSpan timeNow)
        {
            foreach (var ghost in gamePlan.ghosts)
            {
                ghost.ChangeMode(GhostMode.Frightened, timeNow);
            }
        }
        public bool CheckTime(TimeSpan timeNow, TimeSpan lastTime, int period)
        {
            if (timeNow.TotalSeconds > lastTime.TotalSeconds + period)
            {
                return true;
            }
            return false;
        }

        public void ChangeMode(GhostMode newMode, TimeSpan timeNow)
        {
            mode = newMode;
            modeLastChanged = timeNow;
        }

        public bool IsOppositeDirection(Tuple<int, int> newDir)
        {
            if (newDir.Item1 == direction.Item1 * -1 && newDir.Item2 == direction.Item2 * -1)
            {
                return true;
            }
            return false;
        }
    }

    public class RedGhost : Ghost
    {
        public RedGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
            dotsEatenToStart = 0;
        }

        public override void Chase(TimeSpan timeNow)
        {
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            ChaseTarget(pacmanX, pacmanY, timeNow);
        }

    }

    public class PinkGhost : Ghost
    {
        public PinkGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
            dotsEatenToStart = 0;
        }

        public override void Chase(TimeSpan timeNow)
        {
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            int targetX = pacmanX + gamePlan.pacman.Direction.Item1 * 4;  // 4 tiles before packman in direction of movement
            int targetY = pacmanY + gamePlan.pacman.Direction.Item2 * 4;
            ChaseTarget(targetX, targetY, timeNow);
        }
    }

    public class YellowGhost : Ghost
    {
        public const int minDistToPacman = 10;
        public YellowGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
            dotsEatenToStart = 30;
        }

        public override void Chase(TimeSpan timeNow)
        {
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            int pacmanDistance = gamePlan.CountDistance(pacmanX, pacmanY, x, y);
            if (pacmanDistance >= minDistToPacman)
            {
                ChaseTarget(pacmanX, pacmanY, timeNow);
            }
            else
            {
                Scatter(timeNow);
            }
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