namespace TarbikMap.Common
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public static class ImageResizer
    {
        private const int ExifOrientationId = 0x112; // 274

        public static byte[] ResizeIfNeeded(byte[] orig)
        {
            using (var ms_orig = new MemoryStream(orig))
            {
                using (var origImage = Image.FromStream(ms_orig))
                {
                    Rotate(origImage);
                    using (var newImage = ScaleImage(origImage, 1000, 1000))
                    {
                        using (var ms_new = new MemoryStream())
                        {
                            newImage.Save(ms_new, ImageFormat.Jpeg);
                            return ms_new.ToArray();
                        }
                    }
                }
            }
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            if (ratio > 1)
            {
                ratio = 1;
            }

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        // https://stackoverflow.com/a/48347653/7164302
        private static void Rotate(Image img)
        {
            if (!img.PropertyIdList.Contains(ExifOrientationId))
            {
                return;
            }

            var prop = img.GetPropertyItem(ExifOrientationId);
            var val = prop.Value.Where(b => b != 0).FirstOrDefault();

            var rot = RotateFlipType.RotateNoneFlipNone;

            if (val == 3 || val == 4)
            {
                rot = RotateFlipType.Rotate180FlipNone;
            }
            else if (val == 5 || val == 6)
            {
                rot = RotateFlipType.Rotate90FlipNone;
            }
            else if (val == 7 || val == 8)
            {
                rot = RotateFlipType.Rotate270FlipNone;
            }

            if (val == 2 || val == 4 || val == 5 || val == 7)
            {
                rot |= RotateFlipType.RotateNoneFlipX;
            }

            if (rot != RotateFlipType.RotateNoneFlipNone)
            {
                img.RotateFlip(rot);
            }
        }
    }
}