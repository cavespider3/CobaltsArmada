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
    public class RainItem_FasterTreads : RainItem
    {
        public override void OnStart(ref Tank tank) {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
            tank.Properties.MaxSpeed += 0.08f * Stacks;
        }
        public override Color ItemColor => Color.Red;

        public override string InternalName => "SpeedUpBasic";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Finer Traction"
        });

    }

    
}
