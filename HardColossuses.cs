using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Mod.Courier.Helpers;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class HardColossuses {
        public static MethodInfo bfmOnBossDeadInfo = typeof(SearingCragsBossFightManager).GetMethod("OnBossDead", BindingFlags.NonPublic | BindingFlags.Instance);

        public bool Enabled => HardBossesModule.Save.hardColossuses;

        public void Toggle() {
            HardBossesModule.Save.hardColossuses = !HardBossesModule.Save.hardColossuses;
            HardBossesModule.Instance.colossusesButton.UpdateStateText();
        }

        // TODO swap fg and bg colossus randomly for states affected in second phase
        public void Load() {
            On.SearingCragsBossFightManager.BeginFight += SearingCragsBossFightManager_BeginFight;

            On.ColossusesBoss.OnDie += ColossusesBoss_OnDie;
            On.ColossusesBoss.DoBossMove += ColossusesBoss_DoBossMove;
            On.ColossusesBoss.DoSpecialMove += ColossusesBoss_DoSpecialMove;

            On.SearingCragsBossOutroCutscene.Explosion += SearingCragsBossOutroCutscene_Explosion;
            On.SearingCragsBossOutroCutscene.OnCutsceneDone += SearingCragsBossOutroCutscene_OnCutsceneDone;

            On.ColossusesRequestRock.Behave += ColossusesRequestRock_Behave;

            On.ColossusesHighFiveInState.Behave += ColossusesHighFiveInState_Behave;

            On.ColossusesPuntInState.Behave += ColossusesPuntInState_Behave;

            On.ColossusesColosFrontState.StateEnter += ColossusesColosFrontState_StateEnter;

            On.ColossusesSusesFrontState.StateEnter += ColossusesSusesFrontState_StateEnter;
            On.ColossusesSusesFrontState.Ground += ColossusesSusesFrontState_Ground;
        }

        void ColossusesSusesFrontState_StateEnter(On.ColossusesSusesFrontState.orig_StateEnter orig, ColossusesSusesFrontState self, StateMachine stateMachine) {
            var fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                self.walkMinTime = 0.75f;
                self.walkMaxTime = 3f;
                self.speedX = 3f;
            }
            orig(self, stateMachine);
        }

        void ColossusesColosFrontState_StateEnter(On.ColossusesColosFrontState.orig_StateEnter orig, ColossusesColosFrontState self, StateMachine stateMachine) {
            var fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                self.walkMinTime = 0.75f;
                self.walkMaxTime = 3f;
                self.speedX = 3f;
            }
            orig(self, stateMachine);
        }

        void ColossusesBoss_DoSpecialMove(On.ColossusesBoss.orig_DoSpecialMove orig, ColossusesBoss self) {
            var fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                self.moveRequested = true;
                if(!(self.colos.GetCurrentState() is ColossusesEmptyState) && !(self.colos.GetCurrentState() is ColossusesColosFrontState)) {
                    return;
                }
                if (!(self.suses.GetCurrentState() is ColossusesEmptyState) && !(self.suses.GetCurrentState() is ColossusesSusesFrontState)) {
                    return;
                }
            }
            orig(self);
        }

        void ColossusesBoss_DoBossMove(On.ColossusesBoss.orig_DoBossMove orig, ColossusesBoss self, ColossusesBoss.attacks move) {
            var fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                // Sometimes have both bosses do their melee attack at the same time
                if (move == ColossusesBoss.attacks.melee && UnityEngine.Random.Range(0f, 1f) < 0.4f) {
                    self.bgColossus.DoMeleeAttack();
                }
            }
            orig(self, move);
        }

        void ColossusesSusesFrontState_Ground(On.ColossusesSusesFrontState.orig_Ground orig, ColossusesSusesFrontState self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            var selfData = new DynData<ColossusesSusesFrontState>(self);
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false && selfData.Get<bool?>("skipGrounding").HasValue) {
                // TODO I feel like there are probably side effects to this but I haven't seen them 
            } else {
                orig(self);
            }
        }

        IEnumerator ColossusesPuntInState_Behave(On.ColossusesPuntInState.orig_Behave orig, ColossusesPuntInState self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            var selfData = new DynData<ColossusesPuntInState>(self);
            ColossusesBoss boss = selfData.Get<ColossusesBoss>("boss");
            StateMachine stateMachine = selfData.Get<StateMachine>("stateMachine");

            // If in second phase
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                boss.bgColossus.SetState<ColossusesEmptyState>();
                boss.bgColossus.jumpInDone = false;
                boss.bgColossus.SetTrigger("JumpToBG");
                while (!boss.bgColossus.jumpInDone) {
                    yield return null;
                }
                boss.bgColossus.SetTrigger("JumpLoop");
                float timeDone4 = 0f;
                float totalTime4 = 1f;
                Vector3 startPos = boss.bgColossus.transform.position;
                Vector3 endPos = startPos;
                endPos.y = boss.offScreenPos.position.y;
                while (timeDone4 < totalTime4) {
                    boss.bgColossus.transform.position = Vector3.Lerp(startPos, endPos, timeDone4 / totalTime4);
                    timeDone4 += TimeVars.GetDeltaTime();
                    yield return null;
                }
                boss.fgColossus.SetState<ColossusesEmptyState>();
                boss.fgColossus.FacePlayer();
                boss.fgColossus.SetTrigger("PuntIn");
                Vector3 punterGroundPos = boss.fgColossus.transform.position;
                if (boss.fgColossus.FacingRight()) {
                    boss.bgColossus.FaceRight();
                    punterGroundPos += Vector3.left * 2f;
                } else {
                    boss.bgColossus.FaceLeft();
                    punterGroundPos += Vector3.right * 2f;
                }
                boss.bgColossus.transform.position = new Vector3(punterGroundPos.x, boss.bgColossus.transform.position.y);
                Vector3 punterSkyPos = boss.bgColossus.transform.position;
                timeDone4 = 0f;
                totalTime4 = 1f;
                while (timeDone4 < totalTime4) {
                    boss.bgColossus.transform.position = Vector3.Lerp(punterSkyPos, punterGroundPos, timeDone4 / totalTime4);
                    timeDone4 += TimeVars.GetDeltaTime();
                    yield return null;
                }
                boss.bgColossus.transform.position = punterGroundPos;
                boss.bgColossus.SetTrigger("FGLand");
                yield return new WaitForSeconds(1f);
                selfData["puntEventDone"] = false;
                Action d = () => {
                    CustomOnPuntEvent(selfData, boss);
                };
                boss.bgColossus.onPunt += d;
                boss.bgColossus.SetTrigger("Punt");
                while (!selfData.Get<bool>("puntEventDone")) {
                    yield return null;
                }
                boss.fgColossus.GetPunted();
                boss.bgColossus.onPunt -= d;
                yield return new WaitForSeconds(0.25f);
                if (boss.bgColossus is Suses) {
                    var stateData = new DynData<ColossusesSusesFrontState>(boss.bgColossus.SetState<ColossusesSusesFrontState>());
                    stateData["skipGrounding"] = true;
                } else {
                    boss.bgColossus.SetState<ColossusesColosFrontState>();
                }
                stateMachine.SetState<ColossusesEmptyState>();
            } else {
                yield return orig(self);
            }
        }

        private void CustomOnPuntEvent(DynData<ColossusesPuntInState> selfData, ColossusesBoss boss) {
            selfData["puntEventDone"] = true;
            boss.fgColossus.GetPunted();
        }

        IEnumerator ColossusesHighFiveInState_Behave(On.ColossusesHighFiveInState.orig_Behave orig, ColossusesHighFiveInState self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            var selfData = new DynData<ColossusesHighFiveInState>(self);
            ColossusesBoss boss = selfData.Get<ColossusesBoss>("boss");
            StateMachine stateMachine = selfData.Get<StateMachine>("stateMachine");

            // If in second phase
            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                if (boss.BothKO) {
                    boss.bgColossus.SetState<ColossusesEmptyState>();
                    boss.fgColossus.SetState<ColossusesEmptyState>();

                    boss.colos.jumpInDone = false;
                    boss.suses.jumpInDone = false;
                    boss.colos.FaceLeft();
                    boss.colos.SetTrigger("JumpToBG");
                    boss.suses.SetTrigger("JumpToBG");
                    while (!boss.colos.jumpInDone || !boss.suses.jumpInDone) {
                        yield return null;
                    }
                    boss.colos.jumpInDone = false;
                    boss.suses.jumpInDone = false;
                    float timeDone = 0f;
                    float totalTime = 0.3f;
                    Vector3 colosStart = boss.colos.transform.position;
                    Vector3 susesStart = boss.suses.transform.position;
                    while (timeDone < totalTime) {
                        float ratio = timeDone / totalTime;
                        boss.colos.transform.position = Vector3.Lerp(colosStart, boss.colosHighFivePosR.position, ratio);
                        boss.suses.transform.position = Vector3.Lerp(susesStart, boss.susesHighFivePosL.position, ratio);
                        timeDone += TimeVars.GetDeltaTime();
                        yield return null;
                    }
                    boss.colos.transform.position = boss.colosHighFivePosR.position;
                    boss.suses.transform.position = boss.susesHighFivePosL.position;
                    boss.colos.SetTrigger("HighFiveFail");
                    boss.suses.SetTrigger("HighFiveFail");
                    yield return new WaitForSeconds(0.5f);
                    boss.fgColossus.SetState<ColossusesEmptyState>();
                    boss.onDie += delegate { bfmOnBossDeadInfo.Invoke(Manager<SearingCragsBossFightManager>.Instance, null); };
                    stateMachine.SetState<ColossusesHighFailOutState>();
                } else {
                    if(boss.suses.GetCurrentState() is ColossusesSusesFrontState state) {
                        var stateData = new DynData<ColossusesSusesFrontState>(state);
                        stateData["specialMoveRequested"] = false;
                    }
                    if (boss.colos.GetCurrentState() is ColossusesColosFrontState state2) {
                        var stateData = new DynData<ColossusesColosFrontState>(state2);
                        stateData["specialMoveRequested"] = false;
                    }
                    BaseColossus temp = boss.fgColossus;
                    boss.fgColossus = boss.bgColossus;
                    boss.bgColossus = temp;
                    stateMachine.SetState<ColossusesEmptyState>();
                }
            } else {
                yield return orig(self);
            }
        }

        IEnumerator ColossusesRequestRock_Behave(On.ColossusesRequestRock.orig_Behave orig, ColossusesRequestRock self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);

            if (fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").HasValue && fightManagerData.Get<bool?>("BrokemiaHarderBossesFakeout").Value == false) {
                var selfData = new DynData<ColossusesRequestRock>(self);
                ColossusesBoss boss = selfData.Get<ColossusesBoss>("boss");
                StateMachine stateMachine = selfData.Get<StateMachine>("stateMachine");
                yield return new WaitForSeconds(0.2f);
                if (boss.fgColossus is Colos) {
                    boss.fgColossus.SetState<ColossusesColosThrowState>();
                } else {
                    boss.fgColossus.SetState<ColossusesSusesThrowState>();
                }
                stateMachine.SetState<ColossusesEmptyState>();
            } else {
                yield return orig(self);
            }
        }

        void SearingCragsBossFightManager_BeginFight(On.SearingCragsBossFightManager.orig_BeginFight orig, SearingCragsBossFightManager self) {
            orig(self);
            DynData<SearingCragsBossFightManager> selfData = new DynData<SearingCragsBossFightManager>(self);
            selfData.Set("BrokemiaHarderBossesFakeout", true);
        }

        void ColossusesBoss_OnDie(On.ColossusesBoss.orig_OnDie orig, ColossusesBoss self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);
            if (fightManagerData.Get<bool>("BrokemiaHarderBossesFakeout")) {
                self.colos.hurtZone.enabled = false;
                self.suses.hurtZone.enabled = false;
                bfmOnBossDeadInfo.Invoke(Manager<SearingCragsBossFightManager>.Instance, null);
                Manager<ProgressionManager>.Instance.bossesDefeated.Remove(BossIds.COLOS_SUSSES);
                Manager<ProgressionManager>.Instance.allTimeBossesDefeated.Remove(BossIds.COLOS_SUSSES);
            } else {
                orig(self);
            }
        }

        IEnumerator SearingCragsBossOutroCutscene_Explosion(On.SearingCragsBossOutroCutscene.orig_Explosion orig, SearingCragsBossOutroCutscene self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);

            if (fightManagerData.Get<bool>("BrokemiaHarderBossesFakeout")) {
                DynData<SearingCragsBossOutroCutscene> selfData = new DynData<SearingCragsBossOutroCutscene>(self);
                Colos colos = selfData.Get<Colos>("colos");
                Suses suses = selfData.Get<Suses>("suses");

                colos.GetComponent<PaletteSwapBlink>().StopBlinking();
                suses.GetComponent<PaletteSwapBlink>().StopBlinking();
                yield return new WaitForSeconds(1f);
                Manager<Level>.Instance.RetroCamera.GetComponent<PaletteSwapImageEffect>().enabled = true;
                yield return Manager<Level>.Instance.RetroCamera.GetComponent<PaletteSwapImageEffect>().FadeToWhite(2f);
                Manager<PlayerManager>.Instance.Player.transform.position = self.playerStewPos.position;
                colos.transform.position = self.colosStewPos.position;
                colos.FacePlayer();
                colos.SetTrigger("Stun");
                suses.transform.position = self.susesStewPos.position;
                suses.FacePlayer();
                suses.SetTrigger("Stun");
                yield return new WaitForSeconds(0.5f);
                yield return Manager<Level>.Instance.RetroCamera.GetComponent<PaletteSwapImageEffect>().FadeWhiteToGame(3f);
                Manager<Level>.Instance.RetroCamera.GetComponent<PaletteSwapImageEffect>().enabled = false;
                yield return new WaitForSeconds(0.5f);
                colos.SetTrigger("Idle");
                suses.SetTrigger("Idle");
                yield return new WaitForSeconds(0.75f);
                self.EndCutScene();
            } else {
                yield return orig(self);
            }
        }

        void SearingCragsBossOutroCutscene_OnCutsceneDone(On.SearingCragsBossOutroCutscene.orig_OnCutsceneDone orig, SearingCragsBossOutroCutscene self) {
            DynData<SearingCragsBossFightManager> fightManagerData = new DynData<SearingCragsBossFightManager>(Manager<SearingCragsBossFightManager>.Instance);

            if (fightManagerData.Get<bool>("BrokemiaHarderBossesFakeout")) {
                Manager<GameManager>.Instance.RemovePlayingCutscene(self);
                fightManagerData.Set("BrokemiaHarderBossesFakeout", false);
                DynData<SearingCragsBossOutroCutscene> selfData = new DynData<SearingCragsBossOutroCutscene>(self);
                Colos colos = selfData.Get<Colos>("colos");
                Suses suses = selfData.Get<Suses>("suses");

                colos.SetHP(colos.maxHP);
                suses.SetHP(suses.maxHP);

                colos.hurtZone.enabled = true;
                suses.hurtZone.enabled = true;

                colos.GetComponent<PaletteSwapBlink>().StartBlinking();
                suses.GetComponent<PaletteSwapBlink>().StartBlinking();

                Manager<SearingCragsBossFightManager>.Instance.colossusesInstance.SetState<ColossusesEmptyState>();
                colos.SetState<ColossusesColosChargeState>();
                suses.SetState<ColossusesSusesFrontState>();
            } else {
                orig(self);
            }
        }

        public void Unload() {
            On.SearingCragsBossFightManager.BeginFight -= SearingCragsBossFightManager_BeginFight;

            On.ColossusesBoss.OnDie -= ColossusesBoss_OnDie;
            On.ColossusesBoss.DoBossMove -= ColossusesBoss_DoBossMove;
            On.ColossusesBoss.DoSpecialMove -= ColossusesBoss_DoSpecialMove;

            On.SearingCragsBossOutroCutscene.Explosion -= SearingCragsBossOutroCutscene_Explosion;
            On.SearingCragsBossOutroCutscene.OnCutsceneDone -= SearingCragsBossOutroCutscene_OnCutsceneDone;

            On.ColossusesRequestRock.Behave -= ColossusesRequestRock_Behave;

            On.ColossusesHighFiveInState.Behave -= ColossusesHighFiveInState_Behave;

            On.ColossusesPuntInState.Behave -= ColossusesPuntInState_Behave;

            On.ColossusesColosFrontState.StateEnter -= ColossusesColosFrontState_StateEnter;

            On.ColossusesSusesFrontState.StateEnter -= ColossusesSusesFrontState_StateEnter;
            On.ColossusesSusesFrontState.Ground -= ColossusesSusesFrontState_Ground;
        }
    }
}
