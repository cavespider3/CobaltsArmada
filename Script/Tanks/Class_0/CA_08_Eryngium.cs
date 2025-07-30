using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;

using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using TanksRebirth.Net;
using TanksRebirth.GameContent.Systems.AI;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{

    public class CA_08_Eryngium : CA_ArmadaTank
    {
      //Sea Holly
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Eryngium"
        });

        public override string Texture => "assets/textures/tank_eryngium";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.SteelBlue;
        public override void PostApplyDefaults()
        {
            base.PostApplyDefaults();
            AITank.UsesCustomModel = true;
            AITank.Model = CA_Main.Neo_Mobile;
            AITank.Scaling = Vector3.One * 1.1f;
            Array.Resize(ref AITank.SpecialBehaviors, 3);
            for (int i = 0; i < AITank.SpecialBehaviors.Length; i++)
            {
                AITank.SpecialBehaviors[i] = new TanksRebirth.GameContent.GameMechanics.AiBehavior();
            }
            Properties_Visible(AITank);
            
        }
        static AITank? _Tank;
        static void Properties_Visible(AITank tank)
        {
            var Parameters = tank.Parameters;
            var properties = tank.Properties;
            Parameters.RandomTimerMinMove = 40;
            Parameters.RandomTimerMaxMove = 80;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(25);
            Parameters.TurretMovementTimer = 30;
            Parameters.TurretSpeed = 0.02f;
            Parameters.AimOffset = MathHelper.ToRadians(80);


            Parameters.AggressivenessBias = -0.05f;

            Parameters.AwarenessHostileShell = 60;
            Parameters.AwarenessHostileMine = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(30);

            //RAIL CANNON!
            properties.ShootStun = 20;
            properties.ShellCooldown = 100;
            properties.ShellLimit = 1;
            properties.ShellSpeed = 5f;
            properties.ShellType = ShellID.TrailedRocket;
            properties.RicochetCount = 0;
            //we get a little devious
            
            properties.Stationary = false;
            properties.ShellHoming = new();
            tank.Properties.CanLayTread = true;
            Parameters.SmartRicochets = false;
            properties.InvulnerableToMines = false;
            Parameters.PredictsPositions = true;
            tank.Properties.TrackType = TrackID.Thick;
            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.3f;
            properties.TreadVolume = 0.2f;
            properties.Acceleration = 0.3f;
            tank.Model = CA_Main.Neo_Mobile;
            tank.InitModelSemantics();
            properties.MineCooldown = 700;
            properties.MineLimit = 0;
            properties.MineStun = 10;
            
            Parameters.ChanceMineLay = 0.05f;

            Parameters.ObstacleAwarenessMovement = 90;
            Parameters.ObstacleAwarenessMovement = 10;
           
        }

        static void Properties_Invisible(AITank tank)
        {
            var Parameters = tank.Parameters;
            var properties = tank.Properties;
            Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(25);
            Parameters.TurretSpeed = 0.03f;
            Parameters.TurretMovementTimer = 40;
            Parameters.AimOffset = MathHelper.ToRadians(23);

            Parameters.AimOffset = 0.1f;

            Parameters.AggressivenessBias = 1f;

            Parameters.AwarenessHostileShell = 60;
            Parameters.AwarenessHostileMine = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(30);
            
            //RAIL CANNON! (YASSSS)
            properties.ShootStun = 170;
            properties.ShellCooldown = 180;
            properties.ShellLimit = 1;
            properties.ShellSpeed = 3f;
          
            tank.Properties.ShellType = ModContent.GetSingleton<CA_Shell_Rail>().Type;
            properties.RicochetCount = 0;
            tank.Model = CA_Main.Neo_Stationary;
            tank.InitModelSemantics();
            //we get a little devious

            properties.Stationary = true;
      
            properties.ShellHoming = new();
            properties.InvulnerableToMines = true;
            tank.Properties.CanLayTread = false;
            Parameters.PredictsPositions = true;
            tank.Speed = 0f;

            properties.TreadPitch = -0.2f;
            properties.TreadVolume = 0f;
            properties.MaxSpeed = 0f;
            properties.Acceleration = 0.3f;

            properties.MineCooldown = 700;
            properties.MineLimit = 0;
            properties.MineStun = 10;

            Parameters.ChanceMineLay = 0.00f;

            Parameters.ObstacleAwarenessMovement = 90;
            Parameters.ObstacleAwarenessMovement = 10;

        }




        public override void PostUpdate()
        {

            AITank.Speed *= AITank.Properties.Stationary ? 0f : 1f;
            AITank.Velocity *= AITank.Properties.Stationary ? 0f : 1f;
            AITank.Parameters.TurretSpeed = AITank.CurShootStun > 0 ? 0f : 0.05f;

            AITank.SpecialBehaviors[0].Value += RuntimeData.DeltaTime;

            if (AITank.SpecialBehaviors[1].Value == 0)
                AITank.SpecialBehaviors[1].Value = AITank.Properties.Stationary ?600f: Server.ServerRandom.NextFloat(6,10)*100f;

            if (AITank.SpecialBehaviors[0].Value > AITank.SpecialBehaviors[1].Value)
            {
                _Tank = AITank;
                var ring = GameHandler.Particles.MakeParticle(_Tank.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                ring.Scale = new(0.6f);
                ring.Roll = MathHelper.PiOver2;
                ring.HasAdditiveBlending = false;
                ring.Color = Color.Purple;

                ring.UniqueBehavior = (a) =>
                {
                    ring.Alpha -= 0.06f * RuntimeData.DeltaTime;

                    GeometryUtils.Add(ref ring.Scale, (0.03f) * RuntimeData.DeltaTime);
                    if (ring.Alpha <= 0)
                        ring.Destroy();
                };
                _Tank.SpecialBehaviors[1].Value = 0f;
                _Tank.SpecialBehaviors[0].Value = 0f;

                if (!_Tank.Properties.Stationary)
                {
                    CA_08_Eryngium.Properties_Invisible(_Tank);
                    SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.3f, pitchOverride: -0.25f);
                }
                else
                {
                    CA_08_Eryngium.Properties_Visible(_Tank);
                    SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.3f, pitchOverride: 0.25f);
                }
                CA_08_Eryngium.swap(_Tank);
            }
            base.PostUpdate();
          
        }

       static void swap(AITank tank)
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            if (tank.IsDestroyed) return;
            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f);

            var lightParticle = GameHandler.Particles.MakeParticle(tank.Position3D,
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/light_particle"));

            // lightParticle.TextureScale = new(5f);
            lightParticle.Alpha = 1;
            lightParticle.IsIn2DSpace = true;
            lightParticle.Color = Color.SkyBlue;

            lightParticle.UniqueBehavior = (lp) => {
                lp.Position = tank.Position3D;
                lp.TextureScale = new(5);

                if (lp.LifeTime > 90)
                    lp.Alpha -= 0.01f * RuntimeData.DeltaTime;

                if (lp.Alpha <= 0)
                    lp.Destroy();
                /*if (lp.Scale.X < 5f)
                    GeometryUtils.Add(ref lp.Scale, 0.12f * RuntimeData.DeltaTime);
                if (lp.Alpha < 1f && lp.Scale.X < 5f)
                    lp.Alpha += 0.02f * RuntimeData.DeltaTime;

                if (lp.LifeTime > 90)
                    lp.Alpha -= 0.005f * RuntimeData.DeltaTime;

                if (lp.Scale.X < 0f)
                    lp.Destroy();*/
            };

            const int NUM_LOCATIONS = 8;

            for (int i = 0; i < NUM_LOCATIONS; i++)
            {
                var lp = GameHandler.Particles.MakeParticle(tank.Position3D + new Vector3(0, 5, 0),
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));

                lp.Color = Color.SkyBlue;

                var velocity = Vector2.UnitY.Rotate(MathHelper.ToRadians(360f / NUM_LOCATIONS * i));

                lp.Scale = new(1f);

                lp.UniqueBehavior = (elp) => {
                    elp.Position.X += velocity.X * RuntimeData.DeltaTime;
                    elp.Position.Z += velocity.Y * RuntimeData.DeltaTime;

                    if (elp.LifeTime > 15)
                    {
                        GeometryUtils.Add(ref elp.Scale, -0.03f * RuntimeData.DeltaTime);
                        elp.Alpha -= 0.03f * RuntimeData.DeltaTime;
                    }

                    if (elp.Scale.X <= 0f || elp.Alpha <= 0f)
                        elp.Destroy();
                };
            }
        }
    }
}
