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
    public class RainItem_SpareMine : RainItem
    {
        public override void OnStart(ref Tank tank)
        {
            tank.Properties.MineLimit += 1;
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
        }
        public override Color ItemColor => Color.LightGoldenrodYellow;

        public override string InternalName => "ExtraMine";

        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Spare Mine"
        };

    }


}
