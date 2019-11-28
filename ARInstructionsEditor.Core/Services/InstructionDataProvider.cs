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

        /// <summary>
        /// Loads data from the given .zip-file
        /// </summary>
        /// <param name="zipToOpen"></param>
        /// <returns></returns>
        public static async Task LoadDataFromZipAsync(Stream zipToOpen, string instructionFullPath)
        {
            string extractionPath = instructionFullPath.Substring(0, instructionFullPath.LastIndexOf(@"\"));

            await Task.Run(() => {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    //Task.Delay(10000).Wait(); // for testing

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
                    catch (Exception)
                    {

                    }
                }
            });

            XmlSerializer serializer = new XmlSerializer(typeof(Instruction));
            using (Stream reader = new FileStream(instructionFullPath, FileMode.Open))
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
            //First save instruction to .save file
            XmlSerializer serializer = new XmlSerializer(typeof(Instruction));
            using (Stream writer = new FileStream(Path.Combine(PathToDataFolder, Instruction.Name + ".save"), FileMode.OpenOrCreate))
            {
                // Call the Deserialize method to restore the object's state.
                serializer.Serialize(writer, Instruction);
            }

            await Task.Run(() =>
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(Path.Combine(PathToDataFolder, Instruction.Name + ".save"), Instruction.Name + ".save");

                    foreach (var step in Instruction.Steps)
                    {
                        foreach (var file in step.MediaFiles)
                        {
                            archive.CreateEntryFromFile(Path.Combine(PathToDataFolder, "media", file.FileName), @"media\" + file.FileName);
                        }
                    }
                }
            });


            return true;
        }
    }
}
