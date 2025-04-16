#ifndef MIRRORMODE_H
#define MIRRORMODE_H

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief Modes for mirroring drawing of audio.
     */
    enum class MirrorMode
    {
        Off,
        Full,
        SplitChannels,
        ReverseFull,
        ReverseSplitChannels
    };
}

#endif //MIRRORMODE_H
