
using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.TankSystem.AI;
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
            base.PostApplyDefaults();
            AIParameters aiParams = AITank.Parameters;
            TankProperties properties = AITank.Properties;
            AITank.DrawParamsTank.Model = CA_Main.Neo_Remote;
            //## Visuals ##//

      
            properties.DestructionColor = CA_Main.Dandy;
            properties.Invisible = false;

            //## Behaviour ##//

            properties.Stationary = true;
            aiParams.SmartRicochets = true;

            //## Properties ##//

            properties.ShellCooldown = CA_Main.GetValueByDifficulty(Normal: 20u, Hard: 20u, Lunatic: 10u, Extra: 5u);

            properties.ShellLimit = (Modifiers.Map[Modifiers. MACHINE_GUNS] ? 2 : 1) * CA_Main.GetValueByDifficulty(Normal: 3, Hard: 4, Lunatic: 6, Extra: 10);

            properties.ShellSpeed = 4f;

            AITank.Properties.RicochetCount = CA_Main.GetValueByDifficulty(Normal: 3, Hard: 3, Lunatic: 4, Extra: 6);

            aiParams.RandomTimerMinShoot = CA_Main.GetValueByDifficulty(Normal: 70, Hard: 50, Lunatic: 25, Extra: 5);

            aiParams.RandomTimerMaxShoot = CA_Main.GetValueByDifficulty(Normal: 120, Hard: 80, Lunatic: 40, Extra: 10);

            AITank.Properties.ShellHoming = new();
            aiParams.DetectionForgivenessSelf = MathHelper.ToRadians(10);
            aiParams.DetectionForgivenessHostile = MathHelper.ToRadians(6);
            aiParams.DetectionForgivenessFriendly = MathHelper.ToRadians(20);
            aiParams.AimOffset = MathHelper.ToRadians(CA_Main.GetValueByDifficulty(Normal: 10, Hard: 6, Lunatic: 4, Extra: 0));

            aiParams.TurretSpeed = CA_Main.GetValueByDifficulty(Normal: 0.025f, Hard: 0.035f, Lunatic: 0.05f, Extra: 0.07f);

            aiParams.TurretMovementTimer = CA_Main.GetValueByDifficulty(Normal: 40u, Hard: 25u, Lunatic: 15u, Extra: 10u);

            aiParams.TankAwarenessShoot = 14f;
        }

        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (!destroy || context is TankHurtContextExplosion) return;
            int count = CA_Main.GetValueByDifficulty(Normal: 4, Hard: 6, Lunatic: 8, Extra: 12);
            float speed = CA_Main.GetValueByDifficulty(Normal: 3.1f, Hard: 3.8f, Lunatic: 4.5f, Extra: 7.5f);
            int rics = CA_Main.GetValueByDifficulty(Normal: 0u, Hard: 1u, Lunatic: 1u, Extra: 0u);
            CA_Main.Fire_AbstractShell_Tank(base.AITank, count, context, 1,rics);
        }

        public override void Shoot(Shell shell)
        {
                float rad = CA_Main.GetValueByDifficulty(Normal: 1.7f, Hard: 2.4f, Lunatic: 3.2f, Extra: 4.5f);

                var ring = GameHandler.Particles.MakeParticle(AITank.TargetTank!.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                ring.Scale = new(0.6f);
                ring.Pitch = MathHelper.PiOver2;
                ring.HasAdditiveBlending = true;
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
