using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
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
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Fuchsia;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 100.0f * 1.25f;
            var aiParams = AITank.AiParams;
            var properties = AITank.Properties;
            aiParams.MeanderAngle = MathHelper.ToRadians(30);
            aiParams.MeanderFrequency = 20;
            aiParams.TurretMeanderFrequency = 60;
            aiParams.TurretSpeed = 0.2f;
            aiParams.AimOffset = 0.03f;

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
            properties.ShellCooldown = 40;
            // properties.ShellShootCount = 5;
            // properties.ShellSpread =0.4f;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 6f;
            properties.ShellType = ShellID.Rocket;

            properties.RicochetCount = 0;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.InvulnerableToMines = true;


            aiParams.ShootsMinesSmartly = true;

            properties.TreadPitch = -0.26f;
            properties.Acceleration = 0.4f;
            properties.Deceleration = 0.4f;

            properties.MineCooldown = 850;
            properties.MineLimit = 2;
            properties.MineStun = 0;

            aiParams.MinePlacementChance = 0.2f;

            aiParams.BlockWarinessDistance = 80;
            aiParams.BlockReadTime = 10;

            AITank.BaseExpValue = 0.175f;
        }

        public override void Shoot(Shell shell)
        {
            
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
        public override void PreUpdate()
        {
            base.PreUpdate();
            AITank.Properties.MaxSpeed = 0.8f+MathF.Min(1f, AITank.SpecialBehaviors[0].Value/60f/1.5f)*3.8f + AITank.SpecialBehaviors[2].Value;
            AITank.Properties.TreadPitch = MathHelper.Lerp(-0.5f,0.6f, MathHelper.Clamp(AITank.Properties.MaxSpeed*0.2f,0f,1f));

            //   AITank.Properties.TurningSpeed = 0.06f + MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*0.055f;
            //  AITank.Properties.MaximalTurn = MathHelper.ToRadians(30+ MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*45f);

            if (AITank.SpecialBehaviors[0].Value > 0f) { AITank.SpecialBehaviors[0].Value -= RuntimeData.DeltaTime; }
            else { AITank.SpecialBehaviors[0].Value = 0f; }

            AITank.SpecialBehaviors[1].Value -= RuntimeData.DeltaTime;
        }

        public override void DangerDetected(IAITankDanger danger)
        {
            base.DangerDetected(danger);
            if (danger.IsPlayerSourced && danger is Shell && AITank.SpecialBehaviors[1].Value <0.1f)
            {
                ChatSystem.SendMessage("NOPE", Color.Fuchsia);
                AITank.SpecialBehaviors[0].Value = 1.5f*60f;
                AITank.SpecialBehaviors[1].Value = 2.6f * 60f;
                AITank.SpecialBehaviors[2].Value +=0.02f;
            }
        }
    }
}
