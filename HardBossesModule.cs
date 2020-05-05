using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.Save;
using Mod.Courier.UI;

namespace HarderBosses {
    public class HardBossesModule : CourierModule {
        HardLeafGolem HardLeafGolem = new HardLeafGolem();
        HardNecromancer HardNecromancer = new HardNecromancer();
        HardBambooCreek HardBambooCreek = new HardBambooCreek();
        HardEmeraldGolem HardEmeraldGolem = new HardEmeraldGolem();

        public ToggleButtonInfo leafGolemButton,
            necromancerButton,
            bambooCreekButton,
            emeraldGolemButton;

        public static HardBossesModule Instance { get; private set; }

        public HardBossesModule() {
            Instance = this;
        }

        public override void Load() {
            base.Load();
            leafGolemButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Leaf Golem", HardLeafGolem.Toggle, (t) => HardLeafGolem.Enabled);
            necromancerButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Ruxxtin", HardNecromancer.Toggle, (t) => HardNecromancer.Enabled);
            bambooCreekButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Bamboo Creek Boss", HardBambooCreek.Toggle, (t) => HardBambooCreek.Enabled);
            emeraldGolemButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Emerald Golem", HardEmeraldGolem.Toggle, (t) => HardEmeraldGolem.Enabled);

            // Disable the toggles if in-level
            // Some of these need to be applied before anything is loaded
            // For example, stuff runs in the Start() method for Emerald Golem
            leafGolemButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            necromancerButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            bambooCreekButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            emeraldGolemButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;

            leafGolemButton.SaveMethod = new BooleanOptionSaveMethod("HarderBossesLeafGolem", () => HardLeafGolem.Enabled, (b) => {
                HardLeafGolem.Unload();
                if (HardLeafGolem.Enabled = b) {
                    HardLeafGolem.Load();
                }
            });
            necromancerButton.SaveMethod = new BooleanOptionSaveMethod("HarderBossesNecromancer", () => HardNecromancer.Enabled, (b) => {
                HardNecromancer.Unload();
                if (HardNecromancer.Enabled = b) {
                    HardNecromancer.Load();
                }
            });
            bambooCreekButton.SaveMethod = new BooleanOptionSaveMethod("HarderBossesBambooCreek", () => HardBambooCreek.Enabled, (b) => {
                HardBambooCreek.Unload();
                if (HardBambooCreek.Enabled = b) {
                    HardBambooCreek.Load();
                }
            });
            emeraldGolemButton.SaveMethod = new BooleanOptionSaveMethod("HarderBossesEmeraldGolem", () => HardEmeraldGolem.Enabled, (b) => {
                HardEmeraldGolem.Unload();
                if (HardEmeraldGolem.Enabled = b) {
                    HardEmeraldGolem.Load();
                }
            });

        }

    }
}
