using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;

namespace CobaltsArmada.Script.Objects.items
{
    public class RainItem_ATG : RainItem
    {
      
        public override void OnTankDestroy(ref Tank tank, ref Tank victim)
        {
         if (Client.ClientRandom.Next(0, 10) <= Stacks - 1)
            {
                Shell.HomingProperties homing = new() { HeatSeeks = true, Cooldown = 0, Power = tank.Properties.ShellSpeed * 0.1f, Speed = tank.Properties.ShellSpeed * 2f, Radius = 1500f };
                Vector2 sending = MathUtils.RotatedBy(Vector2.UnitY, MathF.PI + -tank.TurretRotation + MathHelper.ToRadians(Client.ClientRandom.NextFloat(-80, 80)));
                new Shell(sending * 25f + tank.Position, sending * tank.Properties.ShellSpeed, ShellID.Rocket, tank, 0, homing, true);
            }
        }

        public override void OnTankShoot(ref Tank tank)
        {
if (Client.ClientRandom.Next(0, 20) <= Stacks - 1)
            {
                Shell.HomingProperties homing = new() { HeatSeeks = true, Cooldown = 0, Power = tank.Properties.ShellSpeed * 0.1f, Speed = tank.Properties.ShellSpeed * 2f, Radius = 1500f };
                Vector2 sending = MathUtils.RotatedBy(Vector2.UnitY, MathF.PI + -tank.TurretRotation + MathHelper.ToRadians(Client.ClientRandom.NextFloat(-80, 80)));
                new Shell(sending * 25f + tank.Position, sending * tank.Properties.ShellSpeed, ShellID.Rocket, tank, 0, homing, true);
            }
        }

        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name[LangCode.English], ItemColor);
        }

        public override Rarity Tier => Rarity.Green;

        public override Color ItemColor => Color.LimeGreen;

        public override string InternalName => "atgmissle";
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Lotus Rockets"
        };

    }


}
