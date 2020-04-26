using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Email.Models
{
    public class SendgridEmailInfo
    {
        private readonly Dictionary<string, byte[]> _parts = new Dictionary<string, byte[]>();
        private readonly List<string> _attachmentFileNames = new List<string>();
        public string[] To { get; private set; }
        public string From { get; private set; }
        public string Subject
        {
            get
            {
                return Encoding.UTF8.GetString(_parts["subject"]);
            }
        }
        public (string content, string text, string html) Content
        {
            get
            {
                var html = !_parts.ContainsKey("html") ? null : Encoding.UTF8.GetString(_parts["html"]);
                var text = !_parts.ContainsKey("text") ? null : Encoding.UTF8.GetString(_parts["text"]);
                // TODO can be usefull to autoextract text from html if text is not present
                return (html ?? text, html, text);
            }
        }
        public (string fileName, byte[] content)[] Attachments
        {
            get
            {
                return _attachmentFileNames.Select((value, index)
                    => (value, _parts[$"attachment{index + 1}"])).ToArray();
            }
        }

        /// <summary>
        /// Add part of incoming multipart http request from SendGrid WebHook
        /// </summary>
        public async Task AddMailMessagePart(HttpContent part)
        {
            var partName = part.Headers.ContentDisposition.Name.Trim(new char[] { '"' });
            var partContent = await part.ReadAsByteArrayAsync();
            _parts.Add(partName, partContent);

            switch (partName)
            {
                case "envelope":
                    var envelopeInfo = (dynamic) JObject.Parse(Encoding.UTF8.GetString(partContent));
                    To = envelopeInfo.to.ToObject<string[]>();
                    From = envelopeInfo.from;
                    break;

                case "attachment-info":
                    var attachmentInfo = (dynamic)JObject.Parse(Encoding.UTF8.GetString(partContent));

                    int attachmentNo = 1;
                    while (true)
                    {
                        var attachment = attachmentInfo[$"attachment{attachmentNo++}"];
                        if (attachment == null)
                        {
                            break;
                        }
                        _attachmentFileNames.Add(attachment["filename"].ToString());
                    }

                    break;
            }
        }
    }
}