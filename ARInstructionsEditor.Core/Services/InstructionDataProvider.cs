using ARInstructionsEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using System.IO.Compression;
using System.Xml.Serialization;

namespace ARInstructionsEditor.Core.Services
{
    public static class InstructionDataProvider
    {
        public static Instruction Instruction;

        public static event EventHandler NameChanged;

        /// <summary>
        /// Loads data from the given .zip-file
        /// </summary>
        /// <param name="zipToOpen"></param>
        /// <returns></returns>
        public static async Task LoadDataFromZipAsync(Stream zipToOpen, string fullExtractionPath)
        {
            string extractionPath = fullExtractionPath.Substring(0, fullExtractionPath.LastIndexOf(@"\"));

            await Task.Run(() => {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    //TODO: check if file(s) already exists
                    foreach (var entry in archive.Entries)
                    {
                        if (File.Exists(Path.Combine(extractionPath, entry.FullName)))
                        {
                            File.Delete(Path.Combine(extractionPath, entry.FullName));
                        }
                    }

                    try
                    {
                        archive.ExtractToDirectory(extractionPath);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            XmlSerializer serializer = new XmlSerializer(typeof(Instruction));
            using (Stream reader = new FileStream(fullExtractionPath, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                Instruction = (Instruction)serializer.Deserialize(reader);
            }
        }

        public static IEnumerable<MediaFile> GetImagesByStepNumber(int stepNumber)
        {
            return Instruction.Steps[stepNumber].MediaFiles;
        }

        public static void UpdateData(List<Step> steps)
        {
            Instruction.Steps = steps;
        }

        public async static Task<bool> ExportAsync(String PathToDataFolder, string newInstructionName, Stream zipFile)
        {
            if(Instruction == null)
            {
                return false;
            }

            Instruction.Name = newInstructionName;
            NameChanged?.Invoke(Instruction, null);
            //First save instruction to .save file
            XmlSerializer serializer = new XmlSerializer(typeof(Instruction));
            using (Stream writer = new FileStream(Path.Combine(PathToDataFolder, Instruction.Name + ".save"), FileMode.Create))
            {
                // Call the Deserialize method to restore the object's state.
                serializer.Serialize(writer, Instruction);
            }

            await Task.Run(() =>
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    //foreach(var entry in archive.Entries)
                    //{
                    //    entry.Delete();
                    //}
                    archive.CreateEntryFromFile(Path.Combine(PathToDataFolder, Instruction.Name + ".save"), Instruction.Name + ".save", CompressionLevel.Fastest);
                    var hashSet = new HashSet<String>();


                    foreach (var step in Instruction.Steps)
                    {

                        foreach (var file in step.MediaFiles)
                        {
                            if (hashSet.Add(file.FileName))
                            {
                                archive.CreateEntryFromFile(Path.Combine(PathToDataFolder, "media", file.FileName), @"media\" + file.FileName);
                            }
                        }
                    }
                }
            });


            return true;
        }
    }
}
