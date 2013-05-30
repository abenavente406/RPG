using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.GameObjects;
using RPG.Sprites;
using RPG.Screen;

namespace RPG.Items
{
    public class EItem : GameObject
    {
        private const float FLOAT_SPEED = 0.1f;
        private const int FLOAT_TO = 2;

        private float float_at = 0;

        public const int DROP_SIZE = 16;
        public readonly Item Item;
        private readonly int[] drawStacksOffset;

        public EItem(Item item, int x, int y) : base(item.Sprite, x, y, DROP_SIZE, DROP_SIZE) {
            Item = item;
            if (Item.Stackable) {
                drawStacksOffset = new int[Item.Count/12];
                for (int i=0; i<drawStacksOffset.Length; i++) {
                    drawStacksOffset[i] = ScreenManager.Rand.Next(-3,4);
                }
            } else {
                drawStacksOffset = new int[0];
            }
        }

        private int additionalFloat(int amnt) {
            return (int) float_at;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 offset, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            float_at += FLOAT_SPEED * (elapsed.Milliseconds / ScreenManager.TargElapsedTime.Milliseconds);

            Rectangle pRect = Rect;
            pRect.X -= (int) offset.X;
            pRect.Y -= (int) Math.Round(Math.Sin(float_at) * FLOAT_TO + offset.Y);
            spriteBatch.Draw(sprite.Base, pRect, Color.White);

            // If there are a lot of items in the stack draw a few
            for (int i=0; i<drawStacksOffset.Length; i++) {
                Rectangle drawStackRect = Rect;
                drawStackRect.X -= (int) offset.X + drawStacksOffset[i];
                drawStackRect.Y -= (int) Math.Round((Math.Sin(float_at + drawStacksOffset[i]) * FLOAT_TO + offset.Y));
                spriteBatch.Draw(sprite.Base, drawStackRect, Color.White);
            }
        }
    }
}
