using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace HarderBosses {
    public static class EmeraldGolemBossArmExtension {
        public static MethodInfo GoToAnimationStartPositionInfo = typeof(EmeraldGolemBossArm).GetMethod("GoToAnimationStartPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        public static MethodInfo WaitForStompAndShakeInfo = typeof(EmeraldGolemBossArm).GetMethod("WaitForStompAndShake", BindingFlags.NonPublic | BindingFlags.Instance);
        public static MethodInfo WaitForStompInfo = typeof(EmeraldGolemBossArm).GetMethod("WaitForStomp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static MethodInfo ShakeCoroutineInfo = typeof(EmeraldGolemBossArm).GetMethod("ShakeCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
        public static MethodInfo AnimatorTripleStompCoroutineInfo = typeof(EmeraldGolemBossArm).GetMethod("AnimatorTripleStompCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerator AnimatorQuadrupleStompCoroutine(this EmeraldGolemBossArm self, bool shakeOnFirstStomp, bool shakeSecondStomp, bool shakeLastStomp, float fourthStompX) {
            yield return self.StartCoroutine((IEnumerator)GoToAnimationStartPositionInfo.Invoke(self, null));
            self.animator.enabled = true;
            self.animator.SetTrigger("TripleStomp");
            self.animator.Update(0f);
            if (shakeOnFirstStomp) {
                yield return self.StartCoroutine((IEnumerator)WaitForStompAndShakeInfo.Invoke(self, new object[] { self.tripleStompFirstStompShakeDuration }));
            } else {
                yield return self.StartCoroutine((IEnumerator)WaitForStompInfo.Invoke(self, null));
            }
            if (shakeSecondStomp) {
                yield return self.StartCoroutine((IEnumerator)WaitForStompAndShakeInfo.Invoke(self, new object[] { self.tripleStompSecondStompShakeDuration }));
            } else {
                yield return self.StartCoroutine((IEnumerator)WaitForStompInfo.Invoke(self, null));
            }
            if (shakeLastStomp) {
                yield return self.StartCoroutine((IEnumerator)WaitForStompAndShakeInfo.Invoke(self, new object[] { self.tripleStompThirdStompShakeDuration }));
            } else {
                yield return self.StartCoroutine((IEnumerator)WaitForStompInfo.Invoke(self, null));
            }
            self.animator.enabled = false;
            yield return self.Drop(self.basicStompDropSpeed);
            yield return new WaitForSeconds(.417f);
            yield return self.RaiseToPositionCoroutine(fourthStompX, self.dropY + 5.652562f , .42f);
            yield return ShakeCoroutineInfo.Invoke(self, new object[] { self.tripleStompThirdStompShakeDuration });
            yield return self.Drop(self.basicStompDropSpeed);
            yield return self.GoToDefaultPosition();
        }

        public static IEnumerator AnimatorTripleStompFastCoroutine(this EmeraldGolemBossArm self, bool shakeOnFirstStomp, bool shakeSecondStomp, bool shakeLastStomp) {
            self.tripleStompFirstStompShakeDuration /= 1.3f;
            self.tripleStompSecondStompShakeDuration /= 1.3f;
            self.tripleStompThirdStompShakeDuration /= 1.3f;
            yield return self.StartCoroutine((IEnumerator)AnimatorTripleStompCoroutineInfo.Invoke(self, null));
            self.tripleStompFirstStompShakeDuration *= 1.3f;
            self.tripleStompSecondStompShakeDuration *= 1.3f;
            self.tripleStompThirdStompShakeDuration *= 1.3f;
        }

        public static IEnumerator RaiseToPositionCoroutine(this EmeraldGolemBossArm self, float targetX, float targetY, float specificDuration = -1f) {
            float startX = self.transform.position.x;
            float startY = self.transform.position.y;
            Vector3 target = new Vector3(targetX, targetY);
            float distance = Vector2.Distance(self.transform.position, target);
            float duration = (specificDuration != -1f) ? specificDuration : (distance / self.centerStompRaiseSpeed);
            float progress = 0f;
            while (progress < 1f) {
                progress += Time.deltaTime / duration;
                Vector3 newPos = Vector3.zero;
                newPos.x = Mathf.Lerp(startX, targetX, progress);
                newPos.y = TweenFunctions.Quadratic.Out(startY, targetY, progress);
                self.transform.position = newPos;
                yield return null;
            }
        }


    }
}
