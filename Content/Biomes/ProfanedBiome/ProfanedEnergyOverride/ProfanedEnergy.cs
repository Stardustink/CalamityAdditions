using CalamityAdditions.Core.NpcBehaviorOverride;
using CalamityMod.NPCs.NormalNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome.ProfanedEnergyOverride
{
    internal class ProfanedEnergy : NPCBehaviorOverride
    {
        public override int NPCType => ModContent.NPCType<ProfanedEnergyBody>();

        public override void SetDefaults(NPC NPC)
        {
            NPC.lifeMax = 15_000;
            NPC.noGravity = true;
            NPC.Size = new(50);
            NPC.Profaned().IsProfaned = true;
        }

        public override bool OverrideAI(NPC NPC)
        {
            return false;
        }

        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override string TexturePath => this.GetPath();

        public override bool OnSpawn(NPC NPC, IEntitySource source) => false;
    }
}
