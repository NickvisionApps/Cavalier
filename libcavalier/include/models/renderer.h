#ifndef RENDERER_H
#define RENDERER_H

#include <optional>
#include <vector>
#include <skia/include/core/SkCanvas.h>
#include <skia/include/core/SkPath.h>
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
        /**
         * @brief Gets the drawing area options.
         * @return The drawing area options
         */
        const DrawingArea& getDrawingArea() const;
        /**
         * @brief Sets the drawing area options.
         * @param area The new drawing area options
         */
        void setDrawingArea(const DrawingArea& area);
        /**
         * @brief Gets the active color profile.
         * @return The active color profile
         */
        const ColorProfile& getColorProfile() const;
        /**
         * @brief Sets the active color profile.
         * @param profile The new active color profile
         */
        void setColorProfile(const ColorProfile& profile);
        /**
         * @brief Gets the active background image.
         * @return The active backgrouind image
         */
        const std::optional<BackgroundImage>& getBackgroundImage() const;
        /**
         * @brief Sets the active background image.
         * @param image The new active background image
         */
        void setBackgroundImage(const std::optional<BackgroundImage>& image);
        /**
         * @brief Renders and draws the sample on the canvas.
         * @param sample The cava sample
         * @param width The width of the canvas
         * @param height The height of the canvas
         */
        void draw(const std::vector<float>& sample, float width, float height);

    private:
        float getMirrorWidth(float width);
        float getMirrorHeight(float height);
        float getMirrorX(float x);
        float getMirrorY(float y);
        sk_sp<SkShader> createBackgroundGradient(const std::vector<Color>& colors, float width, float height);
        sk_sp<SkShader> createForegroundGradient(const std::vector<Color>& colors, float width, float height);
        void drawWave(const DrawingFunctionArguments& args);
        void drawLevels(const DrawingFunctionArguments& args);
        void drawParticles(const DrawingFunctionArguments& args);
        void drawBars(const DrawingFunctionArguments& args);
        void drawSpine(const DrawingFunctionArguments& args);
        void drawSplitter(const DrawingFunctionArguments& args);
        void drawHearts(const DrawingFunctionArguments& args);
        SkCanvas* m_canvas;
        DrawingArea m_drawingArea;
        ColorProfile m_colorProfile;
        std::optional<BackgroundImage> m_backgroundImage;
    };
}

#endif // RENDERER_H
