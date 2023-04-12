using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace TinyResort;

public class TRItemTools {

    internal static void GenerateChestDetails() {
        
    }

    internal static List<ChestPlaceable> LocateNearbyContainers(int startingX, int startingY, int radius) {
        TRTools.LogError($"Initial - X: {startingX} | Y: {startingY}");

        var foundChests = new List<ChestPlaceable>();

        var allObjects = WorldManager.manageWorld.allObjects;
        var onTileMap = WorldManager.manageWorld.onTileMap;

        // Clamp starting and ending values to not be over the map size.
        var endingX = Mathf.Clamp(startingX, startingX + radius, onTileMap.GetLength(0));
        var endingY = Mathf.Clamp(startingY, startingY + radius, onTileMap.GetLength(0));
        startingX = Mathf.Clamp(startingX, 0, startingX - radius);
        startingY = Mathf.Clamp(startingY, 0, startingY - radius);
        /*var endingX = startingX + radius > onTileMap.GetLength(0) ? onTileMap.GetLength(0) : startingX + radius;
        var endingY = startingY + radius > onTileMap.GetLength(1) ? onTileMap.GetLength(1) : startingY + radius;*/
        /*var startX = startingX - radius < 0 ? 0 : startingX - radius;
        var startY = startingY - radius < 0 ? 0 : startingY - radius;*/

        TRTools.LogError($"Starting - X: {startingX} | Y: {startingY}");
        TRTools.LogError($"Ending - X: {endingX} | Y: {endingY}");

        // Loop through the radius around
        for (var x = startingX; x < endingX; x++) {
            for (var y = startingY; y < endingY; y++) {

                // If the tile is empty, ignore it
                if (onTileMap[x, y] <= -1) continue;

                if (allObjects[onTileMap[x, y]].tileObjectChest) {
                    // Gains access to the contents of the chest
                    allObjects[onTileMap[x, y]].tileObjectChest.checkIfEmpty(x, y, null);

                    // Add the chest to a found chest list to return to the user
                    foundChests.Add(allObjects[onTileMap[x, y]].tileObjectChest);

                    TRTools.LogError($"World Position: ({x},{y})");
                }
                else if (allObjects[onTileMap[x, y]].displayPlayerHouseTiles) {
                    var houseDetails = HouseManager.manage.getHouseInfo(x, y);

                    // Checks every tile inside a house to find custom objects
                    for (var houseTileX = 0; houseTileX < houseDetails.houseMapOnTile.GetLength(0); houseTileX++) {
                        for (var houseTileY = 0; houseTileY < houseDetails.houseMapOnTile.GetLength(1); houseTileY++) {

                            // If nothing is on this tile, ignore it
                            var tileObjectID = houseDetails.houseMapOnTile[houseTileX, houseTileY];
                            var houseMapOnTileStatus = houseDetails.houseMapOnTileStatus[houseTileX, houseTileY];
                            if (tileObjectID <= 0) continue;
                            if (allObjects[tileObjectID].tileObjectChest) {
                                // Gains access to the contents of the chest
                                allObjects[tileObjectID]
                                   .tileObjectChest.checkIfEmpty(houseTileX, houseTileY, houseDetails);

                                // Add the chest to a found chest list to return to the user
                                foundChests.Add(allObjects[tileObjectID].tileObjectChest);
                                TRTools.LogError($"House Position: ({houseTileX},{houseTileY})");
                            }
                        }
                    }
                }
            }
        }
        return foundChests;
    }
}
