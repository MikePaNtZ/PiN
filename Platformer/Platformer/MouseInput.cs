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
    public class MouseInput
    {
        public MouseInput(Camera cam, MouseState mouseState)
        {
            _cam = cam;
            _mouseState = mouseState;
        }


        public MouseState MouseState
        {
            get { return _mouseState; }
            set { }
        }

        public Vector2 ScreenPosition
        {
            get {
                _screenPosition.X = MathHelper.Clamp((float)_mouseState.X, 0.0f, (float) _cam.ViewPort.Width);
                _screenPosition.Y = MathHelper.Clamp((float)_mouseState.Y, 0.0f, (float) _cam.ViewPort.Height);
                return _screenPosition;
            }
            set {}
        }
        public Vector2 Position
        {
            get {
                Vector2 returnVect = new Vector2();
                returnVect.X = ScreenPosition.X + _cam.Position.X;
                returnVect.Y = ScreenPosition.Y + _cam.Position.Y;
                return returnVect;
            }
            set {}
        }

        private readonly Camera _cam;
        private MouseState _mouseState;
        private Vector2 _screenPosition;
    }
}
