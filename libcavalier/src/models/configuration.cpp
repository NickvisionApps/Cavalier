#include "models/configuration.h"
#include <libnick/system/environment.h>

using namespace Nickvision::App;
using namespace Nickvision::System;

namespace Nickvision::Cavalier::Shared::Models
{
    Configuration::Configuration(const std::string& key, const std::string& appName)
        : DataFileBase{ key, appName }
    {
        
    }

    Theme Configuration::getTheme() const
    {
        return m_json["Theme"].is_int64() ? static_cast<Theme>(m_json["Theme"].as_int64()) : Theme::System;
    }

    void Configuration::setTheme(Theme theme)
    {
        m_json["Theme"] = static_cast<int>(theme);
    }

    WindowGeometry Configuration::getWindowGeometry() const
    {
        WindowGeometry geometry;
        if(!m_json["WindowGeometry"].is_object())
        {
            geometry.setWidth(800);
            geometry.setHeight(600);
            geometry.setIsMaximized(false);
            return geometry;
        }
        boost::json::object& obj{ m_json["WindowGeometry"].as_object() };
        geometry.setWidth(obj["Width"].is_int64() ? static_cast<long>(obj["Width"].as_int64()) : 800);
        geometry.setHeight(obj["Height"].is_int64() ? static_cast<long>(obj["Height"].as_int64()) : 600);
        geometry.setIsMaximized(obj["IsMaximized"].is_bool() ? obj["IsMaximized"].as_bool() : false);
        return geometry;
    }

    void Configuration::setWindowGeometry(const WindowGeometry& geometry)
    {
        boost::json::object obj;
        obj["Width"] = geometry.getWidth();
        obj["Height"] = geometry.getHeight();
        obj["IsMaximized"] = geometry.isMaximized();
        m_json["WindowGeometry"] = obj;
    }

    bool Configuration::getAutomaticallyCheckForUpdates() const
    {
        return m_json["AutomaticallyCheckForUpdates"].is_bool() ? m_json["AutomaticallyCheckForUpdates"].as_bool() : Environment::getOperatingSystem() == OperatingSystem::Windows;
    }

    void Configuration::setAutomaticallyCheckForUpdates(bool check)
    {
        m_json["AutomaticallyCheckForUpdates"] = check;
    }

    DrawingArea Configuration::getDrawingArea() const
    {
        return m_json["DrawingArea"].is_object() ? DrawingArea(m_json["DrawingArea"].as_object()) : DrawingArea();
    }

    void Configuration::setDrawingArea(const DrawingArea& area)
    {
        m_json["DrawingArea"] = area.toJson();
    }

    CavaOptions Configuration::getCavaOptions() const
    {
        return m_json["CavaOptions"].is_object() ? CavaOptions(m_json["CavaOptions"].as_object()) : CavaOptions();
    }

    void Configuration::setCavaOptions(const CavaOptions& cava)
    {
        m_json["CavaOptions"] = cava.toJson();
    }

    std::vector<ColorProfile> Configuration::getColorProfiles() const
    {
        std::vector<ColorProfile> profiles;
        if(m_json["ColorProfiles"].is_array())
        {
            for(const boost::json::value& val : m_json["ColorProfiles"].as_array())
            {
                if(val.is_object())
                {
                    profiles.push_back(val.as_object());
                }
            }
        }
        else
        {
            profiles.push_back({}); //Default ColorProfile
        }
        return profiles;
    }

    void Configuration::setColorProfiles(const std::vector<ColorProfile>& profiles)
    {
        boost::json::array arr;
        for(const ColorProfile& profile : profiles)
        {
            arr.push_back(profile.toJson());
        }
        m_json["ColorProfiles"] = arr;
    }

    std::string Configuration::getActiveColorProfileName() const
    {
        return m_json["ActiveColorProfileName"].is_string() ? std::string(m_json["ActiveColorProfileName"].as_string()) : ColorProfile().getName();
    }

    void Configuration::setActiveColorProfileName(const std::string& name)
    {
        m_json["ActiveColorProfileName"] = name;
    }

    std::vector<BackgroundImage> Configuration::getBackgroundImages() const
    {
        std::vector<BackgroundImage> images;
        if(m_json["BackgroundImages"].is_array())
        {
            for(const boost::json::value& val : m_json["BackgroundImages"].as_array())
            {
                if(val.is_object())
                {
                    BackgroundImage image = val.as_object();
                    if(std::filesystem::exists(image.getPath()))
                    {
                        images.push_back(image);
                    }
                }
            }
        }
        return images;
    }

    void Configuration::setBackgroundImages(const std::vector<BackgroundImage>& images)
    {
        boost::json::array arr;
        for(const BackgroundImage& image : images)
        {
            arr.push_back(image.toJson());
        }
        m_json["BackgroundImages"] = arr;
    }

    std::filesystem::path Configuration::getActiveBackgroundImagePath() const
    {
        return m_json["ActiveBackgroundImagePath"].is_string() && std::filesystem::exists(m_json["ActiveBackgroundImagePath"].as_string().c_str()) ? m_json["ActiveBackgroundImagePath"].as_string().c_str() : "";
    }

    void Configuration::setActiveBackgroundImagePath(const std::filesystem::path& path)
    {
        m_json["ActiveBackgroundImagePath"] = path.string();
    }
}