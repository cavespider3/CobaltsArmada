using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using CobaltsArmada.Script.Tanks;

namespace CobaltsArmada
{
    public class CA_Z9_Hydrangea: CA_ArmadaTank
    {
        public override bool HasSong => false;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Hydrangea"
        });

        public override string Texture => "assets/textures/tank_zenith";
        public override int Songs => 1;
        public override Color AssociatedColor => Color.Cyan;


        public override void PostApplyDefaults()
        {
            

            //TANK NO BACK DOWN
            base.PostApplyDefaults();
           
            AITank.Model = CA_Main.Neo_Boss;
            AITank.Scaling = Vector3.One * 1.4f;

            AITank.Parameters.MaxAngleRandomTurn = MathHelper.ToRadians(30);
            AITank.Parameters.RandomTimerMinMove = 10;
            AITank.Parameters.RandomTimerMaxMove = 10;
            AITank.Parameters.TurretMovementTimer = 20;
            AITank.Parameters.TurretSpeed = 0.06f;
            AITank.Parameters.AimOffset = MathHelper.ToRadians(3);
            


            AITank.Parameters.AggressivenessBias = 0.3f;
          

            AITank.Parameters.AwarenessHostileShell = 30;
            AITank.Parameters.AwarenessFriendlyShell = 40;
            AITank.Parameters.AwarenessHostileMine = 0;
            AITank.Parameters.AwarenessFriendlyMine = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21); 
            AITank.SpecialBehaviors[2].Value = Difficulties.Types["RandomizedTanks"] ? 7 : 31;
            AITank.Properties.Armor = new TankArmor(AITank,1);

            CA_Main.boss = new BossBar(AITank, "Hydrangea", "The Unbounded");
            AITank.Properties.Armor.HideArmor = true;
            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 150;
            AITank.Properties.ShellLimit = 9;
            AITank.Properties.ShellSpread = 0.3f;
            AITank.Properties.ShellShootCount = 3;
            AITank.Properties.ShellSpeed = 4f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;

   

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;
            AITank.Properties.CanLayTread = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.7f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.Parameters.ObstacleAwarenessMovement = 74;
            AITank.Parameters.ObstacleAwarenessMine = 15;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (AITank.SpecialBehaviors[2].Value > 1 && AITank.Properties.Armor is not null)
            {
                AITank.Properties.Armor.HitPoints = 1;
                if (context.Source is AITank && context is not TankHurtContextExplosion) return;
                AITank.SpecialBehaviors[2].Value -= 1f;
            }
            base.TakeDamage(destroy, context);

        }


        public override void PostUpdate()
        {
           

            base.PostUpdate();

            AITank.Model.Root.Transform = Matrix.CreateScale(100f) * AITank.Model.Root.Transform;
            if (LevelEditorUI.IsActive || AITank.IsDestroyed || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            AITank.SpecialBehaviors[0].Value -= RuntimeData.DeltaTime;
            //if(AITank.SpecialBehaviors[2].Value < 17f&& AIManager.CountAll(x => x.AiTankType == ModContent.GetSingleton<CA_08_Eryngium>().Type) < 1)
            //{
            //    var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());

            //    var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
            //    crate.TankToSpawn = new TankTemplate()
            //    {
            //        AiTier = ModContent.GetSingleton<CA_08_Eryngium>().Type,
            //        IsPlayer = false,
            //        Team = AITank.Team
            //    };
            //}
            if (AITank.SpecialBehaviors[0].Value < 0f && AITank.SpecialBehaviors[1].Value == 0f)
            {
              if(AIManager.CountAll(x => x.AiTankType == ModContent.GetSingleton<CA_08_Eryngium>().Type) < 1)
                {
                    var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());

                    var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
                    crate.TankToSpawn = new TankTemplate()
                    {
                        AiTier = ModContent.GetSingleton<CA_08_Eryngium>().Type,
                        IsPlayer = false,
                        Team = AITank.Team
                    };
                    AITank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    AITank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                else
                {
                    AITank.SpecialBehaviors[1].Value = 1f;
                }
            }

            if (AITank.SpecialBehaviors[0].Value <= 0f && AITank.SpecialBehaviors[1].Value == 1f)
            {
                AITank.Speed = 0;
                if (AITank.SpecialBehaviors[0].Value < -240f)
                {
                    AITank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    AITank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                float a = MathF.Floor(MathF.Abs(AITank.SpecialBehaviors[0].Value) / 30f) * MathHelper.ToRadians(25f);
                if (MathF.Floor(MathF.Abs(AITank.SpecialBehaviors[0].Value)) % 30==0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var pathPos = new Vector2(0, 40 + MathF.Abs(AITank.SpecialBehaviors[0].Value / 30) * 40f).Rotate(MathHelper.TwoPi*(i/3f) - a);
                        new CA_OrbitalStrike(pathPos + AITank.Position, AITank);


                    }
                    var ring = GameHandler.Particles.MakeParticle(AITank.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    ring.Scale = new(0.6f);
                    ring.Roll = MathHelper.PiOver2;
                    ring.HasAdditiveBlending = true;
                    ring.Color = Color.Cyan;
                   
                    ring.UniqueBehavior = (a) =>
                    {
                        ring.Alpha -= 0.06f * RuntimeData.DeltaTime;

                        GeometryUtils.Add(ref ring.Scale, (0.03f) * RuntimeData.DeltaTime);
                        if (ring.Alpha <= 0)
                            ring.Destroy();
                    };
                }
            
            }

            if (AITank.SpecialBehaviors[0].Value < 0f && AITank.SpecialBehaviors[1].Value >= 3f)
            {
                AITank.Speed = 0;
                if (AITank.SpecialBehaviors[0].Value < -240f)
                {
                    AITank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                    AITank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 4);
                }
                
                if (MathF.Floor(MathF.Abs(AITank.SpecialBehaviors[0].Value)) % 30 == 0)
                {

                    float a = MathF.Floor(MathF.Abs(AITank.SpecialBehaviors[0].Value) / 30f)*MathHelper.ToRadians(25f);
                    for (int i = 0; i < 3; i++)
                    {
                        var pathPos = new Vector2(0,40+ MathF.Abs(AITank.SpecialBehaviors[0].Value / 30 )* 40f).Rotate(MathHelper.TwoPi * (i / 3f)+ a);
                        new CA_OrbitalStrike(pathPos+AITank.Position, AITank);
                    }
                    var ring = GameHandler.Particles.MakeParticle(AITank.Position3D + Vector3.UnitY * 0.01f, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/ring"));
                    ring.Scale = new(0.6f);
                    ring.Roll = MathHelper.PiOver2;
                    ring.HasAdditiveBlending = true;
                    ring.Color = Color.Cyan;

                    ring.UniqueBehavior = (a) =>
                    {
                        ring.Alpha -= 0.06f * RuntimeData.DeltaTime;

                        GeometryUtils.Add(ref ring.Scale, (0.03f) * RuntimeData.DeltaTime);
                        if (ring.Alpha <= 0)
                            ring.Destroy();
                    };
                }
            }

            if (AITank.SpecialBehaviors[0].Value < 0f && AITank.SpecialBehaviors[1].Value == 2f)
            {
                var r = RandomUtils.PickRandom(PlacementSquare.Placements.Where(x => x.BlockId == -1).ToArray());
                var r2 = new int[AITank.SpecialBehaviors[2].Value < 17f ? 8 : 4];
                r2[0] = ModContent.GetSingleton<CA_02_Perwinkle>().Type;
                r2[1] = ModContent.GetSingleton<CA_03_Pansy>().Type;
                r2[2] = ModContent.GetSingleton<CA_01_Dandelion>().Type;
                r2[3] = ModContent.GetSingleton<CA_X3_ForgetMeNot>().Type;
                if (AITank.SpecialBehaviors[2].Value < 17f) {
                    r2[4] = ModContent.GetSingleton<CA_05_Poppy>().Type;
                    r2[5] = ModContent.GetSingleton<CA_X4_Allium>().Type;
                    r2[7] = ModContent.GetSingleton<CA_X5_LilyValley>().Type;
                    r2[6] = Server.ServerRandom.Next(TankID.Brown, TankID.Marine);
                }

                var crate = Crate.SpawnCrate(r.Position + new Vector3(0, 500, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = r2[Server.ServerRandom.Next(0, r2.Length-1)],
                    IsPlayer = false,
                    Team = AITank.Team
                };
                AITank.SpecialBehaviors[0].Value = Server.ServerRandom.NextFloat(200, 400);
                AITank.SpecialBehaviors[1].Value = Server.ServerRandom.Next(0, 1);
            }
        }

    }
}
