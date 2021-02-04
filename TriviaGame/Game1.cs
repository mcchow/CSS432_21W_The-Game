using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TriviaGameClient.Control;

namespace TriviaGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D background;
        private Texture2D button;
        private Texture2D endbutton;

        public void Startpage()
        {

            _spriteBatch.Begin();

            _spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
            _spriteBatch.Draw(button, new Vector2(300, 240), Color.White);

            _spriteBatch.End();
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // This is the code we added earlier.
            background = Content.Load<Texture2D>("stars"); // change these names to the names of your images
            button = Content.Load<Texture2D>("Button");  // if you are using your own images.
            var startbotton = new Button(Content.Load<Texture2D>("Button"), Content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 200),
                Text = "Start"
            };
            startbotton.Click += StartButton_Click;

            var quitbotton = new Button(Content.Load<Texture2D>("Button"), Content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 250),
                Text = "Quit"
            };
            quitbotton.Click += StartButton_Click;

        }
        
        private void StartButton_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            Startpage();

            base.Draw(gameTime);
        }
    }
}
