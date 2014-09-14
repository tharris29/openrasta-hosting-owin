﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.Hosting;
using OpenRasta.Web;

namespace OpenRasta.Owin
{

    public class OpenRastaMiddleware : OwinMiddleware
    {
        private static readonly object SyncRoot = new object();
        private HostManager _hostManager;
        private static ILogger<OwinLogSource> Log { get; set; }
        private static OwinHost Host { get; set; }

        public OpenRastaMiddleware(OwinMiddleware next, IConfigurationSource options)
            : base(next)
        {
            Host = new OwinHost(options);
        }

        public OpenRastaMiddleware(OwinMiddleware next, IConfigurationSource options,IDependencyResolverAccessor resolverAccesor)
            : base(next)
        {
            Host = new OwinHost(options, resolverAccesor);
        }
        
        public override async Task Invoke(IOwinContext owinContext)
        {
            TryInitializeHosting();

            try
            {
              owinContext = ProcessRequest(owinContext);                    

            }
            catch (Exception e)
            {
                owinContext.Response.StatusCode = 500;
                owinContext.Response.Write(e.ToString());
            }
            await Next.Invoke(owinContext);
        }

        private IOwinContext ProcessRequest(IOwinContext owinContext)
        {
            lock (SyncRoot)
            {
                var openRastaContext = new OwinCommunicationContext(owinContext, Log);
                    
                Host.RaiseIncomingRequestReceived(openRastaContext);

                Host.RaiseIncomingRequestProcessed(openRastaContext);
            }

            return owinContext;
        }


        private void TryInitializeHosting()
        {
            if (_hostManager != null) return;
            lock (SyncRoot)
            {
                Thread.MemoryBarrier();
                if (_hostManager != null) return;

                var hostManager = HostManager.RegisterHost(Host);
                Thread.MemoryBarrier();
                _hostManager = hostManager;
                try
                {
                    Host.RaiseStart();
                    _hostManager.Resolver.Resolve<ILogger<OwinLogSource>>();
                }
                catch
                {
                    HostManager.UnregisterHost(Host);
                    _hostManager = null;
                    throw;
                }
            }
        }
    }
}