using CalamityAdditions.Common.Systems;
using CalamityAdditions.Content.Biomes.ProfanedBiome.ProfanedConstructNPC;
using CalamityMod.NPCs.NormalNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;


namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    internal class ProfanedGully : ModBiome
    {
      
        public override bool IsBiomeActive(Player player)
        {
            bool ProfanedRockCount = ModContent.GetInstance<ProfanedBiomeTileCount>().ProfanedTileCount >= 100;


            return ProfanedRockCount;

        }

        public override int Music => ModContent.GetInstance<CalamityMod.CalamityMod>().GetMusicFromMusicMod("SunkenSea") ?? MusicID.OceanNight;

        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        
       
    }

    public class ProfanedGullySpawns : GlobalNPC
    {

        public override bool InstancePerEntity => false;

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            Player Player = spawnInfo.Player;

            if (!Player.InModBiome<ProfanedGully>())
            {
                return;
            }

            //if (spawnInfo.PlayerSafe || spawnInfo.PlayerInTown || spawnInfo.Invasion)
            //return;

            pool.Clear();

            pool[ModContent.NPCType<ProfanedEnergyBody>()] = 0.2f;
            pool[ModContent.NPCType<ImpiousImmolator>()] = 0.7f;
            pool[ModContent.NPCType<ProfanedConstruct>()] = 1;

            if (spawnInfo.SpawnTileType == TileID.Ash)
            {
                pool[ModContent.NPCType<ScornEater>()] = 1;
            }
        }
    }
}
