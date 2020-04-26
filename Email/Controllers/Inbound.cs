using Email.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Email.Controllers
{
    [Route("inbound")]
    public class InboundController : ApiController
    {
        [HttpPost]
        [Route("inbound")]
        public async Task<string> ParseInbound()
        {
            var content = await Request.Content.ReadAsMultipartAsync();

            var sendgridEmailInfo = new SendgridEmailInfo();
            var awaitables = new List<Task>();
            try
            {
                foreach (var part in content.Contents)
                {
                    awaitables.Add(sendgridEmailInfo.AddMailMessagePart(part));
                }
                Task.WaitAll(awaitables.ToArray());
            }
            catch (Exception ex)
            {
                // TODO some error logging otherwise you will not know why your email gets dropped
            }

            return sendgridEmailInfo.Content.content;
        }

        [HttpGet]
        [Route("inbound/test")]
        public async Task Test()
        {
            // Experimental code try parsing using 3rd party lib
            var message = await MimeMessage.LoadAsync(GenerateStreamFromASCIIString(@""));

            return;
        }

        private static Stream GenerateStreamFromASCIIString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}