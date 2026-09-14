using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class SwordBoxNormal : CustomEncounterModel
{
    public SwordBoxNormal() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first", "second"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    private static MonsterModel[] Pyreling => new MonsterModel[]
    {
        ModelDb.Monster<OrbInABox>(),
        ModelDb.Monster<DancingBlade>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<OrbInABox>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var orbInABox0 = (OrbInABox)ModelDb.Monster<OrbInABox>().ToMutable();
        var dancingSword1 = (DancingBlade)ModelDb.Monster<DancingBlade>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (orbInABox0, null),
            (dancingSword1, null)
        };
    }
}