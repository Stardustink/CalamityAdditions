
using BreadLibrary.Core.Graphics.Pixelation;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome.ProfanedConstructNPC
{
    [PoolCapacity(1500)]
    internal class ProfanedConstruct_Particle : BaseParticle<ProfanedConstruct_Particle>
    {
        // static means that there's only ONE instance of this variable shared across all instances of the class.
        // This is perfect for textures, since we only need to load it once and can share it across all particles.
        public static Asset<Texture2D> Texture;

        public Dictionary<int, Asset<Texture2D>> textures;
        /// <summary>
        /// runs once.
        /// </summary>
        public override void SetStaticDefaults()
        {
            //because we've named the particle class the same as the texture, we can just use GetPath to get the path to the texture asset.
            string Path = this.GetPath();
            Texture = ModContent.Request<Texture2D>(Path);
        }
        public override bool DrawsPixelated => true;

        public override PixelLayer PixelLayer => PixelLayer.AboveTiles;

        /// <summary>
        /// the world position of this particle.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// the velocity of this particle, in pixels per tick. 
        /// This is added to the position every tick in the Update method.
        /// </summary>
        public Vector2 Velocity;

        public float Rotation;

        /// <summary>
        /// The Max LifeTime of the particle.
        /// </summary>
        public float MaxTimeLeft;

        /// <summary>
        /// A timer that counts down from MaxTimeLeft to 0. When it reaches 0, the particle should be removed.
        /// </summary>
        public int TimeLeft;

        /// <summary>
        /// An interpolant, made of the ratio of TimeLeft to MaxTimeLeft.
        /// It goes from 0 to 1 as the particle's life progresses. 
        /// </summary>
        public float Progress;
        /// <summary>
        /// this method isn't technically necessary to ready the particle, 
        /// as you could do this initialization when you're instantiating the particle 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="MaxTimeLeft"></param>
        public void Prepare(Vector2 Position, Vector2 Velocity, float MaxTimeLeft = 120f)
        {
            this.Position = Position;
            this.MaxTimeLeft = MaxTimeLeft;
            this.TimeLeft = (int)MaxTimeLeft;
            this.Velocity = Velocity;

            Progress = 1f - (TimeLeft / MaxTimeLeft);
            Rotation = Main.rand.NextFloat(0, MathHelper.Pi);

        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            Progress = 1f - (TimeLeft / MaxTimeLeft);

            Position += Collision.TileCollision(Position, Velocity, 4, 4);
            Velocity *= 0.9f; // Apply some drag to slow down the particle over time.

            //roughly equivelant to 
            //int oldValue = TimeLeft;
            //TimeLeft = TimeLeft - 1;
            //if (oldValue < 0)
            //ShouldBeRemovedFromRenderer = true;
            // the reason we can call -- inside the if statement,
            // is because the if statement is merely evaluating an expression,
            // which just so happens to be subtracting.
            if (TimeLeft-- <= 0)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        private Color ColorFunction(float Progress)
        {
            return Color.Lerp(Color.Firebrick with {  R = 220}, Color.DarkOrange, Progress);
        }
        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            var Tex = Texture.Value;

            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = Tex.Size() / 2;

            float rot = Rotation+Velocity.ToRotation();

            spritebatch.End();
            spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null,
                    Matrix.CreateScale(1f / PixelationSystem.PixelScale, 1f / PixelationSystem.PixelScale, 1f));


            float scale = CalamityUtils.PolyInOutEasing(Progress, 1);
            var Dissolve = GameShaders.Misc["CalamityAdditions:DirectionalDissolve"];
            Dissolve.SetShaderTexture(Texture, 0);

            Dissolve.SetShaderTexture(Assets.Textures.Noise.T_VoronoiNoise001.Asset, 1);
            Dissolve.Shader.Parameters["dissolveProgress"].SetValue(Progress);
            Dissolve.Shader.Parameters["edgeWidth"].SetValue(0.4f);

            Dissolve.Shader.Parameters["opacity"].SetValue(1f);

            Dissolve.Shader.Parameters["edgeColor"].SetValue(ColorFunction(Progress).ToVector4());
            Dissolve.Shader.Parameters["dissolveDirection"].SetValue((rot+ MathHelper.PiOver2).ToRotationVector2());
            Dissolve.Shader.Parameters["directionalStrength"].SetValue(0.2f);


            Dissolve.Shader.Parameters["noiseStrength"].SetValue(1-Progress);
            Dissolve.Shader.Parameters["gradientStrength"].SetValue(1-Progress);

            Dissolve.Apply();
            Main.EntitySpriteDraw(Tex, drawPos, null, ColorFunction(Progress * 0.5f), rot, origin, scale * 0.151f, 0);
            


           Utilities.ResetToDefault(spritebatch);

        }

    }
}
