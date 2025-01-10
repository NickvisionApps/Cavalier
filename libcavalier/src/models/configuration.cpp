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
        if(m_json["DrawingArea"].is_object())
        {
            return m_json["DrawingArea"].as_object();
        }
        return {};
    }

    void Configuration::setDrawingArea(const DrawingArea& area)
    {
        m_json["DrawingArea"] = area.toJson();
    }

    CavaOptions Configuration::getCavaOptions() const
    {
        if(m_json["CavaOptions"].is_object())
        {
            return m_json["CavaOptions"].as_object();
        }
        return {};
    }

    void Configuration::setCavaOptions(const CavaOptions& cava)
    {
        m_json["CavaOptions"] = cava.toJson();
    }
}
