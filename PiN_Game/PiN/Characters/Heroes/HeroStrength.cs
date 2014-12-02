using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiN
{
    class HeroStrength: Hero
    {
        public override int MaxHealth
        {
            get { return 150; }
        }

         public HeroStrength(Level level, Vector2 initialPosition): base(level, initialPosition)
        {
            moveSpeed = 1.0F;
        }

        protected override void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Idle"), 0.1f, true);
            /*shieldPart1Animation is an image of just the shield alone; I couldn't get the shield to be overlayed onto the resetAfterHit
              I may have to do this later for the overdrive meter of the activeHero*/
            shieldPart1Animation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/ShieldPart1"), 0.1f, true);
            shieldAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Shield"), 0.1f, true); //load image for the shield
            shieldSprite = Level.Content.Load<Texture2D>("Sprites/HeroStrength/ShieldPart1");

            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Die"), 0.1f, false);
            flinchAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/HeroStrength/Idle"), 0.1f, false); //placeholder

            // Calculate bounds within texture size.            
            // TODO It needs to be more clear what this is doing, and why it is done here. It is for collision detection.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MaleDie");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/MaleDie");
            hurtSound = Level.Content.Load<SoundEffect>("Sounds/MaleHurt");//placeholder
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/Powerup");

            // Load character's default weapon
            weapon = new HeroStrengthGun(this);
            base.LoadContent();
        }
    }
}
