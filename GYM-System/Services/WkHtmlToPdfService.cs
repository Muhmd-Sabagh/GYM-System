using System.Diagnostics;
using System.Text;
using GYM_System.ViewModels;

namespace GYM_System.Services
{
    /// <summary>
    /// wkhtmltopdf-based PDF generation service.
    /// Uses the wkhtmltopdf executable directly to convert HTML (rendered from Razor views) to PDF.
    /// This implementation embeds images and fonts as Base64 for portability.
    /// </summary>
    public class WkHtmlToPdfService : IPdfService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly string _savedPlansPath;
        private readonly string _logoPath;
        private readonly string _placeholderLogoPath;
        private readonly string _fontsPath;
        private readonly string _wkhtmltopdfExePath;

        // Cached font Base64 strings (loaded once)
        private readonly Lazy<string> _tajawalRegularBase64;
        private readonly Lazy<string> _tajawalMediumBase64;
        private readonly Lazy<string> _tajawalBoldBase64;

        public WkHtmlToPdfService(
            IWebHostEnvironment hostEnvironment,
            IConfiguration configuration,
            IRazorViewToStringRenderer razorRenderer)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _razorRenderer = razorRenderer;

            // Get the path where PDFs will be saved from appsettings.json
            _savedPlansPath = Path.Combine(_hostEnvironment.ContentRootPath, _configuration["AppSettings:SavedPlansFolder"] ?? "SavedPlans");
            if (!Directory.Exists(_savedPlansPath))
            {
                Directory.CreateDirectory(_savedPlansPath);
            }

            // Paths for images (assuming they are in wwwroot/images)
            _logoPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logo", "logo.jpg");
            _placeholderLogoPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "logo", "placeholder_logo.png");

            // Path for fonts
            _fontsPath = Path.Combine(_hostEnvironment.WebRootPath, "fonts", "Tajawal");

            // Lazy load fonts as Base64 (only when needed)
            _tajawalRegularBase64 = new Lazy<string>(() => GetFontBase64("Tajawal-Regular.ttf"));
            _tajawalMediumBase64 = new Lazy<string>(() => GetFontBase64("Tajawal-Medium.ttf"));
            _tajawalBoldBase64 = new Lazy<string>(() => GetFontBase64("Tajawal-Bold.ttf"));

            // Configure wkhtmltopdf path.
            // Prefer appsettings: AppSettings:WkhtmltopdfPath, fallback to PATH.
            var configuredPath = _configuration["AppSettings:WkhtmltopdfPath"];
            _wkhtmltopdfExePath = ResolveWkhtmltopdfPath(configuredPath) ?? "wkhtmltopdf";
        }

        private string? ResolveWkhtmltopdfPath(string? configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
                return null;

            // If config value is already an absolute path and exists, use it.
            if (Path.IsPathFullyQualified(configuredPath) && File.Exists(configuredPath))
                return configuredPath;

            // Treat rooted-but-not-qualified paths as relative to the app content root.
            var trimmed = configuredPath.Trim();

            // Try to make it relative (strip leading slashes) and resolve against content root.
            var relative = trimmed.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var candidate1 = Path.Combine(_hostEnvironment.ContentRootPath, relative);
            if (File.Exists(candidate1))
                return candidate1;

            // Also try output/base directory (e.g., bin/Debug/net9.0/) since the csproj copies PdfTools there.
            var candidate2 = Path.Combine(AppContext.BaseDirectory, relative);
            if (File.Exists(candidate2))
                return candidate2;

            // If nothing exists, return the original value so the error message includes it.
            return configuredPath;
        }

        /// <summary>
        /// Gets the Tajawal Regular font as Base64.
        /// </summary>
        public string TajawalRegularBase64 => _tajawalRegularBase64.Value;

        /// <summary>
        /// Gets the Tajawal Medium font as Base64.
        /// </summary>
        public string TajawalMediumBase64 => _tajawalMediumBase64.Value;

        /// <summary>
        /// Gets the Tajawal Bold font as Base64.
        /// </summary>
        public string TajawalBoldBase64 => _tajawalBoldBase64.Value;

        public async Task<byte[]> GenerateDietPlanPdfAsync(DietPlanViewModel dietPlan)
        {
            var pdfModel = PrepareDietPlanForPdf(dietPlan);
            var htmlContent = await _razorRenderer.RenderViewToStringAsync("~/Views/Pdf/DietPlan.cshtml", pdfModel);
            return await ConvertHtmlToPdfAsync(htmlContent, $"{dietPlan.Client?.Name ?? "Client"} - Diet Plan");
        }

        public async Task<byte[]> GenerateWorkoutPlanPdfAsync(WorkoutPlanViewModel workoutPlan)
        {
            var pdfModel = PrepareWorkoutPlanForPdf(workoutPlan);
            var htmlContent = await _razorRenderer.RenderViewToStringAsync("~/Views/Pdf/WorkoutPlan.cshtml", pdfModel);
            return await ConvertHtmlToPdfAsync(htmlContent, $"{workoutPlan.Client?.Name ?? "Client"} - Workout Plan");
        }

        public async Task<string> SaveDietPlanPdfAsync(byte[] pdfBytes, string planName)
        {
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_DietPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
        }

        public async Task<string> SaveWorkoutPlanPdfAsync(byte[] pdfBytes, string planName)
        {
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_WorkoutPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);
            return filePath;
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent, string documentTitle)
        {
            // Write HTML to a temp file to avoid stdin/encoding quirks.
            var tempDir = Path.Combine(Path.GetTempPath(), "GYM-System", "wkhtmltopdf");
            Directory.CreateDirectory(tempDir);

            var tempHtmlPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}.html");
            var tempPdfPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}.pdf");

            try
            {
                await File.WriteAllTextAsync(tempHtmlPath, htmlContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                // wkhtmltopdf args
                // Using file input/output to make it work consistently on Windows/Linux.
                var args = new StringBuilder();
                args.Append("--encoding utf-8 ");
                args.Append("--print-media-type ");
                args.Append("--enable-local-file-access ");
                args.Append("--margin-top 10mm --margin-bottom 15mm --margin-left 10mm --margin-right 10mm ");
                args.Append("--footer-center \"[page] / [toPage]\" --footer-font-size 9 ");
                args.Append("--title ").Append(Quote(documentTitle)).Append(' ');
                args.Append(Quote(tempHtmlPath)).Append(' ').Append(Quote(tempPdfPath));

                var startInfo = new ProcessStartInfo
                {
                    FileName = _wkhtmltopdfExePath,
                    Arguments = args.ToString(),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };

                try
                {
                    if (!process.Start())
                        throw new InvalidOperationException("Failed to start wkhtmltopdf process.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Unable to start wkhtmltopdf. Configure AppSettings:WkhtmltopdfPath as an absolute path or as a relative path like 'PdfTools/wkhtmltox.exe'. Resolved path: '{_wkhtmltopdfExePath}'.", ex);
                }

                var stdOutTask = process.StandardOutput.ReadToEndAsync();
                var stdErrTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                var stdOut = await stdOutTask;
                var stdErr = await stdErrTask;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"wkhtmltopdf failed with exit code {process.ExitCode}. Output: {stdOut}. Error: {stdErr}");
                }

                if (!File.Exists(tempPdfPath))
                    throw new InvalidOperationException($"wkhtmltopdf did not produce output PDF at '{tempPdfPath}'. Output: {stdOut}. Error: {stdErr}");

                return await File.ReadAllBytesAsync(tempPdfPath);
            }
            finally
            {
                TryDelete(tempHtmlPath);
                TryDelete(tempPdfPath);
            }
        }

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // best-effort cleanup
            }
        }

        private DietPlanPdfViewModel PrepareDietPlanForPdf(DietPlanViewModel dietPlan)
        {
            var pdfModel = new DietPlanPdfViewModel
            {
                ClientName = dietPlan.Client?.Name ?? "Client",
                PlanName = dietPlan.PlanName,
                GeneralNotes = dietPlan.GeneralNotes,
                CreatedDate = dietPlan.CreatedDate,
                LogoBase64 = GetLogoBase64(),
                TajawalRegularBase64 = TajawalRegularBase64,
                TajawalMediumBase64 = TajawalMediumBase64,
                TajawalBoldBase64 = TajawalBoldBase64
            };

            var activeVersions = dietPlan.Versions?.Where(v => v.IsActiveForPdf).ToList() ?? new List<DietPlanVersionViewModel>();

            foreach (var version in activeVersions)
            {
                var versionPdf = new DietPlanVersionPdfViewModel
                {
                    VersionName = version.VersionName,
                    VersionNotes = version.VersionNotes,
                    TotalCalories = version.Meals.Sum(m => m.TotalCalories),
                    TotalProtein = version.Meals.Sum(m => m.TotalProtein),
                    TotalCarbs = version.Meals.Sum(m => m.TotalCarbs),
                    TotalFat = version.Meals.Sum(m => m.TotalFat)
                };

                foreach (var meal in version.Meals)
                {
                    var mealPdf = new MealPdfViewModel
                    {
                        MealName = meal.MealName,
                        MealNotes = meal.MealNotes,
                        TotalCalories = meal.TotalCalories,
                        TotalProtein = meal.TotalProtein,
                        TotalCarbs = meal.TotalCarbs,
                        TotalFat = meal.TotalFat
                    };

                    foreach (var mfi in meal.MealFoodItems)
                    {
                        var foodItemPdf = new MealFoodItemPdfViewModel
                        {
                            FoodItemName = mfi.FoodItem?.Name ?? "Unknown",
                            Unit = mfi.FoodItem?.Unit ?? "unit",
                            Quantity = mfi.Quantity,
                            ImageBase64 = GetFoodItemImageBase64(mfi.FoodItem?.ImagePath)
                        };
                        mealPdf.FoodItems.Add(foodItemPdf);
                    }

                    versionPdf.Meals.Add(mealPdf);
                }

                pdfModel.Versions.Add(versionPdf);
            }

            pdfModel.ShowVersionHeaders = pdfModel.Versions.Count > 1;

            return pdfModel;
        }

        private WorkoutPlanPdfViewModel PrepareWorkoutPlanForPdf(WorkoutPlanViewModel workoutPlan)
        {
            var pdfModel = new WorkoutPlanPdfViewModel
            {
                ClientName = workoutPlan.Client?.Name ?? "Client",
                PlanName = workoutPlan.PlanName ?? "خطة التمرين",
                GeneralNotes = workoutPlan.GeneralNotes,
                CreatedDate = workoutPlan.CreatedDate,
                LogoBase64 = GetLogoBase64(),
                TotalDays = workoutPlan.WorkoutDays.Count,
                TotalExercises = workoutPlan.WorkoutDays.Sum(d => d.WorkoutExercises.Count),
                TajawalRegularBase64 = TajawalRegularBase64,
                TajawalMediumBase64 = TajawalMediumBase64,
                TajawalBoldBase64 = TajawalBoldBase64
            };

            foreach (var day in workoutPlan.WorkoutDays)
            {
                var dayPdf = new WorkoutDayPdfViewModel
                {
                    DayName = day.DayName,
                    Subtitle = day.Subtitle,
                    DayNotes = day.DayNotes,
                    ExerciseCount = day.WorkoutExercises.Count
                };

                foreach (var we in day.WorkoutExercises)
                {
                    var exercisePdf = new WorkoutExercisePdfViewModel
                    {
                        ExerciseName = we.Exercise?.Name ?? "غير متاح",
                        Sets = we.Sets ?? "-",
                        Reps = we.Reps ?? "-",
                        Rest = we.Rest ?? "-",
                        Tempo = we.Tempo ?? "-",
                        RpeRir = we.RpeRir ?? "-",
                        ExerciseNotes = we.ExerciseNotes,
                        YouTubeLink = we.Exercise?.YouTubeLink
                    };
                    dayPdf.Exercises.Add(exercisePdf);
                }

                pdfModel.WorkoutDays.Add(dayPdf);
            }

            return pdfModel;
        }

        private string GetLogoBase64()
        {
            string logoPath = File.Exists(_logoPath) ? _logoPath : _placeholderLogoPath;

            if (!File.Exists(logoPath))
            {
                return string.Empty;
            }

            byte[] imageBytes = File.ReadAllBytes(logoPath);
            string extension = Path.GetExtension(logoPath).ToLowerInvariant();
            string mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };

            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }

        private string GetFoodItemImageBase64(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return string.Empty;
            }

            string relativePath = imagePath.StartsWith('/') ? imagePath[1..] : imagePath;
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                return string.Empty;
            }

            byte[] imageBytes = File.ReadAllBytes(fullPath);
            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            string mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };

            return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
        }

        private string GetFontBase64(string fontFileName)
        {
            string fontPath = Path.Combine(_fontsPath, fontFileName);

            if (!File.Exists(fontPath))
            {
                return string.Empty;
            }

            byte[] fontBytes = File.ReadAllBytes(fontPath);
            return $"data:font/truetype;base64,{Convert.ToBase64String(fontBytes)}";
        }
    }
}
