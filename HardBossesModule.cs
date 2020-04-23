using Mod.Courier.Module;

namespace HarderBosses {
    public class HardBossesModule : CourierModule {
        HardLeafGolem HardLeafGolem = new HardLeafGolem();
        HardNecromancer HardNecromancer = new HardNecromancer();

        public override void Load() {
            base.Load();
            HardLeafGolem.Load();
            HardNecromancer.Load();
        }

    }
}
