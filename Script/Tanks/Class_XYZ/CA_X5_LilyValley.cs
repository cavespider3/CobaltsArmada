using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using TanksRebirth.GameContent.Systems.TankSystem;

namespace CobaltsArmada
{
    /// <summary>A tank that gives the nightshade buff to other tanks when it dies</summary>
    public class CA_X5_LilyValley: CA_ArmadaTank
    {
      
   
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Lily"
        });
        public override int Songs => 3;

        public override string Texture => "assets/textures/tank_medicine";

        public override Color AssociatedColor => Color.DarkMagenta;
        public override void PostApplyDefaults()
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults();

            AITank.DrawParamsTank.Model = CA_Main.Neo_Mobile;
            AITank.DrawParams.Scaling = Vector3.One;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);


            AITank.Parameters.AggressivenessBias = 1f;

            AITank.Parameters.AwarenessHostileShell = 0;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 500;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;



            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.MaxQueuedMovements = 4;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(destroy, context);
            if (!destroy) return;
            CA_Main.SpawnPoisonCloud(AITank,AITank.Position3D,180f);
        }
        public override void Shoot(Shell shell)
        {
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
