﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;

namespace Elsa.Activities.ControlFlow
{
    public class Switch : Activity
    {
        private readonly IWorkflowExpressionEvaluator expressionEvaluator;
        
        public Switch(IWorkflowExpressionEvaluator expressionEvaluator)
        {
            this.expressionEvaluator = expressionEvaluator;
            Cases = new List<string>();
        }
        
        public WorkflowExpression<string> Expression
        {
            get => GetState<WorkflowExpression<string>>();
            set => SetState(value);
        }

        public IReadOnlyCollection<string> Cases
        {
            get => GetState<IReadOnlyCollection<string>>();
            set => SetState(value);
        }
        
        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext workflowContext, CancellationToken cancellationToken)
        {
            var result = await expressionEvaluator.EvaluateAsync(Expression, workflowContext, cancellationToken);
            return Outcome(result);
        }
    }
}