using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using svm_compute;

namespace svm_compute
{
    public class hash
    {
        public static void RenameToSha256Hash(string filename)
        {
            var checksum = GetSha256Hash(filename);
            var checksumFilename = $@"{Path.GetDirectoryName(filename)}\{checksum}{Path.GetExtension(filename)}";

            if (checksumFilename == filename) return;

            var n = 0;
            while (File.Exists(checksumFilename))
            {
                n++;
                checksumFilename = $@"{Path.GetDirectoryName(filename)}\_{n.ToString().PadLeft(5, '0')}_{checksum}{Path.GetExtension(filename)}";
            }

            File.Move(filename, checksumFilename);


            if (program.write_console_log) program.WriteLine($@"--> {checksumFilename}");
            if (program.write_console_log) program.WriteLine($@"");
        }

        public static string GetSha256Hash(string filename)
        {
            using (var stream = new BufferedStream(File.OpenRead(filename), 4194304 /* 4 MB */))
            {
                var sha256 = new SHA256Managed();
                var hashBytes = sha256.ComputeHash(stream);
                var hashString = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                return hashString;
            }
        }

    }
}
