using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class OxidizerNormal : CustomEncounterModel
{
    public OxidizerNormal() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    private static MonsterModel[] Oxidizer => new MonsterModel[]
    {
        ModelDb.Monster<Oxidizer>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Oxidizer>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var oxidizer0 = (Oxidizer)ModelDb.Monster<Oxidizer>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (oxidizer0, null)
        };
    }
}