using CobaltsArmada.Script.Tanks;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
using TanksRebirth.Net;
//Boss AITank
namespace CobaltsArmada
{
    /// <summary>
    /// The 3rd boss AITank you fight, fought and rematched at mission's 60 and 98(only on extra and above)
    /// </summary>
    public class CA_Y3_Peony: CA_ArmadaTank
    {

        public override int Songs => 1;
         public override bool HasSong => true;
        public override LocalizedString Name => new()
        {
            [LangCode.English] = "Peony"
        };

        public override string Texture => "assets/textures/tank_peony";
        
        public override Color AssociatedColor => Color.PeachPuff;

        public int Health;


        public override void PostApplyDefaults()
        {
         
            CA_Main.MissionIsDestroyedline = new VindicationTimer(AITank);
            base.PostApplyDefaults();
            Health = (Modifiers.Map[Modifiers.RANDOM_ENEMY] ? 3 : 15) + Server.CurrentClientCount * 10;
            AITank.Properties.Armor = new TankArmor(AITank, 1);
            AITank.Properties.Armor.HideArmor = true;
          //  CA_Main.boss = new BossBar(AITank, "Peony", "The Wilting");
            AITank.DrawParamsTank.Model = CA_Main.Neo_Boss;
            AITank.DrawParams.Scaling = Vector3.One * 1.1f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(20);


            AITank.Parameters.AggressivenessBias = 1f;


            AITank.Parameters.AwarenessHostileShell = 0;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 500;
            AITank.Properties.ShellLimit = 3;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ModRegistry.GetSingleton<CA_Shell_Glaive>().Type;
            AITank.Properties.RicochetCount = 7;



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
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (Health > 1 && AITank.Properties.Armor is not null)
            {
                AITank.Properties.Armor.HitPoints = 1;
                if (context.Source is AITank && context is not TankHurtContextExplosion) return;
                Health -= 1;
            }
            base.TakeDamage(destroy, context);

        }
     

    }
}
