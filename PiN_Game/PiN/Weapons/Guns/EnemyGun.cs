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
        public EnemyGun(Texture2D loadedTexture, GameCharacter theShooter) : base(loadedTexture, theShooter)
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

        protected override void LoadContent()
        {
            // set the default weapon to a gun.
            theWeapon = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));
            crosshair = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Crosshair"));
            // load all bullets
            bullets = new GameObject[MAX_BULLETS];

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new GameObject(weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
            }
        }


        protected override void checkBulletCollision(GameObject bullet, Rectangle bulletRect)
        {
            Hero theTarget = weaponWielder.Level.ActiveHero;
            if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsPoweredUp)
            {
                //bullet is deflected
                bullet.Velocity = -bullet.Velocity;

            }

            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsBlocking)
            {
                //hero doesn't take damage when blocking with shield
                bullet.Velocity = -bullet.Velocity;
            }
            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive)
            {
                theTarget.OnHit(weaponWielder);

                bullet.IsAlive = false;
            }
        }

    }
}

