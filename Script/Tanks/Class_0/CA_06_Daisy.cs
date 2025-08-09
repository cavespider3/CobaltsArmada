using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    /// <summary>
    /// A stationary turret tank that fires well calculated, multi bounce frag shells.
    /// </summary>
    public class CA_06_Daisy: CA_ArmadaTank
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Daisy"
        });

        public override string Texture => "assets/textures/tank_daisy";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.Orange;
        public override void PostApplyDefaults()
        {
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Stationary;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
     
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);

 

            AITank.Parameters.AggressivenessBias  = 0.9f;
   

            AITank.Parameters.AwarenessHostileShell = 120;
            AITank.Parameters.AwarenessFriendlyShell = 100;
            AITank.Parameters.AwarenessHostileMine = 14;
            AITank.Parameters.AwarenessFriendlyMine = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 300;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_ShatterBouncer>().Type;
            AITank.Properties.RicochetCount = 2;
            AITank.Parameters.SmartRicochets =true;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = true;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.TankAwarenessShoot = 50;

            base.PostApplyDefaults();
        }

        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (context.Source == AITank) return;
            base.TakeDamage(destroy, context);
            if (!destroy) return;
            new Mine(AITank, AITank.Position, 50f, 1f);
        }

        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }


    }
}
