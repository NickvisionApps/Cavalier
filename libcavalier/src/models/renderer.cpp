#include "models/renderer.h"
#include <algorithm>
#include <cmath>
#include <filesystem>
#include <functional>
#include <numbers>
#include <skia/include/codec/SkCodec.h>
#include <skia/include/core/SkBitmap.h>
#include <skia/include/core/SkColor.h>
#include <skia/include/core/SkData.h>
#include <skia/include/core/SkPaint.h>
#include <skia/include/core/SkPoint.h>
#include <skia/include/core/SkSurface.h>
#include <skia/include/core/SkTileMode.h>
#include <skia/include/effects/SkGradientShader.h>
#include <skia/include/encode/SkPngEncoder.h>

#define INNER_RADIUS 0.5f
#define LINE_THICKNESS 5
#define PI std::numbers::pi_v<float>
#define ROTATION 0.0f

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
        if(bitmap->tryAllocPixels(info))
        {
            codec->getPixels(info, bitmap->getPixels(), bitmap->rowBytes());
            return bitmap;
        }
        return nullptr;
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

    static SkPath getHeartPath(float itemSize)
    {
        SkPath path;
        path.moveTo(0.0f, itemSize / 2.0f);
        path.cubicTo(0.0f, itemSize / 2.2f,
                     -itemSize / 1.8f, itemSize / 3.0f,
                     -itemSize / 2.0f, -itemSize / 6.0f);
        path.cubicTo(-itemSize / 2.5f, -itemSize / 2.0f,
                     -itemSize / 6.5f, -itemSize / 2.0f,
                     0.0, -itemSize / 5.5f);
        path.cubicTo(itemSize / 6.5f, -itemSize / 2.0f,
                     itemSize / 2.5f, -itemSize / 2.0f,
                     itemSize / 2.0f, -itemSize / 6.0f);
        path.cubicTo(itemSize / 1.8f, itemSize / 3.0f,
                     0.0f, itemSize / 2.2f,
                     0.0f, itemSize / 2.0f);
        path.close();
        return path;
    }

    Renderer::Renderer(const std::optional<Canvas>& canvas)
        : m_canvas{ canvas },
        m_backgroundImage{ std::nullopt }
    {

    }

    Renderer::Renderer(const DrawingArea& drawingArea, const ColorProfile& colorProfile, const std::optional<BackgroundImage>& backgroundImage, const std::optional<Canvas>& canvas)
        : m_canvas{ canvas },
        m_drawingArea{ drawingArea },
        m_colorProfile{ colorProfile },
        m_backgroundImage{ backgroundImage }
    {

    }

    const std::optional<Canvas>& Renderer::getCanvas() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_canvas;
    }

    void Renderer::setCanvas(const std::optional<Canvas>& canvas)
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_canvas = canvas;
    }

    const DrawingArea& Renderer::getDrawingArea() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_drawingArea;
    }

    void Renderer::setDrawingArea(const DrawingArea& area)
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_drawingArea = area;
    }

    const ColorProfile& Renderer::getColorProfile() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_colorProfile;
    }

    void Renderer::setColorProfile(const ColorProfile& profile)
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_colorProfile = profile;
    }

    const std::optional<BackgroundImage>& Renderer::getBackgroundImage() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_backgroundImage;
    }

    void Renderer::setBackgroundImage(const std::optional<BackgroundImage>& image)
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_backgroundImage = image;
    }

    std::optional<PngImage> Renderer::draw(const std::vector<float>& sample)
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        if(!m_canvas || sample.empty())
        {
            return std::nullopt;
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
            bgPaint.setShader(getBackgroundGradient());
        }
        else
        {
            const Color& color{ m_colorProfile.getBackgroundColors()[0] };
            bgPaint.setColor(SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB()));
        }
        (*m_canvas)->drawRect({ 0, 0, static_cast<float>(width), static_cast<float>(height) }, bgPaint);
        //Draw Background Image
        if(m_backgroundImage)
        {
            backgroundBitmap = newBitmapFromImagePath(m_backgroundImage->getPath());
            if(backgroundBitmap)
            {
                //Scale Image
                float scale{ static_cast<float>(std::max(width / backgroundBitmap->width(), height / backgroundBitmap->height())) * (m_backgroundImage->getScale() / 100.0f) };
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
            fgPaint.setShader(getForegroundGradient());
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
        Point start{ static_cast<float>((width + m_drawingArea.getMargin() * 2) * (m_drawingArea.getXOffset() / 100.0f) + m_drawingArea.getMargin()), static_cast<float>((height + m_drawingArea.getMargin() * 2) * (m_drawingArea.getYOffset() / 100.0f) + m_drawingArea.getMargin()) };
        Point end{ static_cast<float>(width), static_cast<float>(height) };
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
        //Get PNG Image
        sk_sp<SkImage> image{ m_canvas->getSkiaSurface()->makeImageSnapshot() };
        if(image)
        {
            sk_sp<SkData> png{ SkPngEncoder::Encode(nullptr, image.get(), {}) };
            if(png)
            {
                return PngImage{ m_canvas->getWidth(), m_canvas->getHeight(), png->bytes(), png->size() };
            }
        }
        return std::nullopt;
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

    sk_sp<SkShader> Renderer::getBackgroundGradient(bool useForegroundColors)
    {
        if(!m_canvas || (!useForegroundColors ? m_colorProfile.getBackgroundColors().size() <= 1: m_colorProfile.getForegroundColors().size() <= 1))
        {
            return nullptr;
        }
        std::vector<SkColor> skColors(!useForegroundColors ? m_colorProfile.getBackgroundColors().size() : m_colorProfile.getForegroundColors().size());
        for(size_t i = skColors.size(); i > 0; i--)
        {
            const Color& color{ !useForegroundColors ? m_colorProfile.getBackgroundColors()[i - 1] : m_colorProfile.getForegroundColors()[i - 1] };
            skColors[i - 1] = SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB());
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
            points[0] = { static_cast<float>(m_drawingArea.getMargin()),
                          static_cast<float>(m_drawingArea.getMargin() + m_canvas->getHeight() + m_drawingArea.getYOffset()) };
            points[1] = { static_cast<float>(m_drawingArea.getMargin()),
                          static_cast<float>(m_canvas->getHeight() * (1 + m_drawingArea.getYOffset())) };
            break;
        }
        case DrawingDirection::BottomToTop:
        {
            points[0] = { static_cast<float>(m_drawingArea.getMargin()),
                          static_cast<float>(m_canvas->getHeight() * (1 + m_drawingArea.getYOffset())) };
            points[1] = { static_cast<float>(m_drawingArea.getMargin()),
                          static_cast<float>(m_drawingArea.getMargin() + m_canvas->getHeight() * m_drawingArea.getYOffset()) };
            break;
        }
        case DrawingDirection::LeftToRight:
        {
            points[0] = { static_cast<float>(m_drawingArea.getMargin() + m_canvas->getWidth() * m_drawingArea.getXOffset()),
                          static_cast<float>(m_drawingArea.getMargin()) };
            points[1] = { static_cast<float>(m_canvas->getWidth() * (1 + m_drawingArea.getXOffset())),
                          static_cast<float>(m_drawingArea.getMargin()) };
            break;
        }
        default:
        {
            points[0] = { static_cast<float>(m_canvas->getWidth() * (1 + m_drawingArea.getXOffset())),
                          static_cast<float>(m_drawingArea.getMargin()) };
            points[1] = { static_cast<float>(m_drawingArea.getMargin() + m_canvas->getWidth() * m_drawingArea.getXOffset()),
                          static_cast<float>(m_drawingArea.getMargin()) };
            break;
        }
        }
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }

    sk_sp<SkShader> Renderer::getForegroundGradient()
    {
        if(!m_canvas || m_colorProfile.getForegroundColors().size() <= 1)
        {
            return nullptr;
        }
        std::vector<SkColor> skColors(m_colorProfile.getForegroundColors().size());
        float width{ static_cast<float>(m_canvas->getWidth()) };
        float height{ static_cast<float>(m_canvas->getHeight()) };
        for(size_t i = skColors.size(); i > 0; i--)
        {
            const Color& color{ m_colorProfile.getForegroundColors()[i - 1] };
            skColors[i - 1] = SkColorSetARGB(color.getA(), color.getR(), color.getG(), color.getB());
        }
        if(m_drawingArea.getMode() == DrawingMode::Box || m_drawingArea.getShape() == DrawingShape::Splitter)
        {
            return getBackgroundGradient(true);
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
        points[0] = { static_cast<float>(m_drawingArea.getMargin()),
                      std::min(width, height) * INNER_RADIUS / 2.0f };
        points[1] = { static_cast<float>(m_drawingArea.getMargin()),
                      std::min(width, height) / 2.0f };
        return SkGradientShader::MakeLinear(&points[0], &skColors[0], nullptr, skColors.size(), SkTileMode::kClamp);
    }

    SkPaint Renderer::getPaintForSpine(const SkPaint& paint, float sample)
    {
        SkPaint newPaint{ paint };
        if(m_colorProfile.getForegroundColors().size() > 1)
        {
            float pos{ (m_colorProfile.getForegroundColors().size() - 1) * (1 - sample) };
            const Color& color1{ m_colorProfile.getForegroundColors()[static_cast<size_t>(std::floor(pos))] };
            const Color& color2{ m_colorProfile.getForegroundColors()[static_cast<size_t>(std::ceil(pos))] };
            float weight{ sample < 1 ? std::fmod(pos, 1.0f) : 1.0f };
            newPaint.setColor(SkColorSetARGB(color1.getA() * (1 - weight) + color2.getA() * weight,
                                             color1.getR() * (1 - weight) + color2.getR() * weight,
                                             color1.getG() * (1 - weight) + color2.getG() * weight,
                                             color1.getB() * (1 - weight) + color2.getB() * weight));
        }
        return newPaint;
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
                for(size_t i = 0; i < args.getSample().size(); i++)
                {
                    points[i] = { step * i,
                                  args.getEnd().getY() * (1 - args.getSample()[i]) };
                }
                //Calculate gradient between the two neighbouring points for each point
                for(size_t i = 0; i < points.size(); i++)
                {
                    //Determine the previous and next point
                    //If there isn't one, use the current point
                    const Point& previous{ points[std::max(i - 1, static_cast<size_t>(0))] };
                    const Point& next{ points[std::min(i + 1, points.size() - 1)] };
                    float gradient{ next.getY() - previous.getY() };
                    //If using the current point (when at the edges), then the run is rise/run = 1
                    //Otherwise, a two step run exists
                    gradients[i] = i == 0 || i == points.size() - 1 ? gradient : gradient / 2;
                }
                float yOffset{ args.getStart().getY() + (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS / 2) };
                path.moveTo(args.getStart().getX() + points[0].getX(),
                            yOffset + flipCoord(points[0].getY(), args.getEnd().getY(), flipImage));
                for(size_t i = 0; i < points.size() - 1; i++)
                {
                    SkPoint a{ args.getStart().getX() + points[i].getX() + step * 0.5f,
                               yOffset + flipCoord(points[i].getY() + gradients[i] * 0.5f, args.getEnd().getY(), flipImage) };
                    SkPoint b{ args.getStart().getX() + points[i + 1].getX() + step * -0.5f,
                               yOffset + flipCoord(points[i + 1].getY() + gradients[i + 1] * -0.5f, args.getEnd().getY(), flipImage) };
                    SkPoint c{ args.getStart().getX() + points[i + 1].getX(),
                               yOffset + flipCoord(points[i + 1].getY(), args.getEnd().getY(), flipImage) };
                    path.cubicTo(a, b, c);
                }
                if(m_drawingArea.getFillShape())
                {
                    path.lineTo({ args.getStart().getX() + args.getEnd().getX(),
                                  args.getStart().getY() + flipCoord(args.getEnd().getY(), args.getEnd().getY(), flipImage) });
                    path.lineTo({ args.getStart().getX(),
                                  args.getStart().getY() + flipCoord(args.getEnd().getY(), args.getEnd().getY(), flipImage) });
                    path.close();
                }
                break;
            }
            case DrawingDirection::LeftToRight:
            case DrawingDirection::RightToLeft:
            {
                flipImage = args.getDirection() == DrawingDirection::RightToLeft;
                for(size_t i = 0; i < args.getSample().size(); i++)
                {
                    points[i] = { args.getEnd().getX() * args.getSample()[i], step * i };
                }
                for(size_t i = 0; i < points.size(); i++)
                {
                    const Point& previous{ points[std::max(i - 1, static_cast<size_t>(0))] };
                    const Point& next{ points[std::min(i + 1, points.size() - 1)] };
                    float gradient{ next.getX() - previous.getX() };
                    gradients[i] = i == 0 || i == points.size() - 1 ? gradient : gradient / 2;
                }
                float xOffset{ args.getStart().getX() - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS / 2) };
                path.moveTo(xOffset + flipCoord(points[0].getX(), args.getEnd().getX(), flipImage),
                            args.getStart().getY() + points[0].getY());
                for(size_t i = 0; i < points.size() - 1; i++)
                {
                    SkPoint a{ xOffset + flipCoord(points[i].getX() + gradients[i] * 0.5f, args.getEnd().getX(), flipImage),
                               args.getStart().getY() + points[i].getY() + step * 0.5f };
                    SkPoint b{ xOffset + flipCoord(points[i + 1].getX() + gradients[i + 1] * -0.5f, args.getEnd().getX(), flipImage),
                               args.getStart().getY() + points[i + 1].getY() + step * -0.5f };
                    SkPoint c{ xOffset + flipCoord(points[i + 1].getX(), args.getEnd().getX(), flipImage),
                               args.getStart().getY() + points[i + 1].getY() };
                    path.cubicTo(a, b, c);
                }
                if(m_drawingArea.getFillShape())
                {
                    path.lineTo({ args.getStart().getX() + flipCoord(0, args.getEnd().getX(), flipImage),
                                  args.getStart().getY() + args.getEnd().getY() });
                    path.lineTo({ args.getStart().getX() + flipCoord(0, args.getEnd().getX(), flipImage),
                                  args.getStart().getY() });
                    path.close();
                }
                break;
            }
            }
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        else if(args.getMode() == DrawingMode::Circle)
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float radius{ fullRadius - innerRadius };
            SkPaint paint{ args.getPaint() };
            SkPath path;
            (*m_canvas)->save();
            (*m_canvas)->translate(args.getStart().getX(), args.getStart().getY());
            paint.setStyle(SkPaint::kStroke_Style);
            paint.setStrokeWidth(m_drawingArea.getFillShape() ? radius : LINE_THICKNESS);
            path.moveTo(args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[0]) * std::cos(PI / 2 + ROTATION),
                        args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[0]) * std::sin(PI / 2 + ROTATION));
            for(size_t i = 0; i < args.getSample().size() - 1; i++)
            {
                SkPoint a{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[i]) * std::cos(PI / 2 + PI * 2 * (i + 0.5f) / args.getSample().size() + ROTATION),
                           args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[i]) * std::sin(PI / 2 + PI * 2 * (i + 0.5f) / args.getSample().size() + ROTATION) };
                SkPoint b{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[i + 1]) * std::cos(PI / 2 + PI * 2 * (i + 0.5f) / args.getSample().size() + ROTATION),
                           args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[i + 1]) * std::sin(PI / 2 + PI * 2 * (i + 0.5f) / args.getSample().size() + ROTATION) };
                SkPoint c{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[i + 1]) * std::cos(PI / 2 + PI * 2 * (i + 1.0f) / args.getSample().size() + ROTATION),
                           args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[i + 1]) * std::sin(PI / 2 + PI * 2 * (i + 1.0f) / args.getSample().size() + ROTATION) };
                path.cubicTo(a, b, c);
            }
            SkPoint a{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[args.getSample().size() - 1]) * std::cos(PI / 2 + PI * 2 * (args.getSample().size() - 0.5f) / args.getSample().size() + ROTATION),
                       args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[args.getSample().size() - 1]) * std::sin(PI / 2 + PI * 2 * (args.getSample().size() - 0.5f) / args.getSample().size() + ROTATION) };
            SkPoint b{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[0]) * std::cos(PI / 2 + PI * 2 * (args.getSample().size() - 0.5f) / args.getSample().size() + ROTATION),
                       args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[0]) * std::sin(PI / 2 + PI * 2 * (args.getSample().size() - 0.5f) / args.getSample().size() + ROTATION) };
            SkPoint c{ args.getEnd().getX() / 2 + (innerRadius + radius * args.getSample()[0]) * std::cos(PI / 2 + ROTATION),
                       args.getEnd().getY() / 2 + (innerRadius + radius * args.getSample()[0]) * std::sin(PI / 2 + ROTATION) };
            path.cubicTo(a, b, c);
            path.close();
            if(m_drawingArea.getFillShape())
            {
                (*m_canvas)->clipPath(path, SkClipOp::kIntersect, true);
                (*m_canvas)->drawCircle({ args.getEnd().getX() / 2, args.getEnd().getY() / 2 }, innerRadius + radius / 2, paint);
            }
            else
            {
                (*m_canvas)->drawPath(path, paint);
            }
            (*m_canvas)->restore();
        }
    }

    void Renderer::drawLevels(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
            float fill{ m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f };
            float itemWidth{ (args.getDirection() < DrawingDirection::LeftToRight ? step : args.getEnd().getX() / 10.0f) * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - fill };
            float itemHeight{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getY() / 10.0f : step) * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - fill };
            float rx{ itemWidth / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
            float ry{ itemHeight / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
            SkPath path;
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                for(float j = 0; j < std::floor(args.getSample()[i] * 10.0f); j++)
                {
                    SkRect rect;
                    switch(m_drawingArea.getDirection())
                    {
                    case DrawingDirection::TopToBottom:
                        rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                                 args.getStart().getY() + args.getEnd().getY() / 10.0f * j + args.getEnd().getY() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                                 args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + itemWidth,
                                 args.getStart().getY() + args.getEnd().getY() / 10.0f * j + args.getEnd().getY() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemHeight };
                        break;
                    case DrawingDirection::BottomToTop:
                        rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                                 args.getStart().getY() + args.getEnd().getY() / 10.0f * (9 - j) + args.getEnd().getY() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                                 args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + itemWidth,
                                 args.getStart().getY() + args.getEnd().getY() / 10.0f * (9 - j) + args.getEnd().getY() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemHeight };
                        break;
                    case DrawingDirection::LeftToRight:
                        rect = { args.getStart().getX() + args.getEnd().getX() / 10.0f * j + args.getEnd().getX() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                                 args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                                 args.getStart().getX() + args.getEnd().getX() / 10.0f * j + args.getEnd().getX() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemWidth,
                                 args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + itemHeight };
                        break;
                    case DrawingDirection::RightToLeft:
                        rect = { args.getStart().getX() + args.getEnd().getX() / 10.0f * (9 - j) + args.getEnd().getX() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                                 args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                                 args.getStart().getX() + args.getEnd().getX() / 10.0f * (9 - j) + args.getEnd().getX() / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemWidth,
                                 args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + itemHeight };
                        break;
                    }
                    path.addRoundRect(rect, rx, ry);
                }
            }
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        else if(args.getMode() == DrawingMode::Circle)
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float radius{ fullRadius - innerRadius };
            float barWidth{ 2.0f * PI * innerRadius / args.getSample().size() };
            float rx{ (barWidth * (1 - (m_drawingArea.getItemSpacing() / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS)) * (m_drawingArea.getItemRoundness() / 100.0f) };
            float ry{ radius / 10.0f * (1 - (m_drawingArea.getItemSpacing() / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) * (m_drawingArea.getItemRoundness() / 100.0f) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                (*m_canvas)->save();
                (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                       args.getStart().getY() + args.getEnd().getY() / 2.0f);
                (*m_canvas)->rotate(SkRadiansToDegrees(2.0f * PI * (i + 0.5f) / args.getSample().size() + ROTATION));
                for(float j = 0; j < std::floor(args.getSample()[i] * 10.0f); j++)
                {
                    SkRect rect{ SkRect::MakeXYWH(-barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) / 2.0f + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                                  innerRadius + radius / 10.0f * j + radius / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                                  barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                                                  radius / 10.0f * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS)) };
                    (*m_canvas)->drawRoundRect(rect, rx, ry, args.getPaint());
                }
                (*m_canvas)->restore();
            }
        }
    }

    void Renderer::drawParticles(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
            float fill{ m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f };
            float itemWidth{ (args.getDirection() < DrawingDirection::LeftToRight ? step : args.getEnd().getX() / 11.0f) * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - fill };
            float itemHeight{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getY() / 11.0f : step) * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - fill };
            float rx{ itemWidth / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
            float ry{ itemHeight / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
            SkPath path;
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                SkRect rect;
                switch(m_drawingArea.getDirection())
                {
                case DrawingDirection::TopToBottom:
                    rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getY() + args.getEnd().getY() / 11.0f * 10.0f * args.getSample()[i] + args.getEnd().getY() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                             args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + itemWidth,
                             args.getStart().getY() + args.getEnd().getY() / 11.0f * 10.0f * args.getSample()[i] + args.getEnd().getY() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemHeight };
                    break;
                case DrawingDirection::BottomToTop:
                    rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getY() + args.getEnd().getY() / 11.0f * 10.0f * (1 - args.getSample()[i]) + args.getEnd().getY() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                             args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + itemWidth,
                             args.getStart().getY() + args.getEnd().getY() / 11.0f * 10.0f * (1 - args.getSample()[i]) + args.getEnd().getY() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemHeight };
                    break;
                case DrawingDirection::LeftToRight:
                    rect = { args.getStart().getX() + args.getEnd().getX() / 11.0f * 10.0f * args.getSample()[i] + args.getEnd().getX() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                             args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getX() + args.getEnd().getX() / 11.0f * 10.0f * args.getSample()[i] + args.getEnd().getX() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemWidth,
                             args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + itemHeight };
                    break;
                case DrawingDirection::RightToLeft:
                    rect = { args.getStart().getX() + args.getEnd().getX() / 11.0f * 10.0f * (1 - args.getSample()[i]) + args.getEnd().getX() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + fill,
                             args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getX() + args.getEnd().getX() / 11.0f * 10.0f * (1 - args.getSample()[i]) + args.getEnd().getX() / 11.0f * (m_drawingArea.getItemSpacing() / 100.0f) + itemWidth,
                             args.getStart().getY() + step * (i * (m_drawingArea.getItemSpacing() / 100.0f)) + itemHeight };
                    break;
                }
                path.addRoundRect(rect, rx, ry);
            }
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        else //Circle
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float radius{ fullRadius - innerRadius };
            float barWidth{ 2.0f * PI * innerRadius / args.getSample().size() };
            float rx{ (barWidth * (1 - (m_drawingArea.getItemSpacing() / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS)) * (m_drawingArea.getItemRoundness() / 100.0f) };
            float ry{ radius / 10.0f * (1 - (m_drawingArea.getItemSpacing() / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) * (m_drawingArea.getItemRoundness() / 100.0f) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                (*m_canvas)->save();
                (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                       args.getStart().getY() + args.getEnd().getY() / 2.0f);
                (*m_canvas)->rotate(SkRadiansToDegrees(2.0f * PI * (i + 0.5f) / args.getSample().size() + ROTATION));
                SkRect rect{ SkRect::MakeXYWH(-barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) / 2.0f + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                             innerRadius + radius / 10.0f * 9 * args.getSample()[i] + radius / 10.0f * (m_drawingArea.getItemSpacing() / 100.0f) + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                             barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                                             radius / 10.0f * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS)) };
                (*m_canvas)->drawRoundRect(rect, rx, ry, args.getPaint());
                (*m_canvas)->restore();
            }
        }
    }

    void Renderer::drawBars(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
            float fill{ m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f };
            SkPath path;
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                if(args.getSample()[i] == 0)
                {
                    continue;
                }
                SkRect rect;
                switch(m_drawingArea.getDirection())
                {
                case DrawingDirection::TopToBottom:
                    rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             (m_drawingArea.getFillShape() ? args.getStart().getY() : args.getStart().getY() + LINE_THICKNESS / 2.0f) - 1,
                             args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill + step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                             args.getStart().getY() + args.getEnd().getY() * args.getSample()[i] - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) + 1 };
                    break;
                case DrawingDirection::BottomToTop:
                    rect = { args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getY() + args.getEnd().getY() * (1 - args.getSample()[i]) + fill,
                             args.getStart().getX() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill + step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                             args.getStart().getY() + args.getEnd().getY() - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) + 1 };
                    break;
                case DrawingDirection::LeftToRight:
                    rect = { m_drawingArea.getFillShape() ? args.getStart().getX() : args.getStart().getX() + LINE_THICKNESS / 2.0f,
                             args.getStart().getY() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getX() + args.getEnd().getX() * args.getSample()[i] - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                             args.getStart().getY() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill + step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) };
                    break;
                case DrawingDirection::RightToLeft:
                    rect = { args.getStart().getX() + args.getEnd().getX() * (1 - args.getSample()[i]) + fill,
                             args.getStart().getY() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill,
                             args.getStart().getX() + args.getEnd().getX() - fill,
                             args.getStart().getY() + step * (i + (m_drawingArea.getItemSpacing() / 100.0f)) + fill + step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) };
                    break;
                }
                path.addRect(rect);
            }
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        else if(args.getMode() == DrawingMode::Circle)
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float radius{ fullRadius - innerRadius };
            float barWidth{ 2.0f * PI * innerRadius / args.getSample().size() };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                (*m_canvas)->save();
                (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                       args.getStart().getY() + args.getEnd().getY() / 2.0f);
                (*m_canvas)->rotate(SkRadiansToDegrees(2.0f * PI * (i + 0.5f) / args.getSample().size() + ROTATION));
                SkRect rect{ SkRect::MakeXYWH(-barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) / 2.0f + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                             innerRadius + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                             barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS),
                                             radius * args.getSample()[i] - (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS) + 1) };
                (*m_canvas)->drawRect(rect, args.getPaint());
                (*m_canvas)->restore();
            }
        }
    }

    void Renderer::drawSpine(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
            float itemSize{ step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                if(args.getSample()[i] == 0)
                {
                    continue;
                }
                float r{ itemSize * args.getSample()[i] / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
                SkRect rect;
                switch(m_drawingArea.getDirection())
                {
                case DrawingDirection::TopToBottom:
                case DrawingDirection::BottomToTop:
                    rect = SkRect::MakeXYWH(args.getStart().getX() + step * (i + 0.5f) + (1 - itemSize * args.getSample()[i]) / 2.0f,
                                            args.getStart().getY() + args.getEnd().getY() / 2.0f - itemSize * args.getSample()[i] / 2.0f,
                                            itemSize * args.getSample()[i],
                                            itemSize * args.getSample()[i]);
                    break;
                case DrawingDirection::LeftToRight:
                case DrawingDirection::RightToLeft:
                    rect = SkRect::MakeXYWH(args.getStart().getX() + args.getEnd().getX() / 2.0f - itemSize * args.getSample()[i] / 2.0f,
                                            args.getStart().getY() + step * (i + 0.5f) + (1 - itemSize * args.getSample()[i]) / 2.0f,
                                            itemSize * args.getSample()[i],
                                            itemSize * args.getSample()[i]);
                    break;
                }
                (*m_canvas)->drawRoundRect(rect, r, r, getPaintForSpine(args.getPaint(), args.getSample()[i]));
            }
        }
        else if(args.getMode() == DrawingMode::Circle)
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float barWidth{ 2.0f * PI * innerRadius / args.getSample().size() };
            float itemSize{ barWidth * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                float r{ itemSize * args.getSample()[i] / 2.0f * (m_drawingArea.getItemRoundness() / 100.0f) };
                (*m_canvas)->save();
                (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                       args.getStart().getY() + args.getEnd().getY() / 2.0f);
                (*m_canvas)->rotate(SkRadiansToDegrees(2.0f * PI * (i + 0.5f) / args.getSample().size() + ROTATION));
                SkRect rect{ SkRect::MakeXYWH(-barWidth * (1 - (m_drawingArea.getItemSpacing() * 2.0f / 100.0f)) / 2.0f + (m_drawingArea.getFillShape() ? 0.0f : LINE_THICKNESS / 2.0f),
                                              innerRadius - itemSize * args.getSample()[i] / 2.0f,
                                              itemSize * args.getSample()[i],
                                              itemSize * args.getSample()[i]) };
                (*m_canvas)->drawRoundRect(rect, r, r, getPaintForSpine(args.getPaint(), args.getSample()[i]));
                (*m_canvas)->restore();
            }
        }
    }

    void Renderer::drawSplitter(const DrawingFunctionArguments& args)
    {
        float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
        float orient{ 1.0f };
        SkPath path;
        switch(m_drawingArea.getDirection())
        {
        case DrawingDirection::TopToBottom:
            path.moveTo(args.getStart().getX(),
                        args.getStart().getY() + args.getEnd().getY() / 2.0f * (1 + args.getSample()[0]));
            break;
        case DrawingDirection::BottomToTop:
            orient = -1;
            path.moveTo(args.getStart().getX(),
                        args.getStart().getY() + args.getEnd().getY() / 2.0f * (1 + args.getSample()[0] * orient));
            break;
        case DrawingDirection::LeftToRight:
            path.moveTo(args.getStart().getX() + args.getEnd().getX() / 2.0f * (1 + args.getSample()[0]),
                        args.getStart().getY());
            break;
        case DrawingDirection::RightToLeft:
            orient = -1;
            path.moveTo(args.getStart().getX() + args.getEnd().getX() / 2.0f * (1 + args.getSample()[0] * orient),
                        args.getStart().getY());
            break;
        }
        for(size_t i = 0; i < args.getSample().size(); i++)
        {
            switch(m_drawingArea.getDirection())
            {
            case DrawingDirection::TopToBottom:
            case DrawingDirection::BottomToTop:
            {
                if(i > 0)
                {
                    path.lineTo(args.getStart().getX() + step * i,
                                args.getStart().getY() + args.getEnd().getY() / 2.0f);
                }
                path.lineTo(args.getStart().getX() + step * i,
                            args.getStart().getY() + args.getEnd().getY() / 2.0f * (1 + args.getSample()[i] * (i % 2 == 0 ? orient : -orient)));
                path.lineTo(args.getStart().getX() + step * (i + 1),
                            args.getStart().getY() + args.getEnd().getY() / 2.0f * (1 + args.getSample()[i] * (i % 2 == 0 ? orient : -orient)));
                if(i < args.getSample().size() - 1)
                {
                    path.lineTo(args.getStart().getX() + step * (i + 1),
                                args.getStart().getY() + args.getEnd().getY() / 2.0f);
                }
                break;
            }
            case DrawingDirection::LeftToRight:
            case DrawingDirection::RightToLeft:
            {
                if(i > 0)
                {
                    path.lineTo(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                args.getStart().getY() + step * i);
                }
                path.lineTo(args.getStart().getX() + args.getEnd().getX() / 2.0f * (1 + args.getSample()[i] * (i % 2 == 0 ? orient : -orient)),
                            args.getStart().getY() + step * i);
                path.lineTo(args.getStart().getX() + args.getEnd().getX() / 2.0f * (1 + args.getSample()[i] * (i % 2 == 0 ? orient : -orient)),
                            args.getStart().getY() + step * (i + 1));
                if(i < args.getSample().size() - 1)
                {
                    path.lineTo(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                args.getStart().getY() + step * (i + 1));
                }
                break;
            }
            }
        }
        if(!m_drawingArea.getFillShape())
        {
            (*m_canvas)->drawPath(path, args.getPaint());
        }
        switch(m_drawingArea.getDirection())
        {
        case DrawingDirection::TopToBottom:
            path.lineTo(args.getStart().getX() + args.getEnd().getX(), args.getStart().getY());
            path.lineTo(args.getStart().getX(), args.getStart().getY());
            break;
        case DrawingDirection::BottomToTop:
            path.lineTo(args.getStart().getX() + args.getEnd().getX(), args.getStart().getY() + args.getEnd().getY());
            path.lineTo(args.getStart().getX(), args.getStart().getY() + args.getEnd().getY());
            break;
        case DrawingDirection::LeftToRight:
            path.lineTo(args.getStart().getX(), args.getStart().getY() + args.getEnd().getY());
            path.lineTo(args.getStart().getX(), args.getStart().getY());
            break;
        case DrawingDirection::RightToLeft:
            path.lineTo(args.getStart().getX() + args.getEnd().getX(), args.getStart().getY() + args.getEnd().getY());
            path.lineTo(args.getStart().getX() + args.getEnd().getX(), args.getStart().getY());
            break;
        }
        path.close();
        if(m_drawingArea.getFillShape())
        {
            (*m_canvas)->drawPath(path, args.getPaint());
        }
    }

    void Renderer::drawHearts(const DrawingFunctionArguments& args)
    {
        if(args.getMode() == DrawingMode::Box)
        {
            float step{ (args.getDirection() < DrawingDirection::LeftToRight ? args.getEnd().getX() : args.getEnd().getY()) / args.getSample().size() };
            float itemSize{ step * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                if(args.getSample()[i] == 0)
                {
                    continue;
                }
                (*m_canvas)->save();
                switch(m_drawingArea.getDirection())
                {
                case DrawingDirection::TopToBottom:
                case DrawingDirection::BottomToTop:
                    (*m_canvas)->translate(args.getStart().getX() + step * i + step / 2.0f,
                                           args.getStart().getY() + args.getEnd().getY() / 2.0f);
                    break;
                case DrawingDirection::LeftToRight:
                case DrawingDirection::RightToLeft:
                    (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f,
                                           args.getStart().getY() + step * i + step / 2.0f);
                    break;
                }
                (*m_canvas)->scale(args.getSample()[i], args.getSample()[i]);
                (*m_canvas)->drawPath(getHeartPath(itemSize), getPaintForSpine(args.getPaint(), args.getSample()[i]));
                (*m_canvas)->restore();
            }
        }
        else if(args.getMode() == DrawingMode::Circle)
        {
            float fullRadius{ std::min(args.getEnd().getX(), args.getEnd().getY()) / 2.0f };
            float innerRadius{ fullRadius * INNER_RADIUS };
            float barWidth{ 2.0f * PI * innerRadius / args.getSample().size() };
            float itemSize{ barWidth * (1 - (m_drawingArea.getItemSpacing() / 100.0f) * 2) - (m_drawingArea.getFillShape() ? 0 : LINE_THICKNESS) };
            for(size_t i = 0; i < args.getSample().size(); i++)
            {
                (*m_canvas)->save();
                (*m_canvas)->translate(args.getStart().getX() + args.getEnd().getX() / 2.0f + innerRadius * std::cos(ROTATION + PI / 2.0f + PI * 2.0f * i / args.getSample().size()),
                                       args.getStart().getY() + args.getEnd().getY() / 2.0f + innerRadius * std::sin(PI / 2.0f + PI * 2 * i / args.getSample().size()));
                (*m_canvas)->scale(args.getSample()[i], args.getSample()[i]);
                (*m_canvas)->drawPath(getHeartPath(itemSize), getPaintForSpine(args.getPaint(), args.getSample()[i]));
                (*m_canvas)->restore();
            }
        }
    }
}
