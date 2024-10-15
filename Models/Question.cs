
namespace WebGalleryProject.Models;

public class Question
{
    public static readonly string[] Questions = new[]
            {
            "Czy powyzsze dzielo moze uchodzic za przygotowane przez czlowieka?",
            "Czy miales wczesniej do czynienia z dzielem podobnym?",
            "Czy uwazasz ze jest sens uzywac sztucznej inteligencji do generowania podobnych dziel?",
            "Czy wygenerowane dzielo jest twoim zdaniem zgodne z instrukcja?",
            "Jak oceniasz dzieło w skali od 1 do 5?"
        };

    public static readonly int RatingQuestionIndex = 4; // Indeks pytania o ocenę
}
