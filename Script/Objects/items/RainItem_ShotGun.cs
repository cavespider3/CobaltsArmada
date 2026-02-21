using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_ShotGun : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
            tank.Properties.ShellShootCount += 1 * Stacks;
            tank.Properties.ShellSpread =  (tank.Properties.ShellShootCount / 12 ) - 0.01f;
            tank.Properties.ShellLimit += 2 * Stacks;
        }
        public override Rarity Tier => Rarity.Green;
        public override Color ItemColor => Color.Orange;

        public override string InternalName => "SpreadShot";
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Multi-Shot"
        };

    }


}
