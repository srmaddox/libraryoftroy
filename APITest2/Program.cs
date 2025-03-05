using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System.Diagnostics;
using StableSwarmWrapper;

using Newtonsoft.Json.Linq;

namespace APITest2;
class Program {
    static async Task Main ( string [] args ) {
        Console.WriteLine ( "StableSwarm Image Generator" );
        Console.WriteLine ( "==========================" );

        try {
            // Initialize client with a 10-minute timeout
            using var client = new StableSwarmClient(timeout: TimeSpan.FromMinutes(10));

            // Initialize session and models
            Console.WriteLine ( "Initializing client..." );
            var initResult = await client.InitializeAsync();
            if ( !initResult.IsT0 ) {
                throw initResult.AsT1;
            }

            Console.WriteLine ( "Connection established successfully!" );

            // Display available models
            await DisplayAvailableModels ( client );

            // Prompt user for model selection
            await SelectModel ( client );

            // Generate image
            await GenerateImage ( client );
        } catch ( Exception ex ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine ( $"Error: {ex.Message}" );
            Console.ResetColor ( );

            if ( ex.InnerException != null ) {
                Console.WriteLine ( $"Details: {ex.InnerException.Message}" );
            }
        }

        Console.WriteLine ( "\nPress any key to exit..." );
        Console.ReadKey ( );
    }

    static async Task DisplayAvailableModels ( StableSwarmClient client ) {
        Console.WriteLine ( "\nAvailable Model Categories:" );

        foreach ( var category in client.AvailableModels.Categories ) {
            Console.WriteLine ( $"  {category.Key} ({category.Value.Count} models)" );

            // Show first few models in each category
            int count = Math.Min(3, category.Value.Count);
            for ( int i = 0; i < count; i++ ) {
                Console.WriteLine ( $"    [{i}] {Path.GetFileName ( category.Value [ i ] )}" );
            }

            if ( category.Value.Count > count ) {
                Console.WriteLine ( $"    ... and {category.Value.Count - count} more" );
            }
        }

        // Show current model
        if ( !string.IsNullOrEmpty ( client.CurrentModel ) ) {
            Console.WriteLine ( $"\nCurrent model: {client.CurrentModel}" );
        }
    }

    static async Task SelectModel ( StableSwarmClient client ) {
        Console.WriteLine ( "\nSelect a model:" );
        Console.WriteLine ( "1. Use current model" );
        Console.WriteLine ( "2. Select from Stable-Diffusion models" );
        Console.WriteLine ( "3. Select from all models" );

        string choice = GetUserInput("Enter your choice (1-3): ", "1");

        switch ( choice ) {
            case "1":
            if ( string.IsNullOrEmpty ( client.CurrentModel ) ) {
                Console.WriteLine ( "No current model selected. Choosing the first available model." );
                var setResult = client.SetModelByIndex(0);
                if ( !setResult.IsT0 ) {
                    throw setResult.AsT1;
                }
            }
            break;

            case "2":
            await SelectModelFromCategory ( client, "Stable-Diffusion" );
            break;

            case "3":
            await SelectModelFromAllCategories ( client );
            break;

            default:
            Console.WriteLine ( "Invalid choice. Using current model." );
            break;
        }

        Console.WriteLine ( $"Using model: {client.CurrentModel}" );
    }

    static async Task SelectModelFromCategory ( StableSwarmClient client, string category ) {
        if ( !client.AvailableModels.Categories.TryGetValue ( category, out var models ) ) {
            Console.WriteLine ( $"Category {category} not found. Using current model." );
            return;
        }

        Console.WriteLine ( $"\nAvailable {category} models:" );
        for ( int i = 0; i < models.Count; i++ ) {
            Console.WriteLine ( $"  [{i}] {Path.GetFileName ( models [ i ] )}" );
        }

        string indexStr = GetUserInput($"Enter model index (0-{models.Count - 1}): ", "0");
        if ( !int.TryParse ( indexStr, out int index ) || index < 0 || index >= models.Count ) {
            Console.WriteLine ( "Invalid index. Using the first model." );
            index = 0;
        }

        var setResult = client.SetModelByIndex(index, category);
        if ( !setResult.IsT0 ) {
            throw setResult.AsT1;
        }
    }

    static async Task SelectModelFromAllCategories ( StableSwarmClient client ) {
        List<IndexedModel> allModels = client.AvailableModels.GetIndexedModels();

        Console.WriteLine ( "\nAll available models:" );
        foreach ( var model in allModels ) {
            Console.WriteLine ( $"  [{model.Index}] {model.Category}/{model.Name}" );
        }

        string indexStr = GetUserInput($"Enter model index (0-{allModels.Count - 1}): ", "0");
        if ( !int.TryParse ( indexStr, out int index ) || index < 0 || index >= allModels.Count ) {
            Console.WriteLine ( "Invalid index. Using the first model." );
            index = 0;
        }

        var setResult = client.SetCurrentModel(allModels[index].FullPath);
        if ( !setResult.IsT0 ) {
            throw setResult.AsT1;
        }
    }

    static async Task GenerateImage ( StableSwarmClient client ) {
        // Get user prompt
        string prompt = GetUserInput("\nEnter image prompt: ", "a beautiful landscape with mountains and a lake");
        string negativePrompt = GetUserInput("Enter negative prompt (optional): ", "blurry, low quality");

        // Get image settings
        string widthStr = GetUserInput("Width (512-1024): ", "1024");
        int width = int.TryParse(widthStr, out int w) ? w : 1024;

        string heightStr = GetUserInput("Height (512-1024): ", "1024");
        int height = int.TryParse(heightStr, out int h) ? h : 1024;

        string stepsStr = GetUserInput("Steps (10-50): ", "25");
        int steps = int.TryParse(stepsStr, out int s) ? s : 25;

        string seedStr = GetUserInput("Seed (-1 for random): ", "-1");
        int seed = int.TryParse(seedStr, out int sd) ? sd : -1;

        // Generate the image
        Console.WriteLine ( $"\nGenerating image with model {client.CurrentModel}..." );
        Console.WriteLine ( $"This may take several minutes depending on the model..." );

        var startTime = DateTime.Now;
        var imageResult = await client.GenerateImageAsync(
            prompt,
            negativePrompt,
            1,
            null, // Use current model
            width,
            height,
            steps,
            seed
        );

        if ( !imageResult.IsT0 ) {
            throw imageResult.AsT1;
        }

        var imagePaths = imageResult.AsT0;
        var elapsed = DateTime.Now - startTime;

        Console.WriteLine ( $"Image generated in {elapsed.TotalSeconds:F1} seconds!" );

        if ( imagePaths.Count == 0 ) {
            Console.WriteLine ( "No images were returned." );
            return;
        }

        // Save and display image
        string imagePath = imagePaths[0];
        Console.WriteLine ( $"Image path on server: {imagePath}" );

        // Create a directory for saved images
        string outputDir = Path.Combine(Environment.CurrentDirectory, "generated_images");
        Directory.CreateDirectory ( outputDir );

        // Generate a filename based on timestamp and prompt
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string shortPrompt = prompt.Length > 30 ? prompt.Substring(0, 30).Replace(' ', '_') : prompt.Replace(' ', '_');
        string fileName = $"{timestamp}_{shortPrompt}.png";
        string localPath = Path.Combine(outputDir, fileName);

        // Download and save the image
        Console.WriteLine ( $"Downloading image..." );
        var saveResult = await client.SaveImageAsync(imagePath, localPath);

        if ( !saveResult.IsT0 ) {
            throw saveResult.AsT1;
        }

        Console.WriteLine ( $"Image saved to: {localPath}" );

        // Try to display the image
        try {
            Console.WriteLine ( "Opening image with default viewer..." );
            Process.Start ( new ProcessStartInfo {
                FileName = localPath,
                UseShellExecute = true
            } );
        } catch ( Exception ex ) {
            Console.WriteLine ( $"Could not open image automatically: {ex.Message}" );
        }
    }

    static string GetUserInput ( string prompt, string defaultValue ) {
        Console.Write ( prompt );
        string input = Console.ReadLine();
        return string.IsNullOrWhiteSpace ( input ) ? defaultValue : input;
    }
}