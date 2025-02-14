#ifndef CANVAS_H
#define CANVAS_H

#include <GLFW/glfw3.h>
#include <skia/include/core/SkCanvas.h>
#include <skia/include/core/SkRefCnt.h>
#include <skia/include/core/SkSurface.h>
#include <skia/include/gpu/ganesh/GrDirectContext.h>

#define DEFAULT_CANVAS_WIDTH 800
#define DEFAULT_CANVAS_HEIGHT 600

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a skia canvas.
     */
    class Canvas
    {
    public:
        /**
         * @brief Constructs a Canvas.
         * @param width The width of the canvas
         * @param height The height of the canvas
         */
        Canvas(int width, int height);
        /**
         * @brief Destructs a Canvas.
         */
        ~Canvas();
        /**
         * @brief Gets whether or not the Canvas is properly initalized.
         * @return True if properly initalized
         * @return False if not properly initalized
         */
        bool isValid() const;
        /**
         * @brief Gets whether or not the canvas is using the GPU.
         * @return True if using GPU
         * @return False if using CPU
         */
        bool isGPUCanvas() const;
        /**
         * @brief Gets the skia surface object.
         * @return SkSurface
         */
        const sk_sp<SkSurface>& getSkiaSurface() const;
        /**
         * @brief Gets the skia canvas object.
         * @return SkCanvas
         */
        SkCanvas* getSkiaCanvas() const;
        /**
         * @brief Gets the skia gpu context object.
         * @return GrDirectContext
         */
        GrDirectContext* getSkiaContext() const;
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
         * @brief Flushes the canvas.
         */
        void flush();
        /**
         * @brief Gets the backing skia canvas object.
         * @return SkCanvas
         */
        SkCanvas* operator->();
        /**
         * @brief Gets whether or not the Canvas is properly initalized.
         * @return True if properly initalized
         * @return False if not properly initalized
         */
        operator bool() const;

    private:
        GLFWwindow* m_glfw;
        sk_sp<SkSurface> m_surface;
        sk_sp<GrDirectContext> m_context;
        bool m_isGPUCanvas;
        int m_width;
        int m_height;
    };
}

#endif //CANVAS_H
