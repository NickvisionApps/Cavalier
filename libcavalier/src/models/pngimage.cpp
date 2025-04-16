#include "models/pngimage.h"
#include <fstream>

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

    bool PngImage::save(const std::filesystem::path& file) const
    {
        if(file.extension() != ".png" && file.extension() != ".PNG")
        {
            return false;
        }
        std::ofstream out{ file, std::ios::trunc | std::ios::binary };
        out.write(reinterpret_cast<const char*>(&m_bytes[0]), m_bytes.size());
        return true;
    }
}
