#include "models/renderer.h"
#include <algorithm>
#include <skia/include/codec/SkCodec.h>
#include <skia/include/core/SkBitmap.h>
#include <skia/include/core/SkColor.h>
#include <skia/include/core/SkData.h>
#include <skia/include/core/SkPaint.h>
#include <skia/include/core/SkPoint.h>
#include <skia/include/core/SkTileMode.h>
#include <skia/include/effects/SkGradientShader.h>

#define INNER_RADIUS 0.5f

namespace Nickvision::Cavalier::Shared::Models
{
    static float getMirrorWidth(float width, const DrawingArea& drawingArea)
    {
        if(drawingArea.getDirection() == DrawingDirection::LeftToRight || drawingArea.getDirection() == DrawingDirection::RightToLeft)
        {
            return width / 2;
        }
        return width;
    }

    static float getMirrorHeight(float height, const DrawingArea& drawingArea)
    {
        if(drawingArea.getDirection() == DrawingDirection::TopToBottom || drawingArea.getDirection() == DrawingDirection::BottomToTop)
        {
            return height / 2;
        }
        return height;
    }

    static DrawingDirection getMirrorDirection(DrawingDirection direction)
    {
        switch(direction)
        {
        case DrawingDirection::TopToBottom:
            return DrawingDirection::BottomToTop;
        case DrawingDirection::BottomToTop:
            return DrawingDirection::TopToBottom;
        case DrawingDirection::LeftToRight:
            return DrawingDirection::RightToLeft;
        case DrawingDirection::RightToLeft:
            return DrawingDirection::LeftToRight;
        default:
            return DrawingDirection::TopToBottom;
        }
    }

    static float getMirrorX(float width, const DrawingArea& drawingArea)
    {
        if(drawingArea.getDirection() == DrawingDirection::LeftToRight || drawingArea.getDirection() == DrawingDirection::RightToLeft)
        {
            return width / 2 + drawingArea.getMargin();
        }
        return drawingArea.getMargin();
    }

    static float getMirrorY(float height, const DrawingArea& drawingArea)
    {
        if(drawingArea.getDirection() == DrawingDirection::TopToBottom || drawingArea.getDirection() == DrawingDirection::BottomToTop)
        {
            return height / 2 + drawingArea.getMargin();
        }
        return drawingArea.getMargin();
    }

    static float flipCoord(float coordinate, float screenDimension, bool enabled)
    {
        float max{ std::max(0.f, std::min(coordinate, screenDimension)) };
        return enabled ? screenDimension - max : max;
    }


    Renderer::Renderer(SkCanvas* canvas)
        : m_canvas{ canvas }
    {

    }

    SkCanvas* Renderer::getCanvas() const
    {
        return m_canvas;
    }

    void Renderer::setCanvas(SkCanvas* canvas)
    {
        m_canvas = canvas;
    }

    const DrawingArea& Renderer::getDrawingArea() const
    {
        return m_drawingArea;
    }

    void Renderer::setDrawingArea(const DrawingArea& area)
    {
        m_drawingArea = area;
    }

    const ColorProfile& Renderer::getColorProfile() const
    {
        return m_colorProfile;
    }

    void Renderer::setColorProfile(const ColorProfile& profile)
    {
        m_colorProfile = profile;
    }

    const std::optional<BackgroundImage>& Renderer::getBackgroundImage() const
    {
        return m_backgroundImage;
    }

    void Renderer::setBackgroundImage(const std::optional<BackgroundImage>& image)
    {
        m_backgroundImage = image;
    }

    void Renderer::draw(const std::vector<float>& sample, float width, float height)
    {
        if(!m_canvas)
        {
            return;
        }
        m_canvas->clear(SkColors::kTransparent);
        //Draw Background
        SkPaint bgPaint;
        bgPaint.setStyle(SkPaint::kFill_Style);
        bgPaint.setAntiAlias(true);
        if(m_colorProfile.getBackgroundColors().size() > 1)
        {
            bgPaint.setShader(createBackgroundGradient(m_colorProfile.getBackgroundColors(), width, height));
        }
        else
        {
            const Color& color{ m_colorProfile.getBackgroundColors()[0] };
            bgPaint.setColor(SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB()));
        }
        m_canvas->drawRect({ 0, 0, width, height }, bgPaint);
    }

    sk_sp<SkShader> Renderer::createBackgroundGradient(const std::vector<Color>& colors, float width, float height, float margin)
    {
        std::vector<SkColor> skColors(colors.size());
        for(size_t i = colors.size(); i > 0; i--)
        {
            const Color& color{ colors[i - 1] };
            skColors[i] = SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB());
        }
        if(m_drawingArea.getMirrorMode() != MirrorMode::Off)
        {
            std::vector<SkColor> mirrorColors(skColors.size() * 2);
            if(m_drawingArea.getDirection() == DrawingDirection::BottomToTop || m_drawingArea.getDirection() == DrawingDirection::RightToLeft)
            {
                std::reverse(skColors.begin(), skColors.end());
            }
            for(size_t i = 0; i < skColors.size(); i++)
            {
                mirrorColors[i] = skColors[i];
                mirrorColors[mirrorColors.size() - 1 - i] = skColors[i];
            }
            skColors = mirrorColors;
        }
        std::vector<SkPoint> points(2);
        switch(m_drawingArea.getDirection())
        {
        case DrawingDirection::TopToBottom:
        {
            points[0] = { margin, margin + height + m_drawingArea.getYOffset() };
            points[1] = { margin, height * (1 + m_drawingArea.getYOffset()) };
            break;
        }
        case DrawingDirection::BottomToTop:
        {
            points[0] = { margin, height * (1 + m_drawingArea.getYOffset()) };
            points[1] = { margin, margin + height * m_drawingArea.getYOffset() };
            break;
        }
        case DrawingDirection::LeftToRight:
        {
            points[0] = { margin + width * m_drawingArea.getXOffset(), margin };
            points[1] = { width * (1 + m_drawingArea.getXOffset()), margin };
            break;
        }
        default:
        {
            points[0] = { width * (1 + m_drawingArea.getXOffset()), margin };
            points[1] = { margin + width * m_drawingArea.getXOffset(), margin };
            break;
        }
        }
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }

    sk_sp<SkShader> Renderer::createForegroundGradient(const std::vector<Color>& colors, float width, float height, float margin)
    {
        std::vector<SkColor> skColors(colors.size());
        for(size_t i = colors.size(); i > 0; i--)
        {
            const Color& color{ colors[i - 1] };
            skColors[i] = SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB());
        }
        if(m_drawingArea.getMode() == DrawingMode::Box || m_drawingArea.getShape() == DrawingShape::Splitter)
        {
            return createBackgroundGradient(colors, width, height, margin);
        }
        if(m_drawingArea.getMirrorMode() == MirrorMode::Off)
        {
            width = getMirrorWidth(width, m_drawingArea);
            height = getMirrorHeight(height, m_drawingArea);
        }
        if(m_drawingArea.getShape() == DrawingShape::Wave)
        {
            float fullRadius{ std::min(width, height) / 2 };
            float innerRadius{ fullRadius * INNER_RADIUS };
            std::vector<float> positions(skColors.size());
            for(size_t i = 0; i < skColors.size(); i++)
            {
                positions[i] = (i + (skColors.size() - i - 1) * innerRadius / fullRadius) / (skColors.size() - 1);
            }
            return SkGradientShader::MakeRadial({ width / 2, height / 2 }, fullRadius, &skColors[0], &positions[0], skColors.size(), SkTileMode::kClamp);
        }
        std::vector<SkPoint> points(2);
        points[0] = { margin, std::min(width, height) * INNER_RADIUS / 2 };
        points[1] = { margin, std::min(width, height) / 2 };
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }
}
