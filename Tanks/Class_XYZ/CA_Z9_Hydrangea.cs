using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;

namespace CobaltsArmada
{
    public class CA_Z9_Hydrangea: ModTank 
    {
        public override bool HasSong => false;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Hydrangea"
        });

        public override string Texture => "assets/textures/tank_Hydrangea";
        public override int Songs => 0;
        public override Color AssociatedColor => Color.Cyan;


        public override void PostApplyDefaults(AITank tank)
        {
          
           

            CA_Main.boss = new BossBar(tank, "Hydrangea", "The Unbounded");

            //TANK NO BACK DOWN
            base.PostApplyDefaults(tank);
           
            tank.Model = CA_Main.Neo_Boss;
            tank.Scaling = Vector3.One * 1.4f;
          
            tank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            tank.AiParams.MeanderFrequency = 10;
            tank.AiParams.TurretMeanderFrequency = 20;
            tank.AiParams.TurretSpeed = 0.06f;
            tank.AiParams.AimOffset = MathHelper.ToRadians(3);
            
            tank.AiParams.Inaccuracy = 0.6f;

            tank.AiParams.PursuitLevel = 0.3f;
            tank.AiParams.PursuitFrequency = 40;

            tank.AiParams.ProjectileWarinessRadius_PlayerShot = 30;
            tank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            tank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            tank.AiParams.MineWarinessRadius_AILaid = 50;

            tank.Properties.TurningSpeed = 0.09f;
            tank.Properties.MaximalTurn = MathHelper.ToRadians(21); 
            tank.SpecialBehaviors[2].Value = 32;
            tank.Properties.Armor = new Armor(tank,1);
            tank.Properties.ShootStun = 12;
            tank.Properties.ShellCooldown = 150;
            tank.Properties.ShellLimit = 9;
            tank.Properties.ShellSpread = 0.3f;
            tank.Properties.ShellShootCount = 3;
            tank.Properties.ShellSpeed = 4f;
            tank.Properties.ShellType = ShellID.Standard;
            tank.Properties.RicochetCount = 0;

            tank.AiParams.ShootChance = 0.25f;

            tank.Properties.Invisible = false;
            tank.Properties.Stationary = false;
            tank.Properties.CanLayTread = false;

            tank.Properties.TreadVolume = 0.1f;
            tank.Properties.TreadPitch = 0.3f;
            tank.Properties.MaxSpeed = 1.7f;

            tank.Properties.Acceleration = 0.1f;

            tank.Properties.MineCooldown = 0;
            tank.Properties.MineLimit = 0;
            tank.Properties.MineStun = 0;

            tank.AiParams.BlockWarinessDistance = 74;
            tank.AiParams.BlockReadTime = 15;
        }
        public override void TakeDamage(AITank tank, bool destroy, ITankHurtContext context)
        {
            if (tank.SpecialBehaviors[2].Value > 1 && tank.Properties.Armor is not null)
            {
                
                tank.Properties.Armor.HitPoints = 1;
                if (!context.IsPlayer && context is not TankHurtContextMine) return;
                    tank.SpecialBehaviors[2].Value -= 1f;
            }
            base.TakeDamage(tank, destroy, context);
           
        }


        public override void PostUpdate(AITank tank)
        {
            if (LevelEditor.Active) return;
            base.PostUpdate(tank);
            tank.SpecialBehaviors[0].Value -= TankGame.DeltaTime;
            if(tank.SpecialBehaviors[2].Value < 17f&& AIManager.CountAll(x => x.AiTankType == ModContent.GetSingleton<CA_08_Eryngium>().Type) < 1)
            {
                var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());

                var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = ModContent.GetSingleton<CA_08_Eryngium>().Type,
                    IsPlayer = false,
                    Team = tank.Team
                };
            }
            if (tank.SpecialBehaviors[0].Value < 0f && tank.SpecialBehaviors[1].Value == 0f)
            {
              if(AIManager.CountAll(x => x.AiTankType == ModContent.GetSingleton<CA_08_Eryngium>().Type) < 1)
                {
                    var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());

                    var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
                    crate.TankToSpawn = new TankTemplate()
                    {
                        AiTier = ModContent.GetSingleton<CA_08_Eryngium>().Type,
                        IsPlayer = false,
                        Team = tank.Team
                    };
                    tank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    tank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                else
                {
                    tank.SpecialBehaviors[1].Value = 1f;
                }
            }

            if (tank.SpecialBehaviors[0].Value <= 0f && tank.SpecialBehaviors[1].Value == 1f)
            {
                tank.Speed = 0;
                if (tank.SpecialBehaviors[0].Value < -240f)
                {
                    tank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    tank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                float a = MathF.Floor(MathF.Abs(tank.SpecialBehaviors[0].Value) / 30f) * MathHelper.ToRadians(25f);
                if (MathF.Floor(MathF.Abs(tank.SpecialBehaviors[0].Value)) % 30==0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var pathPos = new Vector2(0, 40 + MathF.Abs(tank.SpecialBehaviors[0].Value / 30) * 40f).RotatedByRadians(MathHelper.TwoPi*(i/3f) - a);
                        new CA_OrbitalStrike(pathPos + tank.Position, tank);


                    }
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
                }
            
            }

            if (tank.SpecialBehaviors[0].Value < 0f && tank.SpecialBehaviors[1].Value >= 3f)
            {
                tank.Speed = 0;
                if (tank.SpecialBehaviors[0].Value < -240f)
                {
                    tank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    tank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                
                if (MathF.Floor(MathF.Abs(tank.SpecialBehaviors[0].Value)) % 30 == 0)
                {

                    float a = MathF.Floor(MathF.Abs(tank.SpecialBehaviors[0].Value) / 30f)*MathHelper.ToRadians(25f);
                    for (int i = 0; i < 3; i++)
                    {
                        var pathPos = new Vector2(0,40+ MathF.Abs(tank.SpecialBehaviors[0].Value / 30 )* 40f).RotatedByRadians(MathHelper.TwoPi * (i / 3f)+ a);
                        new CA_OrbitalStrike(pathPos+tank.Position, tank);
                    }
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
                }
            }

            if (tank.SpecialBehaviors[0].Value < 0f && tank.SpecialBehaviors[1].Value == 2f)
            {
                var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());
                var r2 = new int[tank.SpecialBehaviors[2].Value < 17f ? 6 : 3];
                r2[0] = ModContent.GetSingleton<CA_02_Perwinkle>().Type;
                r2[1] = ModContent.GetSingleton<CA_03_Pansy>().Type;
                r2[2] = ModContent.GetSingleton<CA_01_Dandelion>().Type;
                if (tank.SpecialBehaviors[2].Value < 17f) {
                    r2[3] = ModContent.GetSingleton<CA_05_Poppy>().Type;
                    r2[4] = ModContent.GetSingleton<CA_X2_CorpseFlower>().Type;
                    r2[5] = Server.ServerRandom.Next(TankID.Brown, TankID.Marine);
                }

                var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = r2[Server.ServerRandom.Next(0, r2.Length-1)],
                    IsPlayer = false,
                    Team = tank.Team
                };
                tank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                tank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 1);
            }
        }

        public override void OnUnload()
        {
            base.OnUnload();
           
        }
        public override void Shoot(AITank tank, ref Shell shell)
        {
            base.Shoot(tank, ref shell);

        }
    }
}
