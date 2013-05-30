using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG.Screen
{
    class InputScreen : PopUpScreen
    {
        readonly int CURSOR_TIME = 10;
        readonly int MAX_LENGTH = 20;
        int cursorTime;

        string input;
        KeyboardState oldKb;
        List<Keys> oldPressedKeys;
        Func<string, string> function;

        public new bool DoUpdate {
            get { return base.DoUpdate; }
            set { base.DoUpdate = value; input = ""; }
        }

        public InputScreen(ScreenManager sm)
            : base(sm, "Input", "", new string[] { "Enter", "Back" }, new Action[] { MenuItemFunctions.ReturnInput, MenuItemFunctions.MainMenu })
        {
            input = "";
            oldPressedKeys = new List<Keys>();
            oldKb = Keyboard.GetState();
            // cursorTime > CURSOR_TIME show, else don't show
            cursorTime = CURSOR_TIME;
        }

        public void setTitle(string title) {
            this.Title = title;
        }

        public void setAction(Func<string, string> func) {
            function = func;
        }

        public override void Update(GameTime gTime) {
            KeyboardState kb = Keyboard.GetState();
            if (input.Length > MAX_LENGTH)
                input = input.Substring(0, MAX_LENGTH);

            foreach (Keys k in oldPressedKeys) {
                if (kb.IsKeyUp(k)) {
                    String s = k.ToString().ToLower();
                    if (s.Length == 1) {
                        // Capitalize
                        if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift) || 
                                oldKb.IsKeyDown(Keys.LeftShift) || oldKb.IsKeyDown(Keys.RightShift)) {
                            s = s.ToUpper();
                        }
                        input += s;
                    } else if (k == Keys.Space) {
                        input += " ";
                    } else if (k == Keys.Back && input.Length > 0) {
                        input = input.Substring(0, input.Length - 1);
                        cursorTime = CURSOR_TIME;
                    }
                        
                    break;
                }
            }

            cursorTime = (cursorTime + 1) % (CURSOR_TIME * 2);

            oldPressedKeys.Clear();
            oldPressedKeys.AddRange(kb.GetPressedKeys());

            oldKb = kb;

            base.Update(gTime);
        }

        // Calls the action function with the current input
        // Return: the result of the action function
        public string returnInput() {
            return function(input);
        }

        public override void Draw(GameTime gTime) {
            SpriteBatch.End(); // End normal, drawing different

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive); // Draw additive

            SpriteBatch.Draw(ScreenManager.WhiteRect, new Rectangle(0, 0,
                getScreenManager().GraphicsDevice.Viewport.Width, getScreenManager().GraphicsDevice.Viewport.Height), ScreenManager.AdditiveColor);

            Vector2 pos = new Vector2(getScreenManager().GraphicsDevice.Viewport.Width * 0.36f, getScreenManager().GraphicsDevice.Viewport.Height * 0.7f);
            Text = input;
            if (cursorTime > CURSOR_TIME)
                Text += "|";
            else
                Text += " ";

            base.Draw(gTime); // Draw menu items
            SpriteBatch.End(); // End additive
            SpriteBatch.Begin(); // Restart normal
        }
    }
}
