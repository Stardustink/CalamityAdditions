using CalamityAdditions.Content.Biomes.ProfanedBiome;
using CalamityMod.NPCs.NormalNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Core
{
    internal class OverrideSpawns : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            Player Player = spawnInfo.Player;

            if (Player.ZoneHallow && !Player.InModBiome<ProfanedGully>())
            {
                pool.Remove(ModContent.NPCType<ProfanedEnergyBody>());
                pool.Remove(ModContent.NPCType<ImpiousImmolator>());
                pool.Remove(ModContent.NPCType<ScornEater>());
            }

            if (Player.ZoneUnderworldHeight && !Player.InModBiome<ProfanedGully>())
            {
                pool.Remove(ModContent.NPCType<ProfanedEnergyBody>());
                pool.Remove(ModContent.NPCType<ImpiousImmolator>());
                pool.Remove(ModContent.NPCType<ScornEater>());
            }
        }
    }
}
