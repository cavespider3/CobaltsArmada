
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
using static CobaltsArmada.CA_Main;

namespace CobaltsArmada
{
    /// <summary>A stationary, slow firing tank that uses orbital 
    /// strikes instead of shells.<br/>
    /// 
    /// 
    ///<remarks><br/><br/><c>Technical Info:</c><br/>
    ///The spawning of oribital strikes is dependent on the path of the bullet and what it collides with
    /// </remarks>
    ///</summary>
    public class CA_01_Dandelion : ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Dandelion"
        });
       
        public override string Texture => "assets/textures/tank_dandy";
        public override int Songs => 2;
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
            uint cooler =
             CA_Main.modifier_Difficulty > ModDifficulty.Easy ? 
             CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
             CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
             CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
             75u : 100u : 150u : 225u : 300u ;

            tank.Properties.ShellCooldown = Difficulties.Types["MachineGuns"]?25u: cooler;

            tank.Properties.ShellLimit = Difficulties.Types["MachineGuns"] ? 10 : 5;

            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShootStun = CA_Main.modifier_Difficulty >= ModDifficulty.Extra ? 3u : 10u ;

            tank.Properties.ShellType = ShellID.Standard;

            tank.Properties.RicochetCount = CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
             CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
             CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
             CA_Main.modifier_Difficulty >= ModDifficulty.Extra ?
             4 : 3u : 2u : 1u : 1u;

            tank.AiParams.ShootChance = CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
            CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
            0.75f : 0.5f : 0.25f;

            tank.Properties.Invisible = false;

            tank.AiParams.SmartRicochets = true;

            tank.Properties.Stationary = true;

            tank.Properties.ShellHoming = new();

            tank.BaseExpValue = 0.025f;

        }
        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
  
            base.TakeDamage(tank, destroy, context);
            int count = 
            CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
            CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
            CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
            12:8:6:4;
            float speed =
            CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
            CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
            CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
            7f : 6.5f : 5f : 3.5f;
            CA_Main.Fire_AbstractShell_Tank(tank, count, context, 1, CA_Main.modifier_Difficulty >= ModDifficulty.Lunatic ? 2u : CA_Main.modifier_Difficulty > ModDifficulty.Easy ? 1u : 0, 5f);
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
                float rad =
                    CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
               CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
               CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
               CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
                 4.5f : 3.75f : 3f : 2.25f : 1.5f;
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
               

                CA_OrbitalStrike orbitalStrike = new CA_OrbitalStrike(tank.ShotPathTankCollPoints[0], tank, rad, 2f, 0.1f, CA_Main.modifier_Difficulty >= ModDifficulty.Extra ? 2.5f:1f, CA_OrbitalStrike.TeamkillContext.All);
                orbitalStrike._LaserTexture = CA_Main.Beam_Dan;
            }
            shell.Remove();

        }

    }
}
