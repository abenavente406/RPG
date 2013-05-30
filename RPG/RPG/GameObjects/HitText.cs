using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Entities;
using RPG.Screen;

namespace RPG.GameObjects
{
    public class HitText
    {
        static float MAX_MS = 500;
        static float X_MS = 1f, Y_MS = -1.5f;

        public bool Alive;
        public string Text;
        public Color TextColor;
        
        private float alpha;
        private Vector2 offset;
        private Entity entity;

        int ms;

        public HitText(Entity e, int dmg) {
            Alive = true;
            Text = dmg.ToString();
            TextColor = Color.Red;
            alpha = 1;
            offset = new Vector2(-8, -8);
            entity = e;
            ms = 0;
        }

        public void update(TimeSpan elapsed) {
            ms += elapsed.Milliseconds;
            if (ms > MAX_MS) Alive = false;
            else {
                offset.X += X_MS; 
                offset.Y += Y_MS; 
                alpha -= ms / MAX_MS; 
                TextColor = Color.Lerp(Color.Red, Color.Transparent, alpha);
            }
        }

        public void draw(SpriteBatch spriteBatch, Vector2 offset) {
            Vector2 pos = getPosition();
            pos.X -= (int) offset.X;
            pos.Y -= (int) offset.Y;

            spriteBatch.DrawString(ScreenManager.Small_Font, Text, pos, TextColor);
        }

        public Vector2 getPosition() {
            return new Vector2(entity.EBounds.Right + offset.X, entity.EBounds.Top - offset.Y);
        }
    }
}
