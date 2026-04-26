using CalamityMod.Tiles.FurnitureProfaned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    internal class ProfanedBiomeDebug : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 10;
            Item.consumable = false;
        }

        public override void UseAnimation(Player player)
        {

            if (player is null || !player.active)
                return;

            // Convert player world position to tile position.
            Point tilePos = player.Center.ToTileCoordinates();

            int centerX = tilePos.X;
            int centerY = tilePos.Y + 10;

            bool success = ProfanedWorldGen.ProfanedAshFormationPlacer.TryPlaceFormation(centerX, centerY, 700, 300, (ushort)ModContent.TileType<ProfanedRock>(), TileID.Ash);


        }
    }
}
