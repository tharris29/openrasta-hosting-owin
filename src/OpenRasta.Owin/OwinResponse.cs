using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using OpenRasta.DI;
using OpenRasta.Web;

namespace OpenRasta.Owin
{
    public class OwinResponse : IResponse
    {
        public OwinResponse(IOwinContext context)
        {
            var response = context.Response;

            NativeContext = response;
            Headers = new HttpHeaderDictionary();
            var delayedStream = new DelayedStream(context.Response.Body);
            Entity = new HttpEntity(Headers, delayedStream);
        }

        private IOwinResponse NativeContext { get; set; }
        public IHttpEntity Entity { get; private set; }
        public HttpHeaderDictionary Headers { get; private set; }
        public bool HeadersSent { get; private set; }
        private bool FlushSent { get; set; }

        public int StatusCode
        {
            get { return NativeContext.StatusCode; }
            set { NativeContext.StatusCode = value; }
        }

        public void WriteHeaders()
        {
            if (HeadersSent)
            {
                throw new InvalidOperationException("The headers have already been sent.");
            }

            var commcontext = DependencyManager.GetService<ICommunicationContext>();
            foreach (var header in Headers.Where(h => h.Key != "Content-Type"))
            {
                try
                {
                    NativeContext.Headers.Remove(header.Key);
                    NativeContext.Headers.Append(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    if (commcontext != null)
                    {
                        commcontext.ServerErrors.Add(new Error {Message = ex.ToString()});
                    }
                }
            }
            HeadersSent = true;
            if (Headers.ContentType != null)
            {
                NativeContext.Headers.Add(new KeyValuePair<string, string[]>("Content-Type",
                    new[] {Headers.ContentType.MediaType}));
            }

            WriteResponse(NativeContext, commcontext);
            
        }

        private void WriteResponse(IOwinResponse owinContext, ICommunicationContext context)
        {
            owinContext.Environment.Add("OR_ServerErrors", context.ServerErrors);
            Entity.Stream.Flush();
        }
    }
}