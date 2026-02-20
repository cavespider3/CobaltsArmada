using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Cosmetics;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Framework.Collections;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// Base Class for items used with the Rogue-lite modifier
    /// </summary>
#pragma warning disable CS8618
    public abstract class RainItem : ILoadable, IModdedRainItemContent
    {
        /// <summary>
        /// How powerful the item is.
        /// </summary>
        public enum Rarity
        {
            White,
            Green,
            Red,
            Yellow,
            Blue,
        }
        public virtual void OnStart(ref Tank tank) { }
        public virtual void OnShellRicochet(ref Shell shell) { }
        public virtual void OnTankShoot(ref Tank tank) { }
        public virtual void OnTankDestroy(ref Tank owner, ref Tank victim) { }

        public virtual void OnMinePlaced(ref Tank tank, ref Mine mine) { }
        public virtual void OnMineExplode(ref Mine mine) { }
        public virtual void OnShellDestroy(ref Shell shell) { }

        public virtual void OnTankUpdate(ref Tank tank) { }
        public virtual void OnLoad() { }
        public virtual void OnUnload() { }

        public virtual string InternalName { get; set; }
        public virtual LocalizedString Name { get; internal set; }

        /// <summary>
        /// The basic description for what the item does
        /// </summary>
        public virtual LocalizedString Description { get; internal set; } = new(new()
        {
            [LangCode.English] = ""
        });

        public virtual int Priority { get; set; } = 0;
        public virtual Color ItemColor { get; set; }
        public virtual Rarity Tier { get; set; } = Rarity.White;

        /// <summary>
        /// This item cannot be collected nor used by <seealso cref="PlayerTank"/>.
        /// </summary>
        public virtual bool PreventPlayerTankUse { get; set; } = false;

        /// <summary>
        /// This item cannot be collected nor used by <seealso cref="AITank"/>.
        /// </summary>
        public virtual bool PreventAITankUse { get; set; } = false;

        /// <summary>
        /// Prevents the listed tank ids from gaining this item (they can still pick it up)
        /// </summary>
        public virtual int[] AITankBlacklist { get; set; } = [];

        /// <summary>
        /// Turns <see cref="AITankBlacklist"/> into a Whitelist.
        /// </summary>
        public virtual bool InvertBlacklist { get; set; } = false;

        public int Stacks { get; set; } = 1;
        public int Type { get; set; }
        public TanksMod Mod { get; set; }

        internal static int unloadOffset = 0;
        public sealed class ItemID
        {
            // tanks from the original game
            public const int None = 0;
            public static ReflectionDictionary<ItemID> Collection { get; internal set; } = new(MemberType.Fields);
        }

        public void Load() {
            var name = InternalName!;
            Type = ItemID.Collection.ForcefullyInsert(name);
        }

        public void Unload() {
            ItemID.Collection.TryRemove(Type - unloadOffset);
        }

        internal virtual RainItem Clone() => (RainItem)MemberwiseClone();


    }
}
