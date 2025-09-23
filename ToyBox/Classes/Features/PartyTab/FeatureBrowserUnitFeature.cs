using Kingmaker.EntitySystem.Entities;
using ToyBox.Infrastructure.Utilities;

namespace ToyBox.Features.PartyTab;

public partial class FeatureBrowserUnitFeature : Feature, INeedContextFeature<UnitEntityData> {
    [LocalizedString("ToyBox_Features_PartyTab_FeatureBrowserUnitFeature_Name", "Unit Feature Browser")]
    public override partial string Name { get; }
    [LocalizedString("ToyBox_Features_PartyTab_FeatureBrowserUnitFeature_Description", "Views a Browser containing all the features of the unit in question and adding/removing them")]
    public override partial string Description { get; }
    public bool GetContext(out UnitEntityData? context) => ContextProvider.UnitEntityData(out context);
    public override void OnGui() {
        UnitEntityData? unit;
        if (GetContext(out unit)) {
            OnGui(unit!);
        }
    }
    public void OnGui(UnitEntityData unit) {
        throw new NotImplementedException();
    }
}
