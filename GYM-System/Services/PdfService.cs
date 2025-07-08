using Microsoft.AspNetCore.Hosting; // To get wwwroot path for logo
using Microsoft.Extensions.Configuration; // To get SavedPlansPath from config
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GYM_System.ViewModels;
using System.IO;
using System.Linq;

namespace GYM_System.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string _savedPlansPath;
        private readonly string _logoPath;

        public PdfService(IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;

            // Get the path where PDFs will be saved from appsettings.json
            _savedPlansPath = Path.Combine(_hostEnvironment.ContentRootPath, _configuration["AppSettings:SavedPlansFolder"] ?? "SavedPlans");
            if (!Directory.Exists(_savedPlansPath))
            {
                Directory.CreateDirectory(_savedPlansPath);
            }

            // Path to your gym logo (relative to wwwroot)
            _logoPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logo.png");
        }

        public byte[] GenerateDietPlanPdf(DietPlanViewModel dietPlan)
        {
            // Register fonts if needed (e.g., for Arabic characters or custom fonts)
            // FontManager.RegisterSystemFonts(); // Registers all system fonts
            // FontManager.RegisterFont(File.OpenRead("path/to/your/font.ttf"));

            // Ensure QuestPDF is licensed (if using commercial features)
            // License.LicenseKey = LicenseKey.Community; // Or your commercial key

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Inter")); // Using Inter font

                    page.Header()
                        .PaddingBottom(15)
                        .Column(column =>
                        {
                            
                            column.Item().Row(row =>
                            {
                                row.ConstantItem(100).Element(container =>
                                {
                                    container.Image(File.Exists(_logoPath) ? _logoPath : "https://placehold.co/100x50/cccccc/333333?text=Logo")
                                        .FitWidth();
                                });

                                row.RelativeItem()
                                    .AlignRight()
                                    .Text("Super Sheets Gym Management")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                            });

                            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("Diet Plan: ").SemiBold().FontSize(14);
                                text.Span(dietPlan.PlanName).FontSize(14);
                            });

                            if (dietPlan.Client != null)
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Client: ").SemiBold();
                                    text.Span(dietPlan.Client.Name);
                                });
                            }

                            if (!string.IsNullOrEmpty(dietPlan.GeneralNotes))
                            {
                                column.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("General Notes: ").SemiBold();
                                    text.Span(dietPlan.GeneralNotes);
                                });
                            }

                            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            // Render each active version
                            foreach (var version in dietPlan.Versions.Where(v => v.IsActiveForPdf))
                            {
                                column.Item().PaddingTop(15).PageBreak(); // Start new version on a new page or add space
                                column.Item().Text(text =>
                                {
                                    text.Span("Version: ").SemiBold().FontSize(12);
                                    text.Span(version.VersionName).FontSize(12).FontColor(Colors.Blue.Medium);
                                });

                                if (!string.IsNullOrEmpty(version.VersionNotes))
                                {
                                    column.Item().PaddingTop(3).Text(text =>
                                    {
                                        text.Span("Notes: ").SemiBold();
                                        text.Span(version.VersionNotes);
                                    });
                                }

                                column.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                                // Render each meal in the version
                                foreach (var meal in version.Meals)
                                {
                                    column.Item().PaddingVertical(10).Column(mealColumn =>
                                    {
                                        mealColumn.Item().Text(text =>
                                        {
                                            text.Span("Meal: ").SemiBold().FontSize(11);
                                            text.Span(meal.MealName).FontSize(11).FontColor(Colors.Green.Darken1);
                                        });

                                        if (!string.IsNullOrEmpty(meal.MealNotes))
                                        {
                                            mealColumn.Item().PaddingTop(2).Text(text =>
                                            {
                                                text.Span("Notes: ").SemiBold().FontSize(9);
                                                text.Span(meal.MealNotes).FontSize(9);
                                            });
                                        }

                                        // Meal Food Items Table
                                        mealColumn.Item().PaddingTop(5).Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(3); // Food Item Name
                                                columns.RelativeColumn(1); // Quantity
                                                columns.RelativeColumn(1); // Calories
                                                columns.RelativeColumn(1); // Protein
                                                columns.RelativeColumn(1); // Carbs
                                                columns.RelativeColumn(1); // Fat
                                            });

                                            table.Header(header =>
                                            {
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Food Item").SemiBold();
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Qty").SemiBold();
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Cal").SemiBold();
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Prot").SemiBold();
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Carb").SemiBold();
                                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Fat").SemiBold();
                                            });

                                            foreach (var mfi in meal.MealFoodItems)
                                            {
                                                table.Cell().PaddingVertical(2).Text($"{mfi.FoodItem?.Name} ({mfi.FoodItem?.Unit})");
                                                table.Cell().PaddingVertical(2).Text(mfi.Quantity.ToString("F1"));
                                                table.Cell().PaddingVertical(2).Text(mfi.Calories.ToString("F1"));
                                                table.Cell().PaddingVertical(2).Text(mfi.Protein.ToString("F1"));
                                                table.Cell().PaddingVertical(2).Text(mfi.Carbs.ToString("F1"));
                                                table.Cell().PaddingVertical(2).Text(mfi.Fat.ToString("F1"));
                                            }

                                            // Meal Totals Row
                                            table.Cell().ColumnSpan(2).AlignRight().PaddingVertical(3).Text("Meal Totals:").SemiBold();
                                            table.Cell().PaddingVertical(3).Text(meal.TotalCalories.ToString("F1")).SemiBold();
                                            table.Cell().PaddingVertical(3).Text(meal.TotalProtein.ToString("F1")).SemiBold();
                                            table.Cell().PaddingVertical(3).Text(meal.TotalCarbs.ToString("F1")).SemiBold();
                                            table.Cell().PaddingVertical(3).Text(meal.TotalFat.ToString("F1")).SemiBold();
                                        });
                                    });
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf(); // Generates the PDF as a byte array
        }

        public string SaveDietPlanPdf(byte[] pdfBytes, string planName)
        {
            // Sanitize plan name for file system
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            File.WriteAllBytes(filePath, pdfBytes);
            return filePath; // Return the full path where the file was saved
        }
    }
}