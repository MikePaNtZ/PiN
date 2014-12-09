#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
#endregion

namespace PiN
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization

        ContentManager content;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Play Game");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry instructionMenuEntry = new MenuEntry("Instructions");
            MenuEntry storyMenuEntry = new MenuEntry("Story");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            instructionMenuEntry.Selected += InstructionsMenuEntrySelected;
            storyMenuEntry.Selected += storyMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(instructionMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(storyMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        public override void LoadContent()
        {
            content = new ContentManager(ScreenManager.Game.Services, "Content");
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(content.Load<Song>("Sounds/MainMenuMusic"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //base.LoadContent();
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Program menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen());
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the menu.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit the game?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }

        protected void InstructionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "How to Play the Game\n\n" +
                                   "Pause...........Escape\n" +
                                   "Run L..........A\n" +
                                   "Run R..........D\n" +
                                   "Jump............Spacebar\n" +
                                   "Shoot............L-Mouse Btn\n" +
                                   "Shield...........R-Mouse Btn\n" +
                                   "Hero 1..........1\n" +
                                   "Hero 2..........2\n" +
                                   "Hero 3..........3\n\n" +
                                   "Green powerups revive dead allies, blue powerups give hero health,\n" +
                                   "and red powerups grant invincibility for a short time.\n\n" +
                                   "Make it to the portal at the end of the level before time is up.\n" +
                                   "Game over if all heroes are dead.\n" +
                                   "Good luck soldier!";

            MessageBoxScreen viewedInstructionsMsgBox = new MessageBoxScreen(message, false);

            ScreenManager.AddScreen(viewedInstructionsMsgBox, 0);
        }

        protected void storyMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "The year is 4362. Three elite members of the\n" +
                                   "Colonial Superheroes Coalition for Independence (CSCI)\n" +
                                   "embark on a mission to rescue their squad commander Tyler,\n" +
                                   "code named Portal. The Blatant Supervillains of Destruction (BSOD)\n" +
                                   "threaten to destroy one of the 3 colonized planets\n" +
                                   "(Mars, Neptune, or Venus) with a Planet Destructo Ray.\n" +
                                   "Commander Portal was sent on a reconnaissance mission to one\n" +
                                   "of these planets to discover enemy plans, but the enemy saw\n" +
                                   "him coming, and Tyler teleported into a trap!\n\n" +
                                   "The mission was only supposed to last one week. It has been three.\n" +
                                   "It's time to rescue the squad commander, or die trying.\n" +
                                   "Retrace his footsteps, find portals he has left behind,\n" +
                                   "and save the day!";

            MessageBoxScreen viewedStoryMsgBox = new MessageBoxScreen(message, false);

            ScreenManager.AddScreen(viewedStoryMsgBox, 0);
        }

        void ViewedInstructionsMsgBox(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MainMenuScreen(), e.PlayerIndex);
        }

        void ViewedStoryMsgBox(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MainMenuScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        #endregion
    }
}
