using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace URLTesting.SampleWeb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            var app = builder.Build();

            app.UseStaticFiles();
            app.Map("/tile-{id}.jpg", async (int id) =>
            {
                var bytes = await GenerateImage(id);
                return Results.Bytes(bytes, "image/jpeg");
            });

            app.Run();
        }

        [SupportedOSPlatform("windows")]
        private static async Task<byte[]> GenerateImage(int number)
        {
            using var bitmap = new Bitmap(256, 256);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Red);

                using var font = new Font("Arial", 24);
                graphics.DrawString(number.ToString(), font, Brushes.Black, new PointF(10, 10));
            }

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            return memoryStream.ToArray();
        }
    }
}
