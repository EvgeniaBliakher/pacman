using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public static class Global
    {
        public static int PICTURESIZE = 24;          // size of 1 tile in pixels
        public static int DOTPOINTS = 10;            // points for 1 dot  
        public static int GHOSTPOINTS = 100;         // points for eating a ghost
        public const int GHOSTCOUNT = 4;
        public const int MAXLIVES = 3;

        public static int chaseTimeSec;
        public static int scatterTimeSec;
        public static int frightenedTimeSec;
        
        public static char WALL = 'X';
        public static char FLOOR = ' ';
        public static char DOT = 'd';
        public static char BIGDOT = 'D';
        public static char RIGHT = '>';
        public static char LEFT = '<';
        public static char UP = '^';
        public static char DOWN = 'v';
        public static char DOOR = '-';
        
        public static bool ResetToPrepareFlag;    // when true game should be reset to Prepare mode
        
        // /Users/evgeniagolubeva/RiderProjects/pacman/pacman/
        public const string PATH_TO_MAP_FILE = "map3.txt";
        public const string PATH_TO_DATA_FILE = "level_data.txt";
        
        public static Dictionary<char, Rectangle> ItemToSourceRectangle;
        public static Dictionary<int, Rectangle> GhostIdxToSourceRectangle;
        static Global()
        {
            ResetToPrepareFlag = false;

            chaseTimeSec = 0;
            scatterTimeSec = 0;
            frightenedTimeSec = 0;
            
            ItemToSourceRectangle = new Dictionary<char, Rectangle>();
            ItemToSourceRectangle.Add(WALL, SourceRectangle.wall);
            ItemToSourceRectangle.Add(FLOOR, SourceRectangle.floor);
            ItemToSourceRectangle.Add(DOT, SourceRectangle.dot);
            ItemToSourceRectangle.Add(BIGDOT, SourceRectangle.bigDot);
            ItemToSourceRectangle.Add(RIGHT, SourceRectangle.right);
            ItemToSourceRectangle.Add(LEFT, SourceRectangle.left);
            ItemToSourceRectangle.Add(UP, SourceRectangle.up);
            ItemToSourceRectangle.Add(DOWN, SourceRectangle.down);
            
            GhostIdxToSourceRectangle = new Dictionary<int, Rectangle>();
            GhostIdxToSourceRectangle.Add(0, SourceRectangle.red);
            GhostIdxToSourceRectangle.Add(1, SourceRectangle.pink);
            GhostIdxToSourceRectangle.Add(2, SourceRectangle.blue);
            GhostIdxToSourceRectangle.Add(3, SourceRectangle.yellow);
        }
    }
    
    public static class SourceRectangle
    {
        // source rectangles of the objects in a texture - cut out from the whole picture
        // needed for Draw functions
        
        public static Rectangle wall = new Rectangle(0,0,24,24);
        public static Rectangle floor = new Rectangle(24,0,24,24);
        public static Rectangle dot = new Rectangle(48,0,24,24);
        public static Rectangle bigDot = new Rectangle(72,0,24,24);
        public static Rectangle right = new Rectangle(96,0,24,24);
        public static Rectangle left = new Rectangle(120,0,24,24);
        public static Rectangle up = new Rectangle(144,0,24,24);
        public static Rectangle down = new Rectangle(168,0,24,24);
        
        public static Rectangle red = new Rectangle(192, 0, 24, 24);
        public static Rectangle pink = new Rectangle(216, 0, 24, 24);
        public static Rectangle blue = new Rectangle(240, 0, 24, 24);
        public static Rectangle yellow = new Rectangle(264, 0, 24, 24);
        public static Rectangle frightened = new Rectangle(288, 0, 24, 24);

        public static Rectangle score = new Rectangle(0, 0, 120, 24);
        public static Rectangle lives = new Rectangle(120, 0, 120, 24);

        public static Rectangle numberRectangle(int num)
        {
            if (num >= 0 && num <= 9)
            {
                int x = num * Global.PICTURESIZE;
                int y = Global.PICTURESIZE;
                return new Rectangle(x, y, Global.PICTURESIZE, Global.PICTURESIZE);
            }
            return new Rectangle(0,0,0,0);
        }
    }

    public static class CountDown
    {
        // countdown displayed in Prepare game mode 
        
        public static bool countDownStarted;        // if countdown is running
        public static TimeSpan digitLastChanged;    
        public static int displayIntervalSec;       // for how long digit is displayed
        public static int curDigit;                 // current digit displayed

        static CountDown()
        {
            countDownStarted = false;
            digitLastChanged = TimeSpan.Zero;
            displayIntervalSec = 1;
            curDigit = 3;
        }

        public static void StartCountdown(TimeSpan timeNow)
        {
            countDownStarted = true;
            digitLastChanged = timeNow;
            curDigit = 3;
        }

        public static bool ChangeToNextDigit(TimeSpan timeNow)
        {
            // returns false if countdown reaches zero - time to stop countdown
            curDigit--;
            digitLastChanged = timeNow;
            return curDigit > 0;

        }
    }

    public enum GameMode
    {
        Start,        // start frame displayed, first level loaded
        Prepare,      // ghosts are reset to start positions, countdown dislayed
        Play,         // game runs
        Win,          // win frame with score displayed, player can continue to new level
        Lose          // lose frame displayed, player can repeat the current level
    }
    
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D startTexture;        // start frame
        private Texture2D iconsTexture;        // tiles - game field, pacman, ghosts
        private Texture2D textTexture;         // text and numbers
        private Texture2D winTexture;          // win frame
        private Texture2D loseTexture;         // lose frame

        private GamePlan gamePlan;
        private GameMode gameMode;

        private LevelData levelData;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 25 * Global.PICTURESIZE;
            _graphics.PreferredBackBufferHeight = 26 * Global.PICTURESIZE;
            _graphics.ApplyChanges();
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gamePlan = new GamePlan(Global.PATH_TO_MAP_FILE, Global.DOTPOINTS, Global.GHOSTPOINTS,
                new int[] {12, 8}, new int[] {0, 0},
                new []{10, 10}, new []{24, 0},
                new []{11, 10}, new []{0, 22},
                new []{14, 11}, new []{24, 22});   
            //ToDo: Change to relative path
            gameMode = GameMode.Start; 
            
            levelData = new LevelData(Global.PATH_TO_DATA_FILE);
            
            this.TargetElapsedTime = new TimeSpan(0,0,0,0,180); // how often update is called
        }

        protected override void Initialize()
        {
            // I don't use initialize

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // loads pictures from content folder to textures
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            startTexture = this.Content.Load<Texture2D>("start_frame");
            iconsTexture = this.Content.Load<Texture2D>("all_icons_0");
            textTexture = this.Content.Load<Texture2D>("letters_numbers");
            winTexture = this.Content.Load<Texture2D>("win_frame");
            loseTexture = this.Content.Load<Texture2D>("lose_frame");
        }

        protected override void Update(GameTime gameTime)
        {
            // updates the game state according to current game mode
            
            KeyboardState keybordState = Keyboard.GetState();

            switch (gameMode)
            {
                case GameMode.Start:
                    if (keybordState.GetPressedKeys().Length > 0)    // waits for any pressed key
                    {
                        levelData.GetNextLevelData();
                        gameMode = GameMode.Prepare;
                    }
                    break;
                case GameMode.Prepare:
                    if (!CountDown.countDownStarted)
                    {
                        CountDown.StartCountdown(gameTime.TotalGameTime);
                    }
                    // if digit is displayed for the display time, change to next digit
                    if (CountDown.digitLastChanged.TotalSeconds + CountDown.displayIntervalSec <= gameTime.TotalGameTime.TotalSeconds)
                    {
                        if (CountDown.ChangeToNextDigit(gameTime.TotalGameTime))
                        {
                            // Display digit
                        }
                        else
                        {
                            // count down reached zero -> stop countdown
                            CountDown.countDownStarted = false;
                            gameMode = GameMode.Play;
                        }
                    }
                    break;
                case GameMode.Play:
                    if (Global.ResetToPrepareFlag && gamePlan.livesLeft > 0)    
                    {
                        gameMode = GameMode.Prepare;
                        Global.ResetToPrepareFlag = false;
                    }
                    else if (gamePlan.dotsLeft == 0 && gamePlan.livesLeft != 0)
                    {
                        // pacman ate all dots -> victory
                        gameMode = GameMode.Win;
                    }
                    else if (gamePlan.livesLeft == 0)
                    {
                        // pacman is killed by a ghost - loses the last life
                        gameMode = GameMode.Lose;
                    }
                    else
                    {
                        // game runs
                        // checks for pressed keys then moves pacman and ghosts
                        var pressed = keybordState.GetPressedKeys();
                        if (pressed.Length == 1)
                        {
                            Tuple<int, int> wished;    // direction player wants pacman to take
                            switch (pressed[0])
                            {
                                // wished direction asigned according to arrow keys
                                case Keys.Right: 
                                    wished = DirectionGlobal.KeyToDirection[Keys.Right];
                                    break;
                                case Keys.Left:
                                    wished = DirectionGlobal.KeyToDirection[Keys.Left];
                                    break;
                                case Keys.Up:
                                    wished = DirectionGlobal.KeyToDirection[Keys.Up];
                                    break;
                                case Keys.Down:
                                    wished = DirectionGlobal.KeyToDirection[Keys.Down];
                                    break;
                                default:
                                    wished = gamePlan.pacman.Direction;    // if a wrong key is pressed, direction stayes the same
                                    break;
                            }
                            gamePlan.pacman.ChangeWishedDirection(wished);    

                        }
                        gamePlan.pacman.Move(gameTime.TotalGameTime);
                        MoveGhosts(gameTime.TotalGameTime);
                    }
                    break;
                case GameMode.Win:
                    if (keybordState.GetPressedKeys().Length > 0)
                    {
                        // next level data loaded
                        PrepareNextLevel(true, Global.PATH_TO_MAP_FILE, gameTime.TotalGameTime);
                        gameMode = GameMode.Prepare;
                    }
                    break;
                
                case GameMode.Lose:
                    if (keybordState.GetPressedKeys().Length > 0)
                    {
                        // uses level data of the same level
                        PrepareNextLevel(false, Global.PATH_TO_MAP_FILE, gameTime.TotalGameTime);
                        gameMode = GameMode.Prepare;
                    }
                    break;
                    
            }
            
            if (keybordState.IsKeyDown(Keys.Escape))    // exits the game if esc key pressed
                Exit();
            
            base.Update(gameTime);
        }

        public void PrepareNextLevel(bool getNextLevel, string pathToMapFile, TimeSpan timeNow)
        {
            // prepares game plan, resets ghosts to start tiles and modes
            // getNextLevel   - true -> data of next level loaded
            //                - false -> continue with same level data
            prepareGamePlan(pathToMapFile);
            if (getNextLevel)
            {
                levelData.GetNextLevelData();
            }
            foreach (var ghost in gamePlan.ghosts)
            {
                ghost.ResetGhost(timeNow);
            }
        }
        
        private void prepareGamePlan(string pathToMapFile)
        {
            // sets game plan to initial settings
            // loads game field from file and sets points and lives
            gamePlan.ReadMapFromFile(pathToMapFile);
            gamePlan.dotsEaten = 0;
            gamePlan.curPoints = 0;
            gamePlan.livesLeft = Global.MAXLIVES;
        }
        public void MoveGhosts(TimeSpan timeNow)
        {
            // moves all ghosts for one step
            foreach (var ghost in gamePlan.ghosts)
            {
                ghost.Move(timeNow);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // draws game state to screen according to game mode
            GraphicsDevice.Clear(Color.Black);
            switch (gameMode)
            {
                case GameMode.Start:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(startTexture, Vector2.Zero, Color.White);    // start frame
                    _spriteBatch.End();
                    break;
                
                case GameMode.Prepare:
                    drawGameplan();        // draws initial state of game field
                    drawLivesAndScore();    
                    drawCountDown();        // shows countdown
                    break;
                
                case GameMode.Play:
                    drawGameplan();
                    drawLivesAndScore();
                    break;
                case GameMode.Win:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(winTexture, Vector2.Zero, Color.White);    // win frame
                    int textLine = 420;
                    int offsetX = 180 + 120 + 5*24;
                    drawScore(offsetX, textLine);    // draws score in line with the word score on the win frame
                    _spriteBatch.End();
                    break;
                case GameMode.Lose:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(loseTexture, Vector2.Zero, Color.White);    // lose frame
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void drawGameplan()
        {
            // draws using icons texture
            _spriteBatch.Begin(); 
            // draw gameplan
            // goes through map in game plan and draws each tile
            int offsetX = 0; 
            int offsetY = 0; 
            for (int x = 0; x < gamePlan.width; x++)   
            {
                for (int y = 0; y < gamePlan.height; y++)
                {
                    char mapChar = gamePlan.map[x, y];
                    Rectangle source = Global.ItemToSourceRectangle[mapChar];
                    _spriteBatch.Draw(iconsTexture, new Vector2(offsetX, offsetY), sourceRectangle: source, Color.White);
                    offsetY += Global.PICTURESIZE;
                }
                offsetY = 0;
                offsetX += Global.PICTURESIZE;
            }
            // draw icons for lives left
            int lives = gamePlan.livesLeft;
            Rectangle pacman = Global.ItemToSourceRectangle[Global.LEFT];
            int livesX = 7 * Global.PICTURESIZE;
            int livesY = 24 * Global.PICTURESIZE;
            for (int i = 0; i < lives; i++)
            {
                _spriteBatch.Draw(iconsTexture, new Vector2(livesX, livesY), pacman, Color.White );
                livesX += Global.PICTURESIZE;
            }
            // draw ghosts
            for (int i = 0; i < Global.GHOSTCOUNT; i++)
            {
                Ghost ghost = gamePlan.ghosts[i];
                int ghostX = ghost.x * Global.PICTURESIZE;
                int ghostY = ghost.y * Global.PICTURESIZE;
                Rectangle ghostRect = Global.GhostIdxToSourceRectangle[i];
                if (ghost.mode == GhostMode.Frightened)    // in frightened mode frightened ghost icon is used for all ghosts
                {
                    _spriteBatch.Draw(iconsTexture, new Vector2(ghostX, ghostY), SourceRectangle.frightened, Color.White );
                }
                else
                {
                    _spriteBatch.Draw(iconsTexture, new Vector2(ghostX, ghostY), ghostRect, Color.White );   
                }
            }

            _spriteBatch.End();
        }

        private void drawLivesAndScore()
        {
            // draws words "lives" and "score", draws score
            _spriteBatch.Begin();
            int textLine = 24 * Global.PICTURESIZE;    // offset on Y axis of the line with text
            _spriteBatch.Draw(textTexture, new Vector2(1 * Global.PICTURESIZE, textLine), SourceRectangle.lives, Color.White );
            _spriteBatch.Draw(textTexture, new Vector2(13 * Global.PICTURESIZE, textLine), SourceRectangle.score, Color.White);

            if (gamePlan.curPoints == 0) 
            {
                Rectangle zero = SourceRectangle.numberRectangle(0);
                _spriteBatch.Draw(textTexture, new Vector2(19 * Global.PICTURESIZE, textLine),  zero, Color.White);
            }
            else
            {
                int points = gamePlan.curPoints;
                int offset = 23 * Global.PICTURESIZE;
                drawScore(offset, textLine);
            }
            _spriteBatch.End();
        }

        private void drawScore(int offsetX, int textLine)
        {
            // draws number at a given offset
            // offset of the LAST digit given
            int points = gamePlan.curPoints;
            while (points > 0)
            {
                int digit = points % 10;
                Rectangle digitRect = SourceRectangle.numberRectangle(digit);
                _spriteBatch.Draw(textTexture, new Vector2(offsetX, textLine), digitRect, Color.White );
                offsetX -= Global.PICTURESIZE;
                points /= 10;
            }
        }

        private void drawCountDown()
        {
            // draw current digit of countdown based on CountDown class
            _spriteBatch.Begin();
            int digit = CountDown.curDigit;
            Rectangle digitRect = SourceRectangle.numberRectangle(digit);
            _spriteBatch.Draw(textTexture, new Vector2(12 * Global.PICTURESIZE, 13 * Global.PICTURESIZE), digitRect, Color.White );
            _spriteBatch.End();
        }
    }

}