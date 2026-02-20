using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.GameContent.ID;
using TanksRebirth.Net;

namespace CobaltsArmada.Script.Tanks.Class_T
{
    public class DroneParameters
    {
        public struct Skill
        {
            /// <summary>Is the drone capable of using this skill </summary>
            public bool Enabled { get; set; }

            /// <summary> Is the drone capable of using this skill when relayed </summary>
            public bool EnabledViaRelay { get; set; }

            /// <summary>Can the drone relay this task to other drone capable of this task?</summary>
            public bool RelayTaskToOthers { get; set; }

            public bool CanUse => RelayTaskToOthers || EnabledViaRelay || Enabled;


            /// <summary>Can the drone use this skill without the owner being able to see its target?</summary>
            public bool GlobalSkill { get; set; }

            /// <summary>How far can the signal be relayed to other drones</summary>
            public float RelayTaskRange { get; set; }

            /// <summary>How weak does the signal get the further the signal is</summary>
            public float RelayTaskRangeDecay { get; set; }

            private float _cooldown { get; set; }

            /// <summary>How long before the drone can use this ability</summary>
            public float Cooldown { get { return _cooldown; } set { _cooldown = value; } }


            private float _curcooldown { get; set; }

            /// <summary>The current cooldown of the ability</summary>
            public float curCooldown { get { return _curcooldown; } set { _curcooldown = MathF.Max(-1f,value); } }

            /// <summary>Checks if <see cref="curCooldown"/> is less than 0 and is enabled. </summary>
            public bool ReadyForUse => _curcooldown < 0 && (Enabled || RelayTaskToOthers);

            private float _usechance { get; set; }

            /// <summary>The chance for this to activate every 5 ticks, if possible.</summary>
            public float ChanceToActivate { get { return _usechance; } set { _usechance = MathHelper.Clamp(value, 0f, 1f); } }

            /// <summary>The minimum distance for the skill to activate</summary>
            public float Minimum { get; set; }

            /// <summary>The maximum distance for the skill to activate</summary>
            public float Maximum { get; set; }

            /// <summary>How inaccurate the command given is</summary>
            public float Inaccuracy { get; set; }

            public Skill()
            {
                Minimum = 0f;
                Maximum = float.MaxValue;
            }

            public object this[string i]
            {
                get {
                    var x = this.GetType().GetProperty(i);
                    return x.GetValue(this, null); }
            }
        }
        /// <summary>Is the drone capable of setting traps</summary>
        public Skill TrapsGeneral = new();

        /// <summary>Is the drone capable of bringing in backup</summary>
        public Skill RecruitGeneral = new();

        /// <summary>a list of tank ids that can be recruited. This list is ignored if the random modifier is active.</summary>
        public int[] ValidRecruits = [];

        /// <summary>a list of tank ids that cannot be recruited. This list is not ignored, even if the random modifier is active.</summary>
        public int[] InvalidRecruits = [];

        /// <summary>Is the drone capable of holding a position</summary>
        public Skill HoldGeneral = new();

        /// <summary>Is the drone capable of patroling </summary>
        public Skill PatrolingGeneral = new();

        /// <summary>Is the drone capable of dropping smoke bombs. Affected by the <see cref="Elite"/> parameter. </summary>
        public Skill SmokeBomberGeneral = new();

        /// <summary>Is the drone capable of dropping nightshade bombs. Affected by the <see cref="Elite"/> parameter. </summary>
        public Skill NightBomberGeneral = new();

        /// <summary>When true, the drone will find another tank to be owned by. Affected by the <see cref="Elite"/> parameter.</summary>
        public bool HitchHikerMode = false;

        public float HitchHikerRange = 100f;

        /// <summary>When true, the drone will not perish if its owner is destroyed. Affected by the <see cref="Elite"/> parameter, and synrgizes with <see cref="HitchHikerMode"/>.</summary>
        public bool CanBeOrphaned = false;

        /// <summary>The drone can turn invisible if idling. Affected by the <see cref="Elite"/> parameter.</summary>
        public bool HasCamo = false;

        /// <summary>The drone makes less noise. Affected by the <see cref="Elite"/> parameter.</summary>
        public bool SilentEngines = false;

        private int _maxarmour { get; set; }
        private int _armour { get; set; }

        /// <summary>The drones health value.</summary>
        public int Armor { get { return _armour; } set {
            if (_maxarmour == 0) { 
                _maxarmour = value;}
                _armour = Math.Clamp(value, -1, _maxarmour);
            } }

        /// <summary>The drone is capable of taking damage.</summary>
        public bool CanBeDestroyed = true;

        /// <summary>The drone is capable of taking damage from mines. Immune drones will be knocked back instead.</summary>
        public bool InvulnerableToMines = false;

        /// <summary>The drone is capable of taking damage from mines. Immune drones will be knocked back instead.</summary>
        public bool InvulnerableToShells = false;

        /// <summary>Changes the drone to a much deadlier varient, with the following changes:<list type="bullet">
        ///  <item>Can hold a reserve of up to 3 smoke and nightshade grenades before needing to reload</item>
        ///  <item>Does not fully exit camo. Holding a crate still makes it visible.</item>
        ///  <item><see cref="HitchHikerMode"/> gains global range.</item>
        ///  <item>If <see cref="CanBeOrphaned"/> is enabled, orphaned elites will act of their own volition. If an elite that is capable of recruiting becomes orphaned, it WILL recruit a tank, regardless if <see cref="RecruitGeneral"/> is on cooldown. This trait has priority over <see cref="HitchHikerMode"/></item>
        /// </list></summary>
        public bool Elite;
    }
}
