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

using RPG.Entities;
using RPG.Items;
using RPG.GameObjects;

namespace RPG.Screen
{
    class InventoryScreen : PopUpScreen {
        class ItemBlock {
            public Item Item;
            public readonly Rectangle Rect;

            public ItemBlock(Item i) {
                Item = i;
                Rect = new Rectangle(0, 0, 0, 0);
            }

            public ItemBlock(int idx) {
                Item = null;
                Rect = new Rectangle(128 + (idx * 32), 122, 24, 24);
            }

            public bool IsOn(MouseState ms) {
                return (Rect.Width * Rect.Height > 0 && Rect.Contains(new Point(ms.X, ms.Y)));
            }

            public void Action(Player p) {
                if (Item != null && p != null) {
                    Item.UseFunc(p, Item);
                }
            }
        }

        private Player player;
        private Texture2D background;
        private ItemBlock hoverOver;
        private ItemBlock[] itemBlocks;
        private MouseState oldMouse;

        // When change state, update player
        public new bool DoDraw { 
            get { return base.DoDraw; }
            set { if (value) player = ((GameScreen) ScreenManager.getScreen(ScreenId.Game)).Player; base.DoDraw = value; }
        }

        public new bool DoUpdate { 
            get { return base.DoUpdate; }
            set { if (value) player = ((GameScreen) ScreenManager.getScreen(ScreenId.Game)).Player; base.DoUpdate = value; }
        }

        Rectangle helmRect, bodyRect, legsRect;

        public bool buttonPressed;

        public InventoryScreen(ScreenManager sm)
            : base(sm, "Status", "", new string[] { "Back" }, new Action[] { MenuItemFunctions.BackToGame })
        {
            buttonPressed = false;
            itemBlocks = new ItemBlock[] { 
                new ItemBlock(0), new ItemBlock(1), new ItemBlock(2), new ItemBlock(3), new ItemBlock(4), new ItemBlock(5), 
                new ItemBlock(6), new ItemBlock(7), new ItemBlock(8), new ItemBlock(9), new ItemBlock(10), new ItemBlock(11)
            };

            oldMouse = Mouse.GetState();

            helmRect = new Rectangle(100, 52, 155, 16);
            bodyRect = new Rectangle(100, 76, 155, 16);
            legsRect = new Rectangle(100, 100, 155, 16);
        }

        public override void LoadContent()
        {
            background = Content.Load<Texture2D>("GUI/Inventory");
            base.LoadContent();
        }

        public void toggleDrawing() { 
            DoDraw = !DoDraw;
        }

        public override void Update(GameTime gTime) {
            MouseState ms = Mouse.GetState();

            if (ScreenManager.kbState.IsKeyDown(Keys.I)) {
                buttonPressed = true;
            } else if (buttonPressed) {
                buttonPressed = false;
                toggleDrawing();

                // If paused draw this and not game
                ScreenManager.getScreen(ScreenId.Game).DoUpdate = !DoDraw;
                ScreenManager.getScreen(ScreenId.Pause).DoUpdate = !DoDraw;
            }

            hoverOver = null;
            foreach (ItemBlock ib in itemBlocks) {
                if (ib.Item != null && ib.IsOn(ms)) {
                    if (player.Alive) {
                        if (oldMouse.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released) {
                            ib.Action(player);
                        } else if (oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                            player.removeItem(ib.Item);
                            player.GScreen.TileMap.dropItem(ib.Item, player);
                        }
                    }
                    hoverOver = ib;
                    break;
                }
            }

            Point p = new Point(ms.X, ms.Y);
            if (helmRect.Contains(p)) {
                if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                    player.addItem(player.Equipment.Head.Item);
                    player.Equipment.setHead(null);
                } else if (player.Equipment.Head != null)
                    hoverOver = new ItemBlock(player.Equipment.Head.Item);
            } else if (bodyRect.Contains(p)) {
                if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                    player.addItem(player.Equipment.Body.Item);
                    player.Equipment.setBody(null);
                } else if (player.Equipment.Body != null)
                    hoverOver = new ItemBlock(player.Equipment.Body.Item);
            } else if (legsRect.Contains(p)) {
                if (player.Alive && oldMouse.RightButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released) {
                    player.addItem(player.Equipment.Legs.Item);
                    player.Equipment.setLegs(null);
                } else if (player.Equipment.Legs != null)
                    hoverOver = new ItemBlock(player.Equipment.Legs.Item);
            }

            oldMouse = ms;

            base.Update(gTime);
        }

        public override void Draw(GameTime gTime) {
            SpriteBatch.End(); // End normal, drawing different

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive); // Draw additive

            SpriteBatch.Draw(ScreenManager.WhiteRect, new Rectangle(0, 0,
                getScreenManager().GraphicsDevice.Viewport.Width, getScreenManager().GraphicsDevice.Viewport.Height), ScreenManager.AdditiveColor);

            SpriteBatch.End();

            // Draw the menu normal
            SpriteBatch.Begin();

            Rectangle bgRect = new Rectangle(85, 10, 460, 140);
            SpriteBatch.Draw(background, bgRect, Color.White);

            Vector2 pos = new Vector2(110, 25);
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Hp: " + player.Stats.Hp + " (" + (player.Stats.HpPercent * 100).ToString("0") + "%)", pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.Equipment.Head.ToString(14), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.Equipment.Body.ToString(14), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, player.Equipment.Legs.ToString(14), pos, Color.White);

            pos = new Vector2(380, 25);
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Att Mult: " + player.Stats.AttackPower.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Head Dmg Mult: " + player.Stats.THeadMultiplier.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Body Dmg Mult: " + player.Stats.TBodyMultiplier.ToString("0.00"), pos, Color.White);
            pos.Y += 24;
            SpriteBatch.DrawString(ScreenManager.Small_Font, "Legs Dmg Mult: " + player.Stats.TLegsMultiplier.ToString("0.00"), pos, Color.White);
            

            // Draw armour display
            Rectangle armourRect = new Rectangle(SpriteBatch.GraphicsDevice.Viewport.Width/2 - TileMap.SPRITE_SIZE/2, 
                    SpriteBatch.GraphicsDevice.Viewport.Height/2 - TileMap.SPRITE_SIZE/2, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE);
            SpriteBatch.Draw(player.Sprite.Base, armourRect, Color.White);
            if (player.Equipment.Head.Stand != null) SpriteBatch.Draw(player.Equipment.Head.Stand, armourRect, Color.White);
            if (player.Equipment.Body.Stand != null) SpriteBatch.Draw(player.Equipment.Body.Stand, armourRect, Color.White);
            if (player.Equipment.Legs.Stand != null) SpriteBatch.Draw(player.Equipment.Legs.Stand, armourRect, Color.White);

            int idx = 0;
            foreach (Item item in player.itemIterator()) {
                itemBlocks[idx].Item = item;
                SpriteBatch.Draw(item.Sprite.Base, itemBlocks[idx].Rect, Color.White);
                if (item.Stackable) {
                    Vector2 txtSize = ScreenManager.Small_Font.MeasureString(item.Count.ToString());
                    SpriteBatch.DrawString(ScreenManager.Small_Font, item.Count.ToString(), 
                        new Vector2(itemBlocks[idx].Rect.Right - txtSize.X - 2, itemBlocks[idx].Rect.Bottom - txtSize.Y + 2), Color.White);
                }
                idx++;
            }
            // Clear extra items from display
            for (; idx < itemBlocks.Length; idx++) itemBlocks[idx].Item = null;

            if (hoverOver != null && hoverOver.Item != null) {
                string str = hoverOver.Item.ToString();
                Vector2 strSize = ScreenManager.Small_Font.MeasureString(str);
                Rectangle bgSize = new Rectangle(oldMouse.X - (int) strSize.X - 6, oldMouse.Y, 
                                                (int) strSize.X + 6, (int) strSize.Y);
                SpriteBatch.Draw(ScreenManager.WhiteRect, bgSize, ScreenManager.AdditiveColor);
                SpriteBatch.DrawString(ScreenManager.Small_Font, str, new Vector2(bgSize.X + 2, bgSize.Y), Color.White);
            }

            base.Draw(gTime, Color.White); // Draw menu items
        }
    }
}
