using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runnymede.Common.Utils
{
  public  static class RecordingUtils
    {

        public static async Task<int> GetMp3DurationMsec(string blobName)
        {
            // Read the blob and try to determine the duration directly.
            var outputBlob = AzureStorageUtils.GetBlob(AzureStorageUtils.ContainerNames.Recordings, blobName);
            using (var stream = new MemoryStream())
            {
                await outputBlob.DownloadToStreamAsync(stream);
                return GetMp3DurationMsec(stream); // Seeks the stream to the beginning internally.
            }
        }

        public static int GetMp3DurationMsec(Stream stream)
        {
            double durationSec = 0.0;

            var version = MpegVersion.Reserved;
            var layer = MpegLayer.Reserved;
            var mode = ChannelMode.DualChannel;

            try
            {
                if (stream.Position != 0)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                var frame = Mp3Frame.LoadFromStream(stream);

                if (frame != null)
                {
                    version = frame.MpegVersion;
                    layer = frame.MpegLayer;
                    mode = frame.ChannelMode;
                }

                while (frame != null)
                {
                    // A user can rename a PNG file and try to upload it :) We expect a stable bit pattern in all the frames of a MP3 file.
                    if (frame.MpegVersion != version || frame.MpegLayer != layer || frame.ChannelMode != mode)
                    {
                        return 0;
                    }

                    // 1.0 needed to avoid integer arithmetic and preserve fractions. 
                    durationSec += 1.0 * frame.SampleCount / frame.SampleRate /*(frame.ChannelMode == ChannelMode.Mono ? 1.0 : 1.0)*/;

                    frame = Mp3Frame.LoadFromStream(stream);
                }
            }
            catch (EndOfStreamException)
            {
                // Media players tolerate abruptly terminated MP3 files.
            }

            return Convert.ToInt32(durationSec * 1000);
        }


    }
}
