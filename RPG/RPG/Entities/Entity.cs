using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Sprites;
using RPG.Screen;
using RPG.Helpers;
using RPG.GameObjects;
using RPG.Items;

namespace RPG.Entities
{
    public enum EntityState { Attacking, AttackCrouch, Standing, Jumping, Moving, Crouching, Blocking, Dying, Dead };

    [Serializable]
    public class Entity : GameObject, ISerializable {
        // Static
        public const float SPEED_PER_MS = 0.1f;
        public const float JUMP_PER_MS = 0.4f;
        public const float GRAVITY_PER_MS = 0.0675f;

        public const int JUMP_DELAY_MS = 300;
        public const int ATTACK_DELAY_MS = 600;
        public const int HP_BAR_SHOW_MS = 3000;

        public const float BASE_HEAD_MULT = 1.125f;
        public const float BASE_BODY_MULT = 1.0f;
        public const float BASE_LEGS_MULT = 0.75f;

        public static readonly Color HIT_BOX_COLOR = Color.Lerp(Color.Red, Color.Transparent, 0.8f);
        protected Dictionary<string, object> properties = new Dictionary<string,object>();

        // Entity States
        public GameScreen GScreen;

        public string Name;
        public Equipment Equipment;
        public EntityStats Stats;
        public PossibleDrop[] pDrops;

        public readonly int XPValue;

        // Display States
        private Direction facing;
        public EntityState State { get; private set; }

        private readonly float MAX_SPEED;
        private float speedMultiplier;
        protected int jumpDelay, attackDelay;
        public bool DidMove { get; private set; }

        protected EntityPart lastHitPart;
        protected int showHpTicks;

        protected TimeSpan tElapsed;
        protected Vector2 msVel;

        protected Func<Entity, TileMap, bool> ai;

        public Entity(SerializationInfo info, StreamingContext cntxt) : base(info, cntxt) {
            Name = (string) info.GetValue("Entity_Name", typeof(string));
            Equipment = (Equipment) info.GetValue("Entity_Equipment", typeof(Equipment));
            Stats = (EntityStats) info.GetValue("Entity_Stats", typeof(EntityStats));
            XPValue = (int) info.GetValue("Entity_XPValue", typeof(int));
            facing = (Direction) info.GetValue("Entity_Facing", typeof(Direction));
            MAX_SPEED = (float) info.GetValue("Entity_MaxSpeed", typeof(float));
            msVel = (Vector2) info.GetValue("Entity_MsVel", typeof(Vector2));
            bounds = (EntityBounds) info.GetValue("Entity_EBounds", typeof(EntityBounds));
            State = (EntityState) info.GetValue("Entity_State", typeof(EntityState));

            // Init entity in loaded classes
            Stats.setEntity(this);
            EBounds.setEntity(this);
            Equipment.setEntity(this);

            // Un-saved values
            GScreen = (GameScreen) ScreenManager.getScreen(ScreenId.Game);
            lastHitPart = EntityPart.Body;
            jumpDelay = attackDelay = showHpTicks = 0;
            speedMultiplier = MAX_SPEED;
            pDrops = new PossibleDrop[0];
            sprite.setFrame(250, 3);
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            base.GetObjectData(info, cntxt);

            info.AddValue("Entity_Name", Name);
            info.AddValue("Entity_Equipment", Equipment);
            info.AddValue("Entity_Stats", Stats);
            info.AddValue("Entity_XPValue", XPValue);
            info.AddValue("Entity_Facing", facing);
            info.AddValue("Entity_MaxSpeed", MAX_SPEED);
            info.AddValue("Entity_MsVel", msVel);
            info.AddValue("Entity_State", State);
            info.AddValue("Entity_EBounds", bounds);
        }

        public Entity(GameScreen screen, int x, int y, Sprite s, Func<Entity, TileMap, bool> ai, 
            int hp=75, double ap=1, int xp=6, float speed=1, PossibleDrop[] pDrops=null) : base(s, x, y) 
        {
            this.ai = ai;
            GScreen = screen;
            Name = RName.newName();
            bounds = new EntityBounds(this, x, y, (int) TileMap.SPRITE_SIZE, 12, 14, 6, 24);
            State = EntityState.Standing;
            msVel = new Vector2(0, 0);
            facing = Direction.Right;
            jumpDelay = attackDelay = showHpTicks = 0;
            MAX_SPEED = speedMultiplier = speed;

            XPValue = xp;

            // Stats
            Equipment = new Equipment(this);
            Stats = new EntityStats(this, hp, (float) ap);

            // Initialize drops, if none given empty array
            this.pDrops = (pDrops == null) ? new PossibleDrop[0] : pDrops;

            // Due to a quirk in the code the frame count here must be one more than actual
            sprite.setFrame(250, 3);
            if (!sprite.hasSpriteParts(SpriteParts.Entity))
                throw new ArgumentException("The sprite passed is not an Entity sprite");
        }

        protected virtual void runAI(TileMap map) {
            EntityAIs.Basic(this, map);
        }

        public override void update(TileMap map, TimeSpan elapsed) {
            if (SlatedToRemove) return;

            tElapsed = elapsed;
            isOnFloor = map.isRectOnFloor(EBounds.StandRect);

            if (attackDelay > 0)
                attackDelay -= elapsed.Milliseconds;
            if (showHpTicks > 0)
                showHpTicks -= elapsed.Milliseconds;

            // ### Update entity State
            if (!Alive) {
                DidMove = false;
                if (State == EntityState.Dead)
                    return;
                else if (State != EntityState.Dying)
                    die();
                else if (!isOnFloor)
                    EBounds.moveY(2);
                else {
                    setState(EntityState.Dead);
                    dropItems();
                }

                return;
            }


            isOnFloor = map.isRectOnFloor(EBounds.StandRect);
            // ### Run the entities customizable AI
            if (ai != null)
                ai(this, map);
            else
                runAI(map);

            // ### Update movement State based on movement
            if (msVel.X != 0 && State != EntityState.Jumping) {
                setState(EntityState.Moving);
                if (msVel.X < 0) facing = Direction.Left;
                else facing = Direction.Right;
            } else if (State == EntityState.Moving) { // If State still 'Moving' but not moving, change State
                setState(EntityState.Standing);
            }

            // ### Update X position

            int currXSpeed = (int) (getRealXSpeed() * speedMultiplier);
            if (attackDelay > ATTACK_DELAY_MS * 0.625f && speedMultiplier > 0.25f) // If attacked recently while jumping, move slower
                speedMultiplier *= 0.93f;
            else if (speedMultiplier < MAX_SPEED)
                speedMultiplier += 0.033f;
            else
                speedMultiplier = MAX_SPEED; // Don't overshoot

            int oldX = EBounds.X;
            EBounds.moveX(currXSpeed);
            if (bounds.Right > map.getPixelWidth()) {
                EBounds.moveX((map.getPixelWidth() - bounds.Width) - bounds.X);
                // msVel.X = 0;
            } else if (bounds.Left <= 0) {
                EBounds.moveX(-bounds.X);
                // msVel.X = 0;
            } else if (msVel.X > 0) {
                int newX = map.checkBoundsXRight(bounds.Rect);
                updateBoundsX(map, newX);
            } else if (msVel.X < 0) {
                int newX = map.checkBoundsXLeft(bounds.Rect);
                updateBoundsX(map, newX);
            }
            if (oldX != EBounds.X) DidMove = true;
            else DidMove = false;

            // ### Update Y Position

            if (State == EntityState.Jumping) { // Gravity
                msVel.Y -= GRAVITY_PER_MS;
            } else if(jumpDelay > 0) {  // Tick jump delay
                jumpDelay -= elapsed.Milliseconds;
            }

            // Subtract so everything else doesn't have to be switched (0 is top)
            EBounds.moveY((int) -getRealYSpeed());
            if (bounds.Top >= map.getPixelHeight() - bounds.Height / 2) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                fallWithGravity();
            } else if (bounds.Bottom <= 0) {
                EBounds.moveY((int) getRealYSpeed()); // Undo the move
                hitGround();
            } else if (getRealYSpeed() > 0) {
                int newY = map.checkBoundsYUp(bounds.Rect);
                if (newY != bounds.Y) { // Hit something
                    EBounds.moveY(newY - bounds.Y); // Move down correct amount (+)
                    fallWithGravity();
                }
            } else if (getRealYSpeed() < 0) {
                int newY = map.checkBoundsYDown(bounds.Rect);
                if (newY != bounds.Y) { // Hit something
                    EBounds.moveY(newY - bounds.Y); // Move up correct amount (-)
                    hitGround();
                }
            }
        }

        private void fallWithGravity() {
            msVel.Y = 0;
            setState(EntityState.Jumping);
        }

        private void updateBoundsX(TileMap map, int newX) {
            if (!isOnFloor && State != EntityState.Jumping) {
                fallWithGravity();
            } else if (newX != bounds.X) {
                EBounds.moveX(newX - bounds.X);
                // msVel.X = 0;
            }
        }

        public void setXSpeedPerMs(float speedPerMs) {
            msVel.X = speedPerMs;

            if (msVel.X < 0) facing = Direction.Left;
            else if (msVel.X > 0) facing = Direction.Right;
        }

        protected void hitGround() {
            isOnFloor = true;
            msVel.Y = 0;
            setState(EntityState.Standing);
        }

        public Attack attack(TileMap map, EntityPart part, Func<Entity, EntityPart, TileMap, Attack> factoryFunc) {
            if (canAttack()) {
                Attack a = factoryFunc(this, part, map);
                map.addAttack(a);
                attackDelay = ATTACK_DELAY_MS;
                return a;
            }
            return null;
        }
        
        public bool canAttack() {
            return (attackDelay <= 0 && State != EntityState.Blocking && State != EntityState.Dead);
        }

        public void heal(int amnt) {
            if (Alive) {
                Stats.addHp(amnt);
            }
        }

        public int hitInThe(EntityPart part, int dmg, float reducer) {
            showHpTicks = HP_BAR_SHOW_MS;
            lastHitPart = part;

            int realDmg = 0;
            if (part == EntityPart.Legs)
                realDmg = (int)(dmg * Stats.TLegsMultiplier);
            else if (part == EntityPart.Head)
                realDmg = (int)(dmg * Stats.THeadMultiplier);
            else if (part == EntityPart.Body)
                realDmg = (int) (dmg * Stats.TBodyMultiplier);
            realDmg = (int) (realDmg * reducer);

            Stats.addHp(-realDmg);
            return realDmg;
        }

        public void jump() {
            if (jumpDelay <= 0 && isOnFloor) {
                msVel.Y = JUMP_PER_MS;
                setState(EntityState.Jumping);
                jumpDelay = JUMP_DELAY_MS; // Won't start decreasing until no longer jumping
                isOnFloor = false;
            }
        }

        public void duck() {
            if (isOnFloor) {
                setState(EntityState.Crouching);
                EBounds.duck(); // Resets position
                setXSpeedPerMs(0);
                Stats.headMultiplier -= 0.1f;
                Stats.legsMultiplier += 0.2f;
            }
        }

        public void block() {
            // Must wait a while after attacking to block again
            if (isOnFloor && attackDelay < ATTACK_DELAY_MS*0.8) {
                setState(EntityState.Blocking);
                EBounds.block(facing); // Resets position
                setXSpeedPerMs(0);
                Stats.bodyMultiplier += 0.5f;
                Stats.headMultiplier += 0.2f;
            }
        }

        protected void die() {
            EBounds.die();
            msVel.X = msVel.Y = 0;
            State = EntityState.Dying;
        }

        protected void setState(EntityState State) {
            if (Alive || State == EntityState.Dying || State == EntityState.Dead) {
                EBounds.resetPositions();  // Resets position
                Stats.resetReducers();
                this.State = State;
            }
        }

        protected void dropItems() {
            for (int i=0; i < pDrops.Length; i++) {
                if (pDrops[i].Chance > ScreenManager.Rand.NextDouble())
                    GScreen.TileMap.dropItem(pDrops[i].Item, this);
            }
        }

        public EntityState getDrawState() {
            if (attackDelay > ATTACK_DELAY_MS*0.2) {
                if (State == EntityState.Standing || State == EntityState.Moving || State == EntityState.Jumping) 
                    return EntityState.Attacking;
                else if (State == EntityState.Crouching)
                    return EntityState.AttackCrouch;
            }
            
            return State; 
        }

        protected Texture2D getSprite(int elapsed)
        {
            EntityState State = getDrawState();
            if (!Alive) { // State == EntityState.Dead || State == EntityState.Dying
                return sprite.getSpritePart(SpriteParts.Part.Dead);
            } else if (State == EntityState.Crouching) {
                return sprite.getSpritePart(SpriteParts.Part.Crouch);
            } else if (State == EntityState.Blocking) {
                return sprite.getSpritePart(SpriteParts.Part.Block);
            } else if (State == EntityState.Moving) {
                sprite.tick(elapsed);

                if (sprite.FrameIdx == 0)
                    return sprite.getSpritePart(SpriteParts.Part.Move);
                else
                    return sprite.Base;
            } else if (State == EntityState.Attacking) {
                return sprite.getSpritePart(SpriteParts.Part.Attack);
            } else if (State == EntityState.AttackCrouch) {
                return sprite.getSpritePart(SpriteParts.Part.CrouchAttack);
            } else if (State == EntityState.Jumping) {
                return sprite.getSpritePart(SpriteParts.Part.Move);
            } else {
                return sprite.Base;
            }
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 offset, TimeSpan elapsed) {
            if (SlatedToRemove) return;
            
            Rectangle pRect = Rect;
            pRect.X -= (int) offset.X;
            pRect.Y -= (int) offset.Y;

            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward())
                spriteBatch.Draw(sprite, pRect, Color.White);
            else
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

            // Was hit resently, show hp bar
            if (Alive && showHpTicks != 0) {
                Rectangle hpRect = Rect;
                hpRect.X -= (int) (offset.X + hpRect.Width * 0.125); // Offset and a bit over from the absolute left side
                hpRect.Y -= 7 + (int) offset.Y;
                hpRect.Height = 3;
                hpRect.Width = (int) Math.Round(hpRect.Width * 0.75 * Stats.HpPercent) + 1;
                spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Red);

                Vector2 vect = ScreenManager.Small_Font.MeasureString(Name);
                spriteBatch.DrawString(ScreenManager.Small_Font, Name, new Vector2(pRect.Center.X - vect.X/2, hpRect.Y - 19), Color.White);

                Rectangle lastRect = EBounds.getRectFromPart(lastHitPart);
                if (showHpTicks > HP_BAR_SHOW_MS / 2 && lastRect.Width != 0) {     
                    lastRect.X += (int) (lastRect.Height * 0.1 - offset.X); 
                    lastRect.Width = (int) (lastRect.Width * 0.8 - offset.Y);
                    spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                }
            }
        }
        
        public EntityBounds EBounds { 
            get {
                if (!(bounds is EntityBounds))
                    throw new ArgumentException("Expected EntityBounds but got " + bounds.GetType().ToString());
                else 
                    return (EntityBounds) bounds;
            } 
        }
        
        protected float getRealXSpeed() { return msVel.X * (tElapsed.Milliseconds); }
        protected float getRealYSpeed() { return msVel.Y * (tElapsed.Milliseconds); }
        public bool Alive { get { return Stats.Hp > 0; } }

        public float getSpeedX() { return msVel.X; }
        public float getSpeedY() { return msVel.Y; }
        public bool isFacingForward() { return facing != Direction.Left; }

        public object this[string s] {
            get { return (properties.ContainsKey(s)) ? properties[s] : null; }
            set { properties[s] = value; }
        }
    }
}
