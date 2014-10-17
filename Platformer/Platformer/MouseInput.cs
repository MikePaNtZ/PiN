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
        public MouseInput(Viewport viewport, MouseState mouseState)
        {
            _viewport = viewport;
            _mouseState = mouseState;
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
        }
        public Rectangle? Limits
        {
            get { return _limits; }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    _limits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = System.Math.Max(_viewport.Width, value.Value.Width),
                        Height = System.Math.Max(_viewport.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    Position = Position;
                }
                else
                {
                    _limits = null;
                }
            }
        }


        public MouseState MouseState
        {
            get { return _mouseState; }
            set { }
        }

        public Vector2 Position
        {
            get {
                Vector2 returnVect = new Vector2();
                returnVect.X = _mouseState.X;
                returnVect.Y = _mouseState.Y;
                return returnVect;
            }
            set {}
        }


        

        public Vector2 Origin { get; set; }

        private readonly Viewport _viewport;
        private MouseState _mouseState;
        private Vector2 _position;
        private Rectangle? _limits;
    }
}
