using System.ComponentModel.DataAnnotations;

namespace GYM_System.Models
{
    public class SpreadSheet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Google Sheet ID is required")]
        [RegularExpression(@"^[a-zA-Z0-9-_]+$", ErrorMessage = "Invalid Google Sheet ID format")]
        public string SheetId { get; set; }

        [Required(ErrorMessage = "Update Sheet Range is required")]
        [RegularExpression(@"^[^!]+![A-Z]+:[A-Z]+$", ErrorMessage = "Format must be: SheetName!A:Z")]
        public string? InitialAssessmentSheetNameAndRange { get; set; }

        [Required(ErrorMessage = "Update Sheet Range is required")]
        [RegularExpression(@"^[^!]+![A-Z]+:[A-Z]+$", ErrorMessage = "Format must be: SheetName!A:Z")]
        public string? UpdateFormSheetNameAndRange { get; set; }
    }
}
