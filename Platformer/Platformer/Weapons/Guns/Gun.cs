
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Platformer
{

    class Gun : Weapon
    {
        private GameObject gunObj;
        private GameObject crosshair;
        private GameObject[] bullets;
        private int MAX_BULLETS = 12;

        /// <summary>
        /// Gun constructor
        /// </summary>
        public Gun(Texture2D loadedTexture, GameCharacter theShooter)
            : base(loadedTexture, theShooter)
        {
            LoadContent();

        }

        /// <summary>
        /// Loads the weapon.
        /// </summary>
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

        public override void UpdateWeaponState(InputHandler gameInputs)
        {
            // Shooting related updates
            crosshair.Position = gameInputs.MouseInput.Position;

            if (weaponWielder.Flip == SpriteEffects.FlipHorizontally)
                theWeapon.Position = new Vector2(weaponWielder.Position.X + 5, weaponWielder.Position.Y - 60);
            else
                theWeapon.Position = new Vector2(weaponWielder.Position.X - 5, weaponWielder.Position.Y - 60);

            // Updates the state of all bullets
            UpdateBullets();
            updateShooting(gameInputs);
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

            //            System.Diagnostics.Debug.WriteLine("X-Hair Pos: " + crosshair.Position);
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


        private void updateShooting(InputHandler gameInputs)
        {
            Vector2 aimDirection = theWeapon.Position - crosshair.Position;
            theWeapon.Rotation = (float)Math.Atan2(aimDirection.Y, aimDirection.X) - (float)Math.PI / 2; //this will return the mouse angle(in radians).

            if (weaponWielder.Flip == SpriteEffects.FlipHorizontally) //Facing right
            {
                //If we try to aim behind our head then flip the
                //character around.
                if (theWeapon.Rotation < 0)
                    weaponWielder.Flip = SpriteEffects.None;

                //If we aren't rotating our gun then set it to the
                //default position. Aiming in front of us.
                if (theWeapon.Rotation == 0)
                    theWeapon.Rotation = MathHelper.PiOver2;
            }
            else //Facing left
            {
                //Once again, if we try to aim behind us then
                //flip our character.
                if (theWeapon.Rotation > 0)
                    weaponWielder.Flip = SpriteEffects.FlipHorizontally;

                //If we're not rotating our gun, default it to
                //aim the same direction we're facing.
                if (theWeapon.Rotation == 0)
                    theWeapon.Rotation = -MathHelper.PiOver2;
            }
        }

        private void FireBullet()
        {
            foreach (GameObject bullet in bullets)
            {
                //Find a bullet that isn't alive
                if (!bullet.IsAlive)
                {
                    //And set it to alive.
                    bullet.IsAlive = true;

                    if (weaponWielder.Flip == SpriteEffects.FlipHorizontally) //Facing right
                    {
                        float theWeaponCos = (float)Math.Cos(theWeapon.Rotation - MathHelper.PiOver2);
                        float theWeaponSin = (float)Math.Sin(theWeapon.Rotation - MathHelper.PiOver2);

                        //Set the initial position of our bullet at the end of our gun
                        //42 is obtained be taking the width of the Arm_Gun texture / 2
                        //and subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
                        bullet.Position = new Vector2(
                            theWeapon.Position.X + 42 * theWeaponCos,
                            theWeapon.Position.Y + 42 * theWeaponSin);

                        //And give it a velocity of the direction we're aiming.
                        //Increase/decrease speed by changing 15.0f
                        bullet.Velocity = new Vector2(
                            (float)Math.Cos(theWeapon.Rotation - MathHelper.PiOver2),
                            (float)Math.Sin(theWeapon.Rotation - MathHelper.PiOver2)) * 15.0f;
                    }
                    else //Facing left
                    {
                        float theWeaponCos = (float)Math.Cos(theWeapon.Rotation + MathHelper.PiOver2);
                        float theWeaponSin = (float)Math.Sin(theWeapon.Rotation + MathHelper.PiOver2);

                        //Set the initial position of our bullet at the end of our gun
                        //42 is obtained be taking the width of the Arm_Gun texture / 2
                        //and subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
                        bullet.Position = new Vector2(
                            theWeapon.Position.X - 42 * theWeaponCos,
                            theWeapon.Position.Y - 42 * theWeaponSin);

                        //And give it a velocity of the direction we're aiming.
                        //Increase/decrease speed by changing 15.0f
                        bullet.Velocity = new Vector2(
                           -theWeaponCos,
                           -theWeaponSin) * 15.0f;
                    }
                    return;
                }
            }
        }

        private void UpdateBullets()
        {
            //Check all of our bullets
            foreach (GameObject bullet in bullets)
            {
                //Only update them if they're alive
                if (bullet.IsAlive)
                {
                    //Move our bullet based on it's velocity
                    bullet.Position += bullet.Velocity;

                    //Rectangle the size of the screen so bullets that
                    //fly off screen are deleted.
                    Rectangle screenRect = new Rectangle(0, 0, (int)weaponWielder.Level.Camera.Position.X + weaponWielder.Level.Camera.ViewPort.Width, (int)weaponWielder.Level.Camera.Position.Y + weaponWielder.Level.Camera.ViewPort.Height * 2);
                    if (!screenRect.Contains(new Point(
                        (int)bullet.Position.X,
                        (int)bullet.Position.Y)))
                    {
                        bullet.IsAlive = false;
                        continue;
                    }

                    //Collision rectangle for each bullet -Will also be
                    //used for collisions with enemies.
                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.Position.X - bullet.Texture.Width * 2,
                        (int)bullet.Position.Y - bullet.Texture.Height * 2,
                        bullet.Texture.Width * 4,
                        bullet.Texture.Height * 4);


                    //Check for collisions with the enemies
                    foreach (Enemy enemy in weaponWielder.Level.enemies)
                    {
                        if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                        {
                            //We're going to want to put some enemy health reduction code here
                            //Enemy class needs a health member variable too
                            enemy.OnHit();
                            bullet.IsAlive = false;
                        }
                    }

                    //Everything below here can be deleted if you want
                    //your bullets to shoot through all tiles.

                    //Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / weaponWielder.Level.TileWidth);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / weaponWielder.Level.TileWidth)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / weaponWielder.Level.TileHeight);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / weaponWielder.Level.TileHeight)) - 1;

                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = weaponWielder.Level.GetCollision(x, y);

                            //If we collide with an Impassable or Platform tile
                            //then delete our bullet.
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    bullet.IsAlive = false;
                            }
                        }
                    }
                }
            }
        }


    }
}