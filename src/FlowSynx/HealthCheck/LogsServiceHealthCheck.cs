﻿using FlowSynx.Application.Models;
using FlowSynx.Domain.Log;
using FlowSynx.PluginCore.Exceptions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowSynx.HealthCheck;

public class LogsServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<LogsServiceHealthCheck> _logger;
    private readonly ILoggerService _loggerService;

    public LogsServiceHealthCheck(ILogger<LogsServiceHealthCheck> logger,
        ILoggerService loggerService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(loggerService);
        _logger = logger;
        _loggerService = loggerService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthStatus = await _loggerService.CheckHealthAsync();
            if (healthStatus is false)
                throw new FlowSynxException((int)ErrorCode.ApplicationHealthCheck, Resources.LoggerServiceHealthCheckFailed);

            return HealthCheckResult.Healthy(Resources.LoggerServiceHealthCheckLoggerServiceAvailable);
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.ApplicationHealthCheck, 
                $"Error in checking logger service health. Error: {ex.Message}");
            _logger.LogError(errorMessage.ToString());
            return HealthCheckResult.Unhealthy(Resources.LoggerServiceHealthCheckFailed);
        }
    }
}