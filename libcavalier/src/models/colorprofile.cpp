#include "models/colorprofile.h"
#include <algorithm>
#include <libnick/localization/gettext.h>

#define DEFAULT_FOREGROUND "#3584e4"
#define DEFAULT_BACKGROUND "#242424"

namespace Nickvision::Cavalier::Shared::Models
{
    ColorProfile::ColorProfile()
        : m_name{ _("Default") },
        m_appTheme{ Theme::System }
    {
        m_foregroundColors.push_back({ DEFAULT_FOREGROUND });
        m_backgroundColors.push_back({ DEFAULT_BACKGROUND });
    }

    ColorProfile::ColorProfile(const std::string& name)
        : m_name{ name },
        m_appTheme{ Theme::System }
    {
        m_foregroundColors.push_back({ DEFAULT_FOREGROUND });
        m_backgroundColors.push_back({ DEFAULT_BACKGROUND });
    }

    ColorProfile::ColorProfile(boost::json::object json)
        : m_name{ json["Name"].is_string() ? json["Name"].as_string() : _("Default") },
        m_appTheme{ json["ApplicationTheme"].is_int64() ? static_cast<Theme>(json["ApplicationTheme"].as_int64()) : Theme::System }
    {
        if(json["ForegroundColors"].is_array())
        {
            for(const boost::json::value& val : json["ForegroundColors"].as_array())
            {
                if(val.is_object())
                {
                    m_foregroundColors.push_back(val.as_object());
                }
            }
        }
        else
        {
            m_foregroundColors.push_back({ DEFAULT_FOREGROUND });
        }
        if(json["BackgroundColors"].is_array())
        {
            for(const boost::json::value& val : json["BackgroundColors"].as_array())
            {
                if(val.is_object())
                {
                    m_backgroundColors.push_back(val.as_object());
                }
            }
        }
        else
        {
            m_backgroundColors.push_back({ DEFAULT_BACKGROUND });
        }
    }

    const std::string& ColorProfile::getName() const
    {
        return m_name;
    }

    void ColorProfile::setName(const std::string& name)
    {
        m_name = name;
    }

    Theme ColorProfile::getApplicationTheme() const
    {
        return m_appTheme;
    }

    void ColorProfile::setApplicationTheme(Theme theme)
    {
        m_appTheme = theme;
    }

    const std::vector<Color>& ColorProfile::getForegroundColors() const
    {
        return m_foregroundColors;
    }

    bool ColorProfile::addForegroundColor(const Color& color)
    {
        if(std::find(m_foregroundColors.begin(), m_foregroundColors.end(), color) != m_foregroundColors.end())
        {
            return false;
        }
        m_foregroundColors.push_back(color);
        return true;
    }

    bool ColorProfile::removeForegroundColor(const Color& color)
    {
        std::vector<Color>::iterator it{ std::find(m_foregroundColors.begin(), m_foregroundColors.end(), color) };
        if(it == m_foregroundColors.end())
        {
            return false;
        }
        m_foregroundColors.erase(it);
        return true;
    }

    bool ColorProfile::resetForegroundColors()
    {
        m_foregroundColors.clear();
        m_foregroundColors.push_back({ DEFAULT_FOREGROUND });
        return true;
    }

    const std::vector<Color>& ColorProfile::getBackgroundColors() const
    {
        return m_backgroundColors;
    }

    bool ColorProfile::addBackgroundColor(const Color& color)
    {
        if(std::find(m_backgroundColors.begin(), m_backgroundColors.end(), color) != m_backgroundColors.end())
        {
            return false;
        }
        m_backgroundColors.push_back(color);
        return true;
    }

    bool ColorProfile::removeBackgroundColor(const Color& color)
    {
        std::vector<Color>::iterator it{ std::find(m_backgroundColors.begin(), m_backgroundColors.end(), color) };
        if(it == m_backgroundColors.end())
        {
            return false;
        }
        m_backgroundColors.erase(it);
        return true;
    }

    bool ColorProfile::resetBackgroundColors()
    {
        m_backgroundColors.clear();
        m_backgroundColors.push_back({ DEFAULT_BACKGROUND });
        return true;
    }

    boost::json::object ColorProfile::toJson() const
    {
        boost::json::object obj;
        boost::json::array foregroundColors;
        boost::json::array backgroundColors;
        for(const Color& color : m_foregroundColors)
        {
            foregroundColors.push_back(color.toJson());
        }
        for(const Color& color : m_backgroundColors)
        {
            backgroundColors.push_back(color.toJson());
        }
        obj["Name"] = m_name;
        obj["ApplicationTheme"] = static_cast<int>(m_appTheme);
        obj["ForegroundColors"] = foregroundColors;
        obj["BackgroundColors"] = backgroundColors;
        return obj;
    }

    bool ColorProfile::operator==(const ColorProfile& compare) const
    {
        return m_name == compare.m_name;
    }

    bool ColorProfile::operator!=(const ColorProfile& compare) const
    {
        return !(operator==(compare));
    }
}
