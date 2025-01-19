#ifndef PREFERENCESVIEWCONTROLLER_H
#define PREFERENCESVIEWCONTROLLER_H

#include <string>
#include "models/configuration.h"

namespace Nickvision::Cavalier::Shared::Controllers
{
    /**
     * @brief A controller for a PreferencesView.
     */
    class PreferencesViewController
    {
    public:
        /**
         * @brief Constructs a PreferencesViewController.
         * @param configuration The reference to the configuration to use
         */
        PreferencesViewController(Models::Configuration& configuration);
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Models::Theme getTheme() const;
        /**
         * @brief Sets the preferred theme for the application.
         * @param theme The new preferred theme
         */
        void setTheme(Models::Theme theme);
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
        Models::DrawingArea getDrawingArea() const;
        /**
         * @brief Sets the drawing area for the application.
         * @param area The new drawing area
         */
        void setDrawingArea(const Models::DrawingArea& area);
        /**
         * @brief Gets the cava options for the application.
         * @return The cava options
         */
        Models::CavaOptions getCavaOptions() const;
        /**
         * @brief Sets the cava options for the application.
         * @param cava The new cava options
         */
        void setCavaOptions(const Models::CavaOptions& cava);
        /**
         * @brief Gets the color profiles for the application.
         * @return The list of color profiles
         */
        std::vector<Models::ColorProfile> getColorProfiles() const;
        /**
         * @brief Sets the color profiles for the application.
         * @param profiles The new list of color profiles
         */
        void setColorProfiles(const std::vector<Models::ColorProfile>& profiles);
        /**
         * @brief Gets the name of the active color profile for the application.
         * @return The name of the active color profile
         */
        std::string getActiveColorProfileName() const;
        /**
         * @brief Sets the name of the active color profile for the application..
         * @param name The new name of the active color profile
         */
        void setActiveColorProfileName(const std::string& name);
        /**
         * @brief Gets the background images for the application.
         * @return The list of background images
         */
        std::vector<Models::BackgroundImage> getBackgroundImages() const;
        /**
         * @brief Sets the background images for the application.
         * @param images The new lsit of background images
         */
        void setBackgroundImages(const std::vector<Models::BackgroundImage>& images);
        /**
         * @brief Gets the path of the active background image for the application.
         * @return The path of the active background image
         */
        std::filesystem::path getActiveBackgroundImagePath() const;
        /**
         * @brief Sets the path of the active background image for the application..
         * @param path The new path of the active background image
         */
        void setActiveBackgroundImagePath(const std::filesystem::path& path);
        /**
         * @brief Saves the current configuration to disk.
         */
        void saveConfiguration();

    private:
        Models::Configuration& m_configuration;
    };
}

#endif //PREFERENCESVIEWCONTROLLER_H
