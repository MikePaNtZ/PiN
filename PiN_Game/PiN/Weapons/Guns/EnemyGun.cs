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
    class EnemyGun : Gun
    {
        protected int MAX_BULLETS = 1;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyGun(GameCharacter theShooter) : base(theShooter)
        {
        }

        protected override void LoadContent()
        {
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Bullet");
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/SilencerShot");
            base.LoadContent();
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

