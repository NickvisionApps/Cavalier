#ifndef STARTUPINFORMATION_H
#define STARTUPINFORMATION_H

#include <libnick/app/windowgeometry.h>

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model for the startup information of the application.
     */
    class StartupInformation
    {
    public:
        /**
         * @brief Constructs a StartupInformation.
         */
        StartupInformation();
        /**
         * @brief Constructs a StartupInformation.
         * @param windowGeometry The window geometry
         */
        StartupInformation(const Nickvision::App::WindowGeometry& windowGeometry);
        /**
         * @brief Gets the window geometry.
         * @return The window geometry
         */
        const Nickvision::App::WindowGeometry& getWindowGeometry() const;
        /**
         * @brief Sets the window geometry.
         * @param windowGeometry The window geometry to set
         */
        void setWindowGeometry(const Nickvision::App::WindowGeometry& windowGeometry);

    private:
        Nickvision::App::WindowGeometry m_windowGeometry;
    };
}

#endif //STARTUPINFORMATION_H