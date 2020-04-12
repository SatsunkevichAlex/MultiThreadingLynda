using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Networking
{
    [TestClass]
    public class TestDownload
    {
        const string url = "http://deelay.me/5000/http://www.delsink.com";

        [TestMethod]
        public void Test_Download_DelsingCom_Syncronous()
        {
            HttpWebRequest httpRequestInfo = HttpWebRequest.CreateHttp(url);
            WebResponse httpResponseInfo = httpRequestInfo.GetResponse();

            Stream responseStream = httpResponseInfo.GetResponseStream();
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string webPage = sr.ReadToEnd();
            }
        }

        [TestMethod]
        public void Test_Download_DelsinCom_BeginEnd()
        {
            WebRequest httpRequestInfo = HttpWebRequest.CreateHttp(url);
            AsyncCallback callback = new AsyncCallback(HttpResponseAvailable);
            IAsyncResult ar = httpRequestInfo.BeginGetResponse(callback, httpRequestInfo);

            ar.AsyncWaitHandle.WaitOne();
        }
        private static void HttpResponseAvailable(IAsyncResult ar)
        {
            WebRequest request = ar.AsyncState as HttpWebRequest;
            WebResponse response = request.EndGetResponse(ar) as HttpWebResponse;

            Stream responseStream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string page = reader.ReadToEnd();
            }
        }


        [TestMethod]
        public void Test_Download_DelsingCom_AsynTask()
        {
            WebRequest httpRequestInfo = HttpWebRequest.CreateHttp(url);
            Task<WebResponse> taskWebResponse = httpRequestInfo.GetResponseAsync();
            Task taskContinuation = taskWebResponse.ContinueWith(HttpResponseConfiguration, TaskContinuationOptions.OnlyOnRanToCompletion);

            Task.WaitAll(taskWebResponse, taskContinuation);
        }        
        private static void HttpResponseConfiguration(Task<WebResponse> taskResponse)            
        {
            HttpWebResponse httpResponseInfo = taskResponse.Result as HttpWebResponse;
            Stream responseStream = httpResponseInfo.GetResponseStream();
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string webPage = sr.ReadToEnd();
            }
        }

        [TestMethod]
        public async Task Test_Download_DelsingCom_AsyncAwait()
        {
            WebRequest request = HttpWebRequest.CreateHttp(url);
            WebResponse response = await request.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(responseStream))
            {
                string webPage = sr.ReadToEnd();
            }
        }
    }
}