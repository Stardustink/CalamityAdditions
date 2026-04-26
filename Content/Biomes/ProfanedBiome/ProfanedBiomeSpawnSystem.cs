using CalamityMod;
using CalamityMod.Tiles.FurnitureProfaned;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static CalamityAdditions.Content.Biomes.ProfanedBiome.ProfanedWorldGen;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome
{
    internal class ProfanedBiomeSpawnSystem : ModSystem
    {
        public static bool HasSpawned;
        private static bool AttemptedSpawnThisWorld;

        public override void OnWorldLoad()
        {
            HasSpawned = false;
            AttemptedSpawnThisWorld = false;
        }

        public override void OnWorldUnload()
        {
            HasSpawned = false;
            AttemptedSpawnThisWorld = false;
        }

        public override void PostUpdateEverything()
        {
            if (HasSpawned)
                return;

            
            SpawnProfanedGully();
        }

        private void SpawnProfanedGully()
        {
            int desiredWidth = WorldGen.genRand.Next(
                (int)(Main.maxTilesX * 0.075f),
                (int)(Main.maxTilesX * 0.105f) + 1
            );

            // Shorter relative height so it reads more like a hell sub-biome than a giant cavern.
            int desiredHeight = WorldGen.genRand.Next(
                (int)(desiredWidth * 0.20f),
                (int)(desiredWidth * 0.30f) + 1
            );

            int bestScore = int.MinValue;
            Point bestOrigin = Point.Zero;
            bool foundSpot = false;

            int minX = 100;
            int maxX = Main.maxTilesX - desiredWidth - 100;

            for (int x = minX; x <= maxX; x += 24)
            {
                int anchorY = FindUnderworldPlacementY(x, desiredWidth, desiredHeight);
                if (anchorY == -1)
                    continue;

                Rectangle candidateRect = new Rectangle(x, anchorY, desiredWidth, desiredHeight);
                if (!CanPlaceProfanedGully(candidateRect))
                    continue;

                int score = ScoreAshRegionFast(x, anchorY, desiredWidth, desiredHeight);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestOrigin = new Point(x, anchorY);
                    foundSpot = true;
                }
            }

            if (!foundSpot)
                return;

            bool placed = ProfanedAshFormationPlacer.TryPlaceFormation(
                bestOrigin.X,
                bestOrigin.Y,
                desiredWidth,
                desiredHeight,
                (ushort)ModContent.TileType<ProfanedRock>(),
                (ushort)TileID.AshGrass
            );

            if (placed)
                HasSpawned = true;
        }

        private static int FindUnderworldPlacementY(int startX, int width, int height)
        {
            int sampleStep = 12;
            int underworldTop = Main.maxTilesY - 300;
            int underworldBottom = Main.maxTilesY - 90;

            int totalSurfaceY = 0;
            int sampleCount = 0;

            for (int x = startX; x < startX + width; x += sampleStep)
            {
                int foundY = -1;

                for (int y = underworldTop; y <= underworldBottom; y++)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        continue;

                    Tile tile = Framing.GetTileSafely(x, y);
                    Tile above = Framing.GetTileSafely(x, y - 1);

                    if (tile.HasTile && (tile.TileType == TileID.Ash || tile.TileType == TileID.AshGrass) && !above.HasTile)
                    {
                        foundY = y;
                        break;
                    }
                }

                if (foundY != -1)
                {
                    totalSurfaceY += foundY;
                    sampleCount++;
                }
            }

            if (sampleCount < 4)
                return -1;

            int averageSurfaceY = totalSurfaceY / sampleCount;

            // scooch biome slightly above the average ash surface so it sits in the band instead of hanging too low.
            int placementY = averageSurfaceY - Math.Max(18, height / 4);

            if (!WorldGen.InWorld(startX, placementY, 20) || !WorldGen.InWorld(startX + width, placementY + height, 20))
                return -1;

            return placementY;
        }

        private static bool CanPlaceProfanedGully(Rectangle area)
        {
            const int padding = 12;

            Rectangle padded = new Rectangle(
                area.X - padding,
                area.Y - padding,
                area.Width + padding * 2,
                area.Height + padding * 2
            );

            return !IntersectsProtectedStructure(padded);
        }

        private static bool IntersectsProtectedStructure(Rectangle area)
        {
            const int stride = 8;

            for (int x = area.Left; x <= area.Right; x += stride)
            {
                for (int y = area.Top; y <= area.Bottom; y += stride)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        continue;

                    if (DraedonStructures.ShouldAvoidLocation(new Point(x, y), false))
                        return true;
                }
            }

            for (int x = area.Left; x <= area.Right; x++)
            {
                if (WorldGen.InWorld(x, area.Top, 10) && DraedonStructures.ShouldAvoidLocation(new Point(x, area.Top), false))
                    return true;
                if (WorldGen.InWorld(x, area.Bottom, 10) && DraedonStructures.ShouldAvoidLocation(new Point(x, area.Bottom), false))
                    return true;
            }

            for (int y = area.Top; y <= area.Bottom; y++)
            {
                if (WorldGen.InWorld(area.Left, y, 10) && DraedonStructures.ShouldAvoidLocation(new Point(area.Left, y), false))
                    return true;
                if (WorldGen.InWorld(area.Right, y, 10) && DraedonStructures.ShouldAvoidLocation(new Point(area.Right, y), false))
                    return true;
            }

            return false;
        }

        private static int ScoreAshRegionFast(int startX, int startY, int width, int height)
        {
            int score = 0;
            const int stepX = 8;
            const int stepY = 6;

            for (int x = startX; x < startX + width; x += stepX)
            {
                if (x <= 10 || x >= Main.maxTilesX - 10)
                    continue;

                for (int y = startY; y < startY + height; y += stepY)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        continue;

                    Tile tile = Framing.GetTileSafely(x, y);

                    if (!tile.HasTile)
                    {
                        score -= 1;
                        continue;
                    }

                    if (tile.TileType == TileID.Ash)
                        score += 3;
                    else if (tile.TileType == TileID.AshGrass)
                        score += 4;
                    else if (tile.TileType == TileID.Hellstone)
                        score += 1;
                    else if (tile.TileType == TileID.HellstoneBrick || tile.TileType == TileID.ObsidianBrick)
                        score -= 5;
                    else
                        score -= 2;
                }
            }

            return score;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["HasSpawned"] = HasSpawned;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            HasSpawned = tag.GetBool("HasSpawned");
        }
    }
}