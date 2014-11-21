
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
        protected int MAX_BULLETS = 12;
        protected Bullet[] bullets;
        protected float bulletSpeed = 15.0f;
        protected Vector2 target;

        /// <summary>
        /// Gun constructor
        /// </summary>
        public Gun(GameCharacter theShooter)
            : base(theShooter)
        {
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
                bullets[i].BulletSpeed = bulletSpeed;
            }
            
        }

        public override void UpdateWeaponState(Vector2 crosshairPosition)
        {
            // Shooting related updates
            target = crosshairPosition;
            position = weaponWielder.ArmPosition;

            // Updates the state of all bullets
            updateShooting();
            UpdateBullets();
            
        }

        public override void PerformNormalAttack()
        {
            FireBullet();
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

            XnaDebugDrawer.DebugDrawer.DrawLineSegment(spriteBatch, position, target, Color.Blue, 1);
            
        }


        private void updateShooting()
        {
            Vector2 aimDirection = position - target;
            aimDirection.Y *= -1.0f;
            aimDirection.Normalize();
            rotation = (float)Math.Atan2(aimDirection.Y, aimDirection.X) + (float)Math.PI;
        }

        private void FireBullet()
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