using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_X3_Peony: ModTank 
    {
      
 
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Peony"
        });

        public override string Texture => "assets/textures/tank_lavender";
    
        public override Color AssociatedColor => Color.PeachPuff;
        
        public override void PostApplyDefaults(AITank tank)
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults(tank);

            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 1.1f;

            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.06f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(3);

            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 1f;
            tank.AiParams.PursuitFrequency = 20;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 0;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            tank.AiParams.MineWarinessRadius_AILaid = 50;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            tank.Properties.ShootStun = 12;
            tank.Properties.ShellCooldown = 500;
            tank.Properties.ShellLimit = 1;
            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShellType = ShellID.Standard;
            tank.Properties.RicochetCount = 0;

            tank.AiParams.ShootChance = 0.8f;

            tank.Properties.Invisible = false;
            tank.Properties.Stationary = false;

            tank.Properties.TreadVolume = 0.1f;
            tank.Properties.TreadPitch = 0.3f;
            tank.Properties.MaxSpeed = 1.5f;

            tank.Properties.Acceleration = 0.1f;

            tank.Properties.MineCooldown = 0;
            tank.Properties.MineLimit = 0;
            tank.Properties.MineStun = 0;

            tank.AiParams.BlockWarinessDistance = 44;
        }
        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
            base.TakeDamage(tank, destroy, context);
            if (!destroy) return;
            new Mine(tank, tank.Position, 200f, 2.1f);
        }
        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
    }
}
