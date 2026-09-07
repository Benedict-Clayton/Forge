using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class DancingBladeWeak : CustomEncounterModel
{
    public DancingBladeWeak() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first", "second", "third"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override bool IsWeak => true;

    private static MonsterModel[] DancingBlade => new MonsterModel[]
    {
        ModelDb.Monster<DancingBlade>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DancingBlade>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var dancingBlade0 = (DancingBlade)ModelDb.Monster<DancingBlade>().ToMutable();;

        return new List<(MonsterModel, string?)>
        {
            (dancingBlade0, null)
        };
    }
}