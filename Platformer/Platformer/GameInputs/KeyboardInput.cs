using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    public class KeyboardInput
    {
        public KeyboardInput(KeyboardState keyboardState)
        {
            _keyboardState = keyboardState;
        }

        public KeyboardState KeyboardState
        {
            get { return _keyboardState; }
            set { }
        }

        private KeyboardState _keyboardState;
    }
}
