using CalamityMod.Buffs.DamageOverTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    public class ProfanedNPC : GlobalNPC
    {
        public bool IsProfaned { get; set; }
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return base.AppliesToEntity(entity, lateInstantiation);
        }


        public override void Load()
        {
            On_NPC.SetDefaults += PerformProfanedBuffs;
        }

        private void PerformProfanedBuffs(On_NPC.orig_SetDefaults orig, NPC self, int Type, NPCSpawnParams spawnparams)
        {
            orig(self, Type, spawnparams);

            self.buffImmune[ModContent.BuffType<HolyFlames>()] = true;
            self.lavaImmune = true;
        }
    }

    public static class ProfanedNPCExtension
    {
        public static ProfanedNPC Profaned(this NPC npc) =>
          npc.GetGlobalNPC<ProfanedNPC>();
    }
}
