using GYM_System.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GYM_System.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string _savedPlansPath;
        private readonly string _logoPath;
        private readonly string _placeholderLogoPath;

        public PdfService(IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;

            // --- ADD THIS LINE TO SET THE QUESTPDF LICENSE TYPE ---
            QuestPDF.Settings.License = LicenseType.Community;
            // If your organization's annual gross revenue exceeds $1M USD,
            // you would need a commercial license and would set:
            // QuestPDF.Settings.License = LicenseType.Professional; // Or other commercial types

            // Get the path where PDFs will be saved from appsettings.json
            _savedPlansPath = Path.Combine(_hostEnvironment.ContentRootPath, _configuration["AppSettings:SavedPlansFolder"] ?? "SavedPlans");
            if (!Directory.Exists(_savedPlansPath))
            {
                Directory.CreateDirectory(_savedPlansPath);
            }

            // Paths for images (assuming they are in wwwroot/images)
            _logoPath = Path.Combine(_hostEnvironment.WebRootPath, "images/logo", "logo.jpg");
            _placeholderLogoPath = Path.Combine(_hostEnvironment.WebRootPath, "images/logo", "placeholder_logo.png");

            // Register Inter font for better Arabic rendering if available
            // You might need to place Inter.ttf in a 'fonts' folder within wwwroot
            // Or ensure it's a system font on the server.
            // For example: FontManager.RegisterFont(File.OpenRead(Path.Combine(_hostEnvironment.WebRootPath, "fonts", "Inter-Regular.ttf")));
            // For now, relying on QuestPDF's default font or system fonts if not explicitly registered.
        }

        public byte[] GenerateDietPlanPdf(DietPlanViewModel dietPlan)
        {
            // Calculate overall totals for the entire diet plan
            var allMeals = dietPlan.Versions?.Where(v => v.IsActiveForPdf).SelectMany(v => v.Meals).ToList() ?? new List<MealViewModel>();
            var totalPlanCalories = allMeals.Sum(m => m.TotalCalories);
            var totalPlanProtein = allMeals.Sum(m => m.TotalProtein);
            var totalPlanCarbs = allMeals.Sum(m => m.TotalCarbs);
            var totalPlanFat = allMeals.Sum(m => m.TotalFat);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Inter")); // Using Inter as requested

                    page.Header()
                        .PaddingBottom(7)
                        .Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                // Left side: Logo
                                row.RelativeItem(1)
                                    .Column(clientInfoColumn =>
                                    {
                                        clientInfoColumn.Item().AlignLeft().Element(container =>
                                        {
                                            container.Width(80).Height(80).Image(File.Exists(_logoPath) ? _logoPath : _placeholderLogoPath)
                                                .FitWidth();
                                        });
                                    });

                                // Center: Client Name - Diet Plan
                                row.RelativeItem(4)
                                    .AlignCenter()
                                    .Column(clientInfoColumn =>
                                    {
                                        clientInfoColumn.Item().AlignCenter().Text($"{dietPlan.Client?.Name ?? "Client"} - Diet Plan")
                                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                                    });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        });

                    page.Content()
                        .PaddingVertical(5)
                        .Column(column =>
                        {
                            if (!string.IsNullOrEmpty(dietPlan.GeneralNotes))
                            {
                                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(7).AlignRight().Text(text =>
                                {
                                    text.Span("ملاحظات عامة: ").SemiBold().DirectionFromRightToLeft();
                                    text.Span(dietPlan.GeneralNotes).DirectionFromRightToLeft();
                                });
                            }

                            var activeVersions = dietPlan.Versions.Where(v => v.IsActiveForPdf).ToList();
                            DietPlanVersionViewModel version;

                            // Render each active version
                            for (int i = 0; i < activeVersions.Count; i++)
                            {
                                version = activeVersions[i];

                                if (activeVersions.Count > 1)
                                {
                                    if (i > 0)
                                    {
                                        column.Item().PageBreak(); // Start new version on a new page
                                    }

                                    column.Item().AlignRight().PaddingTop(10).Text(text =>
                                    {
                                        text.Span("النسخة: ").SemiBold().FontSize(12).DirectionFromRightToLeft();
                                        text.Span(version.VersionName).FontSize(12).FontColor(Colors.Blue.Medium).DirectionFromRightToLeft();
                                    });

                                    if (!string.IsNullOrEmpty(version.VersionNotes))
                                    {
                                        column.Item().PaddingTop(3).AlignRight().Text(text =>
                                        {
                                            text.Span("ملاحظات النسخة: ").SemiBold().DirectionFromRightToLeft();
                                            text.Span(version.VersionNotes).DirectionFromRightToLeft();
                                        });
                                    }
                                }

                                // Calculate totals for the current version
                                var totalVersionCalories = version.Meals.Sum(m => m.TotalCalories);
                                var totalVersionProtein = version.Meals.Sum(m => m.TotalProtein);
                                var totalVersionCarbs = version.Meals.Sum(m => m.TotalCarbs);
                                var totalVersionFat = version.Meals.Sum(m => m.TotalFat);

                                // Version Totals Table
                                column.Item().PaddingTop(10).Border(1).BorderColor(Colors.Black.Blue).Padding(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(); // Calories
                                        columns.RelativeColumn(); // Protein
                                        columns.RelativeColumn(); // Carbs
                                        columns.RelativeColumn(); // Fat
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("السعرات").SemiBold().AlignCenter().FontColor(Colors.Blue.Darken1);
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("بروتين").SemiBold().AlignCenter().FontColor(Colors.Red.Darken1);
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("كاربوهيدرات").SemiBold().AlignCenter().FontColor(Colors.Brown.Darken1);
                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("دهون").SemiBold().AlignCenter().FontColor(Colors.Green.Darken1);
                                    });

                                    table.Cell().PaddingVertical(2).Text(totalVersionCalories.ToString("F1")).AlignCenter().FontColor(Colors.Blue.Darken1);
                                    table.Cell().PaddingVertical(2).Text(totalVersionProtein.ToString("F1")).AlignCenter().FontColor(Colors.Red.Darken1);
                                    table.Cell().PaddingVertical(2).Text(totalVersionCarbs.ToString("F1")).AlignCenter().FontColor(Colors.Brown.Darken1);
                                    table.Cell().PaddingVertical(2).Text(totalVersionFat.ToString("F1")).AlignCenter().FontColor(Colors.Green.Darken1);
                                });


                                column.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                                // Render each meal in the version
                                foreach (var meal in version.Meals)
                                {
                                    column.Item().PaddingVertical(7).Column(mealColumn =>
                                    {
                                        // Meal Header Bar
                                        mealColumn.Item().Background(Colors.Blue.Medium).Padding(8).Row(mealHeaderRow =>
                                        {
                                            mealHeaderRow.RelativeItem().AlignLeft().Text($"السعرات: {meal.TotalCalories:F1}").SemiBold().FontSize(11).FontColor(Colors.White);
                                            mealHeaderRow.RelativeItem().AlignRight().Text($"{meal.MealName}").SemiBold().FontSize(11).FontColor(Colors.White);
                                        });

                                        mealColumn.Item().PaddingTop(5).Row(mealContentRow =>
                                        {
                                            // Left Column: Meal Notes and Image Placeholder
                                            mealContentRow.RelativeItem(1)
                                                .Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                                .Column(leftCol =>
                                                {
                                                    leftCol.Item().AlignCenter().Text("ملاحظات").SemiBold().FontSize(10).LineHeight(2);
                                                    leftCol.Item().AlignCenter().Text(meal.MealNotes ?? "لا توجد ملاحظات").FontSize(9);
                                                });

                                            // Right Column: Meal Food Items Table
                                            mealContentRow.RelativeItem(2)
                                                .PaddingLeft(10) // Space between columns
                                                .Table(table =>
                                                {
                                                    table.ColumnsDefinition(columns =>
                                                    {
                                                        columns.RelativeColumn(1); // Unit
                                                        columns.RelativeColumn(1); // Quantity
                                                        columns.RelativeColumn(3); // Food Item Name
                                                        columns.ConstantColumn(60); // Food Image
                                                    });

                                                    table.Header(header =>
                                                    {
                                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("الوحدة").SemiBold().AlignRight();
                                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("الكمية").SemiBold().AlignRight();
                                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("النوع").SemiBold().AlignRight();
                                                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("صورة").SemiBold().AlignRight();
                                                    });

                                                    foreach (var mfi in meal.MealFoodItems)
                                                    {
                                                        table.Cell().PaddingVertical(2).Text(mfi.FoodItem?.Unit.ToString()).AlignRight();
                                                        table.Cell().PaddingVertical(2).Text(mfi.Quantity.ToString("F1")).AlignRight();
                                                        table.Cell().PaddingVertical(2).Text($"{mfi.FoodItem?.Name}").AlignRight();
                                                        table.Cell().PaddingVertical(2).AlignRight().Element(imgContainer =>
                                                        {
                                                            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, mfi.FoodItem.ImagePath.Substring(1));
                                                            imgContainer.Width(20).Height(20).Image(imagePath).FitWidth();
                                                        });
                                                    }
                                                });
                                        });
                                    });
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("صفحة ");
                            x.CurrentPageNumber();
                            x.Span(" من ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateWorkoutPlanPdf(WorkoutPlanViewModel workoutPlan)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Inter")); // Using Inter as requested

                    page.Header()
                        .PaddingBottom(7)
                        .Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                // Left side: Logo
                                row.RelativeItem(1)
                                    .Column(clientInfoColumn =>
                                    {
                                        clientInfoColumn.Item().AlignLeft().Element(container =>
                                        {
                                            container.Width(80).Height(80).Image(File.Exists(_logoPath) ? _logoPath : _placeholderLogoPath)
                                                .FitWidth();
                                        });
                                    });

                                // Center: Client Name - Workout Plan
                                row.RelativeItem(4)
                                    .AlignCenter()
                                    .Column(clientInfoColumn =>
                                    {
                                        clientInfoColumn.Item().AlignCenter().Text($"{workoutPlan.Client?.Name ?? "Client"} - Workout Plan")
                                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                                    });
                            });

                            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        });

                    page.Content()
                        .PaddingVertical(5)
                        .Column(column =>
                        {
                            if (!string.IsNullOrEmpty(workoutPlan.GeneralNotes))
                            {
                                column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(7).AlignRight().Text(text =>
                                {
                                    text.Span("ملاحظات عامة: ").SemiBold().DirectionFromRightToLeft();
                                    text.Span(workoutPlan.GeneralNotes).DirectionFromRightToLeft();
                                });
                            }

                            // Calculate workout plan totals
                            var totalExercises = workoutPlan.WorkoutDays.Sum(d => d.WorkoutExercises.Count());
                            var totalDays = workoutPlan.WorkoutDays.Count();

                            // Plan Totals Table
                            column.Item().PaddingTop(10).Border(1).BorderColor(Colors.Black.Blue).Padding(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Total Days
                                    columns.RelativeColumn(); // Total Exercises
                                    columns.RelativeColumn(); // Plan Name
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("إجمالي الأيام").SemiBold().AlignCenter().FontColor(Colors.Blue.Darken1);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("إجمالي التمارين").SemiBold().AlignCenter().FontColor(Colors.Red.Darken1);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("اسم الخطة").SemiBold().AlignCenter().FontColor(Colors.Green.Darken1);
                                });

                                table.Cell().PaddingVertical(2).Text(totalDays.ToString()).AlignCenter().FontColor(Colors.Blue.Darken1);
                                table.Cell().PaddingVertical(2).Text(totalExercises.ToString()).AlignCenter().FontColor(Colors.Red.Darken1);
                                table.Cell().PaddingVertical(2).Text(workoutPlan.PlanName ?? "خطة التمرين").AlignCenter().FontColor(Colors.Green.Darken1);
                            });

                            column.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                            // Render each workout day
                            foreach (var day in workoutPlan.WorkoutDays)
                            {
                                column.Item().PaddingVertical(7).Column(dayColumn =>
                                {
                                    // Day Header Bar (similar to meal header)
                                    dayColumn.Item().Background(Colors.Blue.Medium).Padding(8).Row(dayHeaderRow =>
                                    {
                                        dayHeaderRow.RelativeItem().AlignLeft().Text($"إجمالي التمارين: {day.WorkoutExercises.Count()}").SemiBold().FontSize(11).FontColor(Colors.White);
                                        dayHeaderRow.RelativeItem().AlignRight().Text($"{day.DayName}").SemiBold().FontSize(11).FontColor(Colors.White);
                                        if (!string.IsNullOrEmpty(day.Subtitle))
                                        {
                                            dayHeaderRow.RelativeItem().AlignRight().Text($" ({day.Subtitle})").FontSize(10).Italic().FontColor(Colors.White);
                                        }
                                    });

                                    dayColumn.Item().PaddingTop(5).Row(dayContentRow =>
                                    {
                                        // Left Column: Day Notes
                                        dayContentRow.RelativeItem(1)
                                            .Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Column(leftCol =>
                                            {
                                                leftCol.Item().AlignCenter().Text("ملاحظات اليوم").SemiBold().FontSize(10).LineHeight(2);
                                                leftCol.Item().AlignCenter().Text(day.DayNotes ?? "لا توجد ملاحظات").FontSize(9);
                                            });

                                        // Right Column: Exercises Table
                                        dayContentRow.RelativeItem(4)
                                            .PaddingLeft(10)
                                            .Table(table =>
                                            {
                                                table.ColumnsDefinition(columns =>
                                                {
                                                    columns.RelativeColumn(1); // Exercise Video Link
                                                    columns.RelativeColumn(1); // RIR
                                                    columns.RelativeColumn(1); // Tempo
                                                    columns.RelativeColumn(1); // Rest
                                                    columns.RelativeColumn(1); // Reps
                                                    columns.RelativeColumn(1); // Sets
                                                    columns.RelativeColumn(2); // Exercise Name
                                                });

                                                table.Header(header =>
                                                {
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("فيديو").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("RIR").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("الإيقاع").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("الراحة").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("التكرارات").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("المجموعات").SemiBold().AlignCenter();
                                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("التمرين").SemiBold().AlignRight();
                                                });

                                                foreach (var workoutExercise in day.WorkoutExercises)
                                                {
                                                    table.Cell().PaddingVertical(3).Hyperlink(workoutExercise.Exercise?.YouTubeLink ?? "_").Text("فيديو").FontSize(10).FontColor(Colors.Blue.Darken1).Underline().AlignCenter();
                                                    table.Cell().PaddingVertical(3).Text(workoutExercise.RpeRir ?? "-").AlignCenter();
                                                    table.Cell().PaddingVertical(3).Text(workoutExercise.Tempo ?? "-").AlignCenter();
                                                    table.Cell().PaddingVertical(3).Text(workoutExercise.Rest ?? "-").AlignCenter();
                                                    table.Cell().PaddingVertical(3).Text(workoutExercise.Reps ?? "-").AlignCenter();
                                                    table.Cell().PaddingVertical(3).Text(workoutExercise.Sets ?? "-").AlignCenter();
                                                    table.Cell().PaddingVertical(3).Column(exerciseCol =>
                                                    {
                                                        exerciseCol.Item().Text($"{workoutExercise.Exercise?.Name ?? "غير متاح"}").AlignRight().DirectionFromRightToLeft();
                                                        if (!string.IsNullOrEmpty(workoutExercise.ExerciseNotes))
                                                        {
                                                            exerciseCol.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(workoutExercise.ExerciseNotes).FontSize(8).FontColor(Colors.Grey.Darken1);
                                                        }
                                                    });
                                                }
                                            });
                                    });
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("صفحة ");
                            x.CurrentPageNumber();
                            x.Span(" من ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public string SaveDietPlanPdf(byte[] pdfBytes, string planName)
        {
            // Sanitize plan name for file system
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_DietPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            File.WriteAllBytes(filePath, pdfBytes);
            return filePath; // Return the full path where the file was saved
        }

        public string SaveWorkoutPlanPdf(byte[] pdfBytes, string planName)
        {
            // Sanitize plan name for file system
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_WorkoutPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            File.WriteAllBytes(filePath, pdfBytes);
            return filePath; // Return the full path where the file was saved
        }
    }
}