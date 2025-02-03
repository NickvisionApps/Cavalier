#ifndef POINT_H
#define POINT_H

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a coordinate point (x, y).
     */
    class Point
    {
    public:
        /**
         * @brief Constructs a Point.
         * @brief Default: (0,0)
         */
        Point();
        /**
         * @brief Constructs a Point.
         * @param x The x value of the point
         * @param y The y value of the point
         */
        Point(float x, float y);
        /**
         * @brief Gets the x value of the point.
         * @return The x value
         */
        float getX() const;
        /**
         * @brief Sets the x value of the point.
         * @param x The new x value
         */
        void setX(float x);
        /**
         * @brief Gets the y value of the point.
         * @return The y value
         */
        float getY() const;
        /**
         * @brief Sets the y value of the point.
         * @param y The new y value
         */
        void setY(float y);

    private:
        float m_x;
        float m_y;
    };
}

#endif //POINT_H
