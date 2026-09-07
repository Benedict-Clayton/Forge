using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class PyrelingNormal : CustomEncounterModel
{
    public PyrelingNormal() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first", "second", "third", "fourth"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    // public override string? CustomScenePath => "res://Forge/scenes/encounters/Forge-pyreling_weak.tscn";

    private static MonsterModel[] Pyreling => new MonsterModel[]
    {
        ModelDb.Monster<Pyreling>(),
        ModelDb.Monster<Pyreling>(),
        ModelDb.Monster<Pyreling>(),
        ModelDb.Monster<Pyreling>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Pyreling>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var pyreling0 = (Pyreling)ModelDb.Monster<Pyreling>().ToMutable();
        var pyreling1 = (Pyreling)ModelDb.Monster<Pyreling>().ToMutable();
        var pyreling2 = (Pyreling)ModelDb.Monster<Pyreling>().ToMutable();
        var pyreling3 = (Pyreling)ModelDb.Monster<Pyreling>().ToMutable();

        pyreling0.BurnFirst = false;
        pyreling1.BurnFirst = true;
        pyreling2.BurnFirst = false;
        pyreling3.BurnFirst = true;

        return new List<(MonsterModel, string?)>
        {
            (pyreling0, null),
            (pyreling1, null),
            (pyreling2, null),
            (pyreling3, null)
        };
    }
}