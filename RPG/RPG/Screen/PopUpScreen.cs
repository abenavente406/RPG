using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RPG.Screen
{
    class PopUpScreen : MenuScreen {
        protected string Title, Text;

        public PopUpScreen(ScreenManager sm, string title, string text = "", string[] items = null, Action[] funcs = null) : 
            base (sm, items, funcs) {

            Text = text;
            Title = title;
        }

        public override void Update(GameTime gTime) {
            // If is showing, run the base update
            if (this.DoDraw)
                base.Update(gTime);
        }

        public override void Draw(GameTime gTime) { Draw(gTime, Color.White);  }
        public override void Draw(GameTime gTime, Color textColor) {
            float y = -(SELECTED_EXTRA_SIZE.Y * (menuItems.Count - 1) * .75f) + (screenManager.GraphicsDevice.Viewport.Height - SELECTED_EXTRA_SIZE.Y) / 2;

            // Draw title
            if (Title != null && Title != "") {
                Vector2 size = ScreenManager.Font.MeasureString(Title);
                Vector2 pos = new Vector2(getScreenManager().GraphicsDevice.Viewport.Width / 2 - (int) (size.X / 2), (int) (y - size.Y * 2));
                SpriteBatch.DrawString(ScreenManager.Font, Title, pos, textColor);
            }

            // Draw text
            if (Text != null && Text != "") {
                Vector2 size = ScreenManager.Font.MeasureString(Text);
                Vector2 pos = new Vector2(getScreenManager().GraphicsDevice.Viewport.Width / 2 - (int) (size.X / 2), (int) (y - size.Y));
                SpriteBatch.DrawString(ScreenManager.Small_Font, Text, pos, textColor);
            }

            base.Draw(gTime, textColor); // Draw menu items
        }
    }
}
