using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            public float curCooldown { get { return _curcooldown; } set { _curcooldown = MathF.Max(-1f,value); } }

            /// <summary>How long before the drone can use this ability</summary>
            public bool ReadyForUse => _curcooldown < 0 && (Enabled || RelayTaskToOthers);

            private float _usechance { get; set; }

            /// <summary>The chance for this to activate each frame, if possible.</summary>
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

        /// <summary>Is the drone capable of holding a position</summary>
        public Skill HoldGeneral = new();

        /// <summary>Is the drone capable of patroling </summary>
        public Skill PatrolingGeneral = new();

        public int Armor;

    }
}
