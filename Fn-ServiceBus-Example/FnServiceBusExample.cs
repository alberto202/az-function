using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fn_ServiceBus_Example
{
    public static class FnServiceBusExample
    {
        /// <summary>
        /// The trigger works in PeekLock mode. If an exception is thrown during the execution of the function, the message will be 
        /// abandoned. If the function completes without errors, the message will be completed and removed from the queue.
        /// </summary>
        /// <param name="queueMessageContent">
        /// The message body is be passed as a string parameter to the queue. Unfortunately the trigger does not allow us to 
        /// access the instance of the Microsoft.Azure.ServiceBus.Message class with it's full details.
        /// </param>
        /// <param name="log"></param>
        [FunctionName("dequeue")]
        public static void Subscriber([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")] string queueMessageContent)
        {
            try
            {
                Console.WriteLine("Dequeued task: " + queueMessageContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);

                throw;
            }
        }

        [FunctionName("FnHttpExample")]
        [return: ServiceBus("myqueue", Connection = "ServiceBusConnectionString")]
        public static async Task<string> Publisher(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return responseMessage;
        }
    }
}
