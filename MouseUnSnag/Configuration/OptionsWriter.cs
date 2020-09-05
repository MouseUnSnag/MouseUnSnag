using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MouseUnSnag.Configuration
{
    public class OptionsWriter
    {
        private OptionsSerializer _serializer;

        public string FileName { get; }
        public string TempFileName => Path.ChangeExtension(FileName, "tmp");
        public string BackupFileName => Path.ChangeExtension(FileName, "bak");
        

        /// <summary>
        /// Initializes a new <see cref="OptionsWriter"/>
        /// </summary>
        /// <param name="fileName">Filename of the file to write</param>
        /// <param name="serializer">Options Serializer that will be used to serialize / deserialize the data.</param>
        public OptionsWriter(string fileName, OptionsSerializer serializer = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            FileName = fileName;
            _serializer = serializer ?? new OptionsSerializer();
        }

        /// <summary>
        /// Write the options to the file
        /// </summary>
        /// <param name="options"></param>
        public void Write(Options options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var text = _serializer.Serialize(options);

            var dir = Path.GetDirectoryName(FileName) ?? throw new InvalidOperationException();
            CreateDirectory(dir);

            TryDeleteFile(TempFileName);
            File.WriteAllText(TempFileName, text);

            TryDeleteFile(BackupFileName);
            if (File.Exists(FileName))
                File.Move(FileName, BackupFileName);

            File.Move(TempFileName, FileName);
        }

        /// <summary>
        /// Try to read options from the file. Returns null if an error occurred
        /// </summary>
        /// <returns></returns>
        public Options TryRead()
        {
            try
            {
                return Read();
            }
            catch (IOException e) { Debug.WriteLine(e); }
            catch (SecurityException e) { Debug.WriteLine(e); }
            catch (UnauthorizedAccessException e) { Debug.WriteLine(e); }

            return null;
        }

        /// <summary>
        /// Read Options from the file.
        /// </summary>
        /// <returns>Options that were read</returns>
        public Options Read()
        {
            RestoreBackupsIfNeeded();

            var text = File.ReadAllText(FileName);
            return _serializer.Deserialize(text);
        }

        private void RestoreBackupsIfNeeded()
        {
            try
            {
                if (File.Exists(FileName))
                    return;

                if (File.Exists(BackupFileName))
                {
                    File.Move(BackupFileName, FileName);
                    return;
                }

                if (File.Exists(TempFileName))
                {
                    File.Move(TempFileName, FileName);
                }

            }
            catch (IOException e) { Debug.WriteLine(e); }
        }


        private static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void TryDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (IOException e) { Debug.WriteLine(e); }
            catch (SecurityException e) { Debug.WriteLine(e); }
            catch (UnauthorizedAccessException e) { Debug.WriteLine(e); }
        }

    }
}
