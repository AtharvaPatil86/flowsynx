﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FlowSynx.Domain.Workflow;

namespace FlowSynx.Persistence.Postgres.Configurations;

public class WorkflowApprovalEntityConfiguration : IEntityTypeConfiguration<WorkflowApprovalEntity>
{
    public void Configure(EntityTypeBuilder<WorkflowApprovalEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(t => t.Id).IsRequired();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.WorkflowId).IsRequired();
        builder.Property(t => t.ExecutionId).IsRequired();
        builder.Property(t => t.TaskName).IsRequired();

        var statusConverter = new ValueConverter<WorkflowApprovalStatus, string>(
            v => v.ToString(),
            v => (WorkflowApprovalStatus)Enum.Parse(typeof(WorkflowApprovalStatus), v, true)
        );

        builder.Property(t => t.Status).IsRequired().HasConversion(statusConverter);
    }
}