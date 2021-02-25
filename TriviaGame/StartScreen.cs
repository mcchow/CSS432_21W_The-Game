using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
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

        private String playername;

        /// <summary>
        /// meun content
        /// </summary>
        private Button CreatelobbyButton;
        private Button JoinlobbyButton;
        public event EventHandler<string> Next;

        /// <summary>
        /// Join lobby
        /// </summary>
        /// 

        private Button lobbybackmeunButton;
        //list of button that go to different room
        private List<Button> JoinroomButtons = new List<Button>();
        //just a place holder, do not have any action with the button
        private List<Button> roomlist = new List<Button>();
        private int numroom = 2;

        /// <summary>
        /// room
        /// </summary>
        private Button playButton;
        private List<Button> playerlist = new List<Button>();
        private int numplayer = 2;

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            Next?.Invoke(this, textField.Text);
            //do something with the text here????

            if (stage != "meun") stage = "meun";
            else stage = "startScreen";
        }

        private void Createroom_Click(object sender, System.EventArgs e)
        {
            stage = "room";
        }

        private void Joinroom_Click(object sender, System.EventArgs e)
        {
            stage = "lobby";
        }

        private void backmeun_Click(object sender, System.EventArgs e)
        {
            stage = "meun";
        }

        private void joinroom_Click(object sender, System.EventArgs e)
        {
            stage = "room";
        }

        public StartScreen(ContentManager content)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///start page
            ///
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

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///lobby page
            ///

            lobbybackmeunButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(10, 10),
                Text = "Back"
            };
            lobbybackmeunButton.Click += backmeun_Click;

            for (int i = 0; i < 7; i++)
            {
                Button tempbutton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(650, 65 + 40 * i),
                    Text = "Join"
                };
                tempbutton.Click += joinroom_Click;
                JoinroomButtons.Add(tempbutton);
            }
            for (int i = 0; i < 7; i++)
            {
                String name = String.Format("Room {0}", i);
                Button tempbutton = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(50, 60 + 40 * i),
                    Text = name
                };
                roomlist.Add(tempbutton);
            }


            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///room page
            ///

            for (int i = 0; i < 7; i++)
            {
                String name = String.Format("player {0}", i);
                Button tempbutton = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 60 + 70 * i),
                    Text = name
                };
                playerlist.Add(tempbutton);
            }
            playButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 400),
                Text = "Play"
            };
            //playButtons.Click += Createroom_Click;

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
                case "lobby":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    for (int i = 0; i < numroom; i++)
                    {
                        JoinroomButtons[i].Draw(gameTime, spriteBatch);
                        roomlist[i].Draw(gameTime, spriteBatch);
                    }
                    break;
                case "room":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    for (int i = 0; i < numplayer; i++)
                    {
                        playerlist[i].Draw(gameTime, spriteBatch);
                    }
                    playButton.Draw(gameTime, spriteBatch);
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
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
                case "lobby":
                    lobbybackmeunButton.Update(gameTime);
                    for (int i = 0; i < numroom; i++)
                    {
                        JoinroomButtons[i].Update(gameTime);
                    }
                    break;
                case "room":
                    lobbybackmeunButton.Update(gameTime);
                    playButton.Update(gameTime);
                    break;
            }
        }
    }
}
