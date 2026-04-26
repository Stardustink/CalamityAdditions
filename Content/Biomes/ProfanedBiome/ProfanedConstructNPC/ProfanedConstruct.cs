


using BreadLibrary.Core.Graphics.Pixelation;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityAdditions.Content.Biomes.ProfanedBiome.ProfanedConstructNPC
{
    public class ProfanedConstruct : ModNPC
    {
        #region Values
        /// <summary>
        /// Stores the GlowTex upon loading.
        /// </summary>
        /// <remarks>Apparently, it's more efficient to store the Texture2D within an <see cref="Asset{T}"/>. 
        /// Since this is static, we only should set it once, as static means that every instance of this class, will share that same value.</remarks>
        public static Asset<Texture2D> GlowTex;

        public static Asset<Texture2D> GlowBlurTex;

        /// <summary>
        /// an internal timer that ticks up continuously.
        /// </summary>
        /// <remarks> make sure to reset it to -1 every time state changes.</remarks>
        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        /// <summary>
        /// also an ever-incrementing timer, but we do not reset this one.
        /// used to manage visual effects on the npc.
        /// </summary>
        public int CosmeticTime
        {
            get => (int)NPC.localAI[3];
            set => NPC.localAI[3] = value;
        }

        /// <summary>
        /// Represents the direction the NPC wants to move in this tick.
        /// </summary>
        public Vector2 DirectionalIntent;

        /// <summary>
        /// A temporary variable to store a desired location.
        /// </summary>
        public Vector2 TargetPos;
        /// <summary>
        /// A state that contains values for this profaned Construct.
        /// </summary>
        public enum State
        {
            Debug,

            Patrol,


        }
        /// <summary>
        /// Represents an instance of the State enum.
        /// </summary>
        /// <remarks>We can hijack <see cref="NPC.ai"/> in order to automatically netsync this value.
        /// that being said, you should still set <see cref="NPC.netUpdate"/> just to make sure that everything remains synced.</remarks>
        public State CurrentState
        {
            get => (State)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }

        public bool ShouldBeLevitating = true;

        #endregion

        #region Spawn/SetDefaults/Load

        public override void Load()
        {
            //automatically load the ProfanedConstruct_Glow.png . 
            //so long as the Glow Texture is placed in the same file, it will load into glowtex.
            ///this is because <see cref="ModContent.Request{T}(string, AssetRequestMode)"/> looks in the filepath for the value, and <see cref="Utilities.GetPath(object)"/> converts it into a proper filepath.
            string Path = this.GetPath();
            GlowTex = ModContent.Request<Texture2D>(Path + "_Glow");
            GlowBlurTex = ModContent.Request<Texture2D>(Path + "_Glow_Blur");
        }
        public override void SetStaticDefaults()
        {
            //Sets the FrameCount of the npc; this is less important since we're manually drawing it, but Its important to still set this.
            Main.npcFrameCount[Type] = 27;

            //forces this NPC to always draw, as it says on the tin.
            NPCID.Sets.MustAlwaysDraw[Type] = true;
        }
        public override void OnSpawn(IEntitySource source)
        {

        }

        public override void SetDefaults()
        {
            // by using the _, it still remains as 90000 (90k) but in a more easily sight-readable form.
            NPC.lifeMax = 90_000;
            ///since NPC.width and npc.height are used to create a <see cref="Rectangle"/>,
            /// we can just pass in a <see cref="Vector2"/> for the size in this parameter, and skip the tedium of writing NPC.width = 40 and NPC.Height = 40.
            NPC.Size = new(40, 80);

            NPC.noGravity = true;
            NPC.GravityMultiplier *= 0.2f;
            NPC.Profaned().IsProfaned = true;
        }
        #endregion

        #region AI
        /// <summary>
        /// Runs Directly before AI.
        /// </summary>
        /// <returns>False if you want the NPC's AI to not run, true elsewise. </returns>
        public override bool PreAI()
        {
            CosmeticTime++;
            DirectionalIntent = NPC.velocity;
            return base.PreAI();
        }

        public override void AI()
        {
            CurrentState = State.Patrol;
            StateMachine();

            
            NPC.direction = NPC.velocity.X.DirectionalSign();
            NPC.spriteDirection = NPC.direction;
            //Call this last, since updating the timer should be the last action the NPC takes.
            Time++;
        }

        /// <summary>
        /// Always will run, even if Pre-AI returns null. 
        /// useful for Visual effects that need to be made/updated, even if the npc might be lobotomized for one reason or another.
        /// </summary>
        public override void PostAI()
        {
            Levitate();
            ProfanedConstruct_Particle particle = ProfanedConstruct_Particle.Pool.RequestParticle();
            particle.Prepare(NPC.Bottom + Vector2.UnitY * -20, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(20 + NPC.velocity.LengthSquared())) * 5, 40);
            ParticleEngine.Particles.Add(particle);
            if (Main.LocalPlayer.controlUseItem)
                TargetPos = Main.MouseWorld;
        }
        #endregion

        #region Collision and stuff

        public override void HitEffect(NPC.HitInfo hit)
        {
            ProfanedConstruct_Particle particle = ProfanedConstruct_Particle.Pool.RequestParticle();
            particle.Prepare(NPC.Center + Vector2.UnitY * -20, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(20 + NPC.velocity.LengthSquared())) * 4, 40);
            ParticleEngine.Particles.Add(particle);

        }
        #endregion

        #region HelperMethods

        /// <summary>
        /// while its called a statemachine, each state actually controls the flow. this basically just acts as an easy way for the NPC to call specific methods,
        /// without having to 
        /// </summary>
        private void StateMachine()
        {
            switch (CurrentState)
            {
                case State.Debug:
                    ManageDebug();
                    break;

                case State.Patrol:
                    ManagePatrol();
                    break;
            }
        }

        /// <summary>
        /// so long as the curent state is debug, this method will be called.
        /// </summary>
        private void ManageDebug()
        {
            float thing = NPC.DistanceSQ(TargetPos) * 0.0005f;
            NPC.velocity.X = NPC.DirectionTo(TargetPos).X * MathHelper.SmoothStep(0, 4, thing);

            CurrentState = State.Patrol;
        }

        private void ManagePatrol()
        {
            TargetPos = NPC.Center + Vector2.UnitX * 10;

            float thing = NPC.DistanceSQ(TargetPos) * 0.0005f;

            Player Player;

            Player = Main.player[NPC.FindClosestPlayer()];

            if (Player != null)
            {
                if (Player.Distance(NPC.Center) < 400)
                {
                    TargetPos = Player.Center;
                }
            }

            NPC.velocity.X = NPC.DirectionTo(TargetPos).X * MathHelper.SmoothStep(1, 4, thing);
        }


        private void Levitate()
        {
            if (!ShouldBeLevitating)
                return;

            //Total distance in WorldCoordinates that this NPC should check down.
            float maxCheck = 170f +
                (NPC.Center.Y - TargetPos.Y);
            ;
            int hitCount = 0;
            float accumulatedHeight = 0f;

            bool isClimbingWall = false;
            //Cast 3 rays, so that it can levitate over gaps that might otherwise cause it to fall through
            const float MaxSensors = 3;
            //adjusting this will allow you to increase the spread of the sensors.
            //AKA: Making it a bit more accurate when it comes to rough or uneven terrain.
            const float MaxAngle = MathHelper.PiOver2;
            for (int i = 0; i < MaxSensors; i++)
            {
                float interp = i / MaxSensors;
                Vector2 start = NPC.Top;

                //form a neat cone of raycasts.
                Vector2 end = start + Vector2.UnitY.RotatedBy(MaxAngle * interp - MaxAngle / 1.5f + DirectionalIntent.ToRotation() * 0.2f) * maxCheck;

                Point? hit = Utilities.RaycastTo(start, end, ShouldCountWater: true, debug: false);

                if (!hit.HasValue)
                    continue;

                float height = hit.Value.ToWorldCoordinates().Y - NPC.Center.Y;

                accumulatedHeight += height;
                hitCount++;
            }

            
            for (int i = 0; i < MaxSensors; i++)
            {
                float interp = i / MaxSensors;
                //If the NPC has a meaningful movement intent (DirectionalIntent isn't near-zero), attempt a short raycast in that direction.
                if (DirectionalIntent.LengthSquared() > 0.001f)
                {
                    Vector2 dir = DirectionalIntent;
                    //Normalize DirectionalIntent to get direction.
                    Vector2 dirNorm = dir.LengthSquared() > 0.0001f ? Vector2.Normalize(dir) : Vector2.Zero;

                    if (dirNorm != Vector2.Zero)
                    {
                        //distance to probe for walls
                        float checkDistance = NPC.width;
                        Vector2 start = NPC.Bottom + new Vector2(NPC.width * NPC.direction, 0);
                        Vector2 end = start + Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * interp) * NPC.direction * checkDistance;

                        // Raycast for a wall in the movement direction. 
                        Point? wallHit = Utilities.RaycastTo(start, end, ShouldCountWater: false, debug: false);

                        if (wallHit.HasValue)
                        {

                            NPC.velocity.X *= 0.5f;
                            const float climbImpulse = -0.5f; 
                            NPC.noGravity = true;

                            isClimbingWall = true;
                            NPC.position.Y += climbImpulse;
                            continue;

                        }
                        else continue;
                    }
                    else
                        continue;
                }
            }


         

            if (!isClimbingWall)
                if (hitCount < MaxSensors - 1)
                {
                    NPC.noGravity = false;
                    return;
                }

            float actualHeight = accumulatedHeight / hitCount;
            float tolerance = 1.5f;


            //offsetting the desired height by CosmeticTime allows us to create a smooth, up and down motion.
            float BobIntensity = NPC.velocity.LengthSquared();
            BobIntensity = MathHelper.SmoothStep(1, 0, BobIntensity);
            //right now, this is just adjusted by the velocity of the npc. this works just fine, except it looks a bit odd and is a bit janky.
            float desiredHeight =
              (NPC.Center.Y - TargetPos.Y);


                //100f + MathF.Sin(CosmeticTime * 0.05f + NPC.whoAmI) * 10 * BobIntensity;

            float error = desiredHeight - actualHeight;

            if (MathF.Abs(error) < tolerance)
            {
                NPC.noGravity = true;
                return;
            }

            float correctionStrength = 0.08f;

            float moveAmount = error * correctionStrength;
            moveAmount = MathHelper.Clamp(moveAmount, -2f, 2f*NPC.GravityMultiplier.Value);

            if (moveAmount == float.NaN)
                moveAmount = 0;

            NPC.position.Y -= moveAmount;
            NPC.noGravity = true;
            NPC.velocity.Y *= 0f;
        }

        #endregion

        #region Animation / DrawCode
        //Frame Values
        internal const int _FloatStart = 0;
        internal const int _FloatEnd = 4;

        internal const int _StabStart = 5;
        internal const int _StabEnd = 15;

        internal const int _ThrowStart = 16;
        internal const int _ThrowEnd = 27;

        public override void FindFrame(int frameHeight)
        {
            if (CurrentState == State.Debug)
            {
                if (NPC.frameCounter > 8)
                {
                    NPC.frame.Y++;
                    if (NPC.frame.Y > _FloatEnd)
                        NPC.frame.Y = 0;
                    NPC.frameCounter = -1;
                }

                NPC.frameCounter++;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            if (NPC.IsABestiaryIconDummy)
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            if (TargetPos != Vector2.Zero)
            {
                Utils.DrawLine(spriteBatch, NPC.Center, TargetPos, Color.White);
            }


            //var is like a wildcard. it can be any variable, but it has to be instantiated.
            //AKA: you cannot do 'var Apple'; and then assign 'Apple' a value later.
            // in this case, we're using it because im far too lazy to type "Texture2D".

            ///additionally, TextureAssets can be used to Quickly grab the <see cref="Asset{T}"/> of an object.
            // yes, this can work for stuff like projectiles, or even from another npc (so long as you input it's type first).
            // Noteably, if you are attempting to pull a vanilla texture, you need to ensure that it's loaded first.
            var Tex = TextureAssets.Npc[Type].Value;


            //Instantiates a new Rectangle. this is different from the normal NPC.frame, since we're manually setting it up.
            //this does mean we don't  need to use frameHeight, and we can just set the NPC.frame.Y directly.
            Rectangle Frame = Tex.Frame(1, Main.npcFrameCount[Type] + 1, 0, NPC.frame.Y);
            //if this isn't satisfactory, you can just use Rectangle Frame = NPC.frame, which will revert you back to using NPC.FindFrame.


            Vector2 drawPos = NPC.Center - Main.screenPosition;
            // by subtracting 3 from this origin, we canshift the entire animation over without having to adjust drawpos.
            Vector2 Origin = Frame.Size() / 2 + new Vector2(3 * -NPC.spriteDirection, 0);

            float Scale = NPC.scale;
            float Rot = NPC.rotation;

            SpriteEffects Flip = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Draw the main Texture.
            Main.EntitySpriteDraw(Tex, drawPos, Frame, drawColor, Rot, Origin, Scale, Flip);

            //Now, Instantiate the GlowTex.
            var Glow = GlowTex.Value;

            Main.EntitySpriteDraw(Glow, drawPos, Frame, Color.White, Rot, Origin, Scale, Flip);

            var GlowBlur = GlowBlurTex.Value;

            spriteBatch.UseBlendState(BlendState.Additive);
            Rectangle BlurFrame = GlowBlur.Frame(1, Main.npcFrameCount[Type] + 1, 0, NPC.frame.Y);
            Vector2 BlurOrigin = BlurFrame.Size() / 2 + new Vector2(1 * -NPC.spriteDirection, -2);

            for(int i = 0; i< 4; i++)
            {

                Main.EntitySpriteDraw(GlowBlur, drawPos + Vector2.UnitX.RotatedBy(i/4f * MathHelper.TwoPi + NPC.whoAmI + Main.GlobalTimeWrappedHourly), BlurFrame, Color.White*0.5f, Rot, BlurOrigin, Scale * 1.1f, Flip);
            }
            spriteBatch.UseBlendState(BlendState.AlphaBlend);
            //don't draw automatically, because we will need to do things behind this npc.


            Utils.DrawBorderString(spriteBatch, CurrentState.ToString(), NPC.Center - Main.screenPosition, Color.White);
            return false;
        }
        #endregion
    }
}
