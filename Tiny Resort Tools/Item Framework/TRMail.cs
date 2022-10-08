namespace TinyResort {

    // TODO: Create LetterType template for Mods
    // TODO: Create more robust system for developers to send user mail
    // TODO: Look at ShowLetter function
    public class TRMail {

        public static void SendMailImmediately(TRCustomItem itemToSend, int quantity, int NPC = 1) => 
            MailManager.manage.mailInBox.Add(new Letter(NPC, Letter.LetterType.AnimalTrapReturn, itemToSend.invItem.getItemId(), quantity));

        public static void SendMail(TRCustomItem itemToSend, int quantity, int NPC = 1) => 
            MailManager.manage.tomorrowsLetters.Add(new Letter(NPC, Letter.LetterType.AnimalTrapReturn, itemToSend.invItem.getItemId(), quantity));
        

        
    }

}
