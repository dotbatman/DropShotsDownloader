using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;


namespace DropShotsDownloader
{
    class Program
    {
        private static HashSet<string> pathHashes = new HashSet<string>();
        static void Main(string[] args)
        {

            using (System.IO.StreamWriter logFile = new System.IO.StreamWriter(@"log.txt"))
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                var client = new RestClient("https://www.dropshots.com/V1.0/");
                //client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                client.UseSerializer(new JsonNetSerializer());
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36";
                var listOfAllTheThings = new List<MediaItem>();
                for (int j = 1; j <= 6; j++)
                {
                    var request = new RestRequest("Media.getList", DataFormat.Json);
                    request.AddParameter("appid", "dropshots");
                    request.AddParameter("username", "<yourusernamegoeshere>");
                    request.AddParameter("userid", "undefined");
                    request.AddParameter("passwordhash", "undefined");
                    request.AddParameter("mediatype", "all");
                    request.AddParameter("passwordprotection", "false");
                    request.AddParameter("page", j);
                    request.AddParameter("ouput", "json");
                    request.AddParameter("min_taken_date", "2015-05-26");
                    request.AddParameter("max_taken_date", "2020-09-26");
                    request.AddParameter("per_page", 3000);

                    var results = client.Get<Stupid>(request);
                    if (results.ResponseStatus != ResponseStatus.Completed)
                        throw new Exception("Couldn't get list");

                    if(results.Data != null)
                        listOfAllTheThings.AddRange(results.Data.media.Item);
                }


                using (WebClient wc = new WebClient())
                {
                    int i = 1;
                    client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    foreach (var mi in listOfAllTheThings)
                    {
                        if (pathHashes.Add(mi.download))
                        {
                            DateTime timeStamp = (mi.date + mi.time);
                            string fileName = timeStamp.ToString(@"yyyy-MM-dd-HH-mm-ss") + "-" + i + "-" + mi.downloadname;
                            Console.Write(String.Format("{0} --> {1}", mi.download, fileName));
                            try
                            {
                                wc.DownloadFile(mi.download, fileName);
                                Console.WriteLine(" success " + i);
                                logFile.WriteLine(" success " + i);
                                System.Diagnostics.Debug.WriteLine(" success " + i);
                                File.SetCreationTime(fileName, timeStamp);
                                File.SetLastWriteTime(fileName, timeStamp);
                            }
                            catch (WebException we)
                            {
                                var response = (we.Response as HttpWebResponse);
                                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                                {
                                    Console.WriteLine(" not found " + i);
                                    logFile.WriteLine(" not found " + i);
                                    System.Diagnostics.Debug.WriteLine(" not found " + i);
                                }
                                else
                                    throw;
                            }
                        }
                        i++;
                    }
                }

                logFile.Flush();
            }
        }
    }

    public class JsonNetSerializer : IRestSerializer
    {
        public string Serialize(object obj) =>
            JsonConvert.SerializeObject(obj);

        public string Serialize(Parameter parameter) =>
            JsonConvert.SerializeObject(parameter.Value);

        public T Deserialize<T>(IRestResponse response) =>
            JsonConvert.DeserializeObject<T>(response.Content);

        public string[] SupportedContentTypes { get; } =
        {
                "text/html", "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
            };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }
}
