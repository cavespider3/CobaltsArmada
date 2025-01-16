
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.Internals;
using TanksRebirth.Localization;

namespace CobaltsArmada
{
    public class CA_01_Dandelion : ModTank 
    {
      
        public override bool HasSong => false;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Dandelion"
        });
       
        public override string Texture => "assets/textures/tank_dandy";
        public override int Songs => 3;
        public override Color AssociatedColor => CA_Main.Dandy;

        public override void PostApplyDefaults(AITank tank)
        {
            base.PostApplyDefaults(tank);

            

            tank.Model = CA_Main.Neo_Stationary;
            tank.Scaling = Vector3.One * 100.0f;
            tank.AiParams.TurretMeanderFrequency = 15;
            tank.AiParams.TurretSpeed = 0.05f;
            tank.AiParams.AimOffset = 0.005f;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 140;

            tank.AiParams.Inaccuracy = 0.7f;

            tank.Properties.DestructionColor = CA_Main.Dandy;

            tank.Properties.ShellCooldown = 500;

            tank.Properties.ShellLimit = 2;

            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShootStun = 120;

            tank.Properties.ShellType = ShellID.Standard;

            tank.Properties.RicochetCount = 1;

            tank.AiParams.ShootChance = 0.5f;

            tank.Properties.Invisible = false;

            tank.AiParams.SmartRicochets = true;

            tank.Properties.Stationary = true;

            tank.Properties.ShellHoming = new();

            tank.BaseExpValue = 0.025f;

        }
        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
  
            base.TakeDamage(tank, destroy, context);
            CA_Main.Fire_AbstractShell_Tank(tank, 6, context, 1, 1, 5f);
        }
        public override void PreUpdate(AITank tank)
        {
            base.PreUpdate(tank);
            //tank.AiParams.TurretSpeed = tank.CurShootStun > 0 ? 0f : 0.05f;
        }

        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
            shell.Properties.FlameColor = AssociatedColor;
        }

    }
}
