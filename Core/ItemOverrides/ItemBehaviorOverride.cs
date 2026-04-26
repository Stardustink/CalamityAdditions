using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAdditions.Core.ItemOverrides
{
    public abstract class ItemBehaviorOverride
    {
        /// <summary>
        /// The item type this override controls.
        /// Return ItemID.CopperShortsword, ModContent.ItemType<SomeItem>(), etc.
        /// </summary>
        public abstract int TargetItemType { get; }

        /// <summary>
        /// Called after vanilla/modded SetDefaults has already happened.
        /// Use this to forcibly replace stats.
        /// </summary>
        public virtual void SetDefaults(Item item) { }

        /// <summary>
        /// Return false to prevent item use entirely.
        /// </summary>
        public virtual bool CanUseItem(Item item, Player player) => true;

        /// <summary>
        /// Called while the item is being used.
        /// </summary>
        public virtual void UseStyle(Item item, Player player, Rectangle heldItemFrame) { }

        /// <summary>
        /// Return true if the item use counted as successful.
        /// </summary>
        public virtual bool? UseItem(Item item, Player player) => null;

        /// <summary>
        /// Return false to suppress vanilla/modded shooting behavior.
        /// </summary>
        public virtual bool Shoot(
            Item item,
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            return true;
        }

        public virtual void HoldItem(Item item, Player player) { }

        public virtual void UpdateInventory(Item item, Player player) { }

        public virtual void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) { }

        public virtual void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) { }

        public virtual void ModifyWeaponCrit(Item item, Player player, ref float crit) { }

        public virtual bool AllowPrefix(Item item, int pre) => true;
    }
}