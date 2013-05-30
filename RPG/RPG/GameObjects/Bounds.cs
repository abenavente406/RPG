using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;

namespace RPG.GameObjects
{
    [Serializable]
    public class Bounds : ISerializable
    {
        protected Rectangle rect;
        protected bool canCollide;

        public Bounds(int x, int y, int width, int height) {
            rect = new Rectangle(x, y, width, height);
            canCollide = false;
        }

        public Bounds(SerializationInfo info, StreamingContext cntxt) {
            rect = (Rectangle) info.GetValue("Bounds_Rect", typeof(Rectangle));
            canCollide = (bool) info.GetValue("Bounds_CanCollide", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("Bounds_Rect", rect);
            info.AddValue("Bounds_CanCollide", canCollide);
        }

        public bool doesCollide(Rectangle otherRect) {
            return canCollide && rect.Intersects(otherRect);
        }

        public virtual void moveY(int y) {
            rect.Y += y;
        }

        public virtual void moveX(int x) {
            rect.X += x;
        }

        public int X { get { return rect.X; } }
        public int Y { get { return rect.Y; } }

        public int Height { get { return rect.Height; } }
        public int Width { get { return rect.Width; } }

        public Rectangle Rect { get { return rect; } }
        public Vector2 Location { get { return new Vector2(rect.Left, rect.Top); } }

        public int Top { get { return rect.Top; } }
        public int Bottom { get { return rect.Bottom; } }
        public int Left { get { return rect.Left; } }
        public int Right { get { return rect.Right; } }
        public Point Center { get { return rect.Center; } }
    }
}
