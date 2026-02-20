using CobaltsArmada.Script.Tanks;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_07_Lavender: CA_ArmadaTank
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Lavender"
        });

        public override string Texture => "assets/textures/tank_lavender";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.Lavender;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.DrawParamsTank.Model = CA_Main.Neo_Mobile;
            AITank.DrawParams.Scaling = Vector3.One * 1.1f;
            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 50;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);

            AITank.Parameters.AggressivenessBias = 0.6f;
            AITank.Parameters.MaxQueuedMovements = 4;

            AITank.Parameters.AwarenessHostileShell = 70;
            AITank.Parameters.AwarenessFriendlyShell = 70;
            AITank.Parameters.AwarenessHostileMine = 14;
            AITank.Parameters.AwarenessFriendlyMine = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 180;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ModSingletonRegistry.GetSingleton<CA_ShatterShell>().Type;
            AITank.Properties.RicochetCount = 0;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.3f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);
            AITank.Parameters.TankAwarenessShoot = 50;
        }

        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(destroy, context);
            if (context is TankHurtContextShell shellcontext)
            {
                if (context.Source is AITank ai && shellcontext.Shell.Owner is not null)
                {
                    if (ai.AiTankType == Type ||
                        ai.AiTankType == ModSingletonRegistry.GetSingleton<CA_05_Poppy>().Type) return;
                }
            
            CA_Main.Fire_AbstractShell_Tank(AITank, 8, context, 1, 0, 5f);
            }
        }
    

    }
}
