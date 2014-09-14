using System.Linq;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace OpenRasta.Owin.Test
{
    [TestFixture]
    public class WhenSendingABadRequest : TestServerBase
    {
        string Url = "http://testserver/Get/WithParams?value=BADTYPE";

        [Test]
        public async void ResponseIsNotNull()
        {
            var response = await CallGetUrlAsync(Url);
            Assert.IsNotNull(response);
        }
        
        [Test]
        public async void ResponseStatusCodeIsBadRequest()
        {
            var response = await CallGetUrlAsync(Url);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
        }

        [Test]
        public async void ResponseHasAResult()
        {
            var response = await CallGetUrlAsync(Url);
            var readTask = response.Content.ReadAsStringAsync();
            readTask.Wait();
            Assert.IsNotNull(readTask.Result);
        }
    }
}
