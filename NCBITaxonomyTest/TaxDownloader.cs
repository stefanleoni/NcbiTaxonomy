using System;
using System.IO;
using System.Net;
using Ionic.Zip;

namespace NCBITaxonomyTest
{
    public class FtpDownloader
    {
        public void DownloadFileAnonymous(string uri, string targetFile)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest requestSize = (FtpWebRequest) WebRequest.Create(uri);

            requestSize.Method = WebRequestMethods.Ftp.GetFileSize;

            FtpWebResponse sizeResponse = (FtpWebResponse)requestSize.GetResponse();
            long size = sizeResponse.ContentLength;
            sizeResponse.Close();
            Progress?.Invoke(DownloadProgressStatus.Started, -1, -1);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");

            using (FtpWebResponse response = (FtpWebResponse) request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (FileStream fileStream = new FileStream(targetFile, FileMode.Create))
                    {
                        byte[] buffer = new byte[2048];
                        long sumRead = 0;

                        while (true)
                        {
                            if (responseStream != null)
                            {
                                var bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                                sumRead += bytesRead;
                                Progress?.Invoke(DownloadProgressStatus.Progress ,sumRead, size);
                                if (bytesRead == 0)
                                    break;
                                fileStream.Write(buffer, 0, bytesRead);
                            }
                        }
                        fileStream.Close();
                        Progress?.Invoke(DownloadProgressStatus.Finished, -1, -1);
                    }
                    response.Close();
                }
            }
        }

        public Action<DownloadProgressStatus, long, long> Progress { get; set; }

        public void ExtractTaxDump(string zipFile, string targetPath)
        {
            Progress?.Invoke(DownloadProgressStatus.Extracting, -1, -1);
            using (ZipFile z = new ZipFile(zipFile))
            {
                z.ExtractAll(targetPath);
            }
            Progress?.Invoke(DownloadProgressStatus.Complete, -1, -1);
        }
    }

    public enum DownloadProgressStatus
    {
        Started,
        Progress,
        Finished,
        Extracting,
        Complete,
        Error
    }

    public class TaxDumpSource
    {
        public TaxDumpSource(string pathOfDumps, string ncbiDumpArchiveFile = "new_taxdump.zip")
        {
            dumpsPath = pathOfDumps;
            ncbiDumpFilename = ncbiDumpArchiveFile;
        }

        private string dumpsPath;
        private string ncbiDumpFilename;

        public const string NcbiTaxDumpUri = "ftp://ftp.ncbi.nih.gov/pub/taxonomy/new_taxdump/new_taxdump.zip";

        public FtpDownloader Downloader { get; private set; }
        public string NodesDumpFile => Path.Combine(dumpsPath, "nodes.dmp");
        public string NamesDumpFile => Path.Combine(dumpsPath, "names.dmp");
        public string BrukerDumpFile => Path.Combine(dumpsPath, "bruker.dmp");
        public string BrukerNamesDumpFile => Path.Combine(dumpsPath, "brukerNames.dmp");
        public string BrukerNodesDumpFile => Path.Combine(dumpsPath, "brukerNodes.dmp");


        public void Create(Action<DownloadProgressStatus, long, long> progress = null)
        {
            var targetFilename = Path.Combine(dumpsPath, ncbiDumpFilename);

            if (!File.Exists(targetFilename))
            {
                Directory.CreateDirectory(dumpsPath);
                if (Downloader == null)
                {
                    Downloader = new FtpDownloader();
                    if (progress != null)
                    {
                        Downloader.Progress = progress;
                    }
                }
                //progress?.Invoke(DownloadProgressStatus.Started, -1, -1);
                Downloader.DownloadFileAnonymous(NcbiTaxDumpUri, targetFilename);
            }

            if (!File.Exists(Path.Combine(dumpsPath, "nodes.dmp")))
            {
                if (Downloader == null)
                {
                    Downloader = new FtpDownloader();
                    if (progress != null)
                    {
                        Downloader.Progress = progress;
                    }
                }
                Downloader.ExtractTaxDump(targetFilename, dumpsPath);
            }

        }
    }
}