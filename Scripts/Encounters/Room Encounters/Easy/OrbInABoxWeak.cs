using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class OrbInABoxWeak : CustomEncounterModel
{
    public OrbInABoxWeak() : base(RoomType.Monster)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first", "second"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    public override bool IsWeak => true;

    // public override string? CustomScenePath => "res://Forge/scenes/encounters/Forge-pyreling_weak.tscn";

    private static MonsterModel[] Pyreling => new MonsterModel[]
    {
        ModelDb.Monster<OrbInABox>(),
        ModelDb.Monster<OrbInABox>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Pyreling>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var orbInABox0 = (OrbInABox)ModelDb.Monster<OrbInABox>().ToMutable();
        var orbInABox1 = (OrbInABox)ModelDb.Monster<OrbInABox>().ToMutable();

        return new List<(MonsterModel, string?)>
        {
            (orbInABox0, null),
            (orbInABox1, null)
        };
    }
}