using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
//Boss tank
namespace CobaltsArmada
{
    /// <summary>
    /// The 3rd boss tank you fight, fought and rematched at mission's 60 and 98(only on extra and above)
    /// </summary>
    public class CA_Y3_Peony: ModTank 
    {

        public override int Songs => 2;
         public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Peony"
        });

        public override string Texture => "assets/textures/tank_peony";
    
        public override Color AssociatedColor => Color.PeachPuff;
        
        public override void PostApplyDefaults(AITank tank)
        {
            //TANK NO BACK DOWN

            CA_Main.MissionDeadline = new VindicationTimer(tank);
            base.PostApplyDefaults(tank);
            tank.SpecialBehaviors[2].Value = 25;
            tank.Properties.Armor = new Armor(tank, 1);
            CA_Main.boss = new BossBar(tank, "Peony", "The Wilting");
            tank.Model = CA_Main.Neo_Boss;
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
            tank.Properties.ShellLimit = 3;
            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Glave>().Type;
            tank.Properties.RicochetCount = 7;

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
            if (tank.SpecialBehaviors[2].Value > 1 && tank.Properties.Armor is not null)
            {

                tank.Properties.Armor.HitPoints = 1;
                if (!context.IsPlayer && context is not TankHurtContextMine) return;
                tank.SpecialBehaviors[2].Value -= 1f;
            }
            base.TakeDamage(tank, destroy, context);

        }
        
       
    }
}
