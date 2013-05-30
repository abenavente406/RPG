using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

using RPG.Entities;
using RPG.Helpers;

namespace RPG.Screen {
    public class MenuItemFunctions
    {
        public static void Exit() { Environment.Exit(1); }

        public static void Play()
        {
            Screen mainmenu = ScreenManager.getScreen(ScreenId.MainMenu);
            mainmenu.DoDraw = mainmenu.DoUpdate = false;

            GameScreen game = (GameScreen) ScreenManager.getScreen(ScreenId.Game);

            InputScreen input = (InputScreen) ScreenManager.getScreen(ScreenId.Input);
            // This function will be called to initialize the game screens and create the player with the inputed name
            input.setTitle("Input Name:");
            input.setAction(game.init);
            input.DoDraw = input.DoUpdate = true;
        }

        public static void BackToGame()
        {
            Screen game = ScreenManager.getScreen(ScreenId.Game);
            game.DoDraw = game.DoUpdate = true;

            foreach (Screen s in ScreenManager.screenIterator())
            {
                if (s is PopUpScreen)
                {
                    s.DoDraw = false;
                    s.DoUpdate = true;
                }
            }

            PauseScreen pause = (PauseScreen) ScreenManager.getScreen(ScreenId.Pause);
            pause.DoDraw = false;
            pause.DoUpdate = true;
            pause.escPressed = false;
            if (pause.isPaused())
                pause.togglePause();

            InventoryScreen stats = (InventoryScreen) ScreenManager.getScreen(ScreenId.Inventory);
            stats.DoUpdate = true;
            stats.buttonPressed = false;
        }

        public static void Save() { Save(true);  }
        public static void Save(bool print) {
            if (!Directory.Exists(@".\saves")) 
                Directory.CreateDirectory(@".\saves");

            try {
                GameScreen game = (GameScreen) ScreenManager.getScreen(ScreenId.Game);
                if (game.Player != null) {
                    Stream stream = File.Open(@".\saves\save1.save", FileMode.Create);

                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, game.Player);

                    stream.Close();
                    if (print) MessageBox.Show("Saved successfully");
                } else if (print) MessageBox.Show("No player to save");
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static void Load() {
            Screen mainmenu = ScreenManager.getScreen(ScreenId.MainMenu);
            mainmenu.DoUpdate = false;

            if (!Directory.Exists(@".\saves")) 
                Directory.CreateDirectory(@".\saves");

            try {
                Stream stream = File.Open(@".\saves\save1.save", FileMode.Open);
                if (stream != null) {
                    GameScreen game = (GameScreen) ScreenManager.getScreen(ScreenId.Game);
                    BinaryFormatter formatter = new BinaryFormatter();
                    game.Player = (Player) formatter.Deserialize(stream);
                    game.newMainRoom();

                    stream.Close();
                    
                    mainmenu.DoDraw = false;
                    BackToGame();
                } else {
                    throw new FileNotFoundException();
                }
            } catch (FileNotFoundException) {
                MessageBox.Show("No save found");
            } catch (SerializationException) {
                MessageBox.Show("Failed to load, save file corrupted.");
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }

            // Do draw is set to false if load was successful, otherwise it will still be true
            mainmenu.DoUpdate = mainmenu.DoDraw;
        }

        public static void ReturnInput()
        {
            InputScreen input = (InputScreen)ScreenManager.getScreen(ScreenId.Input);
            input.returnInput();

            BackToGame();
        }

        public static void MainMenuHelp()
        {
            // Close all
            foreach (Screen s in ScreenManager.screenIterator()) {
                s.DoDraw = s.DoUpdate = false;
            }

            Screen help = ScreenManager.getScreen(ScreenId.MainMenuHelp);
            help.DoDraw = help.DoUpdate = true;
        }

        public static void MainMenu() {
            // Close all
            foreach (Screen s in ScreenManager.screenIterator()) s.DoDraw = s.DoUpdate = false;

            MenuItemFunctions.Save(false);

            Screen mainMenu = ScreenManager.getScreen(ScreenId.MainMenu);
            mainMenu.DoUpdate = mainMenu.DoDraw = true;
        }
    }
}