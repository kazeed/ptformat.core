using Microsoft.Extensions.DependencyInjection;
using Ptformat.Core.Model;
using Ptformat.Core.Parsers.Ptformat.Core.Parsers;
using Ptformat.Core.Parsers;
using Ptformat.Core.Readers;

namespace Ptformat.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all the parser and reader classes for Dependency Injection.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddParsers(this IServiceCollection services)
        {
            // Add all the parsers
            services.AddScoped<IListParser<AudioTrack>, AudioParser>();
            services.AddScoped<IListParser<Region>, MidiRegionParser>();
            services.AddScoped<IListParser<Region>, AudioRegionParser>();
            services.AddScoped<IListParser<Region>, CompoundRegionParser>();
            services.AddScoped<ISingleParser<HeaderInfo>, HeaderParser>();
            services.AddScoped<IListParser<MidiEvent>, MidiEventsParser>();
            services.AddScoped<IListParser<Track>, TrackParser>();

            // Add the utility readers (e.g., XorDecoderReader)
            services.AddScoped<XorDecoderStream>();

            // Add the main session parser
            services.AddScoped<SessionParser>();

            return services;
        }
    }
}
