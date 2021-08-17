using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.Save;
using Mod.Courier.UI;
using System;

namespace HarderBosses {
    public class HardBossesModule : CourierModule {
        public HardLeafGolem HardLeafGolem = new HardLeafGolem();
        public HardNecromancer HardNecromancer = new HardNecromancer();
        public HardBambooCreek HardBambooCreek = new HardBambooCreek();
        public HardEmeraldGolem HardEmeraldGolem = new HardEmeraldGolem();
        public HardQoQ HardQoQ = new HardQoQ();
        public HardColossuses HardColossuses = new HardColossuses();

        public ToggleButtonInfo leafGolemButton,
            necromancerButton,
            bambooCreekButton,
            emeraldGolemButton,
            qoqButton,
            colossusesButton;

        public override Type ModuleSaveType => typeof(HarderBossesSave);

        public static HardBossesModule Instance { get; private set; }

        public static HarderBossesSave Save => (HarderBossesSave)Instance.ModuleSave;

        public HardBossesModule() {
            Instance = this;
        }

        public override void Load() {
            base.Load();
            leafGolemButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Leaf Golem", HardLeafGolem.Toggle, (t) => HardLeafGolem.Enabled);
            necromancerButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Ruxxtin", HardNecromancer.Toggle, (t) => HardNecromancer.Enabled);
            bambooCreekButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Bamboo Creek Boss", HardBambooCreek.Toggle, (t) => HardBambooCreek.Enabled);
            emeraldGolemButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Emerald Golem", HardEmeraldGolem.Toggle, (t) => HardEmeraldGolem.Enabled);
            qoqButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Queen of Quills", HardQoQ.Toggle, (t) => HardQoQ.Enabled);
            colossusesButton = Courier.UI.RegisterToggleModOptionButton(() => "Hard Colos & Suses", HardColossuses.Toggle, (t) => HardColossuses.Enabled);

            // Disable the toggles if in-level
            // Some of these need to be applied before anything is loaded
            // For example, stuff runs in the Start() method for Emerald Golem
            leafGolemButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            necromancerButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            bambooCreekButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            emeraldGolemButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            qoqButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            colossusesButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;

        }

        public override void SaveLoaded() {
            Save.hardLeafGolem = Save.hardLeafGolem;
            Save.hardNecromancer = Save.hardNecromancer;
            Save.hardBambooCreek = Save.hardBambooCreek;
            Save.hardEmeraldGolem = Save.hardEmeraldGolem;
            Save.hardQoQ = Save.hardQoQ;
            Save.hardColossuses = Save.hardColossuses;
        }

    }
}
