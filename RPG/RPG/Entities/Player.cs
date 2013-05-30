using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Sprites;
using RPG.Screen;
using RPG.Helpers;
using RPG.GameObjects;
using RPG.Items;

namespace RPG.Entities
{
    [Serializable]
    public class Player : Entity, ISerializable {
        public int RoomCount { get; private set; }
        int lvlXp = 33, xp;
        List<Attack> myAttacks;
        List<Item> items;

        public Player(GameScreen screen, int x, int y, String name, Sprite s) : base(screen, x, y, s, null, 90, 1) {
            this.xp = 0;
            this.Name = name;
            this.RoomCount = 0;

            myAttacks = new List<Attack>();
            items = new List<Item>(12);
        }

        public Player(SerializationInfo info, StreamingContext cntxt) : base (info, cntxt) {
            lvlXp = (int) info.GetValue("Player_Lvlxp", typeof(int));
            xp = (int) info.GetValue("Player_Xp", typeof(int));
            items = (List<Item>) info.GetValue("Player_Items", typeof(List<Item>));
            RoomCount = (int) info.GetValue("Player_RoomCount", typeof(int));

            // Unsaved stuff
            myAttacks = new List<Attack>();
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("Player_Lvlxp", lvlXp);
            info.AddValue("Player_Xp", xp);
            info.AddValue("Player_Items", items);
            info.AddValue("Player_RoomCount", RoomCount);
        }

        public void addXP(int i) {
            xp += i;
        }

        protected override void runAI(TileMap map) {
            foreach (Attack a in myAttacks)
                xp += a.getXP();

            // Level up
            if (xp > lvlXp) {
                xp = 0;
                lvlXp = (int) (lvlXp * 1.5f);
                Stats.levelUp();
            }

            myAttacks.RemoveAll(new Predicate<Attack>(AttackXpAdded));
        }

        private static bool AttackXpAdded(Attack a) {
            return (!a.Alive && !a.HasXP);
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 offset, TimeSpan elapsed) {
            // Draw hp bar
            Rectangle hpRect = new Rectangle(2, 2, 100, 16);
            hpRect.Width = (int) Math.Round(hpRect.Width * Stats.HpPercent) + 1;
            spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Green);

            // Draw level
            spriteBatch.DrawString(ScreenManager.Small_Font, "Lvl " + Stats.Level, new Vector2(105, 0), Color.White);

            // Draw name
            spriteBatch.DrawString(ScreenManager.Small_Font, Name, new Vector2(150, 0), Color.White);

            // Draw entity
            Rectangle pRect = Rect;
            pRect.X -= (int) offset.X;
            pRect.Y -= (int) offset.Y;

            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward()) {
                spriteBatch.Draw(sprite, pRect, Color.White);
            } else {
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }

            // Draw armour stats
            if (Alive) {
                // Draw armour
                Equipment.draw(spriteBatch, offset);
           
                // Draw xp bar
                Rectangle xpBar = new Rectangle(0, spriteBatch.GraphicsDevice.Viewport.Height - 3,
                    (int) (xp / (float)lvlXp * spriteBatch.GraphicsDevice.Viewport.Width), 2);
                spriteBatch.Draw(ScreenManager.WhiteRect, xpBar, Color.LightBlue);

                Viewport vp = spriteBatch.GraphicsDevice.Viewport;
                Rectangle armourImg = new Rectangle(vp.Width - 45, 2, 32, 32);
                Texture2D img;
                switch (State) {
                    case EntityState.Blocking:
                        img = GameScreen.sprGUI[GUISpriteId.Blocking]; break;
                    case EntityState.Crouching:
                        img = GameScreen.sprGUI[GUISpriteId.Ducking]; break;
                    default:
                        img = GameScreen.sprGUI[GUISpriteId.Standing]; break;
                }
                if (isFacingForward())
                    spriteBatch.Draw(img, armourImg, Color.Black);
                else
                    spriteBatch.Draw(img, armourImg, img.Bounds, Color.Black, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.THeadMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 0), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.TBodyMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 11), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.TLegsMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 22), Color.White);
            
                // Draw hit box
                if (showHpTicks > HP_BAR_SHOW_MS / 2) {
                    Rectangle lastRect = EBounds.getRectFromPart(lastHitPart);
                    if (lastRect.Width != 0) {     
                        lastRect.X += (int) (lastRect.Height * 0.1 - offset.X);
                        lastRect.Y -= (int) offset.Y;
                        lastRect.Width = (int) (lastRect.Width * 0.8);
                        spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                    }
                }
            }
        }

        public IEnumerable<Item> itemIterator() {
            return items.AsEnumerable<Item>();
        }

        public int getItemIndex(ItemId id) {
            for (int i=0; i<items.Count; i++) {
                if (items[i].Id == id)
                    return i;
            }
            return -1;
        }

        public bool addItem(Item i) {
            if (i != null) {
                int invIdx = items.IndexOf(i);
                // Stackable and contains
                if (i.Stackable && invIdx != -1 && items[invIdx].Count < Item.MAX_STACK) {
                    Item invItem = items[invIdx], overflowStack;
                    if (invItem.Copy) {
                        overflowStack = invItem.addToStack(i.Count);
                    } else {
                        // Make copy
                        items[invIdx] = new Item(invItem);
                        overflowStack = items[invIdx].addToStack(invItem.Count + i.Count - 1);
                    }
                    // Add extra items to own stack
                    addItem(overflowStack);
                } else if (items.Count < 12) {
                    items.Add(i);
                }
                return true;
            }
            return false;
        }

        public Item removeItem() {
            return removeItem(0);
        }

        public Item removeItem(int idx) {
            if (idx >= 0 && idx < items.Count) {
                Item i = items[0];
                items.RemoveAt(idx);
                return i;
            }
            return null;
        }

        public Item removeItem(Item item) {
            if (item == null) return null;

            int idx = -1;
            for (int i=0; i<items.Count; i++) {
                if (items[i] == item) {
                    idx = i;
                    break;
                }
            }
            return removeItem(idx);
        }
        
        public void newRoom() {
            RoomCount++;
            moveTo(new Vector2(0, (GScreen.TileMap.Height - 2) * TileMap.SPRITE_SIZE));
        }

        public void moveTo(Vector2 targ) {
            EBounds.moveX((int) targ.X - bounds.X);
            EBounds.moveY((int) targ.Y - bounds.Y);
        }

        public void doAttack(TileMap map, EntityPart part) {
            if (Alive) {
                Attack attack = base.attack(map, part, AttackFactory.FireBall);
                if (attack != null) {
                    myAttacks.Add(attack);
                }
            }
        }

        public void doBlock() {
            if (Alive) base.block();
        }

        public void doDuck() {
            if (Alive) base.duck();
        }

        public void doJump() {
            if (Alive) base.jump();
        }

        public void doMove(Direction dir) {
            if (Alive) 
                base.setXSpeedPerMs(Entity.SPEED_PER_MS * (float) dir);
        }

        public void stand() {
            if (Alive) base.setState(EntityState.Standing);
        }
    }
}
