﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Diagnostics.Monitoring.RestServer.Controllers
{
    [Route("")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private const string ArtifactType_Metrics = "metrics";

        private readonly ILogger<MetricsController> _logger;
        private readonly MetricsStoreService _metricsStore;
        private readonly MetricsOptions _metricsOptions;

        public MetricsController(ILogger<MetricsController> logger,
            IServiceProvider serviceProvider,
            IOptions<MetricsOptions> metricsOptions)
        {
            _logger = logger;
            _metricsStore = serviceProvider.GetService<MetricsStoreService>();
            _metricsOptions = metricsOptions.Value;
        }

        [HttpGet("metrics")]
        public ActionResult Metrics()
        {
            return this.InvokeService(() =>
            {
                if (!_metricsOptions.Enabled)
                {
                    throw new InvalidOperationException("Metrics was not enabled");
                }

                KeyValueLogScope scope = new KeyValueLogScope();
                scope.AddArtifactType(ArtifactType_Metrics);

                return new OutputStreamResult(async (outputStream, token) =>
                    {
                        await _metricsStore.MetricsStore.SnapshotMetrics(outputStream, token);
                    },
                    "text/plain; version=0.0.4",
                    null,
                    scope);
            }, _logger);
        }
    }
}
