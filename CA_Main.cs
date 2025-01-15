using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth;
using TanksRebirth.Internals.Common.Framework.Audio;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Properties;
using TanksRebirth.GameContent.UI;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth.Net;
using TanksRebirth.GameContent.Systems;
using TanksRebirth.Localization;
using TanksRebirth.GameContent.Systems.Coordinates;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace CobaltsArmada;

public class CA_Main : TanksMod {
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


    public static bool CustomDifficulty_Invasion = true;

    [AllowNull]
    public static Model Neo_Stationary;
    [AllowNull]
    public static Model Neo_Mobile;
    [AllowNull]
    public static Model Shell_Beam;
    public override void OnLoad() {
     
        Campaign.OnMissionLoad += Highjack_Mission;
   
        Shell.OnDestroy += Shell_OnDestroy;
 
        Shell.OnRicochet += Shell_OnRicochet;
        Shell.OnRicochetWithBlock += Shell_OnRicochetWithBlock;
    
        Mine.OnExplode += Mine_OnExplode;

        Neo_Stationary = ImportAsset<Model>("assets/models/tank_static");
        Neo_Mobile = ImportAsset<Model>("assets/models/tank_moving");
        Shell_Beam = ImportAsset<Model>("assets/models/laser_beam");
    }

    private void Highjack_Mission(ref Tank[] tanks, ref Block[] blocks)
    {
        if (MainMenu.Active) return;
        if (!CustomDifficulty_Invasion) return;
        TankGame.ClientLog.Write("Invading campaign...", TanksRebirth.Internals.LogType.Info);
       // ChatSystem.SendMessage("Invading campaign...", Color.Yellow);
            for (int i = 0; i < tanks.Length; i++)
            {
            
            if (tanks[i] is PlayerTank || tanks[i] is null || tanks[i] as AITank is null) continue;
                var ai = tanks[i] as AITank;
            if (ai is null) continue;
            float secret_tank_chance = (float)GameProperties.LoadedCampaign.CurrentMissionId / GameProperties.LoadedCampaign.CachedMissions.Length;
            var nextFloat = Server.ServerRandom.NextFloat(0, 1);
            ChatSystem.SendMessage($"{nextFloat}", Color.White);
            if (nextFloat <= float.Lerp(0, 0.075f, secret_tank_chance) * (1 + secret_tank_chance / 2f))
            {
                TankGame.ClientLog.Write("RARE TANK GO!", TanksRebirth.Internals.LogType.Info);
                if (Server.ServerRandom.NextFloat(0, 1) < 0.25) ai.Swap(ModContent.GetSingleton<CA_X1_Kudzu>().Type, true);
                else ai.Swap(ModContent.GetSingleton<CA_X2_CorpseFlower>().Type, true);

            }
            else
            {
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
                    default: break;

                }
               
            }
            ai.InitModelSemantics();
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


    private void Shell_OnRicochetWithBlock(Block block, Shell shell)
    {
        Shell_OnRicochet(shell);
    }

    private void Shell_OnRicochet(Shell shell)
    {
        if (shell.Owner is null) return;
        if (shell.Owner is PlayerTank) return;
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
        var ai = (AITank)shell.Owner;
       if (ai.AiTankType == Daisy.Type && shell.Type == ShellID.Rocket)
        Fire_AbstractShell(shell, 4,1,0,3.5f);
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

        if ((ai.AiTankType == Poppy.Type && shell.Type == ShellID.Rocket) || (ai.AiTankType == Lavi.Type && shell.Type == ShellID.Rocket))
            Fire_AbstractShell(shell, ai.AiTankType == Poppy.Type ? 4 : 6,1,0,3f);
        if ((ai.AiTankType == Sunny.Type && shell.Type == ShellID.Rocket))
           new Mine(shell.Owner, shell.Position - new Vector2(0f, 10f).RotatedByRadians(shell.Rotation), 900f, 0.1f);
    }


    public static void Lay_AbstractMine(Shell origin)
    {
        if ((!MainMenu.Active && !GameProperties.InMission))return;
        Mine mine = new Mine(origin.Owner, origin.Position - new Vector2(0f, 10f).RotatedByRadians(origin.Rotation), 400f, 1f);
    }

    /// <summary>
    /// Spawn a shell from somewhere. used with the burst shells
    /// </summary>
    public static void Fire_AbstractShell(Shell origin,int count, int newType = 1, uint burst_bounces = 0,float burst_expand=3.5f)
    {
        if (MainMenu.Active || !GameProperties.InMission) return;
        if (origin is null||origin.Owner is null) return;
        float angle = 0;
        float rng_burst = origin.Rotation+ (MathF.PI * 2f / (count*2f));
        for (int i = 0; i < count; i++)
        {
         
                angle =(MathF.PI * 2f / count * i)+ rng_burst;
                float newAngle = angle;
                Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0U, origin.Properties.HomeProperties, true);
                Vector2 new2d2 = Vector2.UnitY.RotatedByRadians(newAngle);
                Vector2 newPos2 = origin.Position + new Vector2(0f, 14f).RotatedByRadians(-newAngle);
                shell2.Position = new Vector2(newPos2.X, newPos2.Y);
                shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
                shell2.RicochetsRemaining = burst_bounces;
            }
        }

    public static void Fire_AbstractShell_Tank(Tank origin, int count,ITankHurtContext player_kill, int newType = 1, uint burst_bounces=0, float burst_expand = 3.4f)
    {
        if (MainMenu.Active || !GameProperties.InMission) return;
        if (origin is null) return;

        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin , 0U, origin.Properties.ShellHoming, false);
            Vector2 new2d2 = Vector2.UnitY.RotatedByRadians(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 20f).RotatedByRadians(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y)* burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }
    public static void Fire_AbstractShell_Mine(Mine origin, int count, int newType = 1, uint burst_bounces = 0, float burst_expand = 3.5f)
    {
        if ((!MainMenu.Active && !GameProperties.InMission)) return;
        if (origin is null || origin.Owner is null) return;
        float angle = 0;
        float rng_burst = Server.ServerRandom.NextFloat(-MathF.PI, MathF.PI);
        for (int i = 0; i < count; i++)
        {

            angle = (MathF.PI * 2f / count * i) + rng_burst;
            float newAngle = angle;
            Shell shell2 = new Shell(origin.Position, Vector2.Zero, newType, origin.Owner, 0U, new Shell.HomingProperties(), true);
            Vector2 new2d2 = Vector2.UnitY.RotatedByRadians(newAngle);
            Vector2 newPos2 = origin.Position + new Vector2(0f, 25f).RotatedByRadians(-newAngle);
            shell2.Position = new Vector2(newPos2.X, newPos2.Y);
            shell2.Velocity = new Vector2(-new2d2.X, new2d2.Y) * burst_expand;
            shell2.RicochetsRemaining = burst_bounces;
        }
    }


    public override void OnUnload() {
        Shell.OnDestroy -= Shell_OnDestroy;
     
        Shell.OnRicochet -= Shell_OnRicochet;
        Shell.OnRicochetWithBlock -= Shell_OnRicochetWithBlock;
       
        Mine.OnExplode -= Mine_OnExplode;
        Campaign.OnMissionLoad -= Highjack_Mission;
    }
}