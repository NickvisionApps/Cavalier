#ifndef RENDERER_H
#define RENDERER_H

#include <optional>
#include <vector>
#include <skia/include/core/SkCanvas.h>
#include <skia/include/core/SkRefCnt.h>
#include <skia/include/core/SkShader.h>
#include "backgroundimage.h"
#include "color.h"
#include "colorprofile.h"
#include "drawingarea.h"
#include "drawingfunctionarguments.h"
#include "point.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A renderer of music data.
     */
    class Renderer
    {
    public:
        /**
         * @brief Constructs a Renderer.
         * @param canvas The SkCanvas
         */
        Renderer(SkCanvas* canvas = nullptr);
        /**
         * @brief Gets the SkCanvas.
         * @return The SkCanvas
         */
        SkCanvas* getCanvas() const;
        /**
         * @brief Sets the SkCanvas.
         * @param canvas The new SkCanvas
         */
        void setCanvas(SkCanvas* canvas);
        const DrawingArea& getDrawingArea() const;
        void setDrawingArea(const DrawingArea& area);
        const ColorProfile& getColorProfile() const;
        void setColorProfile(const ColorProfile& profile);
        const std::optional<BackgroundImage>& getBackgroundImage() const;
        void setBackgroundImage(const std::optional<BackgroundImage>& image);
        void draw(const std::vector<float>& sample, float width, float height);

    private:
        sk_sp<SkShader> createBackgroundGradient(const std::vector<Color>& colors, float width, float height, float margin = 0);
        sk_sp<SkShader> createForegroundGradient(const std::vector<Color>& colors, float width, float height, float margin = 0);
        SkCanvas* m_canvas;
        DrawingArea m_drawingArea;
        ColorProfile m_colorProfile;
        std::optional<BackgroundImage> m_backgroundImage;
    };
}

#endif // RENDERER_H
