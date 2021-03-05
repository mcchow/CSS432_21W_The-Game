﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TriviaGameClient.Control;
using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;

namespace TriviaGameClient
{
    class StartScreen : Component
    {
        /// <summary>
        /// test function
        /// </summary>
        public void blah ()
        {
            //protocol
            //connection

            /* Sends the message to the server immediately.*/
            connection.Send(new JoinRoom("Some Room ID"));

            /* Tells the protocol object to run the lambda function upon recipt whenever an AskForCard message is recieved.*/
            protocol.RegisterMessageHandler<AskForCard>((AskForCard message, Connection c) =>
            {

            });

        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private bool gofirst = false;
        private string stage = "startScreen";
        //make const value on time in game
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
        private int roomnum = -1;
        private Button playButton;
        private List<Button> playerlist = new List<Button>();
        private int numplayer = 2;

        ///
        /// category picking
        ///
        private string[] CardCat = { "History", "Art", "Science", "Geography", "Sports", "Entertainment" };
        private List<Button> CatButton = new List<Button>();

        /// <summary>
        /// gmae
        /// </summary>
        private List<Button> ansButtons = new List<Button>();
        private List<Button> ansButtonsr = new List<Button>(); //red background
        private List<Button> ansButtonsg = new List<Button>(); // green background
        private string Question = "Question";
        private Button QuestionBox;
        private int ans = -1;
        private int CorrectAns = -1;
        private const int timer = 20;
        private bool update = false;
        private int count = -1;

        /// <summary>
        /// waiting
        /// </summary>
        /// 
        private Button waitingtext;

        /// <summary>
        /// result win/lose
        /// </summary>
        private Button winlosetext;
        private string gameresult = "";


        private void setQuestion()
        {
            QuestionBox.Text = Question;
        }

        private void StartButton_Click(object sender, System.EventArgs e)
        {
            Next?.Invoke(this, textField.Text);
            //do something with the text here????
            //send user name
            Register tempuser = new Register();
            tempuser.Name = textField.Text;
            connection.Send(tempuser);

            if (stage != "meun") stage = "meun";
            else stage = "startScreen";
        }

        private void Createroom_Click(object sender, System.EventArgs e)
        {
            stage = "wait";
            waitingtext.Text = "Waiting for another player to Join";
            CreateRoom createRoom = new CreateRoom();
            connection.Send(createRoom);
            protocol.RegisterMessageHandler<RoomEntry>(updateplayerlist);
        }

        private void Joinroom_Click(object sender, System.EventArgs e)
        {
            stage = "lobby";
        }

        private void backmeun_Click(object sender, System.EventArgs e)
        {
            stage = "menu";
        }

        /*
        private void gotoplay_Click(object sender, System.EventArgs e)
        {
            //protocol.RegisterMessageHandler<RoomEntry>(updateplayerlist);
            if(playerlist[0].Text != "" && playerlist[1].Text != "")
            {
                stage = "cat";
                stage = "wait";
            }
        }*/

        public Connection connection;
        public Protocol protocol;
        //updater
        public void updateCorans(AnswerAndResult a , Connection b) {
            CorrectAns = a.correctAnswer-97;// case char to int, -97, lazy chasing
        }

        public void updateplayerlist(RoomEntry a, Connection b)
        {
            playerlist[0].Text = a.player1;
            playerlist[1].Text = a.player2;
        }

        public void updatePlayStage(RoomEntry a, Connection b)
        {
            stage = "cat";
        }

        public StartScreen(ContentManager content,Connection connectionin ,Protocol protocolin)
        {
            ////////////////////////////////////////////
            /// set up connection

            connection = connectionin;
            protocol = protocolin;

            //
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

                //find the first empty room and join it

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
                //join a room when room button click
                void joinroom_Click(object sender, System.EventArgs e)
                {
                    stage = "room";
                    //send room
                    JoinRoom test = new JoinRoom(i.ToString());
                    connection.Send(test);
                    roomnum = i;
                }
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
            

            //playButtons.Click += Createroom_Click;

            //////////////////////////////////////////////////////////////////////////////////////////////////////
            ///category pick page
            ///
            int temp = 0;
            foreach (string cat in CardCat)
            {
                Button tempbutton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 10 + 60 * temp),
                    Text = cat
                };
                void CatClick(object sender, System.EventArgs e)
                {
                    //do somthing with server
                    
                    stage = "play";
                }
                tempbutton.Click += CatClick;
                CatButton.Add(tempbutton);
                temp++;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            ///play screen
            ///

            for (int i = 0; i < 4; i++)
            {
                String name = String.Format("player {0}", i);
                Button tempbutton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 100 + 70 * i),
                    Text = name
                };
                //when ans click
                void AnsClick(object sender, System.EventArgs e)
                {
                    ans = i;
                    //send the ans
                    PlayerAnswer tempans = new PlayerAnswer();
                    tempans.playerAns = (char)(i + 97);
                    connection.Send(tempans);
                    //set show ans timer
                    count = 100;
                    //get correct ans?
                    protocolin.RegisterMessageHandler<AnswerAndResult>(updateCorans);
                }
                
                tempbutton.Click += AnsClick;
                ansButtons.Add(tempbutton);
                tempbutton = new Button(content.Load<Texture2D>("Buttong"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 100 + 70 * i),
                    Text = name
                };
                ansButtonsg.Add(tempbutton);
                tempbutton = new Button(content.Load<Texture2D>("Buttonr"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 100 + 70 * i),
                    Text = name
                };
                ansButtonsr.Add(tempbutton);
            }

            QuestionBox = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(50, 60),
                Text = Question
            };

            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///wait
            ///

            waitingtext = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(50, 60),
                Text = "Waiting for other player..."
            };
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///result
            ///

            winlosetext = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(50, 60),
                Text = "You Win"
            };

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
                case "menu":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    CreatelobbyButton.Draw(gameTime, spriteBatch);
                    JoinlobbyButton.Draw(gameTime, spriteBatch);
                    gofirst = true;
                    break;
                case "lobby":
                    gofirst = false;
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    for (int i = 0; i < numroom; i++)
                    {
                        JoinroomButtons[i].Draw(gameTime, spriteBatch);
                        roomlist[i].Draw(gameTime, spriteBatch);
                    }
                    break;
                case "cat":
                    //protocol.RegisterMessageHandler<RoomEntry>(updateplayerlist);??????
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    foreach (Button button in CatButton)
                    {
                        button.Draw(gameTime, spriteBatch);
                    }
                    break;
                case "play":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    if (ans == -1)
                        for (int i = 0; i < 4; i++)
                        {
                            ansButtons[i].Draw(gameTime, spriteBatch);
                        }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (CorrectAns == i)
                            {
                                ansButtonsg[i].Draw(gameTime, spriteBatch);
                            }
                            else
                            {
                                ansButtonsr[i].Draw(gameTime, spriteBatch);
                            }
                            
                        }
                        if (count > 0)
                        {
                            count--;
                        }
                        else
                        {
                            ans = -1;
                            //To Do go to waiting screen
                            stage = "result";

                        }
                        
                    }
                    QuestionBox.Draw(gameTime, spriteBatch);
                    break;
                case "waiting":
                    waitingtext.Draw(gameTime, spriteBatch);
                    break;
                case "result":
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    winlosetext.Draw(gameTime, spriteBatch);
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
                case "menu":
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
                case "cat":
                    foreach (Button button in CatButton)
                    {
                        button.Update(gameTime);
                    }
                    break;
                case "play":
                    if(ans == -1)
                    for (int i = 0; i < 4; i++)
                    {
                        ansButtons[i].Update(gameTime);
                    }
                    break;
                case "result":
                    lobbybackmeunButton.Update(gameTime);
                    break;
            }
        }
    }
}
