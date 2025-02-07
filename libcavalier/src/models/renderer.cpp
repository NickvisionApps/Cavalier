#include "models/renderer.h"
#include <algorithm>
#include <filesystem>
#include <functional>
#include <skia/include/codec/SkCodec.h>
#include <skia/include/core/SkBitmap.h>
#include <skia/include/core/SkColor.h>
#include <skia/include/core/SkData.h>
#include <skia/include/core/SkPaint.h>
#include <skia/include/core/SkPoint.h>
#include <skia/include/core/SkSurface.h>
#include <skia/include/core/SkTileMode.h>
#include <skia/include/effects/SkGradientShader.h>

#define INNER_RADIUS 0.5f
#define LINE_THICKNESS 5

namespace Nickvision::Cavalier::Shared::Models
{
    static SkBitmap* newBitmapFromImagePath(const std::filesystem::path& path)
    {
        if(!std::filesystem::exists(path))
        {
            return nullptr;
        }
        SkBitmap* bitmap{ new SkBitmap() };
        sk_sp<SkData> data{ SkData::MakeFromFileName(path.string().c_str()) };
        std::unique_ptr<SkCodec> codec{ SkCodec::MakeFromData(data) };
        SkImageInfo info{ codec->getInfo().makeColorType(kN32_SkColorType).makeAlphaType(kPremul_SkAlphaType) };
        bitmap->tryAllocPixels(info);
        codec->getPixels(info, bitmap->getPixels(), bitmap->rowBytes());
        return bitmap;
    }

    static float flipCoord(float coordinate, float screenDimension, bool enabled)
    {
        float max{ std::max(0.f, std::min(coordinate, screenDimension)) };
        return enabled ? screenDimension - max : max;
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

    Renderer::Renderer(const std::optional<Canvas>& canvas)
        : m_canvas{ canvas }
    {

    }

    const std::optional<Canvas>& Renderer::getCanvas() const
    {
        return m_canvas;
    }

    void Renderer::setCanvas(const std::optional<Canvas>& canvas)
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

    void Renderer::draw(const std::vector<float>& sample)
    {
        if(!m_canvas || sample.empty())
        {
            return;
        }
        //Setup
        SkBitmap* backgroundBitmap{ nullptr };
        int width{ m_canvas->getWidth() };
        int height{ m_canvas->getHeight() };
        ((*m_canvas))->clear(SkColors::kTransparent);
        //Draw Background Colors
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
        (*m_canvas)->drawRect({ 0, 0, width, height }, bgPaint);
        //Draw Background Image
        if(m_backgroundImage)
        {
            backgroundBitmap = newBitmapFromImagePath(m_backgroundImage->getPath());
            if(backgroundBitmap)
            {
                //Scale Image
                float scale{ std::max(width / backgroundBitmap->width(), height / backgroundBitmap->height()) * (m_backgroundImage->getScale() / 100) };
                float newWidth{ backgroundBitmap->width() * scale };
                float newHeight{ backgroundBitmap->height() * scale };
                SkBitmap* newBitmap{ new SkBitmap() };
                SkImageInfo newInfo{ SkImageInfo::Make(newWidth, newHeight, kN32_SkColorType, kPremul_SkAlphaType) };
                SkCanvas canvas{ *newBitmap };
                newBitmap->setInfo(newInfo);
                newBitmap->allocPixels();
                canvas.drawImageRect(backgroundBitmap->asImage(), SkRect::MakeWH(newWidth, newHeight), SkSamplingOptions(SkFilterMode::kLinear));
                delete backgroundBitmap;
                backgroundBitmap = newBitmap;
                //Draw Image
                SkPaint paint;
                paint.setColor(SkColorSetARGB(255 * (m_backgroundImage->getAlpha() / 100), SkColorGetR(paint.getColor()), SkColorGetG(paint.getColor()), SkColorGetB(paint.getColor())));
                (*m_canvas)->drawImage(backgroundBitmap->asImage(), width / 2 - backgroundBitmap->width() / 2, height / 2 - backgroundBitmap->height() / 2, SkSamplingOptions(SkFilterMode::kLinear), &paint);
                delete backgroundBitmap;
            }
        }
        //Draw Foreground Colors
        width -= m_drawingArea.getMargin() * 2;
        height -= m_drawingArea.getMargin() * 2;
        SkPaint fgPaint;
        fgPaint.setStyle(m_drawingArea.getFillShape() ? SkPaint::kFill_Style : SkPaint::kStroke_Style);
        fgPaint.setStrokeWidth(LINE_THICKNESS);
        fgPaint.setAntiAlias(true);
        if(m_colorProfile.getForegroundColors().size() > 1 && m_drawingArea.getShape() != DrawingShape::Spine)
        {
            fgPaint.setShader(createForegroundGradient(m_colorProfile.getForegroundColors(), width, height));
        }
        else
        {
            const Color& color{ m_colorProfile.getForegroundColors()[0] };
            fgPaint.setColor(SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB()));
        }
        //Get Drawing Function
        std::function<void(const DrawingFunctionArguments&)> drawFunction;
        switch(m_drawingArea.getShape())
        {
        case DrawingShape::Wave:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawWave(args); };
            break;
        case DrawingShape::Levels:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawLevels(args); };;
            break;
        case DrawingShape::Particles:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawParticles(args); };;
            break;
        case DrawingShape::Bars:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawBars(args); };;
            break;
        case DrawingShape::Spine:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawSpine(args); };;
            break;
        case DrawingShape::Splitter:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawSplitter(args); };;
            break;
        case DrawingShape::Hearts:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawHearts(args); };;
            break;
        default:
            drawFunction = [this](const DrawingFunctionArguments& args){ drawWave(args); };;
            break;
        }
        //Draw Shape
        Point start{ (width + m_drawingArea.getMargin() * 2) * m_drawingArea.getXOffset() + m_drawingArea.getMargin(), (height + m_drawingArea.getMargin() * 2) * m_drawingArea.getYOffset() + m_drawingArea.getMargin() };
        Point end{ width, height };
        Point reverseEnd{ getMirrorWidth(width), getMirrorHeight(height) };
        if(m_drawingArea.getMirrorMode() == MirrorMode::Full || m_drawingArea.getMirrorMode() == MirrorMode::ReverseFull)
        {
            std::vector<float> reverseSample = sample;
            std::reverse(reverseSample.begin(), reverseSample.end());
            drawFunction({ sample, m_drawingArea.getMode(), m_drawingArea.getDirection(), start, reverseEnd, 0, fgPaint });
            drawFunction({ m_drawingArea.getMirrorMode() == MirrorMode::ReverseFull ? reverseSample : sample, m_drawingArea.getMode(), getMirrorDirection(m_drawingArea.getDirection()), start, reverseEnd, 0, fgPaint });
        }
        else if(m_drawingArea.getMirrorMode() == MirrorMode::SplitChannels || m_drawingArea.getMirrorMode() == MirrorMode::ReverseSplitChannels)
        {
            std::vector<float> splitSample{ sample.begin(), sample.begin() + (sample.size() / 2) };
            std::vector<float> splitSampleEnd{ sample.begin() + (sample.size() / 2), sample.end() };
            std::vector<float> reverseSplitSampleEnd = splitSampleEnd;
            std::reverse(reverseSplitSampleEnd.begin(), reverseSplitSampleEnd.end());
            drawFunction({ splitSample, m_drawingArea.getMode(), m_drawingArea.getDirection(), start, reverseEnd, 0, fgPaint });
            drawFunction({ m_drawingArea.getMirrorMode() == MirrorMode::ReverseSplitChannels ? splitSampleEnd : reverseSplitSampleEnd, m_drawingArea.getMode(), getMirrorDirection(m_drawingArea.getDirection()), start, reverseEnd, 0, fgPaint });
        }
        else
        {
            drawFunction({ sample, m_drawingArea.getMode(), m_drawingArea.getDirection(), start, end, 0, fgPaint });
        }
    }

    float Renderer::getMirrorWidth(float width)
    {
        if(m_drawingArea.getDirection() == DrawingDirection::LeftToRight || m_drawingArea.getDirection() == DrawingDirection::RightToLeft)
        {
            return width / 2;
        }
        return width;
    }

    float Renderer::getMirrorHeight(float height)
    {
        if(m_drawingArea.getDirection() == DrawingDirection::TopToBottom || m_drawingArea.getDirection() == DrawingDirection::BottomToTop)
        {
            return height / 2;
        }
        return height;
    }

    float Renderer::getMirrorX(float x)
    {
        if(m_drawingArea.getDirection() == DrawingDirection::LeftToRight || m_drawingArea.getDirection() == DrawingDirection::RightToLeft)
        {
            return x / 2 + m_drawingArea.getMargin();
        }
        return m_drawingArea.getMargin();
    }

    float Renderer::getMirrorY(float y)
    {
        if(m_drawingArea.getDirection() == DrawingDirection::TopToBottom || m_drawingArea.getDirection() == DrawingDirection::BottomToTop)
        {
            return y / 2 + m_drawingArea.getMargin();
        }
        return m_drawingArea.getMargin();
    }

    sk_sp<SkShader> Renderer::createBackgroundGradient(const std::vector<Color>& colors, float width, float height)
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
            points[0] = { m_drawingArea.getMargin(), m_drawingArea.getMargin() + height + m_drawingArea.getYOffset() };
            points[1] = { m_drawingArea.getMargin(), height * (1 + m_drawingArea.getYOffset()) };
            break;
        }
        case DrawingDirection::BottomToTop:
        {
            points[0] = { m_drawingArea.getMargin(), height * (1 + m_drawingArea.getYOffset()) };
            points[1] = { m_drawingArea.getMargin(), m_drawingArea.getMargin() + height * m_drawingArea.getYOffset() };
            break;
        }
        case DrawingDirection::LeftToRight:
        {
            points[0] = { m_drawingArea.getMargin() + width * m_drawingArea.getXOffset(), m_drawingArea.getMargin() };
            points[1] = { width * (1 + m_drawingArea.getXOffset()), m_drawingArea.getMargin() };
            break;
        }
        default:
        {
            points[0] = { width * (1 + m_drawingArea.getXOffset()), m_drawingArea.getMargin() };
            points[1] = { m_drawingArea.getMargin() + width * m_drawingArea.getXOffset(), m_drawingArea.getMargin() };
            break;
        }
        }
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }

    sk_sp<SkShader> Renderer::createForegroundGradient(const std::vector<Color>& colors, float width, float height)
    {
        std::vector<SkColor> skColors(colors.size());
        for(size_t i = colors.size(); i > 0; i--)
        {
            const Color& color{ colors[i - 1] };
            skColors[i] = SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB());
        }
        if(m_drawingArea.getMode() == DrawingMode::Box || m_drawingArea.getShape() == DrawingShape::Splitter)
        {
            return createBackgroundGradient(colors, width, height);
        }
        if(m_drawingArea.getMirrorMode() == MirrorMode::Off)
        {
            width = getMirrorWidth(width);
            height = getMirrorHeight(height);
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
        points[0] = { m_drawingArea.getMargin(), std::min(width, height) * INNER_RADIUS / 2 };
        points[1] = { m_drawingArea.getMargin(), std::min(width, height) / 2 };
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }

    void Renderer::drawWave(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / (args.getSample().size() - 1) };
            SkPath path;
            bool flipImage{ false };
            std::vector<Point> points(args.getSample().size());
            std::vector<float> gradients(args.getSample().size());
            switch(args.getDirection())
            {
            case DrawingDirection::TopToBottom:
            case DrawingDirection::BottomToTop:
            {
                flipImage = args.getDirection() == DrawingDirection::TopToBottom;
                //Create a list of points of where the curve must pass through
                for(int i = 0; i < args.getSample().size(); i++)
                {
                    points[i] = { step * i, args.getEnd().getY() * (1 - args.getSample()[i]) };
                }
                //Calculate gradient between the two neighbouring points for each point
                for(int i = 0; i < points.size(); i++)
                {
                    //Determine the previous and next point
                    //If there isn't one, use the current point
                    const Point& previous{ points[std::max(i - 1, 0)] };
                    const Point& next{ points[std::min(i + 1, static_cast<int>(points.size()) - 1)] };
                    float gradient{ next.getY() - previous.getY() };
                    //If using the current point (when at the edges), then the run is rise/run = 1
                    //Otherwise, a two step run exists
                    gradients[i] = i == 0 || i == points.size() - 1 ? gradient : gradient / 2;
                }
                float yOffset{ args.getStart().getY() + (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS / 2) };
                path.moveTo(args.getStart().getX() + points[0].getX(), yOffset + flipCoord(points[0].getY(), args.getEnd().getY(), flipImage));
                for(int i = 0; i < points.size() - 1; i++)
                {
                    SkPoint a{ args.getStart().getX() + points[i].getX() + step * 0.5f, yOffset + flipCoord(points[i].getY() + gradients[i] * 0.5f, args.getEnd().getY(), flipImage) };
                    SkPoint b{ args.getStart().getX() + points[i + 1].getX() + step * -0.5f, yOffset + flipCoord(points[i + 1].getY() + gradients[i + 1] * -0.5f, args.getEnd().getY(), flipImage) };
                    SkPoint c{ args.getStart().getX() + points[i + 1].getX(), yOffset + flipCoord(points[i + 1].getY(), args.getEnd().getY(), flipImage) };
                    path.cubicTo(a, b, c);
                }
                if(m_drawingArea.getFillShape())
                {
                    path.lineTo({ args.getStart().getX() + args.getEnd().getX(), args.getStart().getY() + flipCoord(args.getEnd().getY(), args.getEnd().getY(), flipImage) });
                    path.lineTo({ args.getStart().getX(), args.getStart().getY() + flipCoord(args.getEnd().getY(), args.getEnd().getY(), flipImage) });
                    path.close();
                }
                break;
            }
            case DrawingDirection::LeftToRight:
            case DrawingDirection::RightToLeft:
            {
                flipImage = args.getDirection() == DrawingDirection::RightToLeft;
                for(int i = 0; i < args.getSample().size(); i++)
                {
                    points[i] = { args.getEnd().getX() * args.getSample()[i], step * i };
                }
                for(int i = 0; i < points.size(); i++)
                {
                    const Point& previous{ points[std::max(i - 1, 0)] };
                    const Point& next{ points[std::min(i + 1, static_cast<int>(points.size()) - 1)] };
                    float gradient{ next.getX() - previous.getX() };
                    gradients[i] = i == 0 || i == points.size() - 1 ? gradient : gradient / 2;
                }
                float xOffset{ args.getStart().getX() - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS / 2) };
                path.moveTo(xOffset + flipCoord(points[0].getX(), args.getEnd().getX(), flipImage), args.getStart().getY() + points[0].getY());
                for(int i = 0; i < points.size() - 1; i++)
                {
                    SkPoint a{ xOffset + flipCoord(points[i].getX() + gradients[i] * 0.5f, args.getEnd().getX(), flipImage), args.getStart().getY() + points[i].getY() + step * 0.5f };
                    SkPoint b{ xOffset + flipCoord(points[i + 1].getX() + gradients[i + 1] * -0.5f, args.getEnd().getX(), flipImage), args.getStart().getY() + points[i + 1].getY() + step * -0.5f };
                    SkPoint c{ xOffset + flipCoord(points[i + 1].getX(), args.getEnd().getX(), flipImage), args.getStart().getY() + points[i + 1].getY() };
                    path.cubicTo(a, b, c);
                }
                if(m_drawingArea.getFillShape())
                {
                    path.lineTo({ args.getStart().getX() + flipCoord(0, args.getEnd().getX(), flipImage), args.getStart().getY() + args.getEnd().getY() });
                    path.lineTo({ args.getStart().getX() + flipCoord(0, args.getEnd().getX(), flipImage), args.getStart().getY() });
                    path.close();
                }
                break;
            }
            }
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawLevels(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawParticles(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawBars(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawSpine(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawSplitter(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }

    void Renderer::drawHearts(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            //TODO
        }
        else //Circle
        {
            //TODO
        }
    }
}
