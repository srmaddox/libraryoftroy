using OneOf;

using StableSwarmWrapper;

namespace SwarmTroyGen;

class Program {
    static async Task Main ( string [] args ) {
        try {
            using ( var client = new StableSwarmClient ( "http://localhost:7801" ) ) {
                await client.InitializeSessionAsync ( );

                OneOf<List<IndexedModel>, Exception> modelResult = await client.GetIndexedModelsAsync ( );

                if ( modelResult.TryPickT0 ( out var models, out Exception eInner ) ) throw eInner;

                foreach ( var model in models ) {
                    Console.WriteLine ( $"Model: [{model.Index}]\t{model.Name}" );
                }
            }
        } catch ( Exception e ) {
            Console.WriteLine ( e );
            return;
        }
    }

    static async Task gen2 ( ) {
        try {
            using ( var client = new StableSwarmClient ( "http://localhost:7801" ) ) {
                await client.InitializeSessionAsync ( );

                Console.WriteLine ( "Generating..." );

                string prompt = "A large, dark gas cloud obscurring the sun over a meadow.";

                var imagePathsResult = await client.GenerateImageWithModelIndexAsync(prompt, 0);

                if ( !imagePathsResult.TryPickT0 ( out List<string> imagePaths, out Exception remainder ) ) {
                    Console.WriteLine ( $"Image Generation Failed:\n{remainder}" );
                    return;
                }

                Console.WriteLine ( $"Generated {imagePaths.Count} image(s)" );
                for ( int i = 0; i < imagePaths.Count; i++ ) {
                    string outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"generated_image_{i}.png");
                    Console.WriteLine ( $"Saving image to {outputPath}" );
                    await client.SaveImageAsync ( imagePaths [ i ], outputPath );
                }
            }
        } catch ( Exception ex ) {
            Console.WriteLine ( ex );
            return;
        }
    }

    /*
    static async Task gen1 ( ) {
        // Create the client with default URL (change if your server is hosted elsewhere)
        using ( var client = new StableSwarmClient ( "http://localhost:7801" ) ) {
            try {
                // Initialize a session
                Console.WriteLine ( "Initializing session..." );
                await client.InitializeSessionAsync ( );

                // Simple image generation with a prompt
                Console.WriteLine ( "Generating image..." );
                string prompt = "a beautiful landscape with mountains and a lake, photorealistic, 8k";
                var imagePaths = await client.GenerateImageAsync(
                        prompt: prompt,
                        negativePrompt: "blurry, low quality, distortion",
                        count: 1,
                        model: "OfficialStableDiffusion/sd_xl_base_1.0",
                        width: 1024,
                        height: 1024,
                        steps: 30,
                        seed: -1 // random seed
                    );

                // Save the generated image(s)
                Console.WriteLine ( $"Generated {imagePaths.Count} image(s)" );
                for ( int i = 0; i < imagePaths.Count; i++ ) {
                    string outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"generated_image_{i}.png");
                    Console.WriteLine ( $"Saving image to {outputPath}" );
                    await client.SaveImageAsync ( imagePaths [ i ], outputPath );
                }

                // Get available model categories
                Console.WriteLine ( "Getting available model categories..." );
                var categories = await client.GetModelCategoriesAsync();
                Console.WriteLine ( "Available model categories:" );
                foreach ( var category in categories ) {
                    Console.WriteLine ( $"- {category}" );
                }

                // List available models with indices for Stable-Diffusion
                Console.WriteLine ( "\nListing available Stable Diffusion models:" );
                var indexedModels = await client.GetIndexedModelsAsync("Stable-Diffusion");
                foreach ( var model in indexedModels ) {
                    Console.WriteLine ( $"[{model.Index}] {model.Name}" );
                }

                // Generate an image using a model by index
                if ( indexedModels.Count > 0 ) {
                    // Use the first model (index 0) for this example
                    int selectedModelIndex = 0;
                    Console.WriteLine ( $"\nGenerating image with model index {selectedModelIndex} ({indexedModels [ selectedModelIndex ].Name})..." );

                    var modelIndexImagePaths = await client.GenerateImageWithModelIndexAsync(
                            prompt: "a futuristic cityscape at night with neon lights",
                            modelIndex: selectedModelIndex,
                            categoryName: "Stable-Diffusion",
                            width: 768,
                            height: 768,
                            steps: 25
                        );

                    // Save the generated image
                    if ( modelIndexImagePaths.Count ( ) > 0 ) {
                        string indexModelOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "model_index_image.png");
                        Console.WriteLine ( $"Saving model index image to {indexModelOutputPath}" );
                        await client.SaveImageAsync ( modelIndexImagePaths [ 0 ], indexModelOutputPath );
                    }
                }

                // Example of using custom parameters
                Console.WriteLine ( "\nGenerating image with custom parameters..." );
                var customParams = new Dictionary<string, object>
                    {
                    ["prompt"] = "a portrait of a cat wearing a space suit, digital art",
                    ["negativeprompt"] = "blurry, low quality",
                    ["model"] = "OfficialStableDiffusion/sd_xl_base_1.0",
                    ["width"] = 768,
                    ["height"] = 768,
                    ["cfgscale"] = 8.0,
                    ["steps"] = 25,
                    ["seed"] = 12345 // fixed seed for reproducibility
                };

                var customImagePaths = await client.GenerateWithCustomParamsAsync(customParams);

                // Save the custom generated image
                if ( customImagePaths.Count > 0 ) {
                    string customOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "custom_image.png");
                    Console.WriteLine ( $"Saving custom image to {customOutputPath}" );
                    await client.SaveImageAsync ( customImagePaths [ 0 ], customOutputPath );
                }

                // List recent images
                Console.WriteLine ( "Listing recent images..." );
                var imageList = await client.ListImagesAsync();
                Console.WriteLine ( $"Found {imageList.Files.Count} images in history" );

                // Display image paths
                foreach ( var image in imageList.Files ) {
                    Console.WriteLine ( $"Image: {image.Path}" );
                }
            } catch ( SessionInvalidException ex ) {
                Console.WriteLine ( $"Session error: {ex.Message}" );
            } catch ( ApiException ex ) {
                Console.WriteLine ( $"API error ({ex.ErrorId}): {ex.Message}" );
            } catch ( Exception ex ) {
                Console.WriteLine ( $"Unexpected error: {ex.Message}" );
            }
        }

        Console.WriteLine ( "Press any key to exit..." );
        Console.ReadKey ( );
    }
    */
}
