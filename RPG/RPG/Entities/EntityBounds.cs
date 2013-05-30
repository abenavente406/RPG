using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;

using RPG.Helpers;
using RPG.GameObjects;

namespace RPG.Entities
{
    public enum EntityPart { Head, Body, Legs, None, Miss };
    public struct EntityHit {
        public static EntityHit NONE = new EntityHit(EntityPart.None, 0);
        public static EntityHit MISS = new EntityHit(EntityPart.Miss, 0);

        public EntityPart Part;
        public float PercFromCenter;
        public EntityHit(EntityPart part, float percFromCenter) { 
            Part = part; PercFromCenter = percFromCenter;
        }
    }

    [Serializable]
    public class EntityBounds : Bounds, ISerializable {
        Rectangle standRect;
        readonly int HEAD_HEIGHT, BODY_HEIGHT, LEGS_HEIGHT;
        Rectangle head, body, legs;

        public const int SPRITE_DUCK_AMNT = 3;
        private static Rectangle ZERO_RECT = new Rectangle(0, 0, 0, 0);
        protected Entity entity;

        public EntityBounds(SerializationInfo info, StreamingContext cntxt) : base(info, cntxt) {
            standRect = (Rectangle) info.GetValue("EBounds_StandRect", typeof(Rectangle));
            HEAD_HEIGHT = (int) info.GetValue("EBounds_HeadHeight", typeof(int));
            BODY_HEIGHT = (int) info.GetValue("EBounds_BodyHeight", typeof(int));
            LEGS_HEIGHT = (int) info.GetValue("EBounds_LegsHeight", typeof(int));
            head = (Rectangle) info.GetValue("EBounds_HeadRect", typeof(Rectangle));
            body = (Rectangle) info.GetValue("EBounds_BodyRect", typeof(Rectangle));
            legs = (Rectangle) info.GetValue("EBounds_LegsRect", typeof(Rectangle));
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("EBounds_StandRect", standRect);
            info.AddValue("EBounds_HeadHeight", HEAD_HEIGHT);
            info.AddValue("EBounds_BodyHeight", BODY_HEIGHT);
            info.AddValue("EBounds_LegsHeight", LEGS_HEIGHT);
            info.AddValue("EBounds_HeadRect", head);
            info.AddValue("EBounds_BodyRect", body);
            info.AddValue("EBounds_LegsRect", legs);
        }

        public EntityBounds(Entity e, int x, int y, int width, int headHeight, int bodyHeight, int legsHeight, int baseWidth) :
                base(x, y, width, headHeight + bodyHeight + legsHeight) {
            entity = e;

            standRect = new Rectangle(x + (rect.Width / 2) - (baseWidth / 2), y, baseWidth, rect.Height);

            HEAD_HEIGHT = headHeight;
            head = new Rectangle(x, y, width, HEAD_HEIGHT);

            BODY_HEIGHT = bodyHeight;
            body = new Rectangle(x, y + HEAD_HEIGHT, width, BODY_HEIGHT);

            LEGS_HEIGHT = legsHeight;
            legs = new Rectangle(x, y + HEAD_HEIGHT + BODY_HEIGHT, width, LEGS_HEIGHT);

            canCollide = true;
            resetPositions();
        }

        public void setEntity(Entity e) {
            if (this.entity == null)
                this.entity = e;
            else
                throw new ArgumentException("Entity already set!");
        }

        public void resetPositions() {
            head = new Rectangle(rect.X, rect.Y, rect.Width, HEAD_HEIGHT);
            body = new Rectangle(rect.X, rect.Y + HEAD_HEIGHT, rect.Width, BODY_HEIGHT);
            legs = new Rectangle(rect.X, rect.Y + HEAD_HEIGHT + BODY_HEIGHT, rect.Width, BODY_HEIGHT);
        }

        public void die() {
            // No colliding
            canCollide = false;
        }

        public void duck() {
            resetPositions();

            head.Y += SPRITE_DUCK_AMNT + 1;
            body.Y += SPRITE_DUCK_AMNT + 1;
            body.Height -= 1;
            legs.Height -= SPRITE_DUCK_AMNT;
        }

        public void block(Direction dir) {
            resetPositions();

            body.Width /= 2;
            if (dir == Direction.Left)
                body.X += body.Width / 2;
        }

        public EntityHit collide(Point p) {
            if (canCollide && rect.Contains(p)) {
                if (head.Contains(p)) {
                     return new EntityHit(EntityPart.Head, Math.Abs(head.Center.Y - p.Y) / (float) head.Height);
                } else if (body.Contains(p)) {
                    return new EntityHit(EntityPart.Body, Math.Abs(body.Center.Y - p.Y) / (float) body.Height);
                } else if (legs.Contains(p)) {
                    return new EntityHit(EntityPart.Legs, Math.Abs(legs.Top - p.Y) / (float) legs.Height);
                } else {
                    return EntityHit.MISS;
                }
            }

            return EntityHit.NONE;
        }

        public int getFireFrom(EntityPart part) {
            if (part == EntityPart.Legs)
                return legs.Top + 1;
            else if (part == EntityPart.Head)
                return head.Center.Y;
            else
                return body.Center.Y;
        }

        public Rectangle getRectFromPart(EntityPart part) {
            if (part == EntityPart.Body)
                return body;
            else if (part == EntityPart.Head)
                return head;
            else if (part == EntityPart.Legs)
                return legs;

            return ZERO_RECT;
        }

        public override void moveY(int y) {
            base.moveY(y);
            
            standRect.Y += y;
            head.Y += y;
            body.Y += y;
            legs.Y += y;
        }

        public override void moveX(int x) {
            base.moveX(x);

            standRect.X += x;
            head.X += x;
            body.X += x;
            legs.X += x;
        }

        public Rectangle LegsRect { get { return legs; } }
        public Rectangle BodyRect { get { return body; } }
        public Rectangle HeadRect { get { return head; } }
        public Rectangle StandRect { get { return standRect; } }
    }
}
