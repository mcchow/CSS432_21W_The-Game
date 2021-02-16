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
        private string stage = "startScreen";
        /// <summary>
        /// Start page content
        /// </summary>
        private Texture2D background;
        private Button startButton;
        private TextField textField;



        /// <summary>
        /// meun content
        /// </summary>
        private Button CreatelobbyButton;
        private Button JoinlobbyButton;

        public event EventHandler<string> Next;

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            Next?.Invoke(this, textField.Text);
            //do something with the text here????

            if (stage != "meun") stage = "meun";
            else stage = "startScreen";
        }

        private void Createroom_Click(object sender, System.EventArgs e)
        {
            stage = "startScreen";
        }

        private void Joinroom_Click(object sender, System.EventArgs e)
        {
            stage = "startScreen";
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

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///meun page
            ///

            CreatelobbyButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 200),
                Text = "Create Lobby"
            };
            CreatelobbyButton.Click += Createroom_Click;
            JoinlobbyButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 300),
                Text = "Join Lobby"
            };
            JoinlobbyButton.Click += Joinroom_Click;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            switch (stage)
            {
                case "startScreen":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    startButton.Draw(gameTime, spriteBatch);
                    textField.Draw(gameTime, spriteBatch);
                    break;
                case "meun":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    CreatelobbyButton.Draw(gameTime, spriteBatch);
                    JoinlobbyButton.Draw(gameTime, spriteBatch);
                    break;

            }
            spriteBatch.End();

        }

        public override void Update(GameTime gameTime)
        {
            switch (stage)
            {
                case "startScreen":
                    startButton.Update(gameTime);
                    textField.Update(gameTime);
                    break;
                case "meun":
                    CreatelobbyButton.Update(gameTime);
                    JoinlobbyButton.Update(gameTime);
                    break;
            }
        }
    }
}
