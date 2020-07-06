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
        
        public static char WALL = 'X';
        public static char FLOOR = ' ';
        public static char DOT = 'd';
        public static char BIGDOT = 'D';
        public static char RIGHT = '>';
        public static char LEFT = '<';
        public static char UP = '^';
        public static char DOWN = 'v';
        
        public static Dictionary<char, Rectangle> ItemToSourceRectangle;
        static Global()
        {
            ItemToSourceRectangle = new Dictionary<char, Rectangle>();
            ItemToSourceRectangle.Add(WALL, SourceRectangle.wall);
            ItemToSourceRectangle.Add(FLOOR, SourceRectangle.floor);
            ItemToSourceRectangle.Add(DOT, SourceRectangle.dot);
            ItemToSourceRectangle.Add(BIGDOT, SourceRectangle.bigDot);
            ItemToSourceRectangle.Add(RIGHT, SourceRectangle.right);
            ItemToSourceRectangle.Add(LEFT, SourceRectangle.left);
            ItemToSourceRectangle.Add(UP, SourceRectangle.up);
            ItemToSourceRectangle.Add(DOWN, SourceRectangle.down);
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
    }

    public enum GameMode
    {
        Start,
        Play
    }
    
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D texture;
        private Texture2D startTexture;

        private GamePlan gamePlan;
        private GameMode gameMode;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gamePlan = new GamePlan("/Users/evgeniagolubeva/RiderProjects/pacman/pacman/map.txt");   
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

            // TODO: use this.Content to load your game content here
            
            texture = this.Content.Load<Texture2D>("all_icons");
            startTexture = this.Content.Load<Texture2D>("press_key");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            switch (gameMode)
            {
                case GameMode.Start:
                    if (state.GetPressedKeys().Length > 0)
                    {
                        gameMode = GameMode.Play;
                    }
                    break;
                case GameMode.Play:
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
                    gamePlan.pacman.Move();
                    break;
            }
            
            if (state.IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
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
                
                case GameMode.Play:
                    drawGameplan();
                    break;
            }

            base.Draw(gameTime);
        }

        private void drawGameplan()
        {
            _spriteBatch.Begin(); 
            
            int offsetX = 0; 
            int offsetY = 0; 
            for (int x = 0; x < gamePlan.width; x++)
            {
                for (int y = 0; y < gamePlan.height; y++)
                {
                    char mapChar = gamePlan.map[x, y];
                    Rectangle source = Global.ItemToSourceRectangle[mapChar];
                    _spriteBatch.Draw(texture, new Vector2(offsetX, offsetY), sourceRectangle: source, Color.White);
                    offsetY += Global.PICTURESIZE;
                }
                offsetY = 0;
                offsetX += Global.PICTURESIZE;
            }
            _spriteBatch.End();
        }
    }

}