using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Forge;

public sealed class LivingLodestoneElite : CustomEncounterModel
{
    public LivingLodestoneElite() : base(RoomType.Elite)
    {
    }

    public override bool IsValidForAct(ActModel act) => act is ForgeAct;

    private static readonly string[] SlotNames = ["first"];

    public override IEnumerable<EncounterTag> Tags => Array.Empty<EncounterTag>();

    private static MonsterModel[] LivingLodestone => new MonsterModel[]
    {
        ModelDb.Monster<LivingLodestone>()
    };

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<LivingLodestone>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var livingLodestone0 = (LivingLodestone)ModelDb.Monster<LivingLodestone>().ToMutable(); ;

        return new List<(MonsterModel, string?)>
        {
            (livingLodestone0, null)
        };
    }
}