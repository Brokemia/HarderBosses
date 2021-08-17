using Mod.Courier.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HarderBosses {
    [Serializable]
    public class HarderBossesSave : CourierModSave {
        [SerializeField]
        private bool _hardLeafGolem;

        public bool hardLeafGolem {
            get { return _hardLeafGolem; }
            set {
                HardBossesModule.Instance.HardLeafGolem.Unload();
                if (_hardLeafGolem = value) {
                    HardBossesModule.Instance.HardLeafGolem.Load();
                }
            } }

        [SerializeField]
        private bool _hardNecromancer;

        public bool hardNecromancer {
            get { return _hardNecromancer; }
            set {
                HardBossesModule.Instance.HardNecromancer.Unload();
                if (_hardNecromancer = value) {
                    HardBossesModule.Instance.HardNecromancer.Load();
                }
            }
        }

        [SerializeField]
        private bool _hardBambooCreek;

        public bool hardBambooCreek {
            get { return _hardBambooCreek; }
            set {
                HardBossesModule.Instance.HardBambooCreek.Unload();
                if (_hardBambooCreek = value) {
                    HardBossesModule.Instance.HardBambooCreek.Load();
                }
            }
        }

        [SerializeField]
        private bool _hardEmeraldGolem;

        public bool hardEmeraldGolem {
            get { return _hardEmeraldGolem; }
            set {
                HardBossesModule.Instance.HardEmeraldGolem.Unload();
                if (_hardEmeraldGolem = value) {
                    HardBossesModule.Instance.HardEmeraldGolem.Load();
                }
            }
        }

        [SerializeField]
        private bool _hardQoQ;

        public bool hardQoQ {
            get { return _hardQoQ; }
            set {
                HardBossesModule.Instance.HardQoQ.Unload();
                if (_hardQoQ = value) {
                    HardBossesModule.Instance.HardQoQ.Load();
                }
            }
        }

        [SerializeField]
        private bool _hardColossuses;

        public bool hardColossuses {
            get { return _hardColossuses; }
            set {
                HardBossesModule.Instance.HardColossuses.Unload();
                if (_hardColossuses = value) {
                    HardBossesModule.Instance.HardColossuses.Load();
                }
            }
        }
    }
}
