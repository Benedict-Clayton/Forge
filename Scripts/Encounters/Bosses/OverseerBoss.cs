using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Godot;

namespace Forge;

public sealed class OverseerBoss : CustomEncounterModel
{
    public const string _overseerSlot = "overseer";
    public const string _servoSlotPrefix = "servo";

    public OverseerBoss() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    // public override bool HasScene => true;

    public override IReadOnlyList<string> Slots
    {
        get
        {
            return new string[]
            {
                "servo1",
                "servo2",
                "servo3",
                "servo4",
                "overseer"
            };
        }
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override float GetCameraScaling() => 0.85f;
    public override Vector2 GetCameraOffset() => Vector2.Down * 40f;

    private static MonsterModel[] Overseer => new MonsterModel[]
    {
        ModelDb.Monster<Overseer>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            return new MonsterModel[]
            {
                ModelDb.Monster<Overseer>(),
                ModelDb.Monster<ServoA>()
            };
        }
    }


    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var overseer0 = (Overseer)ModelDb.Monster<Overseer>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (overseer0, _overseerSlot)
        };
    }
}