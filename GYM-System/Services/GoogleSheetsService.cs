using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GYM_System.Services
{
    public class GoogleSheetsService
    {
        // Path to your client_secret.json downloaded from Google Cloud Console
        // This file should be placed in the root of your ASP.NET Core project
        private static readonly string CredentialsPath = "client_secret.json";

        // The name of the application that will be used to identify the authentication flow.
        private static readonly string ApplicationName = "Gym Management App";

        // Defines the scopes the application needs to access the user's Google Sheets data.
        // SheetsService.Scope.SpreadsheetsReadonly: Allows read-only access to spreadsheets.
        // SheetsService.Scope.Spreadsheets: Allows read/write access to spreadsheets.
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly }; // Start with Readonly

        private SheetsService? _sheetsService;

        public GoogleSheetsService()
        {
            // The SheetsService will be initialized on demand when needed
        }

        // Authenticates and returns a SheetsService instance.
        // This method handles the OAuth 2.0 flow, prompting the user for authorization
        // the first time and caching the refresh token for subsequent uses.
        private async Task<SheetsService> GetSheetsService()
        {
            if (_sheetsService == null)
            {
                UserCredential credential;

                // Load client secrets from client_secret.json file.
                using (var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
                {
                    // The fileDataStore is used to cache the user's credentials
                    // This means the user only has to authenticate once.
                    // The token is stored in a subfolder named 'GYM_System.Auth.Store'
                    // in the user's application data directory.
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user", // User identity (can be any string, but typically 'user')
                        CancellationToken.None,
                        new FileDataStore("GYM_System.Auth.Store") // Directory to store credentials
                    );
                }

                // Create Google Sheets API service.
                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            return _sheetsService;
        }

        // Reads data from a specified Google Sheet and range.
        // The data is returned as a list of lists of objects (rows and columns).
        public async Task<IList<IList<object>>> ReadSheetData(string spreadsheetId, string range)
        {
            var service = await GetSheetsService();
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>>? values = response.Values;

            if (values == null || values.Count == 0)
            {
                // Return an empty list if no data is found
                return new List<IList<object>>();
            }
            return values;
        }
    }
}