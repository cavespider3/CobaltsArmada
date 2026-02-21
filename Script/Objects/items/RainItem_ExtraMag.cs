using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Localization;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_ExtraMag : RainItem
    {
        public override void OnStart(ref Tank tank) {
            tank.Properties.ShellLimit += 1;
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
        }
        public override Color ItemColor => Color.Aqua;


        public override string InternalName => "ExtraAmmo";
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Additional Mag"
        };

    }

    
}
