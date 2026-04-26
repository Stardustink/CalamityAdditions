using CalamityAdditions.Common.IK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Content.BossOverhauls.StormWeaver
{ 
    public class StormWeaverLeg : ModType
    {
        public Vector2 Anchor;

        public IKSkeleton LegSkeleton;
        public bool IsLeftLeg;
        public bool IsRightLeg => !IsLeftLeg;

        public StormWeaverLeg(Vector2 anchor, params IKSkeleton.JointSetup[] joints)
        {
            Anchor = anchor;
            LegSkeleton = new IKSkeleton(joints);

        }


        public void Update(Vector2 Anchor, Vector2 target)
        {
            this.Anchor = Anchor;
            LegSkeleton.Solve(this.Anchor, target);
        }



        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            for(int i = 0; i< this.LegSkeleton.JointCount -1; i++)
            {
                float scale = 4;
                Utils.DrawLine(spriteBatch, LegSkeleton.GetJointPosition(i), LegSkeleton.GetJointPosition(i + 1), drawColor, drawColor, scale);
            }
        }

        protected override void Register()
        {

        }
    }
}
