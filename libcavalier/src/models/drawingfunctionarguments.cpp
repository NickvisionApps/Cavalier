#include "models/drawingfunctionarguments.h"

namespace Nickvision::Cavalier::Shared::Models
{
    DrawingFunctionArguments::DrawingFunctionArguments(const std::vector<float>& sample, DrawingMode mode, DrawingDirection direction, const Point& start, const Point& end, float rotation, const SkPaint& paint)
        : m_sample{ sample },
        m_mode{ mode },
        m_direction{ direction },
        m_start{ start },
        m_end{ end },
        m_rotation{ rotation },
        m_paint{ paint }
    {

    }

    const std::vector<float>& DrawingFunctionArguments::getSample() const
    {
        return m_sample;
    }

    DrawingMode DrawingFunctionArguments::getMode() const
    {
        return m_mode;
    }

    DrawingDirection DrawingFunctionArguments::getDirection() const
    {
        return m_direction;
    }

    const Point& DrawingFunctionArguments::getStart() const
    {
        return m_start;
    }

    const Point& DrawingFunctionArguments::getEnd() const
    {
        return m_end;
    }

    float DrawingFunctionArguments::getRotation() const
    {
        return m_rotation;
    }

    const SkPaint& DrawingFunctionArguments::getPaint() const
    {
        return m_paint;
    }
}
