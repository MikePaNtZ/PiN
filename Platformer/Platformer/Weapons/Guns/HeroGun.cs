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

namespace Platformer
{

    /// <summary>
    /// Monster A derives from base enemy type
    /// </summary>
    class HeroGun : Gun
    {

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public HeroGun(Texture2D loadedTexture, GameCharacter theShooter) : base(loadedTexture, theShooter)
        {
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

