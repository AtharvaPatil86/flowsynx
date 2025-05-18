﻿using FlowSynx.Application.Features.WorkflowExecutions.Command.ExecuteWorkflow;
using FlowSynx.Application.Localizations;
using FlowSynx.Infrastructure.Workflow.BackoffStrategies;
using Microsoft.Extensions.Logging;

namespace FlowSynx.Infrastructure.Workflow.ErrorHandlingStrategies;

public class ErrorHandlingStrategyFactory: IErrorHandlingStrategyFactory
{
    private readonly ILogger<ErrorHandlingStrategyFactory> _logger;
    private readonly ILocalization _localization;

    public ErrorHandlingStrategyFactory(
        ILogger<ErrorHandlingStrategyFactory> logger,
        ILocalization localization)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(localization);
        _logger = logger;
        _localization = localization;
    }

    public IErrorHandlingStrategy Create(ErrorHandling? errorHandling)
    {
        return errorHandling?.Strategy switch
        {
            ErrorStrategy.Retry => CreateRetry(errorHandling.RetryPolicy),
            ErrorStrategy.Skip => new SkipStrategy(_logger),
            ErrorStrategy.Abort => new AbortStrategy(_logger),
            _ => throw new ArgumentException(_localization.Get("Workflow_ErrorHandlingStratgeyFactory_UnknownErrorHandlingStrategy", errorHandling?.Strategy))
        };
    }

    private IErrorHandlingStrategy CreateRetry(RetryPolicy? retryPolicy)
    {
        IBackoffStrategy backoff = retryPolicy?.BackoffStrategy switch
        {
            BackoffStrategy.Exponential => new ExponentialBackoffStrategy(
                retryPolicy.InitialDelay,
                retryPolicy.BackoffCoefficient
            ),
            BackoffStrategy.Linear => new LinearBackoffStrategy(
                retryPolicy.InitialDelay
            ),
            BackoffStrategy.Jitter => new JitterBackoffStrategy(
                retryPolicy.InitialDelay,
                retryPolicy.BackoffCoefficient
            ),
            BackoffStrategy.Fixed => new FixedBackoffStrategy(
                retryPolicy.InitialDelay
            ),
            _ => throw new ArgumentException(_localization.Get("Workflow_ErrorHandlingStratgeyFactory_UnknownBackkoffStrategyType", retryPolicy?.BackoffStrategy))
        };

        return new RetryStrategy(retryPolicy.MaxRetries, backoff, _logger);
    }
}
