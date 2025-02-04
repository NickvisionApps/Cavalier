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

    int PreferencesViewController::getActiveColorProfileIndex() const
    {
        return m_configuration.getActiveColorProfileIndex();
    }

    void PreferencesViewController::setActiveColorProfileIndex(int index)
    {
        m_configuration.setActiveColorProfileIndex(index);
    }

    std::vector<BackgroundImage> PreferencesViewController::getBackgroundImages() const
    {
        return m_configuration.getBackgroundImages();
    }

    void PreferencesViewController::setBackgroundImages(const std::vector<BackgroundImage>& images)
    {
        m_configuration.setBackgroundImages(images);
    }

    int PreferencesViewController::getActiveBackgroundImageIndex() const
    {
        return m_configuration.getActiveBackgroundImageIndex();
    }

    void PreferencesViewController::setActiveBackgroundImageIndex(int index)
    {
        m_configuration.setActiveBackgroundImageIndex(index);
    }

    void PreferencesViewController::saveConfiguration()
    {
        m_configuration.save();
    }
}
