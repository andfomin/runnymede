using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Utils.NAudio
{
    public static class Mp3FileHelper
    {
        private static int GetMp3DurationMsec(string fileName)
        {
            double duration = 0.0;
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var frame = Mp3Frame.LoadFromStream(stream);
                    while (frame != null)
                    {
                        // 1.0 needed to avoid integer arithmetic and preserve fractions. 
                        duration += 1.0 * frame.SampleCount / frame.SampleRate /*(frame.ChannelMode == ChannelMode.Mono ? 1.0 : 1.0)*/;

                        frame = Mp3Frame.LoadFromStream(stream);
                    }
                }
            }
            catch (EndOfStreamException)
            {
            }
            catch (Exception)
            {
                throw;
            }
            return Convert.ToInt32(duration * 1000);
        }
    }
}