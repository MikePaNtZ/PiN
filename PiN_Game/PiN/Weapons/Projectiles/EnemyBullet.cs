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

        protected override void checkBulletCollision()
        {
            Hero theTarget = gun.WeaponWielder.Level.ActiveHero;
            if (rectangle.Intersects(theTarget.BoundingRectangle))
            {
                theTarget.OnHit(this);
            }


        }

    }
}

