using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Properties;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using TanksRebirth.Net;

namespace CobaltsArmada
{
    public class CA_08_Eryngium : ModTank 
    {
      
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Eryngium"
        });

        public override string Texture => "assets/textures/tank_eryngium";
        public override int Songs => 3;
        public override Color AssociatedColor => Color.SteelBlue;
        public override void PostApplyDefaults(AITank tank)
        {
            
            base.PostApplyDefaults(tank);
            tank.Model = CA_Main.Neo_Mobile;
            tank.Scaling = Vector3.One * 100.0f * 1.1f;
            Properties_Visible(tank);
        }

        void Properties_Visible(AITank tank)
        {
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderFrequency = 40;
            aiParams.MeanderAngle = MathHelper.ToRadians(25);
            aiParams.TurretMeanderFrequency = 30;
            aiParams.TurretSpeed = 0.02f;
            aiParams.AimOffset = MathHelper.ToRadians(80);
            aiParams.Inaccuracy = MathHelper.ToRadians(25);

            aiParams.TurretSpeed = 0.03f;
            aiParams.AimOffset = 0.18f;

            aiParams.PursuitLevel = 0.05f;
            aiParams.PursuitFrequency = 240;

            aiParams.ProjectileWarinessRadius_PlayerShot = 60;
            aiParams.MineWarinessRadius_PlayerLaid = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(30);

            //RAIL CANNON!
            properties.ShootStun = 20;
            properties.ShellCooldown = 100;
            properties.ShellLimit = 1;
            properties.ShellSpeed = 5f;
            properties.Recoil = 500f;
            properties.ShellType = ShellID.TrailedRocket;
            properties.RicochetCount = 0;
            //we get a little devious
            
            properties.Stationary = false;
            properties.ShellHoming = new();
            tank.Properties.CanLayTread = true;
            aiParams.SmartRicochets = false;
            aiParams.PredictsPositions = true;
            tank.Properties.TrackType = TrackID.Thick;
            properties.TreadPitch = -0.2f;
            properties.MaxSpeed = 1.4f;
            properties.TreadVolume = 0.2f;
            properties.Acceleration = 0.3f;

            properties.MineCooldown = 700;
            properties.MineLimit = 2;
            properties.MineStun = 10;

            aiParams.MinePlacementChance = 0.05f;

            aiParams.BlockWarinessDistance = 90;
            aiParams.BlockReadTime = 10;

            tank.BaseExpValue = 0.1f;
        }

        void Properties_Invisible(AITank tank)
        {
            var aiParams = tank.AiParams;
            var properties = tank.Properties;
            aiParams.MeanderFrequency = 15;
            aiParams.MeanderAngle = MathHelper.ToRadians(25);
            aiParams.TurretSpeed = 0.02f;
            aiParams.AimOffset = MathHelper.ToRadians(80);
            aiParams.Inaccuracy = MathHelper.ToRadians(25);

            aiParams.TurretSpeed = 0.03f;
            aiParams.AimOffset = 0.18f;

            aiParams.PursuitLevel = 0.05f;
            aiParams.PursuitFrequency = 240;

            aiParams.ProjectileWarinessRadius_PlayerShot = 60;
            aiParams.MineWarinessRadius_PlayerLaid = 160;

            properties.TurningSpeed = 0.06f;
            properties.MaximalTurn = MathHelper.ToRadians(30);

            //RAIL CANNON!
            properties.ShootStun = 20;
            properties.ShellCooldown = 30;
            properties.ShellLimit = 5;
            properties.ShellSpeed = 5f;
            properties.Recoil = 500f;
            properties.ShellType = ShellID.TrailedRocket;
            properties.RicochetCount = 0;
           
            //we get a little devious
        
            properties.Stationary = false;
            properties.ShellHoming = new();

            tank.Properties.CanLayTread = false;
            aiParams.PredictsPositions = false;
            aiParams.SmartRicochets = true;

            properties.TreadPitch = -0.2f;
            properties.TreadVolume = 0f;
            properties.MaxSpeed = 1.4f*2f;
            properties.Acceleration = 0.3f;

            properties.MineCooldown = 700;
            properties.MineLimit = 2;
            properties.MineStun = 10;

            aiParams.MinePlacementChance = 0.05f;

            aiParams.BlockWarinessDistance = 90;
            aiParams.BlockReadTime = 10;

            tank.BaseExpValue = 0.1f;
        }

        public override void Shoot(AITank tank, ref Shell shell)
        {

            base.Shoot(tank, ref shell);
             shell.Properties.FlameColor = Color.AliceBlue;
            shell.Properties.TrailColor = Color.AliceBlue;
        }

        public override void PreUpdate(AITank tank)
        {
            base.PreUpdate(tank);
          
            tank.SpecialBehaviors[0].Value += TankGame.DeltaTime;
            if (tank.SpecialBehaviors[1].Value == 0)
                tank.SpecialBehaviors[1].Value = 600;

            if (tank.SpecialBehaviors[0].Value > tank.SpecialBehaviors[1].Value)
            {
                tank.SpecialBehaviors[1].Value = 0f;
                tank.SpecialBehaviors[0].Value = 0f;

                
                tank.Properties.Invisible = !tank.Properties.Invisible;
                
                swap(tank);
            }
        }

        void swap(AITank tank)
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            if (tank.Dead) return;
            if (tank.Properties.Invisible) Properties_Invisible(tank);
            else Properties_Visible(tank);
            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, gameplaySound: true);

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
                    lp.Alpha -= 0.01f * TankGame.DeltaTime;

                if (lp.Alpha <= 0)
                    lp.Destroy();
                /*if (lp.Scale.X < 5f)
                    GeometryUtils.Add(ref lp.Scale, 0.12f * TankGame.DeltaTime);
                if (lp.Alpha < 1f && lp.Scale.X < 5f)
                    lp.Alpha += 0.02f * TankGame.DeltaTime;

                if (lp.LifeTime > 90)
                    lp.Alpha -= 0.005f * TankGame.DeltaTime;

                if (lp.Scale.X < 0f)
                    lp.Destroy();*/
            };

            const int NUM_LOCATIONS = 8;

            for (int i = 0; i < NUM_LOCATIONS; i++)
            {
                var lp = GameHandler.Particles.MakeParticle(tank.Position3D + new Vector3(0, 5, 0),
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));

                lp.Color = Color.SkyBlue;

                var velocity = Vector2.UnitY.RotatedByRadians(MathHelper.ToRadians(360f / NUM_LOCATIONS * i));

                lp.Scale = new(1f);

                lp.UniqueBehavior = (elp) => {
                    elp.Position.X += velocity.X * TankGame.DeltaTime;
                    elp.Position.Z += velocity.Y * TankGame.DeltaTime;

                    if (elp.LifeTime > 15)
                    {
                        GeometryUtils.Add(ref elp.Scale, -0.03f * TankGame.DeltaTime);
                        elp.Alpha -= 0.03f * TankGame.DeltaTime;
                    }

                    if (elp.Scale.X <= 0f || elp.Alpha <= 0f)
                        elp.Destroy();
                };
            }
        }
    }
}
