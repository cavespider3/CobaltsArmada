using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    public class CA_05_Poppy: CA_ArmadaTank
    {
    
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Rose"
        });

        public override string Texture => "assets/textures/tank_poppy";
        public override int Songs => 2;
        public override Color AssociatedColor => Color.Red;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 1.05f;
            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);

            AITank.Parameters.AggressivenessBias = 0.4f;
    

            AITank.Parameters.AwarenessHostileShell = 120;
            AITank.Parameters.AwarenessFriendlyShell = 100;
            AITank.Parameters.AwarenessHostileMine = 14;
            AITank.Parameters.AwarenessFriendlyMine = 70;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 180;
            AITank.Properties.ShellLimit = 1;
            AITank.Properties.ShellSpeed = 5f;
            AITank.Properties.ShellType = ModContent.GetSingleton<CA_ShatterShell>().Type;
            AITank.Properties.RicochetCount = 0;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.CanLayTread = false;
            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.4f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 44;
        }

        public override void Shoot(Shell shell)
        {

            base.Shoot(shell);
             shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
