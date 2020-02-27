﻿using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EventStore.Core.Authorization {
	public interface IPolicyEvaluator {
		ValueTask<EvaluationResult> EvaluateAsync(ClaimsPrincipal cp, Operation operation,
			CancellationToken cancellationToken);
	}
}
