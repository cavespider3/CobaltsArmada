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
    public class RainItem_Micro : RainItem
    {


        public override void OnStart(ref Tank tank)
        { 
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
            tank.DrawParams.Scaling -= Vector3.One * 0.05f * Math.Clamp(Stacks,0,18);
            tank.Properties.MaxSpeed += 0.05f * Math.Clamp(Stacks,0,18);
            tank.Properties.TurningSpeed += 0.075f * Math.Clamp(Stacks,0,18);
            tank.Properties.TreadPitch += 0.05f * Math.Clamp(Stacks,0,18);
        }

        public override Rarity Tier => Rarity.Green;

        public override int Priority => -1;
        public override Color ItemColor => Color.LightBlue;

        public override string InternalName => "minimush";

        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Compact Plating"
        });

    }


}
