using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.GameMechanics;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.UI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Framework;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using static CobaltsArmada.CA_Main;
//Boss AITank
namespace CobaltsArmada
{
    public class CA_Y2_NightShade : ModTank
    {
        /// <summary>
        /// The 2nd boss AITank you fight, fought and rematched at mission's 40 and 97(only on extra and above).
        /// function, while active, this AITank calls for backup. any tanks closeby are poisioned, becoming much more aggressive
        /// </summary
        public override int Songs => 2;
        public override bool HasSong => true;
        public override LocalizedString Name => new(new()
        {
            [LangCode.English] = "Nightshade"
        });


        public override string Texture => "assets/textures/tank_nightshade";

        public override Color AssociatedColor => Color.DarkViolet;

       
        public override void OnLoad()
        {
            base.OnLoad();
        }

       

        public override void PostApplyDefaults()
        {
            //TANK NO BACK DOWN
            Array.Resize(ref AITank.SpecialBehaviors, AITank.SpecialBehaviors.Length + 4);

            base.PostApplyDefaults();

            AITank.SpecialBehaviors[6] = new() { Value = 30 };
            AITank.SpecialBehaviors[5] = new() { Value = 0 };
            AITank.SpecialBehaviors[4] = new() { Value = 0 };
            AITank.SpecialBehaviors[3] = new() { Value = 0 };
            AITank.SpecialBehaviors[2].Value = Difficulties.Types["RandomizedTanks"] ? 5 : AITank.SpecialBehaviors[6].Value;
            AITank.Properties.Armor = new TankArmor(AITank, 1);
            AITank.Properties.Armor.HideArmor = true;
            CA_Main.boss = new BossBar(AITank, "Nightshade", "The Infector");

            AITank.SpecialBehaviors[3].Value =
           CA_Main.modifier_Difficulty > ModDifficulty.Normal ?
           CA_Main.modifier_Difficulty > ModDifficulty.Lunatic ?
           CA_Main.modifier_Difficulty > ModDifficulty.Extra ?
            150f : 105f : 75f : 60f;

            AITank.Model = CA_Main.Neo_Boss;
            AITank.Scaling = Vector3.One * 100f * 1.03f;

            AITank.AiParams.MeanderAngle = MathHelper.ToRadians(30);
            AITank.AiParams.MeanderFrequency = 10;
            AITank.AiParams.TurretMeanderFrequency = 20;
            AITank.AiParams.TurretSpeed = 0.06f;
            AITank.AiParams.AimOffset = MathHelper.ToRadians(3);

            AITank.AiParams.Inaccuracy = 0.6f;

            AITank.AiParams.PursuitLevel = 1f;
            AITank.AiParams.PursuitFrequency = 20;

            AITank.AiParams.ProjectileWarinessRadius_PlayerShot = 0;
            AITank.AiParams.ProjectileWarinessRadius_AIShot = 40;
            AITank.AiParams.MineWarinessRadius_PlayerLaid = 0;
            AITank.AiParams.MineWarinessRadius_AILaid = 50;

            AITank.Properties.TurningSpeed = 0.09f;
            AITank.Properties.MaximalTurn = MathHelper.ToRadians(21);

            AITank.Properties.ShootStun = 12;
            AITank.Properties.ShellCooldown = 250;
            AITank.Properties.ShellLimit = 3;
            AITank.Properties.ShellSpeed = 2.7f;
            AITank.Properties.ShellType = ShellID.Standard;
            AITank.Properties.RicochetCount = 0;

            AITank.AiParams.ShootChance = 0.8f;

            AITank.Properties.Invisible = false;
            AITank.Properties.Stationary = false;

            AITank.Properties.TreadVolume = 0.1f;
            AITank.Properties.TreadPitch = 0.3f;
            AITank.Properties.MaxSpeed = 1.5f;

            AITank.Properties.Acceleration = 0.1f;

            AITank.Properties.MineCooldown = 0;
            AITank.Properties.MineLimit = 0;
            AITank.Properties.MineStun = 0;

            AITank.AiParams.BlockWarinessDistance = 44;
        }
        public override void TakeDamage(bool destroy, ITankHurtContext context)
        {
            if (AITank.SpecialBehaviors[2].Value > 0 && AITank.Properties.Armor is not null)
            {
                AITank.Properties.Armor.HitPoints = Math.Min((int)AITank.SpecialBehaviors[2].Value, 1);
                AITank.SpecialBehaviors[2].Value -= 1f;
          
            }
            base.TakeDamage(destroy, context);

        }

       

        public override void PostUpdate()
        {
          
            base.PostUpdate();
            if (LevelEditorUI.Active || AITank.Dead || !GameScene.ShouldRenderAll || !CampaignGlobals.InMission) return;
            bool Enraged = AITank.SpecialBehaviors[6].Value/2f>= AITank.SpecialBehaviors[2].Value;
            Vector2 smokey = Vector2.One.Rotate(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * Client.ClientRandom.NextFloat(0.1f, AITank.SpecialBehaviors[3].Value);
            var smoke = GameHandler.Particles.MakeParticle(AITank.Position3D + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
            smoke.Pitch = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
            smoke.Scale = new(0.8f * Client.ClientRandom.NextFloat(0.1f, 1f));
            smoke.Color = Color.DarkViolet;
            smoke.HasAddativeBlending = false;
            smoke.UniqueBehavior = (part) => {

                GeometryUtils.Add(ref part.Scale, -0.004f * RuntimeData.DeltaTime);
                part.Position += Vector3.UnitY * 1f * RuntimeData.DeltaTime;
                part.Alpha -= 0.04f * RuntimeData.DeltaTime;

                if (part.Alpha <= 0)
                    part.Destroy();

            };

            if (LevelEditorUI.Active) return;



            AITank.SpecialBehaviors[0].Value += RuntimeData.DeltaTime;
            if (AITank.SpecialBehaviors[1].Value == 0)
                AITank.SpecialBehaviors[1].Value = 5;

            if (AITank.SpecialBehaviors[0].Value > AITank.SpecialBehaviors[1].Value)
            {
                AITank.SpecialBehaviors[1].Value = 0f;
                AITank.SpecialBehaviors[0].Value = 0f;
                ref Tank[] tanks = ref GameHandler.AllTanks;
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i] is Tank ai)
                    {

                        if (ai.Dead || AITank == ai || ai.Team != AITank.Team && AITank.Team != TeamID.NoTeam ||
                            ai is AITank ai2 && (ai2.AiTankType == NightShade || ai2.AiTankType == Lily)) continue;

                        if (Vector2.Distance(ai.Position, AITank.Position3D.FlattenZ()) > AITank.SpecialBehaviors[3].Value) continue;
                        PoisonTank(ai);
                    }
                }
            }

            AITank.SpecialBehaviors[4].Value += RuntimeData.DeltaTime;
            if (AITank.SpecialBehaviors[5].Value == 0)
                AITank.SpecialBehaviors[5].Value = 900;

            if (AITank.SpecialBehaviors[4].Value > AITank.SpecialBehaviors[5].Value)
            {
                AITank.SpecialBehaviors[5].Value = 0f;
                AITank.SpecialBehaviors[4].Value = 0f;
                //Check to see if within bounds
                if (AITank.Position.X != Math.Clamp(AITank.Position.X, GameScene.MIN_X, GameScene.MAX_X) && AITank.Position.Y != Math.Clamp(AITank.Position.Y, GameScene.MIN_Z, GameScene.MAX_Z)) return;
                int[] Pool;
                switch (CA_Main.modifier_Difficulty)
                {
                    default:Pool = !Enraged?
                            [TankID.Brown,TankID.Ash,TankID.Marine,TankID.Pink]:
                            [TankID.Brown,TankID.Ash,TankID.Marine,TankID.Pink,TankID.Violet,TankID.Green]; break;
                }

                var crate = Crate.SpawnCrate(AITank.Position3D + new Vector3(0, 20, 0), 2f);
                crate.TankToSpawn = new TankTemplate()
                {
                    AiTier = Pool[Server.ServerRandom.Next(0, Pool.Length - 1)],
                    IsPlayer = false,
                    Team = AITank.Team
                };
            }


        }
        

     


    }
}
