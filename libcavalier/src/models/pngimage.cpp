#include "models/pngimage.h"

namespace Nickvision::Cavalier::Shared::Models
{
    PngImage::PngImage(int width, int height, const std::uint8_t* bytes, size_t size)
        : m_width{ width },
        m_height{ height },
        m_bytes{ bytes, bytes + size }
    {

    }

    int PngImage::getWidth() const
    {
        return m_width;
    }

    int PngImage::getHeight() const
    {
        return m_height;
    }

    const std::vector<std::uint8_t>& PngImage::getBytes() const
    {
        return m_bytes;
    }
}
