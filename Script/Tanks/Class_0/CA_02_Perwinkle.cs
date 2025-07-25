using Microsoft.Xna.Framework;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.Localization;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    /// <summary> Periwinkles
    /// 
    /// </summary>
    public class CA_02_Perwinkle: CA_ArmadaTank
    {
    
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Periwinkle"
        });

        public override string Texture => "assets/textures/tank_periwinkle";
        public override int Songs => 2;
        public override Color AssociatedColor => new Color(204, 204, 255);
       
        public override void PostApplyDefaults()
        {
           //Periwinkles are probably the most generic AITank of the mod
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One;
            var Parameters = AITank.Parameters;
            var properties = AITank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(7);
            Parameters.RandomTimerMinMove = 20;
            Parameters.RandomTimerMaxMove = 60;
            Parameters.TurretMovementTimer = 60;
            Parameters.TurretSpeed = 0.045f;
            Parameters.AimOffset = 0.9f;



            Parameters.AggressivenessBias = 1f;
        
            Parameters.AwarenessHostileShell = 70;
            Parameters.AwarenessHostileMine = 140;

            properties.TurningSpeed = 0.13f;
            properties.MaximalTurn = MathHelper.PiOver4/2;

            properties.ShootStun = 0;
            properties.ShellCooldown = 70;
            properties.ShellLimit = 3;
            properties.ShellSpeed = 4f;
            properties.ShellType = ShellID.Standard;
            properties.RicochetCount = 1;

            properties.Invisible = false;
            properties.Stationary = false;
            properties.CanLayTread = false;

            properties.TreadPitch = 0.2f;
            properties.MaxSpeed = 1.3f;
            properties.Acceleration = 0.1f;
            properties.Deceleration = 0.1f;

            properties.MineCooldown = 60 * 20;
            properties.MineLimit = 1;
            properties.MineStun = 10;
            Parameters.SmartRicochets = true;
            Parameters.PredictsPositions = true;
            Parameters.ChanceMineLay = 0.05f;
        }
    
    }
}
