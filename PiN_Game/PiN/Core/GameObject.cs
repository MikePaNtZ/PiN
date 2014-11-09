
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
 
namespace PiN
{
    class GameObject
    {
        public GameObject()
        {
            rotation = 0.0f;
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            isAlive = false;
        }

        public Vector2 Position
        {
            get { return position; }
            set { this.position = value; }
        }
        public Vector2 Velocity
        {
            get { return velocity; }
            set { this.velocity = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { this.rotation = value; }
        }

        public Vector2 Center
        {
            get { return center; }
            set { this.center = value; }
        }

        public bool IsAlive
        {
            get { return isAlive; }
            set { this.isAlive = value; }
        }

        public Rectangle rectangle
        {
            get
            {
                int left = (int)position.X;
                int width = texture.Width;
                int top = (int)position.Y;
                int height = texture.Height;
                return new Rectangle(left, top, width, height);
            }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { this.texture = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// Resets the base components of the game object
        public virtual void Reset(Vector2 pos)
        {
            position = pos;
        }

        /// <summary>
        /// Updates the base class game object instance. This is intended to be overriden by derived game object types.
        /// </summary>
        protected virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

        }

        // Sprite associated with the game object. This is what's drawn to the screen in the OnDraw method.
        protected Texture2D texture;
        // tint color drawn on top of game object.
        protected Color color;
        // Center of the game object's texture
        protected Vector2 center;
        // Physics state variables
        protected Vector2 position;
        protected Vector2 velocity;
        protected float rotation;
        // Flag indicating the game object is "alive".
        protected bool isAlive;

    }
}