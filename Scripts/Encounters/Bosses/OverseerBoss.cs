using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class OverseerBoss : CustomEncounterModel
{
    public const string _overseerSlot = "overseer";
    public const string _servoSlotPrefix = "servo";

    public OverseerBoss() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first"];

    public override IReadOnlyList<string> Slots
    {
        get
        {
            return (IReadOnlyList<string>)new \u003C\u003Ez__ReadOnlyArray<string>(new string[5]
            {
                "servo1",
                "servo2",
                "overseer",
                "servo3",
                "servo4"
            });
        }
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override float GetCameraScaling() => 0.85f;
    public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

    private static MonsterModel[] Overseer => new MonsterModel[]
    {
        ModelDb.Monster<Overseer>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Overseer>()];


    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var overseer0 = (Overseer)ModelDb.Monster<Overseer>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (overseer0, null)
        };
    }
}