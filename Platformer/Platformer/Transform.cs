using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    public static class Transform
    {
        public static Vector2 FromGfToVpf(Vector2 vector, Viewport viewPort)
        {
            Vector2 returnVect = new Vector2((float)0.0, (float)0.0);
            returnVect.X = vector.X - viewPort.Width / 2;
            returnVect.Y = -(vector.Y - viewPort.Height/2) ;
            return returnVect;
        }
        public static Vector2 FromVpfToGf(Vector2 vector, Viewport viewPort)
        {
            Vector2 returnVect = new Vector2((float)0.0, (float)0.0);
            returnVect.X = vector.X - viewPort.Width / 2;
            returnVect.Y = (vector.Y + viewPort.Height/2);
            return returnVect;
        }
        public static Vector2 FromCfToVpf(Vector2 vector, Viewport viewPort)
        {
            Vector2 returnVect = new Vector2((float)0.0, (float)0.0);
            returnVect.X = vector.X + viewPort.Width / 2;
            returnVect.Y = vector.Y;
            return returnVect;
        }
        public static Vector2 FromVpfToCf(Vector2 vector, Viewport viewPort)
        {
            Vector2 returnVect = new Vector2((float)0.0, (float)0.0);
            returnVect.X = vector.X - viewPort.Width / 2;
            returnVect.Y = vector.Y;
            return returnVect;
        }
        
    }
}
