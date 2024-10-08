﻿namespace PtInfo.Core.Model
{
    public enum ContentType
    {
        InfoProductVersion = 0x0030,
        WavSampleRateSize = 0x1001,
        WavMetadata = 0x1003,
        WavListFull = 0x1004,
        RegionNameNumber = 0x1007,
        AudioRegionNameNumberV5 = 0x1008,
        AudioRegionListV5 = 0x100B,
        AudioRegionTrackEntry = 0x100F,
        AudioRegionTrackMapEntries = 0x1011,
        AudioRegionTrackFullMap = 0x1012,
        AudioTrackNameNumber = 0x1014,
        AudioTracks = 0x1015,
        PluginEntry = 0x1017,
        PluginFullList = 0x1018,
        IOChannelEntry = 0x1021,
        IOChannelList = 0x1022,
        InfoSampleRate = 0x1028,
        WavNames = 0x103A,
        AudioRegionTrackSubentryV8 = 0x104F,
        AudioRegionTrackEntryV8 = 0x1050,
        AudioRegionTrackMapEntriesV8 = 0x1052,
        AudioRegionTrackFullMapV8 = 0x1054,
        MidiRegionTrackEntry = 0x1056,
        MidiRegionTrackMapEntries = 0x1057,
        MidiRegionTrackFullMap = 0x1058,
        MidiEventsBlock = 0x2000,
        MidiRegionNameNumberV5 = 0x2001,
        MidiRegionsMapV5 = 0x2002,
        InfoPathOfSession = 0x2067,
        SnapsBlock = 0x2511,
        MidiTrackFullList = 0x2619,
        MidiTrackNameNumber = 0x261A,
        CompoundRegionElement = 0x2623,
        IORoute = 0x2602,
        IORoutingTable = 0x2603,
        CompoundRegionGroup = 0x2628,
        AudioRegionNameNumberV10 = 0x2629,
        AudioRegionListV10 = 0x262A,
        CompoundRegionFullMap = 0x262C,
        MidiRegionsNameNumberV10 = 0x2633,
        MidiRegionsMapV10 = 0x2634,
        MarkerList = 0x271A,
        Invalid = -1, // Assigning -1 as a default for unknown types
    }
}
