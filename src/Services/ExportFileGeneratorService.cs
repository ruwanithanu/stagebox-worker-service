using System;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using Models.Extensions;
using Common;
using Renci.SshNet;
using DidiSoft.Pgp;

namespace Services
{
    public static class ExportFileGeneratorService
    {       
        /// <summary>
        /// Generates CSV file for a given list in the given network shared folder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="networkPath"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static void GenerateCSV<T>(string separator, List<T> list, string networkPath, NetworkCredential credentials)
        {
            var archiveFolder = Path.Combine(networkPath, "Archive");

            Type type = list.FirstOrDefault().GetType();
            IList<PropertyInfo> properties = new List<PropertyInfo>(type.GetFilteredProperties());

            var networkConn = new NetworkConnection(networkPath, credentials);

            using (networkConn)
            {
                if (File.Exists(networkPath + $"\\{Constants.csvFileName}"))
                {
                    if (!Directory.Exists(archiveFolder))
                        Directory.CreateDirectory(archiveFolder);

                    File.Move(networkPath + $"\\{Constants.csvFileName}", archiveFolder + $"\\{DateTime.Now:yyyyMMddHHmmssffff}_{Constants.csvFileName}");
                }

                using (StreamWriter writer = new StreamWriter(networkPath + $"\\{Constants.csvFileName}"))
                {
                    //No need of headers for shipnet exchange rates txt
                    //writer.WriteLine(string.Join(separator, properties.Select(p => p.GetPropertyDisplayName())));

                    foreach (var item in list)
                    {
                        if (list.IndexOf(item) == list.Count() - 1) //to avoid a blank line at the end of the file (writeline puts an end of line character after each line causing extra blank line at the end)
                            writer.Write(string.Join(separator, properties.Select(p => p.GetValue(item, null))));
                        else
                            writer.WriteLine(string.Join(separator, properties.Select(p => p.GetValue(item, null))));
                    }
                }
            }           
        }

        /// <summary>
        /// Generates CSV file for a given list in the given local archive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="networkPath"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static (bool,string) GenerateCSVInLocalArchive<T>(string separator, List<T> list, string localArchivePath, string localArchiveFolder)
        {
            Type type = list.FirstOrDefault().GetType();
            IList<PropertyInfo> properties = new List<PropertyInfo>(type.GetFilteredProperties());
            var localSNArchiveFolder = Path.Combine(localArchivePath, localArchiveFolder);

            if (!Directory.Exists(localArchivePath))
                Directory.CreateDirectory(localArchivePath);

            if (!Directory.Exists(localSNArchiveFolder))
                Directory.CreateDirectory(localSNArchiveFolder);

            var localSNArchiveFileName = $"{DateTime.Now:yyyyMMddHHmmssffff}_{Constants.csvFileName}"; 

            using (StreamWriter writer = new StreamWriter(localSNArchiveFolder + $"\\{localSNArchiveFileName}"))
            {
                foreach (var item in list)
                {
                    if (list.IndexOf(item) == list.Count() - 1) //to avoid a blank line at the end of the file (writeline puts an end of line character after each line causing extra blank line at the end)
                        writer.Write(string.Join(separator, properties.Select(p => p.GetValue(item, null))));
                    else
                        writer.WriteLine(string.Join(separator, properties.Select(p => p.GetValue(item, null))));
                }
            }

            return (File.Exists(localSNArchiveFolder + $"\\{localSNArchiveFileName}"),localSNArchiveFileName);
        }

        /// <summary>
        /// Copies a file from local archive to shared folders
        /// </summary>
        /// <param name="localArchiveFilePath"></param>
        /// <param name="networkPath"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static void CopyFileFromLocalArchiveToSharedFolders(string localArchiveFilePath, string networkPath, NetworkCredential credentials)
        {
            var archiveFolder = Path.Combine(networkPath, "Archive");
            var OKLogFolder = Path.Combine(networkPath, "OK");
            var ERRLogFolder = Path.Combine(networkPath, "ERR");
            var networkConn = new NetworkConnection(networkPath, credentials);

            using (networkConn)
            {
                if (!Directory.Exists(archiveFolder))
                    Directory.CreateDirectory(archiveFolder);

                if (File.Exists(networkPath + $"\\{Constants.csvFileName}"))
                    File.Move(networkPath + $"\\{Constants.csvFileName}", archiveFolder + $"\\{DateTime.Now:yyyyMMddHHmmssffff}_{Constants.csvFileName}");

                if (Directory.Exists(OKLogFolder))
                {
                    if (File.Exists(OKLogFolder + $"\\{Constants.csvlogFileName}"))
                        File.Move(OKLogFolder + $"\\{Constants.csvlogFileName}", archiveFolder + $"\\{DateTime.Now:yyyyMMddHHmmssffff}_OK_{Constants.csvlogFileName}");
                }

                if (Directory.Exists(ERRLogFolder))
                {
                    if (File.Exists(ERRLogFolder + $"\\{Constants.csvlogFileName}"))
                        File.Move(ERRLogFolder + $"\\{Constants.csvlogFileName}", archiveFolder + $"\\{DateTime.Now:yyyyMMddHHmmssffff}_ERR_{Constants.csvlogFileName}");
                }

                File.Copy(localArchiveFilePath, networkPath + $"\\{Constants.csvFileName}");
            }
        }

        /// <summary>
        /// Generates excel file for a given list in the given location
        /// </summary>
        /// <param name="list"></param>
        /// <param name="networkPath"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static void GenerateExcel<T>(List<T> list, string networkPath, string username, string password, string domain)
        {
            var archiveFolder = Path.Combine(networkPath, "Archive");

            Type type = list.FirstOrDefault().GetType();
            IList<PropertyInfo> properties = new List<PropertyInfo>(type.GetFilteredProperties());

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Exchange Rates");
                var currentRow = 1;
                int columnCounter = 1;

                foreach (var prop in properties)
                {
                    worksheet.Cell(currentRow, columnCounter).Value = prop.GetPropertyDisplayName();
                    columnCounter++;
                }
               
                foreach (var item in list)
                {
                    currentRow++;
                    columnCounter = 1;

                    foreach (var prop in properties)
                    {
                        worksheet.Cell(currentRow, columnCounter).Value = prop.GetValue(item, null);
                        columnCounter++;
                    }
                }

                var credentials = new NetworkCredential(username, password, domain);
                var networkConn = new NetworkConnection(networkPath, credentials);

                using (networkConn)
                {
                    if (File.Exists(networkPath + $"\\{Constants.excelFileName}"))
                    {
                        if (!Directory.Exists(archiveFolder))
                            Directory.CreateDirectory(archiveFolder);

                        File.Move(networkPath + $"\\{Constants.excelFileName}", archiveFolder + $"\\{DateTime.Now:yyyyMMddHHmmssffff}_{Constants.excelFileName}");
                    }
                    workbook.SaveAs(networkPath + $"\\{Constants.excelFileName}");
                }
            }
        }


        /// <summary>
        /// https://stackoverflow.com/questions/26700765/c-sharp-sftp-upload-files
        /// </summary>
        /// <param name="sftpClient"></param>
        /// <param name="networkCredentials"></param>
        /// <param name="networkPath"></param>
        /// <param name="filePrefix"></param>
        /// <param name="fileContent"></param>
        /// <param name="sftpPgpKey"></param>
        public static void GenarateCSVInSFTP<T>(SftpClient sftpClient, NetworkCredential networkCredentials, string networkPath, string filePrefix, List<T> fileContent, string sftpPgpKey)
        {
            var networkConn = new NetworkConnection(networkPath, networkCredentials);
            var fileName = $"{filePrefix}{DateTime.Now.ToString("yyyyMMddhhmmss")}.txt";
            var filePath = Path.Combine(networkPath, fileName);

            using (networkConn)
            {
                //archive
                var archiveFolder = Path.Combine(networkPath, "Archive");
                var filePathsToArchive = Directory.GetFiles(networkPath).Where(x => x.Contains(filePrefix)).ToList();
                foreach (string filePathToArchive in filePathsToArchive)
                {
                    File.Move(filePathToArchive, Path.Combine(archiveFolder, Path.GetFileName(filePathToArchive)));
                }

                //create txt
                using (StreamWriter writer = new StreamWriter(filePath))
                {                  
                    foreach (var line in fileContent)
                    {
                        writer.WriteLine(line);
                    }
                }

                if (false) //enable on Feb 28th
                {
                    //create encrypt pgp
                    var pgp = new PGPLib();
                    var filePgpPath = Path.Combine(networkPath, $"{fileName}.pgp");
                    var publickeyFilePath = Path.Combine(new[] { networkPath, sftpPgpKey, $"{sftpPgpKey}.asc" });
                    pgp.EncryptFile(filePath, publickeyFilePath, filePgpPath, asciiArmor: false, withIntegrityCheck: false);

                    //create in sftp
                    using (sftpClient)
                    {
                        sftpClient.Connect();
                        sftpClient.ChangeDirectory(@"in");
                        if (sftpClient.IsConnected)
                        {
                            using (var fileStream = new FileStream(filePgpPath, FileMode.Open))
                            {
                                sftpClient.BufferSize = 4 * 1024; // bypass Payload error large files
                                sftpClient.UploadFile(fileStream, Path.GetFileName(filePgpPath));
                            }
                        }
                    }
                }
            }
        }
    }
}
