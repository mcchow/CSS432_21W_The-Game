using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TriviaGameClient.Control;
using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;
using Microsoft.Xna.Framework.Audio;

namespace TriviaGameClient
{
    class StartScreen : Component
    {
        /// <summary>
        /// test function
        /// </summary>
        /// 

        //////////////////////////////////////////////////////////////////////////////////////////////////
        ///error handleing
        private string ErrorText = "";
        private int Errorcount = 0;

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private bool gofirst = false;
        private string stage = "startScreen";
        private int playerid = -1;
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
        private Button unregisterButton;
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
        private int roomnum = 0;
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
        //public List<string> Playername;
        private Button LeaveGameButton;
        private List<Button> ansButtons = new List<Button>();
        private List<Button> ansButtonsr = new List<Button>(); //red background
        private List<Button> ansButtonsg = new List<Button>(); // green background
        private string Question = "Question";
        private Button QuestionBox;
        // the player answered Question
        private int ans = -1;
        //correct answer index, -1 is not recevie 
        private int CorrectAns = -1;
        //const timer for answering question, currently not using
        private const int timer = 20;

        //a timer for some function
        private int count = -1;
        //number of card that user have(points)
        private int points = 0;
        private Button PointBox, PointBox1;

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
            playername = textField.Text;
            connection.Send(tempuser);
            stage = "meun";
        }

        private void Createroom_Click(object sender, System.EventArgs e)
        {
            stage = "wait";
            points = 0;
            PointBox.Text = "You:" + points.ToString();
            PointBox1.Text = "Opponent:" + points.ToString();
            waitingtext.Text = "Waiting for Opponent to Join";
            CreateRoom createRoom = new CreateRoom();
            connection.Send(createRoom);
        }

        private void Joinroom_Click(object sender, System.EventArgs e)
        {
            roomlist.Clear();
            JoinroomButtons.Clear();
            points = 0;
            PointBox.Text = "You:" + points.ToString();
            PointBox1.Text = "Opponent:" + points.ToString();
            roomnum = 0;
            connection.Send(new ListRoomsRequest());
            stage = "lobby";
        }

        private void backmeun_Click(object sender, System.EventArgs e)
        {
            stage = "meun";
        }

        private void leavegame_Click(object sender, System.EventArgs e)
        {
            connection.Send(new LeaveRoom()); 
            stage = "meun";
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
        ContentManager contentManager;
        public Connection connection;
        public Protocol protocol;
        //updater

        public void updateRoomList(RoomEntry a, Connection b)
        {
            roomnum++;
            points = 0;
            PointBox.Text = "You:" + points.ToString();
            PointBox1.Text = "Opponent:" + points.ToString();
            String name = a.player1 + "'s Room";
            Button tempbutton = new Button(contentManager.Load<Texture2D>("roombox"), contentManager.Load<SpriteFont>("normal"), contentManager.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(50, 60 + 40 * roomnum),
                Text = name
            };
            roomlist.Add(tempbutton);

            //join a room when room button click
            void joinroom_Click(object sender, System.EventArgs e)
            {
                stage = "room";
                //send room
                JoinRoom test = new JoinRoom(a.roomID);
                connection.Send(test);
            }

            Button tempbutton2 = new Button(contentManager.Load<Texture2D>("Button"), contentManager.Load<SpriteFont>("normal"), contentManager.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(650, 65 + 40 * roomnum),
                Text = "Join"
            };
            
            tempbutton2.Click += joinroom_Click;
            JoinroomButtons.Add(tempbutton2);

            //playerlist[1].Text = a.player2;
        }

        //join a room when room button click
        public void unregister_Click(object sender, System.EventArgs e)
        {
            stage = "startScreen";
            //send room
            connection.Send(new Unregister());
        }

        public void nextPlayerTurn(NextPlayerTurn a, Connection b)
        {
            points = a.curNumCards;
            PointBox.Text = "You:" + a.curNumCards.ToString();
            //PointBox1.Text = "Score:" + points.ToString();
            waitingtext.Text = "Wait for " + a.whosTurn + " to Answer the question...";
            stage = "wait";
        }

        public void answerAndResult(AnswerAndResult a, Connection b)
        {
            CorrectAns = a.correctAnswer-97;
            points = a.numCards;
            if(a.whosTurn != playerid)
            PointBox.Text = "You:" + a.numCards.ToString();
            else
            PointBox1.Text = "Opponent:" + a.numCards.ToString();
        }
        public void updateQuestion(TriviaQuestion a, Connection b)
        {
            ans = -1;
            CorrectAns = -1;
            count = 0;
            QuestionBox.Text = a.question;
            ansButtons[0].Text = a.optionA;
            ansButtonsg[0].Text = a.optionA;
            ansButtonsr[0].Text = a.optionA;
            ansButtons[1].Text = a.optionB;
            ansButtonsg[1].Text = a.optionB;
            ansButtonsr[1].Text = a.optionB;
            ansButtons[2].Text = a.optionC;
            ansButtonsg[2].Text = a.optionC;
            ansButtonsr[2].Text = a.optionC;
            ansButtons[3].Text = a.optionD;
            ansButtonsg[3].Text = a.optionD;
            ansButtonsr[3].Text = a.optionD;
            stage = "play";
            waitingtext.Text = "Wait for Opponent to Answer the question...";
        }
        public void askForCard(AskForCard a, Connection b)
        {
            stage = "cat";
        }
        public void opponentQuit(OpponentQuit a, Connection b)
        {
            stage = "meun";
        }
        public void winner(Winner a, Connection b)
        {
            winlosetext.Text = a.winner + "wins the game";
            stage = "result";
        }
        public void roomUnavailable(RoomUnavailable a, Connection b)
        {
            stage = "meun";
            ErrorText = a.Reason;
            Errorcount = 100;
        }

        public void ErrorstringDraw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(contentManager.Load<SpriteFont>("normal"), ErrorText, new Vector2(20, 450), Color.Red);
        }
        public void opponentQuit(OpponentQuit gameTime, SpriteBatch spriteBatch)
        {
            ErrorText = "Opponent Quit";
            Errorcount = 100;
            stage = "meun";
        }

        public StartScreen(ContentManager content,Connection connectionin ,Protocol protocolin)
        {
            contentManager = content;
            connection = connectionin;
            protocol = protocolin;
            ////////////////////////////////////////////
            /// set up connection
            protocol.RegisterMessageHandler<RoomEntry>(updateRoomList);
            protocol.RegisterMessageHandler<NextPlayerTurn>(nextPlayerTurn);
            protocol.RegisterMessageHandler<TriviaQuestion>(updateQuestion);
            protocol.RegisterMessageHandler<AskForCard>(askForCard);
            protocol.RegisterMessageHandler<OpponentQuit>(opponentQuit);
            protocol.RegisterMessageHandler<AnswerAndResult>(answerAndResult);
            protocol.RegisterMessageHandler<Winner>(winner);
            protocol.RegisterMessageHandler<RoomUnavailable>(roomUnavailable);
            protocol.RegisterMessageHandler<OpponentQuit>(opponentQuit);


            //point box
            PointBox = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(350, 10),
                Text = ""
            };

            PointBox1 = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(500, 10),
                Text = ""
            };

            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///start page
            ///
            background = content.Load<Texture2D>("backgroundworld"); // change these names to the names of your images
            startButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
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
            unregisterButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(10, 10),
                Text = "Back"
            };
            unregisterButton.Click += unregister_Click;
            CreatelobbyButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(350, 200),
                Text = "Create Lobby"

                //find the first empty room and join it

            };
            CreatelobbyButton.Click += Createroom_Click;

            JoinlobbyButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(350, 300),
                Text = "Join Lobby"
            };
            JoinlobbyButton.Click += Joinroom_Click;

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///lobby page
            ///

            lobbybackmeunButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(10, 10),
                Text = "Back"
            };
            lobbybackmeunButton.Click += backmeun_Click;
            /*
            for (int i = 0; i < 7; i++)
            {
                String name = String.Format("Room {0}", i);
                Button tempbutton = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
                {
                    Position = new Vector2(50, 60 + 40 * i),
                    Text = name
                };
                roomlist.Add(tempbutton);
            }
            */

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            ///room page
            ///
            LeaveGameButton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(10, 10),
                Text = "Back"
            };
            LeaveGameButton.Click += leavegame_Click;
            
            for (int i = 0; i < 7; i++)
            {
                String name = String.Format("player {0}", i);
                Button tempbutton = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
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
                Button tempbutton = new Button(content.Load<Texture2D>(cat), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
                {
                    Position = new Vector2(40 + 130 * temp, 200),
                    Text = cat
                };
                if( cat == "Entertainment")
                {
                    tempbutton = new Button(content.Load<Texture2D>(cat), content.Load<SpriteFont>("small"), content.Load<SoundEffect>("hover"))
                    {
                        Position = new Vector2(40 + 130 * temp, 200),
                        Text = cat
                    };
                }
                //tempbutton.PenColour = Color.Black;
                void CatClick(object sender, System.EventArgs e)
                {
                    //do somthing with server
                    connection.Send(new ChosenCard(cat));
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
                Button tempbutton = new Button(content.Load<Texture2D>("Buttonw"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
                {
                    Position = new Vector2(120, 200 + 50 * i),
                    Text = name
                };
                //when ans click
                int i2 = i;
                void AnsClick(object sender, System.EventArgs e)
                {
                    ans = i;
                    //send the ans
                    PlayerAnswer tempans = new PlayerAnswer();
                    tempans.playerAns = (char)(i2 + 'a');
                    connection.Send(tempans);
                    //set show ans timer
                    count = 100;
                    //get correct ans?
                }
                
                tempbutton.Click += AnsClick;
                ansButtons.Add(tempbutton);
                tempbutton = new Button(content.Load<Texture2D>("Buttong"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
                {
                    Position = new Vector2(120, 200 + 50 * i),
                    Text = name
                };
                ansButtonsg.Add(tempbutton);
                tempbutton = new Button(content.Load<Texture2D>("Buttonr"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
                {
                    Position = new Vector2(120, 200 + 50 * i),
                    Text = name
                };
                ansButtonsr.Add(tempbutton);
            }
            
            QuestionBox = new Button(content.Load<Texture2D>("Question"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(50, 60),
                Text = Question
            };

            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///wait
            ///

            waitingtext = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
            {
                Position = new Vector2(50, 60),
                Text = "Waiting for Opponent..."
            };
            //////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///result
            ///

            winlosetext = new Button(content.Load<Texture2D>("roombox"), content.Load<SpriteFont>("normal"), content.Load<SoundEffect>("hover"))
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
                    spriteBatch.DrawString(contentManager.Load<SpriteFont>("normal"), "Please enter a user name", new Vector2(300, 70), Color.Violet);
                    textField.Draw(gameTime, spriteBatch);
                    break;
                case "meun":
                    playerid = 1;
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    unregisterButton.Draw(gameTime, spriteBatch);
                    CreatelobbyButton.Draw(gameTime, spriteBatch);
                    JoinlobbyButton.Draw(gameTime, spriteBatch);
                    gofirst = true;
                    break;
                case "lobby":
                    gofirst = false;
                    playerid = 2;
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    for (int i = 0; i < roomnum; i++)
                    {
                        JoinroomButtons[i].Draw(gameTime, spriteBatch);
                        roomlist[i].Draw(gameTime, spriteBatch);
                    }
                    break;
                case "cat":
                    //protocol.RegisterMessageHandler<RoomEntry>(updateplayerlist);??????
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    spriteBatch.DrawString(contentManager.Load<SpriteFont>("normal"), "Please pick a category", new Vector2(300, 70), Color.Violet);
                    LeaveGameButton.Draw(gameTime, spriteBatch);
                    PointBox.Draw(gameTime, spriteBatch);
                    PointBox1.Draw(gameTime, spriteBatch);
                    foreach (Button button in CatButton)
                    {
                        button.Draw(gameTime, spriteBatch);
                    }
                    break;
                case "play":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    PointBox.Draw(gameTime, spriteBatch);
                    PointBox1.Draw(gameTime, spriteBatch);
                    LeaveGameButton.Draw(gameTime, spriteBatch);
                    if (CorrectAns == -1)
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
                            CorrectAns = -1;
                            ans = -1;
                            //To Do go to waiting screen
                            stage = "wait";
                        }
                        
                    }
                    QuestionBox.Draw(gameTime, spriteBatch);
                    break;
                case "wait":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    LeaveGameButton.Draw(gameTime, spriteBatch);
                    waitingtext.Draw(gameTime, spriteBatch);
                    PointBox.Draw(gameTime, spriteBatch);
                    PointBox1.Draw(gameTime, spriteBatch);
                    break;
                case "result":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    PointBox.Draw(gameTime, spriteBatch);
                    PointBox1.Draw(gameTime, spriteBatch);
                    winlosetext.Draw(gameTime, spriteBatch);
                    break;
                default:
                    spriteBatch.DrawString(contentManager.Load<SpriteFont>("normal"), "Loading...", new Vector2(20, 450), Color.Blue);
                    break;
            }
            if(Errorcount > 0)
            {
                ErrorstringDraw(gameTime, spriteBatch);
                Errorcount--;
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
                    unregisterButton.Update(gameTime);
                    CreatelobbyButton.Update(gameTime);
                    JoinlobbyButton.Update(gameTime);
                    break;
                case "lobby":
                    lobbybackmeunButton.Update(gameTime);
                    for (int i = 0; i < JoinroomButtons.Count; i++)
                    {
                        JoinroomButtons[i].Update(gameTime);
                    }
                    break;
                case "room":
                    lobbybackmeunButton.Update(gameTime);
                    break;
                case "cat":
                    LeaveGameButton.Update(gameTime);
                    foreach (Button button in CatButton)
                    {
                        button.Update(gameTime);
                    }
                    break;
                case "play":
                    LeaveGameButton.Update(gameTime);
                    if (ans == -1)
                    for (int i = 0; i < 4; i++)
                    {
                        ansButtons[i].Update(gameTime);
                    }
                    break;
                case "wait":
                    LeaveGameButton.Update(gameTime);
                    break;
                case "result":
                    lobbybackmeunButton.Update(gameTime);
                    break;
            }
        }
    }
}
