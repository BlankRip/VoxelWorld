using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using BlockyWorld.WorldBuilding;

namespace BlockyWorld.SaveLoadSystem
{
    public static class FileSaver
    {
        private static WorldData worldData;
        static string BuildFileName() {
            return $"{Application.persistentDataPath}/saveData/World_c_{World.chunkDimensions.x}_{World.chunkDimensions.y}_{World.chunkDimensions.z}_w_{World.worldDimensions.x}_{World.worldDimensions.y}_{World.worldDimensions.z}.dat";
        }

        public static void Save(World world) {
            string fileName = BuildFileName();
            if(!File.Exists(fileName))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fileName, FileMode.OpenOrCreate);
            worldData = new WorldData(world.chunkChecker, world.chunkColumns, world.chunks, world.firstPersonController.transform.position);
            bf.Serialize(file, worldData);
            file.Close();
            Debug.Log($"Saved World to File: {fileName}");
        }

        public static WorldData Load() {
            string fileName = BuildFileName();
            if(File.Exists(fileName)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(fileName, FileMode.Open);
                worldData = (WorldData) bf.Deserialize(file);
                Debug.Log($"Loaded World from File: {fileName}");
                return worldData;
            }
            return null;
        }
    }
}
