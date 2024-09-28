namespace Ptformat.Core
{
    public static class Constants
    {
        private static string InvalidPTFile => "Invalid PT file";
        private static char ZMark => '\x5a';
        private static long ZeroTicks => 0xe8d4a51000L;
        private static int MaxContentType => 0x3000;
        private static int MaxChanelsPerTrack => 8;
        private static int ContentTypeSessionRate => 0x1028;
        private static int ContentTypeAudioBlock => 0x1004;
        private static int ContentTypeAudioFile => 0x103a;
        public static byte[] BitCode1 => [0x2F, 0x2B];
    }
}
