using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Items;

namespace RPG.Entities
{
    [Serializable()]
    public class Equipment : ISerializable
    {
        Entity entity;
        public Armour Head { get; private set; }
        public Armour Body { get; private set; }
        public Armour Legs { get; private set; }

        public Equipment(Entity e) {
            entity = e;
            Head = Body = Legs = Armour.NONE;
        }

        public Equipment(SerializationInfo info, StreamingContext cntxt) {
            Head = (Armour) info.GetValue("Equipment_Head", typeof(Armour));
            Body = (Armour) info.GetValue("Equipment_Body", typeof(Armour));
            Legs = (Armour) info.GetValue("Equipment_Legs", typeof(Armour));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("Equipment_Head", Head);
            info.AddValue("Equipment_Body", Body);
            info.AddValue("Equipment_Legs", Legs);
        }

        public void setEntity(Entity e) {
            if (entity == null)
                entity = e;
            else
                throw new ArgumentException("Entity already set!");
        }
        
        public void setHead(Armour a) { Head = (a != null) ? a : Armour.NONE; }
        public void setBody(Armour a) { Body = (a != null) ? a : Armour.NONE; }
        public void setLegs(Armour a) { Legs = (a != null) ? a : Armour.NONE; }

        public void draw(SpriteBatch spriteBatch, Vector2 offset) {
            Rectangle pRect = entity.Rect;
            pRect.X -= (int) offset.X;
            pRect.Y -= (int) offset.Y;

            Texture2D[] sprite = new Texture2D[3];
            switch (entity.getDrawState()) {
                case EntityState.Crouching:
                    pRect.Y += EntityBounds.SPRITE_DUCK_AMNT;
                    goto case EntityState.Standing; // Fall Through
                case EntityState.AttackCrouch:
                    pRect.Y += EntityBounds.SPRITE_DUCK_AMNT;
                    goto case EntityState.Attacking; 
                case EntityState.Jumping:
                case EntityState.Blocking:
                case EntityState.Moving:
                case EntityState.Standing:
                    sprite[0] = Head.Stand;
                    sprite[1] = Body.Stand;
                    sprite[2] = Legs.Stand;
                    break;
                case EntityState.Attacking:
                    sprite[0] = Head.Attack;
                    sprite[1] = Body.Attack;
                    sprite[2] = Legs.Attack;
                    break;
            }

            for (int i=0; i<sprite.Length; i++) {
                if (sprite[i] != null) {
                    if (entity.isFacingForward())
                        spriteBatch.Draw(sprite[i], pRect, Color.White);
                    else
                        spriteBatch.Draw(sprite[i], pRect, sprite[i].Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                }
        
            }
        }
    }
}
