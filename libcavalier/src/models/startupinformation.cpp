#include "models/startupinformation.h"

namespace Nickvision::Cavalier::Shared::Models
{
    StartupInformation::StartupInformation()
    {

    }

    StartupInformation::StartupInformation(const Nickvision::App::WindowGeometry& windowGeometry)
        : m_windowGeometry{ windowGeometry }
    {

    }

    const Nickvision::App::WindowGeometry& StartupInformation::getWindowGeometry() const
    {
        return m_windowGeometry;
    }

    void StartupInformation::setWindowGeometry(const Nickvision::App::WindowGeometry& windowGeometry)
    {
        m_windowGeometry = windowGeometry;
    }
}