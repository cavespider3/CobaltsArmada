using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_09_Carnation: ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Carnation"
        });

        public override string Texture => "assets/textures/tank_carnation";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.Fuchsia;
        public override void PostApplyDefaults(AITank tank)
        {
           
            base.PostApplyDefaults(tank);
            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 1.25f;
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(30);
            aiParams.MeanderFrequency = 20;
            aiParams.TurretMeanderFrequency = 40;
            aiParams.TurretSpeed = 0.2f;
            aiParams.AimOffset = 0.18f;

            aiParams.Inaccuracy = 0.9f;

            //also maximum agro lmfao
            aiParams.PursuitLevel = 1f;
            aiParams.PursuitFrequency = 50;

            aiParams.ProjectileWarinessRadius_PlayerShot = 70;
            aiParams.ProjectileWarinessRadius_AIShot = 70;
            //they're immune to mines
            aiParams.MineWarinessRadius_PlayerLaid = 0;
            aiParams.MineWarinessRadius_AILaid = 0;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(34);

            properties.ShootStun = 1;
            properties.ShellCooldown = 2;
            properties.ShellShootCount = 5;
            properties.ShellSpread =0.4f;
            properties.ShellLimit = 20;
            properties.ShellSpeed = 6.5f;
            properties.ShellType = ShellID.Rocket;

            properties.RicochetCount = 0;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.VulnerableToMines = false;

         
            aiParams.ShootsMinesSmartly = true;

            properties.TreadPitch = -0.26f;
            properties.MaxSpeed = 2.6f;
            properties.Acceleration = 0.4f;
            properties.Deceleration = 0.4f;

            properties.MineCooldown = 850;
            properties.MineLimit = 2;
            properties.MineStun = 0;

            aiParams.MinePlacementChance = 0.2f;

            aiParams.BlockWarinessDistance = 80;
            aiParams.BlockReadTime = 10;

            tank.BaseExpValue = 0.175f;
        }

        public override void Shoot(AITank tank, ref Shell shell)
        {
            
            base.Shoot(tank, ref shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
    }
}
