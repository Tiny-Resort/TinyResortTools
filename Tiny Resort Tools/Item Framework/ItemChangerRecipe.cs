using System.Collections.Generic;
using System.Linq;

namespace TinyResort {

    internal class ItemChangerRecipe {

        internal static ItemChangeType CreateICT(int output) {
            
            ItemChangeType ict = new ItemChangeType() {
                depositInto = WorldManager.manageWorld.allObjects[50],
                amountNeededed = 1,
                secondsToComplete = 0,
                daysToComplete = 2,
                cycles = 1,
                changesWhenComplete = Inventory.inv.allItems[output],
                changesWhenCompleteTable = null,
                taskType = DailyTaskGenerator.genericTaskType.SmeltOre,
                givesXp = true,
                xPType = CharLevelManager.SkillTypes.Mining
            };

            return ict;
        }

        internal static ItemChange CreateIC(int input) {
            ItemChange ic;

            if (Inventory.inv.allItems[input].itemChange != null)
                ic = Inventory.inv.allItems[input].itemChange;
            else
                ic = Inventory.inv.allItems[input].gameObject.AddComponent<ItemChange>();

            return ic;
        }

        internal static List<ItemChangeType> CreateICTL(ItemChange ic) {
            List<ItemChangeType> ictl;

            if (ic.changesAndTheirChanger != null)
                ictl = ic.changesAndTheirChanger.ToList();
            else
                ictl = new List<ItemChangeType>();

            return ictl;
        }
    }

}
