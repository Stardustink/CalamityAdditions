using CalamityAdditions.Core.NpcBehaviorOverride;
using CalamityMod.NPCs.NormalNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome.ScornEaterOverride
{
    internal class ScornEater : NPCBehaviorOverride
    {
        public override int NPCType => ModContent.NPCType<CalamityMod.NPCs.NormalNPCs.ScornEater>();

        public override void SetDefaults(NPC NPC)
        {
            NPC.lifeMax = 15_000;
            NPC.noGravity = true;
            NPC.Size = new(50);
            NPC.Profaned().IsProfaned = true;
        }

        public override bool OverrideAI(NPC NPC)
        {
            return true;
        }

        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return true;
        }

        public override string TexturePath => this.GetPath();

        public override bool OnSpawn(NPC NPC, IEntitySource source) => true;

        public enum State
        {
            debug,
            roam,
            chase
        }

        public State currentState;
    }
}
