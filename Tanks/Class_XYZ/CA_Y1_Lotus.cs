using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
//Boss tank
namespace CobaltsArmada
{
    public class CA_Y1_Lotus: ModTank 
    {
        /// <summary>
        /// The 1st boss tank you fight, fought and rematched at mission's 20 and 96(only on extra and above)
        /// while active,
        /// </summary
        public override int Songs => 2;
         public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Lotus"
        });

        public override string Texture => "assets/textures/tank_lotus";
    
        public override Color AssociatedColor => Color.Magenta;
        
        public override void PostApplyDefaults(AITank tank)
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults(tank);
            tank.SpecialBehaviors[2].Value = 20;
            tank.Properties.Armor = new Armor(tank, 1);
            CA_Main.boss = new BossBar(tank, "Lotus", "The Reformer");
            tank.Properties.Armor.HideArmor = true;

            tank.Model = CA_Main.Neo_Boss;
            tank.Scaling = Vector3.One * 100.0f * 1.1f;

            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.06f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(3);

            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 0.6f;
            tank.AiParams.PursuitFrequency = 70;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 10;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 10;
            tank.AiParams.MineWarinessRadius_AILaid = 50;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            tank.Properties.ShootStun = 12;
            tank.Properties.ShellCooldown = 100;
            tank.Properties.ShellLimit = 3;
            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Glave>().Type;
            tank.Properties.RicochetCount = 7;

            tank.AiParams.ShootChance = 0.5f;

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
            if (tank.SpecialBehaviors[2].Value > 0 && tank.Properties.Armor is not null)
            {
                tank.Properties.Armor.HitPoints = Math.Min((int)tank.SpecialBehaviors[2].Value, 1);
                tank.SpecialBehaviors[2].Value -= 1f;
                CA_Main.Fire_AbstractShell_Tank(tank, 1, context, 0, 1, 3f);
            }
            base.TakeDamage(tank, destroy, context);

        }
        public override void Shoot(AITank tank, ref Shell shell)
        {
            base.Shoot(tank, ref shell);
            shell.SwapTexture(CA_Main.Tank_Y1);
            shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
