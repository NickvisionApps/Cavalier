#ifndef PNGIMAGE_H
#define PNGIMAGE_H

#include <cstdint>
#include <vector>

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a png image.
     */
    class PngImage
    {
    public:
        /**
         * @brief Creates a PngImage.
         * @param width The width of the image
         * @param height The height of the image
         * @param bytes The bytes of the image
         * @param size The size of the bytes of the image
         */
        PngImage(int width, int height, const std::uint8_t* bytes, size_t size);
        /**
         * @brief Gets the width of the image.
         * @return The image width
         */
        int getWidth() const;
        /**
         * @brief Gets the height of the image.
         * @return The image height
         */
        int getHeight() const;
        /**
         * @brief Gets the bytes of the image.
         * @return The image bytes
         */
        const std::vector<std::uint8_t>& getBytes() const;

    private:
        int m_width;
        int m_height;
        std::vector<std::uint8_t> m_bytes;
    };
}

#endif //PNGIMAGE_H
