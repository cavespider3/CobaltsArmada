using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;

namespace CobaltsArmada
{
    /// <summary> A mine laying tank, laying mines remotely with the shells it fires. Always partnered with a drone, and has immunity explosions.
    /// 
    /// </summary>
    
    public class CA_04_Sunflower: CA_ArmadaTank
    {
     
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Sunflower"
        });

        public override string Texture => "assets/textures/tank_sunflower";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Yellow;


        public override void PostApplyDefaults()
        {
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 70;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);

            AITank.Parameters.AggressivenessBias = -0.2f;

            AITank.Parameters.DetectionForgivenessSelf = MathHelper.ToRadians(5); // 31 
            AITank.Parameters.DetectionForgivenessFriendly = MathHelper.ToRadians(20); // 32 
            AITank.Parameters.DetectionForgivenessHostile = MathHelper.ToRadians(20); // 33

            AITank.Parameters.AwarenessHostileMine = 120;
            AITank.Parameters.AwarenessHostileShell = 100;
            AITank.Parameters.AwarenessFriendlyShell = 14;
            AITank.Parameters.AwarenessFriendlyMine = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 250;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ShellID.Rocket;
            AITank.Properties.RicochetCount = 1;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.CanLayTread = false;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.3f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
            base.PostApplyDefaults();
        }

        public override void Shoot(Shell shell)
        {
            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

     

    }
}
