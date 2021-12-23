﻿using CluedIn.Core;
using CluedIn.Core.Connectors;
using CluedIn.Core.DataStore;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CluedIn.Connector.Common.Connectors
{
    public abstract class CommonConnectorBase<TConnector, TClient> : ConnectorBase
        where TConnector : CommonConnectorBase<TConnector, TClient>
    {
        protected readonly TClient _client;
        protected readonly ILogger<TConnector> _logger;

        protected CommonConnectorBase(IConfigurationRepository repository, ILogger<TConnector> logger, TClient client,
            Guid providerId) : base(repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));            

            ProviderId = providerId;
        }

        protected string GetEdgesContainerName(string containerName) => $"{containerName}Edges";

        /// <summary>
        ///     Strip non-alpha numeric characters
        /// </summary>
        public override Task<string> GetValidDataTypeName(ExecutionContext executionContext, Guid providerDefinitionId,
            string name)
        {
            return Task.FromResult(Regex.Replace(name, @"[^A-Za-z0-9]+", ""));
        }

        protected virtual async Task<string> GetValidContainerName(ExecutionContext executionContext, Guid providerDefinitionId,
            string name,
            Func<ExecutionContext, Guid, string, Task<bool>> checkTableExistPredicate)
        {
            // Strip non-alpha numeric characters
            var cleanName = Regex.Replace(name, @"[^A-Za-z0-9]+", "");

            if (!await checkTableExistPredicate(executionContext, providerDefinitionId, cleanName))
                return cleanName;

            // If exists, append count like in windows explorer
            var count = 0;
            string newName;
            do
            {
                count++;
                newName = $"{cleanName}{count}";
            } while (await checkTableExistPredicate(executionContext, providerDefinitionId, newName));

            cleanName = newName;

            return cleanName;
        }
    }
}
