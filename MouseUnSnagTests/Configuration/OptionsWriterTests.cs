using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using MouseUnSnagTests.Configuration;

namespace MouseUnSnag.Configuration.Tests
{
    [TestClass()]
    public class OptionsWriterTests
    {
        private string _optionsFileName => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OptionsWriterTests", "config.txt");

        [TestMethod()]
        public void OptionsWriterTest()
        {
            var optionsWriter = new OptionsWriter(_optionsFileName, new OptionsSerializer());
            Assert.IsNotNull(optionsWriter);
            
            optionsWriter = new OptionsWriter(_optionsFileName, null);
            Assert.IsNotNull(optionsWriter);

            var gotException = false;
            try
            {
                _ = new OptionsWriter(null, null);
            }
            catch (ArgumentNullException)
            {
                gotException = true;
            }

            Assert.IsTrue(gotException, "Should have given an exception");

        }

        [TestMethod()]
        public void WriteTest()
        {
            var writer = new OptionsWriter(_optionsFileName);

            DeleteAllWriterFiles();
            writer.Write(new Options());
            Assert.IsTrue(File.Exists(_optionsFileName));
            writer.Write(new Options());
            writer.Write(new Options());
        }

        [TestMethod()]
        public void ReadTest()
        {
            var writer = new OptionsWriter(_optionsFileName);

            
            DeleteAllWriterFiles();
            var gotException = false;
            try
            {
                writer.Read();
            }
            catch (IOException)
            {
                gotException = true;
            }
            Assert.IsTrue(gotException, "Should have failed due to nonexisting file");

            writer.Write(new Options());
            var options = writer.Read();
            Assert.IsNotNull(options);

            // Restore from Backup
            writer.Write(new Options()); 
            File.Delete(_optionsFileName);
            options = writer.Read();
            Assert.IsNotNull(options);
        }

        [TestMethod()]
        public void TryReadTest()
        {
            var writer = new OptionsWriter(_optionsFileName);

            DeleteAllWriterFiles();
            var result = writer.TryRead();
            Assert.IsNull(result);

            writer.Write(new Options());
            var options = writer.Read();
            Assert.IsNotNull(options);
        }


        [TestMethod()]
        public void RoundTripTest()
        {
            var writer = new OptionsWriter(_optionsFileName);
            
            DeleteAllWriterFiles();


            foreach (var options in OptionsHelpers.OptionPermutations())
            {
                writer.Write(options);
                var actual = writer.Read();
                OptionsHelpers.Compare(options, actual);
            }

        }


        private void DeleteAllWriterFiles()
        {
            var writer = new OptionsWriter(_optionsFileName);

            TryDeleteFile(writer.FileName);
            TryDeleteFile(writer.TempFileName);
            TryDeleteFile(writer.BackupFileName);
        }

        private void TryDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (DirectoryNotFoundException) { }
        }
    }
}
