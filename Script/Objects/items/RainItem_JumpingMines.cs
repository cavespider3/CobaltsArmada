using CobaltsArmada.Script.Objects.hazards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using TanksRebirth.Internals;
using TanksRebirth.Localization;
using static CobaltsArmada.Script.Tanks.Class_T.DroneParameters;

namespace CobaltsArmada.Script.Objects.items
{
    /// <summary>
    /// Don't.
    /// </summary>
    public class RainItem_JumpingMine : RainItem
    {
        public float JumpTime;
        public bool NatrualImmunity = false;
        public override void OnTankUpdate(ref Tank tank) {
            if (NatrualImmunity) return;
            tank.Properties.InvulnerableToMines = JumpTime > 0;
            JumpTime -= RuntimeData.DeltaTime / 60f;
        }

        public override void OnStart(ref Tank tank)
        {
            TankGame.IngameConsole.Log("Activating the power of " + Name.GetLocalizedString(LangCode.English), ItemColor);
            NatrualImmunity = tank.Properties.InvulnerableToMines;
        }


        public override void OnMinePlaced(ref Tank tank, ref Mine mine)
        {
            JumpTime = 5f;
            mine.ExplosionRadius = 0.0f;
            mine.DetonateTime = 1f;
            tank.Properties.MineCooldown = 300u;
           
        }

        //public override void OnTankUpdate(ref Tank tank)
        //{
        //    Tank tank2 = tank;
        //    foreach(var mine in Mine.AllMines.Where(x => x is not null && !x.Detonated && x.Owner is not null && x.Owner == tank2))
        //    {
                

        //    }

        //}

        public override void OnMineExplode(ref Mine mine)
        {
            if (mine.Owner is AITank ai)
            {
                CA_Main.FakeAITankGravity[ai.AITankId] = MathF.Log(Stacks * 1.3f) * 2.1f + 0.9f;

            }
            else if (mine.Owner is PlayerTank player)
            {
                CA_Main.FakePlayerTankGravity[Math.Max(0, player.PlayerId)] = MathF.Log(Stacks * 1.3f) * 2.1f + 0.9f;
            }

        }
        public override int Priority => 5;

        public override Rarity Tier => Rarity.Blue;

        public override Color ItemColor => Color.DarkMagenta;

        public override string InternalName => "rocketjumping";
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Sticky Jumper"
        });

    }

    
}
