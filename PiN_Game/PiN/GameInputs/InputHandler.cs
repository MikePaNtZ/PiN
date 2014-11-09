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
    public class InputHandler
    {
        public InputHandler(Camera cam, MouseState mouseState, MouseState prevMouseState, KeyboardState keyboardState, KeyboardState prevKeyboardState)
        {
            _cam = cam;
            _mouseInput = new MouseInput(cam, mouseState, prevMouseState);
            _keyboardInput = new KeyboardInput(keyboardState, prevKeyboardState);
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

        public MouseState PreviousMouseState
        {
            get { return _mouseInput.PreviousMouseState; }
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

        public KeyboardState PreviousKeyboardState
        {
            get { return _keyboardInput.PreviousKeyboardState; }
            set { }
        }

        private readonly Camera _cam;
        private MouseInput _mouseInput;
        private KeyboardInput _keyboardInput;
    }
}
