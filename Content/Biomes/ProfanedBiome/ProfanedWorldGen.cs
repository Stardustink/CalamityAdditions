using CalamityMod.Tiles.FurnitureProfaned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    internal class ProfanedWorldGen
    {

    }

    public static class ProfanedBiomePlacer
    {

        public static bool PlaceHellRock(int centerX, int centerY, int radiusX, int radiusY)
        {
            if (!WorldGen.InWorld(centerX, centerY, 50))
                return false;

           
            if (centerY < Main.maxTilesY - 300)
                return false;

            int left = centerX - radiusX - 4;
            int right = centerX + radiusX + 4;
            int top = centerY - radiusY - 4;
            int bottom = centerY + radiusY + 4;

            if (!WorldGen.InWorld(left, top, 10) || !WorldGen.InWorld(right, bottom, 10))
                return false;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    float dx = (x - centerX) / (float)radiusX;
                    float dy = (y - centerY) / (float)radiusY;
                    float dist = dx * dx + dy * dy;

                    float noise =
                        (float)Math.Sin(x * 0.19f) * 0.08f +
                        (float)Math.Cos(y * 0.23f) * 0.08f +
                        WorldGen.genRand.NextFloat(-0.04f, 0.04f);

                    if (dist > 1f + noise)
                        continue;

                    WorldGen.PlaceTile(x, y, ModContent.TileType<ProfanedRock>(), mute: true, forced: true);
                }
            }

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        continue;

                    WorldGen.TileFrame(x, y, true, false);
                }
            }

            SyncRectangle(left, top, right - left + 1, bottom - top + 1);
            return true;
        }

        private static void SyncRectangle(int left, int top, int width, int height)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            NetMessage.SendTileSquare(-1, left + width / 2, top + height / 2, Math.Max(width, height));
        }

    }
}
