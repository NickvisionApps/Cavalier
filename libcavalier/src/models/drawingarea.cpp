#include "models/drawingarea.h"

namespace Nickvision::Cavalier::Shared::Models
{
    DrawingArea::DrawingArea()
        : m_mode{ DrawingMode::Box },
        m_shape{ DrawingShape::Wave },
        m_direction{ DrawingDirection::BottomToTop },
        m_fillShape{ true },
        m_mirrorMode{ MirrorMode::Off },
        m_margin{ 0 },
        m_xOffset{ 0 },
        m_yOffset{ 0 },
        m_itemSpacing{ 10 },
        m_itemRoundness{ 50 }
    {

    }

    DrawingArea::DrawingArea(boost::json::object json)
        : m_mode{ json["Mode"].is_int64() ? static_cast<DrawingMode>(json["Mode"].as_int64()) : DrawingMode::Box },
        m_shape{ json["Shape"].is_int64() ? static_cast<DrawingShape>(json["Shape"].as_int64()) : DrawingShape::Wave },
        m_direction{ json["Direction"].is_int64() ? static_cast<DrawingDirection>(json["Direction"].as_int64()) : DrawingDirection::BottomToTop },
        m_fillShape{ json["FillShape"].is_bool() ? json["FillShape"].as_bool() : true },
        m_mirrorMode{ json["MirrorMode"].is_int64() ? static_cast<MirrorMode>(json["MirrorMode"].as_int64()) : MirrorMode::Off },
        m_margin{ json["Margin"].is_uint64() ? json["Margin"].as_uint64() : 0 },
        m_xOffset{ json["XOffset"].is_int64() ? json["XOffset"].as_int64() : 0 },
        m_yOffset{ json["YOffset"].is_int64() ? json["YOffset"].as_int64() : 0 },
        m_itemSpacing{ json["ItemSpacing"].is_uint64() ? json["ItemSpacing"].as_uint64() : 10 },
        m_itemRoundness{ json["ItemRoundness"].is_uint64() ? json["ItemRoundness"].as_uint64() : 50 }
    {

    }

    DrawingMode DrawingArea::getMode() const
    {
        return m_mode;
    }

    void DrawingArea::setMode(DrawingMode mode)
    {
        m_mode = mode;
    }

    DrawingShape DrawingArea::getShape() const
    {
        return m_shape;
    }

    void DrawingArea::setShape(DrawingShape shape)
    {
        m_shape = shape;
    }

    DrawingDirection DrawingArea::getDirection() const
    {
        return m_direction;
    }

    void DrawingArea::setDirection(DrawingDirection direction)
    {
        m_direction = direction;
    }

    bool DrawingArea::getFillShape() const
    {
        return m_fillShape;
    }

    void DrawingArea::setFillShape(bool fillShape)
    {
        m_fillShape = fillShape;
    }

    MirrorMode DrawingArea::getMirrorMode() const
    {
        return m_mirrorMode;
    }

    void DrawingArea::setMirrorMode(MirrorMode mode)
    {
        m_mirrorMode = mode;
    }

    unsigned int DrawingArea::getMargin() const
    {
        return m_margin;
    }

    void DrawingArea::setMargin(unsigned int margin)
    {
        if(margin > 40)
        {
            margin = 0;
        }
        m_margin = margin;
    }

    int DrawingArea::getXOffset() const
    {
        return m_xOffset;
    }

    void DrawingArea::setXOffset(int offset)
    {
        if(offset < -5 || offset > 5)
        {
            offset = 0;
        }
        m_xOffset = offset;
    }

    int DrawingArea::getYOffset() const
    {
        return m_yOffset;
    }

    void DrawingArea::setYOffset(int offset)
    {
        if(offset < -5 || offset > 5)
        {
            offset = 0;
        }
        m_yOffset = offset;
    }

    unsigned int DrawingArea::getItemSpacing() const
    {
        return m_itemSpacing;
    }

    void DrawingArea::setItemSpacing(unsigned int spacing)
    {
        if(spacing > 20)
        {
            spacing = 10;
        }
        m_itemSpacing = spacing;
    }

    unsigned int DrawingArea::getItemRoundness() const
    {
        return m_itemRoundness;
    }

    void DrawingArea::setItemRoundness(unsigned int roundness)
    {
        if(roundness > 100)
        {
            roundness = 50;
        }
        m_itemRoundness = roundness;
    }

    boost::json::object DrawingArea::toJson() const
    {
        boost::json::object obj;
        obj["Mode"] = static_cast<int>(m_mode);
        obj["Shape"] = static_cast<int>(m_shape);
        obj["Direction"] = static_cast<int>(m_direction);
        obj["FillShape"] = m_fillShape;
        obj["MirrorMode"] = static_cast<int>(m_mirrorMode);
        obj["Margin"] = m_margin;
        obj["XOffset"] = m_xOffset;
        obj["YOffset"] = m_yOffset;
        obj["ItemSpacing"] = m_itemSpacing;
        obj["ItemRoundness"] = m_itemRoundness;
        return obj;
    }
}
