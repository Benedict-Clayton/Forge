using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class SnekorbNormal : CustomEncounterModel
{
    public SnekorbNormal() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    private static MonsterModel[] Sneckorb => new MonsterModel[]
    {
        ModelDb.Monster<Sneckorb>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Sneckorb>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var sneckorb0 = (Sneckorb)ModelDb.Monster<Sneckorb>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (sneckorb0, null)
        };
    }
}