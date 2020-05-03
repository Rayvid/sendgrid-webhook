// Code sample for ASP.NET MVC on .NET Framework 4.6.1+
// In Package Manager, run:
using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace Twilio.Controllers
{
    [Route("sms")]
    public class SmsController : TwilioController
    {
        [HttpPost]
        [Route("sms")]
        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            return new TwiMLResult();
        }
    }
}