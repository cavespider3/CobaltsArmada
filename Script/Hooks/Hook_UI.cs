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
        public static bool DisplayAdvancedDifficulties = false;

        //Custom modifiers
        public static UITextButton? Invasion;

        public static UITextButton? BadFactory;

        public static UITextButton? Touhoumode_2;
        
        public static UITextButton? MasterSpark;
        
        public static UITextButton? Prenerf_enemies;
        
        public static UITextButton? Tankbonic_Plague;
        
        public static UITextButton? Tanktosis;

        public static UITextButton? SkynetModeA;

        public static UITextButton? SkynetModeB;

        public static UITextButton? oopsAllIdol;

        public static UITextButton? TankSiblings;

        public static UITextButton? InfiniteMission;

        public static UITextButton? RogueLike;


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
                    Modifiers.Map[M_ARMADA] = !Modifiers.Map[M_ARMADA];
                    
                },
                Color = Modifiers.Map[M_ARMADA] ? Color.Lime : Color.Red

            };

            TankSiblings = new("Matryoshka Tanks", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Enemy tanks will change into a lower tier when destroyed.",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_LAYERS] = !Modifiers.Map[M_LAYERS];

                },
                Color = Modifiers.Map[M_LAYERS] ? Color.Lime : Color.Red

            };


            SkynetModeA = new("Players have drones", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "All players are given a drone (Currently doesn't work).",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_PLAYERDRONE] = !Modifiers.Map[M_PLAYERDRONE];

                },
                Color = Modifiers.Map[M_PLAYERDRONE] ? Color.Lime : Color.Red

            };

            SkynetModeB = new("Enemies have drones", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "All enemy tanks are given a drone.",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_ENEMYDRONE] = !Modifiers.Map[M_ENEMYDRONE];

                },
                Color = Modifiers.Map[M_ENEMYDRONE] ? Color.Lime : Color.Red

            };

            BadFactory = new("Faulty Production", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Enemy tanks are replaced with defects",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_BROKENFACTORY] = !Modifiers.Map[M_BROKENFACTORY];

                },
                Color = Modifiers.Map[M_BROKENFACTORY] ? Color.Lime : Color.Red
            };


            Touhoumode_2 = new("Difficulty", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Modifies Armada tanks to be easier or harder.",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Difficulty + 1 > ModDifficulty.Extra) modifier_Difficulty = ModDifficulty.Normal;
                    else modifier_Difficulty += 1;
                    Modifiers.Map[M_HARDER] = modifier_Difficulty != ModDifficulty.Normal;

                   
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Difficulty - 1 < ModDifficulty.Normal) modifier_Difficulty = ModDifficulty.Extra;
                    else modifier_Difficulty -= 1;
                    Modifiers.Map[M_HARDER] = modifier_Difficulty != ModDifficulty.Normal;
                },

                Color = DifficultyColor(modifier_Difficulty)
            };
           

            Tankbonic_Plague = new("Nightshaded", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Every non-player tank will be nightshaded, making them significantly more aggresive.",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_NIGHTSHADE] = !Modifiers.Map[M_NIGHTSHADE];
                    
                },
                Color = Modifiers.Map[M_NIGHTSHADE] ? Color.Lime : Color.Red

            };
       

            Tanktosis = new("Double Trouble", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Multiplies the tanks into smaller, but just as dangerous clones",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Tanktosis + 1 > CA_Main.Tanktosis.Quad) modifier_Tanktosis = CA_Main.Tanktosis.Single;
                    else modifier_Tanktosis += 1;
                    Modifiers.Map[M_MULT] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                    
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Tanktosis - 1 < CA_Main.Tanktosis.Single) modifier_Tanktosis = CA_Main.Tanktosis.Quad;
                    else modifier_Tanktosis -= 1;
                    Modifiers.Map[M_MULT] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                   
                },

                Color = DifficultyColor(modifier_Difficulty)

            };
           
            oopsAllIdol = new("Idol Support", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Forget-Me-Nots will spawn to assist the enemy.",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_IDOL] = !Modifiers.Map[M_IDOL];
                   
                },
                Color = Modifiers.Map[M_IDOL] ? Color.Lime : Color.Red

            };

            RogueLike = new("Rogue Mode", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Allows the player to upgrade their tank.",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_RAINRISK] = !Modifiers.Map[M_RAINRISK];

                },
                Color = Modifiers.Map[M_RAINRISK] ? Color.Lime : Color.Red

            };

            InfiniteMission = new("Endless Campaign", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = false,
                Tooltip = "Campaign will keep going forever... how far can you go?",
                OnLeftClick = (elem) =>
                {
                    Modifiers.Map[M_INFINITE] = !Modifiers.Map[M_INFINITE];

                },
                Color = Modifiers.Map[M_INFINITE] ? Color.Lime : Color.Red

            };

            //Hooks

            buttons.AddRange([
               BadFactory, Invasion, Touhoumode_2, Tankbonic_Plague, Tanktosis, oopsAllIdol,SkynetModeA,SkynetModeB,TankSiblings,InfiniteMission,RogueLike
           ]);
            startIndex = MainMenuUI.AllDifficultyButtons.Count - 1;
            MainMenuUI.AllDifficultyButtons.AddRange(buttons);

        }
        
        public static void Hook_UpdateUI()
        {
          //  if (MainMenuUI.MenuState != MainMenuUI.UIState.Difficulties) return;
            if (Invasion is null) return;
            try
            {
                if (!MainMenuUI.IsActive) return;
                if (MainMenuUI.BulletHell.IsVisible)
                {
                    Invasion!.IsVisible = true;
                    BadFactory!.IsVisible = true;
                    Touhoumode_2!.IsVisible = true;
                    Tankbonic_Plague!.IsVisible = true;
                    Tanktosis!.IsVisible = true;
                    oopsAllIdol!.IsVisible = true;

                    SkynetModeA!.IsVisible = true;
                    RogueLike!.IsVisible = true;
                    InfiniteMission!.IsVisible = true;
                    SkynetModeB!.IsVisible = true;
                    TankSiblings!.IsVisible = true;

                    Touhoumode_2.Text = "Difficulty: " + modifier_Difficulty.ToString();
                    Touhoumode_2.Tooltip = "Modifies Armada tanks to be easier or harder.\n\"" + (
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

                    Tankbonic_Plague.Color = Modifiers.Map[M_NIGHTSHADE] ? Color.Lime : Color.Red;
                    Tanktosis.Color = DifficultyColor(modifier_Tanktosis);
                    oopsAllIdol.Color = Modifiers.Map[M_IDOL] ? Color.Lime : Color.Red;
                    Touhoumode_2.Color = DifficultyColor(modifier_Difficulty);
                    Invasion.Color = Modifiers.Map[M_ARMADA] ? Color.Lime : Color.Red;

                    RogueLike.Color = Modifiers.Map[M_RAINRISK] ? Color.Lime : Color.Red;
                    InfiniteMission.Color = Modifiers.Map[M_INFINITE] ? Color.Lime : Color.Red;

                    SkynetModeA!.Color = Modifiers.Map[M_PLAYERDRONE] ? Color.Lime : Color.Red;

                    SkynetModeB!.Color = Modifiers.Map[M_ENEMYDRONE] ? Color.Lime : Color.Red;
                    TankSiblings!.Color = Modifiers.Map[M_LAYERS] ? Color.Lime : Color.Red;
                    BadFactory!.Color = Modifiers.Map[M_BROKENFACTORY] ? Color.Lime : Color.Red;
                }
                else
                {
                    Invasion!.IsVisible = false;
                    BadFactory!.IsVisible = false;
                    Touhoumode_2!.IsVisible = false;
                    Tankbonic_Plague!.IsVisible = false;
                    Tanktosis!.IsVisible = false;
                    oopsAllIdol!.IsVisible = false;
                    SkynetModeA!.IsVisible = false;
                    SkynetModeB!.IsVisible = false;
                    TankSiblings!.IsVisible = false;
                    RogueLike!.IsVisible = false;
                    InfiniteMission!.IsVisible = false;
                }
            }
            catch
            {

            }
           
            //Gameplay (these don't work, an will likely be patched out
            //boss?.Update();
            //MissionIsDestroyedline?.Update();
        }

    }
}
