using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Platformer
{
    static class EnemyFactory
    {
        public static Enemy NewEnemy(Level level, Vector2 position, string enemyType)
        {
            switch(enemyType)
            {
                case "MonsterA":
                    return new MonsterA(level, position);
                case "MonsterB":
                    return new MonsterB(level, position);
                case "MonsterC":
                    return new MonsterC(level, position);
                case "MonsterD":
                    return new MonsterD(level, position);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
