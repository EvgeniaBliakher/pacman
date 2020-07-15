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
        public char[,] map;    // map representing game field, ghosts not on the map
        public int height;     // of game field
        public int width;
        public Pacman pacman;
        public Ghost[] ghosts;
        
        public int curPoints;
        public int dotsLeft;
        public int dotsEaten;
        public int livesLeft;
        
        public int dotPoints;
        public int ghostPoints;

        public int[] door;  // coordinates of the door to ghosts' home

        public GamePlan(string pathToMap, int dotPoints, int ghostPoints, int[] redStart, int[] redHome, int[] pinkStart, int[] pinkHome,
            int[] blueStart, int[] blueHome, int[] yellowStart, int[] yellowHome)
        {
            // reads map from file and creates 4 ghosts according to initial data
            // redStart - coordinates of the red ghost starting tile
            // redHome - coordinates of the red ghost home - tile where it goes in Scatter mode
            ReadMapFromFile(pathToMap);
            this.dotPoints = dotPoints;
            this.ghostPoints = ghostPoints;
            this.curPoints = 0;
            dotsEaten = 0;
            livesLeft = Global.MAXLIVES;
            RedGhost red = new RedGhost(redStart[0], redStart[1], this, GhostMode.Chase, redHome[0], redHome[1]);
            PinkGhost pink = new PinkGhost(pinkStart[0], pinkStart[1], this, GhostMode.Wait, pinkHome[0], pinkHome[1]);
            BlueGhost blue = new BlueGhost(blueStart[0], blueStart[1], this, GhostMode.Wait, blueHome[0], blueHome[1]);
            YellowGhost yellow = new YellowGhost(yellowStart[0], yellowStart[1], this, GhostMode.Wait, yellowHome[0], yellowHome[1]);

            ghosts = new Ghost[] {red, pink, blue, yellow};
        }

        public void ReadMapFromFile(string pathToFile)
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
            // mapChar - char in map from game plan
            if (mapChar == Global.FLOOR)
            {
                return true;
            }
            return false;
        }

        public bool IsDot(char mapChar)
        {
            // mapChar - char in map from game plan
            if (mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }

        public bool IsBigDot(char mapChar)
        {
            // mapChar - char in map from game plan
            if (mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }
        public bool IsFreeOrDot(char mapChar)
        {
            // mapChar - char in map from game plan
            if (mapChar == Global.FLOOR || mapChar == Global.DOT || mapChar == Global.BIGDOT)
            {
                return true;
            }
            return false;
        }

        public bool IsPacman(char mapChar)
        {
            // mapChar - char in map from game plan
            // returns true for pacman in any direction
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
            // move item from one tile to another
            // source tile is replaced with floor tile
            char mapChar = map[fromX, fromY];
            map[fromX, fromY] = Global.FLOOR;
            map[toX, toY] = mapChar;
        }

        public void ChangeOnPosition(int x, int y, char newChar)
        {
            // change tile on coordinates
            map[x, y] = newChar;
        }

        public int CountDistance(int x, int y, int a, int b)
        {
            // calculates distance using manhattan metric 
            return Math.Abs(x - a) + Math.Abs(y - b);
        }

        public List<int[]> FindFreeNeighbors(int x, int y)
        {
            // looks at tiles up, down, right, left
            // return free tiles or occupied by pacman as list of their coordinates
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
        // generic item on the game plan
        public int x;    // coordinates on the map in game plan
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
        // representing pacman in the game plan
        public Tuple<int, int>  Direction; // direction in x and y, one of them always zero - no diagonal movement
        public char DirectionChar;    // >, <, ^ or v
        private Tuple<int, int> WishedDirection;    // direction pacman wants to change to

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
        }
        
        public void Move(TimeSpan timeNow)
        {
            // if current direction is different from wished dirrection
            // tries to change direction
            // then moves one step in direction
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
            // moves in direction if the tile is free
            // if new tile contains dot, eats dot
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
            // adds points for eating dot
            // if it's a big dot, changes ghosts to Frightened mode
            gamePlan.curPoints += gamePlan.dotPoints;
            gamePlan.dotsLeft--;
            gamePlan.dotsEaten++;
            Console.WriteLine("cur points: " + gamePlan.curPoints);    
            if (gamePlan.IsBigDot(dotChar))
            {
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
        Wait,        // wait in ghosts' home
        Chase,       // chase pacman
        Scatter,     // go to the home tile
        Frightened   // move chaotically, pacman can eat ghosts
    }
    public abstract class Ghost : Item
    {
        public GhostMode mode;
        public Tuple<int, int> direction;    // direction in x and y, one of them always zero - no diagonal movement
        public GhostMode startMode;          // mode to which ghost is turned in the begining of each level
        public int startTileX;
        public int startTileY;
        public int homeTileX;
        public int homeTileY;

        public TimeSpan modeLastChanged;
        public int dotsEatenToStart;            // until this much dots are eaten, ghost in Wait mode
        
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
        public abstract void Chase(TimeSpan timeNow);    // each kind of ghost has different chase logic
        
        public void Move(TimeSpan timeNow)
        {
            // moves ghosts for one step according to ghost mode
            // checks time to change modes
            switch (mode)
            {
                case GhostMode.Wait:
                    Wait(timeNow);
                    break;
                case GhostMode.Chase:
                    if (CheckTime(timeNow, modeLastChanged, Global.chaseTimeSec))
                    {
                        ChangeMode(GhostMode.Scatter, timeNow);
                    }
                    else
                    {
                        Chase(timeNow);
                    }
                    break;
                case GhostMode.Scatter:
                    if (CheckTime(timeNow, modeLastChanged, Global.scatterTimeSec))
                    {
                        ChangeMode(GhostMode.Chase, timeNow);
                    }
                    else
                    {
                        Scatter(timeNow);
                    }
                    break;
                case GhostMode.Frightened:
                    if (CheckTime(timeNow, modeLastChanged, Global.frightenedTimeSec))
                    {
                        ChangeMode(GhostMode.Scatter, timeNow);    //ToDo: change back to mode before frightened
                    }
                    else
                    {
                        // ghost chooses a random tile on the game field as chase target
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
            // gets all posible tiles to move to
            // calculates distance from each tile to target tile
            // chooses the tile with least distance
            // but ghost can't turn change direction for 180 degrees -> IsOppositeDirection function
            
            List<int[]> posibleMoves = gamePlan.FindFreeNeighbors(x, y);
            int smallestDistance = Int32.MaxValue;
            int[] bestMove = new []{0, 0};     
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
            // exits ghosts' home through the door
            // when out starts chasing pacman
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
            // waits until enough dots are eaten, the exits th ghosts' home
            if (gamePlan.dotsEaten >= dotsEatenToStart)
            {
                GoToDoor(timeNow);
            }
            else { /* wait */ }
        }
            
        private void changeCoordinates(int newX, int newY, Tuple<int, int> newDir, TimeSpan timeNow)
        {
            // moves to new tile
            // resolves conflicts - when ghost and pacman are on the same tile
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
            // in Frightened mode - pacman kills ghost
            // in other modes ghost kills pacman
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
            // all ghosts are reset to start positions
            gamePlan.livesLeft--;
            resetGhosts(timeNow);
        }

        private void killGhost(TimeSpan timeNow)
        {
            // only killed ghost is reset to start
            ResetGhost(timeNow);
            ChangeMode(GhostMode.Frightened, timeNow);
            gamePlan.curPoints += gamePlan.ghostPoints;
            Console.WriteLine("ghost eaten + " + Global.GHOSTPOINTS);
        }
        private void resetGhosts(TimeSpan timeNow)
        {
            // reset all ghost
            // set prepare flag -> game should be turned to Prepare mode
            foreach (var ghost in gamePlan.ghosts)
            {
                ghost.ResetGhost(timeNow);
            }
            Global.ResetToPrepareFlag = true;
        }

        public void ResetGhost(TimeSpan timeNow)
        {
            this.x = this.startTileX;
            this.y = this.startTileY;
            this.ChangeMode(this.startMode, timeNow);
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
            // check if certain time period has pased
            // period in seconds
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
            // checks if given direction is right opposite to current direction
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
            // has pacman as target
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
            // chase target 3 tiles before packman in direction of movement
            int pacmanX = gamePlan.pacman.x;
            int pacmanY = gamePlan.pacman.y;
            int targetX = pacmanX + gamePlan.pacman.Direction.Item1 * 3;  
            int targetY = pacmanY + gamePlan.pacman.Direction.Item2 * 3;
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
            // when far away - chase same as red ghost
            // when crosses the minimal distance to pacman, changes target to home tile as in Scatter
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

    public class BlueGhost : Ghost
    {
        public const int minDistToPacman = 10;
        public BlueGhost(int x, int y, GamePlan gamePlan, GhostMode mode, int homeTileX, int homeTileY) : base(x, y, gamePlan, mode, homeTileX, homeTileY)
        {
            dotsEatenToStart = 45;
        }

        public override void Chase(TimeSpan timeNow)
        {
            // chase the same as yellow ghost
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
        // everithing concerning direction
        public static Dictionary<char, Tuple<int, int>> CharToDirection;
        public static Dictionary<Tuple<int, int>, char> DirectionToChar;

        public static Dictionary<Keys, Tuple<int, int>> KeyToDirection;    // get direction from pressed key

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