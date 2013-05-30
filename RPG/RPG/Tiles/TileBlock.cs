using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Sprites;
using RPG.Screen;
using RPG.Entities;
using RPG.Items;

namespace RPG.GameObjects
{
    public class TileBlock {
        public static readonly Rectangle NORECT = new Rectangle(0, 0, 0, 0);

        public static readonly TileBlock NONE = new TileBlock(TerrainSpriteId.None, NORECT);
        public static readonly TileBlock STONE_WALL = new TileBlock(TerrainSpriteId.Stone_Wall, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE));
        public static readonly TileBlock STONE2_WALL = new TileBlock(TerrainSpriteId.Stone2_Wall, new Rectangle(0, TileMap.SPRITE_SIZE/2, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE/2));
        public static readonly TileBlock DOOR = new TileBlock(TerrainSpriteId.Door, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false).addEvent(TileBlockEvent.NewMainRoom);
        public static readonly TileBlock HPPOOL = new TileBlock(GameScreen.Animations[Animation.RedSpiral], new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false).addEvent(TileBlockEvent.WallHeal);
        public static readonly TileBlock EMPTYMAGIC_WALL = new TileBlock(TerrainSpriteId.EmptyMagicWall, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false);
        public static readonly TileBlock CLOSED_CHEST = new TileBlock(TerrainSpriteId.ClosedChest, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false).addEvent(TileBlockEvent.OpenChest);
        public static readonly TileBlock OPEN_CHEST = new TileBlock(TerrainSpriteId.OpenChest, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false);
        public static readonly TileBlock IRON_DOOR = new TileBlock(TerrainSpriteId.IronDoor, new Rectangle(0, 0, TileMap.SPRITE_SIZE, TileMap.SPRITE_SIZE), false);

        Sprite sprite;
        Rectangle bounds;        
        bool collide;
        Queue<Func<GameScreen, TileBlock, bool>> intEvents;
        List<GameObject> gameObjects;

        private TileBlock(Sprite sprite, Rectangle bounds, bool collide=true) {
            this.sprite = sprite.Clone();
            this.bounds = bounds;
            this.collide = collide;
            this.intEvents = new Queue<Func<GameScreen, TileBlock, bool>>();
            this.gameObjects = null;
        }

        private TileBlock(TerrainSpriteId spriteId, Rectangle bounds, bool collide=true) {
            this.sprite = new Sprite(GameScreen.sprTerrains[spriteId]);
            this.bounds = bounds;
            this.collide = collide;
            this.intEvents = new Queue<Func<GameScreen, TileBlock, bool>>();
            this.gameObjects = null;
        }

        public TileBlock(TileBlock tb) {
            gameObjects = new List<GameObject>();
            intEvents = new Queue<Func<GameScreen,TileBlock,bool>>();

            setTile(tb);
        }

        public TileBlock Clone() {
            TileBlock b = new TileBlock(this);

            // Copy event list
            Func<GameScreen, TileBlock, bool>[] ies = new Func<GameScreen,TileBlock,bool>[this.intEvents.Count];
            this.intEvents.CopyTo(ies, 0);
            b.intEvents = new Queue<Func<GameScreen, TileBlock, bool>>(ies);

            return b;
        }

        public bool isWalkable() {
            return (!collide || bounds.Height < TileMap.SPRITE_SIZE || bounds.Width <= 0);
        }

        public bool isShared() {
            // Is gameObjects is null then this is one of the static TileBlocks from above
            return (gameObjects != null);
        }

        public void setTile(TileBlock tb=null) {
            if (isShared()) {
                if (tb == null) {
                    collide = false;
                    sprite = new Sprite(GameScreen.sprTerrains[TerrainSpriteId.None]);
                } else {
                    this.sprite = tb.sprite.Clone();
                    this.bounds = tb.bounds;
                    this.collide = tb.collide;
                }
            }
        }

        public IEnumerable<GameObject> gameObjecterIterator() {
            return gameObjects.AsEnumerable<GameObject>();
        }

        public void clearObjects(Predicate<GameObject> predicate=null) {
            if (gameObjects == null)
                return;
            else if (predicate == null)
                gameObjects.Clear();
            else
                gameObjects.RemoveAll(predicate);
        }

        public bool addGameObject(GameObject o) {
            if (gameObjects != null) {
                gameObjects.Add(o);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an event to be run on interact. The events are run in LIFO
        /// </summary>
        /// <param name="intEvent">The event to run</param>
        /// <returns>This</returns>
        public TileBlock addEvent(Func<GameScreen, TileBlock, bool> intEvent) {
            if (!intEvents.Contains(intEvent))
                intEvents.Enqueue(intEvent);

            return this;
        }

        public void interact(GameScreen gs) {
            // Give player all items on this tile
            foreach (GameObject o in gameObjects) {
                if (o is EItem) {
                    bool added = gs.Player.addItem(((EItem) o).Item);
                    if (added) {
                        o.SlatedToRemove = true;
                    }
                }
            }

            var rmnEvents = new Queue<Func<GameScreen,TileBlock,bool>>(intEvents.Count);
            // Run the last event added first
            foreach (var evnt in intEvents) {
                bool remove = evnt(gs, this);
                if (!remove) {
                    rmnEvents.Enqueue(evnt);
                }
            }            
            // Set the events to the remaining events
            intEvents = rmnEvents;

            // Remove game objects marked for removal
            clearObjects(new Predicate<GameObject>(TileMap.DoRemoveGameObject));
        }

        public Rectangle getDrawRectangle() { return bounds; }
        public Rectangle getBounds() { return (collide) ? bounds : NORECT; }

        public Texture2D getSprite(int elapsedTime) {
            sprite.tick(elapsedTime);
            return sprite.getFrame();
        }
    }
}