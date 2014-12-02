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
        //protected int MAX_BULLETS = 10;
        protected Texture2D shootingTexture;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public EnemyGun(GameCharacter theShooter) : base(theShooter)
        {

            // set enemy's gun to fire bullets at 1 per sec
            attackRate = 1;
        }

        protected override void LoadContent()
        {
            bulletTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Bullet");
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/SilencerShot");
            shootingTexture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/MonsterB/Shoot");

            // load all bullets
            bullets = new EnemyBullet[MAX_BULLETS];
            // Slow down enemy bullets a bit.
            bulletSpeed = 10.0f;
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new EnemyBullet(this);
                bullets[i].Texture = bulletTexture; 
                bullets[i].BulletSpeed = bulletSpeed;
            }

        }

    }
}

