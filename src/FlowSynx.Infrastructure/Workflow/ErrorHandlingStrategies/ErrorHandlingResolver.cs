﻿using FlowSynx.Application.Features.WorkflowExecutions.Command.ExecuteWorkflow;

namespace FlowSynx.Infrastructure.Workflow.ErrorHandlingStrategies;

public class ErrorHandlingResolver : IErrorHandlingResolver
{
    public void Resolve(WorkflowDefinition definition)
    {
        foreach (var task in definition.Tasks)
        {
            task.ErrorHandling ??= new ErrorHandling();
            task.ErrorHandling = GetEffectiveErrorHandling(task, definition.Configuration);
        }
    }

    private static ErrorHandling? GetEffectiveErrorHandling(WorkflowTask task, WorkflowConfiguration config)
    {
        var defaultHandling = config.ErrorHandling;
        var taskHandling = task.ErrorHandling;

        if (defaultHandling is null && taskHandling is null)
            return null;

        if (defaultHandling is null)
            return taskHandling;

        if (taskHandling is null)
            return defaultHandling;

        return new ErrorHandling
        {
            Strategy = taskHandling.Strategy ?? defaultHandling.Strategy,
            RetryPolicy = MergeRetryPolicy(taskHandling.RetryPolicy, defaultHandling.RetryPolicy ?? new RetryPolicy())
        };
    }

    private static RetryPolicy MergeRetryPolicy(RetryPolicy? taskPolicy, RetryPolicy defaultPolicy)
    {
        if (taskPolicy is null)
            return defaultPolicy;

        return new RetryPolicy
        {
            MaxRetries = IsValid(taskPolicy.MaxRetries) ? taskPolicy.MaxRetries : defaultPolicy.MaxRetries,
            BackoffStrategy = IsDefined(taskPolicy.BackoffStrategy) ? taskPolicy.BackoffStrategy : defaultPolicy.BackoffStrategy,
            InitialDelay = IsPositive(taskPolicy.InitialDelay) ? taskPolicy.InitialDelay : defaultPolicy.InitialDelay,
            MaxDelay = IsPositive(taskPolicy.MaxDelay) ? taskPolicy.MaxDelay : defaultPolicy.MaxDelay,
            BackoffCoefficient = IsPositive(taskPolicy.BackoffCoefficient) ? taskPolicy.BackoffCoefficient : defaultPolicy.BackoffCoefficient
        };
    }

    private static bool IsValid(int value) => value >= 0;
    private static bool IsPositive(int value) => value > 0;
    private static bool IsPositive(double value) => value > 0.0;
    private static bool IsDefined(BackoffStrategy strategy) => Enum.IsDefined(typeof(BackoffStrategy), strategy);
}
