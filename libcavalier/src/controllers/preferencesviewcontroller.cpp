#include "controllers/preferencesviewcontroller.h"

using namespace Nickvision::Cavalier::Shared::Models;

namespace Nickvision::Cavalier::Shared::Controllers
{
    PreferencesViewController::PreferencesViewController(Configuration& configuration)
        : m_configuration{ configuration }
    {

    }
    
    Theme PreferencesViewController::getTheme() const
    {
        return m_configuration.getTheme();
    }

    void PreferencesViewController::setTheme(Theme theme)
    {
        m_configuration.setTheme(theme);
    }

    bool PreferencesViewController::getAutomaticallyCheckForUpdates() const
    {
        return m_configuration.getAutomaticallyCheckForUpdates();
    }

    void PreferencesViewController::setAutomaticallyCheckForUpdates(bool check)
    {
        m_configuration.setAutomaticallyCheckForUpdates(check);
    }

    DrawingArea PreferencesViewController::getDrawingArea() const
    {
        return m_configuration.getDrawingArea();
    }

    void PreferencesViewController::setDrawingArea(const DrawingArea& area)
    {
        m_configuration.setDrawingArea(area);
    }

    CavaOptions PreferencesViewController::getCavaOptions() const
    {
        return m_configuration.getCavaOptions();
    }

    void PreferencesViewController::setCavaOptions(const CavaOptions& cava)
    {
        m_configuration.setCavaOptions(cava);
    }

    std::vector<ColorProfile> PreferencesViewController::getColorProfiles() const
    {
        return m_configuration.getColorProfiles();
    }

    void PreferencesViewController::setColorProfiles(const std::vector<ColorProfile>& profiles)
    {
        m_configuration.setColorProfiles(profiles);
    }

    std::string PreferencesViewController::getActiveColorProfileName() const
    {
        return m_configuration.getActiveColorProfileName();
    }

    void PreferencesViewController::setActiveColorProfileName(const std::string& name)
    {
        m_configuration.setActiveColorProfileName(name);
    }

    std::vector<BackgroundImage> PreferencesViewController::getBackgroundImages() const
    {
        return m_configuration.getBackgroundImages();
    }

    void PreferencesViewController::setBackgroundImages(const std::vector<BackgroundImage>& images)
    {
        m_configuration.setBackgroundImages(images);
    }

    std::filesystem::path PreferencesViewController::getActiveBackgroundImagePath() const
    {
        return m_configuration.getActiveBackgroundImagePath();
    }

    void PreferencesViewController::setActiveBackgroundImagePath(const std::filesystem::path& path)
    {
        m_configuration.setActiveBackgroundImagePath(path);
    }

    void PreferencesViewController::saveConfiguration()
    {
        m_configuration.save();
    }
}
