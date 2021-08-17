using System;
using System.Collections;
using System.Collections.Generic;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class HardEmeraldGolem {
        DynData<EmeraldGolemBoss> emeraldBossData;

        Vector3 regularBeginPos;

        public bool Enabled => HardBossesModule.Save.hardEmeraldGolem;

        public void Toggle() {
            HardBossesModule.Save.hardEmeraldGolem = !HardBossesModule.Save.hardEmeraldGolem;
            HardBossesModule.Instance.emeraldGolemButton.UpdateStateText();
        }

        public void Load() {
            On.EmeraldGolemBoss.Start += EmeraldGolemBoss_Start;
            On.EmeraldGolemBoss.HeadVulnerabilityWindowCoroutine += EmeraldGolemBoss_HeadVulnerabilityWindowCoroutine;

            On.EmeraldGolemFightManager.OnEmeraldBossDestroyed += EmeraldGolemFightManager_OnEmeraldBossDestroyed;

            On.EmeraldGolemLeftTripleStompState.TripleStompCoroutine += EmeraldGolemLeftTripleStompState_TripleStompCoroutine;
            On.EmeraldGolemRightTripleStompState.TripleStompCoroutine += EmeraldGolemRightTripleStompState_TripleStompCoroutine;

            On.EmeraldGolemDoubleTripleStompState.TripleStompCoroutine += EmeraldGolemDoubleTripleStompState_TripleStompCoroutine;

            On.EmeraldGolemBossArm.StompCoroutine += EmeraldGolemBossArm_StompCoroutine;

            On.EmeraldGolemEssence.MovementCoroutine += EmeraldGolemEssence_MovementCoroutine;
        }

        public void Unload() {
            On.EmeraldGolemBoss.Start -= EmeraldGolemBoss_Start;
            On.EmeraldGolemBoss.HeadVulnerabilityWindowCoroutine -= EmeraldGolemBoss_HeadVulnerabilityWindowCoroutine;

            On.EmeraldGolemFightManager.OnEmeraldBossDestroyed -= EmeraldGolemFightManager_OnEmeraldBossDestroyed;

            On.EmeraldGolemLeftTripleStompState.TripleStompCoroutine -= EmeraldGolemLeftTripleStompState_TripleStompCoroutine;
            On.EmeraldGolemRightTripleStompState.TripleStompCoroutine -= EmeraldGolemRightTripleStompState_TripleStompCoroutine;

            On.EmeraldGolemDoubleTripleStompState.TripleStompCoroutine -= EmeraldGolemDoubleTripleStompState_TripleStompCoroutine;

            On.EmeraldGolemBossArm.StompCoroutine -= EmeraldGolemBossArm_StompCoroutine;

            On.EmeraldGolemEssence.MovementCoroutine -= EmeraldGolemEssence_MovementCoroutine;
        }

        void EmeraldGolemBoss_Start(On.EmeraldGolemBoss.orig_Start orig, EmeraldGolemBoss self) {
            orig(self);
            for (int i = 0; i < Manager<EmeraldGolemFightManager>.Instance.fightGeysers.Count; i++) {
                Manager<EmeraldGolemFightManager>.Instance.fightGeysers[i].transform.position = (i % 2 == 0 ? self.SpikesL.position : self.SpikesR.position);
                Manager<EmeraldGolemFightManager>.Instance.fightGeysers[i].transform.position += new Vector3(i % 2 == 0 ? 2 : -2, -1.5f);
                //AirGeyser geyser = Manager<EmeraldGolemFightManager>.Instance.fightGeysers[i].GetComponent<AirGeyser>();
                //geyser.boostHeight = 8;
                //geyser.windCollider.size = new Vector2(geyser.windCollider.size.x, 8);
                //geyser.windCollider.offset = new Vector2(geyser.windCollider.size.x, geyser.windCollider.size.y * 0.5f - 0.6f);
            }
        }


        IEnumerator EmeraldGolemBoss_HeadVulnerabilityWindowCoroutine(On.EmeraldGolemBoss.orig_HeadVulnerabilityWindowCoroutine orig, EmeraldGolemBoss self) {
            Console.WriteLine("better logging");
            DynData<EmeraldGolemBoss> selfData = emeraldBossData = new DynData<EmeraldGolemBoss>(self);
            if (selfData.Get<Coroutine>("shootCoroutine") != null) {
                self.StopCoroutine(selfData.Get<Coroutine>("shootCoroutine"));
                selfData.Set<Coroutine>("shootCoroutine", null);
            }
            Manager<AudioManager>.Instance.PlaySoundEffect(self.deactivateSFX);
            Animator gemAnimator = self.gem.GetComponent<Animator>();
            // This is an invincibru moment
            self.gem.invincibru = true;
            // Don't deactivate spikes
            gemAnimator.SetTrigger("Off");
            self.head.GetComponent<Animator>().ResetTrigger("Shoot");
            self.head.GetComponent<Animator>().SetTrigger("IdleOn");
            selfData.Set("headInvincible", false);
            yield return new WaitForSeconds(1f);
            // Shoot energy balls at the player
            for (int i = 0; i < 35; i++) {
                // Don't shoot if the player is above the golem
                if (Manager<PlayerManager>.Instance.Player.transform.position.y < self.head.position.y) {
                    Manager<AudioManager>.Instance.PlaySoundEffect(self.headShootSFX);
                    WallShmuProjectile projectile = Manager<PoolManager>.Instance.GetObjectInstance(self.headProjectilePrefab).GetComponent<WallShmuProjectile>();
                    projectile.GetComponent<DisableNotifier>().onDisabled += OnProjectileDisabled;
                    Console.WriteLine(projectile.transform.position);
                    selfData.Get<List<GameObject>>("projectileList").Add(projectile.gameObject);
                    projectile.transform.position = self.head.position;
                    Vector3 dir = Manager<PlayerManager>.Instance.Player.transform.position + new Vector3(0f, 2.7f, 0f) - self.head.position;
                    dir.Normalize();
                    projectile.Initialize(23f, dir);
                }
                yield return new WaitForSeconds(0.333333333333f);
            }

            int blinkDone2 = 0;
            for (int totalBlinks2 = 2; blinkDone2 < totalBlinks2; blinkDone2++) {
                gemAnimator.SetTrigger("On");
                yield return new WaitForSeconds(0.3f);
                gemAnimator.SetTrigger("Off");
                yield return new WaitForSeconds(0.3f);
            }
            blinkDone2 = 0;
            for (int totalBlinks2 = 8; blinkDone2 < totalBlinks2; blinkDone2++) {
                gemAnimator.SetTrigger("On");
                yield return new WaitForSeconds(0.05f);
                gemAnimator.SetTrigger("Off");
                yield return new WaitForSeconds(0.05f);
            }
            gemAnimator.SetTrigger("On");
            self.head.GetComponent<Animator>().SetTrigger("IdleOff");
            selfData.Set("headInvincible", true);
            selfData.Set("gemHP", self.gemMaxHP);
            self.gem.invincibru = false;
            Manager<AudioManager>.Instance.PlaySoundEffect(self.activateSFX);
            self.stateMachine.SetState<EmeraldGolemStunnedDoneState>();
            yield return null;
        }

        void OnProjectileDisabled(DisableNotifier disabledObject) {
            disabledObject.onDisabled -= OnProjectileDisabled;
            emeraldBossData.Get<List<GameObject>>("projectileList").Remove(disabledObject.gameObject);
        }

        void EmeraldGolemFightManager_OnEmeraldBossDestroyed(On.EmeraldGolemFightManager.orig_OnEmeraldBossDestroyed orig, EmeraldGolemFightManager self) {
            Rect roomRect = Manager<Level>.Instance.CurrentRoom.roomRect;
            self.essenceSpawnPos.position = new Vector3(UnityEngine.Random.Range(roomRect.xMin + 2f, roomRect.xMax - 2f), self.essenceSpawnPos.position.y);
            if (regularBeginPos == default(Vector3))
                regularBeginPos = self.essenceBeginPos.position;
            self.essenceBeginPos.position = new Vector3(self.essenceSpawnPos.position.x, self.essenceBeginPos.position.y);
            orig(self);
            DynData<EmeraldGolemFightManager> selfData = new DynData<EmeraldGolemFightManager>(self);
            self.StopCoroutine(selfData.Get<Coroutine>("spawnManaCoroutine"));
        }

        IEnumerator EmeraldGolemLeftTripleStompState_TripleStompCoroutine(On.EmeraldGolemLeftTripleStompState.orig_TripleStompCoroutine orig, EmeraldGolemLeftTripleStompState self) {
            DynData<EmeraldGolemLeftTripleStompState> selfData = new DynData<EmeraldGolemLeftTripleStompState>(self);
            EmeraldGolemBoss boss = selfData.Get<EmeraldGolemBoss>("boss");
            selfData.Set("rightArmCoroutine", boss.rightArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            selfData.Set("leftArmCoroutine", boss.StartCoroutine(boss.leftArm.AnimatorQuadrupleStompCoroutine(shakeOnFirstStomp: true, shakeSecondStomp: true, shakeLastStomp: true, 570)));
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            selfData.Set("leftArmCoroutine", boss.leftArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            selfData.Get<StateMachine>("stateMachine").SetState<EmeraldGolemStompLeftRightState>(new EmeraldGolemStompLeftRightStateParams(1));
        }

        IEnumerator EmeraldGolemRightTripleStompState_TripleStompCoroutine(On.EmeraldGolemRightTripleStompState.orig_TripleStompCoroutine orig, EmeraldGolemRightTripleStompState self) {
            DynData<EmeraldGolemRightTripleStompState> selfData = new DynData<EmeraldGolemRightTripleStompState>(self);
            EmeraldGolemBoss boss = selfData.Get<EmeraldGolemBoss>("boss");
            selfData.Set("leftArmCoroutine", boss.leftArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            selfData.Set("rightArmCoroutine", boss.StartCoroutine(boss.rightArm.AnimatorQuadrupleStompCoroutine(true, true, true, 573.5f)));
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            selfData.Set("rightArmCoroutine", boss.rightArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            selfData.Get<StateMachine>("stateMachine").SetState<EmeraldGolemStompLeftRightState>(new EmeraldGolemStompLeftRightStateParams(1));
        }

        IEnumerator EmeraldGolemDoubleTripleStompState_TripleStompCoroutine(On.EmeraldGolemDoubleTripleStompState.orig_TripleStompCoroutine orig, EmeraldGolemDoubleTripleStompState self) {
            DynData<EmeraldGolemDoubleTripleStompState> selfData = new DynData<EmeraldGolemDoubleTripleStompState>(self);
            EmeraldGolemBoss boss = selfData.Get<EmeraldGolemBoss>("boss");
            selfData.Set("rightArmCoroutine", boss.rightArm.GoToDefaultPosition());
            selfData.Set("leftArmCoroutine", boss.leftArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            selfData.Set("rightArmCoroutine", boss.rightArm.AnimatorTripleStomp(true, true, true));
            selfData.Set("leftArmCoroutine", boss.leftArm.AnimatorTripleStomp(true, true, true));
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            selfData.Set("rightArmCoroutine", boss.rightArm.GoToDefaultPosition());
            selfData.Set("leftArmCoroutine", boss.leftArm.GoToDefaultPosition());
            yield return selfData.Get<Coroutine>("rightArmCoroutine");
            yield return selfData.Get<Coroutine>("leftArmCoroutine");
            if (boss.GetCurrentHpThreshold() == EmeraldGolemBoss.EEmeralGolemBossMood.MAD || boss.GetCurrentHpThreshold() == EmeraldGolemBoss.EEmeralGolemBossMood.VERY_MAD) {
                if (boss.centerPlayerDetectionBox.PlayerIsInBox()) {
                    selfData.Get<StateMachine>("stateMachine").SetState<EmeraldGolemLeftRightCenterStompState>(new EmeraldGolemLeftRightCenterStompStateParams(2));
                    yield return null;
                } else {
                    selfData.Get<StateMachine>("stateMachine").SetState<EmeraldGolemStompLeftRightState>(new EmeraldGolemStompLeftRightStateParams(1));
                }
            } else {
                selfData.Get<StateMachine>("stateMachine").SetState<EmeraldGolemStompLeftRightState>(new EmeraldGolemStompLeftRightStateParams(1));
            }
        }


        IEnumerator EmeraldGolemBossArm_StompCoroutine(On.EmeraldGolemBossArm.orig_StompCoroutine orig, EmeraldGolemBossArm self) {
            float oldDuration = self.followPlayerStompShakeDuration;
            self.followPlayerStompShakeDuration = .88f;
            yield return orig(self);
            self.followPlayerStompShakeDuration = oldDuration;
        }

        IEnumerator EmeraldGolemEssence_MovementCoroutine(On.EmeraldGolemEssence.orig_MovementCoroutine orig, EmeraldGolemEssence self) {
            DynData<EmeraldGolemEssence> selfData = new DynData<EmeraldGolemEssence>(self);
            Vector3 startPos = selfData.Get<Vector3>("startPos");
            Vector3 targetPos = startPos;
            Vector3 vel = Vector3.zero;
            float maxVel = 10f;
            float accel = 0.5f;
            List<Vector3> movePoints = new List<Vector3>
            {
                new Vector3(regularBeginPos.x - 10f, regularBeginPos.y),
                new Vector3(regularBeginPos.x - 10f, regularBeginPos.y + 6f),
                new Vector3(regularBeginPos.x + 10f, regularBeginPos.y),
                new Vector3(regularBeginPos.x + 10f, regularBeginPos.y + 6f)
            };
            int idx = -1;
            Vector3 ab3 = targetPos - self.transform.position;
            float xDir = Mathf.Sign(ab3.x);
            float yDir = Mathf.Sign(ab3.y);
            while (true) {
                ab3 = targetPos - self.transform.position;
                vel += ab3.normalized * accel;
                if (vel.magnitude > maxVel) {
                    vel.Normalize();
                    vel *= maxVel;
                }
                self.transform.position += vel * TimeVars.GetDeltaTime();
                if (Mathf.Sign(ab3.x) != Mathf.Sign(xDir) || Mathf.Sign(ab3.y) != Mathf.Sign(yDir)) {
                    if (idx != -1) {
                        List<Vector3> list = new List<Vector3>(movePoints);
                        list.RemoveAt(idx);
                        idx = UnityEngine.Random.Range(0, list.Count);
                    } else {
                        idx = UnityEngine.Random.Range(0, movePoints.Count);
                    }
                    targetPos = movePoints[idx];
                    ab3 = targetPos - self.transform.position;
                    xDir = Mathf.Sign(ab3.x);
                    yDir = Mathf.Sign(ab3.y);
                }
                yield return null;
            }
        }

    }
}
