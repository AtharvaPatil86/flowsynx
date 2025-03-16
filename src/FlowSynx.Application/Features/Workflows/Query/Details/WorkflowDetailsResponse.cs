﻿namespace FlowSynx.Application.Features.Workflows.Query.Details;

public class WorkflowDetailsResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Workflow { get; set; }
}