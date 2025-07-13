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

            aiParams.Inaccuracy = 0.2f;
            properties.TurningSpeed = 0.1f;
            properties.MaximalTurn = MathHelper.PiOver4;
            //also maximum agro lmfao
            aiParams.PursuitLevel = 0.7f;
            aiParams.PursuitFrequency = 180;

            aiParams.ProjectileWarinessRadius_PlayerShot = 140;
            aiParams.ProjectileWarinessRadius_AIShot = 160;
            //they're immune to mines
            aiParams.MineWarinessRadius_PlayerLaid = 160;
            aiParams.MineWarinessRadius_AILaid = 160;

            properties.ShootStun = 1;
            properties.ShellCooldown = 40;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 6f;
            properties.ShellType = ShellID.TrailedRocket;

            properties.RicochetCount = 2;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.InvulnerableToMines = true;

            properties.TreadPitch = -0.26f;
            properties.Acceleration = 0.4f;
            properties.Deceleration = 0.9f;

            properties.MineCooldown = 850;
            properties.MineLimit = 1;
            properties.MineStun = 0;

            aiParams.MinePlacementChance = 0.1f;

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
            AITank.Properties.MaxSpeed = 2f+MathF.Min(1f, AITank.SpecialBehaviors[0].Value/60f/2f)*2.6f;
            AITank.Properties.TreadPitch = MathHelper.Lerp(-0.8f,0.9f, MathHelper.Clamp((AITank.Properties.MaxSpeed-2f)/2.6f,0f,1f));

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
                AITank.SpecialBehaviors[0].Value = 2.06f*60f;
                AITank.SpecialBehaviors[1].Value = 2.6f * 60f;
            }
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (context.Source == AITank) return;
            base.TakeDamage(destroy, context);
        }
    }
}
