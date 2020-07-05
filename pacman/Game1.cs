using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pacman
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D texture;

        private GamePlan gamePlan;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            gamePlan = new GamePlan("/Users/evgeniagolubeva/RiderProjects/pacman/pacman/map.txt");
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
            
            texture = this.Content.Load<Texture2D>("wall_floor_dots");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
           // _spriteBatch.Draw(texture, Vector2.Zero, sourceRectangle: Global.ItemToSourceRectangle['D'] ,Color.White);
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

            base.Draw(gameTime);
        }
    }

    public static class SourceRectangle
    {
        public static Rectangle wall = new Rectangle(0,0,24,24);
        public static Rectangle floor = new Rectangle(24,0,24,24);
        public static Rectangle dot = new Rectangle(48,0,24,24);
        public static Rectangle bigDot = new Rectangle(72,0,24,24);
        
    }

    public static class Global
    {
        public static int PICTURESIZE = 24;
        
        public static char WALL = 'X';
        public static char FLOOR = ' ';
        public static char DOT = 'd';
        public static char BIGDOT = 'D';

        public static Dictionary<char, Rectangle> ItemToSourceRectangle;
        static Global()
        {
            ItemToSourceRectangle = new Dictionary<char, Rectangle>();
            ItemToSourceRectangle.Add(WALL, SourceRectangle.wall);
            ItemToSourceRectangle.Add(FLOOR, SourceRectangle.floor);
            ItemToSourceRectangle.Add(DOT, SourceRectangle.dot);
            ItemToSourceRectangle.Add(BIGDOT, SourceRectangle.bigDot);
        }
    }
}