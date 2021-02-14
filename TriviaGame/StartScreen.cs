using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TriviaGameClient.Control;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameClient
{
    class StartScreen : Component
    {
        private Texture2D background;
        private Button startButton;
        private TextField textField;

        public event EventHandler<string> Next;

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            Next?.Invoke(this, textField.Text);
        }

        public StartScreen(ContentManager content)
        {
            background = content.Load<Texture2D>("stars"); // change these names to the names of your images
            startButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 200),
                Text = "Start"
            };
            startButton.Click += StartButton_Click;
            textField = new TextField(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"));
            textField.Rectangle = new Rectangle(350, 100, 100, 20);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
            startButton.Draw(gameTime, spriteBatch);
            textField.Draw(gameTime, spriteBatch);

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            startButton.Update(gameTime);
            textField.Update(gameTime);
        }
    }
}
