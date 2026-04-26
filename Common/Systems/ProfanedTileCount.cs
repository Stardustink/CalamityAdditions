using CalamityAdditions.Content;
using CalamityMod.Tiles.FurnitureProfaned;
using System;
using Terraria.ModLoader;

namespace CalamityAdditions.Common.Systems
{
    public class ProfanedBiomeTileCount : ModSystem
    {
        public int ProfanedTileCount;

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            ProfanedTileCount = tileCounts[ModContent.TileType<ProfanedRock>()];

            ProfanedTileCount += tileCounts[ModContent.TileType<ProfanedCrystal>()];

        }
    }
}