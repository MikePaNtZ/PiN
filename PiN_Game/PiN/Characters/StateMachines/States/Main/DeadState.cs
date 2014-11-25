using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PiN
{
    class DeadState : MainState
    {
        public DeadState(CharacterStateMachine HSM, GameObject killedBy) : base(HSM)
        {
            if (killedBy != null)
            {
                character.Health = 0;
                character.KilledSound.Play();
            }
            else //fell
            {
                character.Health = 0;
                if (character.FallSound != null)
                    character.FallSound.Play();
            }
            character.sprite.LoadAnimation(character.DieAnimation);
        }
        public override void Update(GameTime gameTime, InputHandler gameInputs)
        {
            character.Movement = 0;
            //dead nothing updates
        }
        public override void OnHit(GameObject hitBy)
        {
            //can't get hit if you're dead
        }
    }
}
