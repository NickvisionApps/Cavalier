#ifndef RENDERER_H
#define RENDERER_H

#include <mutex>
#include <optional>
#include <vector>
#include <skia/include/core/SkPath.h>
#include <skia/include/core/SkRefCnt.h>
#include <skia/include/core/SkShader.h>
#include "backgroundimage.h"
#include "canvas.h"
#include "color.h"
#include "colorprofile.h"
#include "drawingarea.h"
#include "drawingfunctionarguments.h"
#include "pngimage.h"
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
         * @param canvas The optional canvas to render too
         */
        Renderer(const std::optional<Canvas>& canvas = std::nullopt);
        /**
         * @brief Constructs a Renderer.
         * @param drawingArea The drawing area options
         * @param colorProfile The active color profile
         * @param backgroundImage The optional background image to render
         * @param canvas The optional canvas to render too
         */
        Renderer(const DrawingArea& drawingArea, const ColorProfile& colorProfile, const std::optional<BackgroundImage>& backgroundImage = std::nullopt, const std::optional<Canvas>& canvas = std::nullopt);
        /**
         * @brief Gets the canvas.
         * @return The canvas
         */
        const std::optional<Canvas>& getCanvas() const;
        /**
         * @brief Sets the canvas.
         * @param canvas The new canvas
         */
        void setCanvas(const std::optional<Canvas>& canvas);
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
         * @return The rendered png image if successful
         * @return std::nullopt if rendering not successful
         */
        std::optional<PngImage> draw(const std::vector<float>& sample);

    private:
        /**
         * @brief Gets a mirrored width according to drawing area configuration.
         * @param width The width to mirror
         * @return The mirrored width
         */
        float getMirrorWidth(float width);
        /**
         * @brief Gets a mirrored height according to drawing area configuration.
         * @param height The height to mirror
         * @return The mirrored height
         */
        float getMirrorHeight(float height);
        /**
         * @brief Gets a mirrored x-value according to drawing area configuration.
         * @param x The x-value to mirror
         * @return The mirrored x-value
         */
        float getMirrorX(float x);
        /**
         * @brief Gets a mirrored y-value according to drawing area configuration.
         * @param y The y-value to mirror
         * @return The mirrored y-value
         */
        float getMirrorY(float y);
        /**
         * @brief Gets a gradient based on the current color profile's background colors.
         * @param useForegroundColors Whether or not to use the color profile's foreground colors for the background gradient instead.
         * @return The background color gradient
         */
        sk_sp<SkShader> getBackgroundGradient(bool useForegroundColors = false);
        /**
         * @brief Gets a gradient based on the current color profile's background colors.
         * @return The background color gradient
         */
        sk_sp<SkShader> getForegroundGradient();
        /**
         * @brief Gets a paint brush for a spine drawing.
         * @param paint The paint brush to modify
         * @param sample The sample value
         * @return The modified paint brush
         */
        SkPaint getPaintForSpine(const SkPaint& paint, float sample);
        /**
         * @brief Creates a wave drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawWave(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a levels drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawLevels(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a particles drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawParticles(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a bars drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawBars(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a spine drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawSpine(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a splitter drawing.
         * @param args The DrawingFunctionArguments
         * @note The splitter shape does not support circle mode. Thus, this function will always draw in box mode.
         */
        void drawSplitter(const DrawingFunctionArguments& args);
        /**
         * @brief Creates a hearts drawing.
         * @param args The DrawingFunctionArguments
         */
        void drawHearts(const DrawingFunctionArguments& args);
        mutable std::mutex m_mutex;
        std::optional<Canvas> m_canvas;
        DrawingArea m_drawingArea;
        ColorProfile m_colorProfile;
        std::optional<BackgroundImage> m_backgroundImage;
    };
}

#endif // RENDERER_H
