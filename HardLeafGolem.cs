using System;
using MonoMod.Utils;

namespace HarderBosses {
    public class HardLeafGolem {

        public bool Enabled => HardBossesModule.Save.hardLeafGolem;

        public void Toggle() {
            HardBossesModule.Save.hardLeafGolem = !HardBossesModule.Save.hardLeafGolem;
            HardBossesModule.Instance.leafGolemButton.UpdateStateText();
        }

        public void Load() {
            On.LeafGolemBoss.Start += LeafGolemBoss_Start;
            On.LeafGolemBoss.Explode += LeafGolemBoss_Explode;
            On.LeafGolemBoss.StopExploding += LeafGolemBoss_StopExploding;
            On.LeafGolemBoss.Kill += LeafGolemBoss_Kill;

            On.LeafGolemThrowLeavesSpecialState.OnThrow += ThrowLeavesSpecialStateReplace.LeafGolemThrowLeavesSpecialState_OnThrow;
            On.LeafGolemThrowLeavesSpecialState.StateExit += ThrowLeavesSpecialStateReplace.LeafGolemThrowLeavesSpecialState_StateExit;

            On.LeafGolemDeadState.StateEnter += LeafGolemDeadState_StateEnter;
        }

        public void Unload() {
            On.LeafGolemBoss.Start -= LeafGolemBoss_Start;
            On.LeafGolemBoss.Explode -= LeafGolemBoss_Explode;
            On.LeafGolemBoss.StopExploding -= LeafGolemBoss_StopExploding;
            On.LeafGolemBoss.Kill -= LeafGolemBoss_Kill;
            
            On.LeafGolemThrowLeavesSpecialState.OnThrow -= ThrowLeavesSpecialStateReplace.LeafGolemThrowLeavesSpecialState_OnThrow;
            On.LeafGolemThrowLeavesSpecialState.StateExit -= ThrowLeavesSpecialStateReplace.LeafGolemThrowLeavesSpecialState_StateExit;

            On.LeafGolemDeadState.StateEnter -= LeafGolemDeadState_StateEnter;
        }

        void LeafGolemBoss_Start(On.LeafGolemBoss.orig_Start orig, LeafGolemBoss self) {
            orig(self);
            ThrowLeavesStateReplace stateReplace = self.gameObject.AddComponent<ThrowLeavesStateReplace>();
            On.LeafGolemThrowLeavesState.OnThrow += stateReplace.LeafGolemThrowLeavesState_OnThrow;

            if(ThrowLeavesSpecialStateReplace.projectile_3 != null) {
                UnityEngine.Object.Destroy(ThrowLeavesSpecialStateReplace.projectile_3.gameObject);
                UnityEngine.Object.Destroy(ThrowLeavesSpecialStateReplace.projectile_4.gameObject);
                ThrowLeavesSpecialStateReplace.projectile_3 = null;
                ThrowLeavesSpecialStateReplace.projectile_4 = null;
            }
            ThrowLeavesSpecialStateReplace.projectile_3 = UnityEngine.Object.Instantiate(self.leafProjectile).GetComponent<LeafGolemProjectile>();
            ThrowLeavesSpecialStateReplace.projectile_4 = UnityEngine.Object.Instantiate(self.leafProjectile).GetComponent<LeafGolemProjectile>();
            ThrowLeavesSpecialStateReplace.projectile_3.gameObject.SetActive(false);
            ThrowLeavesSpecialStateReplace.projectile_4.gameObject.SetActive(false);
            DynData<LeafGolemBoss> selfData = new DynData<LeafGolemBoss>(self);

            selfData.Get<StateMachine>("stateMachine").GetState<LeafGolemThrowLeavesState>().delayBeforeLeafComeBack = 0.2f;
            selfData.Get<StateMachine>("stateMachine").GetState<LeafGolemThrowLeavesState>().projectileSpeed += 16;
            // I already made it harder, no need to be much faster
            selfData.Get<StateMachine>("stateMachine").GetState<LeafGolemThrowLeavesSpecialState>().projectileSpeed += 5;
            selfData.Get<StateMachine>("stateMachine").GetState<LeafGolemMoveTowardsPlayerState>().moveSpeed *= 3;
        }

        void LeafGolemBoss_Explode(On.LeafGolemBoss.orig_Explode orig, LeafGolemBoss self) {
            orig(self);
            if (ThrowLeavesSpecialStateReplace.projectile_3.gameObject.activeInHierarchy) {
                ThrowLeavesSpecialStateReplace.projectile_3.explosionSpawner.StartSpawning();
            }
            if (ThrowLeavesSpecialStateReplace.projectile_4.gameObject.activeInHierarchy) {
                ThrowLeavesSpecialStateReplace.projectile_4.explosionSpawner.StartSpawning();
            }
        }

        void LeafGolemBoss_StopExploding(On.LeafGolemBoss.orig_StopExploding orig, LeafGolemBoss self) {
            orig(self);
            if (ThrowLeavesSpecialStateReplace.projectile_3.gameObject.activeInHierarchy) {
                ThrowLeavesSpecialStateReplace.projectile_3.explosionSpawner.StopSpawning();
            }
            if (ThrowLeavesSpecialStateReplace.projectile_4.gameObject.activeInHierarchy) {
                ThrowLeavesSpecialStateReplace.projectile_4.explosionSpawner.StopSpawning();
            }
        }

        void LeafGolemBoss_Kill(On.LeafGolemBoss.orig_Kill orig, LeafGolemBoss self) {
            On.LeafGolemThrowLeavesState.OnThrow -= self.gameObject.GetComponent<ThrowLeavesStateReplace>().LeafGolemThrowLeavesState_OnThrow;

            UnityEngine.Object.Destroy(ThrowLeavesSpecialStateReplace.projectile_3.gameObject);
            UnityEngine.Object.Destroy(ThrowLeavesSpecialStateReplace.projectile_4.gameObject);
            orig(self);
        }

        void LeafGolemDeadState_StateEnter(On.LeafGolemDeadState.orig_StateEnter orig, LeafGolemDeadState self, StateMachine stateMachine) {
            orig(self, stateMachine);
            ThrowLeavesSpecialStateReplace.projectile_3.Pause();
            ThrowLeavesSpecialStateReplace.projectile_4.Pause();
        }
    }
}
