#ifndef CANVAS_H
#define CANVAS_H

#include <skia/include/core/SkCanvas.h>
#include <skia/include/core/SkRefCnt.h>
#include <skia/include/core/SkSurface.h>

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a skia canvas.
     */
    class Canvas
    {
    public:
        /**
         * @brief Creates a Canvas to use with skia.
         * @param width The width of the canvas
         * @param height The height of the canvas
         */
        Canvas(int width, int height);
        /**
         * @brief Gets the skia surface object.
         * @return SkSurface
         */
        sk_sp<SkSurface> getSkiaSurface() const;
        /**
         * @brief Gets the skia canvas object.
         * @return SkCanvas
         */
        SkCanvas* getSkiaCanvas() const;
        /**
         * @brief Gets the width of the canvas.
         * @return The width of the canvas
         */
        int getWidth() const;
        /**
         * @brief Gets the height of the canvas.
         * @return The height of the canvas
         */
        int getHeight() const;
        /**
         * @brief Gets the backing skia canvas object.
         * @return SkCanvas
         */
        SkCanvas* operator->();

    private:
        sk_sp<SkSurface> m_surface;
        int m_width;
        int m_height;
    };
}

#endif //CANVAS_H
