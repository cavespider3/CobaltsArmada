using CobaltsArmada.AI;
using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;
using Microsoft.Xna.Framework;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.GameContent.Tanks.AI;
using TanksRebirth.GameContent.Tanks.AI.VanillaAI;
using TanksRebirth.Localization;


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
            [LangCode.English] = Modifiers.Map[CA_Main.M_LEGACY] ? "A powerful tank that speeds up to avoid danger, fires fast rockets that ricochet twice, and is immune to mines. Also comes with a drone." : "A slow, but massive armoured train, each car protected by bullet-proof casing,\n requiring the use of mines or explosives to break through. "
        };

        public float FearCooldown = 0f;
        public float FearMult = 0f;

        public override string Texture => "assets/textures/tank_carnation";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Fuchsia;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            if (Modifiers.Map[CA_Main.M_LEGACY])
            {
            
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
            properties.Resistance = ResistanceFlags.Explosions;

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
            else
            {

                AITank.DrawParams.Scaling = Vector3.One * 1.3f;
                var Parameters = AITank.Parameters;
                var properties = AITank.Properties;
                Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
                Parameters.RandomTimerMinMove = 20;
                Parameters.RandomTimerMaxMove = 60;
                Parameters.TurretMovementTimer = 20;
                Parameters.TurretSpeed = 0.03f;
               
                Parameters.AimOffset = 0.03f;
                AITank.Parameters.MaxQueuedMovements = 5;
                properties.TurningSpeed = 0.1f;
                properties.MaximalTurn = MathHelper.PiOver4;
                //also maximum agro lmfao
                Parameters.AggressivenessBias = 0.4f;

                Parameters.AwarenessHostileShell = 140;
                Parameters.AwarenessFriendlyShell = 160;
                Parameters.RandomTimerMaxShoot = 100;
                Parameters.RandomTimerMinShoot = 40;
                
                //they're immune to mines. NO.
                Parameters.AwarenessHostileMine = 160;
                Parameters.AwarenessFriendlyMine = 160;
                Parameters.PredictsPositions = true;

                properties.ShootStun = 1;
                properties.ShellCooldown = 60;
                properties.ShellLimit = 5;
                properties.ShellSpeed = 4f;
                properties.ShellType = ShellID.Rocket;

                properties.RicochetCount = 1;

                properties.Invisible = false;
                properties.Stationary = false;
                properties.Resistance = ResistanceFlags.Shells;

                properties.TreadPitch = -0.26f;
                properties.Acceleration = 0.4f;
                properties.Deceleration = 0.9f;
                properties.MaxSpeed = 1.1f;

                Parameters.ObstacleAwarenessMovement = 80;
                Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(1);
                Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(14);
                Parameters.Rememberance = 600;
                Parameters.SmartTargeting = true;
                Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20);

                AITank.Extras.Armor = new(AITank,3);
                AITank.Extras.Armor.HideArmor = false;

                if (AITank.TankAI is not CentipedeAISystem)
                   { AITank.TankAI = new CentipedeAISystem(AITank, null);
                    if (AITank.TankAI is CentipedeAISystem centipede)
                    {
                        centipede.Segments = 10;
                        centipede.StupidTrain = false;
                        centipede.TurretPattern = [false, true, true, false];
                    }
}
            }


            

        }

        public override void Shoot(Shell shell)
        {
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }
      
        public override void PreUpdate()
        {

            base.PreUpdate();
            if (Modifiers.Map[CA_Main.M_LEGACY])
            {
                AITank.Properties.MaxSpeed = 2f + MathF.Min(1f, FearMult / 60f / 2f) * 2.6f;
                AITank.Properties.TreadPitch = MathHelper.Lerp(-0.8f, 0.9f, MathHelper.Clamp((AITank.Properties.MaxSpeed - 2f) / 2.6f, 0f, 1f));

                //   AITank.Properties.TurningSpeed = 0.06f + MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*0.055f;
                //  AITank.Properties.MaximalTurn = MathHelper.ToRadians(30+ MathF.Min(1f, AITank.SpecialBehaviors[0].Value)*45f);

                if (FearMult > 0f) { FearMult -= RuntimeData.DeltaTime; }
                else { FearMult = 0f; }

                FearCooldown -= RuntimeData.DeltaTime;
            }
        }

        public override void DangerDetected()
        {
            base.DangerDetected();
            if (Modifiers.Map[CA_Main.M_LEGACY])
            {
                if (AITank.TankAI is VanillaAISystem AiSystem && AiSystem.ClosestDanger!.Team != AITank.Team && FearCooldown < 0.1f)
                {
                    FearMult = 2.06f * 60f;
                    FearCooldown = 2.6f * 60f;
                }
            }
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (Modifiers.Map[CA_Main.M_LEGACY])
            {
                if (context.Source == AITank) return;
                base.TakeDamage(destroy, context);
            }
            else
            {
             
                if (!destroy)
                {
                    if (AITank.Extras.Armor is null || AITank.Extras.Armor.HitPoints == 0)
                    {
                        AITank.Properties.Resistance = 0;
                        if (context.Source is not null && (context.Source == AITank || context.Source.Team == AITank.Team)) return;
                        base.TakeDamage(destroy, context);
                    }
                   
                }

            }
        }
      
    }
}
