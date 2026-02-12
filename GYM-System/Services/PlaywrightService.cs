using Microsoft.Playwright;
using GYM_System.ViewModels;

namespace GYM_System.Services
{
    /// <summary>
    /// Playwright-based PDF generation service.
    /// Uses headless Chromium via Microsoft.Playwright to convert HTML (rendered from Razor views) to PDF.
    /// This implementation embeds images and fonts as Base64 for portability.
    /// </summary>
    public class PlaywrightService : IPdfService
    {
        private static readonly SemaphoreSlim _browserLock = new(1, 1);
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;

        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly string _savedPlansPath;
        private readonly string _logoPath;
        private readonly string _placeholderLogoPath;
        private readonly string _fontsPath;

        // Cached font Base64 strings (loaded once)
        private readonly Lazy<string> _tajawalRegularBase64;
        private readonly Lazy<string> _tajawalMediumBase64;
        private readonly Lazy<string> _tajawalBoldBase64;

        public PlaywrightService(
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

        /// <summary>
        /// Generates a Diet Plan PDF using DinkToPdf.
        /// </summary>
        public byte[] GenerateDietPlanPdf(DietPlanViewModel dietPlan)
        {
            // Prepare the view model with Base64 images and fonts for portability
            var pdfModel = PrepareDietPlanForPdf(dietPlan);

            // Render the Razor view to HTML
            var htmlContent = _razorRenderer.RenderViewToStringAsync("~/Views/Pdf/DietPlan.cshtml", pdfModel).GetAwaiter().GetResult();

            // Convert HTML to PDF
            return ConvertHtmlToPdf(htmlContent, $"{dietPlan.Client?.Name ?? "Client"} - Diet Plan");
        }

        /// <summary>
        /// Generates a Workout Plan PDF using DinkToPdf.
        /// </summary>
        public byte[] GenerateWorkoutPlanPdf(WorkoutPlanViewModel workoutPlan)
        {
            // Prepare the view model with Base64 images and fonts for portability
            var pdfModel = PrepareWorkoutPlanForPdf(workoutPlan);

            // Render the Razor view to HTML
            var htmlContent = _razorRenderer.RenderViewToStringAsync("~/Views/Pdf/WorkoutPlan.cshtml", pdfModel).GetAwaiter().GetResult();

            // Convert HTML to PDF
            return ConvertHtmlToPdf(htmlContent, $"{workoutPlan.Client?.Name ?? "Client"} - Workout Plan");
        }

        /// <summary>
        /// Saves a diet plan PDF to disk.
        /// </summary>
        public string SaveDietPlanPdf(byte[] pdfBytes, string planName)
        {
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_DietPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            File.WriteAllBytes(filePath, pdfBytes);
            return filePath;
        }

        /// <summary>
        /// Saves a workout plan PDF to disk.
        /// </summary>
        public string SaveWorkoutPlanPdf(byte[] pdfBytes, string planName)
        {
            string safePlanName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safePlanName}_WorkoutPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(_savedPlansPath, fileName);

            File.WriteAllBytes(filePath, pdfBytes);
            return filePath;
        }

        /// <summary>
        /// Converts HTML content to PDF bytes using DinkToPdf.
        /// </summary>
        private byte[] ConvertHtmlToPdf(string htmlContent, string documentTitle)
        {
            // Playwright APIs are async; keep the public interface sync for compatibility.
            return ConvertHtmlToPdfAsync(htmlContent, documentTitle).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Converts HTML content to PDF bytes using DinkToPdf (async version).
        /// </summary>
        private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent, string documentTitle)
        {
            await EnsureBrowserAsync();

            var browser = _browser!;
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                Locale = "en-US"
            });

            try
            {
                var page = await context.NewPageAsync();

                // Ensure proper encoding + allow embedded base64 assets.
                await page.SetContentAsync(htmlContent, new PageSetContentOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

                return await page.PdfAsync(new PagePdfOptions
                {
                    PrintBackground = true,
                    Format = "A4",
                    DisplayHeaderFooter = false,
                    Margin = new Margin
                    {
                        Top = "10mm",
                        Bottom = "15mm",
                        Left = "10mm",
                        Right = "10mm"
                    }
                });
            }
            finally
            {
                await context.CloseAsync();
            }
        }

        /// <summary>
        /// Ensures that the Playwright browser is available (lazy initialization).
        /// </summary>
        private static async Task EnsureBrowserAsync()
        {
            if (_browser is not null)
                return;

            await _browserLock.WaitAsync();
            try
            {
                if (_browser is not null)
                    return;

                _playwright ??= await Playwright.CreateAsync();

                var baseDir = AppContext.BaseDirectory;
                var chromiumExePath = Path.Combine(baseDir, "ms-playwright", "chromium-1208", "chrome-win", "chrome.exe");

                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    ExecutablePath = File.Exists(chromiumExePath) ? chromiumExePath : null
                });
            }
            finally
            {
                _browserLock.Release();
            }
        }

        /// <summary>
        /// Prepares the diet plan view model for PDF generation by converting images to Base64.
        /// </summary>
        private DietPlanPdfViewModel PrepareDietPlanForPdf(DietPlanViewModel dietPlan)
        {
            var pdfModel = new DietPlanPdfViewModel
            {
                ClientName = dietPlan.Client?.Name ?? "Client",
                PlanName = dietPlan.PlanName,
                GeneralNotes = dietPlan.GeneralNotes,
                CreatedDate = dietPlan.CreatedDate,
                LogoBase64 = GetLogoBase64(),
                // Add font Base64 strings
                TajawalRegularBase64 = TajawalRegularBase64,
                TajawalMediumBase64 = TajawalMediumBase64,
                TajawalBoldBase64 = TajawalBoldBase64
            };

            // Get active versions only
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

        /// <summary>
        /// Prepares the workout plan view model for PDF generation by converting images to Base64.
        /// </summary>
        private WorkoutPlanPdfViewModel PrepareWorkoutPlanForPdf(WorkoutPlanViewModel workoutPlan)
        {
            var pdfModel = new WorkoutPlanPdfViewModel
            {
                ClientName = workoutPlan.Client?.Name ?? "Client",
                PlanName = workoutPlan.PlanName ?? "??? ???????",
                GeneralNotes = workoutPlan.GeneralNotes,
                CreatedDate = workoutPlan.CreatedDate,
                LogoBase64 = GetLogoBase64(),
                TotalDays = workoutPlan.WorkoutDays.Count,
                TotalExercises = workoutPlan.WorkoutDays.Sum(d => d.WorkoutExercises.Count),
                // Add font Base64 strings
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
                        ExerciseName = we.Exercise?.Name ?? "??? ????",
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

        /// <summary>
        /// Gets the logo image as a Base64 encoded string.
        /// </summary>
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

        /// <summary>
        /// Gets a food item image as a Base64 encoded string.
        /// </summary>
        private string GetFoodItemImageBase64(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return string.Empty;
            }

            // Remove leading slash if present and combine with wwwroot
            string relativePath = imagePath.StartsWith("/") ? imagePath.Substring(1) : imagePath;
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

        /// <summary>
        /// Gets a font file as a Base64 encoded data URI string.
        /// </summary>
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

    #region PDF-Specific ViewModels (for Base64 image and font embedding)

    /// <summary>
    /// ViewModel specifically for Diet Plan PDF generation with Base64 images and fonts.
    /// </summary>
    public class DietPlanPdfViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoBase64 { get; set; } = string.Empty;
        public bool ShowVersionHeaders { get; set; }
        public List<DietPlanVersionPdfViewModel> Versions { get; set; } = new();

        // Font Base64 strings for embedding in HTML
        public string TajawalRegularBase64 { get; set; } = string.Empty;
        public string TajawalMediumBase64 { get; set; } = string.Empty;
        public string TajawalBoldBase64 { get; set; } = string.Empty;
    }

    public class DietPlanVersionPdfViewModel
    {
        public string VersionName { get; set; } = string.Empty;
        public string? VersionNotes { get; set; }
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }
        public List<MealPdfViewModel> Meals { get; set; } = new();
    }

    public class MealPdfViewModel
    {
        public string MealName { get; set; } = string.Empty;
        public string? MealNotes { get; set; }
        public decimal TotalCalories { get; set; }
        public decimal TotalProtein { get; set; }
        public decimal TotalCarbs { get; set; }
        public decimal TotalFat { get; set; }
        public List<MealFoodItemPdfViewModel> FoodItems { get; set; } = new();
    }

    public class MealFoodItemPdfViewModel
    {
        public string FoodItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string ImageBase64 { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel specifically for Workout Plan PDF generation with Base64 images and fonts.
    /// </summary>
    public class WorkoutPlanPdfViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoBase64 { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int TotalExercises { get; set; }
        public List<WorkoutDayPdfViewModel> WorkoutDays { get; set; } = new();

        // Font Base64 strings for embedding in HTML
        public string TajawalRegularBase64 { get; set; } = string.Empty;
        public string TajawalMediumBase64 { get; set; } = string.Empty;
        public string TajawalBoldBase64 { get; set; } = string.Empty;
    }

    public class WorkoutDayPdfViewModel
    {
        public string DayName { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? DayNotes { get; set; }
        public int ExerciseCount { get; set; }
        public List<WorkoutExercisePdfViewModel> Exercises { get; set; } = new();
    }

    public class WorkoutExercisePdfViewModel
    {
        public string ExerciseName { get; set; } = string.Empty;
        public string Sets { get; set; } = "-";
        public string Reps { get; set; } = "-";
        public string Rest { get; set; } = "-";
        public string Tempo { get; set; } = "-";
        public string RpeRir { get; set; } = "-";
        public string? ExerciseNotes { get; set; }
        public string? YouTubeLink { get; set; }
    }

    #endregion
}
