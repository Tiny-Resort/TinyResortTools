using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyResort {
    [Serializable]
    internal class CarryableData : ItemSaveData {

        public static List<CarryableData> all = new();
        public static List<CarryableData> lostAndFound = new();

        public float positionX;
        public float positionY;
        public float positionZ;

        public static void LoadAll(bool firstLoad) {
            lostAndFound = (List<CarryableData>)TRItems.Data.GetValue(
                "CarryableDataLostAndFound", new List<CarryableData>()
            );

            //TRTools.Log($"Loading CarryableData lostAndFound: {lostAndFound.Count}");

            all = (List<CarryableData>)TRItems.Data.GetValue("CarryableData", new List<CarryableData>());

            //TRTools.Log($"Loading CarryableData: {all.Count}");

            foreach (var item in all)
                try {
                    if (item.Load(firstLoad) == null)
                        if (!lostAndFound.Contains(item))
                            lostAndFound.Add(item);
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
        }

        public static void Save(PickUpAndCarry myCarry) {
            all.Add(
                new CarryableData {
                    customItemID = TRItems.customCarryableByID[myCarry.prefabId].customItemID,
                    positionX = myCarry.transform.position.x, positionY = myCarry.transform.position.y,
                    positionZ = myCarry.transform.position.z
                }
            );
            WorldManager.Instance.allCarriables.Remove(myCarry);
        }

        public TRCustomItem Load(bool firstLoad) {

            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;

            // If re-injecting data after saving, just re-add it to list
            if (!firstLoad) {
                WorldManager.Instance.allCarriables.Remove(customItem.pickUpAndCarry);
                return customItem;
            }

            // If loading in to save slot, then create the carryable object
            NetworkMapSharer.Instance.spawnACarryable(
                customItem.pickUpAndCarry.gameObject, new Vector3(positionX, positionY, positionZ), false
            );

            return customItem;
        }
    }
}

// Possibly needed if animals can stay in traps over night after restarting the game?
/*
public bool farmAnimalBox;
public bool trappedAnimal;
public int animalId;
public int animalVariation;
public string animalName;
AnimalCarryBox component = myCarry.GetComponent<AnimalCarryBox>();
if (component) {
    this.farmAnimalBox = true;
    this.trappedAnimal = false;
    this.animalId = component.animalId;
    this.animalVariation = component.variation;
    this.animalName = component.animalName;
}
TrappedAnimal component2 = myCarry.GetComponent<TrappedAnimal>();
if (component2) {
    this.farmAnimalBox = false;
    this.trappedAnimal = true;
    this.animalId = component2.trappedAnimalId;
    this.animalVariation = component2.trappedAnimalVariation;
}*/
