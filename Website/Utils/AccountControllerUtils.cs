using Microsoft.AspNet.Identity;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Runnymede.Website.Utils
{
    public static class AccountControllerUtils
    {
        public const string PayPalUserNameHashSalt = "#Salt7B190B1B03B9#";

        public const int AvatarLargeSize = 120;
        public const int AvatarSmallSize = 36;

        /// <summary>
        /// Protect userName from tampering with when sending form data to PayPal.
        /// </summary>
        public static string GetTimestampedUserName(string userName)
        {
            var timeText = DateTime.UtcNow.ToBinary().ToString("X"); // 16 chars
            var textToHash = userName + timeText + PayPalUserNameHashSalt;
            var bytesToHash = System.Text.Encoding.UTF8.GetBytes(textToHash);
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytesToHash);
            var hashText = new Guid(hash).ToString("N").ToUpper(); // 32 chars
            var text = timeText + hashText;
            var chunkSize = 24;
            // 49 chars = 2 chunks * 24 chars + 1 separating space.
            var chunkedText = string.Join(" ", Enumerable.Range(0, text.Length / chunkSize).Select(i => text.Substring(i * chunkSize, chunkSize)));
            return chunkedText;
        }

        public static int? CalculateTimeOffsetSec(string localTime)
        {
            if (!string.IsNullOrWhiteSpace(localTime))
            {
                // We get the localTime value from the wild web. Be cautious.
                try
                {
                    var parts = localTime.Split('/');
                    int dummy;
                    if (parts.Count() == 6 && parts.All(i => Int32.TryParse(i, out dummy)))
                    {
                        var arr = parts.Select(i => Convert.ToInt32(i)).ToArray();
                        var clientTime = new DateTime(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], DateTimeKind.Utc);
                        var span = clientTime - DateTime.UtcNow;
                        var timeOffsetSec = Convert.ToInt32(span.TotalSeconds);
                        return timeOffsetSec;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        // +http://labs.astrobunny.net/blog/2011/11/22/making-color-from-alpha-hue-saturation-and-brightness/
        private static Color ColorFromAhsb(int a, float h, float s, float b)
        {

            if (0 > a || 255 < a)
            {
                throw new ArgumentOutOfRangeException("a");
            }
            if (0f > h || 360f < h)
            {
                throw new ArgumentOutOfRangeException("h");
            }
            if (0f > s || 1f < s)
            {
                throw new ArgumentOutOfRangeException("s");
            }
            if (0f > b || 1f < b)
            {
                throw new ArgumentOutOfRangeException("b");
            }

            if (0 == s)
            {
                return Color.FromArgb(a, Convert.ToInt32(b * 255),
                    Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b)
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            }
            else
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h)
            {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = h * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        } // end of ColorFromAhsb()

        /// <summary>
        /// Creates a square identicon.
        /// </summary>
        /// <param name="seed">Seed</param>
        /// <param name="stream">Output</param>
        /// <param name="blockSize">The size of blocks in pixels. Total 6 blocks.</param>
        private static void CreateIdenticon(Guid seed, Stream stream, int blockSize) // Total 6 blocks. 20 x 6 = 120; 8 x 6 = 48
        {
            int size = 5; // blocks
            var bitmapSize = (size + 1) * blockSize; // pixels
            var offset = blockSize / 2; // pixels

            byte grayValue = 0xf0;
            Color bgColor = Color.FromArgb(255, grayValue, grayValue, grayValue);

            int hue = Math.Abs(seed.GetHashCode()) % 360;
            var color = ColorFromAhsb(255, hue, 1, 0.4f);

            var byteArr = seed.ToByteArray();
            var arrLength = byteArr.Length;

            using (var bitmap = new System.Drawing.Bitmap(bitmapSize, bitmapSize))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                using (var bgBrush = new SolidBrush(bgColor))
                {
                    graphics.FillRectangle(bgBrush, 0, 0, bitmapSize, bitmapSize);
                }

                using (var brush = new SolidBrush(color))
                {
                    for (var x = 0; x < size / 2 + 1; x++)
                    {
                        for (var y = 0; y < size; y++)
                        {
                            var i = x * size + y;
                            var fill = byteArr[i % 16] < 128; 

                            // We may "humanize" the icon look by sacrificing collisions durability.
                            //if (false)
                            //{
                            //    if ((x == 0) && (y == 0))
                            //        fill = false; // shoulders
                            //    if ((x == size / 2) && (y == 0))
                            //        fill = true; // head
                            //    if ((x == size / 2) && (y == size / 2))
                            //        fill = true; // body
                            //    if ((x == size / 2) && (y == size - 1))
                            //        fill = false; // split feet
                            //}

                            if (fill)
                            {
                                Rectangle rect1 = new Rectangle(offset + x * blockSize, offset + y * blockSize, blockSize, blockSize);
                                graphics.FillRectangle(brush, rect1);
                                // make it horizontally symmetrical
                                Rectangle rect2 = new Rectangle(offset + (size - x - 1) * blockSize, offset + y * blockSize, blockSize, blockSize);
                                graphics.FillRectangle(brush, rect2);
                            }
                        }
                    }
                }

                if (stream != null)
                {
                    bitmap.Save(stream, ImageFormat.Png);
                }
            }
        }

        /// <summary>
        /// A fire-and-forget method because it is async void.
        /// </summary>
        /// <param name="extId">Seed</param>
        /// <param name="blockSize">The size of blocks in pixels. Total 6 blocks.</param>
        /// <param name="containerName"></param>
        public static async Task CreateAndSaveIdenticonAsync(Guid extId, int blockSize, string containerName)
        {
            var blobName = extId.ToString().ToUpper();

            using (var stream = new MemoryStream())
            {
                CreateIdenticon(extId, stream, blockSize);
                await AzureStorageUtils.UploadBlobAsync(stream, containerName, blobName, "image/png");
            }
        }

        public static string PlainErrorMessage(this IdentityResult result, string defaultErrorMessage = null)
        {
            return result.Succeeded
                    ? null
                    : result.Errors != null ? string.Join("\n", result.Errors) : defaultErrorMessage;
        }


    }
}