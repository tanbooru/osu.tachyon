// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Text;
using Newtonsoft.Json;
using osu.Game.IO.Legacy;
using osu.Game.IO.Serialization;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Scoring.Legacy
{
    public static class LegacyScoreUserRewriter
    {
        public static void Rewrite(Stream input, Stream output, string username, int userId)
        {
            byte[] original;

            using (var buffer = new MemoryStream())
            {
                input.CopyTo(buffer);
                original = buffer.ToArray();
            }

            byte[] rewritten;

            try
            {
                rewritten = rewrite(original, username, userId);
            }
            catch
            {
                rewritten = original;
            }

            output.Write(rewritten);
        }

        private static byte[] rewrite(byte[] original, string username, int userId)
        {
            using (var inStream = new MemoryStream(original))
            using (var sr = new SerializationReader(inStream))
            using (var outStream = new MemoryStream())
            {
                using (var sw = new SerializationWriter(outStream, true))
                {
                    sw.Write(sr.ReadByte());

                    int version = sr.ReadInt32();
                    sw.Write(version);

                    sw.Write(sr.ReadString());

                    sr.ReadString();
                    sw.Write(username);

                    sw.Write(sr.ReadString());

                    for (int i = 0; i < 6; i++)
                        sw.Write(sr.ReadUInt16());

                    sw.Write(sr.ReadInt32());
                    sw.Write(sr.ReadUInt16());
                    sw.Write(sr.ReadBoolean());
                    sw.Write(sr.ReadInt32());
                    sw.Write(sr.ReadString());
                    sw.Write(sr.ReadInt64());
                    sw.WriteByteArray(sr.ReadByteArray());

                    if (version >= 20140721)
                        sw.Write(sr.ReadInt64());
                    else if (version >= 20121008)
                        sw.Write(sr.ReadInt32());

                    if (version >= 30000001)
                    {
                        byte[] scoreInfo = sr.ReadByteArray();
                        sw.WriteByteArray(scoreInfo?.Length > 0 ? rewriteScoreInfo(scoreInfo, userId) : scoreInfo);
                    }

                    sw.Flush();
                }

                inStream.CopyTo(outStream);

                return outStream.ToArray();
            }
        }

        private static byte[] rewriteScoreInfo(byte[] compressed, int userId)
        {
            var scoreInfo = JsonConvert.DeserializeObject<LegacyReplaySoloScoreInfo>(decompress(compressed));

            if (scoreInfo == null)
                return compressed;

            scoreInfo.UserID = userId;

            return compress(scoreInfo.Serialize());
        }

        private static string decompress(byte[] data)
        {
            using (var inStream = new MemoryStream(data))
            {
                byte[] properties = new byte[5];
                if (inStream.Read(properties, 0, 5) != 5)
                    throw new IOException("input .lzma is too short");

                long outSize = 0;

                for (int i = 0; i < 8; i++)
                {
                    int v = inStream.ReadByte();
                    if (v < 0)
                        throw new IOException("input .lzma is too short");

                    outSize |= (long)(byte)v << (8 * i);
                }

                long compressedSize = inStream.Length - inStream.Position;

                using (var lzma = LzmaStream.Create(properties, inStream, compressedSize, outSize))
                using (var reader = new StreamReader(lzma))
                    return reader.ReadToEnd();
            }
        }

        private static byte[] compress(string data)
        {
            byte[] content = new ASCIIEncoding().GetBytes(data);

            using (var outStream = new MemoryStream())
            {
                using (var lzma = LzmaStream.Create(new LzmaEncoderProperties(false, 1 << 21, 255), false, outStream))
                {
                    outStream.Write(lzma.Properties);

                    long fileSize = content.Length;
                    for (int i = 0; i < 8; i++)
                        outStream.WriteByte((byte)(fileSize >> (8 * i)));

                    lzma.Write(content);
                }

                return outStream.ToArray();
            }
        }
    }
}
