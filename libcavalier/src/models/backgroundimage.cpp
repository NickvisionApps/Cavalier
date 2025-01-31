#include "models/backgroundimage.h"

namespace Nickvision::Cavalier::Shared::Models
{
    BackgroundImage::BackgroundImage(const std::filesystem::path& path)
        : m_path{ path },
        m_scale{ 100 },
        m_alpha{ 100 }
    {

    }

    BackgroundImage::BackgroundImage(const std::filesystem::path& path, unsigned int scale, unsigned int alpha)
        : m_path{ path },
        m_scale{ scale },
        m_alpha{ alpha }
    {

    }

    BackgroundImage::BackgroundImage(boost::json::object json)
        : m_path{ json["Path"].is_string() ? std::string(json["Path"].as_string()) : "" },
        m_scale{ json["Scale"].is_uint64() ? json["Scale"].as_uint64() : 100 },
        m_alpha{ json["Alpha"].is_uint64() ? json["Alpha"].as_uint64() : 100 }
    {

    }

    const std::filesystem::path& BackgroundImage::getPath() const
    {
        return m_path;
    }

    void BackgroundImage::setPath(const std::filesystem::path& path)
    {
        m_path = path;
    }

    unsigned int BackgroundImage::getScale() const
    {
        return m_scale;
    }

    void BackgroundImage::setScale(unsigned int scale)
    {
        m_scale = scale;
    }

    unsigned int BackgroundImage::getAlpha() const
    {
        return m_alpha;
    }

    void BackgroundImage::setAlpha(unsigned int alpha)
    {
        m_alpha = alpha;
    }

    boost::json::object BackgroundImage::toJson() const
    {
        boost::json::object obj;
        obj["Path"] = m_path.string();
        obj["Scale"] = m_scale;
        obj["Alpha"] = m_alpha;
        return obj;
    }

    bool BackgroundImage::operator==(const BackgroundImage& compare) const
    {
        return m_path == compare.m_path;
    }

    bool BackgroundImage::operator!=(const BackgroundImage& compare) const
    {
        return !(operator==(compare));
    }

    BackgroundImage::operator bool() const
    {
        return std::filesystem::exists(m_path);
    }
}
