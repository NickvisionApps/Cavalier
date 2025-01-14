#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <string>
#include <vector>
#include <libnick/app/datafilebase.h>
#include <libnick/app/windowgeometry.h>
#include "cavaoptions.h"
#include "colorprofile.h"
#include "drawingarea.h"
#include "theme.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model for the configuration of the application.
     */
    class Configuration : public Nickvision::App::DataFileBase
    {
    public:
        /**
         * @brief Constructs a Configuration.
         * @param key The key to pass to the DataFileBase
         * @param appName The appName to pass to the DataFileBase
         */
        Configuration(const std::string& key, const std::string& appName);
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Theme getTheme() const;
        /**
         * @brief Sets the preferred theme for the application.
         * @param theme The new preferred theme
         */
        void setTheme(Theme theme);
        /**
         * @brief Gets the window geometry for the application.
         * @return The window geometry
         */
        App::WindowGeometry getWindowGeometry() const;
        /**
         * @brief Sets the window geometry for the application.
         * @param geometry The new window geometry
         */
        void setWindowGeometry(const App::WindowGeometry& geometry);
        /**
         * @brief Gets whether or not to automatically check for application updates.
         * @return True to automatically check for updates, else false
         */
        bool getAutomaticallyCheckForUpdates() const;
        /**
         * @brief Sets whether or not to automatically check for application updates.
         * @param check Whether or not to automatically check for updates
         */
        void setAutomaticallyCheckForUpdates(bool check);
        /**
         * @brief Gets the drawing area for the application.
         * @return The drawing area
         */
        DrawingArea getDrawingArea() const;
        /**
         * @brief Sets the drawing area for the application.
         * @param area The new drawing area
         */
        void setDrawingArea(const DrawingArea& area);
        /**
         * @brief Gets the cava options for the application.
         * @return The cava options
         */
        CavaOptions getCavaOptions() const;
        /**
         * @brief Sets the cava options for the application.
         * @param cava The new cava options
         */
        void setCavaOptions(const CavaOptions& cava);
        /**
         * @brief Gets the color profiles for the application.
         * @return The list of color profiles
         */
        std::vector<ColorProfile> getColorProfiles() const;
        /**
         * @brief Sets the color profiles for the application.
         * @param profiles The new list of color profiles
         */
        void setColorProfiles(const std::vector<ColorProfile>& profiles);
    };
}

#endif //CONFIGURATION_H
