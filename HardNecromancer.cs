using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mod.Courier;
using Mod.Courier.Helpers;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class HardNecromancer {
        public static MethodInfo NecromancerBossShootInfo = typeof(NecromancerBoss).GetMethod("Shoot", Mod.Courier.Helpers.ReflectionHelper.NonPublicInstance | BindingFlags.InvokeMethod);
        public static MethodInfo CastArrowsStateGetRandomArrowPatternInfo = typeof(NecromancerCastArrowsState).GetMethod("GetRandomArrowPattern", Mod.Courier.Helpers.ReflectionHelper.NonPublicInstance | BindingFlags.InvokeMethod);

        bool specialArrows;

        public void Load() {
            On.NecromancerSkullMountState.StartBeam += NecromancerSkullMountState_StartBeam;

            On.NecromancerCastArrowsState.CastArrows += NecromancerCastArrowsState_CastArrows;

            On.NecromancerShootingState.ShootCoroutine += NecromancerShootingState_ShootCoroutine;
            On.NecromancerShootingState.StateExit += NecromancerShootingState_StateExit;

            On.PoisonArrow.StartMovement += PoisonArrow_StartMovement;
            On.PoisonArrow.Update += PoisonArrow_Update;
            On.PoisonArrow.Explode += PoisonArrow_Explode;
        }

        public void Unload() {
            On.NecromancerSkullMountState.StartBeam -= NecromancerSkullMountState_StartBeam;

            On.NecromancerCastArrowsState.CastArrows -= NecromancerCastArrowsState_CastArrows;

            On.NecromancerShootingState.ShootCoroutine -= NecromancerShootingState_ShootCoroutine;
            On.NecromancerShootingState.StateExit -= NecromancerShootingState_StateExit;

            On.PoisonArrow.StartMovement -= PoisonArrow_StartMovement;
            On.PoisonArrow.Update -= PoisonArrow_Update;
            On.PoisonArrow.Explode -= PoisonArrow_Explode;
        }

        void PoisonArrow_StartMovement(On.PoisonArrow.orig_StartMovement orig, PoisonArrow self, int smallShot) {
            orig(self, smallShot);
            if (specialArrows) {
                self.GetComponent<SpriteRenderer>().color = Color.cyan;
                self.damageTypeImmunity = (EDamageType)255;
                DynData<PoisonArrow> arrowData = new DynData<PoisonArrow>(self);
                arrowData.Set("bounceAtWall", true);
            }
        }


        void PoisonArrow_Update(On.PoisonArrow.orig_Update orig, PoisonArrow self) {
            DynData<PoisonArrow> selfData = new DynData<PoisonArrow>(self);
            if (selfData.Get<bool>("movementStarted")) {
                int layerMask = LayerMaskConstants.GROUND_8_AND_16 | LayerMaskConstants.HITTABLE | LayerMaskConstants.MOVING_COLLISION_8_AND_16;
                RaycastHit2D raycastHit2D = Manager<DimensionManager>.Instance.MultiDimensionRaycast(self.transform.position + new Vector3(selfData.Get<Vector2>("dir").x, selfData.Get<Vector2>("dir").y) * 1.25f, selfData.Get<Vector2>("dir"), selfData.Get<float>("speed") * TimeVars.GetDeltaTime(), layerMask, false, false, true);
                if (raycastHit2D.transform != null && (raycastHit2D.transform.gameObject.layer != LayerConstants.HITTABLE || raycastHit2D.transform.gameObject.tag == "SkullMount")) {
                    try {
                        if (selfData.Get<bool>("bounceAtWall")) {
                            self.OverrideDir(new Vector2(selfData.Get<NecromancerBoss>("necromancerRef").transform.localScale.x, 0f));
                            selfData.Set("bounceAtWall", false);
                        }
                    } catch(Exception e) {
                        CourierLogger.LogDetailed(e);
                    }
                }
            }
            orig(self);
        }

        void PoisonArrow_Explode(On.PoisonArrow.orig_Explode orig, PoisonArrow self) {
            orig(self);
            self.GetComponent<SpriteRenderer>().color = Color.white;
            self.damageTypeImmunity = 0;
            DynData<PoisonArrow> selfData = new DynData<PoisonArrow>(self);
            selfData.Set("bounceAtWall", false);
        }


        IEnumerator NecromancerShootingState_ShootCoroutine(On.NecromancerShootingState.orig_ShootCoroutine orig, NecromancerShootingState self) {
            DynData<NecromancerShootingState> selfData = new DynData<NecromancerShootingState>(self);
            NecromancerBoss boss = selfData.Get<NecromancerBoss>("boss");
            Vector3 targetPos = (!(Manager<PlayerManager>.Instance.Player.transform.position.x > boss.arenaCenter.position.x)) ? boss.castingPosRight.position : boss.castingPosLeft.position;
            boss.Invincibru = true;
            boss.hurtZone.enabled = false;
            boss.Animator.Update(0f);
            boss.Animator.SetTrigger("TeleportOut");
            yield return new WaitForSeconds(1f);

            Vector3 position3 = boss.arenaCenter.position;
            if (targetPos.x < position3.x) {
                boss.transform.localScale = new Vector3(-1f, 1f, 1f);
            } else {
                boss.transform.localScale = Vector3.one;
            }

            boss.transform.position = targetPos;
            boss.Animator.SetTrigger("TeleportIn");
            yield return new WaitForSeconds(0.25f);
            boss.hurtZone.enabled = true;
            boss.Invincibru = false;
            specialArrows = true;
            while (selfData.Get<int>("maxWaveCount") > 0) {
                boss.Animator.SetBool("Shooting", true);
                yield return new WaitForSeconds(0.3f);
                float[] pattern = GetRandomShotPattern();
                foreach(float wait in pattern) {
                    boss.Animator.SetTrigger("DoShot");
                    yield return new WaitForSeconds(wait);
                }
                boss.Animator.SetBool("Shooting", false);
                selfData.Set("maxWaveCount", selfData.Get<int>("maxWaveCount") - 1);
                yield return new WaitForSeconds(1f);
            }
            selfData.Set("allDone", true);
        }

        void NecromancerShootingState_StateExit(On.NecromancerShootingState.orig_StateExit orig, NecromancerShootingState self) {
            specialArrows = false;
            orig(self);
        }


        public float[] GetRandomShotPattern() {
            int pattern = new System.Random().Next(3);
            switch(pattern) {
                case 0:
                    return new float[] { .15f, .15f, .15f, .15f, 1f, .15f, .15f, .15f, .15f, 1f, .15f, .15f, .15f, .5f };
                case 1:
                    return new float[] { .7f, .7f, .7f, .7f, .7f };
                case 2:
                    return new float[] { .075f, .075f, .075f, .075f, .075f, .5f};
            }
            return null;
        } 

        IEnumerator NecromancerCastArrowsState_CastArrows(On.NecromancerCastArrowsState.orig_CastArrows orig, NecromancerCastArrowsState self) {
            DynData<NecromancerCastArrowsState> selfData = new DynData<NecromancerCastArrowsState>(self);
            NecromancerBoss boss = selfData.Get<NecromancerBoss>("boss");
            try {
                SkeloutonSpawner spawner = Resources.FindObjectsOfTypeAll<SkeloutonSpawner>()[0];
                for (int i = 0; i < 5; i++) {
                    Vector3 spawnPos = Vector3.Lerp(boss.castingPosLeft.position, boss.castingPosRight.position, 0.25f * i);
                    if (Vector3.Distance(spawnPos, Manager<PlayerManager>.Instance.Player.transform.position) > 5)
                        spawner.Spawn(spawnPos);
                }
            } catch (Exception e) {
                Console.WriteLine("Exception while spawning skeloutons");
                CourierLogger.LogDetailed(e, "HarderBosses");

            }
            for (int wavesDone = 0; wavesDone < self.waveCount; wavesDone++) {
                List<Transform> arrowPattern = (List<Transform>)CastArrowsStateGetRandomArrowPatternInfo.Invoke(self, new object[] { wavesDone });
                int numProjectiles = arrowPattern.Count;

                while (arrowPattern.Count > 0) {
                    try {
                        GameObject projectile = Manager<PoolManager>.Instance.GetObjectInstance(boss.arrowProjectilePrefab);
                        projectile.transform.position = arrowPattern[0].position;
                        projectile.GetComponent<PoisonArrow>().SetNecromancerRef(boss);
                        projectile.GetComponent<PoisonArrow>().OverrideDir(Vector2.down);
                        PoisonArrow arrow = projectile.GetComponent<PoisonArrow>();
                        DynData<PoisonArrow> arrowData = new DynData<PoisonArrow>(arrow);
                        arrow.GetComponent<SpriteRenderer>().color = Color.cyan;
                        arrow.damageTypeImmunity = (EDamageType)255;
                        arrowData.Set("bounceAtWall", false);
                        arrowPattern.RemoveAt(0);
                    } catch (Exception e) {
                        CourierLogger.LogDetailed(e, "HarderBosses");
                    }
                    yield return new WaitForSeconds(self.projectileCD);
                }

                yield return new WaitForSeconds(self.waveCD / 2f);
            }
            boss.GotoNextAttackState();

        }


        IEnumerator NecromancerSkullMountState_StartBeam(On.NecromancerSkullMountState.orig_StartBeam orig, NecromancerSkullMountState self) {
            yield return orig(self);
            DynData<NecromancerSkullMountState> selfData = new DynData<NecromancerSkullMountState>(self);
            NecromancerBoss boss = selfData.Get<NecromancerBoss>("boss");
            while (true) {
                PoisonArrow arrow = Manager<PoolManager>.Instance.GetObjectInstance(boss.arrowProjectilePrefab).GetComponent<PoisonArrow>();
                arrow.transform.position = boss.projectileSpawnPoint.position + new Vector3(1.6f, 1.3f);
                Vector3 dir = Manager<PlayerManager>.Instance.Player.transform.position - arrow.transform.position;
                dir.z = 0;
                dir = dir.normalized;
                arrow.OverrideDir(dir);
                arrow.transform.rotation = Quaternion.Euler(0, 0, (dir.x > 0 ? 0 : 180) + Vector3.SignedAngle(-arrow.transform.right, dir, arrow.transform.forward));
                arrow.SetNecromancerRef(boss);
                arrow.StartMovement(0);
                yield return new WaitForSeconds(0.7f);
            }
        }

    }
}
