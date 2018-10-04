using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Flows.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Flows.Indexing
{
    public class BagPartIndexHandler : ContentPartIndexHandler<BagPart>
    {
        private readonly IServiceProvider _serviceProvider;

        public BagPartIndexHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task BuildIndexAsync(BagPart bagPart, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions();
            if (options == DocumentIndexOptions.None)
            {
                return;
            }

            if (bagPart.ContentItems.Count != 0)
            {
                // Lazy resolution to prevent cyclic dependency
                var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();

                foreach (var contentItemIndexHandler in contentItemIndexHandlers)
                {
                    foreach (var contentItem in bagPart.ContentItems)
                    {
                        var buildIndexContext = new BuildIndexContext(context.DocumentIndex, contentItem, $"{context.Key}.{contentItem.ContentType}");

                        await contentItemIndexHandler.BuildIndexAsync(buildIndexContext);
                    }
                }
            }
        }
    }
}
