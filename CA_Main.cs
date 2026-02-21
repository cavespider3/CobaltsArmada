using CobaltsArmada.Hooks;
using CobaltsArmada.Objects.projectiles.futuristic;
using CobaltsArmada.Script.Objects.hazards;
using CobaltsArmada.Script.Objects.items;
using CobaltsArmada.Script.Tanks;
using CobaltsArmada.Script.Tanks.Class_T;
using CobaltsArmada.Script.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;
using TanksRebirth;
using TanksRebirth.Enums;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.RebirthUtils;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.CommandsSystem;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.GameContent.Systems.TankSystem.AI;
using TanksRebirth.GameContent.UI.LevelEditor;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.Internals.Common.Framework.Interfaces;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Localization;
using TanksRebirth.Net;
using static CobaltsArmada.Script.Tanks.Class_T.DroneParameters;
using static TanksRebirth.GameContent.UI.LevelEditor.LevelEditorUI;

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

    //affects ai
    public const string M_ARMADA = "armda";
    public const string M_LAYERS = "doll_tnks";
    public const string M_NIGHTSHADE = "roided";
    public const string M_MULT = "mitosis_army";
    public const string M_IDOL = "no_forget";
    public const string M_HARDER = "difficulty";

    public const string M_BROKENFACTORY = "scrambled";

    // affects all tanks
    public const string M_ENEMYDRONE = "drn_en";
    public const string M_PLAYERDRONE = "drn_pl";


    // gameplay mixups
    public const string M_INFINITE = "endless_mode";
    public const string M_RAINRISK = "roguelike";

    public static int Modifiers_currentlyactive;


    public static Model? Neo_Stationary;
    public static Model? Neo_Mobile;
    public static Model? Neo_Remote;
    public static Model? Neo_Boss;
    public static Model? Shell_Beam;
    public static Model? Shell_Glaive;

    public static Model? Drone;
    public static Model? Elite_Drone;

    //Ror2 assets used here
    public static OggAudio? Drone_Hover;
    public static OggAudio?[] Drone_Shoot = new OggAudio[6];
    public static OggAudio? Drone_Disable;
    public static OggAudio?[] Drone_Crash = new OggAudio[3];
    public static OggAudio? Drone_Activate;
    public static OggAudio? Drone_Parry;

    public static OggAudio?[] Pickup = new OggAudio[4];
    public static OggAudio? ItemSpawn_Tier1;
    public static OggAudio? ItemSpawn_Tier2;
    public static OggAudio? ItemSpawn_Tier3;

    public static OggAudio? ItemLand_Tier1;
    public static OggAudio? ItemLand_Tier2;
    public static OggAudio? ItemLand_Tier3;


    public static Texture2D? Beam;
    public static Texture2D? Beam_Dan;
    public static Texture2D? Tank_Y1;
    public static Texture2D? Tank_CustomPaint;

    public static BossBar? boss;
    public static VindicationTimer? MissionIsDestroyedline;

    public static float KudzuRegen = 0f;
    /// <summary>
    /// A list of tanks affected by the nightshade buff
    /// </summary>
    public static List<Tank> PoisonedTanks = [];

    //It's probably overkill and poor coding, but if it works, then it works!
    public static int[] FutureSpawns = Array.Empty<int>();
    public static int[] Spawns = Array.Empty<int>();
    public static int[] PreSpawns = Array.Empty<int>();

    public enum ModDifficulty
    {
        Normal, Hard, Lunatic, Extra
    }

    public enum Tanktosis
    {
        Single = 1, Double, Triple, Quad
    }

    public static Tanktosis modifier_Tanktosis = Tanktosis.Single;
    public static ModDifficulty modifier_Difficulty = ModDifficulty.Normal;

    #region TankIds
    #region NormalTanks
    public static int Dandelion => ModRegistry.GetSingleton<CA_01_Dandelion>().Type;

    public static int Periwinkle => ModRegistry.GetSingleton<CA_02_Perwinkle>().Type;

    public static int Pansy => ModRegistry.GetSingleton<CA_03_Pansy>().Type;

    public static int SunFlower => ModRegistry.GetSingleton<CA_04_Sunflower>().Type;

    public static int Poppy => ModRegistry.GetSingleton<CA_05_Poppy>().Type;
    public static int Rose => Poppy;

    public static int Daisy => ModRegistry.GetSingleton<CA_06_Daisy>().Type;

    public static int Lavender => ModRegistry.GetSingleton<CA_07_Lavender>().Type;

    public static int Eryngium => ModRegistry.GetSingleton<CA_08_Eryngium>().Type;
    public static int SeaHolly => Eryngium;

    public static int Carnation => ModRegistry.GetSingleton<CA_09_Carnation>().Type;

    #endregion
    #region SpecialTanks

    public static int Kudzu => ModRegistry.GetSingleton<CA_X1_Kudzu>().Type;

    public static int CorpseFlower => ModRegistry.GetSingleton<CA_X2_CorpseFlower>().Type;

    public static int ForgetMeNot => ModRegistry.GetSingleton<CA_X3_ForgetMeNot>().Type;

    public static int Allium => ModRegistry.GetSingleton<CA_X4_Allium>().Type;

    public static int Lily => ModRegistry.GetSingleton<CA_X5_LilyValley>().Type;

    #endregion
    #region BossTanks
    public static int Lotus => ModRegistry.GetSingleton<CA_Y1_Lotus>().Type;

    public static int NightShade => ModRegistry.GetSingleton<CA_Y2_NightShade>().Type;

    public static int Peony => ModRegistry.GetSingleton<CA_Y3_Peony>().Type;

    public static int Orchid => ModRegistry.GetSingleton<CA_Y4_Orchid>().Type;

    public static int Hydrangea => ModRegistry.GetSingleton<CA_Z9_Hydrangea>().Type;

    #endregion

    #endregion

    #region Rogue-lite game mode

    /// <summary>
    /// A list of items held by players
    /// </summary>
    public static List<RainItem>[] PlayerRainRiskInventory = [[],[],[],[]];

    /// <summary>
    /// Items held by the ai tanks
    /// </summary>
    public static Dictionary<int,List<RainItem>> EnemyRainRiskInventory = [];


    public static Random RogueLikeSeed = new(0);
    public static Random RickyRainSeed = new(0);
    public static bool EndlessModeActive = false;
    public static int DirectorBudget = 0;


    private static Dictionary<TanksMod, List<RainItem>> _modRainItemDictionary = [];
    public static RainItem[] ModRainItems { get; private set; } = [];
    private static List<RainItem> _modRainItems = [];


    public static float[] FakeAITankGravity = new float[GameHandler.MAX_AI_TANKS];
    public static bool[] FakeAITankGrounded= new bool[GameHandler.MAX_AI_TANKS];
    public static float[] FakePlayerTankGravity = new float[GameHandler.MAX_PLAYERS];
    public static bool[] FakePlayerTankGrounded = new bool[GameHandler.MAX_PLAYERS];

    /// <summary>
    /// How the enemy will be chosen to be selected.
    /// </summary>
    public enum SpawnCardPriority
    {
        /// <summary>
        /// If this tank is affordable, then put it in a "potluck" with other affordable tanks.
        /// </summary>
        Random = 0,

        /// <summary>
        /// If this tank is affordable, put it in a "potluck" with other tanks with the same price.
        /// </summary>
        RandomSamePrice = 1,

        /// <summary>
        /// If this tank is affordable, and is the most expensive, then pick it.
        /// </summary>
        MostExpensive = 2,  
    }

    /// <summary>
    /// Used in tandum with the Rogue modifier to determine spawning
    /// </summary>
    public struct SpawnCard
    {
        /// <summary>
        /// How much the <seealso cref="AITank"/> costs to spawn
        /// </summary>
        public int Cost { get; set; } = 1;

        /// <summary>
        /// How common the <seealso cref="AITank"/> will spawn when part of a pool
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// When should this <seealso cref="AITank"/> start spawning
        /// </summary>
        public int MissionThreshold { get; set; } = 0;

        /// <summary>
        /// How much the <seealso cref="AITank"/> costs to be added to the spawn pool
        /// </summary>
        public int PermitCostMultiplier { get; set; } = 2;

        /// <summary>
        /// How much the <seealso cref="AITank"/> actually costs (Multiplies Cost).
        /// Yes, this might make the director go bankrupt
        /// </summary>
        public int CostTax { get; set; } = 1;

        /// <summary>
        /// The selection group for this <seealso cref="AITank"/>
        /// </summary>
        public string Catagory { get; set; } = "Common";

        /// <summary>
        /// The hardcap for how many can be in a mission.
        /// </summary>
        public int PopulationQuantity { get; set; } = -1;

        /// <summary>
        /// The percent for how many can be in a mission.
        /// </summary>
        public float PopulationPercent { get; set; } = -1f;

        /// <summary>
        /// Determines what the system does when picking this <seealso cref="SpawnCard"/>
        /// </summary>
        public SpawnCardPriority Priority { get; set; } = SpawnCardPriority.Random;

        public SpawnCard()
        {
        }
    }


    public static Dictionary<int, SpawnCard> TankCosts = new()
    {
        //Little guys... cheap and common
        [TankID.Brown] = new() { Cost = 1, Weight = 1, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Ash] = new() { Cost = 2, Weight = 1, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Bronze] = new() { Cost = 5, Weight = 2, MissionThreshold = 25, Catagory = "Common" },
        [TankID.Yellow] = new() { Cost = 5, Weight = 4, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Marine] = new() { Cost = 5, Weight = 4, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Silver] = new() { Cost = 10, Weight = 3, MissionThreshold = 25, Catagory = "Common" },
        [TankID.Pink] = new() { Cost = 10, Weight = 8, MissionThreshold = 0, Catagory = "Common" },

        [TankID.Violet] = new() { Cost = 15, Weight = 6, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Sapphire] = new() { Cost = 15, Weight = 2, MissionThreshold = 50, Catagory = "Common" },
        [TankID.Ruby] = new() { Cost = 15, Weight = 2, MissionThreshold = 50, Catagory = "Common" },

        [TankID.White] = new() { Cost = 20, Weight = 3, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Citrine] = new() { Cost = 20, Weight = 3, MissionThreshold = 35, Catagory = "Common" },
        [TankID.Green]  = new() { Cost = 25, Weight = 3, MissionThreshold = 0, Catagory = "Common" },
        
        [TankID.Black]  = new() { Cost = 50, Weight = 2, MissionThreshold = 0, Catagory = "Common" },
        [TankID.Amethyst] = new() { Cost = 50, Weight = 3, MissionThreshold = 60, Catagory = "Common" },
        [TankID.Emerald] = new() { Cost = 50, Weight = 1, MissionThreshold = 60, Catagory = "Common" },
        [TankID.Gold] = new() { Cost = 100, Weight = 1, MissionThreshold = 60, Catagory = "Common" },
        [TankID.Obsidian] = new() { Cost = 100, Weight = 3, MissionThreshold = 75, Catagory = "Common" },
    };
    //The amplified cost required to spawn the enemy with the nightshade buff
    public const float NightShadeEliteCostMultiplier = 16f;
    //The amplified cost required to spawn the enemy with armour
    public const float ArmouredEliteCostMultiplier = 16f;
    //The amplified cost required to spawn the enemy with armour
    public const float DroneEliteCostMultiplier = 16f;


    public static Mission[] MissionPool = [];

    public static float DifficultyCoEff() => MathF.Pow(1.08f, MathF.Max(1, CampaignGlobals.LoadedCampaign.CachedMissions.Length - 1)) * (1 + 1.3f * (GameHandler.ActivePlayerTankCount - 1));

    /// <summary>
    /// Updates the stats of a tank
    /// </summary>
    public static void UpdateItemStats(Tank tank)
    {
        if (tank is null || tank.IsDestroyed) return;
        tank.Properties = new();
        tank.ApplyDefaults(ref tank.Properties);
        
        //then update the defaulted properties
        if (tank is PlayerTank player)
        {
            foreach (var item in PlayerRainRiskInventory[player.PlayerId])
            {
                item.OnStart(ref tank);   
            }
        }
        else if(tank is AITank ai)
        {
            foreach (var item in EnemyRainRiskInventory[ai.AITankId])
            {
                item.OnStart(ref tank);
            }
        }

       
    }

    /// <summary>
    /// Adds an item to a tank's inventory
    /// </summary>
    public static void AddItem(Tank tank,RainItem item,bool SkipStatUpdate = false)
    {
        if (tank is null||!Modifiers.Map[M_RAINRISK] || tank.IsDestroyed) return; //don't give to the dead
        
        static int ComparePriority(RainItem x, RainItem y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal.
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater.
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the
                    // lengths of the two strings.
                    //
                    int retval = x.Priority.CompareTo(y.Priority);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return x.Type.CompareTo(y.Type);
                    }
                }
            }
        }

        if(tank is PlayerTank player)
        {
           if (PlayerRainRiskInventory[player.PlayerId].Any(x => x is not null && x.Type == item.Type))
            {
                PlayerRainRiskInventory[player.PlayerId].Find(x => x is not null && x.Type == item.Type)!.Stacks += 1;
                ChatSystem.SendMessage($"Player'{player.PlayerId}' picked up '{item.Name[LangCode.English]}'('{PlayerRainRiskInventory[player.PlayerId].Find(x => x is not null && x.Type == item.Type)!.Stacks}')!", item.ItemColor);
            }
            else
            {
                item.Stacks = 1;
                PlayerRainRiskInventory[player.PlayerId].Add(item);
                ChatSystem.SendMessage($"Player'{player.PlayerId}' picked up '{item.Name[LangCode.English]}'!",item.ItemColor);
            }
          
            PlayerRainRiskInventory[player.PlayerId].Sort(ComparePriority);
        }
        else if(tank is AITank ai)
        {
            if(EnemyRainRiskInventory.Any(x => x.Key == ai.AITankId))
            {
                if (EnemyRainRiskInventory[ai.AITankId].Any(x => x is not null && x.Type == item.Type))
                {
                    EnemyRainRiskInventory[ai.AITankId].Find(x => x is not null && x.Type == item.Type)!.Stacks += 1;
                    ChatSystem.SendMessage($"'{TankID.Collection.GetKey((tank as AITank).AiTankType)}''{ai.AITankId}' picked up '{item.Name[LangCode.English]}'('{EnemyRainRiskInventory[ai.AITankId].Find(x => x is not null && x.Type == item.Type)!.Stacks}')!", item.ItemColor);
                }
                else
                {
                    item.Stacks = 1;
                    EnemyRainRiskInventory[ai.AITankId].Add(item);

                    ChatSystem.SendMessage($"'{TankID.Collection.GetKey((tank as AITank).AiTankType)}''{ai.AITankId}' picked up '{item.Name[LangCode.English]}'!", item.ItemColor);
                }
                EnemyRainRiskInventory[ai.AITankId].Sort(ComparePriority);
            }
            else
            {
                item.Stacks = 1;
                EnemyRainRiskInventory.Add(ai.AITankId, [item]);
                ChatSystem.SendMessage($"'{TankID.Collection.GetKey((tank as AITank).AiTankType)}''{ai.AITankId}' picked up '{item.Name[LangCode.English]}'!", item.ItemColor);

            }
        }
        //Console.WriteLine(EnemyRainRiskInventory.Count);
       if (!SkipStatUpdate) UpdateItemStats(tank);
    }
    
    /// <summary>
    /// Triggers an item's effects
    /// </summary>
    public void ProcItem(Tank tank,Action<RainItem> method)
    {
        if (!Modifiers.Map[M_RAINRISK]) return;

        if (tank is PlayerTank player && PlayerRainRiskInventory[Math.Max(0,player.PlayerId)].Count > 0)
        {
            foreach (var item in PlayerRainRiskInventory[Math.Max(0, player.PlayerId)])
            {
                method(item);
            } 
        }
        else if(tank is AITank ai)
        {
            if (!EnemyRainRiskInventory.Any(x => x.Key == ai.AITankId)) return;
            foreach (var item in EnemyRainRiskInventory[ai.AITankId])
            {
                method(item);
            }
        }

    }

    private static Mission GetNextMission()
    {
      
        DirectorBudget = (int)(DifficultyCoEff() * 3 + CampaignGlobals.LoadedCampaign.CachedMissions.Length * 2 + (int)MathF.Floor(CampaignGlobals.LoadedCampaign.CachedMissions.Length / 10) * 5 );

        if ((CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1) % 10 == 0) DirectorBudget += 1000;
        else if ((CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1) % 5 == 0) DirectorBudget *= 6;
       

        Console.WriteLine("---- For Mission " + (CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1).ToString() + " ----");
        Console.WriteLine("Starting Budget: " + DirectorBudget.ToString());
        if ((CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1) % 10 == 0) Console.WriteLine("BOSS MISSION!");

        int InitalBudget = DirectorBudget;
        var selectablemissions = MissionPool.Where(x => x.Tanks.Count(tank => !tank.IsPlayer) > (CampaignGlobals.LoadedCampaign.CachedMissions.Length > 4 ? 2 : 1) && x.Tanks.Count(tank => !tank.IsPlayer) <= MathF.Min(MathF.Floor(CampaignGlobals.LoadedCampaign.CachedMissions.Length / 10) + 3, 8)
        && x.Tanks.Count(tank => !tank.IsPlayer) >= MathF.Min(MathF.Max
        (2, MathF.Floor(CampaignGlobals.LoadedCampaign.CachedMissions.Length / 10)), 6)).ToArray();
        if(CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1 == 10)
        {
            selectablemissions = MissionPool.Where(x => x.Tanks.Count(tank => !tank.IsPlayer) <= 2).ToArray();
        }
        //Get the next mission based on a few factors
        var M = selectablemissions[Server.ServerRandom.Next(0, selectablemissions.Length)];



        int SpecialistsMax = (int)Math.Clamp(MathF.Floor((CampaignGlobals.LoadedCampaign.CachedMissions.Length + 1) / 33), 0, 3) + CampaignGlobals.LoadedCampaign.CachedMissions.Length > 8 ? 1 : 0;
        int SpecialistsSelected = 0;
     
        for (int i = 0; i < M.Tanks.Length; i++)
        {
            ref var T = ref M.Tanks[i];
 
            if (T.IsPlayer) continue;
            if (DirectorBudget < 1)
            {
                ChatSystem.SendMessage("POOR AS FUCK", Color.Red);
                var M2 = M.Tanks.ToList();
                M2.Remove(T);
                M.Tanks = M2.ToArray();
                Console.WriteLine("Unable to afford another tank.");
                i -= 1;
            }
            else
            {
                ChatSystem.SendMessage(DirectorBudget.ToString(), Color.CadetBlue);


                int Choice = TankID.Brown;
               
                List<int> weightedChoice = [];

                var Choices = TankCosts.Where(x => x.Value.Catagory == "Common" || (x.Value.Catagory == "Uncommon" && SpecialistsSelected < SpecialistsMax)).ToDictionary();
                foreach (var td in Choices.Keys)
                {
                   
                    if (TankCosts[td].MissionThreshold > CampaignGlobals.LoadedCampaign.CachedMissions.Length ||
                        TankCosts[td].Cost * TankCosts[td].PermitCostMultiplier > DirectorBudget) continue;
                    

                    if (TankCosts[td].Cost == TankCosts[Choice].Cost || TankCosts[td].Priority == SpawnCardPriority.Random)
                    {
                        if (weightedChoice.Count == 0){ for (int j = 0; j < TankCosts[Choice].Weight; j++) { weightedChoice.Add(Choice); }
                    }
                        for (int j = 0; j < TankCosts[td].Weight; j++) weightedChoice.Add(td);
                    }

                    else if(TankCosts[td].Cost > TankCosts[Choice].Cost && TankCosts[td].Priority != SpawnCardPriority.Random)
                    {
                        weightedChoice.Clear();
                        Choice = td;
                        Console.WriteLine(TankID.Collection.GetKey(td)+" is the new most expensive!");
                    }

                }
                if (weightedChoice.Count > 0)
                {
                    Choice = weightedChoice[Client.ClientRandom.Next(0, weightedChoice.Count)];
                }
                T.AiTier = Choice;
                DirectorBudget -= TankCosts[Choice].Cost;
                Console.WriteLine("Purchased: " + TankID.Collection.GetKey(T.AiTier) + " for " + TankCosts[Choice].Cost.ToString() + " Credits.");
                if (TankCosts[Choice].Catagory == "Uncommon") SpecialistsSelected++;
            }


        }
        return M;
    }



    #endregion

   

    public static Color DifficultyColor(ModDifficulty difficulty)
    {
        return difficulty switch
        {
            ModDifficulty.Normal => new(0, 113, 226),
            ModDifficulty.Hard => new(0, 18, 225),
            ModDifficulty.Lunatic => new(179, 0, 179),
            ModDifficulty.Extra => new(176, 0, 0),
            _ => new(120, 120, 120),
        };
    }

    public static Color DifficultyColor(Tanktosis difficulty)
    {
        return difficulty switch
        {
            Tanktosis.Single => Color.Red,
            Tanktosis.Double => Color.Lime,
            Tanktosis.Triple => Color.Orange,
            Tanktosis.Quad => Color.Yellow,
            _ => new(120, 120, 120),
        };
    }

    public static dynamic GetValueByDifficulty<T>(T Normal, T Hard, T Lunatic, T Extra)
    {
        switch (modifier_Difficulty)
        {
            case ModDifficulty.Normal: if (Normal is not null) return Normal; else TankGame.ClientLog.Write("Normal Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Hard: if (Hard is not null) return Hard; else TankGame.ClientLog.Write("Hard Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Lunatic: if (Lunatic is not null) return Lunatic; else TankGame.ClientLog.Write("Lunatic Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
            case ModDifficulty.Extra: if (Extra is not null) return Extra; else TankGame.ClientLog.Write("Extra Difficulty Value is Null!", TanksRebirth.Internals.LogType.ErrorFatal, true); break;
        }
        return null;
    }

    public static void WhilePoisoned_Update(Tank tank)
    {
        if (PoisonedTanks.Find(x => x == tank) is not null && Client.ClientRandom.NextFloat(0.1f, 1f) < 0.7f && !tank.IsDestroyed)
        {
            Vector2 smokey = Vector2.One.RotatedBy(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * Client.ClientRandom.NextFloat(0.1f, 1f) * (Tank.TNK_WIDTH * tank.DrawParams.Scaling.X * 1.1f);

            var smoke = GameHandler.Particles.MakeParticle(tank.Position3D + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));


            smoke.Roll = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;

            smoke.Scale = new(0.5f);

            smoke.Alpha = tank.Properties.Invisible ? 0.1f : 0.7f;

            smoke.Color = Color.DarkViolet;

            smoke.HasAdditiveBlending = false;

            smoke.UniqueBehavior = (part) => {

                GeometryUtils.Add(ref part.Scale, -0.004f * RuntimeData.DeltaTime);
                part.Position += Vector3.UnitY * 0.7f * RuntimeData.DeltaTime * (part.LifeTime / 100f + 1f);
                part.Alpha -= 0.04f * RuntimeData.DeltaTime;

                if (part.Alpha <= 0)
                    part.Destroy();

            };
        }

    }

    public static void SpawnPoisonCloud(Tank? emitter,Vector3 v, float radius = 60f)
    {

        const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

        SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: 0.5f);
        int length = 23;

        for (int i = 0; i < length; i++)
        {
            Vector2 smokey = Vector2.One.RotatedBy(Client.ClientRandom.NextFloat(-MathF.PI, MathF.PI)) * Client.ClientRandom.NextFloat(0.1f, 60f);
            var smoke = GameHandler.Particles.MakeParticle(v + smokey.ExpandZ(),
                GameResources.GetGameResource<Texture2D>("Assets/textures/misc/tank_smokes"));
            smoke.Roll = -CameraGlobals.DEFAULT_ORTHOGRAPHIC_ANGLE;
            smoke.Scale = new(0.8f * Client.ClientRandom.NextFloat(0.1f, 1f));
            smoke.Color = Color.DarkViolet;
            smoke.HasAdditiveBlending = false;
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

            if (tanks[i] is Tank ai)
            {

                if (ai.IsDestroyed || emitter == ai || emitter is not null && ai.Team != emitter.Team && emitter.Team != TeamID.NoTeam || 
                    ai is AITank ai2 && (ai2.AiTankType == NightShade || ai2.AiTankType == Lily)) continue;

                if (Vector2.Distance(ai.Position, v.FlattenZ()) > radius) continue;
                PoisonTank(ai);
            }
        }

    }
    
    public static int FlowerFromBase(int ID)
    {
        return ID switch
        {
            TankID.Brown => Dandelion,
            TankID.Ash => Periwinkle,
            TankID.Marine => Pansy,
            TankID.Yellow => SunFlower,
            TankID.Pink => Rose,
            TankID.Violet => Lavender,
            TankID.Green => Daisy,
            TankID.White => Eryngium,
            TankID.Black => Carnation,
            TankID.Bronze => Kudzu,
            TankID.Silver => CorpseFlower,
            TankID.Sapphire => ForgetMeNot,
            TankID.Ruby => Allium,
            TankID.Citrine => Lily,
            TankID.Amethyst => Lotus,
            TankID.Emerald => NightShade,
            TankID.Gold => Peony,
            TankID.Obsidian => Hydrangea,
            _ => ID,
        };
    }

    public static void Tank_OnPoisoned(Tank _tank)
    {
        const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";
        SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: -0.5f);

        _tank.Properties.ShootStun /= 2;
        _tank.Properties.ShellCooldown /= 2;

        //Fucking saphs overloading the sound system, man...
        _tank.Properties.ShellCooldown = Math.Max(5, _tank.Properties.ShellCooldown);

        _tank.Properties.MaxSpeed *= 1.25f;

        if (_tank is AITank tank)
        {
                tank.Parameters.RandomTimerMinShoot /= 2;
                tank.Parameters.RandomTimerMaxShoot /= 2;
                tank.Parameters.MaxAngleRandomTurn *= 2;
                tank.Parameters.MaxQueuedMovements /= 2;
                tank.Parameters.TurretMovementTimer /= 2;
                tank.Parameters.AimOffset /= 1.5f;
                tank.Parameters.TurretSpeed *= 1.75f;
                tank.Parameters.AimOffset /= 2f;
                  
                tank.Parameters.AggressivenessBias = MathF.Sign(tank.Parameters.AggressivenessBias) * tank.Parameters.AggressivenessBias * 1.3f;
                tank.Parameters.RandomTimerMinMove /= 2;
                tank.Parameters.RandomTimerMaxMove /= 2;

                if (tank.Properties.Stationary && !tank.Properties.Invisible)
                {              
                    tank.Parameters.SmartRicochets = true;
                    tank.Parameters.PredictsPositions = true;
                    tank.Parameters.RandomTimerMinShoot /= 2;
                    tank.Parameters.RandomTimerMaxShoot /= 2;
                }
            if (tank.AiTankType == ModRegistry.GetSingleton<CA_X3_ForgetMeNot>().Type)
            {
                tank.Properties.Armor = new TankArmor(tank, 1);
                tank.Properties.Armor.HideArmor = true;
            }
        }
       

    }
    //Matryoshka

    public override void OnLoad() {

        Modifiers.Map.Add(M_ARMADA, false);
        Modifiers.Map.Add(M_HARDER, false);

        Modifiers.Map.Add(M_ENEMYDRONE, false);
        Modifiers.Map.Add(M_PLAYERDRONE, false);

       // Modifiers.Map.Add("CobaltArmada_MasterSpark", false);
        Modifiers.Map.Add(M_NIGHTSHADE, false);
        Modifiers.Map.Add(M_MULT, false);
        Modifiers.Map.Add(M_IDOL, false);
        Modifiers.Map.Add(M_BROKENFACTORY, false);

        Modifiers.Map.Add(M_LAYERS, false);

        //Fun is infinite
        Modifiers.Map.Add(M_INFINITE, false);

        //There was a risk of rain
        Modifiers.Map.Add(M_RAINRISK, false);

        Neo_Remote = ImportAsset<Model>("assets/models/tank_radio");
        Neo_Stationary = ImportAsset<Model>("assets/models/tank_static");
        Neo_Mobile = ImportAsset<Model>("assets/models/tank_moving");
        Neo_Boss = ImportAsset<Model>("assets/models/tank_elite_a");
        Shell_Beam = ImportAsset<Model>("assets/models/laser_beam");
        Shell_Glaive = ImportAsset<Model>("assets/models/bullet_glave");
        Drone = ImportAsset<Model>("assets/models/tank_drone");
        Elite_Drone = ImportAsset<Model>("assets/models/tank_drone_elite");


        Tank_Y1 = ImportAsset<Texture2D>("assets/textures/tank_lotus");
        Beam = ImportAsset<Texture2D>("assets/textures/tank_zenith");
        Beam_Dan = ImportAsset<Texture2D>("assets/textures/tank_dandy");
        Tank_CustomPaint = ImportAsset<Texture2D>("assets/textures/tank_custompaint");

        Drone_Activate = new OggAudio(Path.Combine(ModPath,"assets/sfx/drone_repair_01.ogg"));
        Drone_Hover = new OggAudio(Path.Combine(ModPath, "assets/sfx/drone_active_01.ogg"));
        Drone_Parry = new OggAudio(Path.Combine(ModPath, "assets/sfx/punch_projectile.ogg"));

        ItemSpawn_Tier1 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_spawn_tier1_01.ogg"));
        ItemSpawn_Tier2 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_spawn_tier2_01.ogg"));
        ItemSpawn_Tier3 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_spawn_tier3_01.ogg"));

        ItemLand_Tier1 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_land_tier1_01.ogg"));
        ItemLand_Tier2 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_land_tier2_01.ogg"));
        ItemLand_Tier3 = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_land_tier3_01.ogg"));


        for (int i = 0; i < 4; i++)
        {
            Pickup[i] = new OggAudio(Path.Combine(ModPath, "assets/sfx/UI_item_pickup_v2_0" + (i + 1).ToString() + ".ogg"));
        }

        for (int i = 0; i < Drone_Shoot.Length; i++)
        {
            Drone_Shoot[i] = new OggAudio(Path.Combine(ModPath, "assets/sfx/drone_attack_v2_0" + (i+1).ToString() + ".ogg"));
        }

        for (int i = 0; i < Drone_Crash.Length; i++)
        {
            Drone_Crash[i] = new OggAudio(Path.Combine(ModPath, "assets/sfx/drone_deathpt2_0" + (i + 1).ToString() + ".ogg"));
        }
        Drone_Disable = new OggAudio(Path.Combine(ModPath, "assets/sfx/drone_deathpt1_01.ogg"));

        GameHandler.OnPostRender += GameHandler_OnPostRender;
        GameHandler.OnPostUpdate += GameHandler_OnPostUpdate;

        MainMenuUI.OnMenuOpen += Open;
        MainMenuUI.OnMenuClose += MainMenu_OnMenuClose;
        MainMenuUI.OnCampaignSelected += MainMenuUI_OnCampaignSelected;

        Campaign.OnPreLoadTank += Campaign_OnPreLoadTank;
        Campaign.OnMissionLoad += Campaign_OnMissionLoad;

        CampaignGlobals.OnMissionStart += GameProperties_OnMissionStart;
        CampaignGlobals.OnMissionEnd += CampaignGlobals_OnMissionEnd;

        SceneManager.OnMissionCleanup += SceneManager_OnMissionCleanup;

        Shell.OnDestroy += Shell_OnDestroy;
        Shell.OnRicochet += Shell_OnRicochet;
        

        Block.OnDestroy += Block_OnDestroy;
      
      
        Mine.OnExplode += Mine_OnExplode;

        Tank.PostApplyDefaults += Tank_PostApplyDefaults;
        Tank.OnPostUpdate += Tank_OnPostUpdate;
        Tank.OnDamage += AITank_OnDamage;
        Tank.OnFire += Tank_OnFire;
        Tank.OnLayMine += Tank_OnLayMine;
        AITank.WhileDangerDetected += AITank_WhileDangerDetected;

        ModLoader.OnPostModLoad += OnLoad2;
        ModLoader.OnFinishModLoading += ModLoader_OnFinishModLoading;

        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_01_Dandelion>().Type] = 0.14f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_02_Perwinkle>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_03_Pansy>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_04_Sunflower>().Type] = 0.32f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_05_Poppy>().Type] = 0.374f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_06_Daisy>().Type] = 0.43f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_07_Lavender>().Type] = 0.5f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_08_Eryngium>().Type] = 0.63f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_09_Carnation>().Type] = 0.85f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_X1_Kudzu>().Type] = 0.42f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_X2_CorpseFlower>().Type] = 0.21f;
        DifficultyAlgorithm.TankDiffs[ModRegistry.GetSingleton<CA_Z9_Hydrangea>().Type] = 1.00f;

       
        Hook_UI.Load();
        TankGame.PreDrawBackBuffer += TankGame_OnPostDraw;

        CA_NetPlay.Load();

        IntermissionSystem.IntermissionAnimator.OnKeyFrameFinish += IntermissionAnimator_OnKeyFrameFinish1;

        CA_DroneLicenseManager.OnApplyLicense += CA_DroneLicenseManager_OnApplyLicense;

        //reset the costs for endless
        TankCosts = new()
        {
            //Little guys... cheap and common
            [TankID.Brown] = new() { Cost = 1, Weight = 2, MissionThreshold = 0, Catagory = "Common" },
            [TankID.Ash] = new() { Cost = 1, Weight = 5, MissionThreshold = 0, Catagory = "Common" },
            [TankID.Marine] = new() { Cost = 5, Weight = 4, MissionThreshold = 0, Catagory = "Common" },
            [TankID.Pink] = new() { Cost = 12, Weight = 8, MissionThreshold = 15, Catagory = "Common" },
            [TankID.Violet] = new() { Cost = 30, Weight = 4, MissionThreshold = 20, Catagory = "Common"
            ,Priority = SpawnCardPriority.RandomSamePrice
            },

            [TankID.Yellow] = new() { Cost = 16, Weight = 4, MissionThreshold = 0, Catagory = "Uncommon" },
            [TankID.Green] = new() { Cost = 24, Weight = 3, MissionThreshold = 0, Catagory = "Uncommon" },
            [TankID.White] = new() { Cost = 40, Weight = 3, MissionThreshold = 40, Catagory = "Uncommon" },
            [TankID.Black] = new() { Cost = 60, Weight = 2, MissionThreshold = 60, Catagory = "Uncommon" },

            //Elite equivlent
            [TankID.Bronze] = new() { Cost = 30, Weight = 4, MissionThreshold = 25, Catagory = "Common", Priority = SpawnCardPriority.RandomSamePrice },
            [TankID.Silver] = new() { Cost = 30, Weight = 5, MissionThreshold = 25, Catagory = "Common", Priority = SpawnCardPriority.RandomSamePrice },  
            [TankID.Sapphire] = new() { Cost = 55, Weight = 4, MissionThreshold = 50, Catagory = "Common" },
            [TankID.Ruby] = new() { Cost = 55, Weight = 8, MissionThreshold = 50, Catagory = "Common" },
            [TankID.Amethyst] = new() { Cost = 20 * 6, Weight = 6, MissionThreshold = 50, Catagory = "Common" },

            [TankID.Citrine] = new() { Cost = 16 * 6, Weight = 4, MissionThreshold = 25, Catagory = "Uncommon" },
            [TankID.Gold] = new() { Cost = 40 * 6, Weight = 3, MissionThreshold = 50, Catagory = "Uncommon" },
            [TankID.Emerald] = new() { Cost = 24 * 6, Weight = 3, MissionThreshold = 50, Catagory = "Uncommon" },
            [TankID.Obsidian] = new() { Cost = 400, Weight = 2, MissionThreshold = 50, Catagory = "Uncommon" },

            [Lotus] = new() { Cost = 1000, Weight = 10, MissionThreshold = 0, Catagory = "Uncommon" , PermitCostMultiplier = 1, Priority = SpawnCardPriority.MostExpensive, PopulationQuantity = 1},
            [NightShade] = new() { Cost = 1000, Weight = 10, MissionThreshold = 0, Catagory = "Uncommon", PermitCostMultiplier = 1, Priority = SpawnCardPriority.MostExpensive, PopulationQuantity =1 },
        };

        try
        {
            if(ModLoader.LoadedMods.Any(mod => mod is not null && mod.InternalName == "AdditionalTanksMarble"))
            {
                Console.WriteLine("MARBLE MOD ACTIVE. ADDING ADDITIONAL CONTENT!");
                TanksMod marb = ModLoader.LoadedMods.Find(mod => mod is not null && mod.InternalName == "AdditionalTanksMarble")!;

                //Granite
                TankCosts.Add(marb.Data.Tanks[0].Type, new() { Cost = 50, Weight = 2, MissionThreshold = 10, Catagory = "Uncommon" });
                //Bubble
                TankCosts.Add(marb.Data.Tanks[1].Type, new() { Cost = 50, Weight = 2, MissionThreshold = 10, Catagory = "Uncommon" });
                //Water
                TankCosts.Add(marb.Data.Tanks[2].Type, new() { Cost = 50, Weight = 1, MissionThreshold = 10, Catagory = "Uncommon" });
                //Crimson
                TankCosts.Add(marb.Data.Tanks[3].Type, new() { Cost = 100, Weight = 1, MissionThreshold = 25, Catagory = "Uncommon" });
                //Tiger
                TankCosts.Add(marb.Data.Tanks[4].Type, new() { Cost = 100, Weight = 1, MissionThreshold = 55, Catagory = "Uncommon" });
                //Fade
                TankCosts.Add(marb.Data.Tanks[5].Type, new() { Cost = 100, Weight = 1, MissionThreshold = 55, Catagory = "Uncommon" });
                //Creeper
                TankCosts.Add(marb.Data.Tanks[6].Type, new() { Cost = 200, Weight = 1, MissionThreshold = 55, Catagory = "Uncommon" });
                //Gamma
                TankCosts.Add(marb.Data.Tanks[7].Type, new() { Cost = 400, Weight = 1, MissionThreshold = 80, Catagory = "Uncommon" });
                //Marble
                TankCosts.Add(marb.Data.Tanks[8].Type, new() { Cost = 400, Weight = 1, MissionThreshold = 80, Catagory = "Uncommon" });
            }

        }
        catch
        {

        }

        //Console Commands
        CommandGlobals.Commands.Add(new CommandInput(name: "RItm_view", description: "List either the player(p) or the enemy's(e) item collections"), new CommandOutput(netSync: false, true, (args) =>
        {
            if (!Modifiers.Map[M_RAINRISK])
            {
                TankGame.IngameConsole.Log("Rogue Mode is not Enabled!",Color.Red);
                return;
            }
            if (args[0].Equals("p", StringComparison.CurrentCultureIgnoreCase))
            {
                TankGame.IngameConsole.Log("Players have the following items:", Color.Blue);
                for (int i = 0; i < GameHandler.MAX_PLAYERS; i++)
                {
                    if (GameHandler.AllPlayerTanks[i] is null) continue;
                    TankGame.IngameConsole.Log("-- Player (" +i.ToString() + ") --", GameHandler.AllPlayerTanks[i].Properties.DestructionColor);
                    if (PlayerRainRiskInventory[i].Count==0) TankGame.IngameConsole.Log("NOTHING", Color.White);
                    else foreach(var item in PlayerRainRiskInventory[i])
                        {
                            TankGame.IngameConsole.Log(item.Name +" x"+item.Stacks.ToString(), Color.White);


                        }
                }

            }
            else if (args[0].Equals("e", StringComparison.CurrentCultureIgnoreCase))
            {
                TankGame.IngameConsole.Log("Enemies have the following items:", Color.Red);
                for (int i = 0; i < GameHandler.MAX_AI_TANKS; i++)
                {
                    if (GameHandler.AllAITanks[i] is null) continue;
                    TankGame.IngameConsole.Log("-- AI Tank (" + i.ToString() + "/" + TankID.Collection.GetKey(GameHandler.AllAITanks[i].AiTankType) + ") --", Color.White);
                    if (EnemyRainRiskInventory[i].Count == 0) TankGame.IngameConsole.Log("NOTHING", Color.White);
                    else foreach (var item in EnemyRainRiskInventory[i])
                        {
                            TankGame.IngameConsole.Log(item.Name[LangCode.English] + " x" + item.Stacks.ToString(), Color.White);
                        }
                }


            }
           



        }));

        CommandGlobals.Commands.Add(new CommandInput(name: "RItm_add", description: "Adds an item to a tank's inventory."), new CommandOutput(netSync: false, true, (args) =>
        {
            if (!Modifiers.Map[M_RAINRISK])
            {
                TankGame.IngameConsole.Log("Rogue Mode is not Enabled!", Color.Red);
                return;
            }
            if (args[0].Equals("p", StringComparison.CurrentCultureIgnoreCase))
            {
                AddItem(GameHandler.AllPlayerTanks[int.Parse(args[1])], GetRainItem(int.Parse(args[2])),false);

            }else if (args[0].Equals("e", StringComparison.CurrentCultureIgnoreCase))
            {
                AddItem(GameHandler.AllAITanks[int.Parse(args[1])], GetRainItem(int.Parse(args[2])), false);
            }




        }));
    }

    private void Shell_OnRicochet(Shell shell, Block? block)
    { 
        if (shell.Owner is not null && CampaignGlobals.InMission && Modifiers.Map[M_RAINRISK])
            ProcItem(shell.Owner, (item) => item.OnShellRicochet(ref shell));
    }

    private void AITank_WhileDangerDetected(AITank tank, IAITankDanger danger)
    {



    }


    #region Hooks

    #region Tank Hooks
    private void Tank_OnLayMine(Tank tank, Mine mine)
    {
        if (CampaignGlobals.InMission && Modifiers.Map[M_RAINRISK]) ProcItem(tank, (item) => item.OnMinePlaced(ref tank, ref mine));
    }
   
    private void Tank_OnFire(Tank tank)
    {
        if (CampaignGlobals.InMission && Modifiers.Map[M_RAINRISK]) ProcItem(tank, (item) => item.OnTankShoot(ref tank));
    }
   
    private void Tank_OnPostUpdate(Tank tank)
    { 
        if (CampaignGlobals.InMission && Modifiers.Map[M_RAINRISK]) ProcItem(tank, (item) => item.OnTankUpdate(ref tank));

       
    }

    private void Tank_PostApplyDefaults(Tank tank, TankProperties properties)
    { 

    }

    private void AITank_OnDamage(Tank victim, bool destroy, ITankHurtContext context)
    {
        if (!destroy) return;
        if (Modifiers.Map[M_LAYERS] && victim is AITank tank && tank.AiTankType != TankID.Brown)
        {
            var t = new AITank(tank.AiTankType - 1);
            t.Physics.Position = tank.Physics.Position;

            t.DrawParams.Scaling *= 0.95f;
            t.Team = tank.Team;
            int I = tank.AITankId;
            tank.Remove(true);
            t.ReassignId(I);

        }
        
        //Stuff for death messages
        if (CampaignGlobals.InMission)
        {


            var A = victim is PlayerTank plyer ? "Player " + (plyer.PlayerId + 1).ToString() : victim is AITank aitank ? TankID.Collection.GetKey(aitank.AiTankType) + " Tank" : null;
            var B = context.Source is PlayerTank plyer2 ? "Player " + (plyer2.PlayerId + 1).ToString() : context.Source is AITank aitank2 ? TankID.Collection.GetKey(aitank2.AiTankType) + " Tank" : null;
            bool Suicide = A is not null && B is not null && A == B;
            string message;
            if (context is TankHurtContextShell shelldeath)
            {

                message = B is not null ? Suicide ? $"{A} shot themselves." : $"{A} was shot by {B}." : $"{A} was shot.";

            }
            else if (context is TankHurtContextExplosion bombdeath)
            {
                message = B is not null ? Suicide ? $"{A} blew themselves up." : $"{A} was blown by {B}." : $"{A} was blown up.";
            }
            else //if(context is TankHurtContextOther otherdeath)
            {

                message = B is not null ? Suicide ? $"{A} killed themselves." : $"{A} died from something related to {B}." : $"{A} died.";
            }
            ChatSystem.SendMessage(message, (context.Source is not null ? context.Source : victim).Properties.DestructionColor);

            if (Modifiers.Map[M_RAINRISK] && victim is AITank)
            {

                if (Client.ClientRandom.Next(0, 20) == 1)
                {
                    new DroppedRainItem(Client.ClientRandom.Next(1, RainItem.ItemID.Collection.Count), victim.Position3D + Vector3.UnitY * 16f, null);
                }
                else if (EnemyRainRiskInventory.Any(x => x.Key == ((AITank)victim).AITankId) && Client.ClientRandom.Next(0, 5) == 1)
                {
                    new DroppedRainItem(EnemyRainRiskInventory[((AITank)victim).AITankId][Client.ClientRandom.Next(0, EnemyRainRiskInventory[((AITank)victim).AITankId].Count)].Type, victim.Position3D + Vector3.UnitY * 16f, null);
                }
                
                if (context.Source is not null)
                {
                    var Own = context.Source!;
                    ProcItem(Own, (item) => item.OnTankDestroy(ref Own, ref victim));
                }
            }

        }
        if (Modifiers.Map[M_RAINRISK] && victim is AITank ai)
        {
            EnemyRainRiskInventory.Remove(ai.AITankId);
        }
    }
    
    private void TankGame_OnPostDraw(GameTime obj)
    {
        TankGame.SpriteRenderer.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, rasterizerState: RenderGlobals.DefaultRasterizer);
        for (int i = 0; i < CA_Popup.AllPopups.Length; i++)
        {
            CA_Popup.AllPopups[i]?.Draw(TankGame.SpriteRenderer);
        }
        foreach (var pu in CA_Drone.AllDrones)
            pu?.DebugRender();
        foreach (var pu in CA_Blackhole.AllBlackholes)
            pu?.DebugRender();
        TankGame.SpriteRenderer.End();
    }

    #endregion
    
    #region Shell Hooks

    private void Shell_OnDestroy(Shell shell, Shell.DestructionContext context)
    {
        if (context == Shell.DestructionContext.WithShell || context == Shell.DestructionContext.WithExplosion || context == Shell.DestructionContext.WithMine) return;
        if (shell.Owner is null) return;

        if (context == Shell.DestructionContext.WithObstacle) { ProcItem(shell.Owner, (item) => item.OnShellDestroy(ref shell)); }


        if (shell.Owner is PlayerTank) return;

        AITank ai = (AITank)shell.Owner;

        if ((ai.AiTankType == SunFlower && shell.Type == ShellID.Rocket))
            new Mine(shell.Owner, shell.Position - new Vector2(0f, 10f).RotatedBy(shell.Rotation), 900f, 0.1f);
    }

    #endregion

    #region Block Hooks
    private void Block_OnDestroy(Block block)
    {
        if (CA_Drone.DroneCollisions.BodyList.Contains(block.Physics))
        {
            CA_Drone.DroneCollisions.Remove(block.Physics);
        }
    }

    private void Block_OnRicochet(Block block, Shell shell)
    {

    }


    #endregion

    #region Mine Hooks

    private void Mine_OnExplode(Mine mine)
    {
        if (mine.Owner is null) return;
        if (CampaignGlobals.InMission && Modifiers.Map[M_RAINRISK]) ProcItem(mine.Owner, (item) => item.OnMineExplode(ref mine));

        if (mine.Owner is PlayerTank) return;
        AITank ai = (AITank)mine.Owner;
        if (ai.AiTankType == SunFlower)
            Fire_AbstractShell_Mine(mine, 8, 1, 0, 4f);
    }

    #endregion

    #region Menu/UI Hooks
    private void MainMenuUI_OnCampaignSelected(Campaign campaign)
    {
        if (campaign == null || !Modifiers.Map[M_INFINITE]) return;
        if (LevelEditorUI.IsActive || campaign.CachedMissions.Length == 0) return;
        PlayerRainRiskInventory = [[], [], [], []];

        //EndlessModeActive = Modifiers.Map["CobaltArmada_Endless"];
    }

    private void IntermissionAnimator_OnKeyFrameFinish1(int frameIndex)
    {
        if (IntermissionSystem.ShouldDrawBanner)
        {
            if (Modifiers.Map.Count(diff => diff.Value) != Modifiers_currentlyactive && Modifiers.Map.Count(diff => diff.Value) != 0 && !LevelEditorUI.IsActive)
            {
                Modifiers_currentlyactive = Modifiers.Map.Count(diff => diff.Value);
                // TankGame.ClientLog.Write(Modifiers_currentlyactive + "Modifiers were active",LogType.Info);
                List<string> bops = new() { "$START$" };
                foreach (var item in Modifiers.Map)
                {
                    if (item.Value) bops.Add(item.Key);
                }

                for (int i = 0; i < bops.Count; i++)
                {
                    new Modifieralert(bops[i], Color.DarkRed, i == 0 ? 0f : 0.1f + i * 0.05f);
                }
            }
        }
    }

    private void MainMenu_OnMenuClose()
    {

    }

    private void Open()
    {
        boss = null;
        MissionIsDestroyedline = null;
        EndlessModeActive = false;
    }

    #endregion

    #region Armada Hooks

    private void CA_DroneLicenseManager_OnApplyLicense(TanksRebirth.GameContent.Systems.AI.AITank tank, ref DroneParameters parameters)
    {
        if (tank.AiTankType == SunFlower)
        {
            ref var Traps = ref parameters.TrapsGeneral;
            ref var Recruit = ref parameters.RecruitGeneral;
            ref var Patrol = ref parameters.HoldGeneral;

            Traps.Enabled = true;
            Traps.RelayTaskToOthers = true;
            Traps.RelayTaskRange = 100;
            Traps.Cooldown = 900;
            Traps.Inaccuracy = 90;
            Traps.Minimum = tank.Parameters.TankAwarenessMine * 2;
        }
    }


    #endregion

    #region Game System Hooks
    private void SceneManager_OnMissionCleanup()
    {
        PoisonedTanks.Clear();
        if (boss is not null) boss.Owner = null;
        foreach (var pu in CA_OrbitalStrike.AllLasers)
            pu?.Remove();
        foreach (var pu in CA_Idol_Tether.AllTethers)
            pu?.Remove();
        foreach (var pu in CA_Drone.AllDrones)
            pu?.Remove();
        foreach (var pu in CA_Blackhole.AllBlackholes)
            pu?.Remove();


        foreach (var item in CA_Popup.AllPopups)
        {
            item?.Remove();
        }
        foreach (var item in DroppedRainItem.AllDroppedRainItems)
        {
            item?.Remove();
        }


        //make sure there isn't anything in the list
        foreach (var pu in CA_Drone.DroneCollisions.BodyList)
            if (pu is Body body) CA_Drone.DroneCollisions.Remove(body);

    }

    private void GameProperties_OnMissionStart()
    {
        if (LevelEditorUI.IsActive || LevelEditorUI.IsTestingLevel)
        {
            if (LevelEditorUI.IsTestingLevel && Modifiers.Map[M_ARMADA])
            {
                ref Tank[] tanks = ref GameHandler.AllTanks;
                foreach (Tank _template in tanks)
                {
                    if (_template is AITank template)
                    {
                        template.UsesCustomModel = true;
                        template.Swap(FlowerFromBase(template.AiTankType), true);
                        var t = new AITank(template.AiTankType);
                        t.Physics.Position = template.Position3D.FlattenZ() / Tank.UNITS_PER_METER;
                        t.Position = template.Position3D.FlattenZ();
                        t.IsDestroyed = false;
                        t.Team = template.Team;
                        template.Remove(true);

                    }
                }


            }
            GiveDrone();
            return;
        }

        if (Modifiers.Map[M_MULT])
        {
            Tank[] tanks = GameHandler.AllTanks;
            for (int i = tanks.Length - 1; i >= 0; i--)
            {
                if (tanks[i] is AITank ai && !ai.IsDestroyed)
                {
                    ai.DrawParams.Scaling *= 1f - (float)modifier_Tanktosis * 0.1f;
                    for (int j = 1; j < (int)modifier_Tanktosis; j++)
                    {
                        var t = new AITank(ai.AiTankType) { Team = ai.Team, Properties = ai.Properties };
                        t.DrawParams.Scaling = ai.DrawParams.Scaling;
                        t.Physics.Position = ai.Physics.Position + Vector2.One * 0.1f;
                    }
                }
            }

        }

        if (Modifiers.Map[M_IDOL])
        {
            Tank[] tanks2 = GameHandler.AllTanks.ToList().FindAll(x => x is not null && x is not PlayerTank && !x.IsDestroyed).ToArray();
            for (int i = tanks2.Length - 1; i >= 0; i--)
            {
                var ai = tanks2[i] as AITank;
                if (ai is null || ai.IsDestroyed) continue;
                var t = new AITank(ForgetMeNot);

                t.Physics.Position = ai.Physics.Position;
                t.Team = ai.Team;

            }
        }

        if (Modifiers.Map[M_NIGHTSHADE])
        {
            const string invisibleTankSound = "Assets/sounds/tnk_invisible.ogg";

            SoundPlayer.PlaySoundInstance(invisibleTankSound, SoundContext.Effect, 0.3f, pitchOverride: 0.5f);
            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is AITank ai && !ai.IsDestroyed && ai.AiTankType != NightShade) PoisonTank(ai);
            }

        }
        GiveDrone();


        if (Modifiers.Map[M_BROKENFACTORY])
        {
            Tank[] tanks = GameHandler.AllTanks;
            for (int i = tanks.Length - 1; i >= 0; i--)
            {
                if (tanks[i] is AITank ai && !ai.IsDestroyed)
                {
                    int original = ai.AiTankType;

                    var TankDisplay = Client.ClientRandom.Next(TankID.Brown, Lily + 1);
                    var TankProperties = Client.ClientRandom.Next(TankID.Brown, Lily + 1);
                    var TankAIParams = Client.ClientRandom.Next(TankID.Brown, Lily + 1);

                    ai.ApplyDefaults(ref ai.Properties);
                    ai.AiTankType = original;
                    ai.Parameters = AIManager.GetAIParameters(TankAIParams);
                    ai.SwapTankTexture(Tank.Assets[$"tank_" + TankID.Collection.GetKey(TankDisplay)!.ToLower()]!);

                    if (TankDisplay >= Dandelion && TankDisplay <= Hydrangea)
                    {
                        ai.DrawParamsTank.Model = ai.Properties.Stationary ? Neo_Stationary! : Neo_Mobile!;
                        if (TankDisplay >= Lotus && TankDisplay <= Hydrangea)
                        {
                            ai.DrawParamsTank.Model = Neo_Boss!;
                        }
                        ai.UsesCustomModel = true;
                        ai.InitModelSemantics();
                    }

                    ChatSystem.SendMessage("Tank" + ai.AITankId.ToString() + ":");
                    ChatSystem.SendMessage("Looks like a " + TankID.Collection.GetKey(TankDisplay));
                    ChatSystem.SendMessage("Moves like a " + TankID.Collection.GetKey(TankProperties));
                    ChatSystem.SendMessage("Thinks like a " + TankID.Collection.GetKey(TankAIParams));
                }
            }
        }
        
        if (Modifiers.Map[M_RAINRISK])
        {
            Console.WriteLine("--- Enemy RainRisk Inventory ---");
            foreach (var Inv in EnemyRainRiskInventory)
            {
                Console.WriteLine(Inv.Key.ToString());
                foreach (var Item in Inv.Value)
                {
                    Console.Write("," + Item.Name[LangCode.English] + "x" + Item.Stacks.ToString());
                }
                Console.WriteLine("");


            }
            Console.WriteLine("--- End of list ---");

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                UpdateItemStats(tanks[i]);
            }

        }
    }

    private void GameHandler_OnPostUpdate()
    {

        KudzuRegen -= RuntimeData.DeltaTime;
        Hook_UI.Hook_UpdateUI();
        if (!CampaignGlobals.InMission)
        {
            foreach (var IT in CA_Drone.AllDrones)
            {
                IT?.FlySound?.Stop(); //SHUT UP
                IT?.EliteFlySound?.Stop(); //YOU AS WELL
            }
        }

        if (Modifiers.Map[M_RAINRISK])
        {
            foreach (var tank in GameHandler.AllTanks)
            {
                if (tank is null || tank.IsDestroyed) continue;
                if (tank is AITank ai)
                {
                    tank.OffsetY += FakeAITankGravity[ai.AITankId];
                    FakeAITankGravity[ai.AITankId] -= 0.04f * RuntimeData.DeltaTime;

                    if (tank.OffsetY < 0)
                    {
                        tank.OffsetY = 0f;
                        FakeAITankGravity[ai.AITankId] = 0f;
                    }

                }
                else if (tank is PlayerTank player)
                {

                    tank.OffsetY += FakePlayerTankGravity[Math.Max(0, player.PlayerId)];
                    FakePlayerTankGravity[Math.Max(0, player.PlayerId)] -= 0.04f * RuntimeData.DeltaTime;
                    if (tank.OffsetY<0)
                    {
                         tank.OffsetY = 0f;
                         FakePlayerTankGravity[Math.Max(0, player.PlayerId)] = 0f;
                    }

                }

            }

        }

        if (!IntermissionSystem.IsAwaitingNewMission)
        {
            foreach (var IT in CA_Drone.AllDrones)
                IT?.Update();

            foreach (var fp in CA_OrbitalStrike.AllLasers)
                fp?.Update();

            foreach (var fp in DroppedRainItem.AllDroppedRainItems)
                fp?.Update();

            foreach (var fp in CA_Blackhole.AllBlackholes)
                fp?.Update();

            ref Tank[] tanks = ref GameHandler.AllTanks;
            for (int i = 0; i < tanks.Length; i++)
            {
                if (tanks[i] is Tank ai)
                {
                    CA_Main.WhilePoisoned_Update(ai);
                }

            }
            if (InputUtils.KeyJustPressed(Keys.B) && DebugManager.DebuggingEnabled && tanks.Length > 0)
            {
                CA_Drone testdrone = new CA_Drone(null, (!CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position).FlattenZ() / 8f);
            }
            if (InputUtils.KeyJustPressed(Keys.Y) && DebugManager.DebuggingEnabled) SpawnPoisonCloud(null, !CameraGlobals.OverheadView ? MatrixUtils.GetWorldPosition(MouseUtils.MousePosition) : PlacementSquare.CurrentlyHovered.Position);

            foreach (var IT in CA_Idol_Tether.AllTethers)
                IT?.Update();




        }
        ref Crate[] crates = ref Crate.AllCrates;
        for (int i = 0; i < crates.Length; i++)
        {
            if (crates[i] is Crate crate)
            {
                if (!crate.IsOpening)
                {
                    crate.ContainsTank = crate.Position.FlattenZ() == Vector2.Clamp(crate.Position.FlattenZ(), new Vector2(GameScene.MIN_X, GameScene.MIN_Z), new Vector2(GameScene.MAX_X, GameScene.MAX_Z));
                }
                //bounding area
                Vector2 nextmove = crate.Position.FlattenZ() + crate.Velocity.FlattenZ() * RuntimeData.DeltaTime;
                if (crate.ContainsTank && nextmove != Vector2.Clamp(nextmove, new Vector2(GameScene.MIN_X, GameScene.MIN_Z), new Vector2(GameScene.MAX_X, GameScene.MAX_Z)))
                {
                    if (nextmove.X < GameScene.MIN_X || nextmove.X > GameScene.MAX_X)
                    {
                        crate.Velocity.X *= -0.9f;
                    }
                    if (nextmove.Y < GameScene.MIN_Z || nextmove.Y > GameScene.MAX_Z)
                    {
                        crate.Velocity.Z *= -0.9f;
                    }
                }
            }
        }

       

        CA_Drone.CollisionUpdate();
        CA_Drone.DroneCollisions.Step(RuntimeData.DeltaTime);

      


    }

    private void GameHandler_OnPostRender()
    {
        foreach (var fp in CA_OrbitalStrike.AllLasers)
            fp?.Render();
        foreach (var fp in CA_Drone.AllDrones)
            fp?.Render();

        boss?.Render(TankGame.SpriteRenderer, new(WindowUtils.WindowWidth - WindowUtils.WindowWidth / 4, WindowUtils.WindowHeight - 60.ToResolutionY()), new Vector2(300, 20).ToResolution(), Anchor.Center, Color.Black, Color.Red);
        MissionIsDestroyedline?.Render(TankGame.SpriteRenderer, new(WindowUtils.WindowWidth - WindowUtils.WindowWidth / 4, WindowUtils.WindowHeight - 60.ToResolutionY()), new Vector2(300, 20).ToResolution(), Anchor.Center, Color.Black, Color.Red);
    }


    private void Campaign_OnMissionLoad(Tank[] tanks, Block[] blocks)
    {

        if (LevelEditorUI.IsActive) return;
        if (MainMenuUI.IsActive) return;

        if (Modifiers.Map[M_RAINRISK])
        {
            EnemyRainRiskInventory.Clear();
          //  Console.Write(EnemyRainRiskInventory.Count.ToString() + " enemies have items!");
            foreach (var t in tanks)
            {
                if (t is AITank enemy)
                {
                    AddItem(enemy, GetRainItem(Client.ClientRandom.Next(1, RainItem.ItemID.Collection.Count)),true);
                }
            }
        }

        if (!Modifiers.Map[M_ARMADA]) return;

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
        if (LevelEditorUI.IsActive) return;
        if (MainMenuUI.IsActive) return;

        if (Modifiers.Map[M_INFINITE] && context == MissionEndContext.Win)
        {
            ChatSystem.SendMessage("!", Color.Pink);

            if (!EndlessModeActive && Modifiers.Map[M_INFINITE])
            {
                ChatSystem.SendMessage("!", Color.Pink);
                CA_Main.RickyRainSeed = new(Server.ServerRandom.Next());
                CA_Main.RogueLikeSeed = new(Server.ServerRandom.Next());
                MissionPool = [];
                for (int i = 0; i < CampaignGlobals.LoadedCampaign.CachedMissions.Length; i++)
                {
                    CampaignGlobals.LoadedCampaign.CachedMissions[i].Note = string.Empty;
                    CampaignGlobals.LoadedCampaign.CachedMissions[i].Name = "Mission " + (i + 1).ToString();
                    var m = CampaignGlobals.LoadedCampaign.CachedMissions[i];
                    MissionPool = MissionPool.Append(m).ToArray();
                   // Console.WriteLine(MissionPool.Length.ToString());
                }
                CampaignGlobals.LoadedCampaign.CachedMissions = [];
                for (int i = 0; i < (MainMenuUI.MissionCheckpoint + 3); i++)
                {
                    CampaignGlobals.LoadedCampaign.CachedMissions = CampaignGlobals.LoadedCampaign.CachedMissions.Append(GetNextMission()).ToArray();
                    CampaignGlobals.LoadedCampaign.CachedMissions[i].Name = "Mission " + (CampaignGlobals.LoadedCampaign.CachedMissions.Length).ToString();
                }
            }

            CampaignGlobals.LoadedCampaign.CachedMissions = CampaignGlobals.LoadedCampaign.CachedMissions.Append(GetNextMission()).ToArray();

            int livesnerf = CampaignGlobals.LoadedCampaign.CachedMissions.Length < 100 ? 5 : 10;
            CampaignGlobals.LoadedCampaign.CachedMissions[CampaignGlobals.LoadedCampaign.CachedMissions.Length - 1].GrantsExtraLife =
            CampaignGlobals.LoadedCampaign.CachedMissions.Length % livesnerf == 0;

            CampaignGlobals.LoadedCampaign.CachedMissions[CampaignGlobals.LoadedCampaign.CachedMissions.Length - 1].Name = "Mission " + (CampaignGlobals.LoadedCampaign.CachedMissions.Length).ToString();
            EndlessModeActive = Modifiers.Map[M_INFINITE];
        }
       

        if (!Modifiers.Map[M_ARMADA]) return;
        if (context == MissionEndContext.Win)
        {
            for (int i = 0; i < TankID.Collection.Keys.Length; i++)
            {
                Spawns[i] = FutureSpawns[i];
                PreSpawns[i] = FutureSpawns[i];
            }





        }
        else if (context == MissionEndContext.Lose)
        {
            for (int i = 0; i < TankID.Collection.Keys.Length; i++)
            {
                Spawns[i] = PreSpawns[i];
            }

        }
    }

    //Hijack mission
    private void Campaign_OnPreLoadTank(ref TankTemplate template)
    {
        if (MainMenuUI.IsActive) return;
        if (LevelEditorUI.IsActive)
        {
            return;
        }


        if (Modifiers.Map[M_LAYERS] && !template.IsPlayer)
        {
            var TrackedSpawnPoints = Campaign.CurrentTrackedSpawns;
            var P = template.Position;
            var TSP = TrackedSpawnPoints[Array.IndexOf(TrackedSpawnPoints, TrackedSpawnPoints.First(pos => pos.Position == P))].Alive = true;
        }

        if (!Modifiers.Map[M_ARMADA]) return;
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



    #endregion

    #region Other Hooks
    private void ModLoader_OnFinishModLoading()
    {
        ModRainItems = [.. _modRainItems];
    }

    //We need to load the item system, which just plagerises the modloader
    private void OnLoad2(TanksMod mod)
    {
        _modRainItemDictionary.Add(mod, []);
        foreach (var type2 in mod.GetType().Assembly.GetTypes())
        {
            var isModTank = type2.IsSubclassOf(typeof(RainItem)) && !type2.IsAbstract;
            if (isModTank)
            {
                var modRItem = (Activator.CreateInstance(type2) as RainItem)!;
                _modRainItemDictionary[mod].Add(modRItem);
                _modRainItems.Add(modRItem);
                modRItem!.Mod = mod;

                // load each tank and its data, add to moddedTypes the singleton of the ModTank.
                RainItemSingletonRegistry._singletonMap.Add(type2, modRItem);

                var itemName = modRItem.GetType().Name;

                modRItem.Name ??= new();
                // modRItem.Texture ??= tankName;

                // doesn't insert anything if there is already something for English
                modRItem.Name.TryAdd(LangCode.English, $"{mod.InternalName}.{itemName}");
                modRItem!.Load();

                TankGame.ClientLog.Write($"Loaded rain item '{modRItem.Name[LangCode.English]}'", LogType.Info);
            }


        }
    }

    #endregion

    #endregion

   
    

   
    
   
    int TankSpawnsWithDrone(AITank tank)
    {
        return tank.AiTankType == SunFlower ? 1 :
            tank.AiTankType == Kudzu ? 3 :
            tank.AiTankType == Carnation ? 1 :
            tank.AiTankType == NightShade ? 2 :
            tank.AiTankType == Hydrangea ? 2 : 0;
    }

    private void GiveDrone()
    {
        ref Tank[] tanks = ref GameHandler.AllTanks;
        foreach (var tank in tanks)
        {
            if (
                (tank is PlayerTank && Modifiers.Map[M_PLAYERDRONE] || 
                tank is AITank && Modifiers.Map[M_ENEMYDRONE]
                || (tank is AITank tankai && TankSpawnsWithDrone(tankai)>0)
                )
                ){
                    int adopts = Math.Max(1 , tank is AITank iTank ? TankSpawnsWithDrone(iTank): 1);
                    for (int j = 0; j < adopts; j++)
                    {
                    Vector2 offset = tank.Position + MathUtils.RotatedBy(Vector2.UnitY, MathF.Tau / adopts * j) * (adopts == 1 ? 0f : CA_Drone.DRN_WIDTH);
                    new CA_Drone(tank, offset/ 8); //Server.ServerRandom.Next(1, TankID.Collection.Count)
                }
                }
        }
    }

    public static RainItem GetRainItem(int ItemType)
    {
        for (int i = 0; i < ModRainItems.Length; i++)
        {
            var modRainItem = ModRainItems[i];

            // associate values properly for modded data
            if (ItemType == modRainItem.Type)
            {
                return modRainItem.Clone();
            }
        }
        throw new Exception($"Unable to find Rain Item with ID'{ItemType}'");
    }


    public static void PoisonTank(Tank ai)
    {
        if ((Client.IsHost() && Client.IsConnected()) || (!Client.IsConnected() && !ai.IsDestroyed) || MainMenuUI.IsActive)
        {
            if (PoisonedTanks.Find(x => x == ai) is null)
            {
                PoisonedTanks.Add(ai);
                Tank_OnPoisoned(ai);
                CA_NetPlay.RequestNightshadeTank(ai);
            }
        }

    }


    #region Stuff To Replace

  


    public static void Lay_AbstractMine(Shell origin)
    {
        if (!MainMenuUI.IsActive && !CampaignGlobals.InMission)return;
        Mine mine = new Mine(origin.Owner, origin.Position - new Vector2(0f, 10f).RotatedBy(origin.Rotation), 400f, 1f);
    }

    /// <summary>
    /// Spawn a shell from somewhere. used with the burst shells
    /// </summary>
    public static void Fire_AbstractShell(Shell origin,int count, int newType = 1, int burst_bounces = 0,float burst_expand=3.5f)
    {
        if (MainMenuUI.IsActive || !CampaignGlobals.InMission) return;
        if (origin is null||origin.Owner is null) return;
        float angle = 0;
        float rng_burst = origin.Rotation+ (MathF.PI * 2f / (count*2f));
        for (int i = 0; i < count; i++)
        {
         
                angle =(MathF.PI * 2f / count * i)+ rng_burst;
                float newAngle = angle;
                Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0, origin.Properties.HomeProperties, true);
                Vector2 new2d2 = Vector2.UnitY.RotatedBy(newAngle);
                Vector2 newPos2 = origin.Position + new Vector2(0f, 14f).RotatedBy(-newAngle);
                shell2.Position = new Vector2(newPos2.X, newPos2.Y);
                shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
                shell2.RicochetsRemaining = burst_bounces;
            }
        }

    public static void Fire_AbstractShell_Tank(Tank origin, int count,ITankHurtContext player_kill, int newType = 1, int burst_bounces=0, float burst_expand = 3.4f)
    {
        if (MainMenuUI.IsActive || !CampaignGlobals.InMission) return;
        if (origin is null) return;

        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin , 0, origin.Properties.ShellHoming, false);
            Vector2 new2d2 = Vector2.UnitY.RotatedBy(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 20f).RotatedBy(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }
    public static void Fire_AbstractShell_Mine(Mine origin, int count, int newType = 1, int burst_bounces = 0, float burst_expand = 3.5f)
    {
        if ((!MainMenuUI.IsActive && !CampaignGlobals.InMission)) return;
        if (origin is null || origin.Owner is null) return;
        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0, new Shell.HomingProperties(), true);
            Vector2 new2d2 = Vector2.UnitY.RotatedBy(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 25f).RotatedBy(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y) * burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }

    #endregion

    public override void OnUnload() {
        MainMenuUI.OnCampaignSelected -= MainMenuUI_OnCampaignSelected;
        CA_DroneLicenseManager.OnApplyLicense -= CA_DroneLicenseManager_OnApplyLicense;
        MainMenuUI.OnMenuOpen -= Open;
        MainMenuUI.OnMenuClose -= MainMenu_OnMenuClose;
        Campaign.OnPreLoadTank -= Campaign_OnPreLoadTank;

        Shell.OnDestroy -= Shell_OnDestroy;
        SceneManager.OnMissionCleanup -= SceneManager_OnMissionCleanup;


        Mine.OnExplode -= Mine_OnExplode;
        GameHandler.OnPostRender -= GameHandler_OnPostRender;
        GameHandler.OnPostUpdate -= GameHandler_OnPostUpdate;
        CampaignGlobals.OnMissionStart -= GameProperties_OnMissionStart;
        TankGame.PreDrawBackBuffer -= TankGame_OnPostDraw;

        Campaign.OnMissionLoad -= Campaign_OnMissionLoad;

        //Difficulties.Types.Remove("CobaltArmada_Swap");
        //Difficulties.Types.Remove("CobaltArmada_GetGud");
        //Difficulties.Types.Remove("CobaltArmada_YouAndWhatArmy");
        //Difficulties.Types.Remove("CobaltArmada_YouAndMyArmy");
        //Difficulties.Types.Remove("CobaltArmada_MasterSpark");
        //Difficulties.Types.Remove("CobaltArmada_TanksOnCrack");
        //Difficulties.Types.Remove("CobaltArmada_Mitosis");
        //Difficulties.Types.Remove("CobaltArmada_P2");
        //Difficulties.Types.Remove("CobaltArmada_RussianDollTanks");
        MainMenuUI.AllDifficultyButtons.RemoveRange(Hook_UI.startIndex, MainMenuUI.AllDifficultyButtons.Count - Hook_UI.startIndex - 1);
        CA_NetPlay.Unload();
        AITank.OnDamage -= AITank_OnDamage;
      
    }

    /// <summary>I hate this only cause _singletonMap from the base game's mod reg is internal and can't be accessed</summary>
    public static class RainItemSingletonRegistry
    {
        internal static Dictionary<Type, IModdedRainItemContent> _singletonMap = [];
        /// <summary>A useful method that gets properties of a modded type. Can be used to manually swap properties after spawning an entity.</summary>
        /// <typeparam name="T">The <see cref="Type"/> of the modded content you wish to request data from.</typeparam>
        /// <returns>A singleton instance of any form of supported mod content.</returns>
        public static T GetSingleton<T>() where T : class, IModdedRainItemContent
        {
            if (_singletonMap.TryGetValue(typeof(T), out var content))
                return (T)content;

            throw new Exception($"Modded type '{typeof(T).Name}' not found.");
        }
    }

}