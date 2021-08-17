using System;
using System.Collections;
using System.Reflection;
using Mod.Courier;
using MonoMod.Utils;
using UnityEngine;

namespace HarderBosses {
    public class HardBambooCreek {
        TurtleManRangedSpawner spawner;

        public bool Enabled => HardBossesModule.Save.hardBambooCreek;

        public void Toggle() {
            HardBossesModule.Save.hardBambooCreek = !HardBossesModule.Save.hardBambooCreek;
            HardBossesModule.Instance.bambooCreekButton.UpdateStateText();
        }

        public void Load() {
            On.SceneLoader.OnSceneLoaded += SceneLoader_OnSceneLoaded;
            On.ObjectSpawner.Start += ObjectSpawner_Start;
        }

        public void Unload() {
            On.SceneLoader.OnSceneLoaded -= SceneLoader_OnSceneLoaded;
            On.ObjectSpawner.Start -= ObjectSpawner_Start;
        }

        void ObjectSpawner_Start(On.ObjectSpawner.orig_Start orig, ObjectSpawner self) {
            orig(self);
            if(self == spawner) {
                DynData<ObjectSpawner> spawnerData = new DynData<ObjectSpawner>(self);
                spawnerData.Set("spawnRect", new Rect(new Vector2(spawner.transform.position.x - 1.03f, spawner.transform.position.y - .5f), new Vector2(2, 3)));
            }
        }

        void SceneLoader_OnSceneLoaded(On.SceneLoader.orig_OnSceneLoaded orig, UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode) {
            orig(scene, loadSceneMode);

            if(scene.name.Equals(ELevel.Level_06_A_BambooCreek.ToString() + "_Build")) {
                try {
                    spawner = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<TurtleManRangedSpawner>()[0]);
                    Vector3 newPos = new Vector3(475f, -90.5f);
                    spawner.transform.position = newPos;
                    Manager<Level>.Instance.LevelRooms["460492-92-76"].roomObjects.Add(spawner.gameObject);

                    SpawnZone zone = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<SkeloutSpawner>()[0].transform.parent.parent.gameObject).GetComponent<SpawnZone>();
                    zone.transform.position = newPos;
                    Manager<Level>.Instance.LevelRooms["460492-92-76"].roomObjects.Add(zone.spawners[0].gameObject);
                    zone.spawnZoneRect = new Rect(new Vector2(460, -92), new Vector2(32, 16));
                    zone.spawnLaneCount = 3;
                    zone.spawnInterval = .5f;
                    zone.maxSpawnedObjects = 30;

                    CourierLogger.Log("HarderBosses", "All bamboo creek spawners placed");
                } catch (Exception e) {
                    Console.WriteLine("Exception while placing bamboo creek spawners");
                    CourierLogger.LogDetailed(e, "HarderBosses");

                }
            }
        }

    }
}
