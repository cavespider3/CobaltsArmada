
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.GameContent.Tanks.AI;
using TanksRebirth.GameContent.Tanks.AI.VanillaAI;
using TanksRebirth.Internals.Common.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CobaltsArmada.AI;
/// <summary>
/// Handles centipede movement. Modified from <seealso cref="VanillaAISystem"/>
/// </summary>
public partial struct CentipedeAISystem : IAISystem
{
    #region Vanilla
    public AITank Tank { get; set; }
    // 0, 1, 2, 3
    public AIBehaviorState ChassisMovement;
    public AIBehaviorState TurretMovement;
    public AIBehaviorState ShellFire;
    public AIBehaviorState MinePlace;

    #endregion

    #region Centipede Properties
    public static AITank[][] CentipedeGroups = new AITank[GameHandler.MAX_AI_TANKS][];

    public bool[] TurretPattern = [false, true];

    public List<Vector4> SegmentPositionHistory;
    public int _segmentdelay = 1;
    public int Segments = 10;

    public int HeadID = 0;

    public int GroupID = 0;
    public int SegmentID = 0;

    public int HISTORY_DELAY { get { return Math.Max(5,48 + (int)((1.2f - Tank.Properties.MaxSpeed) * 10)); } }

    public AITank? CentipedeHead;
    public bool IsHeadSegment { get { return CentipedeHead is null || (CentipedeHead is not null && CentipedeHead.IsDestroyed); } }

    public bool IsLoaded = false;

    /// <summary>
    /// The centipede splits if a middle segment is destroyed
    /// </summary>
    public bool StupidTrain = false;
    #endregion

    public CentipedeAISystem(AITank tank, AITank? head)
    {
        Tank = tank;
        if (head is not null && tank != head) CentipedeHead = head;

        ChassisMovement.SetLabel("ChassisMovement");
        TurretMovement.SetLabel("TurretMovement");
        ShellFire.SetLabel("ShellFire");
        MinePlace.SetLabel("MinePlace");

        NearbyDangers = [];
        SegmentPositionHistory = new List<Vector4>();
    }

    public void ConstructCentipede(int segments, bool stupid = true)
    {
        if (IsLoaded) return;
        StupidTrain = stupid;
        int index = Array.IndexOf(CentipedeGroups, null);
        Segments = segments;
        GroupID = index;
        CentipedeGroups[index] = new AITank[Segments];
        CentipedeGroups[index][0] = Tank;
        SegmentID = 0;
        IsLoaded = true;
        HeadID = Tank.AITankId;
        var previous = Tank;
        for (int i = 1; i < segments; i++)
        {
            var t = new AITank(Tank.AiTankType) { Team = Tank.Team };
            CentipedeAISystem datasegment = new(t, previous);
            datasegment.GroupID = GroupID;
            datasegment.SegmentID = i;
            datasegment.IsLoaded = true;

            datasegment.HeadID = HeadID;
            datasegment.StupidTrain = StupidTrain;

            t.Physics.Position = Tank.Physics.Position;
            t.ChassisRotation = Tank.ChassisRotation;
            t.TankAI = datasegment;
            //FIX IT.
            //t.Properties.HasTurret = data.TurretPattern[(i-1) % data.TurretPattern.Length];
            CentipedeGroups[index][datasegment.SegmentID] = t;
            previous = t;
        }
    }

    public int GetCentipedeHead()
    {
        AITank? indexer = CentipedeGroups[GroupID].FirstOrDefault((x) => {
            return x is not null && !x.IsDestroyed;
        },null);
        if (indexer == null) return -1;
        return CentipedeGroups[GroupID].ToList().IndexOf(indexer);
    }

    public void AILoop()
    {

        ChassisMovement.Value += RuntimeData.DeltaTime;
        TurretMovement.Value += RuntimeData.DeltaTime;
        ShellFire.Value += RuntimeData.DeltaTime;
        MinePlace.Value += RuntimeData.DeltaTime;

        TurretRotationMultiplier = 1f;

        // Array.ForEach(Tank.Behaviors, x => x.Value += RuntimeData.DeltaTime);

        // nearby friendlies checks
        Tank.TanksNearMineAwareness.Clear();
        Tank.TanksNearShootAwareness.Clear();

        Span<Tank?> allTanks = GameHandler.AllTanks;
        ref var search = ref MemoryMarshal.GetReference(allTanks);

        for (int i = 0; i < allTanks.Length; i++)
        {
            var tank = Unsafe.Add(ref search, i);
            if (tank is null || tank == Tank || tank.IsDestroyed)
                continue;

            float distToBody = GameUtils.TanksDistance(Tank.Position, tank.Position);
            float distToTurret = GameUtils.TanksDistance(Tank.TurretPosition, tank.Position);

            if (distToBody <= Tank.Parameters.TankAwarenessMine)
                Tank.TanksNearMineAwareness.Add(tank);

            if (distToTurret <= Tank.Parameters.TankAwarenessShoot)
                Tank.TanksNearShootAwareness.Add(tank);
        }

        Tank.TargetTank = TryOverrideTarget(out bool wasOverwritten);

        if (!wasOverwritten)
            Tank.TargetTank = GetAppropriateTarget();
      

        // if (ModdedData?.CustomAI() == false) return;
        if (Tank.ModdedData is not null)
        {
            if (!Tank.ModdedData.CustomAI())
                return;
        }

        var isShellNear = NearbyDangers.Count > 0 && ClosestDanger is Shell;

        // only use if checking the respective boolean!
        var shell = (ClosestDanger as Shell)!;

        // isShellNear already accounts for the direction arc
        if (Tank.Parameters.DeflectsBullets && isShellNear && Tank.Properties.ShellLimit - Tank.OwnedShellCount > 0)
        {
            DoDeflection(shell);
        }

        HandleTurret();

        if (StupidTrain)
        {
            if (SegmentPositionHistory.Count > _segmentdelay * HISTORY_DELAY + 32)
            {
                SegmentPositionHistory.Remove(SegmentPositionHistory.Last());
            }
            SegmentPositionHistory.Insert(0, new Vector4(Tank.Position3D, Tank.ChassisRotation));
        }
        else
        {
            if (SegmentPositionHistory.Count > _segmentdelay * Segments * HISTORY_DELAY + 32)
            {
                SegmentPositionHistory.Remove(SegmentPositionHistory.Last());
            }
            SegmentPositionHistory.Insert(0, new Vector4(Tank.Position3D, Tank.ChassisRotation));
        }

        if(!IsHeadSegment)
        {
            return;
        }


            if (DoMovements)
        {
            if (Tank.Properties.Stationary && !IsHeadSegment)
                return;

            // facing down = 0 radians/2pi radians

            // "DoMovement" handles danger avoidance.
            // IsSurviving is only set every movement opportunity
            DoMovement();

            // checks if it is entirely unable to lay mines first... however
            TryMineLay();
        }

            // i really hope to remove this hardcode.
            if (DoMoveTowards)
            {
                var dir = Vector2.UnitY.RotatedBy(Tank.ChassisRotation);

                Tank.Velocity = Vector2.Normalize(dir) * Tank.Speed;
                Tank.ChassisRotation = MathUtils.RoughStep(Tank.ChassisRotation, Tank.DesiredChassisRotation, Tank.Properties.TurningSpeed * RuntimeData.DeltaTime);
            }
        }

    }
