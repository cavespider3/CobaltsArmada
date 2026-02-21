using CobaltsArmada.Script.Tanks.Class_T;
using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using TanksRebirth.GameContent.Systems.TankSystem;

namespace CobaltsArmada
{
    /// <summary>Carnations are the big bois. Immune to explosions, move fast, dodge well, and come with a drone capable of calling in back up.
    /// 
    /// </summary>
    public class CA_09_Carnation: CA_ArmadaTank
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Carnation"
        };
        public override LocalizedString Description => new()
        {
            [LangCode.English] = "A powerful tank that speeds up to avoid danger, fires fast rockets that ricochet twice, and is immune to mines. Also comes with a drone."
        };

        public float FearCooldown = 0f;
        public float FearMult = 0f;


        public override string Texture => "assets/textures/tank_carnation";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Fuchsia;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            
            AITank.DrawParams.Scaling = Vector3.One * 1.25f;
            var Parameters = AITank.Parameters;
            var properties = AITank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            Parameters.RandomTimerMinMove = 20;
            Parameters.RandomTimerMaxMove = 60;
            Parameters.TurretMovementTimer = 60;
            Parameters.TurretSpeed = 0.2f;
            Parameters.AimOffset = 0.03f;
            AITank.Parameters.MaxQueuedMovements = 4;
            properties.TurningSpeed = 0.1f;
            properties.MaximalTurn = MathHelper.PiOver4;
            //also maximum agro lmfao
            Parameters.AggressivenessBias = 0.7f;

            Parameters.AwarenessHostileShell = 140;
            Parameters.AwarenessFriendlyShell = 160;
            //they're immune to mines
            Parameters.AwarenessHostileMine = 160;
            Parameters.AwarenessFriendlyMine = 160;

            properties.ShootStun = 1;
            properties.ShellCooldown = 40;
            properties.ShellLimit = 2;
            properties.ShellSpeed = 6f;
            properties.ShellType = ShellID.TrailedRocket;

            properties.RicochetCount = 2;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.Resistance = ResistanceFlags.All;

            properties.TreadPitch = -0.26f;
            properties.Acceleration = 0.4f;
            properties.Deceleration = 0.9f;

            properties.MineCooldown = 850;
            properties.MineLimit = 1;
            properties.MineStun = 0;

            Parameters.ChanceMineLay = 0.1f;

            Parameters.ObstacleAwarenessMovement = 80;
            Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5);
            Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);


        }

        public override void Shoot(Shell shell)
        {
            
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
      
        public override void PreUpdate()
        {
            base.PreUpdate();
           
            AITank.Properties.MaxSpeed = 2f + MathF.Min(1f, FearMult / 60f / 2f) * 2.6f;
            AITank.Properties.TreadPitch = MathHelper.Lerp(-0.8f, 0.9f, MathHelper.Clamp((AITank.Properties.MaxSpeed - 2f) / 2.6f, 0f, 1f));

            //   AITank.Properties.TurningSpeed = 0.06f + MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*0.055f;
            //  AITank.Properties.MaximalTurn = MathHelper.ToRadians(30+ MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*45f);

            if (FearMult> 0f) { FearMult -= RuntimeData.DeltaTime; }
            else { FearMult = 0f; }

            FearCooldown -= RuntimeData.DeltaTime;
        }

        public override void DangerDetected()
        {
            base.DangerDetected();
            if (AITank.ClosestDanger!.Team != AITank.Team && FearCooldown < 0.1f)
            {
                FearMult = 2.06f * 60f;
                FearCooldown = 2.6f * 60f;
            }
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (context.Source == AITank) return;
            base.TakeDamage(destroy, context);
        }
      
    }
}
