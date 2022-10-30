using System;
using System.Collections.Generic;

namespace TinyResort {
    
    [Serializable]
    internal class LetterData : ItemSaveData {

        public static List<LetterData> all = new List<LetterData>();
        public static List<LetterData> lostAndFound = new List<LetterData>();
        public int itemAttached;
        public int itemOriginallAttached;
        public int stackOfItemAttached;
        public Letter.LetterType myType;
        public int seasonSent;
        public int letterTemplateNo;
        public int sentById;
        public bool hasBeenRead;
        public bool tomorrow;

        public static void LoadAll() {
            lostAndFound = (List<LetterData>)TRItems.Data.GetValue("LetterDataLostAndFound", new List<LetterData>());
            //TRTools.Log($"Loading LetterData lostAndFound: {lostAndFound.Count}");

            all = (List<LetterData>)TRItems.Data.GetValue("LetterData", new List<LetterData>());
            //TRTools.Log($"Loading LetterData: {all.Count}");
            foreach (var item in all) {
                try {
                    if (item.Load() == null) {
                        if (!lostAndFound.Contains(item)) { lostAndFound.Add(item); }
                    }
                }
                catch { TRTools.LogError($"Failed to load item: {item.customItemID}"); }
            }
        }
        
        public static void Save(Letter toRemove, bool tomorrow) {
            
            all.Add(new LetterData {
                customItemID = TRItems.customItemsByItemID[toRemove.itemAttached].customItemID, itemAttached = toRemove.itemAttached, itemOriginallAttached = toRemove.itemOriginallAttached, 
                stackOfItemAttached = toRemove.stackOfItemAttached, myType = toRemove.myType, seasonSent = toRemove.seasonSent,
                letterTemplateNo = toRemove.letterTemplateNo, sentById = toRemove.sentById, hasBeenRead = toRemove.hasBeenRead, tomorrow = tomorrow
            });
            
            if (tomorrow) { MailManager.manage.tomorrowsLetters.Remove(toRemove); }
            else { MailManager.manage.mailInBox.Remove(toRemove); }
            
        }
        
        public TRCustomItem Load() {
            if (!TRItems.customItems.TryGetValue(customItemID, out var customItem)) return null;
            Letter letter = new Letter(sentById, myType, itemAttached, stackOfItemAttached);
            letter.itemOriginallAttached = itemOriginallAttached;
            letter.seasonSent = seasonSent;
            letter.hasBeenRead = hasBeenRead;
            if (!tomorrow) { MailManager.manage.mailInBox.Add(letter); }
            else { MailManager.manage.tomorrowsLetters.Add(letter); }
            return customItem;
        }

    }

}
