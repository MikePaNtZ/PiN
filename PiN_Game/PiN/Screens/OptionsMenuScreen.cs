#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace PiN
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry toggleFullscreenMenuEntry;

        public static bool IsFullscreen = false;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {

            // Create our menu entries.
            toggleFullscreenMenuEntry = new MenuEntry(string.Empty);
            

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");

            // Hook up menu event handlers.
            toggleFullscreenMenuEntry.Selected += ToggleFullScreenMenuEntrySelected;
            
            back.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(toggleFullscreenMenuEntry);
            MenuEntries.Add(back);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            if (IsFullscreen)
                toggleFullscreenMenuEntry.Text = "Make Game Windowed";
            else
                toggleFullscreenMenuEntry.Text = "Make Game Fullscreen";
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        void ToggleFullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //if (IsFullscreen)
            //{
            //    Game.graphics.IsFullScreen = false;
            //    graphics.PreferredBackBufferWidth = 800;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //    graphics.PreferredBackBufferHeight = 600;//GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //    graphics.ApplyChanges();
            //    IsFullscreen = false;
            //}
            //else
            //{
            //    graphics.IsFullScreen = true;
            //    graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width;
            //    graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height;
            //    graphics.ApplyChanges();
            //    IsFullscreen = true;
            //}

            SetMenuEntryText();
        }


        #endregion
    }
}
