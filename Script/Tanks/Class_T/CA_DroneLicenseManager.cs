using FontStashSharp.Rasterizers.StbTrueTypeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.AI;
using TanksRebirth.GameContent.Systems.TankSystem;
using TanksRebirth.Localization;
using static tainicom.Aether.Physics2D.Common.PathManager;

namespace CobaltsArmada.Script.Tanks.Class_T
{
    public class CA_DroneLicenseManager
    {

        public delegate void ApplyLicense(AITank tank, ref DroneParameters parameters);
        public static event ApplyLicense? OnApplyLicense;

        public static string[] MarbleTanks = { "Granite", "Bubblegum", "Water", "Crimson", "Tiger", "Fade", "Creeper", "Marble", "Gamma" };
        public static DroneParameters ApplyDefaultLicense(Tank tank,int? Override = null)
        {

            var droneParams = new DroneParameters();

            if (tank is AITank ai)
            {
                
                ref var Traps = ref droneParams.TrapsGeneral;
                ref var Recruit = ref droneParams.RecruitGeneral;
                ref var Patrol = ref droneParams.HoldGeneral;
                ref var Smoke = ref droneParams.SmokeBomberGeneral;
                ref var Night = ref droneParams.NightBomberGeneral;

                var tankType = Override ?? ai.AiTankType;
                var tierName = TankID.Collection.GetKey(tankType)!;

                //I have to hardcode this shit >:(
                if (MarbleTanks.Contains(tierName))
                {
                    //I can see why these were removed... 

                        droneParams.Elite = true;
                        switch (tierName)
                        {
                            default: break;
                            case "Granite":
                            case "Bubblegum":
                            case "Fade":
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 600;
                            Patrol.ChanceToActivate = 0.05f;
                            Patrol.Minimum = 100;
                            droneParams.Armor = 3;
                            Smoke.Enabled = tierName != "Granite";
                            Smoke.Cooldown = 600;
                            Smoke.ChanceToActivate = 0.1f;
                            Smoke.Minimum = 100;
                            droneParams.HasCamo = tierName == "Fade";

                            break;
                            case "Water":
                            case "Crimson":
                                Patrol.Enabled = true;
                                Patrol.Cooldown = 600;
                                Patrol.ChanceToActivate = 0.1f;
                                Patrol.Minimum = 100;
                                Patrol.Maximum = 300;
                                droneParams.Armor = 3;
                                break;
                            case "Tiger": //you like bombs?
                                Traps.Enabled = true;
                                Traps.EnabledViaRelay = true;
                                Traps.Cooldown = 0;
                                Traps.ChanceToActivate = 1f;
                                Traps.GlobalSkill = true;
                                Traps.Inaccuracy = 120f;
                                droneParams.Elite = true;
                                droneParams.InvulnerableToMines = true;
                                droneParams.Armor = 6;
                                break;
                            case "Creeper": //bring in... the boys
                            case "Marble":
                                Recruit.Enabled = true;
                                Recruit.Cooldown = 4;
                                Recruit.ChanceToActivate = 1f;
                                Recruit.GlobalSkill = true;
                                droneParams.CanBeOrphaned = true;
                                droneParams.InvulnerableToShells = true;
                                droneParams.Armor = 9;
                                if (tierName == "Marble")
                                {
                                    droneParams.HitchHikerMode = true; 
                                }
                                break;
                            case "Gamma":
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 600;
                            Patrol.ChanceToActivate = 0.04f;
                            Patrol.Minimum = 400;
                            droneParams.HasCamo = true;
                            break;
                        }

                    

                }
                else
                {
                    switch (tankType)
                    {
                        default: break;
                        #region VanillaTanks

                        case TankID.Yellow:
                            Traps.Enabled = true;
                            Traps.EnabledViaRelay =true;
                            Traps.Cooldown = 600;
                            Traps.Inaccuracy = 70f;
                            Traps.GlobalSkill = true;
                            Traps.ChanceToActivate = 0.1f;
                            droneParams.InvulnerableToMines = true;
                            break;

                        case TankID.Citrine:
                            Traps.Enabled = true;
                            Traps.Cooldown = 0;
                            Traps.Inaccuracy = 20f;
                            Traps.ChanceToActivate = 0.01f;
                            droneParams.HitchHikerMode = true;
                            droneParams.HitchHikerRange = 900f;
                            droneParams.InvulnerableToMines = true;
                            droneParams.Elite = true;
                            break;

                        case TankID.Emerald:
                            Traps.EnabledViaRelay = true;
                            Traps.Cooldown = 1200;
                            Traps.GlobalSkill = true;
                            Traps.ChanceToActivate = 0.1f;
                            droneParams.SilentEngines = true;
                            droneParams.HasCamo = true;
                            droneParams.Elite = true;
                            break;

                        case TankID.Sapphire:
                        case TankID.Silver:
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 360;
                            Patrol.ChanceToActivate = 0.4f;
                            Patrol.Minimum = 200;
                            droneParams.CanBeOrphaned = true;
                            droneParams.Armor = 3;
                            
                            break;

                        case TankID.Green:
                        case TankID.Ruby:
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 360;
                            Patrol.ChanceToActivate = 0.7f;
                            Patrol.Maximum = 900;
                            droneParams.InvulnerableToShells = true;
                            droneParams.CanBeOrphaned = tankType == TankID.Green;
                            droneParams.Armor = tankType == TankID.Green ? 3 : 6;
                            break;

                        case TankID.Pink:
                            Recruit.Enabled = true;
                            Recruit.Cooldown = 600;
                            Recruit.ChanceToActivate = 0.1f;
                            droneParams.Armor = 2;
                            break;
                        


                        case TankID.Gold:
                        case TankID.White:
                            Recruit.Enabled = true;
                            Recruit.GlobalSkill = true;
                            droneParams.HasCamo = true;   
                            
                            if (tankType == TankID.Gold)
                            {
                                droneParams.Elite = true;
                                Recruit.Cooldown = 1800;
                                Recruit.ChanceToActivate = 0.04f;
                                Recruit.Maximum = 300;
                                Traps.RelayTaskToOthers = true;
                                Traps.Cooldown = 400;
                                Traps.ChanceToActivate = 0.04f;
                                droneParams.SilentEngines = true;
             
                            }
                            else
                            {    
                                Recruit.Cooldown = 3600;
                                Recruit.ChanceToActivate = 0.01f;
                               
                            }


                            break;

                        case TankID.Marine:
                        case TankID.Ash:
                            Recruit.EnabledViaRelay = true;
                            Recruit.Cooldown = 3600;
                            Recruit.ChanceToActivate = 0.5f;
                            Recruit.GlobalSkill = true;
                            Recruit.Minimum = 100;
                            Recruit.Maximum = 400;

                            Patrol.Enabled = true;
                            Patrol.Cooldown = 360;
                            Patrol.ChanceToActivate = 0.03f;
                            Patrol.Maximum = 400;
                            droneParams.CanBeOrphaned = true;
                            break;

                        case TankID.Brown:
                            Recruit.EnabledViaRelay = true;
                            Recruit.Cooldown = 3600;
                            Recruit.ChanceToActivate = 0.5f;
                            Recruit.GlobalSkill = true;
                            Recruit.Minimum = 100;
                            Recruit.Maximum = 400;
                            droneParams.HitchHikerMode = true;
                            break;


                        case TankID.Bronze:
                            Recruit.RelayTaskToOthers = true;
                            Recruit.RelayTaskRange = 600;
                            Recruit.Cooldown = 500;
                            Recruit.ChanceToActivate = 0.08f;
                            Recruit.GlobalSkill = true;

                            Patrol.RelayTaskToOthers = true;
                            Patrol.RelayTaskRange = 300;
                            Patrol.Cooldown = 500;
                            Patrol.ChanceToActivate = 0.12f;
                            Patrol.GlobalSkill = true;

                            droneParams.CanBeOrphaned = true;
                            droneParams.HitchHikerMode = true;
                            droneParams.Armor = 1;
                            break;

                        
                        case TankID.Obsidian:
                        case TankID.Violet:
                        case TankID.Amethyst:
                        case TankID.Black:
                            Recruit.Enabled = true;
                            Recruit.Cooldown = 3600;
                            Recruit.ChanceToActivate = 0.5f;
                            Recruit.GlobalSkill = true;
                            Recruit.Minimum = 100;
                            Recruit.Maximum = 400;

                            Patrol.Enabled = true;
                            Patrol.Cooldown = 600;
                            Patrol.ChanceToActivate = 0.1f;
                            Patrol.Minimum = 150;
                            Patrol.Maximum = 1200;
                            Patrol.GlobalSkill = false;

                            Traps.Enabled = true;
                            Traps.Cooldown = 3600;
                            Traps.ChanceToActivate = 0.04f;
                            Traps.Minimum = 250;
                            Traps.Maximum = 600;
                            droneParams.Elite = true;
                            droneParams.HitchHikerMode = tankType == TankID.Black || tankType == TankID.Obsidian;
                            droneParams.CanBeOrphaned = true;
                            droneParams.Armor = 3;
                         
                            break;

                        #endregion
                    }
                }
                if(tankType == CA_Main.Kudzu || tankType == CA_Main.NightShade)
                {
                    Recruit.Enabled = true;
                    Recruit.Cooldown = tankType == CA_Main.NightShade ? 120 : 1;
                    Recruit.ChanceToActivate = tankType == CA_Main.NightShade ? 0.1f : 0.0025f;
                    Recruit.GlobalSkill = true;
                    droneParams.Elite = tankType == CA_Main.NightShade;
                    droneParams.CanBeDestroyed = tankType != CA_Main.NightShade;
                    if(tankType == CA_Main.NightShade)
                    {
                        Night.Enabled = true;
                        Night.Cooldown = 600;
                        Night.ChanceToActivate = 0.03f;
                        Night.GlobalSkill = true;
                    }
                    else
                    {
                        droneParams.HitchHikerMode = true;
                        droneParams.HitchHikerRange = float.MaxValue;
                    }

                }
                else if (tankType == CA_Main.Lily)
                {
                    Night.Enabled = true;
                    Night.Cooldown = 800;
                    Night.GlobalSkill = true;
                    Night.ChanceToActivate = 0.01f;

                }
                else if (tankType == CA_Main.SunFlower)
                {
                    Traps.Enabled = true;
                    Traps.Cooldown = 0;
                    Traps.Inaccuracy = 20f;
                    Traps.ChanceToActivate = 0.01f;
                }
                else if (tankType == CA_Main.Carnation)
                {
                    Patrol.Enabled = true;
                    Patrol.Cooldown = 600;
                    Patrol.ChanceToActivate = 0.1f;
                    Patrol.Minimum = 200;
                    Patrol.Maximum = 300;

                    Traps.Enabled = true;
                    Traps.Cooldown = 3600;
                    Traps.ChanceToActivate = 0.04f;
                    Traps.Minimum = 250;
                    Traps.Maximum = 600;
                    droneParams.InvulnerableToMines = true;
                    droneParams.Elite = true;
                }
                else if (tankType == CA_Main.Lotus)
                {
                    Traps.Enabled = true;
                    Traps.Cooldown = 600;
                    Traps.ChanceToActivate = 0.04f;
                    droneParams.CanBeDestroyed = false;
                }
                else
                {
                    Patrol.Enabled = true;
                    Patrol.Cooldown = 360;
                    Patrol.ChanceToActivate = 0.03f;
                    Patrol.Maximum = 600;
                }
                    OnApplyLicense?.Invoke(ai, ref droneParams);
            }
            //light modded support... it didn't work (for the time being) :(
           
            return droneParams;
        }
    }
}
