
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace PiN
{

    class Gun : Weapon
    {
        protected SoundEffect shootingSound;
        protected Texture2D bulletTexture;
        protected int MAX_BULLETS = 20;
        protected Bullet[] bullets;
        protected float bulletSpeed = 15.0f;
        protected Vector2 target;
        protected int countToFire;
        protected bool CanFire;

        /// <summary>
        /// Gun constructor
        /// </summary>
        public Gun(GameCharacter theShooter)
            : base(theShooter)
        {
            CanFire = true;
        }

        protected virtual float BulletSpeed {
            get {
                return bulletSpeed;
            }
        }

        public SoundEffect GunShotSound {
            get {
                return shootingSound;
            }
        }

        /// <summary>
        /// Loads the weapon.
        /// </summary>
        protected override void LoadContent()
        {
            shootingSound = weaponWielder.Level.Content.Load<SoundEffect>("Sounds/QuickLaser");
            texture = weaponWielder.Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun");
            // load all bullets
            bullets = new Bullet[MAX_BULLETS];

            for (int i = 0; i < MAX_BULLETS; i++)
            {
                bullets[i] = new Bullet(this);
                bullets[i].Texture = bulletTexture;
                bullets[i].BulletSpeed = BulletSpeed;
            }
            
        }

        public override void UpdateWeaponState(Vector2 crosshairPosition, GameTime gameTime)
        {
            // Shooting related updates
            target = crosshairPosition;
            position = weaponWielder.ArmPosition;

            // Updates the state of all bullets
            updateShooting(gameTime);
            UpdateBullets();
            
        }

        public override void PerformNormalAttack()
        {
            if (CanFire)
            {
                CanFire = false;
                countToFire = 0;
                if (GunShotSound != null)
                    GunShotSound.Play();
                FireBullet();
            }
                
        }

        public override void PerformSpecialAttack()
        {
            // Haven't implemented a special gun attack yet....
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
//            spriteBatch.Draw(
//                texture,
//                position,
//                null,
//                Color.White,
//                rotation,
//                Center,
//                1.0f,
//                weaponWielder.Flip,
//                0);

            //Draw the bullets
            foreach (GameObject bullet in bullets)
            {
                bullet.Draw(gameTime, spriteBatch);
            }

            //XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, position, target, Color.Blue, 1);
            
        }


        private void updateShooting(GameTime gameTime)
        {
            Vector2 aimDirection = position - target;
            aimDirection.Y *= -1.0f;
            aimDirection.Normalize();
            rotation = (float)Math.Atan2(aimDirection.Y, aimDirection.X) + (float)Math.PI;

            if (CanFire == false)
            {
                float gameUpdateRateSecs = (1.0f / (1000.0f / (float)gameTime.ElapsedGameTime.Milliseconds));
                int numOfUpdatesToWait = (int)((float)(1.0f / AttackRate) / gameUpdateRateSecs);
                if (countToFire >= numOfUpdatesToWait)
                {
                    CanFire = true;
                }
                else
                {
                    countToFire++;
                }
            }
            
        
        }

        protected virtual void FireBullet()
        {
            foreach (Bullet bullet in bullets)
            {
                if (!bullet.IsAlive)
                {
                    bullet.FireBullet();
                    return;
                }

            }
        }

        private void UpdateBullets()
        {
            foreach (Bullet bullet in bullets)
            {
                bullet.Update();
            }
        }

    }
}