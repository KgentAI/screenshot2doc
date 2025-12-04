using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SETUNA.Main.AI.Utils
{
    /// <summary>
    /// Helper class for image processing operations
    /// </summary>
    public static class ImageHelper
    {
        private const int MaxWidth = 1920;
        private const int MaxHeight = 1080;
        private const long JpegQuality = 85L;

        /// <summary>
        /// Compresses an image to JPEG format with optional resizing
        /// </summary>
        /// <param name="original">Original image</param>
        /// <returns>Compressed JPEG bytes</returns>
        public static byte[] CompressImage(Image original)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            // Calculate scale factor
            var scaleWidth = (double)MaxWidth / original.Width;
            var scaleHeight = (double)MaxHeight / original.Height;
            var scale = Math.Min(scaleWidth, scaleHeight);

            // No resize needed if image is within bounds
            if (scale >= 1.0)
            {
                return EncodeAsJpeg(original, JpegQuality);
            }

            // Resize proportionally
            var newWidth = (int)(original.Width * scale);
            var newHeight = (int)(original.Height * scale);

            using (var resized = new Bitmap(newWidth, newHeight))
            {
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(original, 0, 0, newWidth, newHeight);
                }

                return EncodeAsJpeg(resized, JpegQuality);
            }
        }

        /// <summary>
        /// Encodes an image as JPEG with specified quality
        /// </summary>
        private static byte[] EncodeAsJpeg(Image image, long quality)
        {
            using (var ms = new MemoryStream())
            {
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                image.Save(ms, encoder, encoderParameters);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Gets the image encoder for the specified format
        /// </summary>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts image bytes to base64 data URI
        /// </summary>
        /// <param name="imageBytes">Image bytes (JPEG format)</param>
        /// <returns>Base64 data URI string</returns>
        public static string ToBase64DataUri(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be null or empty", nameof(imageBytes));

            var base64 = Convert.ToBase64String(imageBytes);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
