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

        public static DroneParameters ApplyDefaultLicense(Tank tank)
        {
            var droneParams = new DroneParameters();

            if (tank is AITank ai)
            {
                ref var Traps = ref droneParams.TrapsGeneral;
                ref var Recruit = ref droneParams.RecruitGeneral;
                ref var Patrol = ref droneParams.HoldGeneral;

                var tankType = ai.AiTankType;

                //I have to hardcode this shit >:(
                if (ai.ModdedData is ModTank modTank)
                {
                    //I can see why these were removed... 
                    if (modTank.Mod.InternalName == "AdditionalTanksMarble")
                    {
                        switch (modTank.Name.GetLocalizedString(LangCode.English))
                        {
                            default: break;
                            case "Granite":

                            break;
                            case "Bubblegum":

                            break;
                            case "Water":

                            break;
                            case "Crimson":
                                Patrol.Enabled = true;
                                Patrol.Cooldown = 600;
                                Patrol.ChanceToActivate = 0.1f;
                                Patrol.Minimum = 100;
                                Patrol.Maximum = 300;
                                break;
                            case "Tiger": //you like bombs?
                                Traps.Enabled = true;
                                Traps.EnabledViaRelay = true;
                                Traps.Cooldown = 0;
                                Traps.ChanceToActivate = 1f;
                                Traps.GlobalSkill = true;
                                Traps.Inaccuracy = 120f;
                                break;
                            case "Fade":

                            break;
                            case "Creeper": //bring in... the boys
                            case "Marble":
                                Recruit.Enabled = true;
                                Recruit.Cooldown = 4;
                                Recruit.ChanceToActivate = 1f;
                                Recruit.GlobalSkill = true;

                                break;
                            case "Gamma":

                            break;
                        }

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
                            break;

                        case TankID.Citrine:
                            Traps.Enabled = true;
                            Traps.Cooldown = 0;
                            Traps.Inaccuracy = 20f;
                            Traps.ChanceToActivate = 0.01f;
                            break;

                        case TankID.Emerald:
                            Traps.EnabledViaRelay = true;
                            Traps.Cooldown = 1200;
                            Traps.GlobalSkill = true;
                            Traps.ChanceToActivate = 0.1f;
                            break;

                        case TankID.Sapphire:
                        case TankID.Silver:
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 360;
                            Patrol.ChanceToActivate = 0.4f;
                            Patrol.Minimum = 200;
                            break;

                        case TankID.Green:
                        case TankID.Ruby:
                            Patrol.Enabled = true;
                            Patrol.Cooldown = 360;
                            Patrol.ChanceToActivate = 0.7f;
                            Patrol.Maximum = 900;
                        break;

                        case TankID.Pink:
                            Recruit.Enabled = true;
                            Recruit.Cooldown = 600;
                            Recruit.ChanceToActivate = 0.1f;
                            break;
                        


                        case TankID.Gold:
                        case TankID.White:
                            Recruit.Enabled = true;
                            Recruit.GlobalSkill = true;
                            if (tankType == TankID.Gold)
                            {
                                Recruit.Cooldown = 1800;
                                Recruit.ChanceToActivate = 0.04f;
                                Recruit.Maximum = 300;
                                Traps.RelayTaskToOthers = true;
                                Traps.Cooldown = 400;
                                Traps.ChanceToActivate = 0.04f;
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
                            break;

                        case TankID.Brown:
                            Recruit.Cooldown = 900;
                            Recruit.ChanceToActivate = 0.01f;
                            Recruit.GlobalSkill = true;
                            Recruit.Minimum = 0;
                            Recruit.Maximum = 300;
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
                            Patrol.Minimum = 200;
                            Patrol.Maximum = 300;

                            Traps.Enabled = true;
                            Traps.Cooldown = 3600;
                            Traps.ChanceToActivate = 0.04f;
                            Traps.Minimum = 250;
                            Traps.Maximum = 600;
                            break;

                        #endregion
                    }
                }
                if(tankType == CA_Main.Kudzu)
                {
                    Recruit.Enabled = true;
                    Recruit.Cooldown = 1;
                    Recruit.ChanceToActivate = 1f;
                    Recruit.GlobalSkill = true;

                }else if(tankType == CA_Main.SunFlower)
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
                }
                OnApplyLicense?.Invoke(ai, ref droneParams);
            }
            //light modded support... it didn't work (for the time being) :(
           
            return droneParams;
        }
    }
}
