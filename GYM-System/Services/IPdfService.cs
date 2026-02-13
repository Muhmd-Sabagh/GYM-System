using GYM_System.ViewModels;

namespace GYM_System.Services
{
    /// <summary>
    /// Interface for PDF generation services.
    /// Allows swapping between different PDF generation implementations (QuestPDF, wkhtmltopdf, etc.)
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generates a PDF for a diet plan.
        /// </summary>
        /// <param name="dietPlan">The diet plan view model containing all plan data.</param>
        /// <returns>PDF file as byte array.</returns>
        Task<byte[]> GenerateDietPlanPdfAsync(DietPlanViewModel dietPlan);

        /// <summary>
        /// Generates a PDF for a workout plan.
        /// </summary>
        /// <param name="workoutPlan">The workout plan view model containing all plan data.</param>
        /// <returns>PDF file as byte array.</returns>
        Task<byte[]> GenerateWorkoutPlanPdfAsync(WorkoutPlanViewModel workoutPlan);

        /// <summary>
        /// Saves a diet plan PDF to the configured storage location.
        /// </summary>
        /// <param name="pdfBytes">The PDF file bytes.</param>
        /// <param name="planName">The plan name for the file name.</param>
        /// <returns>The full file path where the PDF was saved.</returns>
        Task<string> SaveDietPlanPdfAsync(byte[] pdfBytes, string planName);

        /// <summary>
        /// Saves a workout plan PDF to the configured storage location.
        /// </summary>
        /// <param name="pdfBytes">The PDF file bytes.</param>
        /// <param name="planName">The plan name for the file name.</param>
        /// <returns>The full file path where the PDF was saved.</returns>
        Task<string> SaveWorkoutPlanPdfAsync(byte[] pdfBytes, string planName);
    }
}
