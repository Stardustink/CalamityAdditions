using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Core.Utilities
{
    public static partial class Utilities
    {
        public static bool RaytraceTo(int x0, int y0, int x1, int y1, bool ignoreHalfTiles = false)
        {
            //Bresenham's algorithm
            var horizontalDistance = Math.Abs(x1 - x0); //Delta X
            var verticalDistance = Math.Abs(y1 - y0); //Delta Y
            var horizontalIncrement = x1 > x0 ? 1 : -1; //S1
            var verticalIncrement = y1 > y0 ? 1 : -1; //S2

            var x = x0;
            var y = y0;
            var E = horizontalDistance - verticalDistance;

            while (true)
            {
                if ((!ignoreHalfTiles || !Main.tile[x, y].IsHalfBlock))
                {
                    return false;
                }

                if (x == x1 && y == y1)
                {
                    return true;
                }

                var E2 = E * 2;

                if (E2 >= -verticalDistance)
                {
                    if (x == x1)
                    {
                        return true;
                    }

                    E -= verticalDistance;
                    x += horizontalIncrement;
                }

                if (E2 <= horizontalDistance)
                {
                    if (y == y1)
                    {
                        return true;
                    }

                    E += horizontalDistance;
                    y += verticalIncrement;
                }
            }
        }

        public static Point? RaycastTo(Vector2 start, Vector2 end, bool ignoreHalfTiles = false, bool debug = false, bool ShouldCountWater = false)
        {
            var x0 = (int)(start.X / 16f);
            var y0 = (int)(start.Y / 16f);
            var x1 = (int)(end.X / 16f);
            var y1 = (int)(end.Y / 16f);

            return RaycastTo(x0, y0, x1, y1, ignoreHalfTiles, debug, ShouldCountWater);
        }

        public static Point? RaycastTo(int x0, int y0, int x1, int y1, bool ignoreHalfTiles = false, bool debug = false, bool ShouldCountWater = false)
        {
            x0 = Utils.Clamp(x0, 0, Main.maxTilesX - 1);
            y0 = Utils.Clamp(y0, 0, Main.maxTilesY - 1);
            x1 = Utils.Clamp(x1, 0, Main.maxTilesX - 1);
            y1 = Utils.Clamp(y1, 0, Main.maxTilesY - 1);

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = x1 > x0 ? 1 : -1;
            var sy = y1 > y0 ? 1 : -1;

            var x = x0;
            var y = y0;
            var err = dx - dy;

            while (true)
            { 
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                {
                    break;
                }



                var tile = Main.tile[x, y];

                if (ShouldCountWater &&
                    tile != null &&
                    tile.LiquidAmount > 0)
                {
                    int surfaceY = y;

                    while (surfaceY > 0)
                    {
                        Tile above = Main.tile[x, surfaceY - 1];

                        if (above == null ||
                            above.LiquidAmount == 0 ||
                            above.LiquidType != tile.LiquidType)
                            break;

                        surfaceY--;
                    }
                    if (debug)
                        RayCastVisualizer.Raycasts.Add(new Raycast(new Vector2(x0, y0), new Vector2(x, surfaceY), Color.Blue));

                    return new Point(x, surfaceY);
                }

                if (tile != null &&
                    tile.HasTile &&
                    (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] || (Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0))
                    &&
                        Main.tileSolid[tile.TileType] &&
                   !Main.tileCut[tile.TileType] &&
                   !Main.tileNoAttach[tile.TileType] && !Main.tileAxe[tile.TileType] &&
                   Main.tileBlockLight[tile.TileType] &&
                   Collision.SolidCollision(new Vector2(x, y).ToWorldCoordinates(), 16, 16) &&
                    WorldGen.SolidTile(tile) && Collision.IsWorldPointSolid(new Point(x, y).ToWorldCoordinates()))
                {
                    int surfaceY = y;
                    if (debug)
                        RayCastVisualizer.Raycasts.Add(new Raycast(new Vector2(x0, y0), new Vector2(x, surfaceY), Color.Blue));
                    return new Point(x, surfaceY);
                }


                if (x == x1 && y == y1)
                {
                    break;
                }

                var e2 = err * 2;

                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }

            // No tile hit
            return null;
        }
    }

    internal class RayCastVisualizer : ModSystem
    {
        public static List<Raycast> Raycasts = new();

        public static List<RaycastText> Texts = new();
        public override void PostUpdateEverything()
        {
            for (int i = Raycasts.Count - 1; i >= 0; i--)
            {
                Raycasts[i].Update();

                if (Raycasts[i].IsDead)
                    Raycasts.RemoveAt(i);
            }

            for (int i = Texts.Count - 1; i >= 0; i--)
            {
                Texts[i].Update();

                if (Texts[i].IsDead)
                    Texts.RemoveAt(i);
            }
        }

        public override void PostDrawTiles()
        {
            foreach (var rays in Raycasts)
                rays.DrawDust();

            foreach (var rays in Texts)
                rays.DrawDust();
        }
    }

    public class Raycast
    {
        public Vector2 Start;

        public Vector2 End;

        public int TimeLeft = 4;
        private Color color;

        public bool IsDead => TimeLeft <= 0;
        public Raycast(Vector2 start, Vector2 end, Color color, int TimeLeft = 4)
        {
            this.Start = start.ToWorldCoordinates();
            this.End = end.ToWorldCoordinates();
            this.color = color;
            this.TimeLeft = 4;
        }



        public void Update()
        {
            TimeLeft--;
        }

        public void DrawDust()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.TransformationMatrix);

            Utils.DrawLine(Main.spriteBatch, Start, End, color, color, 2);

            Main.spriteBatch.End();
        }
    }
    public class RaycastText
    {
        public Vector2 WorldAnchor;

        public Color color;
        public string Value;
        public int TimeLeft = 2;
        public bool IsDead => TimeLeft <= 0;
        public RaycastText(string value, Vector2 WorldAnchor, Color color)
        {
            this.Value = value;
            this.WorldAnchor = WorldAnchor;
            this.color = color;
        }
        public void Update()
        {
            TimeLeft--;
        }
        public void DrawDust()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.TransformationMatrix);


            Utils.DrawBorderString(Main.spriteBatch, Value, WorldAnchor - Main.screenPosition, color, 0.4f);

            Main.spriteBatch.End();
        }
    }
}
