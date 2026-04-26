using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver.Attacks
{
    internal abstract class StormWeaverAttack
    {
        public abstract StormWeaverHeadOverride.State ID { get; }

        public virtual void Enter(StormWeaverHeadOverride boss) { }

        public abstract void Update(StormWeaverHeadOverride boss);
        public virtual void Exit(StormWeaverHeadOverride boss) { }

        public virtual void Draw(StormWeaverHeadOverride boss, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

        protected void Finish(StormWeaverHeadOverride boss) => boss.MoveToNextState();
    }
}
