
using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;
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
    /// <summary>A stationary, slow firing sentry that launches orbital strikes instead of firing bullets.
    ///</summary>
    public class CA_01_Dandelion : CA_ArmadaTank 
    {
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Dandelion"
        });
       
        public override string Texture => "assets/textures/tank_dandy";
        public override int Songs => 1;
        public override Color AssociatedColor => CA_Main.Dandy;
        public override void PostApplyDefaults()
        {
            AITank.UsesCustomModel = true;
            AITank.Scaling = Vector3.One;
            AITank.Parameters.TurretMovementTimer = 40;
            AITank.Parameters.TurretSpeed = 0.03f;
            AITank.Parameters.AimOffset = 0.2f;

            AITank.Properties.DestructionColor = CA_Main.Dandy;
            uint cooler =
             CA_Main.modifier_Difficulty > ModDifficulty.Easy ? 
             CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
             CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
             CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
             75u : 100u : 150u : 225u : 300u ;

            AITank.Properties.ShellCooldown = Difficulties.Types["MachineGuns"]?25u: cooler;

            AITank.Properties.ShellLimit = Difficulties.Types["MachineGuns"] ? 10 : 5;

            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShootStun = CA_Main.modifier_Difficulty >= ModDifficulty.Extra ? 3u : 10u ;

            AITank.Properties.ShellType = ShellID.Standard;

            AITank.Properties.RicochetCount = CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
             CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
             CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
             CA_Main.modifier_Difficulty >= ModDifficulty.Extra ?
             5u : 4u : 3u : 2u : 2u;

            AITank.Parameters.RandomTimerMinShoot = CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
            CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
            30 : 60 : 120;
            AITank.Parameters.RandomTimerMaxShoot = (CA_Main.modifier_Difficulty > ModDifficulty.Easy ?
         CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
         30 : 60 : 120 ) + AITank.Parameters.RandomTimerMinShoot;

           
            AITank.Properties.Invisible = false;

            AITank.Parameters.SmartRicochets = true;

            AITank.Properties.Stationary = true;

            AITank.Properties.ShellHoming = new();

            base.PostApplyDefaults();
           
        }

        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
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
            CA_Main.Fire_AbstractShell_Tank(base.AITank, count, context, 1, CA_Main.modifier_Difficulty >= ModDifficulty.Lunatic ? 2u : CA_Main.modifier_Difficulty > ModDifficulty.Easy ? 1u : 0, 5f);
        }

        public override void Shoot(Shell shell)
        {
                float rad =
                    CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
               CA_Main.modifier_Difficulty > ModDifficulty.Hard ?
               CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
               CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
                 4.5f : 3.75f : 3f : 2.25f : 1.5f;
                var ring = GameHandler.Particles.MakeParticle(AITank.TargetTank!.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                ring.Scale = new(0.6f);
                ring.Pitch = MathHelper.PiOver2;
                ring.HasAddativeBlending = true;
                ring.Color = Color.Cyan;

                ring.UniqueBehavior = (a) =>
                {
                    ring.Alpha -= 0.06f * RuntimeData.DeltaTime;

                    GeometryUtils.Add(ref ring.Scale, (0.03f) * RuntimeData.DeltaTime);
                    if (ring.Alpha <= 0)
                        ring.Destroy();
                };
               
                CA_OrbitalStrike orbitalStrike = new CA_OrbitalStrike(AITank.TargetTank!.Position, AITank, rad, 2f, 0.1f, CA_Main.modifier_Difficulty >= ModDifficulty.Extra ? 2.5f:1f, CA_OrbitalStrike.TeamkillContext.All);
                orbitalStrike._LaserTexture = CA_Main.Beam_Dan;
            shell.Remove();

        }

    }
}
