using System;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class HardQoQ {

        public bool Enabled => HardBossesModule.Save.hardQoQ;

        public void Toggle() {
            HardBossesModule.Save.hardQoQ = !HardBossesModule.Save.hardQoQ;
            HardBossesModule.Instance.qoqButton.UpdateStateText();
        }

        public void Load() {
            On.QueenBoss.BeginFight += QueenBoss_BeginFight;
            On.QueenBoss.ShouldUseHighRings += QueenBoss_ShouldUseHighRings;

            On.QueenPerchedState.OnCartWheelShot += QueenPerchedState_OnCartWheelShot;

            On.QueenPantingState.StateEnter += QueenPantingState_StateEnter;

            On.SporeBullet.OnTriggerEnter2D += SporeBullet_OnTriggerEnter2D;
        }

        public void Unload() {
            On.QueenBoss.BeginFight -= QueenBoss_BeginFight;
            On.QueenBoss.ShouldUseHighRings -= QueenBoss_ShouldUseHighRings;

            On.QueenPerchedState.OnCartWheelShot -= QueenPerchedState_OnCartWheelShot;

            On.QueenPantingState.StateEnter -= QueenPantingState_StateEnter;

            On.SporeBullet.OnTriggerEnter2D -= SporeBullet_OnTriggerEnter2D;
        }

        void QueenBoss_BeginFight(On.QueenBoss.orig_BeginFight orig, QueenBoss self) {
            orig(self);
            self.flurryDuration = 1.75f;
            self.flurryAnticipationDuration = 0.5f;
            // If spore patterns haven't been modified yet
            if(self.sporePatterns[0].spores.Count <= 6) {
                self.sporePatterns[0].spores.Add(new SporePatternInfo { offset = -0.01f });
                self.sporePatterns[1].spores.Add(new SporePatternInfo { offset = 0 });
                self.sporePatterns[3].spores.Insert(2, new SporePatternInfo { offset = 0 });
            }
        }

        bool QueenBoss_ShouldUseHighRings(On.QueenBoss.orig_ShouldUseHighRings orig, QueenBoss self) {
            return true;
        }

        void QueenPerchedState_OnCartWheelShot(On.QueenPerchedState.orig_OnCartWheelShot orig, QueenPerchedState self) {
            DynData<QueenPerchedState> selfData = new DynData<QueenPerchedState>(self);
            QueenBoss boss = selfData.Get<QueenBoss>("boss");

            if (selfData.Get<int>("nextAction") == 1) {
                Manager<AudioManager>.Instance.PlaySoundEffect(boss.throwSeedSFX);
                PlayerController player = Manager<PlayerManager>.Instance.Player;

                // Right to left
                if (selfData.Get<float>("jumpDir") > 0f) {
                    boss.ThrowSeed(boss.rightWallaxerSpawnerLeft, 2, true, selfData.Get<float>("jumpDir"));
                    boss.ThrowSeed(boss.rightWallaxerSpawnerRight, 2, false, selfData.Get<float>("jumpDir"));
                    if(boss.CurrentHP < boss.maxHP * 0.4f) {
                        boss.ThrowSeed(boss.leftWallaxerSpawnerRight, 2, false, -selfData.Get<float>("jumpDir"));
                        boss.ThrowSeed(boss.leftWallaxerSpawnerLeft, 2, true, -selfData.Get<float>("jumpDir"));
                    } else if (boss.CurrentHP < boss.maxHP * 0.8f) {
                        boss.ThrowSeed(boss.leftWallaxerSpawnerRight, 1, false, selfData.Get<float>("jumpDir"));
                    }
                } else { // Left to right
                    boss.ThrowSeed(boss.leftWallaxerSpawnerLeft, 2, true, selfData.Get<float>("jumpDir"));
                    boss.ThrowSeed(boss.leftWallaxerSpawnerRight, 2, false, selfData.Get<float>("jumpDir"));
                    if (boss.CurrentHP < boss.maxHP * 0.4f) {
                        boss.ThrowSeed(boss.rightWallaxerSpawnerLeft, 2, true, -selfData.Get<float>("jumpDir"));
                        boss.ThrowSeed(boss.rightWallaxerSpawnerRight, 2, false, -selfData.Get<float>("jumpDir"));
                    } else if (boss.CurrentHP < boss.maxHP * 0.8f) {
                        boss.ThrowSeed(boss.rightWallaxerSpawnerLeft, 1, true, selfData.Get<float>("jumpDir"));
                    }
                }

                if (boss.CurrentHP < boss.maxHP * 0.6f) {
                    selfData.Set("nextAction", 3);
                    return;
                }
                selfData.Set("SporeShootCount", 0);
                selfData.Set("nextAction", 2);
            } else {
                orig(self);
            }
        }

        void QueenPantingState_StateEnter(On.QueenPantingState.orig_StateEnter orig, QueenPantingState self, StateMachine stateMachine) {
            DynData<QueenPantingState> selfData = new DynData<QueenPantingState>(self);
            selfData.Set("maxPantTime", 3);
            orig(self, stateMachine);
        }

        void SporeBullet_OnTriggerEnter2D(On.SporeBullet.orig_OnTriggerEnter2D orig, SporeBullet self, UnityEngine.Collider2D collision) {
            DynData<SporeBullet> selfData = new DynData<SporeBullet>(self);
            QueenBoss boss = selfData.Get<QueenBoss>("boss");

            if (collision.transform == Manager<PlayerManager>.Instance.Player.transform) {
                PlayerController player = Manager<PlayerManager>.Instance.Player;
                HitData hitData = HitData.GetHitData(1);
                player.ReceiveHit(hitData);
                foreach (WindmillShuriken shuriken in player.ActiveWindmills) {
                    shuriken.Kill();
                }
                boss.shroomEscapeButtonPresses = boss.maxButtonPressToEscape;

                if (!player.IsDead()) {
                    orig(self, collision);
                }
            } else {
                orig(self, collision);
            }
        }

    }
}
