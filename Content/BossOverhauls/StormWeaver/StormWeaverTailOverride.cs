using CalamityAdditions.Core.NpcBehaviorOverride;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.StormWeaver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver
{
    internal class StormWeaverTailOverride : NPCBehaviorOverride
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPCType] = true;
        }
        public override int NPCType => ModContent.NPCType<StormWeaverTail>();
        public override bool OnSpawn(NPC NPC, IEntitySource source)
        {
            if (source is not null && source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC && parentNPC.type == ModContent.NPCType<StormWeaverHead>())
            {
                NPC.realLife = parentNPC.whoAmI;
            }
            return false;

        }
        public override void SetDefaults(NPC NPC)
        {
            NPC.Size = new Vector2(40);
            CalamityGlobalNPC global = NPC.Calamity();
            NPC.defense = 111;
            global.DR = 0;
            global.unbreakableDR = false;
        }
        public override bool OverrideAI(NPC NPC)
        {
            return true;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {

        }
    }
}