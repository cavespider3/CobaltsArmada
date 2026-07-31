using CobaltsArmada.Objects.projectiles.futuristic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.GameContent.UI.MainMenu;
using TanksRebirth.Graphics;
using TanksRebirth.Internals;
using TanksRebirth.Internals.Common.Utilities;

namespace CobaltsArmada
{
    /// <summary>
    /// The basis for spirit-like entities that possess tanks, giving them special properties
    /// </summary>
    public abstract class CA_Soul
    {
        const int MAX_SOULS = 64;

        public static CA_Soul[] AllSouls = new CA_Soul[MAX_SOULS];
        public int Id { get; private set; }

        public Vector3 Position;
        public Vector3 Velocity;
        public float Acceleration = 4.0f;

        /// <summary>
        /// The number of times this <see cref="CA_Soul"/> can move to another target
        /// </summary>
        public int Hauntings = 3;
        public bool IsHaunting;
        public Tank? HauntingTarget;

        public int Team;

        public virtual void OnHaunt(ref Tank tank) { }

        public CA_Soul(Tank? target,Tank owner) {
            Team = owner.Team;
            Position = owner.Position3D + Vector3.Up * 20f;

            int index = Array.IndexOf(AllSouls, null);
            Id = index;
            AllSouls[index] = this;
            if (target is null)
            {
               float distance = float.MaxValue;
               float _distance = float.MaxValue;
               Tank? soulless_induvidual = null;
               foreach(Tank tank in GameHandler.AllTanks)
               {
                    if (tank is null || tank.IsDestroyed || (tank.Team != Team && Team != TeamID.NoTeam)) continue;
                    if (AllSouls.Any((CA_Soul x) => { return x is not null && x.HauntingTarget == tank; })) continue;
                    distance = MathF.Min(Vector3.Distance(tank.Position3D, Position), distance);
                    if (distance < _distance ) soulless_induvidual = tank;

               }
               if (soulless_induvidual is null)
               {
                    Remove();
                    return;
               }
               HauntingTarget = soulless_induvidual;
            } 
        }

        public void Update()
        {
            if (!GameScene.UpdateAndRender || (!CampaignGlobals.InMission && !MainMenuUI.IsActive))
                return;
            if (HauntingTarget is null || HauntingTarget.IsDestroyed)
            {
                IsHaunting = false;
                if (Hauntings == 0)
                {
                    Remove();
                    return;
                }
                float distance = float.MaxValue;
                float _distance = float.MaxValue;
                Tank? soulless_induvidual = null;
                foreach (Tank tank in GameHandler.AllTanks)
                {
                    if (tank is null || tank.IsDestroyed || (tank.Team != Team && Team != TeamID.NoTeam)) continue;
                    if (AllSouls.Any((CA_Soul x) => { return x is not null && x.HauntingTarget == tank; })) continue;
                    distance = MathF.Min(Vector3.Distance(tank.Position3D, Position), distance);
                    if (distance < _distance) soulless_induvidual = tank;

                }
                if (soulless_induvidual is null)
                {
                    Remove();
                    return;
                }
                HauntingTarget = soulless_induvidual;
            }
            if (!IsHaunting)
            {
                float delta = RuntimeData.DeltaTime / 60f;

                var glow1 = GameHandler.Particles.MakeParticle(Position, GameResources.GetGameResource<Texture2D>("Assets/textures/misc/light_particle"));
                glow1.Scale = Vector3.One;

                glow1.HasAdditiveBlending = true;
                glow1.Color = Color.Red;
                glow1.FaceTowardsMe = true;

                glow1.UniqueBehavior = (a) =>
                {
                    glow1.Alpha -= 0.75f * RuntimeData.DeltaTime / 60f;

                    glow1.Scale = Vector3.One * glow1.Alpha;
                    if (glow1.Alpha <= 0)
                        glow1.Destroy();
                };

                Velocity += MathUtils.DirectionTo(Position.FlattenZ(), HauntingTarget!.Position).ExpandZ() * Acceleration * delta;
                Position += Velocity * delta;


                if (Vector3.Distance(Position, HauntingTarget.Position3D) <= Tank.TNK_WIDTH)
                {
                    OnHaunt(ref HauntingTarget);
                    IsHaunting = true;
                    Hauntings -= 1;
                }
            }
            else
            {
                Position = HauntingTarget.Position3D;
                Velocity = Vector3.Zero;
            }
        }

        public void Remove()
        {

            AllSouls[Id] = null;
        }







    }
}
