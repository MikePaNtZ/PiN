using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    public class KeyboardInput
    {
        public KeyboardInput(KeyboardState keyboardState, KeyboardState prevKeyboardState)
        {
            _keyboardState = keyboardState;
            _prevKeyboardState = prevKeyboardState;
        }

        public KeyboardState KeyboardState
        {
            get { return _keyboardState; }
            set { }
        }

        public KeyboardState PreviousKeyboardState
        {
            get { return _prevKeyboardState; }
            set { }
        }

        private KeyboardState _keyboardState;
        private KeyboardState _prevKeyboardState;
    }
}
