using System;
using System.Reflection;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public static class ThrowLeavesSpecialStateReplace {
        public static LeafGolemProjectile projectile_3, projectile_4;
        public static Vector3 extraProjectileMergePos;

        public static LeafGolemBoss boss;
        public static LeafGolemThrowLeavesSpecialState specialState;
        public static DynData<LeafGolemThrowLeavesSpecialState> specialStateData;

        public static MethodInfo OnProjectileCameBackInfo = typeof(LeafGolemThrowLeavesSpecialState).GetMethod("OnProjectileCameBack", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic);

        public static void LeafGolemThrowLeavesSpecialState_OnThrow(On.LeafGolemThrowLeavesSpecialState.orig_OnThrow orig, LeafGolemThrowLeavesSpecialState self) {
            specialState = self;
            specialStateData = new DynData<LeafGolemThrowLeavesSpecialState>(self);
            boss = specialStateData.Get<LeafGolemBoss>("boss");

            boss.ClearOnThrowHandlers();
            specialStateData.Set("attackSFX", Manager<AudioManager>.Instance.PlaySoundEffect(boss.groundAttackSFX));

            boss.Projectile_1.gameObject.SetActive(true);
            boss.Projectile_2.gameObject.SetActive(true);
            projectile_3.gameObject.SetActive(true);
            projectile_4.gameObject.SetActive(true);
            boss.Projectile_1.transform.position = boss.downProjectileSpawnPos.position;
            boss.Projectile_2.transform.position = boss.downProjectileSpawnPos.position + Vector3.down * 0.75f;
            projectile_3.transform.position = boss.downProjectileSpawnPos.position;
            projectile_4.transform.position = boss.downProjectileSpawnPos.position + Vector3.down * 0.75f;

            Vector3 group1Target = boss.Projectile_1.transform.position + Vector3.left * 10f;
            float duration = Vector3.Distance(boss.Projectile_1.transform.position, group1Target) / (int)(self.projectileSpeed * 1.5f);
            boss.Projectile_1.GoToQuad(group1Target, duration);
            boss.Projectile_2.GoToQuad(group1Target + Vector3.down * 0.75f, duration);

            Vector3 group2Target = projectile_3.transform.position + Vector3.right * 10f;
            float duration2 = Vector3.Distance(projectile_3.transform.position, group2Target) / (int)(self.projectileSpeed * 1.5f);
            projectile_3.GoToQuad(group2Target, duration2);
            projectile_4.GoToQuad(group2Target + Vector3.down * 0.75f, duration2);

            boss.Projectile_1.onReachedPosition += OnProjectileReachedUpApart;
            boss.Projectile_2.onReachedPosition += OnProjectileReachedUpApart;
            projectile_3.onReachedPosition += OnProjectileReachedUpApart;
            projectile_4.onReachedPosition += OnProjectileReachedUpApart;
        }

        public static void OnProjectileReachedUpApart(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileReachedUpApart;
            RaycastHit2D[] array = Physics2D.RaycastAll(projectile.transform.position, Vector3.down, 30f, LayerMaskConstants.GROUND_8_AND_16);
            Vector3 projectileTargetPos = Vector3.zero;
            for (int i = 0; i < array.Length; i++) {
                CollisionDirection component = array[i].collider.GetComponent<CollisionDirection>();
                if (component == null || !component.CollideOnlyOnTop()) {
                    projectileTargetPos = array[i].point;
                    break;
                }
            }
            projectileTargetPos.y += 1f;
            if (projectile == boss.Projectile_1 || projectile == boss.Projectile_2)
                specialStateData.Set("projectileMergePosition", projectileTargetPos);
            else
                extraProjectileMergePos = projectileTargetPos;

            projectile.onReachedPosition += OnProjectileReachedGround;

            if (projectile == boss.Projectile_1 || projectile == projectile_3) {
                float duration = Vector3.Distance(projectile.transform.position, projectileTargetPos + Vector3.up * 0.75f) / specialState.projectileSpeed;
                projectile.GoToLinear(projectileTargetPos + Vector3.up * 0.75f, duration);
            } else {
                float duration = Vector3.Distance(projectile.transform.position, projectileTargetPos) / specialState.projectileSpeed;
                projectile.GoToLinear(projectileTargetPos, duration);
            }
        }

        public static void OnProjectileReachedGround(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileReachedGround;
            projectile.onReachedPosition += OnProjectileGoOut;

            // One goes 5, one goes 20, both go at 17 speed
            float duration = 17f / specialState.projectileSpeed;
            if (projectile == boss.Projectile_1) {
                Vector3 vector = (!specialStateData.Get<bool>("higherProjectileGoesRight")) ? (boss.Projectile_1.transform.position + Vector3.left * 5f) : (boss.Projectile_1.transform.position + Vector3.right * 20f);
                boss.Projectile_1.GoToQuad(vector, duration);
            } else if (projectile == boss.Projectile_2) {
                Vector3 vector2 = (!specialStateData.Get<bool>("higherProjectileGoesRight")) ? (boss.Projectile_2.transform.position + Vector3.right * 20f) : (boss.Projectile_2.transform.position + Vector3.left * 5f);
                boss.Projectile_2.GoToQuad(vector2, duration);
            } else if (projectile == projectile_3) {
                Vector3 vector = (!specialStateData.Get<bool>("higherProjectileGoesRight")) ? (projectile_3.transform.position + Vector3.left * 20f) : (projectile_3.transform.position + Vector3.right * 5f);
                projectile_3.GoToQuad(vector, duration);
            } else if (projectile == projectile_4) {
                Vector3 vector2 = (!specialStateData.Get<bool>("higherProjectileGoesRight")) ? (projectile_4.transform.position + Vector3.right * 5f) : (projectile_4.transform.position + Vector3.left * 20f);
                projectile_4.GoToQuad(vector2, duration);
            }
        }

        public static void OnProjectileGoOut(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileGoOut;
            projectile.onReachedPosition += OnProjectileAtMergePosition;

            // One goes 5, one goes 20, both go at 17 speed
            float duration = 17f / specialState.projectileSpeed;
            if (projectile == boss.Projectile_1) {
                Vector3 vector = specialStateData.Get<Vector3>("projectileMergePosition") + Vector3.up * 0.75f;
                projectile.GoToLinear(vector, duration);
            } else if (projectile == boss.Projectile_2) {
                projectile.GoToLinear(specialStateData.Get<Vector3>("projectileMergePosition"), duration);
            } else if (projectile == projectile_3) {
                Vector3 vector = extraProjectileMergePos + Vector3.up * 0.75f;
                projectile.GoToLinear(vector, duration);
            } else if (projectile == projectile_4) {
                projectile.GoToLinear(extraProjectileMergePos, duration);
            }
        }

        public static void OnProjectileAtMergePosition(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileAtMergePosition;
            projectile.onReachedPosition += OnProjectileCameUp;

            Vector3 returnPos = new Vector3(projectile.transform.position.x, boss.downProjectileSpawnPos.position.y, projectile.transform.position.z);
            if (projectile == boss.Projectile_1 || projectile == projectile_3) {
                float duration = Vector3.Distance(projectile.transform.position, returnPos) / specialState.projectileSpeed;
                projectile.GoToLinear(returnPos, duration);
            } else {
                float duration2 = Vector3.Distance(projectile.transform.position, returnPos + Vector3.down * 0.75f) / specialState.projectileSpeed;
                projectile.GoToLinear(returnPos + Vector3.down * 0.75f, duration2);
            }
        }

        public static void OnProjectileCameUp(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileCameUp;
            projectile.onReachedPosition += OnProjectileCameBack;

            float duration = 15f / specialState.projectileSpeed;
            if (projectile == boss.Projectile_1 || projectile == projectile_3) {
                projectile.GoToLinear(boss.downProjectileSpawnPos.position, duration);
            } else {
                projectile.GoToLinear(boss.downProjectileSpawnPos.position + Vector3.down * 0.75f, duration);
            }
        }

        public static void OnProjectileCameBack(LeafGolemProjectile projectile) {
            boss.Projectile_1.onReachedPosition -= OnProjectileCameBack;
            boss.Projectile_2.onReachedPosition -= OnProjectileCameBack;
            projectile_3.onReachedPosition -= OnProjectileCameBack;
            projectile_4.onReachedPosition -= OnProjectileCameBack;
            projectile_3.gameObject.SetActive(false);
            projectile_4.gameObject.SetActive(false);
            OnProjectileCameBackInfo.Invoke(specialState, new object[] { projectile });
        }

        public static void LeafGolemThrowLeavesSpecialState_StateExit(On.LeafGolemThrowLeavesSpecialState.orig_StateExit orig, LeafGolemThrowLeavesSpecialState self) {
            orig(self);
            if (projectile_3 != null && !boss.IsDead()) {
                projectile_3.gameObject.SetActive(false);
            }
            if (projectile_4 != null && !boss.IsDead()) {
                projectile_4.gameObject.SetActive(false);
            }
        }
    }
}
