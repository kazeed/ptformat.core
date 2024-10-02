using System.Collections.Generic;

namespace Ptformat.Core.Model
{
    public class AudioTrack(string name) : Track(name)
    {
        // List of audio file references (WAV files, etc.)
        public List<AudioRef> AudioFiles { get; set; } = [];

        // Method to add an AudioRef to the track
        public void AddAudioFile(AudioRef audioRef)
        {
            AudioFiles.Add(audioRef);
        }
    }
}