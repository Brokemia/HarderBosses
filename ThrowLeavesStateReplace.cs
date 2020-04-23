using System;
using System.Collections;
using System.Reflection;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class ThrowLeavesStateReplace : MonoBehaviour {
        Vector3 projectileTargetPos;

        public LeafGolemBoss boss;
        public LeafGolemThrowLeavesState state;
        public DynData<LeafGolemThrowLeavesState> stateData;

        public static MethodInfo OnProjectileCameBackInfo = typeof(LeafGolemThrowLeavesState).GetMethod("OnProjectileCameBack", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic);

        public void LeafGolemThrowLeavesState_OnThrow(On.LeafGolemThrowLeavesState.orig_OnThrow orig, LeafGolemThrowLeavesState self) {
            state = self;
            stateData = new DynData<LeafGolemThrowLeavesState>(self);
            boss = stateData.Get<LeafGolemBoss>("boss");

            stateData.Set("attackSFX", Manager<AudioManager>.Instance.PlaySoundEffect(boss.groundAttackSFX));
            boss.Projectile_1.gameObject.SetActive(true);
            boss.Projectile_1.transform.position = boss.leftRightProjectileSpawnPos.position;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(boss.Projectile_1.transform.position, Vector3.right * boss.LookDir, float.PositiveInfinity, LayerMaskConstants.GROUND_8_AND_16);
            projectileTargetPos = raycastHit2D.point;
            ref Vector3 reference = ref projectileTargetPos;
            Vector2 point = raycastHit2D.point;
            float x = point.x;
            Vector3 right = Vector3.right;
            reference.x = x + right.x * (-boss.LookDir) * 1.8f;
            boss.Projectile_1.onReachedPosition += OnProjectileReachedPosition;
            float duration = Vector3.Distance(boss.Projectile_1.transform.position, projectileTargetPos) / self.projectileSpeed;
            boss.Projectile_1.GoToQuadSineWave(projectileTargetPos, duration);
        }

        public void OnProjectileReachedPosition(LeafGolemProjectile projectile) {
            projectile.onReachedPosition -= OnProjectileReachedPosition;
            stateData["bringBackProjectileCoroutine"] = boss.StartCoroutine(WaitAndBringBackProjectile());
        }

        public IEnumerator WaitAndBringBackProjectile() {
            yield return new WaitForSeconds(state.delayBeforeLeafComeBack);
            boss.Projectile_1.onReachedPosition += OnProjectileCameBack;
            float moveDuration = Vector3.Distance(boss.Projectile_1.transform.position, boss.leftRightProjectileSpawnPos.position) / state.projectileSpeed;
            boss.Projectile_1.GoToLinear(boss.leftRightProjectileSpawnPos.position, moveDuration);
            stateData["bringBackProjectileCoroutine"] = null;
        }

        public void OnProjectileCameBack(LeafGolemProjectile projectile) {
            boss.Projectile_1.onReachedPosition -= OnProjectileCameBack;
            OnProjectileCameBackInfo.Invoke(state, new object[] { projectile });
        }
    }
}
