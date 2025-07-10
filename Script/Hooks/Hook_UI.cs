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

namespace CobaltsArmada.Hooks
{
    public class Hook_UI
    {
        //Custom modifiers
        [AllowNull]
        public static UITextButton Invasion;
        [AllowNull]
        public static UITextButton Touhoumode_2;
        [AllowNull]
        public static UITextButton MasterSpark;
        [AllowNull]
        public static UITextButton Prenerf_enemies;
        [AllowNull]
        public static UITextButton Tankbonic_Plague;
        [AllowNull]
        public static UITextButton Tanktosis;
        [AllowNull]
        public static UITextButton oopsAllIdol;
        public static void Load()
        {
            Invasion = new("Armada Mod Buff", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = true,
                Tooltip = "Vanilla tanks are converted into their Armada counterpart.\nWARNING: Do not expect it to be a fair fight!\nWill not work with some modifiers.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_Swap"] = !Difficulties.Types["CobaltArmada_Swap"];
                    Invasion.Color = Difficulties.Types["CobaltArmada_Swap"] ? Color.Lime : Color.Red;
                },
                Color = Difficulties.Types["CobaltArmada_Swap"] ? Color.Lime : Color.Red

            };
            Invasion.SetDimensions(800, 550, 300, 40);

            Touhoumode_2 = new("Difficulty", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = true,
                Tooltip = "Modifies Armada tanks to be easier or harder.",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Difficulty + 1 > ModDifficulty.Phantasm) modifier_Difficulty = ModDifficulty.Easy;
                    else modifier_Difficulty += 1;
                    Difficulties.Types["CobaltArmada_GetGud"] = modifier_Difficulty != ModDifficulty.Normal;

                    Touhoumode_2.Color = DifficultyColor(modifier_Difficulty);
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Difficulty - 1 < ModDifficulty.Easy) modifier_Difficulty = ModDifficulty.Phantasm;
                    else modifier_Difficulty -= 1;
                    Difficulties.Types["CobaltArmada_GetGud"] = modifier_Difficulty != ModDifficulty.Normal;

                    Touhoumode_2.Color = DifficultyColor(modifier_Difficulty);
                },

                Color = DifficultyColor(modifier_Difficulty)
            };
            Touhoumode_2.SetDimensions(800, 600, 300, 40);

            Tankbonic_Plague = new("Nightshaded", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = true,
                Tooltip = "Every AITank will be inflicted with the Nightshade AITank's buff.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_TanksOnCrack"] = !Difficulties.Types["CobaltArmada_TanksOnCrack"];
                    Tankbonic_Plague.Color = Difficulties.Types["CobaltArmada_TanksOnCrack"] ? Color.Lime : Color.Red;
                },
                Color = Difficulties.Types["CobaltArmada_TanksOnCrack"] ? Color.Lime : Color.Red

            };
            Tankbonic_Plague.SetDimensions(800, 650, 300, 40);

            Tanktosis = new("Double Trouble", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = true,
                Tooltip = "Multiplies the tanks into smaller, but just as dangerous clones",
                OnLeftClick = (elem) =>
                {
                    if (modifier_Tanktosis + 1 > CA_Main.Tanktosis.Quad) modifier_Tanktosis = CA_Main.Tanktosis.Single;
                    else modifier_Tanktosis += 1;
                    Difficulties.Types["CobaltArmada_Mitosis"] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                    Tanktosis.Color = DifficultyColor(modifier_Tanktosis);
                },
                OnRightClick = (elem) =>
                {
                    if (modifier_Tanktosis - 1 < CA_Main.Tanktosis.Single) modifier_Tanktosis = CA_Main.Tanktosis.Quad;
                    else modifier_Tanktosis -= 1;
                    Difficulties.Types["CobaltArmada_Mitosis"] = modifier_Tanktosis != CA_Main.Tanktosis.Single;

                    Tanktosis.Color = DifficultyColor(modifier_Tanktosis);
                },

                Color = DifficultyColor(modifier_Difficulty)

            };
            Tanktosis.SetDimensions(800, 700, 300, 40);
            oopsAllIdol = new("Idol Support", FontGlobals.RebirthFont, Color.White)
            {
                IsVisible = true,
                Tooltip = "Forget-Me-Nots will spawn to assist the enemy.",
                OnLeftClick = (elem) =>
                {
                    Difficulties.Types["CobaltArmada_P2"] = !Difficulties.Types["CobaltArmada_P2"];
                    oopsAllIdol.Color = Difficulties.Types["CobaltArmada_P2"] ? Color.Lime : Color.Red;
                },
                Color = Difficulties.Types["CobaltArmada_P2"] ? Color.Lime : Color.Red

            };
            oopsAllIdol.SetDimensions(800, 750, 300, 40);
            //Hooks

        }
        public static void Hook_UpdateUI()
        {
            if (MainMenuUI.Active && MainMenuUI.BulletHell.IsVisible)
            {
                Invasion.IsVisible = true;
                Touhoumode_2.IsVisible = true;
                Tankbonic_Plague.IsVisible = true;
                Tanktosis.IsVisible = true;
                oopsAllIdol.IsVisible = true;

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

            }
            else
            {
                Invasion.IsVisible = false;
                Touhoumode_2.IsVisible = false;
                Tankbonic_Plague.IsVisible = false;
                Tanktosis.IsVisible = false;
                oopsAllIdol.IsVisible = false;
            }
            //Gameplay
            boss?.Update();
            MissionDeadline?.Update();
        }

    }
}
