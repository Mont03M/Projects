using System.IO;
using System.Text.Json;

namespace ChessGame.Services
{
    /// <summary>
    /// Represents a service for managing application settings, including loading and saving settings to a JSON file.
    /// </summary>
    public class AppSettingsService
    {
        private readonly string _path;

        public ISettingsService Settings { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the AppSettingsService class, determining the path to the settings.json file based on the project directory structure.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public AppSettingsService()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null &&
                   !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            if (directory == null)
                throw new DirectoryNotFoundException(
                    "Could not locate project folder.");


            _path = Path.Combine(directory.FullName, "settings.json");
        }

        /// <summary>
        /// Asynchronously loads the application settings from the settings.json file, if it exists. 
        /// If the file does not exist, the method returns without modifying the current settings.
        /// </summary>
        /// <returns>A task that represents the asynchronous load operation.</returns>
        public async Task LoadAsync()
        {
            if (!File.Exists(_path))
                return;

            var json = await File.ReadAllTextAsync(_path);

            Settings = JsonSerializer.Deserialize<ISettingsService>(json) ?? new ISettingsService();
        }

        /// <summary>
        /// Asynchronously saves the current application settings to the settings.json file, creating or overwriting the file as necessary.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(Settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            await File.WriteAllTextAsync(_path, json);
        }
    }
}
