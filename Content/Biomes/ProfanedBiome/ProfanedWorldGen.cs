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
        internal static class ProfanedAshFormationPlacer
        {
            public static bool TryPlaceFormation(int startX, int startY, int width, int height, ushort rockTileType, ushort topTileType)
            {
                if (!WorldGen.InWorld(startX, startY, 50) || !WorldGen.InWorld(startX + width, startY + height, 50))
                    return false;

                int left = startX;
                int right = startX + width;
                int top = startY;
                int bottom = startY + height;

                int rim = 12;

                ClearInterior(left, top, right, bottom, rim);

                int segmentCount = WorldGen.genRand.Next(12, 20);
                int segmentWidth = width / segmentCount;

                int[] floorHeights = new int[width + 1];
                for (int i = 0; i <= width; i++)
                    floorHeights[i] = bottom - 30;

                for (int seg = 0; seg < segmentCount; seg++)
                {
                    int segLeft = left + seg * segmentWidth;
                    int segRight = seg == segmentCount - 1 ? right : segLeft + segmentWidth - 1;

                    bool makeGap = WorldGen.genRand.NextBool(3);
                    if (makeGap)
                        continue;

                    int plateauTop = bottom - height / 5;
                    float noiseOffset = WorldGen.genRand.NextFloat(1000f);

                    int segmentWidthActual = segRight - segLeft + 1;
                    int[] topHeights = new int[segmentWidthActual];

                    for (int i = 0; i < segmentWidthActual; i++)
                    {
                        int x = segLeft + i;

                        float progress = i / (float)Math.Max(1, segmentWidthActual - 1);
                        float arch = (float)Math.Sin(progress * MathHelper.Pi) * WorldGen.genRand.NextFloat(4f, 10f);
                        float noise =
                            (float)Math.Sin((x + noiseOffset) * 0.17f) * 2.5f +
                            (float)Math.Cos((x + noiseOffset) * 0.09f) * 1.75f;

                        topHeights[i] = plateauTop - (int)arch + (int)noise;
                    }

                    SmoothHeights(topHeights, 4);

                    int slantAmount = WorldGen.genRand.Next(-10, 11);
                    int localTopMin = topHeights.Min();

                    for (int y = localTopMin; y <= bottom + 100; y++)
                    {
                        float depthProgress = (y - localTopMin) / (float)Math.Max(1, (bottom + 100) - localTopMin);
                        int horizontalOffset = (int)(slantAmount * depthProgress);
                        int taperInset = (int)(depthProgress * 12f);

                        int leftEdge = segLeft + taperInset + horizontalOffset;
                        int rightEdge = segRight - taperInset + horizontalOffset;

                        for (int x = leftEdge; x <= rightEdge; x++)
                        {
                            int sourceIndex = x - horizontalOffset - segLeft;
                            if (sourceIndex < 0 || sourceIndex >= topHeights.Length)
                                continue;

                            int localTop = topHeights[sourceIndex];
                            if (y < localTop || !WorldGen.InWorld(x, y, 20))
                                continue;

                            PlaceSolidTile(x, y, rockTileType);

                            int floorIndex = Math.Clamp(x - left, 0, floorHeights.Length - 1);
                            floorHeights[floorIndex] = Math.Min(floorHeights[floorIndex], localTop);
                        }
                    }

                    int bulgeCount = WorldGen.genRand.Next(2, 5);
                    for (int b = 0; b < bulgeCount; b++)
                    {
                        int cx = WorldGen.genRand.Next(segLeft, segRight + 1);
                        int cy = WorldGen.genRand.Next(plateauTop + 4, bottom - 4);
                        int rx = WorldGen.genRand.Next(4, 10);
                        int ry = WorldGen.genRand.Next(4, 8);

                        StampBlob(cx, cy, rx, ry, rockTileType);
                    }
                }

                SkinTopSurfaces(left, top, right, bottom, rockTileType, topTileType);
                FillLavaPools(left, top, right, bottom, floorHeights);
                MakeRoof(left + rim, top, right - rim, top, rockTileType);
                MakeCeilingStalactites(left + 8, top, right - 8, bottom - 8, (ushort)ModContent.TileType<ProfanedCrystal>());


                CarveConnectorTunnel(left, top, right, bottom, true);
                CarveConnectorTunnel(left, top, right, bottom, false);
                CarveVerticalBreaks(left, top, right, bottom);

                CleanupRim(left, top, right, bottom, rim, rockTileType, topTileType);

                FrameArea(left, top, right, bottom);
                SyncArea(left, top, right, bottom);

                return true;
            }

            private static void ClearInterior(int left, int top, int right, int bottom, int rim)
            {
                for (int x = left + rim; x <= right - rim; x++)
                {
                    for (int y = top + rim; y <= bottom - rim; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);
                        tile.HasTile = false;
                        tile.LiquidAmount = 0;
                        tile.LiquidType = LiquidID.Water;
                        tile.WallType = 0;
                    }
                }
            }

            private static void CarveConnectorTunnel(int left, int top, int right, int bottom, bool fromLeft)
            {
                int startX = fromLeft ? left - 8 : right + 8;
                int endX = fromLeft ? left + 40 : right - 40;

                int y = WorldGen.genRand.Next(top + 24, bottom - 28);
                int radius = WorldGen.genRand.Next(6, 10);

                for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
                {
                    float t = (x - Math.Min(startX, endX)) / (float)Math.Max(1, Math.Abs(endX - startX));
                    int driftY = y + (int)(Math.Sin(t * MathHelper.Pi) * WorldGen.genRand.NextFloat(-8f, 8f));

                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int wx = x + dx;
                            int wy = driftY + dy;

                            if (!WorldGen.InWorld(wx, wy, 20))
                                continue;

                            float dist = dx * dx + dy * dy;
                            if (dist > radius * radius)
                                continue;

                            Tile tile = Framing.GetTileSafely(wx, wy);
                            tile.HasTile = false;
                            tile.LiquidAmount = 0;
                        }
                    }
                }
            }

            private static void CarveVerticalBreaks(int left, int top, int right, int bottom)
            {
                int count = WorldGen.genRand.Next(2, 4);

                for (int i = 0; i < count; i++)
                {
                    int x = WorldGen.genRand.Next(left + 24, right - 24);
                    int length = WorldGen.genRand.Next(18, 40);
                    int width = WorldGen.genRand.Next(3, 6);

                    for (int y = top; y < top + length; y++)
                    {
                        for (int dx = -width; dx <= width; dx++)
                        {
                            if (!WorldGen.InWorld(x + dx, y, 20))
                                continue;

                            if (Math.Abs(dx) == width && WorldGen.genRand.NextBool())
                                continue;

                            Tile tile = Framing.GetTileSafely(x + dx, y);
                            tile.HasTile = false;
                            tile.LiquidAmount = 0;
                        }
                    }
                }
            }

            private static void CleanupRim(int left, int top, int right, int bottom, int rim, ushort rockTileType, ushort topTileType)
            {
                for (int x = left; x <= right; x++)
                {
                    for (int y = top; y <= bottom; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        bool inRim =
                            x < left + rim || x > right - rim ||
                            y < top + rim || y > bottom - rim;

                        if (!inRim)
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);
                        if (!tile.HasTile)
                            continue;

                        if (tile.TileType != rockTileType && tile.TileType != topTileType)
                            continue;

                        int neighbors = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;

                                if (!WorldGen.InWorld(x + dx, y + dy, 20))
                                    continue;

                                if (Framing.GetTileSafely(x + dx, y + dy).HasTile)
                                    neighbors++;
                            }
                        }

                        if (neighbors <= 2)
                            tile.HasTile = false;
                    }
                }
            }
            private static void ClearArea(int left, int top, int right, int bottom)
            {
                for (int x = left; x <= right; x++)
                {
                    for (int y = top; y <= bottom; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);
                        tile.HasTile = false;
                        tile.LiquidAmount = 0;
                        tile.LiquidType = LiquidID.Water;
                        tile.WallType = 0;
                    }
                }
            }
            private static void SmoothHeights(int[] heights, int passes)
            {
                if (heights.Length < 3)
                    return;

                int[] buffer = new int[heights.Length];

                for (int pass = 0; pass < passes; pass++)
                {
                    buffer[0] = heights[0];
                    buffer[^1] = heights[^1];

                    for (int i = 1; i < heights.Length - 1; i++)
                    {
                        buffer[i] = (heights[i - 1] + heights[i] + heights[i + 1]) / 3;
                    }

                    Array.Copy(buffer, heights, heights.Length);
                }
            }


            private static void PlaceSolidTile(int x, int y, ushort tileType)
            {
                if (!WorldGen.InWorld(x, y, 20))
                    return;

                WorldGen.PlaceTile(x, y, tileType, mute: true, forced: true);
                Tile tile = Framing.GetTileSafely(x, y);
                tile.LiquidAmount = 0;
            }

            private static void StampBlob(int centerX, int centerY, int radiusX, int radiusY, ushort tileType)
            {
                for (int x = centerX - radiusX - 1; x <= centerX + radiusX + 1; x++)
                {
                    for (int y = centerY - radiusY - 1; y <= centerY + radiusY + 1; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        float dx = (x - centerX) / (float)Math.Max(1, radiusX);
                        float dy = (y - centerY) / (float)Math.Max(1, radiusY);
                        float dist = dx * dx + dy * dy;

                        float noise =
                            (float)Math.Sin(x * 0.23f) * 0.08f +
                            (float)Math.Cos(y * 0.19f) * 0.08f +
                            WorldGen.genRand.NextFloat(-0.4f, 0.4f);

                        if (dist <= 1f + noise)
                            PlaceSolidTile(x, y, tileType);
                    }
                }
            }

            private static void SkinTopSurfaces(int left, int top, int right, int bottom, ushort baseTileType, ushort topTileType)
            {
                for (int x = left; x <= right; x++)
                {
                    int skinDepth = WorldGen.genRand.Next(2, 5);
                    for (int y = top + 1; y < bottom; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);
                        Tile above = Framing.GetTileSafely(x, y - 1);

                        if (!tile.HasTile || tile.TileType != baseTileType)
                            continue;

                        if (above.HasTile)
                            continue;

                        for (int k = 0; k < skinDepth; k++)
                        {
                            int yy = y + k;
                            if (!WorldGen.InWorld(x, yy, 20))
                                break;

                            Tile skinTile = Framing.GetTileSafely(x, yy);
                            if (!skinTile.HasTile || skinTile.TileType != baseTileType)
                                break;

                            WorldGen.PlaceTile(x, yy, topTileType, mute: true, forced: true);
                        }
                    }
                }
            }

            private static void FillLavaPools(int left, int top, int right, int bottom, int[] floorHeights)
            {
                int lavaSurface = bottom - 50;

                for (int x = left; x <= right; x++)
                {
                    int localIndex = x - left;
                    int rockTop = floorHeights[Math.Clamp(localIndex, 0, floorHeights.Length - 1)];

                    int fillStart = Math.Max(rockTop + 8, lavaSurface);
                    for (int y = fillStart; y <= bottom + 50; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        Tile tile = Framing.GetTileSafely(x, y);
                        if (tile.HasTile)
                            continue;

                        tile.LiquidType = LiquidID.Lava;
                        tile.LiquidAmount = byte.MaxValue;
                    }
                }
            }
            private static void MakeRoof(int left, int top, int right, int bottom, ushort tileType)
            {
                int roofDepth = WorldGen.genRand.Next(10, 18);

                for (int x = left; x <= right; x++)
                {
                    float noise =
                        (float)Math.Sin(x * 0.07f) * 3f +
                        (float)Math.Cos(x * 0.13f) * 2f;

                    int localDepth = roofDepth + (int)noise;

                    for (int y = top; y <= top + localDepth; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        PlaceSolidTile(x, y, tileType);
                    }
                }
            }
            private static void MakeCeilingStalactites(int left, int top, int right, int maxBottom, ushort tileType)
            {
                int count = WorldGen.genRand.Next(5, 10);

                for (int i = 0; i < count; i++)
                {
                    int centerX = WorldGen.genRand.Next(left + 6, right - 6);

                    int startY = top;
                    while (startY < maxBottom)
                    {
                        Tile tile = Framing.GetTileSafely(centerX, startY);
                        if (tile.HasTile)
                            break;

                        startY++;
                    }

                    if (startY >= maxBottom)
                        continue;

                    int length = WorldGen.genRand.Next(24, 60);
                    int halfWidth = WorldGen.genRand.Next(4, 9);

                    int slantAmount = WorldGen.genRand.Next(-12, 13);

                    for (int y = startY + 1; y < startY + length && y <= maxBottom; y++)
                    {
                        float t = (y - (startY + 1)) / (float)Math.Max(1, length - 1);
                        float eased = 1f - t;

                        int widthHere = Math.Max(1, (int)(halfWidth * eased));
                        int horizontalOffset = (int)(slantAmount * t);
                        int currentCenterX = centerX + horizontalOffset;

                        for (int x = currentCenterX - widthHere; x <= currentCenterX + widthHere; x++)
                        {
                            if (!WorldGen.InWorld(x, y, 20))
                                continue;

                            PlaceSolidTile(x, y, tileType);
                        }
                    }
                }
            }

            private static void FrameArea(int left, int top, int right, int bottom)
            {
                for (int x = left - 1; x <= right + 1; x++)
                {
                    for (int y = top - 1; y <= bottom + 1; y++)
                    {
                        if (!WorldGen.InWorld(x, y, 20))
                            continue;

                        WorldGen.TileFrame(x, y, true, false);
                        WorldGen.SquareWallFrame(x, y, true);
                    }
                }
            }

            private static void SyncArea(int left, int top, int right, int bottom)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    return;

                int width = right - left + 1;
                int height = bottom - top + 1;
                int size = Math.Max(width, height);

                NetMessage.SendTileSquare(-1, left + width / 2, top + height / 2, size);
            }
        }
    }

    
}
