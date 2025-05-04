﻿using Microsoft.EntityFrameworkCore;
using FlowSynx.Persistence.Postgres.Contexts;
using FlowSynx.Application.Models;
using FlowSynx.PluginCore.Exceptions;
using Microsoft.Extensions.Logging;
using FlowSynx.Domain.Workflow;

namespace FlowSynx.Persistence.Postgres.Services;

public class WorkflowExecutionService : IWorkflowExecutionService
{
    private readonly IDbContextFactory<ApplicationContext> _appContextFactory;
    private readonly ILogger<WorkflowExecutionService> _logger;

    public WorkflowExecutionService(IDbContextFactory<ApplicationContext> appContextFactory,
        ILogger<WorkflowExecutionService> logger)
    {
        ArgumentNullException.ThrowIfNull(appContextFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _appContextFactory = appContextFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<WorkflowExecutionEntity>> All(string userId, Guid workflowId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            var result = await context.WorkflowExecutions
                .Where(c => c.UserId == userId && c.IsDeleted == false)
                .ToListAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowsGetExecutionList, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task<WorkflowExecutionEntity?> Get(string userId, Guid workflowExecutionId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.WorkflowExecutions
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == workflowExecutionId && x.IsDeleted == false,
                cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowGetExecutionItem, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task<bool> IsExist(string userId, Guid workflowExecutionId, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            var result = await context.WorkflowExecutions
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == workflowExecutionId && x.IsDeleted == false,
                cancellationToken)
                .ConfigureAwait(false);

            return result != null;
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowExecutionCheckExistence, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task Add(WorkflowExecutionEntity workflowExecutionEntity, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            await context.WorkflowExecutions
                .AddAsync(workflowExecutionEntity, cancellationToken)
                .ConfigureAwait(false);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowExecutionAdd, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task Update(WorkflowExecutionEntity workflowExecutionEntity, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            context.Entry(workflowExecutionEntity).State = EntityState.Detached;
            context.WorkflowExecutions.Update(workflowExecutionEntity);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowExecutionUpdate, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task<bool> Delete(WorkflowExecutionEntity workflowExecutionEntity, CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            context.WorkflowExecutions.Remove(workflowExecutionEntity);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = new ErrorMessage((int)ErrorCode.WorkflowExecutionDelete, ex.Message);
            _logger.LogError(errorMessage.ToString());
            throw new FlowSynxException(errorMessage);
        }
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var context = await _appContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}