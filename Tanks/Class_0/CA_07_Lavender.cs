using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_07_Lavender: ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Lavender"
        });

        public override string Texture => "assets/textures/tank_lavender";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.Lavender;
        public override void PostApplyDefaults(AITank tank)
        {
            base.PostApplyDefaults(tank);
            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 1.1f;
            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.06f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(3);

            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 0.9f;
            tank.AiParams.PursuitFrequency = 500;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 70;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 70;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 14;
            tank.AiParams.MineWarinessRadius_AILaid = 70;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            tank.Properties.ShootStun = 12;
            tank.Properties.ShellCooldown = 180;
            tank.Properties.ShellLimit = 1;
            tank.Properties.ShellSpeed = 5f;
            tank.Properties.ShellType = ModContent.GetSingleton<CA_ShatterShell>().Type;
            tank.Properties.RicochetCount = 0;

            tank.Properties.Invisible = false;
            tank.Properties.Stationary = false;

            tank.Properties.TreadVolume = 0.1f;
            tank.Properties.TreadPitch = 0.3f;
            tank.Properties.MaxSpeed = 1.3f;

            tank.Properties.Acceleration = 0.1f;

            tank.Properties.MineCooldown = 0;
            tank.Properties.MineLimit = 0;
            tank.Properties.MineStun = 0;

            tank.AiParams.BlockWarinessDistance = 44;
        }

        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(tank, destroy, context);
            if (context is not TankHurtContextShell) return;
            if (!context.IsPlayer && ((TankHurtContextShell)context).Shell.Owner is not null)
            {
                AITank showner = (AITank)((TankHurtContextShell)context).Shell.Owner;
                if (showner.AiTankType == Type ||
                    showner.AiTankType == ModContent.GetSingleton<CA_05_Poppy>().Type) return;
            }
            CA_Main.Fire_AbstractShell_Tank(tank, 8, context, 1, 0, 5f);
        }

    }
}
