using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public static class Global
    {
        public static int PICTURESIZE = 24;
        public static int DOTPOINTS = 10;
        public static int GHOSTPOINTS = 100;
        public const int GHOSTCOUNT = 4;
        
        public static char WALL = 'X';
        public static char FLOOR = ' ';
        public static char DOT = 'd';
        public static char BIGDOT = 'D';
        public static char RIGHT = '>';
        public static char LEFT = '<';
        public static char UP = '^';
        public static char DOWN = 'v';
        public static char DOOR = '-';
        
        public static bool ResetToPrepareFlag;
        
        public static Dictionary<char, Rectangle> ItemToSourceRectangle;
        public static Dictionary<int, Rectangle> GhostIdxToSourceRectangle;
        static Global()
        {
            ResetToPrepareFlag = false;
            
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
        public static bool countDownStarted;
        public static TimeSpan digitLastChanged;
        public static int displayIntervalSec;
        public static int curDigit;

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
            curDigit--;
            digitLastChanged = timeNow;
            return curDigit > 0;

        }
    }

    public enum GameMode
    {
        Start,
        Prepare,
        Play,
        Win,
        Lose
    }
    
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D startTexture;
        private Texture2D iconsTexture;
        private Texture2D textTexture;
        private Texture2D winTexture;
        private Texture2D loseTexture;

        private GamePlan gamePlan;
        private GameMode gameMode;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 25 * Global.PICTURESIZE;
            _graphics.PreferredBackBufferHeight = 26 * Global.PICTURESIZE;
            _graphics.ApplyChanges();
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gamePlan = new GamePlan("/Users/evgeniagolubeva/RiderProjects/pacman/pacman/map3.txt", 
                Global.DOTPOINTS, Global.GHOSTPOINTS,
                new int[] {12, 8}, new int[] {0, 0},
                new []{10, 10}, new []{24, 0},
                new []{11, 10}, new []{0, 22},
                new []{14, 11}, new []{24, 22});   
            //ToDo: Change to relative path
            gameMode = GameMode.Start; 
            
            this.TargetElapsedTime = new TimeSpan(0,0,0,0,180);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            startTexture = this.Content.Load<Texture2D>("start_frame");
            iconsTexture = this.Content.Load<Texture2D>("all_icons_0");
            textTexture = this.Content.Load<Texture2D>("letters_numbers");
            winTexture = this.Content.Load<Texture2D>("win_frame");
            loseTexture = this.Content.Load<Texture2D>("lose_frame");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            switch (gameMode)
            {
                case GameMode.Start:
                    if (state.GetPressedKeys().Length > 0)
                    {
                        gameMode = GameMode.Prepare;
                    }
                    break;
                case GameMode.Prepare:
                    if (!CountDown.countDownStarted)
                    {
                        CountDown.StartCountdown(gameTime.TotalGameTime);
                    }

                    if (CountDown.digitLastChanged.TotalSeconds + CountDown.displayIntervalSec <= gameTime.TotalGameTime.TotalSeconds)
                    {
                        if (CountDown.ChangeToNextDigit(gameTime.TotalGameTime))
                        {
                            // Display digit
                        }
                        else
                        {
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
                        gameMode = GameMode.Win;
                    }
                    else if (gamePlan.livesLeft == 0)
                    {
                        gameMode = GameMode.Lose;
                    }
                    else
                    {
                        var pressed = state.GetPressedKeys();
                        if (pressed.Length == 1)
                        {
                            Tuple<int, int> wished;
                            switch (pressed[0])
                            {
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
                                    wished = gamePlan.pacman.Direction;
                                    break;
                            }
                            gamePlan.pacman.ChangeWishedDirection(wished);

                        }
                        gamePlan.pacman.Move(gameTime.TotalGameTime);
                        MoveGhosts(gameTime.TotalGameTime);
                    }
                    break;
                case GameMode.Win:
                    if (state.GetPressedKeys().Length > 0)
                    {
                        gameMode = GameMode.Play;
                    }
                    break;
                
                case GameMode.Lose:
                    if (state.GetPressedKeys().Length > 0)
                    {
                        gameMode = GameMode.Play;
                    }
                    break;
                    
            }
            
            if (state.IsKeyDown(Keys.Escape))
                Exit();
            
            base.Update(gameTime);
        }

        public void MoveGhosts(TimeSpan timeNow)
        {
            foreach (var ghost in gamePlan.ghosts)
            {
                ghost.Move(timeNow);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            switch (gameMode)
            {
                case GameMode.Start:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(startTexture, Vector2.Zero, Color.White);
                    _spriteBatch.End();
                    break;
                
                case GameMode.Prepare:
                    drawGameplan();
                    drawLivesAndScore();
                    drawCountDown();
                    break;
                
                case GameMode.Play:
                    drawGameplan();
                    drawLivesAndScore();
                    break;
                case GameMode.Win:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(winTexture, Vector2.Zero, Color.White);
                    int textLine = 420;
                    int offsetX = 180 + 120 + 5*24;
                    drawScore(offsetX, textLine);
                    _spriteBatch.End();
                    break;
                case GameMode.Lose:
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(loseTexture, Vector2.Zero, Color.White);
                    _spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void drawGameplan()
        {
            _spriteBatch.Begin(); 
            // draw gameplan
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
                if (ghost.mode == GhostMode.Frightened)
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
            _spriteBatch.Begin();
            int textLine = 24 * Global.PICTURESIZE;
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
            _spriteBatch.Begin();
            int digit = CountDown.curDigit;
            Rectangle digitRect = SourceRectangle.numberRectangle(digit);
            _spriteBatch.Draw(textTexture, new Vector2(12 * Global.PICTURESIZE, 13 * Global.PICTURESIZE), digitRect, Color.White );
            _spriteBatch.End();
        }
    }

}