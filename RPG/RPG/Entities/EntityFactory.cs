using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Sprites;
using RPG.Items;
using RPG.Helpers;

namespace RPG.Entities
{
    public class EntityFactory
    {
        public static Entity Wraith(int x, int y, GameScreen screen, float scalar=1) {
            return new Entity(screen, x, y, screen.SprEntity[EntitySpriteId.Wraith], EntityAIs.Wraith, (int) (45 * scalar), 0.9 * scalar, (int) (8 * scalar), 0.85f,
                new PossibleDrop[] {
                    new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.05),
                    new PossibleDrop(GameScreen.Items[ItemId.Key], 0.01),
                    new PossibleDrop(GameScreen.Items[ItemId.BronzeBody], 0.03),
                    new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.25)
                });
        }

        public static Entity Warlock(int x, int y, GameScreen screen, float scalar=1) {
            return new Entity(screen, x, y, screen.SprEntity[EntitySpriteId.Warlock], EntityAIs.Basic, (int) (60 * scalar), 0.9 * scalar, (int) (6 * scalar), 1f, 
                new PossibleDrop[] {
                    new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.05),
                    new PossibleDrop(GameScreen.Items[ItemId.BronzeHead], 0.03),
                    new PossibleDrop(GameScreen.Items[ItemId.BronzeLegs], 0.03),
                    new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.25)
                });
        }

        public static Entity Skeleton_King(int x, int y, GameScreen screen, float scalar=1) {
            String name = RName.newCoolName();
            Entity e = new Entity(screen, x, y, screen.SprEntity[EntitySpriteId.Skeleton_King], EntityAIs.Skeleton_King, 120, 1.5, 20, 1.1f,
                new PossibleDrop[] {
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronHead], name + "'s Helm"), 0.1),
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronBody], name + "'s Platebody"), 0.1),
                    new PossibleDrop(new Item(GameScreen.Items[ItemId.IronLegs], name + "'s Legs"), 0.1),
                    new PossibleDrop(GameScreen.Items[ItemId.SmallPotion], 0.33),
                    new PossibleDrop(GameScreen.Items[ItemId.Gold], 0.5)
                });
            e.Name = name;
            return e;
        }
    }
}
