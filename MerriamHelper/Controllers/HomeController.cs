using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WordHelp.Models;
using NAudio.Wave;
using System.Net;
using System.Xml.Linq;

namespace MerriamHelper.Controllers
{
    public class HomeController : Controller
    {
        private WordsDB db = new WordsDB();
        private readonly string KEY = "5e80e08c-5a40-4cec-9c3d-4f98497cfac0";

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Search(string name)
        {

            db.Path = Server.MapPath("~/Download/");
            if (!DownloadFile(db.Path, name))
            {
                db.Path = "Cannot find the word";
                
            }
           

            return PartialView(db);
        }



        private bool DownloadFile(string outputPath, string fileName)
        {
            string requestUri = ("http://www.dictionaryapi.com/api/v1/references/collegiate/xml/" + fileName + "?key=" + KEY);
            string requestWav = null;


            var request = WebRequest.Create(requestUri);
            using (var responseA = request.GetResponse())
            {
                using (var sr = new System.IO.StreamReader(responseA.GetResponseStream()))
                {
                    XDocument xmlDoc = new XDocument();
                    try
                    {
                        xmlDoc = XDocument.Parse(sr.ReadToEnd());


                        string value = xmlDoc.Descendants("wav").First().Value;
                        string subdirectory = null;

                        if (value.Length > 0)
                        {
                            if (value.StartsWith("bix"))
                            {
                                subdirectory = "bix";
                            }
                            else if (value.StartsWith("gg"))
                            {
                                subdirectory = "gg";
                            }
                            else
                            {

                                subdirectory = value[0].ToString();
                            }
                        } // end if 


                        requestWav = "http://media.merriam-webster.com/soundc11/" + subdirectory + "/" + value;

                        
                        string outputFile = outputPath + fileName + ".wav";
                        string sourceFile = outputPath + fileName + "01" + ".wav";
                        int silence = 5000;  // silence to add in ms
                        WebClient wc = new WebClient();


                        wc.DownloadFile(requestWav, sourceFile);
                        Concatenate(outputFile, sourceFile, 10, silence);
                        return true; 

                    }// end try

                         

                    catch (Exception ex)
                    {
                        return false;
                    }

                } // end sub using 
            }// end responesA using
        }
        private void Concatenate(string OutputFile, string SourceFile, int times, int silence)
        {
            if (System.IO.File.Exists(OutputFile))
            {
                System.IO.File.Delete(OutputFile);

                using (System.IO.File.Create(OutputFile)) { }

            }


            byte[] buffer = new byte[1024];

            WaveFileWriter waveFileWriter = null;

            try
            {
                for (int i = 1; i <= times; i++)
                {
                    using (WaveFileReader reader = new WaveFileReader(SourceFile))
                    {
                        if (waveFileWriter == null)
                        {
                            // first time in create new Writer
                            waveFileWriter = new WaveFileWriter(OutputFile, reader.WaveFormat);
                        }
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                            {
                                throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                            }
                        }

                        int avgBytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
                        var silenceArraySize = avgBytesPerMillisecond * silence;
                        byte[] silenceArray = new byte[silenceArraySize];

                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                        // write space 
                        waveFileWriter.Write(silenceArray, 0, silenceArray.Length);
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                {
                    waveFileWriter.Dispose();
                    System.IO.File.Delete(SourceFile);

                }
            }// end finally

        }// end function
    }
}
