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
    /// Enemy bullets collide with players. Need to override checkBulletCollision
    /// </summary>
    class EnemyBullet : Bullet
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyBullet(Gun theGun) : base(theGun)
        {
        }

        protected override void checkBulletCollision(Rectangle bulletRect)
        {
            Hero theTarget = gun.WeaponWielder.Level.ActiveHero;
            if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsPoweredUp)
            {
                //bullet is deflected
                //bullet.Velocity = -0.7f*bullet.Velocity;
                IsAlive = false;
            }

            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive & theTarget.IsBlocking)
            {
                //hero doesn't take damage when blocking with shield
                Velocity = -0.7f*Velocity;
            }
            else if (bulletRect.Intersects(theTarget.BoundingRectangle) & theTarget.IsAlive)
            {
                theTarget.OnHit(gun.WeaponWielder);

                IsAlive = false;
            }


        }

    }
}

