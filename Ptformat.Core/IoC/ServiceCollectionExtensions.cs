using Microsoft.Extensions.DependencyInjection;
using PtInfo.Core.Model;
using PtInfo.Core.Parsers.Ptformat.Core.Parsers;
using PtInfo.Core.Parsers;
using PtInfo.Core.Readers;

namespace PtInfo.Core.Extensions
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
