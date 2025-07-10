using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
//Boss AITank
namespace CobaltsArmada
{
    public class CA_Y1_Lotus: ModTank 
    {
        /// <summary>
        /// The 1st boss AITank you fight, fought and rematched at mission's 20 and 96(only on extra and above)
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
        
        public override void PostApplyDefaults()
        {
            //TANK NO BACK DOWN
            base.PostApplyDefaults();
            AITank.SpecialBehaviors[2].Value = 20;
            AITank.Properties.Armor = new TankArmor(AITank, 1);
            CA_Main.boss = new BossBar(AITank, "Lotus", "The Reformer");
            AITank.Properties.Armor.HideArmor = true;

            AITank.Model = CA_Main.Neo_Boss;
            AITank.Scaling = Vector3.One * 100.0f * 1.1f;

            AITank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            AITank.AiParams.MeanderFrequency = 10;
            AITank.AiParams.TurretMeanderFrequency = 20;
            AITank.AiParams.TurretSpeed = 0.06f;
            AITank.AiParams.AimOffset = MathHelper.ToRadians(3);

            AITank.AiParams.Inaccuracy = 0.6f;

            AITank.AiParams.PursuitLevel = 0.6f;
            AITank.AiParams.PursuitFrequency = 70;

            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = 10;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = 10;
            AITank.AiParams.MineWarinessRadius_AILaid = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 100;
            AITank.Properties.ShellLimit = 3;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Glaive>().Type;
            AITank.Properties.RicochetCount = 7;

            AITank.AiParams.ShootChance = 0.5f;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.AiParams.BlockWarinessDistance = 44;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (AITank.SpecialBehaviors[2].Value > 0 && AITank.Properties.Armor is not null)
            {
                AITank.Properties.Armor.HitPoints = Math.Min((int)AITank.SpecialBehaviors[2].Value, 1);
                AITank.SpecialBehaviors[2].Value -= 1f;
                CA_Main.Fire_AbstractShell_Tank(AITank, 1, context, 0, 1, 3f);
            }
            base.TakeDamage(destroy, context);

        }
        public override void Shoot(Shell shell)
        {
            base.Shoot( shell);
            shell.SwapTexture(CA_Main.Tank_Y1);
            shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
