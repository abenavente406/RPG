using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Entities;
using RPG.Sprites;
using RPG.GameObjects;
using RPG.Screen;

namespace RPG.GameObjects
{
    [Serializable()]
    public class GameObject : ISerializable
    {
        public bool SlatedToRemove;

        protected Bounds bounds; // Top Left Tile
        protected Sprite sprite;
        protected bool isOnFloor;

        public GameObject(Texture2D tex, int x, int y, int width=TileMap.SPRITE_SIZE/2, int height=TileMap.SPRITE_SIZE/2) 
            : this(new Sprite(tex), x, y, width, height) { }
        
        public GameObject(Sprite s, int x, int y, int width=TileMap.SPRITE_SIZE, int height=TileMap.SPRITE_SIZE) {
            sprite = s;
            bounds = new Bounds(x, y, width, height);
            isOnFloor = true;
        }

        public GameObject(SerializationInfo info, StreamingContext cntxt) {
            SlatedToRemove = (bool) info.GetValue("GameObject_SlatedToRemove", typeof(bool));
            bounds = (Bounds) info.GetValue("GameObject_Bounds", typeof(Bounds));
            sprite = GameScreen.sprEntities[(EntitySpriteId) info.GetValue("GameObject_EntitySpriteId", typeof(EntitySpriteId))].Clone();
            isOnFloor = (bool) info.GetValue("GameObject_IsOnFloor", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("GameObject_SlatedToRemove", SlatedToRemove);
            info.AddValue("GameObject_Bounds", bounds);
            // TODO support multiple sprites
            info.AddValue("GameObject_EntitySpriteId", EntitySpriteId.Warrior);
            info.AddValue("GameObject_IsOnFloor", isOnFloor);
        }

        public virtual void update(TileMap map, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            isOnFloor = map.isRectOnFloor(bounds.Rect);

            if (!isOnFloor)
                bounds.moveY(2);
        }

        public virtual void draw(SpriteBatch spriteBatch, Vector2 offset, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            Rectangle pRect = Rect;
            pRect.X -= (int) offset.X;
            pRect.Y -= (int) offset.Y;

            spriteBatch.Draw(sprite.Base, pRect, Color.White);
        }
        
        public Sprite Sprite { get { return sprite; } }
        public Bounds Bounds { get { return bounds; } }
        public Vector2 Location { get { return bounds.Location; } }
        public Rectangle Rect { get { return bounds.Rect; } }
    }
}
