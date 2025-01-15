namespace MongoWebGallery.Models
{
    public static class Question
    {
        public static readonly string[] Questions = new[]
        {
            "Czy powyższe dzieło może uchodzić za przygotowane przez człowieka?",
            "Czy miałeś wcześniej do czynienia z podobnymi dziełami?",
            "Czy uważasz, że jest sens używać sztucznej inteligencji do generowania podobnych dzieł?",
            "Czy wygenerowane dzieło jest twoim zdaniem zgodne z opisem/prompterem?",
            "Jak oceniasz dzieło w skali od 1 do 5?"
        };

        public static readonly int QuestionNumber = 4;
    }
}
