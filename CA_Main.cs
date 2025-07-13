using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;

using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Net;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.Coordinates;

using TanksRebirth.Internals.Common;
using TanksRebirth.GameContent.RebirthUtils;
using Microsoft.Xna.Framework.Input;
using CobaltsArmada.Objects.projectiles.futuristic;
using CobaltsArmada.Hooks;
using TanksRebirth.Internals;

using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.UI.LevelEditor;

using TanksRebirth.Enums;
using TanksRebirth.GameContent.Globals.Assets;
using TanksRebirth.Graphics;
using Octokit;
using System.Threading.Tasks;


namespace CobaltsArmada;

public class CA_Main : TanksMod {
    //Asset loading handled by this
    /*

 Tier 1
 Brown - > Taraxacum (Dandelion)
 Special:  Death shell burst (6, ricochet once), Stationary

 Teal - > Pansy
 Special: Missles

 Tier 2:
 Yellow - > Sunflower
 Special Gimmick: frag mines

 Red - > Rose
 Special: frag missles

 Green - > Daisy
 Special: Death Mine, Stationary, Ricocet frags
 
 Tier 3:
 Purple - > Lavandula (Lavender)
 Special: frag missles, death shell burst

 White - > Eryngium (Sea Holly)
 Special: Invisible

 Black - > Dianthus caryophyllus (Carnation)
 Special: Hell

 Special A - > Kudzu
 Special: Duplication

 Special B -> Corpse Flower
 Special: Death Nuke


     */
    public override string Name => "Cobalt's Armada";
    /// <summary>
    /// Lilac Tank Color
    /// </summary>
    static public Color Lilac 
    {
        get { return Color.MediumPurple; }
    }

    /// <summary>
    /// Dandelion Tank Color
    /// </summary>
    static public Color Dandy => Color.Honeydew; 
    

    /// <summary>
    /// Cornflower Tank Color
    /// </summary>
    static public Color Cornflower => Color.CornflowerBlue; 

    /// <summary>
    /// MOOOONNNN MAAAAAANNN Tank Color
    /// </summary>
    static public Color Cosmium => Color.Fuchsia;  // Black Tank

    public static Model? Neo_Stationary;
    public static Model? Neo_Mobile;
    public static Model? Neo_Boss;
    public static Model? Shell_Beam;
    public static Model? Shell_Glaive;

    public static Texture2D? Beam;
    public static Texture2D? Beam_Dan;
    public static Texture2D? Tank_Y1;

    public static BossBar? boss;
    public static VindicationTimer? MissionDeadline;

    public static float KudzuRegen=0f;
    /// <summary>
    /// A list of tanks affected by the nightshade buff
    /// </summary>
    public static List<Tank> PoisonedTanks = new List<Tank>();

    public enum ModDifficulty
    {
        Easy=-1,Normal,Hard,Lunatic,Extra,Phantasm
    }

    public enum Tanktosis
    {
        Single=1,Double,Triple,Quad
    }
   
    public static Tanktosis modifier_Tanktosis = Tanktosis.Single;
    public static ModDifficulty modifier_Difficulty = ModDifficulty.Normal;

    #region TankIds
    #region NormalTanks
    public static int Dandelion => ModContent.GetSingleton<CA_01_Dandelion>().Type;

    public static int Periwinkle => ModContent.GetSingleton<CA_02_Perwinkle>().Type;

    public static int Pansy => ModContent.GetSingleton<CA_03_Pansy>().Type;

    public static int SunFlower => ModContent.GetSingleton<CA_04_Sunflower>().Type;

    public static int Poppy => ModContent.GetSingleton<CA_05_Poppy>().Type;
    public static int Rose => Poppy;

    public static int Daisy => ModContent.GetSingleton<CA_06_Daisy>().Type;

    public static int Lavender => ModContent.GetSingleton<CA_07_Lavender>().Type;

    public static int Eryngium => ModContent.GetSingleton<CA_08_Eryngium>().Type;
    public static int SeaHolly => Eryngium;

    public static int Carnation => ModContent.GetSingleton<CA_09_Carnation>().Type;

    #endregion
    #region SpecialTanks

    public static int Kudzu => ModContent.GetSingleton<CA_X1_Kudzu>().Type;

    public static int CorpseFlower => ModContent.GetSingleton<CA_X2_CorpseFlower>().Type;

    public static int ForgetMeNot => ModContent.GetSingleton<CA_X3_ForgetMeNot>().Type;

    public static int Allium => ModContent.GetSingleton<CA_X4_Allium>().Type;

    public static int Lily => ModContent.GetSingleton<CA_X5_LilyValley>().Type;

    #endregion
    #region BossTanks
    public static int Lotus => ModContent.GetSingleton<CA_Y1_Lotus>().Type;

    public static int NightShade => ModContent.GetSingleton<CA_Y2_NightShade>().Type;

    public static int Peony => ModContent.GetSingleton<CA_Y3_Peony>().Type;

    public static int Orchid => ModContent.GetSingleton<CA_Y4_Orchid>().Type;

    public static int Hydrangea => ModContent.GetSingleton<CA_Z9_Hydrangea>().Type;

    #endregion

    #endregion

    public static Color DifficultyColor(ModDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ModDifficulty.Easy:
                return new(0, 191, 71);
            case ModDifficulty.Normal:
                return new(0, 113, 226);
            case ModDifficulty.Hard:
                return new(0, 18, 225);
            case ModDifficulty.Lunatic:
                return new(179, 0, 179);
            case ModDifficulty.Extra:
                return new(176, 0, 0);
            case ModDifficulty.Phantasm:
                return new(199, 0, 154);
            default:
                return new(120, 120, 120);
        }
    }

    public static Color DifficultyColor(Tanktosis difficulty)
    {
        switch (difficulty)
        {
            case Tanktosis.Single:
                return Color.Red;
            case Tanktosis.Double:
                return Color.Lime;
            case Tanktosis.Triple:
                return Color.Orange;
            case Tanktosis.Quad:
                return Color.Yellow;
            default:
                return new(120, 120, 120);
        }
    }

    public static dynamic GetValueByDifficulty<T>(T Easy, T Normal, T Hard, T Lunatic, T Extra, T Phantasm)
    {
        switch (modifier_Difficulty)
        {
            case ModDifficulty.Easy: default: if (Easy is not null) return Easy; else TankGame.ClientLog.Write("Easy Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Normal: if (Normal is not null) return Normal; else TankGame.ClientLog.Write("Normal Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Hard: if (Hard is not null) return Hard; else TankGame.ClientLog.Write("Hard Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Lunatic: if (Lunatic is not null) return Lunatic; else TankGame.ClientLog.Write("Lunatic Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Extra: if (Extra is not null) return Extra; else TankGame.ClientLog.Write("Extra Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Phantasm: if (Phantasm is not null) return Phantasm; else TankGame.ClientLog.Write("Phantasm Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
        }
        return null;
    }

    public static void SpawnPoisonCloud(Vector3 v, float radius = 60f)
    {
        const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

        SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: 0.5f);
        int length = 23;

        for (int i = 0; i < length; i++)
        {
            Vector2 smokey = Vector2.One.Rotate(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * Client.ClientRandom.NextFloat(0.1f, 60f);
            var smoke = GameHandler.Particles.MakeParticle(v + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
            smoke.Roll = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
            smoke.Scale = new(0.8f * Client.ClientRandom.NextFloat(0.1f, 1f));
            smoke.Color = Color.DarkViolet;
            smoke.HasAddativeBlending = false;
            smoke.UniqueBehavior = (part) => {

                GeometryUtils.Add(ref part.Scale, -0.004f * RuntimeData.DeltaTime);
                part.Position += Vector3.UnitY * 0.2f * RuntimeData.DeltaTime;
                part.Alpha -= 0.04f * RuntimeData.DeltaTime;

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

            if (Vector2.Distance(ai.Position, v.FlattenZ()) > radius) continue;
            bool NotIntoxicated = true;
            if (PoisonedTanks.Find(x => x == ai) is null)
            {
                PoisonedTanks.Add(ai);
                Tank_OnPoisoned(ai);
            }
        }

    }
    public static int FlowerFromBase(int ID)
    {
        switch (ID)
        {
            case TankID.Brown: return Dandelion;
            case TankID.Ash: return Periwinkle;
            case TankID.Marine: return Pansy;
            case TankID.Yellow: return SunFlower;
            case TankID.Pink: return Rose;
            case TankID.Violet: return Lavender;
            case TankID.Green: return Daisy;
            case TankID.White: return Eryngium;
            case TankID.Black: return Carnation;

            case TankID.Bronze: return Kudzu;
            case TankID.Silver: return CorpseFlower;
            case TankID.Sapphire: return ForgetMeNot;
            case TankID.Ruby: return Allium;
            case TankID.Citrine: return Lily;
            case TankID.Amethyst: return Lotus;
            case TankID.Emerald: return NightShade;
            case TankID.Gold: return Peony;
            case TankID.Obsidian: return Hydrangea;

            default: return ID;

        }
    }


    public static void Tank_OnPoisoned(AITank tank)
    {
        const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";
        SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: -0.5f);

        switch (tank.AiTankType)
        {
            default:
                tank.Properties.ShootStun /= 2;
                tank.Properties.ShellCooldown /= 2;
                tank.AiParams.ShootChance *= 1.5f;
                tank.AiParams.MeanderAngle /= 2;
                tank.AiParams.MeanderFrequency /= 2;
                tank.AiParams.TurretMeanderFrequency /= 2;
                tank.AiParams.Inaccuracy /= 1.5f;
                tank.AiParams.TurretSpeed *= 1.75f;
                tank.AiParams.AimOffset /= 2f;
                tank.Properties.MaxSpeed *= 1.25f;
                tank.AiParams.PursuitLevel = MathF.Sign(tank.AiParams.PursuitLevel) * tank.AiParams.PursuitLevel * 1.3f;
                tank.AiParams.PursuitFrequency /= 2;
                if (tank.Properties.Invisible)
                {
                    tank.Properties.CanLayTread = false;
                    tank.Properties.TreadVolume = 0f;
                }
                if (tank.Properties.Stationary && !tank.Properties.Invisible)
                {
                    tank.Properties.Invisible = true;
                    tank.AiParams.SmartRicochets = true;
                    tank.AiParams.PredictsPositions = true;
                    tank.AiParams.ShootChance *= 2;
                    tank.DoInvisibilityGFXandSFX();
                }
                break;
        }
        if (tank.AiTankType == ModContent.GetSingleton<CA_X3_ForgetMeNot>().Type)
        {
            tank.Properties.Armor = new TankArmor(tank, 1);
            tank.Properties.Armor.HideArmor = true;
        }

    }

    public override void OnLoad() {

        Difficulties.Types.Add("CobaltArmada_Swap", false);
        Difficulties.Types.Add("CobaltArmada_GetGud", false);
        Difficulties.Types.Add("CobaltArmada_YouAndWhatArmy", false);
        Difficulties.Types.Add("CobaltArmada_MasterSpark", false);
        Difficulties.Types.Add("CobaltArmada_TanksOnCrack", false);
        Difficulties.Types.Add("CobaltArmada_Mitosis", false);
        Difficulties.Types.Add("CobaltArmada_P2", false);

        Neo_Stationary = ImportAsset<Model>("assets/models/tank_static");
        Neo_Mobile = ImportAsset<Model>("assets/models/tank_moving");
        Neo_Boss = ImportAsset<Model>("assets/models/tank_elite_a");
        Shell_Beam = ImportAsset<Model>("assets/models/laser_beam");
        Shell_Glaive = ImportAsset<Model>("assets/models/bullet_glave");

        Tank_Y1 = ImportAsset<Texture2D>("assets/textures/tank_lotus");
        Beam = ImportAsset<Texture2D>("assets/textures/tank_zenith");
        Beam_Dan = ImportAsset<Texture2D>("assets/textures/tank_dandy");
        
        MainMenuUI.OnMenuOpen += Open;
        MainMenuUI.OnMenuClose += MainMenu_OnMenuClose;
        Campaign.OnPreLoadTank += Campaign_OnPreLoadTank;
        Campaign.OnMissionLoad += Campaign_OnMissionLoad;
        Shell.OnDestroy += Shell_OnDestroy;
        SceneManager.OnMissionCleanup += SceneManager_OnMissionCleanup;    
        Mine.OnExplode += Mine_OnExplode;
        GameHandler.OnPostRender += GameHandler_OnPostRender;
        GameHandler.OnPostUpdate += GameHandler_OnPostUpdate;

        CampaignGlobals.OnMissionStart += GameProperties_OnMissionStart;
        CampaignGlobals.OnMissionEnd += CampaignGlobals_OnMissionEnd;

        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_01_Dandelion>().Type] = 0.14f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_02_Perwinkle>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_03_Pansy>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_04_Sunflower>().Type] = 0.32f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_05_Poppy>().Type] = 0.374f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_06_Daisy>().Type] = 0.43f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_07_Lavender>().Type] = 0.5f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_08_Eryngium>().Type] = 0.63f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_09_Carnation>().Type] = 0.85f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_X1_Kudzu>().Type] = 0.42f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_X2_CorpseFlower>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModContent.GetSingleton<CA_Z9_Hydrangea>().Type] = 1.00f;

        Hook_UI.Load();
        TankGame.PostDrawEverything += TankGame_OnPostDraw;
    }

  

    private void TankGame_OnPostDraw(GameTime obj)
    {
        TankGame.SpriteRenderer.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, rasterizerState: RenderGlobals.DefaultRasterizer);
        for (int i = 0; i < Modifieralert.AllModifiers.Length; i++)
        {
            Modifieralert.AllModifiers[i]?.Render(TankGame.SpriteRenderer, i, Vector2.One, Anchor.LeftCenter);
        }
        TankGame.SpriteRenderer.End();
    }

    public static int Modifiers_currentlyactive;
   
    private void SceneManager_OnMissionCleanup()
    {
        foreach (var pu in CA_OrbitalStrike.AllLasers)
            pu?.Remove();
        foreach (var pu in CA_Idol_Tether.AllTethers)
            pu?.Remove();
       
        foreach (var item in Modifieralert.AllModifiers)
        {
            item?.Remove();
        }
    }


    private void GameProperties_OnMissionStart()
    {
        if (LevelEditorUI.Active || LevelEditorUI.IsTestingLevel)
        {
            if (LevelEditorUI.IsTestingLevel && Difficulties.Types["CobaltArmada_Swap"])
            {
                ref Tank[] tanks = ref GameHandler.AllTanks;
                foreach (Tank _template in tanks)
                {
                    if (_template is AITank template)
                    {
                        template.UsesCustomModel = true;
                        template.Swap(FlowerFromBase(template.AiTankType), true);
                        var t = new AITank(template.AiTankType);
                        t.Body.Position = template.Position3D.FlattenZ() / Tank.UNITS_PER_METER;
                        t.Position = template.Position3D.FlattenZ();
                        t.Dead = false;
                        t.Team = template.Team;
                        template.Remove(true);

                    }
                }


            }
            return;
        }

        if (Difficulties.Types["CobaltArmada_Mitosis"])
        {
            Tank[] tanks = GameHandler.AllTanks;
            for (int i = tanks.Length - 1; i >= 0; i--)
            {
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
                if (ai is null || ai.Dead) continue;
                ai.Scaling *= 1f - (float)modifier_Tanktosis * 0.1f;
                for (int j = 1; j < (int)modifier_Tanktosis; j++)
                {
                    var t = new AITank(2);
                    t.Swap(ai.AiTankType);

                    t.Body.Position = ai.Body.Position;

                    t.Scaling *= 1f - (float)modifier_Tanktosis * 0.1f;
                    t.Team = ai.Team;
                    t.Properties = ai.Properties;

                }
            }

        }

        if (Difficulties.Types["CobaltArmada_P2"])
        {
            Tank[] tanks = GameHandler.AllTanks;

            Tank[] tanks2 = GameHandler.AllTanks.ToList().FindAll(x => x is not null && x is not PlayerTank && !x.Dead).ToArray();
            for (int i = tanks2.Length - 1; i >= 0; i--)
            {
                var ai = tanks2[i] as AITank;
                if (ai is null || ai.Dead) continue;
                var t = new AITank(ForgetMeNot);

                t.Body.Position = ai.Body.Position;
                t.Team = ai.Team;
                
            }
        }

        if (Difficulties.Types["CobaltArmada_TanksOnCrack"])
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: 0.5f);
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
                if (ai is null) continue;
                if (ai is null || ai.Dead || ai.AiTankType == NightShade) continue;

                if (PoisonedTanks.Find(x => x == ai) is null)
                {
                    PoisonedTanks.Add(ai);
                    Tank_OnPoisoned(ai);
                }
            }

        }
    }

  

    private void GameHandler_OnPostUpdate()
    {
        
        KudzuRegen -= RuntimeData.DeltaTime;
        Hook_UI.Hook_UpdateUI();

        if (!IntermissionSystem.IsAwaitingNewMission)
        {
            foreach (var fp in CA_OrbitalStrike.AllLasers)
                fp?.Update();

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is AITank ai)
                {
                    CA_Y2_NightShade.WhilePoisoned_Update(ai);
                }

            }
            if (InputUtils.KeyJustPressed(Keys.Y) && DebugManager.DebuggingEnabled) SpawnPoisonCloud(!CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position);
            
            foreach (var IT in CA_Idol_Tether.AllTethers)
                IT?.Update();

        }
        else
        {
            if(IntermissionSystem.Alpha>=0.8 && Difficulties.Types.Count(diff => diff.Value)!= Modifiers_currentlyactive && !LevelEditorUI.Active)
            {
                Modifiers_currentlyactive = Difficulties.Types.Count(diff => diff.Value);
               // TankGame.ClientLog.Write(Modifiers_currentlyactive + "Modifiers were active",LogType.Info);
                List<string> bops = new() { "$START$" };
                foreach (var item in Difficulties.Types)
                {
                    if (item.Value) bops.Add(item.Key);
                }  
                   
                for (int i = 0;  i < bops.Count; i++)
                {
                    new Modifieralert(bops[i], Color.DarkRed,i == 0?0f:0.1f+i*0.1f);
                }
            }
            

        }

    }



    private void GameHandler_OnPostRender()
    {
        foreach (var fp in CA_OrbitalStrike.AllLasers)
            fp?.Render();
        boss?.Render(TankGame.SpriteRenderer, new(WindowUtils.WindowWidth-WindowUtils.WindowWidth / 4, WindowUtils.WindowHeight-60.ToResolutionY()), new Vector2(300, 20).ToResolution(), Anchor.Center, Color.Black, Color.Red);
        MissionDeadline?.Render(TankGame.SpriteRenderer, new(WindowUtils.WindowWidth - WindowUtils.WindowWidth / 4, WindowUtils.WindowHeight - 60.ToResolutionY()), new Vector2(300, 20).ToResolution(), Anchor.Center, Color.Black, Color.Red);
    }

    private void MainMenu_OnMenuClose()
    {
       
      
    }

    private void Open()
    {
        boss = null;
        MissionDeadline = null;
    }

    private void Campaign_OnMissionLoad(Tank[] tanks, Block[] blocks)
    {
        if (LevelEditorUI.Active) return;
        if (MainMenuUI.Active) return;
        if (!Difficulties.Types["CobaltArmada_Swap"]) return;

        if (IntermissionHandler.LastResult != MissionEndContext.Lose)
        {
            foreach (TankTemplate _template in CampaignGlobals.LoadedCampaign.CachedMissions[CampaignGlobals.LoadedCampaign.CurrentMissionId].Tanks)
            {
                if (_template.IsPlayer) continue;
                FutureSpawns[_template.AiTier] += 1;
            }
        }

    }
    private void CampaignGlobals_OnMissionEnd(int delay, MissionEndContext context, bool result1up)
    {
        if (LevelEditorUI.Active) return;
        if (MainMenuUI.Active) return;
        if (!Difficulties.Types["CobaltArmada_Swap"]) return;

        if (context == MissionEndContext.Win)
        {
            for (int i = 0; i < TankID.Collection.Keys.Length; i++)
            {
                Spawns[i] = FutureSpawns[i];
                PreSpawns[i] = FutureSpawns[i];
            }

        }else if(context == MissionEndContext.Lose)
        {
            for (int i = 0; i < TankID.Collection.Keys.Length; i++)
            {
                Spawns[i] = PreSpawns[i];
            }

        }
    }



    //It's probably overkill and poor coding, but if it works, then it works!
    public static int[] FutureSpawns = Array.Empty<int>();
    public static int[] Spawns = Array.Empty<int>();
    public static int[] PreSpawns = Array.Empty<int>();
    //Hijack mission
    private void Campaign_OnPreLoadTank(ref TankTemplate template)
    {   if (MainMenuUI.Active) return;
        if (LevelEditorUI.Active)
        {
            return;
        }
        
     
       
        if (!Difficulties.Types["CobaltArmada_Swap"]) return;
        //Set up the new swap out process
        if (Spawns.Length == 0)
        {
            Spawns = new int[TankID.Collection.Keys.Length];
            PreSpawns = new int[TankID.Collection.Keys.Length];
            FutureSpawns = new int[TankID.Collection.Keys.Length];
            if (CampaignGlobals.LoadedCampaign.CurrentMissionId == MainMenuUI.MissionCheckpoint)
            {
                // ChatSystem.SendMessage("New Campaign!", Color.Pink);
                for (int i = 0; i < CampaignGlobals.LoadedCampaign.CurrentMissionId; i++)
                {
                    foreach (TankTemplate _template in CampaignGlobals.LoadedCampaign.CachedMissions[i].Tanks)
                    {
                        if (_template.IsPlayer) continue;
                        PreSpawns[_template.AiTier] += 1;
                        Spawns[_template.AiTier] += 1;
                        FutureSpawns[_template.AiTier] += 1;
                    }
                }
            }
        }

        if (template.IsPlayer) return;
        TankGame.ClientLog.Write("Invading campaign...", LogType.Info);
        //New system, anytime a specific tank shows up, add to a counter. When the counter reaches a certain point, replace that tank with a special type
        //old code that does nothing for now
        Spawns[template.AiTier] += 1;
        template.AiTier = FlowerFromBase(template.AiTier);

    }




    private void Mine_OnExplode(Mine mine)
    {
        if (mine.Owner is PlayerTank||mine.Owner is null) return;
        AITank ai = (AITank)mine.Owner;
        if (ai.AiTankType == SunFlower)
            Fire_AbstractShell_Mine(mine, 8, 1, 0, 4f);
    }



    private void Shell_OnDestroy(Shell shell, Shell.DestructionContext context)
    {
        if (context == Shell.DestructionContext.WithShell || context == Shell.DestructionContext.WithExplosion || context == Shell.DestructionContext.WithMine) return;
        if (shell.Owner is null) return;
        if (shell.Owner is PlayerTank) return;

        AITank ai = (AITank)shell.Owner;

        if ((ai.AiTankType == SunFlower && shell.Type == ShellID.Rocket))
           new Mine(shell.Owner, shell.Position - new Vector2(0f, 10f).Rotate(shell.Rotation), 900f, 0.1f);
    }


    public static void Lay_AbstractMine(Shell origin)
    {
        if (!MainMenuUI.Active && !CampaignGlobals.InMission)return;
        Mine mine = new Mine(origin.Owner, origin.Position - new Vector2(0f, 10f).Rotate(origin.Rotation), 400f, 1f);
    }

    /// <summary>
    /// Spawn a shell from somewhere. used with the burst shells
    /// </summary>
    public static void Fire_AbstractShell(Shell origin,int count, int newType = 1, uint burst_bounces = 0,float burst_expand=3.5f)
    {
        if (MainMenuUI.Active || !CampaignGlobals.InMission) return;
        if (origin is null||origin.Owner is null) return;
        float angle = 0;
        float rng_burst = origin.Rotation+ (MathF.PI * 2f / (count*2f));
        for (int i = 0; i < count; i++)
        {
         
                angle =(MathF.PI * 2f / count * i)+ rng_burst;
                float newAngle = angle;
                Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0U, origin.Properties.HomeProperties, true);
                Vector2 new2d2 = Vector2.UnitY.Rotate(newAngle);
                Vector2 newPos2 = origin.Position + new Vector2(0f, 14f).Rotate(-newAngle);
                shell2.Position = new Vector2(newPos2.X, newPos2.Y);
                shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
                shell2.RicochetsRemaining = burst_bounces;
            }
        }

    public static void Fire_AbstractShell_Tank(Tank origin, int count,ITankHurtContext player_kill, int newType = 1, uint burst_bounces=0, float burst_expand = 3.4f)
    {
        if (MainMenuUI.Active || !CampaignGlobals.InMission) return;
        if (origin is null) return;

        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin , 0U, origin.Properties.ShellHoming, false);
            Vector2 new2d2 = Vector2.UnitY.Rotate(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 20f).Rotate(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }
    public static void Fire_AbstractShell_Mine(Mine origin, int count, int newType = 1, uint burst_bounces = 0, float burst_expand = 3.5f)
    {
        if ((!MainMenuUI.Active && !CampaignGlobals.InMission)) return;
        if (origin is null || origin.Owner is null) return;
        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0U, new Shell.HomingProperties(), true);
            Vector2 new2d2 = Vector2.UnitY.Rotate(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 25f).Rotate(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y) * burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }


    public override void OnUnload() {
        MainMenuUI.OnMenuOpen -= Open;
        MainMenuUI.OnMenuClose -= MainMenu_OnMenuClose;
        Campaign.OnPreLoadTank -= Campaign_OnPreLoadTank;

        Shell.OnDestroy -= Shell_OnDestroy;
        SceneManager.OnMissionCleanup -= SceneManager_OnMissionCleanup;


        Mine.OnExplode -= Mine_OnExplode;
        GameHandler.OnPostRender -= GameHandler_OnPostRender;
        GameHandler.OnPostUpdate -= GameHandler_OnPostUpdate;
        CampaignGlobals.OnMissionStart -= GameProperties_OnMissionStart;
        TankGame.PostDrawEverything -= TankGame_OnPostDraw;

        Difficulties.Types.Remove("CobaltArmada_Swap");
        Difficulties.Types.Remove("CobaltArmada_GetGud");
        Difficulties.Types.Remove("CobaltArmada_YouAndWhatArmy");
        Difficulties.Types.Remove("CobaltArmada_MasterSpark");
        Difficulties.Types.Remove("CobaltArmada_TanksOnCrack");
        Difficulties.Types.Remove("CobaltArmada_Mitosis");
        Difficulties.Types.Remove("CobaltArmada_P2");

        MainMenuUI.AllDifficultyButtons.RemoveRange(Hook_UI.startIndex, MainMenuUI.AllDifficultyButtons.Count - Hook_UI.startIndex - 1);
    }
}