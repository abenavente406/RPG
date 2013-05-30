using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Screen;
using RPG.Sprites;
using RPG.Entities;
using RPG.Helpers;
using RPG.Items;

namespace RPG.GameObjects
{
    public enum MapType { Hall, Treasure };

    public class TileMap {
        public const int SPRITE_SIZE = 32;

        public readonly GameScreen GScreen;
        public readonly int Width, Height;

        List<TileBlock> Map;
        Texture2D background;
        Vector2 bgScale;
        
        // Keep track of old maps
        public TileMap OldMap;
        public Vector2 LeavePoint;

        int numEntities;
        List<Entity> DeadEntities;
        List<Entity> Entities;
        List<GameObject> GameObjects;
        List<Attack> Attacks;
        List<HitText> HitTexts;

        public TileMap(int width, int height, MapType type, GameScreen screen, TileMap oldMap) {
            this.GScreen = screen;
            this.Width = width;
            this.Height = height;
            this.OldMap = oldMap;

            // Init
            numEntities = 0;
            DeadEntities = new List<Entity>();

            Map = new List<TileBlock>();
            Entities = new List<Entity>();
            GameObjects = new List<GameObject>();
            Attacks = new List<Attack>();
            HitTexts = new List<HitText>();

            
            switch (type) {
            case MapType.Treasure:
                setBackground(GameScreen.Backgrounds[BackgroundId.Cave1]);
                for (int w = 0; w < width; w++) {
                    for (int h = 0; h < height; h++) {
                        if (h == height - 2 && w == 0) {
                            Map.Add(TileBlock.IRON_DOOR.Clone().addEvent(TileBlockEvent.MapGoBack));
                        } else if (h == 0 || h == height - 1) {
                            Map.Add(TileBlock.STONE_WALL.Clone());
                        } else if (w > 2 && w < width - 2 && h == height - 2 && ScreenManager.Rand.Next(width / 3) == 0) {
                            Map.Add(TileBlock.CLOSED_CHEST.Clone());
                        } else {
                            Map.Add(TileBlock.NONE.Clone());
                        }
                    }
                }
                break;
            case MapType.Hall:
            default: // Hall Way
                setBackground(GameScreen.Backgrounds[BackgroundId.Cave1]);
                int specialCount = 0;
                for (int w = 0; w < width; w++)
                    for (int h = 0; h < height; h++) {
                        if (h == height - 2 && w == width - 1) {
                            Map.Add(TileBlock.DOOR.Clone());
                            addRandomEntity(w * SPRITE_SIZE, h * SPRITE_SIZE, screen);
                        } else if (h == 0 || h == height - 1) {
                            Map.Add(TileBlock.STONE_WALL.Clone());
                        } else if (w > 3 && w < width - 2 && h == height - 2 && ScreenManager.Rand.Next(25) == 0) {
                            Map.Add(TileBlock.STONE_WALL.Clone());
                        } else if (w > 3 && w < width - 2 && h == height - 2 && ScreenManager.Rand.Next(25) == 0) {
                            Map.Add(TileBlock.STONE2_WALL.Clone());
                        } else if (specialCount == 0 && w > 2 && h == height - 2 && ScreenManager.Rand.Next(150) == 0) {
                            specialCount++;
                            Map.Add(TileBlock.HPPOOL.Clone());
                        } else if (specialCount == 0 && w > 2 && h == height - 2 && ScreenManager.Rand.Next(200) == 0) {
                            specialCount++;
                            Map.Add(TileBlock.IRON_DOOR.Clone().addEvent(TileBlockEvent.NewTreasureRoom));
                        } else {
                            Map.Add(TileBlock.NONE.Clone());

                            if (h == height - 2 && w > 4 && ScreenManager.Rand.Next(12) == 0)
                                addRandomEntity(w * SPRITE_SIZE, h * SPRITE_SIZE, screen);
                        }
                    }
                break;
            }
        }

        private void setBackground(Texture2D bg) {
            background = bg;
            bgScale = new Vector2(bg.Bounds.Width / (float) (Width * SPRITE_SIZE), bg.Bounds.Height / (float) (Height * SPRITE_SIZE));
        }

        public void update(TimeSpan span) {
            // ### Update
            foreach (HitText t in HitTexts)
                t.update(span);
            HitTexts.RemoveAll(new Predicate<HitText>(IsHitTextGone));

            foreach (Attack a in Attacks)
                a.update(this, span);
            // ## Remove attacks that aren't alive
            Attacks.RemoveAll(new Predicate<Attack>(IsAttackAlive));
            
            foreach (Entity e in Entities) {
                e.update(this, span);
                
                // Only add entitys (not players)
                if (!e.Alive) {
                    if (!(e is Player) && !DeadEntities.Contains(e)) {
                        // Tell the map the entity is dead
                        DeadEntities.Add(e);
                        if (allEntitiesDead())
                            dropItem(GameScreen.Items[ItemId.Key], e);
                    }
                }
            }

            foreach (GameObject o in GameObjects)
                o.update(this, span);
            GameObjects.RemoveAll(new Predicate<GameObject>(DoRemoveGameObject));
        }

        public void draw(SpriteBatch spriteBatch, Vector2 offset, TimeSpan elapsedGameTime) {
            // Draw background to scale based on the TileMap
            Rectangle vb = GScreen.Graphics.GraphicsDevice.Viewport.Bounds;
            spriteBatch.Draw(background, vb, new Rectangle((int) (offset.X * bgScale.X), (int) (offset.Y * bgScale.Y), 
                    (int) (vb.Width * bgScale.X), (int) (vb.Height * bgScale.Y)), Color.White);

            // Draw each from tilemap
            for (int w = 0; w < Width; w++) {
                for (int h = 0; h < Height; h++) {
                    TileBlock b = get(w, h);
                    Texture2D texture = b.getSprite(elapsedGameTime.Milliseconds);
                    if (texture != null) {
                        spriteBatch.Draw(texture, 
                                new Rectangle(
                                    w*SPRITE_SIZE + b.getDrawRectangle().X - (int) offset.X, 
                                    h*SPRITE_SIZE + b.getDrawRectangle().Y - (int) offset.Y,
                                    b.getDrawRectangle().Width, b.getDrawRectangle().Height
                                ), Color.White);
                    }
                }
            }

            // Draw each entity
            foreach (Entity e in Entities)
                e.draw(spriteBatch, offset, elapsedGameTime);

            // Draw game objects
            foreach (GameObject o in GameObjects)
                o.draw(spriteBatch, offset, elapsedGameTime);

            // Draw hit text
            foreach (HitText t in HitTexts)
                t.draw(spriteBatch, offset);

            // Draw attacks
            foreach (Attack a in Attacks) {
                Rectangle rect = a.Rectangle;
                rect.X -= (int) offset.X;
                rect.Y -= (int) offset.Y;
                Texture2D sprite = a.getSprite();
                if (sprite != null) {
                    if (!a.doFlipSprite())
                        spriteBatch.Draw(a.getSprite(), rect, Color.White);
                    else
                        spriteBatch.Draw(a.getSprite(), rect, a.getSprite().Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                }
            }
        }

        public static bool DoRemoveGameObject(GameObject o) {
            return o.SlatedToRemove;
        }

        public static bool IsHitTextGone(HitText ht) {
            return !ht.Alive;
        }

        public static bool IsAttackAlive(Attack attack) {
            return !attack.Alive;
        }

        public NearEntity getNearestEntity(Entity thisEntity, Point tPoint) {
            Entity nEntity = GScreen.Player;
            double dist = HMath.Distance(tPoint, nEntity.Bounds.Center);
            foreach (Entity e in Entities) {
                if (!e.Alive || e == thisEntity) continue;

                double d = HMath.Distance(tPoint, e.Bounds.Center);
                if (d < dist) {
                    dist = d;
                    nEntity = e;
                }
            }
            return new NearEntity(nEntity, dist);
        }

        public Attack getNearestAttack(Entity thisEntity, Point tPoint) {
            Attack nAttack = Attacks[0];
            double dist = HMath.Distance(tPoint, thisEntity.Bounds.Center);
            foreach (Attack a in Attacks) {
                if (a.Owner == thisEntity) continue;

                double d = HMath.Distance(tPoint, a.Rectangle.Center);
                if (d < dist) {
                    dist = d;
                    nAttack = a;
                }
            }
            return nAttack;
        }

        public void addHitText(Entity e, int dmg) {
            HitTexts.Add(new HitText(e, dmg));
        }

        public TileBlock getPixel(int x, int y) {
            return get(shrinkX(x, false), shrinkY(y, false));
        }

        private void setPixel(int x, int y, TileBlock b) {
            set(shrinkX(x, false), shrinkY(y, false), b);
        }

        public TileBlock get(int w, int h) {
            if (w < 0 || h < 0 || w >= Width || h >= Height) {
                throw new Exception("Tried to get a tile block out of range!");
            }

            return Map[h + w * Height];
        }

        private void set(int w, int h, TileBlock b) {
            if (w < 0 || h < 0 || w >= Width || h >= Height) {
                throw new Exception("Tried to get a tile block out of range!");
            }

            Map[h + w * Height] = b;
        }

        public int getPixelWidth() { return Width *  SPRITE_SIZE; }
        public int getPixelHeight() { return Height *  SPRITE_SIZE; }

        public int shrinkX(int x, bool rUp) { 
            // return (int) Math.Round(x / (float)SPRITE_WIDTH);
            if (rUp) return (int) Math.Ceiling(x / (float) SPRITE_SIZE);
            else     return (int) (x / (float) SPRITE_SIZE); 
        }
        public int shrinkY(int y, bool rUp) { 
            // return (int) Math.Round(y / (float) SPRITE_SIZE);
            if (rUp) return (int) Math.Ceiling(y / (float) SPRITE_SIZE);
            else     return (int) (y / (float) SPRITE_SIZE);
        }

        /// Get the non-offset bounds
        public Rectangle getRectPixel(int x, int y) {
            int sX = (int) Math.Round(x / (float)SPRITE_SIZE);
            int sY = (int) Math.Round(y / (float) SPRITE_SIZE);

            // Console.WriteLine(" x = " + x + " y = " + y);
            return getRect(sX, sY);
        }

        public Rectangle getRect(int w, int h) {
            if (w < 0 || h < 0 || w >= Width || h >= Height) // Off screen, no collision
                return new Rectangle(0, 0, 0, 0);

            Rectangle bounds = get(w, h).getBounds();

            bounds.X += w *  SPRITE_SIZE;
            bounds.Y += h *  SPRITE_SIZE;

            return bounds;
        }

        public bool checkCollision(Rectangle r1, Rectangle r2) {
            if (r1.Width == 0 || r1.Height == 0 || r2.Width == 0 || r2.Height == 0)
                return false;
            return r1.Intersects(r2);
        }

        public int checkBoundsXRight(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) Math.Round(rect.Center.X / (float) SPRITE_SIZE), (int) Math.Round(rect.Center.Y / (float) SPRITE_SIZE));
            int leftmost = rect.Right;
            for (int y = (int) pos.Y - 1; y < pos.Y + 1; y++) {
                // Console.Write("  X-Right: ");
                Rectangle objRect = getRect((int) pos.X, y);

                if (objRect.Left < leftmost && checkCollision(rect, objRect))
                     leftmost = objRect.Left;
            }

            return leftmost - rect.Width; // Move back to the top left corner
        }

        public int checkBoundsXLeft(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) (rect.Left / SPRITE_SIZE), (int) Math.Round(rect.Center.Y / (float) SPRITE_SIZE));
            int rightmost = rect.Left;
            for (int y = (int) pos.Y - 1; y < pos.Y + 1; y++) {
                // Console.Write("  X-Left: ");
                Rectangle objRect = getRect((int) pos.X, y);
            
                if (objRect.Right > rightmost && checkCollision(rect, objRect))
                    rightmost = objRect.Right;
            }
            return rightmost;
        }

        public int checkBoundsYDown(Rectangle rect) {
            // Console.WriteLine("Checking");
            
            Vector2 pos = new Vector2((int) Math.Round(rect.Center.X / (float) SPRITE_SIZE), (int) Math.Round(rect.Center.Y / (float) SPRITE_SIZE));
            int topmost = rect.Bottom;
            for (int x = (int) pos.X - 1; x < pos.X + 1; x++) {
                // Console.Write("  Y-Down: ");
                Rectangle objRect = getRect(x, (int) pos.Y);
            
                if (objRect.Top < topmost && checkCollision(rect, objRect))
                    topmost = objRect.Top;
            }

            return topmost - rect.Height; // Move back to the top left corner
        }

        public int checkBoundsYUp(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) (rect.Center.X / SPRITE_SIZE), (int) (rect.Top / SPRITE_SIZE));
            int bottommost = rect.Top;
            for (int x = (int) pos.X - 1; x < pos.X + 1; x++) {
                // Console.Write("  Y-Up: ");
                Rectangle objRect = getRect(x, (int) pos.Y);
            
                if (objRect.Bottom > bottommost && checkCollision(rect, objRect))
                    bottommost = objRect.Bottom;
            }

            return bottommost;
        }

        public bool isRectOnFloor(Rectangle rect) {
            // Console.WriteLine("Checking");

            rect.Y += 2;
            Vector2 bottom = new Vector2(shrinkX(rect.Center.X, false), shrinkY(rect.Bottom, false));
            for (int y = (int) bottom.Y; y < bottom.Y + 1; y++) { 
                for (int x = (int) bottom.X - 1; x < bottom.X + 1; x++) {
                    // Console.Write("  On floor:");
                    Rectangle objRect = getRect(x, y);
                    if (checkCollision(rect, objRect))
                        return true;
                }
            }
            return false;
        }

        public IEnumerable<Entity> entityIterator() {
            return Entities.AsEnumerable<Entity>();
        }

        public bool allEntitiesDead() {
            return (numEntities == DeadEntities.Count);
        }

        public void addRandomEntity(int x, int y, GameScreen screen) {
            Entity e;
            int rand = ScreenManager.Rand.Next(500);
            if (rand < 10)
                e = EntityFactory.Skeleton_King(x, y, screen, 1 + (screen.Player.RoomCount/30f)); // After 30 rooms twice as hard
            else if (rand < 255)
                e = EntityFactory.Wraith(x, y, screen, 1 + (screen.Player.RoomCount/30f));
            else
                e = EntityFactory.Warlock(x, y, screen, 1 + (screen.Player.RoomCount/30f));
                
            addEntity(e);
        }

        public void addEntity(Entity e) {
            if (!Entities.Contains(e)) {
                // Only count real entites, not subclasses
                if (e.GetType() == typeof(Entity))
                    numEntities++;

                Entities.Add(e);
            }
        }

        public void dropItem(Item item, Entity e) {
            // TileBlock b = getPixel(item.Bounds.X, item.Bounds.Y);
            int sX = shrinkX(e.Bounds.Center.X, false);
            int sY = shrinkY(e.Bounds.Center.Y, false);

            // Get the floor
            TileBlock b = get(sX, sY);
            while (b.isWalkable()) {
                b = get(sX, ++sY);
            }
            // Up one from the floor
            b = get(sX, --sY);

            EItem eitem = new EItem(item, sX * TileMap.SPRITE_SIZE + 
                    (TileMap.SPRITE_SIZE/2-EItem.DROP_SIZE/2) + ScreenManager.Rand.Next(-4, 5), e.Bounds.Y);

            if (!b.isShared()) {
                b = new TileBlock(b);
                set(sX, sY, b);
            }
            b.addGameObject(eitem);

            addGameObject(eitem);
        }

        public void addGameObject(GameObject o) {
            GameObjects.Add(o);
        }

        public void addAttack(Attack a) {
            Attacks.Add(a);
        }
    }
}
