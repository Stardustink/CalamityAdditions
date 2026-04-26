using CalamityAdditions.Content.BossOverhauls.StormWeaver.Attacks;
using CalamityAdditions.Core.NpcBehaviorOverride;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.StormWeaver;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver
{
    [HasPierceResist]
    [LongDistanceNetSync]
    internal class StormWeaverHeadOverride : NPCBehaviorOverride
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MustAlwaysDraw[NPCType] = true;
        }
        public override string TexturePath => this.GetPath();

        public static int MaxSegmentCount = 30;
        public List<int> Segments;
        public enum State
        {
            Debug,
            Intro,

            WeaveStorm,

        }

        public State CurrentState;

        private StormWeaverAttack currentAttack;



        public override int NPCType => ModContent.NPCType<StormWeaverHead>();
        public override void SetDefaults(NPC NPC)
        {
            NPC.Size = new Vector2(120);
            CalamityGlobalNPC global = NPC.Calamity();
            NPC.defense = 111;
            global.DR = 0;
            global.unbreakableDR = false;
            
            NPC.lifeMax = 2_500_000;
        }
        public override bool OverrideAI(NPC NPC)
        {
            if (Segments is null)
            {
                InitializeSegments(NPC);
            }

            NPC.velocity = new Vector2(MathF.Sin(Main.GameUpdateCount * 0.01f)*20, MathF.Cos(Main.GameUpdateCount * 0.02f)*20);


            UpdateSegments(NPC);

            NPC.rotation = NPC.AngleFrom(Main.npc[Segments[0]].Center) - MathHelper.PiOver2;

            return true;
        }

        public void InitializeSegments(NPC NPC)
        {
            Segments = new List<int>(MaxSegmentCount);

            for (int i = 0; i < MaxSegmentCount; i++)
            {
                int type = ModContent.NPCType<StormWeaverBody>();
                if (i == MaxSegmentCount - 1)
                {
                    type = ModContent.NPCType<StormWeaverTail>();
                }
                int segment = NPC.NewNPC(new EntitySource_Parent(NPC, i.ToString()), (int)NPC.Center.X, (int)NPC.Center.Y, type);
                Segments.Add(segment);
            }



        }

        public float SegmentSpacing => 88f;
        public float SegmentFollowSpeed => 0.78f;
        public void UpdateSegments(NPC NPC)
        {
            Vector2 PreviousCenter = NPC.Center;

            for (int i = 0; i < Segments.Count; i++)
            {
                NPC segment = Main.npc[Segments[i]];
                if (!segment.active)
                    continue;

                Vector2 toSegment = segment.Center - PreviousCenter;

                Vector2 direction = toSegment.LengthSquared() > 0.001f ? Vector2.Normalize(toSegment) : Vector2.UnitY;

                Vector2 desiredCenter = PreviousCenter + direction * SegmentSpacing;

                segment.Center = Vector2.Lerp(segment.Center, desiredCenter, SegmentFollowSpeed);
                segment.rotation = (PreviousCenter - segment.Center).ToRotation();
                PreviousCenter = segment.Center;
            }


        }



        public void SetState(NPC NPC, State newState)
        {
            currentAttack?.Exit(this);

            CurrentState = newState;
            //LocalTimer = 0;
            currentAttack = StormWeaverAttackRegistry.Get(newState);
            currentAttack.Enter(this);
            
            NPC.netUpdate = true;
        }
        internal void MoveToNextState()
        {

        }

        #region DrawCode

        private List<Vector2> GetSegmentCenters(NPC NPC)
        {
            var points = new List<Vector2>(Segments.Count + 1);
            points.Add(NPC.Center);
            for (int i = 1; i < Segments.Count; i++)
            {
                var npc = Main.npc[Segments[i]];
                if (!npc.active)
                    continue;

                points.Add(npc.Hitbox.Center());
            }

            return points;
        }

        private static void SubdividePolyline(List<Vector2> input, List<Vector2> output, int subdivisionsPerSegment)
        {
            output.Clear();

            if (input.Count == 0)
                return;

            for (int i = 0; i < input.Count - 1; i++)
            {
                Vector2 a = input[i];
                Vector2 b = input[i + 1];

                output.Add(a);

                for (int s = 1; s <= subdivisionsPerSegment; s++)
                {
                    float t = s / (float)(subdivisionsPerSegment + 1);
                    output.Add(Vector2.Lerp(a, b, t));
                }
            }

            output.Add(input[^1]);
        }
        private readonly List<Vector2> rawSpinePoints = new();
        private readonly List<Vector2> subdividedSpinePoints = new();


        public BasicEffect spineEffect;
        public List<VertexPositionColorTexture> spineVerts = new();
        public List<short> spineIndices = new();
        private readonly List<Vector2> legPoints = new();
        private readonly List<VertexPositionColorTexture> legVerts = new();
        private readonly List<short> legIndices = new();
        private readonly List<Vector2> subdividedLegPoints = new();
        public delegate float TaperFunction(float t);
        public void DrawSpine(NPC NPC)
        {
            if (Segments.Count < 2)
                return;

            rawSpinePoints.Clear();
            rawSpinePoints.AddRange(GetSegmentCenters(NPC));

            SubdividePolyline(
                rawSpinePoints,
                subdividedSpinePoints,
                subdivisionsPerSegment: 5
            );
            ResampleCatmullRom(rawSpinePoints, subdividedSpinePoints, 10);
            int pivot = FindPivotIndex(subdividedSpinePoints, NPC.Center);

            BuildSpineVerticesPivoted(
                subdividedSpinePoints,
                pivot,
                baseThickness: 48f,
                headTaper: HeadTaper,
                tailTaper: TailTaper,
                color: Color.Crimson,
                vertices: spineVerts,
                indices: spineIndices
            );


            if (spineVerts.Count == 0)
                return;

            var gd = Main.graphics.GraphicsDevice;


            spineEffect ??= new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };

            spineEffect.View = Main.GameViewMatrix.ZoomMatrix;
            spineEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1f, 1f
            );
            gd.RasterizerState = new RasterizerState { FillMode = FillMode.Solid, CullMode = CullMode.None };

            foreach (var pass in spineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    spineVerts.ToArray(),
                    0,
                    spineVerts.Count,
                    spineIndices.ToArray(),
                    0,
                    spineIndices.Count / 3
                );
            }




        }
        private static void ResampleCatmullRom(List<Vector2> controlPoints, List<Vector2> output, int samplesPerSegment)
        {
            output.Clear();

            if (controlPoints.Count < 2)
                return;

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                Vector2 p0 = controlPoints[Math.Max(i - 1, 0)];
                Vector2 p1 = controlPoints[i];
                Vector2 p2 = controlPoints[i + 1];
                Vector2 p3 = controlPoints[Math.Min(i + 2, controlPoints.Count - 1)];

                for (int s = 0; s < samplesPerSegment; s++)
                {
                    float t = s / (float)samplesPerSegment;
                    output.Add(Vector2.CatmullRom(p0, p1, p2, p3, t));
                }
            }

            output.Add(controlPoints[^1]);
        }



        private int FindPivotIndex(List<Vector2> points, Vector2 pivotWorldPos)
        {
            int best = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                float d = Vector2.DistanceSquared(points[i], pivotWorldPos);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }
        private void BuildSpineVerticesPivoted(List<Vector2> points, int pivotIndex, float baseThickness, Func<float, float> headTaper, Func<float, float> tailTaper, Color color, List<VertexPositionColorTexture> vertices, List<short> indices)
        {
            vertices.Clear();
            indices.Clear();

            if (points.Count < 2)
                return;

            float[] dist = new float[points.Count];
            dist[0] = 0f;
            for (int i = 1; i < points.Count; i++)
                dist[i] = dist[i - 1] + Vector2.Distance(points[i - 1], points[i]);

            float pivotDist = dist[pivotIndex];
            float headMax = pivotDist - dist[0];
            float tailMax = dist[^1] - pivotDist;

            if (headMax < 0.001f) headMax = 1f;
            if (tailMax < 0.001f) tailMax = 1f;

            int vertIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 prev = points[Math.Max(i - 1, 0)];
                Vector2 next = points[Math.Min(i + 1, points.Count - 1)];

                Vector2 tangent = next - prev;
                if (tangent.LengthSquared() < 0.001f)
                    tangent = Vector2.UnitX;

                tangent.Normalize();
                Vector2 normal = tangent.RotatedBy(MathHelper.PiOver2);

                float thicknessMul;
                if (i <= pivotIndex)
                {
                    float t = (pivotDist - dist[i]) / headMax;
                    thicknessMul = headTaper(MathHelper.Clamp(t, 0f, 1f));
                }
                else
                {
                    float t = (dist[i] - pivotDist) / tailMax;
                    thicknessMul = tailTaper(MathHelper.Clamp(t, 0f, 1f));
                }

                float thickness = baseThickness * thicknessMul;

                Vector2 offset = normal * (thickness * 0.5f);

                Vector2 left = points[i] - offset;
                Vector2 right = points[i] + offset;

                float v = i / (float)(points.Count - 1);

                vertices.Add(new VertexPositionColorTexture(new Vector3(left - Main.screenPosition, 0f), color, new Vector2(0f, v)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(right - Main.screenPosition, 0f), color, new Vector2(1f, v)));

                if (i < points.Count - 1)
                {
                    indices.Add((short)(vertIndex + 0));
                    indices.Add((short)(vertIndex + 1));
                    indices.Add((short)(vertIndex + 2));

                    indices.Add((short)(vertIndex + 1));
                    indices.Add((short)(vertIndex + 3));
                    indices.Add((short)(vertIndex + 2));
                }

                vertIndex += 2;
            }
        }
        private static float HeadTaper(float t)
        {
            return MathHelper.Lerp(1f, 0, MathF.Pow(t, 0.8f));
        }

        private static float TailTaper(float t)
        {
            return MathHelper.SmoothStep(0, 1, MathF.Pow(0.9f - t, 2.1f) * 1.5f + 0.15f)*2;
        }



        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            if (NPC.IsABestiaryIconDummy)
                return false;
            DrawSpine(NPC);


            var tex = TextureAssets.Npc[NPCType].Value;

            Vector2 Origin = new Vector2(tex.Width / 2.25f, tex.Height / 4);
            Main.EntitySpriteDraw(tex, NPC.Center - screenPos, null, Color.White, NPC.rotation, Origin, 0.9f, 0);


            return base.PreDraw(NPC, spriteBatch, screenPos, drawColor);
        }

      
        #endregion
    }
}
