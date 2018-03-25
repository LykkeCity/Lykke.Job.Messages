using Autofac.Features.Indexed;
using Lykke.Cqrs;
using Lykke.Job.Messages.Workflow;
using System;

namespace Lykke.Job.Messages.Utils
{
    public static class CqrsEngineRetriever
    {
        public static ICqrsEngine GetEngine(RabbitType type, IIndex<Enum, ICqrsEngine> engineFactory)
        {
            if (!engineFactory.TryGetValue(type, out var cqrsEngine))
                throw new ArgumentException($"There is no registered cqrsEngine for {type}");

            return cqrsEngine;
        }
    }
}
