using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Security.Cryptography;
using System.Numerics;

namespace Runnymede.Common.Utils
{
    public static class KeyUtils
    {
        private static long SequenceCounter = 0;
        //private const string Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Base36        
        //private const string Base32Chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // Base32. Exclude 0, O, 1, I to avoid visual ambiguity. Use capital letters only to allow for case-insensitive search.
        private const string Base32Digits = "23456789abcdefghijkmnpqrstuvwxyz"; // Base32 lower case. Exclude 0, O, 1, l to avoid visual ambiguity. Use capital letters only to allow for case-insensitive search.

        //private static int[] LocalTimeLowerLimits = new int[] { 2014, 1, 1, 0, 0, 0 };
        //private static int[] LocalTimeUpperLimits = new int[] { 2099, 12, 31, 23, 59, 59 };

        /// <summary>
        /// 32 digits: 00000000000000000000000000000000
        /// </summary>
        /// <returns></returns>
        public static string GetGuidKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Current time plus uniquifier. Length 32 chars. "2014-11-09T03:22:59.6080696Z0001"
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentTimeKey()
        {
            Interlocked.Increment(ref SequenceCounter);
            return DateTime.UtcNow.ToString("o") + (SequenceCounter % 10000).ToString("d4"); // If required, the number is pre-padded with zeros.        
        }

        /// <summary>
        /// Twelve Base32 characters. Eight chars are sequential over time, resolution upto millisecond. Four chars are random.
        /// </summary>
        /// <param name="sequential">Choose the order of the sequential and random parts. We may want to avoid sequenced partition keys and distribute load in Azure Tables randomly.</param>
        /// <returns></returns>
        public static string GetTwelveBase32Digits(bool sequential = false)
        {
            int radix = Base32Digits.Length;
            char[] charArray = { '2', '2', '2', '2', '2', '2', '2', '2', '2', '2', '2', '2' }; // Number is pre-padded with zeros (wich are "2" in our case) at the beginning.

            // Four chars are random.
            int random = Math.Abs(Guid.NewGuid().GetHashCode());

            int index = sequential ? 11 : 3;
            int toIndex = index - 3;
            while (index >= toIndex)
            {
                int remainder = (int)(random % radix);
                charArray[index--] = Base32Digits[remainder];
                random = random / radix;
            }

            DateTime now = DateTime.UtcNow;

            // Eight first chars are milliseconds since the origin.	Should be enough for 34 years.
            DateTime origin = new DateTime(2014, 10, 10, 10, 10, 10, DateTimeKind.Utc); // Arbitrary origin. 
            TimeSpan interval = now - origin;
            //long milliseconds = Convert.ToInt64(interval.TotalMilliseconds); // Possible precision loss? Double precision is 15 digits. It's more than 30000 years.
            long milliseconds = interval.Ticks / TimeSpan.TicksPerMillisecond; // One tick is 100 nanoseconds. There are 10,000 ticks in a millisecond. 

            index = sequential ? 7 : 11;
            toIndex = index - 7;
            while (index >= toIndex)
            {
                int remainder = (int)(milliseconds % radix);
                charArray[index--] = Base32Digits[remainder];
                milliseconds = milliseconds / radix;
            }

            return new string(charArray);
        }

        /// <summary>
        /// Eight Base32 characters which are sequential over time, resolution upto millisecond.
        /// </summary>
        /// <returns></returns>
        public static string GetTimeAsBase32()
        {
            /* Eight chars are milliseconds since the origin. Should be enough for 34 years. Then the value will be still last eight digits and the higher digits will be not present.
             * The chance of a collision is one millisecond in 34 years multiplyed by the number of generated values (if there is no another uniquefier).
             */
            DateTime now = DateTime.UtcNow;
            DateTime origin = new DateTime(2014, 10, 10, 10, 10, 10, DateTimeKind.Utc); // Arbitrary origin. 
            TimeSpan interval = now - origin;
            //long milliseconds = Convert.ToInt64(interval.TotalMilliseconds); // Possible precision loss? Double precision is 15 digits. It's more than 30000 years.
            long milliseconds = interval.Ticks / TimeSpan.TicksPerMillisecond; // One tick is 100 nanoseconds. There are 10,000 ticks in a millisecond. 
            char[] charArray = { '2', '2', '2', '2', '2', '2', '2', '2', '2', '2' }; // Number is pre-padded with zeros (wich are "2" in our case) at the beginning.
            int radix = Base32Digits.Length;
            int index = charArray.Length - 1;
            while (index >= 0)
            {
                int remainder = (int)(milliseconds % radix);
                charArray[index--] = Base32Digits[remainder];
                milliseconds = milliseconds / radix;
            }
            return new string(charArray);
        }

        /// <summary>
        /// Geenrates a random number in which every digit is a random Base32 character.
        /// </summary>
        /// <param name="digits">The length of the number</param>
        /// <param name="randoms">A sequence generated by Random is determined by the seed. If we need not a trully random number, but a determined and repeatable one based on some contents, seed Random with the contents hash, i.e. new Random(hash), and pass it to the function</param>
        /// <returns></returns>
        ////public static string GetBase32Number(int digits, Random[] randoms = null)
        ////{
        ////    /* Random() uses an int32 seed. There may be 2^32 unique sequenses produced, so we can expect a collision once a year :).
        ////     * The range of 8-digit Base32 number is larger than the range of Int32. We use each Random to generate four Base32 digits.
        ////    */
        ////    if (randoms == null)
        ////    {
        ////        // Random() uses an int32 seed based on the curent value of timer ticks.
        ////        randoms = new[] { new Random() };
        ////    }

        ////    var extraRandomsNeeded = (digits + 3) / 4 - randoms.Length;
        ////    if (extraRandomsNeeded > 0)
        ////    {
        ////        // Ticks discretion is 100 nsec, so two Random() instances instantiated without a delay produce the same sequence. We cannot more use the parameterless Random().
        ////        var extraRandoms = Enumerable
        ////                     .Repeat(new Random(Guid.NewGuid().GetHashCode()), extraRandomsNeeded)
        ////                     ;
        ////        randoms = randoms.Concat(extraRandoms).ToArray();
        ////    }

        ////    return new string(
        ////                 Enumerable
        ////                 .Repeat(Base32Chars, digits)
        ////                 .Select((s, i) => s[randoms[i / 4].Next(s.Length)])
        ////                 .ToArray()
        ////                 );
        ////}

        ////private string ConvertToSixDigitBase36(int number)
        ////{
        ////    const int radix = 36;
        ////    char[] charArray = { '0', '0', '0', '0', '0', '0' }; // The number is padded with zeros at the beginning.
        ////    int index = 5;
        ////    while (number != 0 && index > 0)
        ////    {
        ////        int remainder = (int)(number % radix);
        ////        charArray[index--] = Base36Chars[remainder];
        ////        number = number / radix;
        ////    }
        ////    return new String(charArray);
        ////}

        /// <summary>
        /// Converts GUID to a string, non-enclosed 32 lower-case digits separated by 4 hyphens. Azure data storage URLs are case-sensitive.
        /// Although this method is not actually needed, it is keept as a placeholder for comments to stress the fact that textual Guid representation must be lower-case in all contexts.
        /// </summary>
        /// <param name="value">extId of the user/entity</param>
        /// <returns>string</returns>
        public static string GuidToString(Guid value)
        {
            /*
             * RFC 4122 A Universally Unique IDentifier (UUID) URN Namespace. +http://www.ietf.org/rfc/rfc4122.txt
             * Section 3: The hexadecimal values "a" through "f" are output as lower case characters and are case insensitive on input.
             * 
             * Recommendation ITU-T X.667 : Information technology - Procedures for the operation of object identifier registration authorities: Generation of universally unique identifiers and their use in object identifiers
             * +http://www.itu.int/rec/T-REC-X.667-201210-I/en
             * 6.5.4 Software generating the hexadecimal representation of a UUID shall not use upper case letters.
             * NOTE – It is recommended that the hexadecimal representation used in all human-readable formats be restricted to lower-case letters. 
             * Software processing this representation is, however, required to accept both upper and lower case letters as specified in 6.5.2.
             */
            return value.ToString(); // lower-case        
        }

        public static Guid ComputeHash(string value)
        {
            var bytesToHash = System.Text.Encoding.UTF8.GetBytes(value);
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytesToHash);
            return new Guid(hash);
        }

        /// <summary>
        /// Computes the MD5 hash of the paremeter and represents it as a Base32-encoded string. The length of the result string is 26 chars.
        /// </summary>
        /// <param name="value">Text to hash</param>
        /// <returns>string</returns>
        public static string ComputeHashBase32(string value)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(textBytes);
            return new RadixEncoding(Base32Digits, RadixEncoding.EndianFormat.Little, true).Encode(hashBytes);
        }

        /// <summary>
        /// 10 digits. 0000000001. Prepended with zeroes. Corresponds to app.intToKey()
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns> 
        public static string IntToKey(int value)
        {
            return value.ToString("d10");
        }

        public static int KeyToInt(string value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 19 digits. Counts down from "3155378975999999999" (DateTime.MaxValue.Ticks - time.Ticks). Use it for a RowKey in Azure Table to order last records first for retrieval.
        /// </summary>
        /// <param name="value">DateTimeKind must be Utc</param>
        /// <returns></returns>
        public static string DateTimeToDescKey(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTimeKind must be Utc");
            }
            return (DateTime.MaxValue.Ticks - value.Ticks).ToString("d19");
        }

        /// <summary>
        /// Inverse to TimeToDescKey(). Converts a RowKey value to the original DateTime value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime DescKeyToDateTime(string value)
        {
            return new DateTime(DateTime.MaxValue.Ticks - Convert.ToInt64(value), DateTimeKind.Utc);
        }

        /// <summary>
        /// 12 digits. "2014/1/1/0/0/0" corresponds to "851130235959", "2099/12/31/23/59/59" to "000000000000". Use it for RowKey in Azure Table to order last records first for retrieval.
        /// </summary>
        /// <param name="value">Comes from client as "2014/1/2/3/4/5". See app.getLocalTimeInfo().</param>
        /// <returns></returns>
        public static string LocalTimeToInvertedKey(string value)
        {
            string result = null;
            // We get the localTime value from the wild web. Be cautious.
            if (!String.IsNullOrWhiteSpace(value))
            {
                var chunks = value.Split('/');
                if (chunks.Count() == 6)
                {
                    var parts = chunks.Select(i =>
                    {
                        int part;
                        if (!Int32.TryParse(i, out part))
                        {
                            part = -1;
                        }
                        return part;
                    })
                    .ToArray();

                    var valid = parts
                    .Select((i, idx) => ((i >= TimeUtils.LocalTimeLowerLimits[idx]) && (i <= TimeUtils.LocalTimeUpperLimits[idx])))
                    .All(i => i);

                    if (valid)
                    {
                        var invertedParts = parts
                        .Select((i, idx) => { return (TimeUtils.LocalTimeUpperLimits[idx] - i).ToString("d2"); });
                        result = String.Join("", invertedParts);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Opposite to LocalTimeToInvertedKey(). "851130235959" corresponds to "2014/1/1/0/0/0", "000000000000" to "2099/12/31/23/59/59".
        /// </summary>
        /// <param name="value"></param>
        /// <param name="partCount"></param>
        /// <param name="separator"></param>
        /// <param name="partFormat"></param>
        /// <returns></returns>
        public static string InvertedKeyToLocalTime(string value, int partCount = 6, string separator = "/", string partFormat = null)
        {
            string result = null;
            var chunkSize = 2;
            if (!String.IsNullOrWhiteSpace(value) && (value.Length >= chunkSize * partCount))
            {
                var parts = Enumerable.Range(0, partCount)
                .Select(i =>
                {
                    var chunk = value.Substring(i * chunkSize, chunkSize);
                    int inverted;
                    var part = Int32.TryParse(chunk, out inverted)
                        ? TimeUtils.LocalTimeUpperLimits[i] - inverted
                        : -1;
                    return part;
                })
                .ToArray();

                var valid = parts
                    .Select((i, idx) => { return (i >= TimeUtils.LocalTimeLowerLimits[idx]) && (i <= TimeUtils.LocalTimeUpperLimits[idx]); })
                    .All(i => i);

                if (valid)
                {
                    var chunks = parts.Select(i => i.ToString(partFormat));
                    result = String.Join(separator, chunks);
                }
            }
            return result;
        }

    }

    #region RadixEncoding
    // Based on +http://stackoverflow.com/questions/14110010/base-n-encoding-of-a-byte-array/
    /// <summary>Encodes/decodes bytes to/from a string</summary>
    /// <remarks>
    /// Encoded string is always in big-endian ordering
    /// 
    /// <p>Encode and Decode take a <b>includeProceedingZeros</b> parameter which acts as a work-around
    /// for an edge case with our BigInteger implementation.
    /// MSDN says BigInteger byte arrays are in LSB->MSB ordering. So a byte buffer with zeros at the 
    /// end will have those zeros ignored in the resulting encoded radix string.
    /// If such a loss in precision absolutely cannot occur pass true to <b>includeProceedingZeros</b>
    /// and for a tiny bit of extra processing it will handle the padding of zero digits (encoding)
    /// or bytes (decoding).</p>
    /// <p>Note: doing this for decoding <b>may</b> add an extra byte more than what was originally 
    /// given to Encode.</p>
    /// </remarks>
    public class RadixEncoding
    {
        public enum EndianFormat
        {
            /// <summary>Least Significant Bit order (lsb)</summary>
            /// <remarks>Right-to-Left</remarks>
            /// <see cref="BitConverter.IsLittleEndian"/>
            Little,
            /// <summary>Most Significant Bit order (msb)</summary>
            /// <remarks>Left-to-Right</remarks>
            Big,
        };

        const int kByteBitCount = 8;

        readonly string kDigits;
        readonly double kBitsPerDigit;
        readonly BigInteger kRadixBig;
        readonly EndianFormat kEndian;
        readonly bool kIncludeProceedingZeros;

        /// <summary>Numerial base of this encoding</summary>
        public int Radix { get { return kDigits.Length; } }
        /// <summary>Endian ordering of bytes input to Encode and output by Decode</summary>
        public EndianFormat Endian { get { return kEndian; } }
        /// <summary>True if we want ending zero bytes to be encoded</summary>
        public bool IncludeProceedingZeros { get { return kIncludeProceedingZeros; } }

        public override string ToString()
        {
            return string.Format("Base-{0} {1}", Radix.ToString(), kDigits);
        }

        /// <summary>Create a radix encoder using the given characters as the digits in the radix</summary>
        /// <param name="digits">Digits to use for the radix-encoded string</param>
        /// <param name="bytesEndian">Endian ordering of bytes input to Encode and output by Decode</param>
        /// <param name="includeProceedingZeros">True if we want ending zero bytes to be encoded</param>
        public RadixEncoding(string digits,
            EndianFormat bytesEndian = EndianFormat.Little, bool includeProceedingZeros = false)
        {
            //Contract.Requires<ArgumentNullException>(digits != null);
            int radix = digits.Length;

            kDigits = digits;
            kBitsPerDigit = System.Math.Log(radix, 2);
            kRadixBig = new BigInteger(radix);
            kEndian = bytesEndian;
            kIncludeProceedingZeros = includeProceedingZeros;
        }

        // Number of characters needed for encoding the specified number of bytes
        int EncodingCharsCount(int bytesLength)
        {
            return (int)Math.Ceiling((bytesLength * kByteBitCount) / kBitsPerDigit);
        }
        // Number of bytes needed to decoding the specified number of characters
        int DecodingBytesCount(int charsCount)
        {
            return (int)Math.Ceiling((charsCount * kBitsPerDigit) / kByteBitCount);
        }

        /// <summary>Encode a byte array into a radix-encoded string</summary>
        /// <param name="bytes">byte array to encode</param>
        /// <returns>The bytes in encoded into a radix-encoded string</returns>
        /// <remarks>If <paramref name="bytes"/> is zero length, returns an empty string</remarks>
        public string Encode(byte[] bytes)
        {
            //Contract.Requires<ArgumentNullException>(bytes != null);
            //Contract.Ensures(Contract.Result<string>() != null);

            // Don't really have to do this, our code will build this result (empty string),
            // but why not catch the condition before doing work?
            if (bytes.Length == 0) return string.Empty;

            // if the array ends with zeros, having the capacity set to this will help us know how much
            // 'padding' we will need to add
            int result_length = EncodingCharsCount(bytes.Length);
            // List<> has a(n in-place) Reverse method. StringBuilder doesn't. That's why.
            var result = new List<char>(result_length);

            // HACK: BigInteger uses the last byte as the 'sign' byte. If that byte's MSB is set, 
            // we need to pad the input with an extra 0 (ie, make it positive)
            if ((bytes[bytes.Length - 1] & 0x80) == 0x80)
                Array.Resize(ref bytes, bytes.Length + 1);

            var dividend = new BigInteger(bytes);
            // IsZero's computation is less complex than evaluating "dividend > 0"
            // which invokes BigInteger.CompareTo(BigInteger)
            while (!dividend.IsZero)
            {
                BigInteger remainder;
                dividend = BigInteger.DivRem(dividend, kRadixBig, out remainder);
                int digit_index = System.Math.Abs((int)remainder);
                result.Add(kDigits[digit_index]);
            }

            if (kIncludeProceedingZeros)
                for (int x = result.Count; x < result.Capacity; x++)
                    result.Add(kDigits[0]); // pad with the character that represents 'zero'

            // orientate the characters in big-endian ordering
            if (kEndian == EndianFormat.Little)
                result.Reverse();
            // If we didn't end up adding padding, ToArray will end up returning a TrimExcess'd array, 
            // so nothing wasted
            return new string(result.ToArray());
        }

        void DecodeImplPadResult(ref byte[] result, int padCount)
        {
            if (padCount > 0)
            {
                int new_length = result.Length + DecodingBytesCount(padCount);
                Array.Resize(ref result, new_length); // new bytes will be zero, just the way we want it
            }
        }
        #region Decode (Little Endian)
        byte[] DecodeImpl(string chars, int startIndex = 0)
        {
            var bi = new BigInteger();
            for (int x = startIndex; x < chars.Length; x++)
            {
                int i = kDigits.IndexOf(chars[x]);
                if (i < 0) return null; // invalid character
                bi *= kRadixBig;
                bi += i;
            }

            return bi.ToByteArray();
        }
        byte[] DecodeImplWithPadding(string chars)
        {
            int pad_count = 0;
            for (int x = 0; x < chars.Length; x++, pad_count++)
                if (chars[x] != kDigits[0]) break;

            var result = DecodeImpl(chars, pad_count);
            DecodeImplPadResult(ref result, pad_count);

            return result;
        }
        #endregion
        #region Decode (Big Endian)
        byte[] DecodeImplReversed(string chars, int startIndex = 0)
        {
            var bi = new BigInteger();
            for (int x = (chars.Length - 1) - startIndex; x >= 0; x--)
            {
                int i = kDigits.IndexOf(chars[x]);
                if (i < 0) return null; // invalid character
                bi *= kRadixBig;
                bi += i;
            }

            return bi.ToByteArray();
        }
        byte[] DecodeImplReversedWithPadding(string chars)
        {
            int pad_count = 0;
            for (int x = chars.Length - 1; x >= 0; x--, pad_count++)
                if (chars[x] != kDigits[0]) break;

            var result = DecodeImplReversed(chars, pad_count);
            DecodeImplPadResult(ref result, pad_count);

            return result;
        }
        #endregion
        /// <summary>Decode a radix-encoded string into a byte array</summary>
        /// <param name="radixChars">radix string</param>
        /// <returns>The decoded bytes, or null if an invalid character is encountered</returns>
        /// <remarks>
        /// If <paramref name="radixChars"/> is an empty string, returns a zero length array
        /// 
        /// Using <paramref name="IncludeProceedingZeros"/> has the potential to return a buffer with an
        /// additional zero byte that wasn't in the input. So a 4 byte buffer was encoded, this could end up
        /// returning a 5 byte buffer, with the extra byte being null.
        /// </remarks>
        public byte[] Decode(string radixChars)
        {
            //Contract.Requires<ArgumentNullException>(radixChars != null);

            if (kEndian == EndianFormat.Big)
                return kIncludeProceedingZeros ? DecodeImplReversedWithPadding(radixChars) : DecodeImplReversed(radixChars);
            else
                return kIncludeProceedingZeros ? DecodeImplWithPadding(radixChars) : DecodeImpl(radixChars);
        }
    };

    #endregion

    // Originally +https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs
    // Found on +http://alexandrebrisebois.wordpress.com/2013/11/14/create-predictable-guids-for-your-windows-azure-table-storage-entities/
    /// <summary>
    /// Hashes an URI to a deterministic GUID
    /// </summary>
    ////public static class HashUriToGuid  // IdentityProvider
    ////{   

    ////    public static Guid MakeGuid(Uri uri)
    ////    {
    ////        return MakeGuid(UrlNamespace, uri.AbsoluteUri, 5);
    ////    }

    ////    /// <summary>
    ////    /// Creates a name-based UUID using the algorithm from RFC 4122 4.3.
    ////    /// </summary>
    ////    /// <param name="namespaceId">The ID of the namespace.</param>
    ////    /// <param name="name">The name (within that namespace).</param>
    ////    /// <param name="version">The version number of the UUID to create; this value must be either
    ////    /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
    ////    /// <returns>A UUID derived from the namespace and name.</returns>
    ////    /// <remarks>
    ////    /// See "Generating a deterministic GUID" +http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html
    ////    /// </remarks>
    ////    private static Guid MakeGuid(Guid namespaceId, string name, int version)
    ////    {
    ////        if (name == null)
    ////            throw new ArgumentNullException("name");
    ////        if (version != 3 && version != 5)
    ////            throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");

    ////        // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3) ASSUME: UTF-8 encoding is always appropriate
    ////        byte[] nameBytes = Encoding.UTF8.GetBytes(name);

    ////        // convert the namespace UUID to network order (step 3)
    ////        byte[] namespaceBytes = namespaceId.ToByteArray();
    ////        SwapByteOrder(namespaceBytes);

    ////        // comput the hash of the name space ID concatenated with the name (step 4)
    ////        byte[] hash;
    ////        using (HashAlgorithm algorithm = version == 3 ? (HashAlgorithm)MD5.Create() : SHA1.Create())
    ////        {
    ////            algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
    ////            algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
    ////            hash = algorithm.Hash;
    ////        }

    ////        // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
    ////        byte[] newGuid = new byte[16];
    ////        Array.Copy(hash, 0, newGuid, 0, 16);

    ////        // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
    ////        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

    ////        // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
    ////        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

    ////        // convert the resulting UUID to local byte order (step 13)
    ////        SwapByteOrder(newGuid);
    ////        return new Guid(newGuid);
    ////    }

    ////    /// <summary>
    ////    /// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
    ////    /// </summary>
    ////    private static readonly Guid DnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    ////    /// <summary>
    ////    /// The namespace for URLs (from RFC 4122, Appendix C).
    ////    /// </summary>
    ////    private static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    ////    /// <summary>
    ////    /// The namespace for ISO OIDs (from RFC 4122, Appendix C).
    ////    /// </summary>
    ////    private static readonly Guid IsoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

    ////    // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
    ////    private static void SwapByteOrder(byte[] guid)
    ////    {
    ////        SwapBytes(guid, 0, 3);
    ////        SwapBytes(guid, 1, 2);
    ////        SwapBytes(guid, 4, 5);
    ////        SwapBytes(guid, 6, 7);
    ////    }

    ////    private static void SwapBytes(byte[] guid, int left, int right)
    ////    {
    ////        byte temp = guid[left];
    ////        guid[left] = guid[right];
    ////        guid[right] = temp;
    ////    }
    ////}




}