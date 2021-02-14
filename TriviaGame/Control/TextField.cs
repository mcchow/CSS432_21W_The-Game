using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriviaGameClient.Control
{
    class TextField : Component
    {
        private SpriteFont font;
        private bool hasFocus = true;
        private EventHandler Action;
        private HashSet<Keys> keysPressedLastUpdate;

        public string Text = "Name Here";
        public Color TextColor = Color.Black;
        public Color BackgroundColor = Color.White;
        public Texture2D BackgroundTexture;
        public Rectangle Rectangle = new Rectangle(0, 0, 20, 5);

        public TextField(Texture2D backgroundTexture, SpriteFont font)
        {
            this.BackgroundTexture = backgroundTexture;
            this.font = font;
            this.keysPressedLastUpdate = new HashSet<Keys>();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BackgroundTexture, Rectangle, BackgroundColor);

            float x = Rectangle.X + 4;
            float y = Rectangle.Y + Rectangle.Height / 2 - font.MeasureString(Text).Y / 2;
            spriteBatch.DrawString(font, Text, new Vector2(x, y), TextColor);
        }

        public override void Update(GameTime gameTime)
        {
            if (!hasFocus)
            {
                return;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                Action?.Invoke(this, new EventArgs());
                return;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Back) && !keysPressedLastUpdate.Contains(Keys.Back) && Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
                keysPressedLastUpdate.Clear();
                keysPressedLastUpdate.UnionWith(Keyboard.GetState().GetPressedKeys());
                return;
            }
            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
            {
                string k = key.ToString();
                if (!keysPressedLastUpdate.Contains(key)) {
                      // Alpha
                    if (k.Length == 1 && k[0] >= 'A' && k[0] <= 'Z')
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) ||
                            Keyboard.GetState().IsKeyDown(Keys.RightShift))
                        {
                            Text += k;
                        }
                        else
                        {
                            Text += k.ToLower();
                        }
                    } // Digits
                    else if (k.Length == 2 && k[1] >= '0' && k[1] <= '9')
                    {
                        Text += k[1];
                    }
                }
            }
            keysPressedLastUpdate.Clear();
            keysPressedLastUpdate.UnionWith(Keyboard.GetState().GetPressedKeys());
        }
    }
}
