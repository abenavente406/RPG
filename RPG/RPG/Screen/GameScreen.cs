using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RPG.Sprites;
using RPG.GameObjects;
using RPG.Helpers;
using RPG.Entities;
using RPG.Items;

using Microsoft.VisualBasic;

namespace RPG.Screen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameScreen : Screen
    {
        public static int TRANSITION_MS = 500;

        // -------------------
        // Game Variables
        // -------------------
        public Player Player;
        public TileMap TileMap { get; private set; }
        Vector2 offset;

        int transitionMs;

        KeyboardState oldKb;
        // -------------------
        // Game Textures
        // -------------------
        public static Dictionary<BackgroundId, Texture2D> Backgrounds;
        public static Dictionary<GUISpriteId, Texture2D> sprGUI;
        public static Dictionary<EntitySpriteId, Sprite> sprEntities;
        public static Dictionary<AttackSpriteId, Texture2D> sprAttacks;        
        public static Dictionary<TerrainSpriteId, Texture2D> sprTerrains;
        public static Dictionary<Animation, Sprite> Animations;

        public static Dictionary<ItemId, Armour> Armours;
        public static Dictionary<ItemId, Item> Items;

        public GameScreen(ScreenManager screenManager) : base(screenManager) {
            offset = new Vector2(0, 0);
            oldKb = Keyboard.GetState();
        }

        public override void LoadContent() {
            // Load Entities
            sprEntities = new Dictionary<EntitySpriteId, Sprite>();
            sprEntities.Add(EntitySpriteId.Warrior, new Sprite(Content, "Warrior").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.Warlock, new Sprite(Content, "Warlock").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.Wraith, new Sprite(Content, "Wraith").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.Skeleton_King, new Sprite(Content, "Skeleton_King").loadSpriteParts(SpriteParts.Entity));

            // Load Attacks
            sprAttacks = new Dictionary<AttackSpriteId, Texture2D>();
            sprAttacks.Add(AttackSpriteId.Fireball, Content.Load<Texture2D>("Fireball/Fireball"));
            sprAttacks.Add(AttackSpriteId.Iceball, Content.Load<Texture2D>("Iceball/Iceball"));
            sprAttacks.Add(AttackSpriteId.Scurge_Shot, Content.Load<Texture2D>("Scurge_Shot/Scurge_Shot"));
            sprAttacks.Add(AttackSpriteId.Raise_Death, Content.Load<Texture2D>("Raise_Death/Raise_Death"));

            // Load Terrain
            Texture2D TerrainSpriteSheet = Content.Load<Texture2D>("Terrain/Terrain");
            Texture2D[] TerrainTexs = new Texture2D[TerrainSpriteSheet.Width / TileMap.SPRITE_SIZE];
            for (int i=0; i<TerrainTexs.Length; i++) TerrainTexs[i] = Helper.ParseSpriteSheet(TerrainSpriteSheet, i, TileMap.SPRITE_SIZE);
            sprTerrains = new Dictionary<TerrainSpriteId, Texture2D>();
            sprTerrains.Add(TerrainSpriteId.None, null);
            sprTerrains.Add(TerrainSpriteId.Stone_Wall, TerrainTexs[0]);
            sprTerrains.Add(TerrainSpriteId.Stone2_Wall, TerrainTexs[1]);
            sprTerrains.Add(TerrainSpriteId.Door, TerrainTexs[2]);
            sprTerrains.Add(TerrainSpriteId.EmptyMagicWall, TerrainTexs[7]);
            sprTerrains.Add(TerrainSpriteId.ClosedChest, TerrainTexs[8]);
            sprTerrains.Add(TerrainSpriteId.OpenChest, TerrainTexs[9]);
            sprTerrains.Add(TerrainSpriteId.IronDoor, TerrainTexs[10]);

            Animations = new Dictionary<Animation,Sprite>();
            Animations.Add(Animation.RedSpiral, new Sprite(new Texture2D[] {TerrainTexs[3],TerrainTexs[4],TerrainTexs[5],TerrainTexs[6]}, 4, 100));         

            // Load GUI
            sprGUI = new Dictionary<GUISpriteId, Texture2D>();
            sprGUI.Add(GUISpriteId.Blocking, Content.Load<Texture2D>("GUI/Blocking"));
            sprGUI.Add(GUISpriteId.Ducking, Content.Load<Texture2D>("GUI/Crouching"));
            sprGUI.Add(GUISpriteId.Standing, Content.Load<Texture2D>("GUI/Standing"));

            // Load Backgrounds
            Backgrounds = new Dictionary<BackgroundId,Texture2D>();
            Backgrounds.Add(BackgroundId.Cave1, Content.Load<Texture2D>("Terrain/cave1_background"));

            // Load Items
            Texture2D ItemSpriteSheet = Content.Load<Texture2D>("Items/Items");
            Texture2D[] ItemTexs = new Texture2D[ItemSpriteSheet.Width / Item.SIZE];
            for (int i=0; i<ItemTexs.Length; i++) ItemTexs[i] = Helper.ParseSpriteSheet(ItemSpriteSheet, i, Item.SIZE);
            Items = new Dictionary<ItemId, Item>();
            Items.Add(ItemId.None, null);
            Items.Add(ItemId.Key, new Item(ItemId.Key, "Door Key", ItemTexs[0], Item.NewRoom));
            Items.Add(ItemId.SmallPotion, new Item(ItemId.SmallPotion, "Small Potion", ItemTexs[1], Item.UseSmallPotion));
            Items.Add(ItemId.Gold, new Item(ItemId.Gold, "Gold", ItemTexs[2], Item.NoAction, true));

            // Load Armour
            Texture2D ArmourSpriteSheet = Content.Load<Texture2D>("Armour/Armour");
            Texture2D[,] ArmourTexs = new Texture2D[ArmourSpriteSheet.Height / TileMap.SPRITE_SIZE, ArmourSpriteSheet.Width / TileMap.SPRITE_SIZE];
            for (int h = 0; h < ArmourTexs.GetLength(0); h++)
                for (int w = 0; w < ArmourTexs.GetLength(1); w++)
                    ArmourTexs[h, w] = Helper.ParseSpriteSheet(ArmourSpriteSheet, w, h, TileMap.SPRITE_SIZE);
            Armours = new Dictionary<ItemId, Armour>();
            Armours.Add(ItemId.BronzeHead, new Armour(ArmourTexs[0, 0], ArmourTexs[0, 3], "Bronze Helm", ItemId.BronzeHead, ArmourParts.Head, 0.075f, true));
            Armours.Add(ItemId.BronzeBody, new Armour(ArmourTexs[0, 1], ArmourTexs[0, 4], "Bronze Body", ItemId.BronzeBody, ArmourParts.Body, 0.075f, true));
            Armours.Add(ItemId.BronzeLegs, new Armour(ArmourTexs[0, 2], ArmourTexs[0, 5], "Bronze Legs", ItemId.BronzeLegs, ArmourParts.Legs, 0.075f, true));
            Armours.Add(ItemId.IronHead, new Armour(ArmourTexs[1, 0], ArmourTexs[1, 3], "Iron Helm", ItemId.IronHead, ArmourParts.Head, 0.150f, true));
            Armours.Add(ItemId.IronBody, new Armour(ArmourTexs[1, 1], ArmourTexs[1, 4], "Iron Body", ItemId.IronBody, ArmourParts.Body, 0.150f, true));
            Armours.Add(ItemId.IronLegs, new Armour(ArmourTexs[1, 2], ArmourTexs[1, 5], "Iron Legs", ItemId.IronLegs, ArmourParts.Legs, 0.150f, true));
        }

        public override void UnloadContent() { }

        public override void Update(GameTime gTime) {
            // Don't run until player is created
            if (Player == null) {
                return;
            }

            // Don't do anything while transitioning
            if (transitionMs > 0) {
                transitionMs -= gTime.ElapsedGameTime.Milliseconds;
                return;
            }

            // ### Movement input
            if (ScreenManager.kbState.IsKeyDown(Keys.D) && !ScreenManager.kbState.IsKeyDown(Keys.A))
                Player.doMove(Direction.Right);
            else if (!ScreenManager.kbState.IsKeyDown(Keys.D) && ScreenManager.kbState.IsKeyDown(Keys.A))
                Player.doMove(Direction.Left);
            else
                Player.doMove(Direction.Stopped);

            // ### Jump/Duck input
            if (ScreenManager.kbState.IsKeyDown(Keys.Space))
                Player.doJump();
            else if (!ScreenManager.kbState.IsKeyDown(Keys.Space) && ScreenManager.kbState.IsKeyDown(Keys.S) && !ScreenManager.kbState.IsKeyDown(Keys.W))
                Player.doDuck();
            else if (!ScreenManager.kbState.IsKeyDown(Keys.Space) && !ScreenManager.kbState.IsKeyDown(Keys.S) && ScreenManager.kbState.IsKeyDown(Keys.W))
                Player.doBlock();
            else if (Player.State == EntityState.Crouching || Player.State == EntityState.Blocking)
                Player.stand();

            // ### Attack input
            if (ScreenManager.kbState.IsKeyDown(Keys.D1) || ScreenManager.kbState.IsKeyDown(Keys.Left))
                Player.doAttack(TileMap, EntityPart.Head);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D2) || ScreenManager.kbState.IsKeyDown(Keys.Up) || ScreenManager.kbState.IsKeyDown(Keys.Down))
                Player.doAttack(TileMap, EntityPart.Body);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D3) || ScreenManager.kbState.IsKeyDown(Keys.Right))
                Player.doAttack(TileMap, EntityPart.Legs);

            // Interact with tile block
            if (ScreenManager.kbState.IsKeyDown(Keys.Enter) && ScreenManager.oldKBState.IsKeyUp(Keys.Enter)) {
                Rectangle rect = Player.Rect;
                TileMap.getPixel(rect.Center.X, rect.Center.Y).interact(this);
            }

            TileMap.update(gTime.ElapsedGameTime);           

            // ### Offset
            offset.X = (int) Player.Location.X - ((int) getScreenManager().getSize().X / 2);
            offset.Y = (int) Player.Location.Y - ((int) getScreenManager().getSize().Y / 2);

            if (offset.X < 0) 
                offset.X = 0;
            else if (offset.X + getScreenManager().getSize().X > TileMap.getPixelWidth()) 
                offset.X = (int) (TileMap.getPixelWidth() - getScreenManager().getSize().X);

            if (offset.Y < 0)
                offset.Y = 0;
            else if (offset.Y + getScreenManager().getSize().Y > TileMap.getPixelHeight())
                offset.Y = (int) (TileMap.getPixelHeight() - getScreenManager().getSize().Y);

            oldKb = ScreenManager.kbState;
        }

        // Initialize the various game screens and create the player with the inputed name
        public string init(string name) {
            Player = new Player(this, 0, 0, name, sprEntities[EntitySpriteId.Warrior]);
            newMainRoom();
            Player.moveTo(new Vector2(0, (TileMap.Height - 2)  * TileMap.SPRITE_SIZE));
            return name;
        }

        public void setRoom(TileMap map) {
            if (TileMap != null) TileMap.LeavePoint = Player.Location;
            TileMap = map;
            for(int i=0; i<10; i++)
                TileMap.update(ScreenManager.TargElapsedTime);
            TileMap.addEntity(Player);
            transitionMs = TRANSITION_MS;
        }

        public void newMainRoom() {
            setRoom(new TileMap(40, 6, MapType.Hall, this, TileMap));
            Player.newRoom();
        }
        
        public override void Draw(GameTime time) {
            // While transitioning all black
            if (transitionMs > 0) {
                SpriteBatch.GraphicsDevice.Clear(Color.Black);
                return;
            }

            /*
            // WRAPPING BACKGROUND
            Vector2 background_resize = getScreenManager().getSize();
            int background_offsetX = (int) offset.X % ((int) background_resize.X * 2);

            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);

            // # Draw a second one in front of first to wrap around
            background_offsetX -= (int) background_resize.X;
            SpriteBatch.Draw(sprBackground, new Rectangle(-background_offsetX, 0, (int) background_resize.X, (int) background_resize.Y), Color.White);    
            */

            // Draw tile map
            TileMap.draw(SpriteBatch, offset, time.ElapsedGameTime);
        }

        public Dictionary<EntitySpriteId, Sprite> SprEntity { get { return sprEntities; } }
        public Dictionary<AttackSpriteId, Texture2D> SprAttack { get { return sprAttacks; } }
        public Dictionary<TerrainSpriteId, Texture2D> SprTerrains { get { return sprTerrains; } }
    }
}
