using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Entities;

namespace RPG.GameObjects
{
    public class Attack
    {
        public readonly Entity Owner;

        int xp;
        int dmg;
        bool alive;
        Texture2D sprite;
        Rectangle drawRect, collRect;
        TimeSpan lastElapsed;
        float msSpeed;
        bool horizontal;
        int maxdist, distTraveled;

        public Attack(TileMap map, Entity owner, Texture2D sprite, Rectangle rect, int dmg, float msSpeed, int maxdist, bool horizontal=true) {
            Owner = owner;

            this.xp = 0;
            this.alive = true;
            this.sprite = sprite;
            this.drawRect = rect;
            this.msSpeed = msSpeed;
            this.horizontal = horizontal;
            this.dmg = dmg;

            if (horizontal) {
                if (msSpeed > 0) {
                    this.collRect = new Rectangle(rect.X, rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
                } else {
                    this.collRect = new Rectangle(rect.X + (int) (rect.Width * 0.2), rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
                }
            } else {
                if (msSpeed > 0) {
                    this.collRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - (int) (rect.Height * 0.2));
                } else {
                    this.collRect = new Rectangle(rect.X, rect.Y + (int) (rect.Height * 0.2), rect.Width, rect.Height - (int) (rect.Height * 0.2));
                }
            }
            this.maxdist = maxdist;
            this.distTraveled = 0;

            // Initial bounds text with all containing tiles
            int maxX = map.shrinkX(rect.Right, true);
            int maxY = map.shrinkY(rect.Bottom, true);
            for (int x = map.shrinkX(rect.Left, false); x <= maxX; x++) {
                for (int y = map.shrinkY(rect.Top, false); y <= maxY; y++) {
                    Rectangle wallRect = map.getRect(x, y);
                    if (map.checkCollision(rect, wallRect)) {
                        alive = false;
                        break;
                    }
                }

                if (!alive) break;
            }
        }

        public void update(TileMap map, TimeSpan elapsed) {
            if (alive) {
                lastElapsed = elapsed;

                if (horizontal) {
                    drawRect.X += getRealSpeed();
                    collRect.X += getRealSpeed();
                } else {
                    drawRect.Y += getRealSpeed();
                    collRect.Y += getRealSpeed();
                }

                distTraveled += Math.Abs(getRealSpeed());

                if (distTraveled > maxdist || drawRect.Right < 0 || drawRect.Left >= map.getPixelWidth()) {
                    alive = false;
                } else {
                    // Test walls based on direction (left, right)
                    if (horizontal) {
                        int x;
                        if (msSpeed > 0) x = map.shrinkX(collRect.Right, false);
                        else x = map.shrinkX(collRect.Left, false);

                        int maxY = map.shrinkY(collRect.Bottom, true);
                        for (int y = map.shrinkY(collRect.Top, false); y <= maxY; y++) {
                            Rectangle wallRect = map.getRect(x, y);

                            if (map.checkCollision(collRect, wallRect)) {
                                alive = false;
                                return;
                            }
                        }
                    } else {
                        int y;
                        if (msSpeed > 0) y = map.shrinkX(collRect.Bottom, false);
                        else y = map.shrinkX(collRect.Top, false);

                        int maxX = map.shrinkX(collRect.Left, true);
                        for (int x = map.shrinkX(collRect.Right, false); x <= maxX; x++) {
                            Rectangle wallRect = map.getRect(x, y);

                            if (map.checkCollision(collRect, wallRect)) {
                                alive = false;
                                return;
                            }
                        }
                    }

                    // Test collision with entites
                    foreach (Entity e in map.entityIterator()) {
                        if (!e.Alive) 
                            continue;

                        EntityHit eHit;
                        if (horizontal) {
                            eHit = e.EBounds.collide(new Point(collRect.Right, collRect.Center.Y));
                            // If not hit in front, check back
                            if (eHit.Part == EntityPart.None)
                                eHit = e.EBounds.collide(new Point(collRect.Left, collRect.Center.Y));
                        } else {
                            int y;
                            if (msSpeed > 0) y = collRect.Bottom;
                            else y = collRect.Top;

                            eHit = e.EBounds.collide(new Point(collRect.Left, y));
                            // If not hit on left, check right
                            if (eHit.Part == EntityPart.None)
                                eHit = e.EBounds.collide(new Point(collRect.Right, y));
                        }

                        if (eHit.Part != EntityPart.None) {
                            alive = false;
                            if (eHit.Part != EntityPart.Miss) {
                                float dmgReducer = ((eHit.PercFromCenter < 0.6) ? 1 - eHit.PercFromCenter : 0.4f);
                                int realDmg = e.hitInThe(eHit.Part, dmg, dmgReducer);
                                map.addHitText(e, realDmg);
                                if (!e.Alive)
                                    xp += e.XPValue;
                            }
                            return;
                        }
                    }
                }
            }
        }

        public bool doFlipSprite() {
            return (horizontal && msSpeed < 0);
        }

        public Texture2D getSprite() {
            if (alive)
                return sprite;
            else
                return null;
        }

        protected int getRealSpeed() { return (int) (msSpeed * lastElapsed.Milliseconds); }

        public int Damage { get { return dmg; } }
        public Rectangle Rectangle { get { return drawRect; } }
        public Point Location { get { return drawRect.Location; } }
        public bool Alive { get { return alive; } }
        public bool HasXP { get { return xp > 0; } }

        public int getXP() {
            if (xp > 0) {
                xp--;
                return 1;
            } else {
                return 0;
            }
        }
    }
}
