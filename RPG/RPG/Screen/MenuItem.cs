using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG.Screen {
    public class MenuItem
    {
        public enum MenuItemAlignment { Center };

        public Action Func;
        MenuScreen screen;
        String text;
        Vector2 position;

        static float BLOAT_RATE = 0.025f;
        float bloated;
        bool bloatedInc;

        public MenuItem(MenuScreen screen, String text, Action func, MenuItemAlignment align)
        {
            this.screen = screen;
            this.text = text;
            this.Func = func;

            bloated = 1f;
            bloatedInc = false;

            Vector2 size = ScreenManager.Font.MeasureString(text);
            if (align == MenuItemAlignment.Center) {
                int x = screen.getScreenManager().GraphicsDevice.Viewport.Width / 2 - (int)size.X / 2;
                int y = screen.getScreenManager().GraphicsDevice.Viewport.Height / 2 - (int)size.Y / 2;

                position = new Vector2(x, y);
            } else {
                position = new Vector2(0, 0);
            }
        }

        public void draw(bool selected, int yPosOffset, Color textColor) {
            if (!selected && bloated != 1) {
                // Move quicker towards zero
                bloated -= BLOAT_RATE * ((bloated - 1f) / Math.Abs(bloated - 1f));
                bloatedInc = true;
            } else if (selected && bloatedInc) {
                bloated += BLOAT_RATE;
                if (bloated >= 1 + BLOAT_RATE * 10) bloatedInc = false;
            } else if (selected && !bloatedInc) {
                bloated -= BLOAT_RATE;
                if (bloated <= 1 - BLOAT_RATE * 10) bloatedInc = true;
            }

            Vector2 position = getPosition(); // Copy position
            String text;
            if (selected)
                text = "[ " + getText() + " ]";
            else
                text = getText();
            position.X = screen.getScreenManager().GraphicsDevice.Viewport.Width / 2 - ScreenManager.Font.MeasureString(text).X * bloated / 2;
            // Horizontal offset from extra items
            position.Y += yPosOffset;

            screen.SpriteBatch.DrawString(ScreenManager.Font, text, position, textColor, 0, Vector2.Zero, bloated, SpriteEffects.None, 1);
        }

        public String getText() { return text; }
        public Vector2 getPosition() { return position; }
        public void run() { Func(); }
    }
}