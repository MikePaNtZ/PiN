using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    public class Camera
    {
        public Camera(Viewport viewport)
        {
            _viewport = viewport;
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
            Zoom = 1.0f;
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
        

        
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                // If there's a limit set and the camera is not transformed clamp position to limits
                if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    _position.X = MathHelper.Clamp(_position.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - _viewport.Width);
                    _position.Y = MathHelper.Clamp(_position.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - _viewport.Height);
                }
            }
        }
        

        public Vector2 Origin { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }

        public Viewport ViewPort { 
            get
            {
                return _viewport;
            }
            set
            {
            }
        }
        

        public Matrix GetViewMatrix(Vector2 parallax)
        {
            // To add parallax, simply multiply it by the position
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                // include an additional (negative) Origin translation matrix before the Rotation and Zoom matrices
                //makes your origin calculations only affect zooming and rotating, but leave translation untouched
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }


        // Get a camera relative position from a world position
        public Vector2 GetScreenPosition(Vector2 worldPosition)
        {
            return worldPosition - Position;
        }

        // Get a world position from a camera relative position
        public Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            return screenPosition + Position;
        }

        public void LookAt(Vector2 position)
        {
            Position = position - new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);
        }

        public void Move(Vector2 displacement, bool respectRotation = false)
        {
            if (respectRotation)
            {
                displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(-Rotation));
            }

            Position += displacement;
        }

        private readonly Viewport _viewport;
        private Vector2 _position;
        private Rectangle? _limits;
    }
}
