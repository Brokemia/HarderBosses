using System;
using System.Collections;
using System.Reflection;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public static class LeafGolemProjectileExtensions {
        public static MethodInfo RaiseOnReachedPositionInfo = typeof(LeafGolemProjectile).GetEvent("onReachedPosition", BindingFlags.Public | BindingFlags.Instance).GetRaiseMethod();

        internal static void Raise<TEventArgs>(this object source, string eventName, TEventArgs eventArgs) where TEventArgs : EventArgs {
            var eventDelegate = (MulticastDelegate)source.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
            if (eventDelegate != null) {
                foreach (var handler in eventDelegate.GetInvocationList()) {
                    handler.Method.Invoke(handler.Target, new object[] { source });
                }
            }
        }

        public static void GoToQuadSineWave(this LeafGolemProjectile self, Vector3 position, float duration) {
            DynData<LeafGolemProjectile> selfData = new DynData<LeafGolemProjectile>(self);
            if (selfData["goToCoroutine"] != null) {
                self.StopCoroutine(selfData.Get<Coroutine>("goToCoroutine"));
                selfData["goToCoroutine"] = null;
            }
            selfData["goToCoroutine"] = self.StartCoroutine(self.GoToQuadSineWaveCoroutine(position, duration));
        }

        public static IEnumerator GoToQuadSineWaveCoroutine(this LeafGolemProjectile self, Vector3 targetPosition, float duration) {
            Vector3 startPos = self.transform.position;
            float progress = 0f;
            while (progress < 1f) {
                progress += TimeVars.GetDeltaTime() / duration;
                float tweenProgress = TweenFunctions.Quadratic.Out(0f, 1f, progress);
                float x = Vector3.Lerp(startPos, targetPosition, tweenProgress).x;
                float y = targetPosition.y - 1 + Mathf.Cos(6 * Mathf.PI * progress) / 2;
                self.transform.position = new Vector3(x, y, self.transform.position.z);
                yield return null;
            }
            self.Raise("onReachedPosition", EventArgs.Empty);
        }
    }
}
