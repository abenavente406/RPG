using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.GameObjects;
using RPG.Screen;
using RPG.Helpers;

namespace RPG.Entities
{
    public class EntityAIs
    {
        public static bool Skeleton_King(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);
            
            if (e.State == EntityState.Standing && e.getSpeedX() <= 0)
                e.setXSpeedPerMs(Entity.SPEED_PER_MS);

            if (r < 15) {
                e["run"] = false;
                e.setXSpeedPerMs(Math.Sign(e.getSpeedX()) * -1 * Entity.SPEED_PER_MS);
            } else if (!e.DidMove && e["run"] != null && (bool) e["run"] == true) {
                // If running and hit wall try to jump over
                e.jump();
                e.setXSpeedPerMs(Math.Sign(e.getSpeedX()) * Entity.SPEED_PER_MS);
            } else if (r < 150) {
                NearEntity nEntity = map.getNearestEntity(e, e.Rect.Center);
                if (nEntity.Distance < AttackFactory.RAISE_DEATH_WIDTH * 3.75) {
                    e["run"] = true;
                    // Too close, run
                    if (nEntity.Entity.Rect.Center.X > e.Rect.Center.X)
                        e.setXSpeedPerMs(-Entity.SPEED_PER_MS);
                    else
                        e.setXSpeedPerMs(Entity.SPEED_PER_MS);
                } else if (nEntity.Distance < AttackFactory.RAISE_DEATH_WIDTH * 7.25) {
                    e["run"] = false;
                    // Right range, attack
                    if (nEntity.Entity.Rect.Center.X > e.Rect.Center.X)
                        e.setXSpeedPerMs(0.05f);
                    else
                        e.setXSpeedPerMs(-0.05f);
                    e.attack(map, EntityPart.Body, AttackFactory.Raise_Death);
                }
            }

            return true;
        }

        public static bool Wraith(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);
            
            if (e.State == EntityState.Standing && e.getSpeedX() <= 0)
                e.setXSpeedPerMs(Entity.SPEED_PER_MS);

            if (r < 35) {
                Entity nEntity = map.getNearestEntity(e, e.Rect.Center).Entity;
                if (nEntity.Rect.Center.X > e.Rect.Center.X)
                    e.setXSpeedPerMs(Entity.SPEED_PER_MS);
                else
                    e.setXSpeedPerMs(-Entity.SPEED_PER_MS);
            } else if (r < 60) {
                e.attack(map, EntityPart.Body, AttackFactory.Iceball);
            } else if (r < 80) {
                Entity nEntity = map.getNearestEntity(e, e.Rect.Center).Entity;
                if (nEntity.Rect.Center.X > e.Rect.Center.X)
                    e.setXSpeedPerMs(Entity.SPEED_PER_MS);
                else
                    e.setXSpeedPerMs(-Entity.SPEED_PER_MS);
            } else if (r < 90) {
                e.setXSpeedPerMs(-e.getSpeedX());
            } else if (r < 100) {
                e.block();
            } else if (r < 110) {
                e.jump();
            } else if (r < 120) {
                e.duck();
            }

            return true;
        }

        public static bool Basic(Entity e, TileMap map) {
            // Basic random AI
            int r = ScreenManager.Rand.Next(500);

            if (e.State == EntityState.Standing && e.getSpeedX() <= 0)
                e.setXSpeedPerMs(Entity.SPEED_PER_MS);

            if (r < 10) {
                e.setXSpeedPerMs(-e.getSpeedX());
            } else if (r < 75) {
                if (r < 25) e.attack(map, EntityPart.Head, AttackFactory.Scurge_Shot);
                else if (r < 40) e.attack(map, EntityPart.Body, AttackFactory.Scurge_Shot);
                else e.attack(map, EntityPart.Legs, AttackFactory.Scurge_Shot);
            } else if (r < 85) {
                e.jump();
            } else if (r < 95) {
                e.duck();
            } else if (r < 105) {
                e.block();
            }

            return true;
        }
    }
}
