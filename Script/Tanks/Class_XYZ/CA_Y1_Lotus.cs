using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
//Boss AITank
namespace CobaltsArmada
{
    public class CA_Y1_Lotus: CA_ArmadaTank
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
            AITank.SpecialBehaviors[2].Value = Difficulties.Types["RandomizedTanks"]?5:20;
            AITank.Properties.Armor = new TankArmor(AITank, 1);
            CA_Main.boss = new BossBar(AITank, "Lotus", "The Reformer");
            AITank.Properties.Armor.HideArmor = true;

            AITank.Model = CA_Main.Neo_Boss;
            AITank.Scaling = Vector3.One * 1.1f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(45);


            AITank.Parameters.AggressivenessBias = 0.6f;
     

            AITank.Parameters.AwarenessHostileShell = 10;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 10;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 240;
            AITank.Properties.ShellLimit = 2;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Glaive>().Type;
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
