using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amr.ApiLoadtests
{
    internal class ApiLoadtestHelper
    {
        private readonly string _endpoint;
        int TotalRequests = 0;
        int TotalErrors = 0;
        int TotalOk = 0;
        TimeSpan AccumTime = new(0, 0, 0);

        public ApiLoadtestHelper(string endpoint)
        {
            _endpoint = endpoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threads">Hilos</param>
        /// <param name="totalRequests">Total solicitudes por hilo</param>
        /// <param name="restUrl">Url Rest</param>
        /// <returns></returns>
        public async Task RestLoadTest(int threads, int totalRequests)
        {
            List<Task> tasks = new List<Task>();
            Stopwatch stopwatch = new Stopwatch();
            // string token5519 = "Basic ZGVzYXJyb2xsbzAxOjFuYWxhbUJyMWE=";
            // string token5520 = "Basic ZGVzYXJyb2xsbzAyOjFuYWxhbUJyMWE=";
            string[] auths = { "Basic ZGVzYXJyb2xsbzAxOjFuYWxhbUJyMWE=", "Basic ZGVzYXJyb2xsbzAyOjFuYWxhbUJyMWE=" };
            long[] initphones = { 3023770000, 3115900000, 3162330000 };
            Random random = new Random();
            stopwatch.Start();
            for (int i = 1; i <= threads; i++)
            {
                string testName = $"AMR Load Test thread {i}";
                string auth = auths[random.Next(0, 1)];
                long phone = initphones[random.Next(0, 2)];
                tasks.Add(PrepareRestLoadTest(testName, auth, phone, totalRequests));
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            var eleapsedTime = stopwatch.Elapsed;
            TimeSpan averageTime = new TimeSpan(AccumTime.Ticks / threads);
            Console.WriteLine("");
            Console.WriteLine("########################### RESUMEN ###########################");
            Console.WriteLine("Solicitudes procesadas: {0}. Solicitudes exitosas: {1}. Solicitudes con error: {2}.", TotalRequests, TotalOk, TotalErrors, eleapsedTime, averageTime);
            Console.WriteLine("Tiempo transcurrido: '{0:mm\\:ss\\.fff}' (mm:ss.ml). Tiempo promedio: '{1:mm\\:ss\\.fff}' (mm:ss.ml).", eleapsedTime, averageTime);
        }
        private async Task PrepareRestLoadTest(string taskName, string authToken, long initPhone, int totalRequests)
        {
            int okResponses = 0;
            int errorResponses = 0;
            string message = "{0}. Request: {1}.";
            int requests = totalRequests;
            Console.WriteLine("{0} Inicia proceso. Teléfono inicial: {1}. Solicitudes a realizar: {2} ", taskName, initPhone, totalRequests);
            using (HttpClient httpClient = new HttpClient())
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                long phone = initPhone;
                for (int counter = 1; counter <= requests; counter++)
                {
                    string devices = String.Empty;
                    for (int i = 0; i < 10; i++)
                    {
                        phone++;
                        devices = String.Concat(devices, phone, "-");
                    }

                    devices = devices.Trim('-');
                    string requestBody = string.Concat("{\"Type\":1,\"MessageText\":\"", string.Format(message, taskName, counter), "\", \"Devices\":\"", devices, "\"}"); // replace with your JSON request body

                    var response = await CallRestApi(httpClient, requestBody, authToken);
                    Interlocked.Add(ref TotalRequests, 1);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        okResponses++;
                        Interlocked.Add(ref TotalOk, 1);
                    }
                    else
                    {
                        errorResponses++;
                        Interlocked.Add(ref TotalErrors, 1);
                    }
                    // read the response body as string
                    // string responseBody = await response.Content.ReadAsStringAsync();

                    Console.Write(".");
                }
                stopwatch.Stop();
                var eleapsedTime = stopwatch.Elapsed;

                AccumTime += eleapsedTime;
                Console.WriteLine("");
                Console.WriteLine("{0} Proceso  finalizado. Tiempo transcurrido: '{3:mm\\:ss\\:fff}'.Solicitudes enviadas: {1}, solicitudes fallidas: {2} ", taskName, okResponses, errorResponses, eleapsedTime);
            }
        }

        private async Task<HttpResponseMessage> CallRestApi(HttpClient httpClient, string requestBody, string authToken)
        {
            // create the HTTP request message
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", authToken);

            // send the request and get the response
            return await httpClient.SendAsync(request);
        }
    }
}
