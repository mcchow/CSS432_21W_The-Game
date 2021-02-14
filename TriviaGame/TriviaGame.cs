using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TriviaGameClient.Control;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TriviaGameProtocol;
using System.Threading;
using System;

namespace TriviaGameClient
{
    public class TriviaGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        private Protocol protocol;
        private Connection connection;
        private Thread connectionThread;

        private StartScreen startScreen;
        private Component Screen;

        public TriviaGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            setupProtocol();
        }

        private void setupProtocol()
        {
            protocol = new Protocol();
        }

        private void connect()
        {
            Socket sd = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry serverHostEntry = Dns.GetHostEntry("127.0.0.1");
            IPAddress serverIP = serverHostEntry.AddressList[0];
            IPEndPoint serverEndPoint = new IPEndPoint(serverIP, 8080);
            sd.Connect(serverEndPoint); //TODO handle connection failure

            connection = new Connection(sd, protocol);

            connectionThread = new Thread(new ThreadStart(connection.RecieveLoop));
            connectionThread.Start();
        }

        protected override void Initialize()
        {
            base.Initialize();
            connect();
        }

        private void StartScreen_Next(object sender, string name)
        {
            Register registrationMessage = new Register();
            registrationMessage.Name = name;
            connection.Send(registrationMessage);
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            startScreen = new StartScreen(Content);
            startScreen.Next += StartScreen_Next;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            connection.Disconnect();
            connectionThread.Join();
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            startScreen.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            startScreen.Draw(gameTime, _spriteBatch);
            
            base.Draw(gameTime);
        }
    }
}
