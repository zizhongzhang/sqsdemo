using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon.SQS.Model;

namespace webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IAmazonSQS _sqs;
        private readonly string _queueName = "terraform-example-queue";

        public QueueController(ILogger<QueueController> logger, IAmazonSQS sqs)
        {
            _logger = logger;
            _sqs = sqs;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var queueUrl = (await _sqs.GetQueueUrlAsync(_queueName)).QueueUrl;
            var stopwatch = new Stopwatch();
            // foreach (var number in Enumerable.Range(1, 10))
            // {
            //     await _sqs.SendMessageAsync(queueUrl, $"hello {number}");
            // }
            // await Task.Delay(2000);

            stopwatch.Start();

            var request = new ReceiveMessageRequest(queueUrl) { MaxNumberOfMessages = 10 };
            var response = await _sqs.ReceiveMessageAsync(request);

            return Ok($"total count: {response.Messages.Count}. total duration: {stopwatch.ElapsedMilliseconds}");
        }
    }
}
