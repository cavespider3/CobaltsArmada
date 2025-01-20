
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;
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
            tank.Scaling = Vector3.One * 100f;
            tank.AiParams.MeanderAngle = MathHelper.ToRadians(40);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 40;
            tank.AiParams.TurretSpeed = 0.03f;
            tank.AiParams.AimOffset = 0.2f;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 140;

            tank.AiParams.Inaccuracy = 0.7f;

            tank.Properties.DestructionColor = CA_Main.Dandy;

            tank.Properties.ShellCooldown = Difficulties.Types["MachineGuns"]?25u:250u;

            tank.Properties.ShellLimit = Difficulties.Types["MachineGuns"] ? 10 : 3;

            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShootStun = 10;

            tank.Properties.ShellType = ShellID.Standard;

            tank.Properties.RicochetCount = 3;

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
            if (tank.ShotPathTankCollPoints.Length > 0)
            {
                var ring = GameHandler.Particles.MakeParticle(tank.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                ring.Scale = new(0.6f);
                ring.Roll = MathHelper.PiOver2;
                ring.HasAddativeBlending = true;
                ring.Color = Color.Cyan;

                ring.UniqueBehavior = (a) =>
                {
                    ring.Alpha -= 0.06f * TankGame.DeltaTime;

                    GeometryUtils.Add(ref ring.Scale, (0.03f) * TankGame.DeltaTime);
                    if (ring.Alpha <= 0)
                        ring.Destroy();
                };
               
                CA_OrbitalStrike orbitalStrike = new CA_OrbitalStrike(tank.ShotPathTankCollPoints[0], tank, 1.5f, 2f, 0.1f, 1f, CA_OrbitalStrike.TeamkillContext.All);
                orbitalStrike._LaserTexture = CA_Main.Beam_Dan;
            }
            shell.Remove();

        }

    }
}
