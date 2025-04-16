#ifndef BACKGROUNDIMAGE_H
#define BACKGROUNDIMAGE_H

#include <filesystem>
#include <boost/json.hpp>

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a background image to use within the application.
     */
    class BackgroundImage
    {
    public:
        /**
         * @brief Constructs a BackgroundImage.
         * @param path The path of the background image
         */
        BackgroundImage(const std::filesystem::path& path);
        /**
         * @brief Constructs a BackgroundImage.
         * @param path The path of the background image
         * @param scale The scale of the background image (0 - 100 %)
         * @param alpha The alpha of the background image (0 - 100 %)
         */
        BackgroundImage(const std::filesystem::path& path, unsigned int scale, unsigned int alpha);
        /**
         * @brief Constructs a BackgroundImage from a json object.
         * @param json The json object
         */
        BackgroundImage(boost::json::object json);
        /**
         * @brief Gets the path of the background image.
         * @return The background image's path
         */
        const std::filesystem::path& getPath() const;
        /**
         * @brief Sets the path of the background image.
         * @param path The new background image's path
         */
        void setPath(const std::filesystem::path& path);
        /**
         * @brief Gets the scale of the background image.
         * @return The background image's scale
         */
        unsigned int getScale() const;
        /**
         * @brief Sets the scale of the background image.
         * @param scale The new backgroud image's scale
         */
        void setScale(unsigned int scale);
        /**
         * @brief Gets the alpha of the background image.
         * @return The background image's alpha
         */
        unsigned int getAlpha() const;
        /**
         * @brief Sets the alpha of the background image.
         * @param alpha The new backgroud image's alpha
         */
        void setAlpha(unsigned int alpha);
        /**
         * @brief Converts the DrawingArea to a JSON object.
         * @return The JSON object
         */
        boost::json::object toJson() const;
        /**
         * @brief Gets whether or not this BackgroundImage is equal to compare BackgroundImage.
         * @param compare The BackgroundImage to compare to
         * @return True if this BackgroundImage == compare BackgroundImage
         */
        bool operator==(const BackgroundImage& compare) const;
        /**
         * @brief Gets whether or not this BackgroundImage is not equal to compare BackgroundImage.
         * @param compare The BackgroundImage to compare to
         * @return True if this BackgroundImage != compare BackgroundImage
         */
        bool operator!=(const BackgroundImage& compare) const;
        /**
         * @brief Gets whether or not the image's path exists.
         * @return True if path exists
         * @return False if path does not exist
         */
        operator bool() const;


    private:
        std::filesystem::path m_path;
        unsigned int m_scale;
        unsigned int m_alpha;
    };
}

#endif //BACKGROUNDIMAGE_H
