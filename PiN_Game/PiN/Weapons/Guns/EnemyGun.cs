#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
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
    class EnemyGun : Gun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyGun(GameCharacter theShooter) : base(theShooter)
        {
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(
                theWeapon.Texture,
                theWeapon.Position,
                null,
                Color.White,
                theWeapon.Rotation,
                theWeapon.Center,
                1.0f,
                weaponWielder.Flip,
                0);

            //Draw the bullets
            foreach (GameObject bullet in bullets)
            {
                if (bullet.IsAlive)
                {
                    spriteBatch.Draw(bullet.Texture,
                        bullet.Position, Color.White);
                }
            }
        }

        protected override void checkBulletCollision(GameObject bullet, Rectangle bulletRect)
        {
            Hero theTarget = weaponWielder.Level.ActiveHero;
            if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsPoweredUp)
            {
                //bullet is deflected
                //bullet.Velocity = -0.7f*bullet.Velocity;
                bullet.IsAlive = false;
            }

            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsBlocking)
            {
                //hero doesn't take damage when blocking with shield
                bullet.Velocity = -0.7f*bullet.Velocity;
            }
            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive)
            {
                theTarget.OnHit(weaponWielder);

                bullet.IsAlive = false;
            }


        }

    }
}

