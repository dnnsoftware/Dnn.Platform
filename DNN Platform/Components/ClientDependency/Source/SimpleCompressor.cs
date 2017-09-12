using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace ClientDependency.Core
{
    public class SimpleCompressor
    {
        public static byte[] CompressBytes(CompressionType type, byte[] fileBytes)
        {
            using (var ms = new MemoryStream())
            {
                Stream compressedStream = null;

                if (type == CompressionType.deflate)
                {
                    compressedStream = new DeflateStream(ms, CompressionMode.Compress, true);
                }
                else if (type == CompressionType.gzip)
                {
                    compressedStream = new GZipStream(ms, CompressionMode.Compress, true);
                }

                if (compressedStream != null)
                {
                    //write the bytes to the compressed stream
                    compressedStream.Write(fileBytes, 0, fileBytes.Length);
                    compressedStream.Close();
                    byte[] output = ms.ToArray();
                    ms.Close();
                    return output;
                }

                //not compressed
                return fileBytes;
            }
        }

        public static byte[] DecompressBytes(CompressionType type, byte[] compressedBytes)
        {
            using (var ms = new MemoryStream())
            {
                Stream decompressedStream = null;

                if (type == CompressionType.deflate)
                {
                    decompressedStream = new DeflateStream(ms, CompressionMode.Decompress, true);
                }
                else if (type == CompressionType.gzip)
                {
                    decompressedStream = new GZipStream(ms, CompressionMode.Decompress, true);
                }

                if (decompressedStream != null)
                {
                    //write the bytes to the compressed stream
                    decompressedStream.Write(compressedBytes, 0, compressedBytes.Length);
                    decompressedStream.Close();
                    byte[] output = ms.ToArray();
                    ms.Close();
                    return output;
                }

                //not compressed
                return compressedBytes;
            }
            
        }
    }
}
