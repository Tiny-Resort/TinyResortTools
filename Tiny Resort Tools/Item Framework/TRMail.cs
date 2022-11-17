namespace TinyResort;

// TODO: Create LetterType template for Mods
// TODO: Create more robust system for developers to send user mail
// TODO: Look at ShowLetter function
/// <summary> Tools for sending the player custom mail. </summary>
public class TRMail {

    /// <summary>Sends the player a letter containing the given item.</summary>
    /// <param name="itemToSend">The item you want to send.</param>
    /// <param name="quantity">How many of the item you want to include.</param>
    /// <param name="immediate">
    ///     Normally, the player will recieve this letter upon waking up the following day. Set this to
    ///     true to make it immediately arrive.
    /// </param>
    /// <param name="NPC">The ID of the NPC you want to have sent the letter. 1 = John</param>
    public static void SendItemInMail(TRCustomItem itemToSend, int quantity, bool immediate = false, int NPC = 1) {
        var letter = new Letter(NPC, Letter.LetterType.AnimalTrapReturn, itemToSend.inventoryItem.getItemId(), quantity);
        if (immediate)
            MailManager.manage.mailInBox.Add(letter);
        else
            MailManager.manage.tomorrowsLetters.Add(letter);
    }

    /// <summary>Sends the player a letter containing the given item.</summary>
    /// <param name="itemToSend">The item ID of the item you want to send.</param>
    /// <param name="quantity">How many of the item you want to include.</param>
    /// <param name="immediate">
    ///     Normally, the player will recieve this letter upon waking up the following day. Set this to
    ///     true to make it immediately arrive.
    /// </param>
    /// <param name="NPC">The ID of the NPC you want to have sent the letter. 1 = John</param>
    public static void SendItemInMail(int itemToSend, int quantity, bool immediate = false, int NPC = 1) {
        if (itemToSend < 0 || itemToSend >= Inventory.inv.allItems.Length) return;
        var letter = new Letter(NPC, Letter.LetterType.AnimalTrapReturn, itemToSend, quantity);
        if (immediate)
            MailManager.manage.mailInBox.Add(letter);
        else
            MailManager.manage.tomorrowsLetters.Add(letter);
    }

    /// <summary>Sends the player a letter containing the given item.</summary>
    /// <param name="itemToSend">The item ID of the item you want to send.</param>
    /// <param name="quantity">How many of the item you want to include.</param>
    /// <param name="immediate">
    ///     Normally, the player will recieve this letter upon waking up the following day. Set this to
    ///     true to make it immediately arrive.
    /// </param>
    /// <param name="NPC">The ID of the NPC you want to have sent the letter. 1 = John</param>
    public static void SendItemInMail(InventoryItem itemToSend, int quantity, bool immediate = false, int NPC = 1) {
        if (itemToSend == null) return;
        var letter = new Letter(NPC, Letter.LetterType.AnimalTrapReturn, itemToSend.getItemId(), quantity);
        if (immediate)
            MailManager.manage.mailInBox.Add(letter);
        else
            MailManager.manage.tomorrowsLetters.Add(letter);
    }
}
