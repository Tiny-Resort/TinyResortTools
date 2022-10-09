using System;
using System.Collections.Generic;

namespace TinyResort {
    
    [Serializable]
    internal class LetterData : ItemSaveData {

        public static List<LetterData> all = new List<LetterData>();
        public int itemAttached;
        public int itemOriginallAttached;
        public int stackOfItemAttached;
        public Letter.LetterType myType;
        public int seasonSent;
        public int letterTemplateNo;
        public int sentById;
        public bool hasBeenRead;
        public bool tomorrow;

        public static void Save(Letter toRemove, bool tomorrow) {
            
            all.Add(new LetterData {
                customItemID = TRItems.customItemsByItemID[toRemove.itemAttached].customItemID, itemAttached = toRemove.itemAttached, itemOriginallAttached = toRemove.itemOriginallAttached, 
                stackOfItemAttached = toRemove.stackOfItemAttached, myType = toRemove.myType, seasonSent = toRemove.seasonSent,
                letterTemplateNo = toRemove.letterTemplateNo, sentById = toRemove.sentById, hasBeenRead = toRemove.hasBeenRead, tomorrow = tomorrow
            });
            
            if (tomorrow) { MailManager.manage.tomorrowsLetters.Remove(toRemove); }
            else { MailManager.manage.mailInBox.Remove(toRemove); }
            
        }

        public static void LoadAll() {
            all = (List<LetterData>)TRItems.Data.GetValue("LetterData", new List<LetterData>());
            foreach (var item in all) { item.Load(); }
        }

        public void Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return;
            Letter letter = new Letter(sentById, myType, itemAttached, stackOfItemAttached);
            letter.itemOriginallAttached = itemOriginallAttached;
            letter.seasonSent = seasonSent;
            letter.hasBeenRead = hasBeenRead;
            if (!tomorrow) { MailManager.manage.mailInBox.Add(letter); }
            else { MailManager.manage.tomorrowsLetters.Add(letter); }
        }

    }

}
