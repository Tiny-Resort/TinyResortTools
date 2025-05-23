using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TinyResort
{
    [Serializable]
    internal class VehicleData : ItemSaveData {

        public static List<VehicleData> all = new();
        public static List<VehicleData> lostAndFound = new();
        public int colourVariation;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public static void LoadAll(bool firstLoad) {
            lostAndFound = (List<VehicleData>)TRItems.Data.GetValue("VehicleDataLostAndFound", new List<VehicleData>());

            //TRTools.Log($"Loading VehicleData lostAndFound: {lostAndFound.Count}");

            all = (List<VehicleData>)TRItems.Data.GetValue("VehicleData", new List<VehicleData>());

            //TRTools.Log($"Loading VehicleData: {all.Count}");
            foreach (var item in all)
                try {
                    if (item.Load(firstLoad) == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(Vehicle toRemove) {
            all.Add(
                new VehicleData {
                    customItemID = TRItems.customVehicleByID[toRemove.saveId].customItemID,
                    colourVariation = toRemove.getVariation(), positionX = toRemove.transform.position.x,
                    positionY = toRemove.transform.position.y, positionZ = toRemove.transform.position.z,
                    rotationX = toRemove.transform.eulerAngles.x, rotationY = toRemove.transform.eulerAngles.y,
                    rotationZ = toRemove.transform.eulerAngles.z
                }
            );
            SaveLoad.saveOrLoad.vehiclesToSave.Remove(toRemove);

        }

        public TRCustomItem Load(bool firstLoad) {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;

            // If re-injecting data after saving, just re-add it to list
            if (!firstLoad) {
                SaveLoad.saveOrLoad.vehiclesToSave.Add(customItem.vehicle);
                return customItem;
            }

            // If loading into save slot, then create the item and set it up
            var gameObject = Object.Instantiate(
                customItem.inventoryItem.spawnPlaceable,
                new Vector3(positionX, positionY, positionZ),
                Quaternion.Euler(rotationX, rotationY, rotationZ)
            );

            gameObject.GetComponent<Vehicle>().setVariation(colourVariation);
            NetworkMapSharer.Instance.spawnGameObject(gameObject);

            return customItem;
        }
    }
}
