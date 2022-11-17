using System.Collections.Generic;
using System.Linq;

namespace TinyResort;

internal class ItemChangerRecipe {
    internal static ItemChangeType ict;
    internal static ItemChange ic;
    internal static List<ItemChangeType> ictl = new();

    // Need to be able to have TRCustomItems included in this...
    internal static void AddItemChangerRecipe(
        int output, int input, int machine,
        int requiredAmount, int daysToComplete,
        int secondsToComplete, int cycles,
        DailyTaskGenerator.genericTaskType taskType, bool givesXP,
        CharLevelManager.SkillTypes xpType, InventoryItemLootTable lootTable = null
    ) {
        ict = new ItemChangeType {
            depositInto = WorldManager.manageWorld.allObjects[machine], // Furnace = 50
            amountNeededed = requiredAmount,
            secondsToComplete = secondsToComplete,
            daysToComplete = daysToComplete,
            cycles = cycles,
            changesWhenComplete = Inventory.inv.allItems[output],
            changesWhenCompleteTable = lootTable,
            taskType = taskType,
            givesXp = givesXP,
            xPType = xpType
        };

        if (Inventory.inv.allItems[input].itemChange != null)
            ic = Inventory.inv.allItems[input].itemChange;
        else
            ic = Inventory.inv.allItems[input].gameObject.AddComponent<ItemChange>();

        if (ic.changesAndTheirChanger != null)
            ictl = ic.changesAndTheirChanger.ToList();
        else
            ictl = new List<ItemChangeType>();

        ictl.Add(ict);
        ic.changesAndTheirChanger = ictl.ToArray();
        Inventory.inv.allItems[input].itemChange = ic;

    }
}
