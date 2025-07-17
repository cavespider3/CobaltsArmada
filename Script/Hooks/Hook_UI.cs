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
using TanksRebirth.Internals.Common.GameUI;
using static CobaltsArmada.CA_Main;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.UI.MainMenu;
using DiscordRPC;

namespace CobaltsArmada.Hooks
{
    public class Hook_UI
    {
        //Custom modifiers
        public static UITextButton? Invasion;
        
        public static UITextButton? Touhoumode_2;
        
        public static UITextButton? MasterSpark;
        
        public static UITextButton? Prenerf_enemies;
        
        public static UITextButton? Tankbonic_Plague;
        
        public static UITextButton? Tanktosis;

        public static UITextButton? SkynetModeA;

        public static UITextButton? SkynetModeB;

        public static UITextButton? oopsAllIdol;
        public static List<UITextButton> buttons = new();

        public static int startIndex;
        public static void Load()
        {
            Invasion = new("Armada Mod Buff", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Vanilla tanks are converted into their Armada counterpart.\nWARNING: Do not expect it to be a fair fight!\nWill not work with some modifiers.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_Swap"] = !Difficulties.Types["CobaltArmada_Swap"];
                    
                },
                Color = Difficulties.Types["CobaltArmada_Swap"] ? Color.Lime : Color.Red

            };

            SkynetModeA = new("Players have drones", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "All players are given a drone.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_YouAndMyArmy"] = !Difficulties.Types["CobaltArmada_YouAndMyArmy"];

                },
                Color = Difficulties.Types["CobaltArmada_YouAndMyArmy"] ? Color.Lime : Color.Red

            };

            SkynetModeB = new("Enemies have drones", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "All ai tanks are given a drone.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_YouAndWhatArmy"] = !Difficulties.Types["CobaltArmada_YouAndWhatArmy"];

                },
                Color = Difficulties.Types["CobaltArmada_YouAndWhatArmy"] ? Color.Lime : Color.Red

            };


            Touhoumode_2 = new("Difficulty", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Modifies Armada tanks to be easier or harder.",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Difficulty + 1 > ModDifficulty.Phantasm) modifier_Difficulty = ModDifficulty.Easy;
                    else modifier_Difficulty += 1;
                    Difficulties.Types["CobaltArmada_GetGud"] = modifier_Difficulty != ModDifficulty.Normal;

                   
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Difficulty - 1 < ModDifficulty.Easy) modifier_Difficulty = ModDifficulty.Phantasm;
                    else modifier_Difficulty -= 1;
                    Difficulties.Types["CobaltArmada_GetGud"] = modifier_Difficulty != ModDifficulty.Normal;
                },

                Color = DifficultyColor(modifier_Difficulty)
            };
           

            Tankbonic_Plague = new("Nightshaded", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Every non-player tank will be nightshaded, making them significantly more aggresive.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_TanksOnCrack"] = !Difficulties.Types["CobaltArmada_TanksOnCrack"];
                    
                },
                Color = Difficulties.Types["CobaltArmada_TanksOnCrack"] ? Color.Lime : Color.Red

            };
       

            Tanktosis = new("Double Trouble", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Multiplies the tanks into smaller, but just as dangerous clones",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Tanktosis + 1 > CA_Main.Tanktosis.Quad) modifier_Tanktosis = CA_Main.Tanktosis.Single;
                    else modifier_Tanktosis += 1;
                    Difficulties.Types["CobaltArmada_Mitosis"] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                    
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Tanktosis - 1 < CA_Main.Tanktosis.Single) modifier_Tanktosis = CA_Main.Tanktosis.Quad;
                    else modifier_Tanktosis -= 1;
                    Difficulties.Types["CobaltArmada_Mitosis"] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                   
                },

                Color = DifficultyColor(modifier_Difficulty)

            };
           
            oopsAllIdol = new("Idol Support", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Forget-Me-Nots will spawn to assist the enemy.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_P2"] = !Difficulties.Types["CobaltArmada_P2"];
                   
                },
                Color = Difficulties.Types["CobaltArmada_P2"] ? Color.Lime : Color.Red

            };
       
            //Hooks

            buttons.AddRange([
               Invasion, Touhoumode_2, Tankbonic_Plague, Tanktosis, oopsAllIdol,SkynetModeA,SkynetModeB
           ]);
            startIndex = MainMenuUI.AllDifficultyButtons.Count - 1;
            MainMenuUI.AllDifficultyButtons.AddRange(buttons);


        }
        public static void Hook_UpdateUI()
        {
            try
            {
                if (!MainMenuUI.Active) return;
                if (MainMenuUI.BulletHell.IsVisible)
                {
                    Invasion!.IsVisible = true;
                    Touhoumode_2!.IsVisible = true;
                    Tankbonic_Plague!.IsVisible = true;
                    Tanktosis!.IsVisible = true;
                    oopsAllIdol!.IsVisible = true;

                    SkynetModeA!.IsVisible = true;
                    SkynetModeB!.IsVisible = true;

                    Touhoumode_2.Text = "Difficulty: " + modifier_Difficulty.ToString();
                    Touhoumode_2.Tooltip = "Modifies Armada tanks to be easier or harder.\n\"" + (modifier_Difficulty == ModDifficulty.Easy ? "for people who need to stop and smell the roses" :
                        modifier_Difficulty == ModDifficulty.Normal ? "for people who need a baseline experience" :
                        modifier_Difficulty == ModDifficulty.Hard ? "for people who need a challenge" :
                        modifier_Difficulty == ModDifficulty.Lunatic ? "for people that think master mod was too easy" :
                        modifier_Difficulty == ModDifficulty.Extra ? "for those who's middle name is \"Masochist\"" :
                        "DON'T"
                        ) + "\"";

                    Tanktosis.Tooltip = modifier_Tanktosis == CA_Main.Tanktosis.Single ? "Multiplies the tanks into smaller, but just as dangerous clones" :
                        modifier_Tanktosis == CA_Main.Tanktosis.Double ? "Prepare for trouble..." :
                         modifier_Tanktosis == CA_Main.Tanktosis.Triple ? "That's like more than two!" :
                         "YOU PICK THE WRONG HOUSE, FOOL!";
                    Tanktosis.Text = modifier_Tanktosis == CA_Main.Tanktosis.Single ? "Double Trouble" :
                        modifier_Tanktosis == CA_Main.Tanktosis.Double ? "Double Trouble" :
                         modifier_Tanktosis == CA_Main.Tanktosis.Triple ? "Triple Threat" :
                         "Quad Squad";

                    Tankbonic_Plague.Color = Difficulties.Types["CobaltArmada_TanksOnCrack"] ? Color.Lime : Color.Red;
                    Tanktosis.Color = DifficultyColor(modifier_Tanktosis);
                    oopsAllIdol.Color = Difficulties.Types["CobaltArmada_P2"] ? Color.Lime : Color.Red;
                    Touhoumode_2.Color = DifficultyColor(modifier_Difficulty);
                    Invasion.Color = Difficulties.Types["CobaltArmada_Swap"] ? Color.Lime : Color.Red;

                    SkynetModeA!.Color = Difficulties.Types["CobaltArmada_YouAndMyArmy"] ? Color.Lime : Color.Red;

                    SkynetModeB!.Color = Difficulties.Types["CobaltArmada_YouAndWhatArmy"] ? Color.Lime : Color.Red;
                }
                else
                {
                    Invasion!.IsVisible = false;
                    Touhoumode_2!.IsVisible = false;
                    Tankbonic_Plague!.IsVisible = false;
                    Tanktosis!.IsVisible = false;
                    oopsAllIdol!.IsVisible = false;
                    SkynetModeA!.IsVisible = false;
                    SkynetModeB!.IsVisible = false;
                }
            }
            catch
            {

            }
           
            //Gameplay
            boss?.Update();
            MissionDeadline?.Update();
        }

    }
}
