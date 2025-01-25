using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
//Boss tank
namespace CobaltsArmada
{
    public class CA_Y2_NightShade : ModTank
    {
        /// <summary>
        /// The 2nd boss tank you fight, fought and rematched at mission's 40 and 97(only on extra and above).
        /// function, while active, this tank calls for backup. any tanks closeby are poisioned, becoming much more aggressive
        /// </summary
        public override int Songs => 2;
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Nightshade"
        });

        public delegate void ApplyShade(AITank tank);

        public static event ApplyShade? OnPoisoned;

        public override string Texture => "assets/textures/tank_nightshade";

        public override Color AssociatedColor => Color.DarkViolet;

        public static List<Tank> PoisionedTanks = new List<Tank>();
        public override void OnLoad()
        {
            base.OnLoad();
            OnPoisoned += Tank_OnPoisoned;
        }

        public static void Tank_OnPoisoned(AITank tank)
        {
           switch(tank.AiTankType)
            {
                default:
                    tank.Properties.ShootStun /=2;
                    tank.Properties.ShellCooldown /=2;               
                    tank.AiParams.ShootChance *= 1.5f;
                    tank.AiParams.MeanderAngle /= 2;
                    tank.AiParams.MeanderFrequency /= 2;
                    tank.AiParams.TurretMeanderFrequency /=2;
                    tank.AiParams.Inaccuracy /= 1.5f;
                    tank.AiParams.TurretSpeed *=1.75f;
                    tank.AiParams.AimOffset /=2f;
                    tank.Properties.MaxSpeed *=1.25f;
                    tank.AiParams.PursuitLevel = MathF.Sign(tank.AiParams.PursuitLevel) * tank.AiParams.PursuitLevel * 1.3f;
                    tank.AiParams.PursuitFrequency /= 2;
                    break;
            }
        }

        public override void PostApplyDefaults(AITank tank)
        {
            //TANK NO BACK DOWN


            base.PostApplyDefaults(tank);
            tank.SpecialBehaviors[2].Value = 30;
            tank.Properties.Armor = new Armor(tank, 1);

            CA_Main.boss = new BossBar(tank, "Nightshade", "The Infector");
            tank.Model = CA_Main.Neo_Boss;
            tank.Scaling = Vector3.One * 100f * 1.03f;

            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.06f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(3);

            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 1f;
            tank.AiParams.PursuitFrequency = 20;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 0;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            tank.AiParams.MineWarinessRadius_AILaid = 50;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            tank.Properties.ShootStun = 12;
            tank.Properties.ShellCooldown = 250;
            tank.Properties.ShellLimit = 3;
            tank.Properties.ShellSpeed = 2.7f;
            tank.Properties.ShellType = ShellID.Standard;
            tank.Properties.RicochetCount = 0;

            tank.AiParams.ShootChance = 0.8f;

            tank.Properties.Invisible = false;
            tank.Properties.Stationary = false;

            tank.Properties.TreadVolume = 0.1f;
            tank.Properties.TreadPitch = 0.3f;
            tank.Properties.MaxSpeed = 1.5f;

            tank.Properties.Acceleration = 0.1f;

            tank.Properties.MineCooldown = 0;
            tank.Properties.MineLimit = 0;
            tank.Properties.MineStun = 0;

            tank.AiParams.BlockWarinessDistance = 44;
        }
        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
            if (tank.SpecialBehaviors[2].Value > 0 && tank.Properties.Armor is not null)
            {
                tank.Properties.Armor.HitPoints = Math.Min((int)tank.SpecialBehaviors[2].Value, 1);
                tank.SpecialBehaviors[2].Value -= 1f;
          
            }
            base.TakeDamage(tank, destroy, context);

        }

        public static void SpawnPoisonCloud(Vector3 v)
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f,pitchOverride:0.5f,gameplaySound:true);
            int length = 17;

            for (int i = 0; i < length; i++)
            {
                Vector2 smokey = Vector2.One.RotatedByRadians(Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI)) * Server.ServerRandom.NextFloat(0.1f, 60f);
                var smoke = GameHandler.Particles.MakeParticle(v + smokey.ExpandZ(),
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
                smoke.Roll = -TankGame.DEFAULT_ORTHOGRAPHIC_ANGLE;
                smoke.Scale = new(0.8f * Server.ServerRandom.NextFloat(0.1f, 1f));
                smoke.Color = Color.DarkViolet;
                smoke.HasAddativeBlending = false;
                smoke.UniqueBehavior = (part) => {

                    GeometryUtils.Add(ref part.Scale, -0.004f * TankGame.DeltaTime);
                    part.Position += Vector3.UnitY * 0.2f * TankGame.DeltaTime;
                    part.Alpha -= 0.04f * TankGame.DeltaTime;

                    if (part.Alpha <= 0)
                        part.Destroy();

                };
            }
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {

                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;

                var ai = tanks[i] as AITank;
                if (ai is null || ai.Dead || ai.AiTankType == ModContent.GetSingleton<CA_Y2_NightShade>().Type) continue;

                if (Vector2.Distance(ai.Position, v.FlattenZ()) > 60f) continue;
                bool NotIntoxicated = true;
                if (PoisionedTanks.Find(x => x == ai) is null)
                {
                    PoisionedTanks.Add(ai);
                    CA_Y2_NightShade.Tank_OnPoisoned(ai);
                }
            }

        }

        public override void PostUpdate(AITank tank)
        {
            base.PostUpdate(tank);
            Vector2 smokey = Vector2.One.RotatedByRadians(Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI)) * Server.ServerRandom.NextFloat(0.1f, 60f);
            var smoke = GameHandler.Particles.MakeParticle(tank.Position3D + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
            smoke.Roll = -TankGame.DEFAULT_ORTHOGRAPHIC_ANGLE;
            smoke.Scale = new(0.8f * Server.ServerRandom.NextFloat(0.1f, 1f));
            smoke.Color = Color.DarkViolet;
            smoke.HasAddativeBlending = false;
            smoke.UniqueBehavior = (part) => {

                GeometryUtils.Add(ref part.Scale, -0.004f * TankGame.DeltaTime);
                part.Position += Vector3.UnitY * 0.2f * TankGame.DeltaTime;
                part.Alpha -= 0.04f * TankGame.DeltaTime;

                if (part.Alpha <= 0)
                    part.Destroy();

            };

            if (LevelEditor.Active) return;



            tank.SpecialBehaviors[0].Value += TankGame.DeltaTime;
            if (tank.SpecialBehaviors[1].Value == 0)
                tank.SpecialBehaviors[1].Value = Server.ServerRandom.NextFloat(60, 120);

            if (tank.SpecialBehaviors[0].Value > tank.SpecialBehaviors[1].Value)
            {
                tank.SpecialBehaviors[1].Value = 0f;
                tank.SpecialBehaviors[0].Value = 0f;
                ref Tank[] tanks = ref GameHandler.AllTanks;
                for (int i = 0; i < tanks.Length; i++)
                {

                    if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                    
                    var ai = tanks[i] as AITank;
                    if (ai is null || ai.Dead || ai.AiTankType == Type) continue;

                    if (Vector2.Distance(ai.Position, tank.Position) > 60f) continue;
                    bool NotIntoxicated = true;
                    if (PoisionedTanks.Find(x => x == ai) is null)
                    {
                        PoisionedTanks.Add(ai);
                        Tank_OnPoisoned(ai);
                    }
                }
            }

        }
        
        


        public static void WhilePoisoned(AITank tank)
        {
           if( PoisionedTanks.Find(x => x == tank) is not null && Server.ServerRandom.NextFloat(0.1f, 1f)<0.7f && !tank.Dead)
            {
                Vector2 smokey = Vector2.One.RotatedByRadians(Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI))* Server.ServerRandom.NextFloat(0.1f, 1f)*tank.CollisionCircle.Radius*1.1f;

                var smoke = GameHandler.Particles.MakeParticle(tank.Position3D+ smokey.ExpandZ(),
                    GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));

               
                smoke.Roll = -TankGame.DEFAULT_ORTHOGRAPHIC_ANGLE;

                smoke.Scale = new(0.5f);
           

                smoke.Color = Color.DarkViolet;

                smoke.HasAddativeBlending = false;



            
                smoke.UniqueBehavior = (part) => {

                    GeometryUtils.Add(ref part.Scale, -0.004f * TankGame.DeltaTime);
                    part.Position += Vector3.UnitY * 0.2f * TankGame.DeltaTime;
                    part.Alpha -= 0.04f * TankGame.DeltaTime;

                        if (part.Alpha <= 0)
                            part.Destroy();
             
                };
            }

        }


    }
}
