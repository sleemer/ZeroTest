using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroTransport
{
    public static class FileUtils
    {
        public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken token = default(CancellationToken))
        {
            using (var fileStream = File.OpenRead(path)) {
                byte[] content = new byte[fileStream.Length];
                await fileStream.ReadAsync(content, 0, content.Length, token);
                return content;
            }
        }
    }
}
