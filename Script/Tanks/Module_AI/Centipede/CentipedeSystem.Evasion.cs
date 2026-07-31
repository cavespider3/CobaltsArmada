using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.GameContent.Tanks.AI;
using TanksRebirth.Internals.Common.Utilities;

namespace CobaltsArmada.AI;
public partial struct CentipedeAISystem
{
    public bool IsInDanger;
    public volatile List<IAITankDanger> NearbyDangers;
    public IAITankDanger? ClosestDanger;

    readonly List<IAITankDanger> _evasionDangersBuffer = [];
    /// <summary>Makes this <see cref="AITank"/> profusely avoid the given location.</summary>
    public void Avoid(Vector2 location)
    {
        IsSurviving = true;
        if (Tank.CurMineStun <= 0 && Tank.CurShootStun <= 0)
        {
            var direction = Tank.Position - location;
            //The Centipede cannot turn so easily :[
            Vector2 clock_check = Tank.Position;
            float clock_rotation = Tank.ChassisRotation;
            for (var i = 0;i < 3; i++)
            {
                clock_rotation += Tank.Properties.TurningSpeed * 2f;
                clock_check += Vector2.UnitX.RotatedBy(clock_rotation) * Tank.Properties.MaxSpeed;
            }

            Vector2 counter_check = Tank.Position;
            float counter_rotation = Tank.ChassisRotation;
            for (var i = 0; i < 3; i++)
            {
                counter_rotation -= Tank.Properties.TurningSpeed * 2f;
                counter_check += Vector2.UnitX.RotatedBy(clock_rotation) * Tank.Properties.MaxSpeed;
            }
            Tank.DesiredChassisRotation = Vector2.Distance(counter_check, location) < Vector2.Distance(clock_check, location) ?Tank.Position.DirectionTo(counter_check).ToRotation() : Tank.DesiredChassisRotation = Tank.Position.DirectionTo(counter_check).ToRotation();
        }
    }
    /// <summary>Gets a list of dangerous objects near the <see cref="AITank"/>.</summary>
    public List<IAITankDanger> GetEvasionData()
    {
        _evasionDangersBuffer.Clear();

        foreach (var danger in AITank.Dangers)
        {
            var isHostile = !Tank.IsOnSameTeamAs(danger.Team);

            // mines and explosions should be treated differently and specially
            if (danger is Mine || danger is Explosion)
            {
                var isCloseEnough = GameUtils.TanksDistance(Tank.Position, danger.Position) <=
                    (isHostile ? Tank.Parameters.AwarenessHostileMine : Tank.Parameters.AwarenessFriendlyMine);

                if (isCloseEnough)
                {
                    _evasionDangersBuffer.Add(danger);
                    IsSurviving = true;
                }
            }
            else if (danger is Shell shell)
            {
                var isHeadingTowards = shell.IsHeadingTowards(Tank.Position, isHostile ? Tank.Parameters.AwarenessHostileShell : Tank.Parameters.AwarenessFriendlyShell, MathHelper.Pi);
                // already accounts for hostility via the above ^
                if (isHeadingTowards)
                {
                    _evasionDangersBuffer.Add(danger);
                    IsSurviving = true;
                }
            }
            // non-vanilla sources of danger
            else
            {
                _evasionDangersBuffer.Add(danger);
                IsSurviving = true;
            }
        }
        return _evasionDangersBuffer;
    }
    // this might need to be redone completely because different dangers have difernernejakswklfsadkolf dasjkl fsadjklsaf dkjhlsfda jhknas dfjhkbsadf jhkbsadf jhkfsa djkhsa fd
    [Obsolete("This method is outdated and may not work as expected. Use GetEvasionData() instead.")]
    public bool TryGetDangerNear(float distance, out List<IAITankDanger> dangersNear, out IAITankDanger? dClosest)
    {
        IAITankDanger? closest = null;
        dangersNear = [];

        Span<IAITankDanger> dangers = AITank.Dangers.ToArray();

        ref var dangersSearchSpace = ref MemoryMarshal.GetReference(dangers);

        for (var i = 0; i < AITank.Dangers.Count; i++)
        {
            var currentDanger = Unsafe.Add(ref dangersSearchSpace, i);

            if (currentDanger is null) continue;

            var distanceToDanger = GameUtils.TanksDistance(Tank.Position, currentDanger.Position);

            if (!(distanceToDanger < distance)) continue;

            dangersNear.Add(currentDanger);

            if (closest == null || distanceToDanger <
                GameUtils.TanksDistance(Tank.Position, closest.Position))
            {
                closest = currentDanger;
            }
        }

        dClosest = closest;
        return closest != null;
    }
}
