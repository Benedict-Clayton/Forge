using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class FirekeeperDuoElite : CustomEncounterModel
{
    public FirekeeperDuoElite() : base(RoomType.Elite)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first", "second"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    private static MonsterModel[] Pyreling => new MonsterModel[]
    {
        ModelDb.Monster<Firekeeper>(),
        ModelDb.Monster<Firekeeper>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Firekeeper>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var firekeeper0 = (Firekeeper)ModelDb.Monster<Firekeeper>().ToMutable();
        var firekeeper1 = (Firekeeper)ModelDb.Monster<Firekeeper>().ToMutable();

        firekeeper0.InfernoFirst = false;
        firekeeper1.InfernoFirst = true;

        return new List<(MonsterModel, string?)>
        {
            (firekeeper0, null),
            (firekeeper1, null)
        };
    }
}