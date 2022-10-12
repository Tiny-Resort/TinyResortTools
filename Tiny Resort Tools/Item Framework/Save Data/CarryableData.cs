using System;
using System.Collections.Generic;
using UnityEngine;

namespace TinyResort {
    
    [Serializable] 
    internal class CarryableData : ItemSaveData {

        public static List<CarryableData> all = new List<CarryableData>();

        public float positionX;
        public float positionY;
        public float positionZ;
        
        public static void LoadAll(bool firstLoad) {
            all = (List<CarryableData>)TRItems.Data.GetValue("CarryableData", new List<CarryableData>());
            TRTools.Log($"Loading CarryableData: {all.Count}");
            foreach (var item in all) { item.Load(firstLoad); }
        }

        public static void Save(PickUpAndCarry myCarry) {
            TRTools.Log($"Attempting to remove: {myCarry.name}");
            all.Add(new CarryableData {
                customItemID = TRItems.customCarryableByID[myCarry.prefabId].customItemID, 
                positionX = myCarry.transform.position.x, 
                positionY = myCarry.transform.position.y, 
                positionZ = myCarry.transform.position.z
            });
            WorldManager.manageWorld.allCarriables.Remove(myCarry);
        }

        public void Load(bool firstLoad) {
            
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;

            // If re-injecting data after saving, just re-add it to list
            if (!firstLoad) { 
                WorldManager.manageWorld.allCarriables.Remove(customItem.carryable);
                return;
            }
            
            // If loading in to save slot, then create the carryable object
            TRTools.Log($"DidThisRUn?");
            NetworkMapSharer.share.spawnACarryable(customItem.carryable.gameObject, new Vector3(positionX, positionY, positionZ), false);
            TRTools.Log($"DidThisRun2?");

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
