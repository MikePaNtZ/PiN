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
    public class InputHandler
    {
        public InputHandler(Camera cam, MouseState mouseState, KeyboardState keyboardState)
        {
            _cam = cam;
            _mouseInput = new MouseInput(cam, mouseState);
            _keyboardInput = new KeyboardInput(keyboardState);
        }


        public MouseInput MouseInput
        {
            get { return _mouseInput; }
            set { }
        }
        public MouseState MouseState
        {
            get { return _mouseInput.MouseState; }
            set { }
        }

        public KeyboardInput KeyboardInput
        {
            get { return _keyboardInput; }
            set { }
        }

        public KeyboardState KeyboardState
        {
            get { return _keyboardInput.KeyboardState; }
            set { }
        }

        private readonly Camera _cam;
        private MouseInput _mouseInput;
        private KeyboardInput _keyboardInput;
    }
}
