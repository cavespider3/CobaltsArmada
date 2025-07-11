using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Net;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.Coordinates;
using System.Diagnostics.CodeAnalysis;
using TanksRebirth.Internals.Common;
using TanksRebirth.GameContent.RebirthUtils;
using Microsoft.Xna.Framework.Input;
using CobaltsArmada.Objects.projectiles.futuristic;
using CobaltsArmada.Hooks;
using TanksRebirth.Internals;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Fluids;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.UI.LevelEditor;
using WiimoteLib;


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


    

    [AllowNull]
    public static Model Neo_Stationary;
    [AllowNull]
    public static Model Neo_Mobile;
    [AllowNull]
    public static Model Neo_Boss;
    [AllowNull]
    public static Model Shell_Beam;
    [AllowNull]
    public static Model Shell_Glaive;

    [AllowNull]
    public static Texture2D Beam;
    [AllowNull]
    public static Texture2D Beam_Dan;


    [AllowNull]
    public static Texture2D Tank_Y1;



    public static BossBar? boss;
    public static VindicationTimer? MissionDeadline;

    public static float KudzuRegen=0f; 
    
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
                return new(0, 191, 71);
            case Tanktosis.Double:
                return new(0, 113, 226);
            case Tanktosis.Triple:
                return new(0, 18, 225);
            case Tanktosis.Quad:
                return new(179, 0, 179);
            default:
                return new(120, 120, 120);
        }
    }


    public static float Dif_Scalar_1()
    {
        return  modifier_Difficulty > ModDifficulty.Easy ?
                modifier_Difficulty > ModDifficulty.Normal ?
                modifier_Difficulty > ModDifficulty.Hard ?
                modifier_Difficulty > ModDifficulty.Lunatic ?
                modifier_Difficulty > ModDifficulty.Extra ?
                3f : 2.5f : 2f : 1.5f : 1f : 0.5f;
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

        Shell.OnDestroy += Shell_OnDestroy;
        SceneManager.OnMissionCleanup += SceneManager_OnMissionCleanup;
 
       
        Mine.OnExplode += Mine_OnExplode;
        GameHandler.OnPostRender += GameHandler_OnPostRender;
        GameHandler.OnPostUpdate += GameHandler_OnPostUpdate;

        CampaignGlobals.OnMissionStart += GameProperties_OnMissionStart;

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
        if (Difficulties.Types["CobaltArmada_TanksOnCrack"])
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: 0.5f, gameplaySound: true);
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
                if (ai is null) continue;
                if (ai is null || ai.Dead || ai.AiTankType == ModContent.GetSingleton<CA_Y2_NightShade>().Type) continue;

              
                if (CA_Y2_NightShade.PoisonedTanks.Find(x => x == ai) is null)
                {
                    CA_Y2_NightShade.PoisonedTanks.Add(ai);
                    CA_Y2_NightShade.Tank_OnPoisoned(ai);
                }
            }
            
        }

        if (Difficulties.Types["CobaltArmada_Mitosis"])
        {
            Tank[] tanks = GameHandler.AllTanks;
            for (int i = tanks.Length-1; i >= 0 ; i--)
            {
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
                if (ai is null || ai.Dead) continue;
                ai.Scaling *= 1f - (float)modifier_Tanktosis * 0.1f;
                for (int j = 1; j < (int)modifier_Tanktosis; j++)
                {
                    var t = new AITank(2);
                    t.Swap(ai.AiTankType);
                    t.InitModelSemantics();
                    t.Body.Position = ai.Body.Position;
                    
                    t.Scaling *= 1f - (float)modifier_Tanktosis * 0.1f;
                    t.Team = ai.Team;
                    t.Properties = ai.Properties;
                    if (CA_Y2_NightShade.PoisonedTanks.Find(x => x == ai) is not null)
                    {
                        CA_Y2_NightShade.PoisonedTanks.Add(t);
                        CA_Y2_NightShade.Tank_OnPoisoned(t);
                    }
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
                var t = new AITank(ModContent.GetSingleton<CA_X3_ForgetMeNot>().Type);

                t.InitModelSemantics();
                t.Body.Position = ai.Body.Position;
                t.Team = ai.Team;
                if (CA_Y2_NightShade.PoisonedTanks.Find(x => x == ai) is not null)
                {
                    CA_Y2_NightShade.PoisonedTanks.Add(t);
                    CA_Y2_NightShade.Tank_OnPoisoned(t);
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
                if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
                if (ai is null) continue;
                CA_Y2_NightShade.WhilePoisoned_Update(ai);

            }
            if (InputUtils.KeyJustPressed(Keys.Y) && DebugManager.DebuggingEnabled) CA_Y2_NightShade.SpawnPoisonCloud(!CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position);
            
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

        if(LevelEditorUI.Active)
        {
            //To CobaltTanks
            if (InputUtils.AreKeysJustPressed([Keys.C, Keys.OemPeriod]) && !DebugManager.DebuggingEnabled && Difficulties.Types["CobaltArmada_Swap"])
            {
                SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.5f, pitchOverride: -0.25f, gameplaySound: true);
                ref Tank[] tanks = ref GameHandler.AllTanks;
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                    var ai = tanks[i] as AITank;
                    if (ai is null) continue;
                        switch (ai.AiTankType)
                        {
                            case TankID.Brown: ai.Swap(ModContent.GetSingleton<CA_01_Dandelion>().Type, true); break;
                            case TankID.Ash: ai.Swap(ModContent.GetSingleton<CA_02_Perwinkle>().Type, true); break;
                            case TankID.Marine: ai.Swap(ModContent.GetSingleton<CA_03_Pansy>().Type, true); break;
                            case TankID.Yellow: ai.Swap(ModContent.GetSingleton<CA_04_Sunflower>().Type, true); break;
                            case TankID.Pink: ai.Swap(ModContent.GetSingleton<CA_05_Poppy>().Type, true); break;
                            case TankID.Violet: ai.Swap(ModContent.GetSingleton<CA_07_Lavender>().Type, true); break;
                            case TankID.Green: ai.Swap(ModContent.GetSingleton<CA_06_Daisy>().Type, true); break;
                            case TankID.White: ai.Swap(ModContent.GetSingleton<CA_08_Eryngium>().Type, true); break;
                            case TankID.Black: ai.Swap(ModContent.GetSingleton<CA_09_Carnation>().Type, true); break;
                        //For special tanks, use master mod tanks
                            case TankID.Bronze: ai.Swap(ModContent.GetSingleton<CA_X1_Kudzu>().Type, true); break;
                            case TankID.Silver: ai.Swap(ModContent.GetSingleton<CA_X2_CorpseFlower>().Type, true); break;
                            case TankID.Sapphire: ai.Swap(ModContent.GetSingleton<CA_Y3_Peony>().Type, true); break;
                            case TankID.Obsidian: ai.Swap(ModContent.GetSingleton<CA_Z9_Hydrangea>().Type, true); break;

                        default: break;

                        }   
                    ai.InitModelSemantics();
                }
            }
            //From CobaltTanks
            else if (InputUtils.AreKeysJustPressed([Keys.C, Keys.OemComma]) && !DebugManager.DebuggingEnabled && Difficulties.Types["CobaltArmada_Swap"])
            {
                SoundPlayer.PlaySoundInstance("Assets/sounds/mine_place.ogg", SoundContext.Effect, 0.5f, pitchOverride: -0.25f, gameplaySound: true);
                ref Tank[] tanks = ref GameHandler.AllTanks;
                for (int i = 0; i < tanks.Length; i++)
                {
                    if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                    var ai = tanks[i] as AITank;
                    if (ai is null) continue;

                    if(ModContent.GetSingleton<CA_01_Dandelion>().Type==ai.AiTankType) ai.Swap(TankID.Brown, true);
                    if(ModContent.GetSingleton<CA_02_Perwinkle>().Type==ai.AiTankType) ai.Swap(TankID.Ash, true);
                    if(ModContent.GetSingleton<CA_03_Pansy>().Type==ai.AiTankType) ai.Swap(TankID.Marine, true);
                    if(ModContent.GetSingleton<CA_04_Sunflower>().Type==ai.AiTankType) ai.Swap(TankID.Yellow, true);
                    if(ModContent.GetSingleton<CA_05_Poppy>().Type==ai.AiTankType) ai.Swap(TankID.Pink, true);
                    if(ModContent.GetSingleton<CA_06_Daisy>().Type==ai.AiTankType) ai.Swap(TankID.Green, true);
                    if(ModContent.GetSingleton<CA_07_Lavender>().Type==ai.AiTankType) ai.Swap(TankID.Violet, true);
                    if(ModContent.GetSingleton<CA_08_Eryngium>().Type==ai.AiTankType) ai.Swap(TankID.White, true);
                    if(ModContent.GetSingleton<CA_09_Carnation>().Type==ai.AiTankType) ai.Swap(TankID.Black, true);

                    if(ModContent.GetSingleton<CA_X1_Kudzu>().Type==ai.AiTankType) ai.Swap(TankID.Bronze, true);
                    if(ModContent.GetSingleton<CA_X2_CorpseFlower>().Type==ai.AiTankType) ai.Swap(TankID.Silver, true);
                    if (ModContent.GetSingleton<CA_Y3_Peony>().Type == ai.AiTankType) ai.Swap(TankID.Sapphire, true);
                    if (ModContent.GetSingleton<CA_Z9_Hydrangea>().Type==ai.AiTankType) ai.Swap(TankID.Obsidian, true);

                    ai.InitModelSemantics();
                    ai.Scaling = Vector3.One;
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
    //Hijack mission
    private void Campaign_OnPreLoadTank(ref TankTemplate template)
    {
        if (LevelEditorUI.Active) return;
        if (MainMenuUI.Active) return;
        if (template.IsPlayer) return;
        if (!Difficulties.Types["CobaltArmada_Swap"]) return;

        TankGame.ClientLog.Write("Invading campaign...", LogType.Info);

        float secret_tank_chance = (float)CampaignGlobals.LoadedCampaign.CurrentMissionId / CampaignGlobals.LoadedCampaign.CachedMissions.Length;
        var nextFloat = Server.ServerRandom.NextFloat(0, 1);

        if (nextFloat <= float.Lerp(0, 0.075f, secret_tank_chance) * (1 + secret_tank_chance / 2f))
        {
            TankGame.ClientLog.Write("RARE TANK GO!", LogType.Info);

            if (Server.ServerRandom.NextFloat(0, 1) < 0.25)
                template.AiTier = ModContent.GetSingleton<CA_X1_Kudzu>().Type;
            else
                template.AiTier = ModContent.GetSingleton<CA_X2_CorpseFlower>().Type;
        }
        else
        {
            switch (template.AiTier)
            {
                case TankID.Brown: template.AiTier = ModContent.GetSingleton<CA_01_Dandelion>().Type; break;
                case TankID.Ash: template.AiTier = ModContent.GetSingleton<CA_02_Perwinkle>().Type; break;
                case TankID.Marine: template.AiTier = ModContent.GetSingleton<CA_03_Pansy>().Type; break;
                case TankID.Yellow: template.AiTier = ModContent.GetSingleton<CA_04_Sunflower>().Type; break;
                case TankID.Pink: template.AiTier = ModContent.GetSingleton<CA_05_Poppy>().Type; break;
                case TankID.Violet: template.AiTier = ModContent.GetSingleton<CA_07_Lavender>().Type; break;
                case TankID.Green: template.AiTier = ModContent.GetSingleton<CA_06_Daisy>().Type; break;
                case TankID.White: template.AiTier = ModContent.GetSingleton<CA_08_Eryngium>().Type; break;
                case TankID.Black: template.AiTier = ModContent.GetSingleton<CA_09_Carnation>().Type; break;
                default: break;

            }
        }
    }




    private void Mine_OnExplode(Mine mine)
    {
        if (mine.Owner is PlayerTank||mine.Owner is null) return;
        AITank ai = (AITank)mine.Owner;
        var Dandelion = ModContent.GetSingleton<CA_01_Dandelion>();
        var Peri = ModContent.GetSingleton<CA_02_Perwinkle>();
        var Pansy = ModContent.GetSingleton<CA_03_Pansy>();
        var Sunny = ModContent.GetSingleton<CA_04_Sunflower>();
        var Poppy = ModContent.GetSingleton<CA_05_Poppy>();
        var Daisy = ModContent.GetSingleton<CA_06_Daisy>();
        var Lavi = ModContent.GetSingleton<CA_07_Lavender>();
        var Eryn = ModContent.GetSingleton<CA_08_Eryngium>();
        var Carnation = ModContent.GetSingleton<CA_09_Carnation>();
        var Kudzu = ModContent.GetSingleton<CA_X1_Kudzu>();
        var Corpse = ModContent.GetSingleton<CA_X2_CorpseFlower>();
        
        if (ai.AiTankType == Sunny.Type)
            Fire_AbstractShell_Mine(mine, 8, 1, 0, 4f);

    }



    private void Shell_OnDestroy(Shell shell, Shell.DestructionContext context)
    {
        if (context == Shell.DestructionContext.WithShell || context == Shell.DestructionContext.WithExplosion || context == Shell.DestructionContext.WithMine) return;
        if (shell.Owner is null) return;
        if (shell.Owner is PlayerTank) return;

        AITank ai = (AITank)shell.Owner;
        var Dandelion = ModContent.GetSingleton<CA_01_Dandelion>();
        var Peri = ModContent.GetSingleton<CA_02_Perwinkle>();
        var Pansy = ModContent.GetSingleton<CA_03_Pansy>();
        var Sunny = ModContent.GetSingleton<CA_04_Sunflower>();
        var Poppy = ModContent.GetSingleton<CA_05_Poppy>();
        var Daisy = ModContent.GetSingleton<CA_06_Daisy>();
        var Lavi = ModContent.GetSingleton<CA_07_Lavender>();
        var Eryn = ModContent.GetSingleton<CA_08_Eryngium>();
        var Carnation = ModContent.GetSingleton<CA_09_Carnation>();
        var Kudzu = ModContent.GetSingleton<CA_X1_Kudzu>();
        var Corpse = ModContent.GetSingleton<CA_X2_CorpseFlower>();

        if ((ai.AiTankType == Sunny.Type && shell.Type == ShellID.Rocket))
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