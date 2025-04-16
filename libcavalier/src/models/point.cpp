#include "models/point.h"

namespace Nickvision::Cavalier::Shared::Models
{
    Point::Point()
        : m_x{ 0 },
        m_y{ 0 }
    {

    }

    Point::Point(float x, float y)
        : m_x{ x },
        m_y{ y }
    {

    }

    float Point::getX() const
    {
        return m_x;
    }

    void Point::setX(float x)
    {
        m_x = x;
    }

    float Point::getY() const
    {
        return m_y;
    }

    void Point::setY(float y)
    {
        m_y = y;
    }
}
