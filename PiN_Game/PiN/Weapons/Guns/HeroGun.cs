#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Program Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroGun : Gun
    {

        protected GameObject crosshair;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroGun(GameCharacter theShooter) : base(theShooter)
        {
        }

        protected override void LoadContent()
        {
            crosshair = new GameObject();
            crosshair.Texture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Crosshair");
            base.LoadContent();
        }

        public override void UpdateWeaponState(Vector2 crosshairPosition)
        {
            crosshair.Position = crosshairPosition;
            base.UpdateWeaponState(crosshairPosition);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                crosshair.Texture,
                crosshair.Position,
                null,
                Color.White,
                crosshair.Rotation,
                crosshair.Center,
                1.0f,
                weaponWielder.Flip,
                0);

            base.Draw(gameTime, spriteBatch);
        }
        protected override void checkBulletCollision(GameObject bullet, Rectangle bulletRect)
        {
            //Check for collisions with the enemies
            foreach (Enemy enemy in weaponWielder.Level.enemies)
            {
                if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                {
                    //We're going to want to put some enemy health reduction code here
                    //Enemy class needs a health member variable too
                    enemy.OnHit(weaponWielder);
                    bullet.IsAlive = false;
                }
            }
        }



    }
}

