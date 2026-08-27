using FarmApp.BusinessLogicLayer.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class ImageSharpProcessor : IImageProcessor
    {
        public async Task CreateThumbnailAsync(Stream input,Stream output, int size)
        {
            input.Position = 0;

            using var image = await Image.LoadAsync(input);

            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(size, size),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                });

                x.GaussianSharpen(0.6f);
            });

            await image.SaveAsJpegAsync(output, new JpegEncoder
            {
                Quality = 80
            });

            output.Position = 0;
        }
    }
}
