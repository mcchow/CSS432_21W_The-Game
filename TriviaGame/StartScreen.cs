using Microsoft.Xna.Framework;
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
        /// 

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
        private Button PointBox;

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
            stage = "meun";
        }

        private void Createroom_Click(object sender, System.EventArgs e)
        {
            stage = "wait";
            points = 0;
            PointBox.Text = "Score:" + points.ToString();
            waitingtext.Text = "Waiting for another player to Join";
            CreateRoom createRoom = new CreateRoom();
            connection.Send(createRoom);
        }

        private void Joinroom_Click(object sender, System.EventArgs e)
        {
            roomlist.Clear();
            JoinroomButtons.Clear();
            points = 0;
            PointBox.Text = "Score:" + points.ToString();
            roomnum = 0;
            connection.Send(new ListRoomsRequest());
            stage = "lobby";
        }

        private void backmeun_Click(object sender, System.EventArgs e)
        {
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
        public void updateCorans(AnswerAndResult a , Connection b) {
            CorrectAns = a.correctAnswer-97;// case char to int, -97, lazy chasing
        }

        public void updateRoomList(RoomEntry a, Connection b)
        {
            roomnum++;
            points = 0;
            PointBox.Text = "Score:" + points.ToString();
            String name = String.Format("{0}'s Room", a.player1);
            Button tempbutton = new Button(contentManager.Load<Texture2D>("roombox"), contentManager.Load<SpriteFont>("normal"))
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
            
            //tempbutton.Click += joinroom_Click;
            roomlist.Add(tempbutton);


            Button tempbutton2 = new Button(contentManager.Load<Texture2D>("Button"), contentManager.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(650, 65 + 40 * roomnum),
                Text = "Join"
            };
            
            tempbutton2.Click += joinroom_Click;
            JoinroomButtons.Add(tempbutton2);

            playerlist[1].Text = a.player2;
        }

        public void nextPlayerTurn(NextPlayerTurn a, Connection b)
        {
            points = a.curNumCards;
            PointBox.Text = "Score:" + points.ToString();
            waitingtext.Text = "Wait for " + a.whosTurn + " to Answer the question...";
            stage = "wait";
        }

        public void answerAndResult(AnswerAndResult a, Connection b)
        {
            CorrectAns = a.correctAnswer-97;
            points = a.numCards;
            PointBox.Text = "Score" + a.numCards.ToString();
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
        public void roomFull(RoomFull a, Connection b)
        {
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
            protocol.RegisterMessageHandler<RoomFull>(roomFull);

            //point box
            PointBox = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
            {
                Position = new Vector2(350, 10),
                Text = ""
            };

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
            /*
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
            */

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
                Button tempbutton = new Button(content.Load<Texture2D>("Button"), content.Load<SpriteFont>("normal"))
                {
                    Position = new Vector2(100, 100 + 70 * i),
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
                case "meun":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    CreatelobbyButton.Draw(gameTime, spriteBatch);
                    JoinlobbyButton.Draw(gameTime, spriteBatch);
                    gofirst = true;
                    break;
                case "lobby":
                    gofirst = false;
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
                    PointBox.Draw(gameTime, spriteBatch);
                    foreach (Button button in CatButton)
                    {
                        button.Draw(gameTime, spriteBatch);
                    }
                    break;
                case "play":
                    spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);
                    PointBox.Draw(gameTime, spriteBatch);
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
                            stage = "waiting";

                        }
                        
                    }
                    QuestionBox.Draw(gameTime, spriteBatch);
                    break;
                case "wait":
                    waitingtext.Draw(gameTime, spriteBatch);
                    PointBox.Draw(gameTime, spriteBatch);
                    break;
                case "result":
                    lobbybackmeunButton.Draw(gameTime, spriteBatch);
                    PointBox.Draw(gameTime, spriteBatch);
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
                case "meun":
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
