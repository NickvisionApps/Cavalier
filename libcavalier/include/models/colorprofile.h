#ifndef COLORPROFILE_H
#define COLORPROFILE_H

#include <string>
#include <vector>
#include <boost/json.hpp>
#include "color.h"
#include "theme.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a color profile.
     */
    class ColorProfile
    {
    public:
        /**
         * @brief Constructs a ColorProfile.
         * @note The name of the profile will be "Default" localized.
         */
        ColorProfile();
        /**
         * @brief Constructs a ColorProfile.
         * @param name The name of the profile
         */
        ColorProfile(const std::string& name);
        /**
         * @brief Constructs a ColorProfile from a json object.
         * @param json The json object
         */
        ColorProfile(boost::json::object json);
        /**
         * @brief Gets the name of the color profile.
         * @return The color profile's name
         */
        const std::string& getName() const;
        /**
         * @brief Sets the name of the color profile.
         * @param name The color profile's new name
         */
        void setName(const std::string& name);
        /**
         * @brief Gets the application theme of the color profile.
         * @return The color profile's application theme
         */
        Theme getApplicationTheme() const;
        /**
         * @brief Sets the application theme of the color profile.
         * @param theme The color profile's new application theme
         */
        void setApplicationTheme(Theme theme);
        /**
         * @brief Gets the foreground colors of the color profile.
         * @return The color profile's foreground colors
         */
        const std::vector<Color>& getForegroundColors() const;
        /**
         * @brief Adds a foreground color to the color profile.
         * @param color The new foreground color
         * @return True if added
         * @return False if not added (color already added as foreground color)
         */
        bool addForegroundColor(const Color& color);
        /**
         * @brief Removes a foreground color from the color profile.
         * @param color The foreground color to remove
         * @return True if removed
         * @return False is not removed (color has not been added as foreground color)
         */
        bool removeForegroundColor(const Color& color);
        /**
         * @brief Removes all added foreground colors and adds the default foreground color to the color profile.
         * @return True if foreground colors reset
         * @return False if foreground colors not reset
         */
        bool resetForegroundColors();
        /**
         * @brief Gets the background colors of the color profile.
         * @return The color profile's background colors
         */
        const std::vector<Color>& getBackgroundColors() const;
        /**
         * @brief Adds a background color to the color profile.
         * @param color The new background color
         * @return True if added
         * @return False if not added (color already added as background color)
         */
        bool addBackgroundColor(const Color& color);
        /**
         * @brief Removes a background color from the color profile.
         * @param color The background color to remove
         * @return True if removed
         * @return False is not removed (color has not been added as background color)
         */
        bool removeBackgroundColor(const Color& color);
        /**
         * @brief Removes all added background colors and adds the default background color to the color profile.
         * @return True if background colors reset
         * @return False if background colors not reset
         */
        bool resetBackgroundColors();
        /**
         * @brief Converts the ColorProfile to a JSON object.
         * @return The JSON object
         */
        boost::json::object toJson() const;

    private:
        std::string m_name;
        Theme m_appTheme;
        std::vector<Color> m_foregroundColors;
        std::vector<Color> m_backgroundColors;
    };
}

#endif //COLORPROFILE_H
