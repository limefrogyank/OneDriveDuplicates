using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneDriveDuplicates.Service
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;

        public LoggingHandler(ILogger<LoggingHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending Graph request", httpRequest);// log the request before it goes out.
            // Always call base.SendAsync so that the request is forwarded through the pipeline.
            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);
            _logger.LogInformation("Received Graph response", response);// log the response as it comes back.
            return response;
        }
    }
}
