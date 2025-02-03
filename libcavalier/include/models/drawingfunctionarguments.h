#ifndef DRAWINGFUNCTIONARGUMENTS_H
#define DRAWINGFUNCTIONARGUMENTS_H

#include <vector>
#include <skia/include/core/SkPaint.h>
#include <skia/include/core/SkRefCnt.h>
#include "drawingdirection.h"
#include "point.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of arguments for a renderer's drawing function.
     */
    class DrawingFunctionArguments
    {
    public:
        /**
         * @brief Constructs a DrawingFunctionArguments.
         * @param sample The cava sample
         * @param direction The drawing direction
         * @param start The starting drawing point (top-left corner)
         * @param end The ending drawing point (bottom-right corner)
         * @param rotation Rotation angle in radians (used for circle modes)
         * @param paint Skia paint brush
         */
        DrawingFunctionArguments(const std::vector<float>& sample, DrawingDirection direction, const Point& start, const Point& end, float rotation, const SkPaint& paint);
        /**
         * @brief Gets the cava sample.
         * @return The cava sample
         */
        const std::vector<float>& getSample() const;
        /**
         * @brief Gets the drawing direction.
         * @return The drawing direction
         */
        DrawingDirection getDirection() const;
        /**
         * @brief Gets the starting drawing point.
         * @return The start point
         */
        const Point& getStart() const;
        /**
         * @brief Gets the ending drawing point.
         * @return The end point
         */
        const Point& getEnd() const;
        /**
         * @brief Gets the rotation angle in radians.
         * @return The rotation angle
         */
        float getRotation() const;
        /**
         * @brief Gets the skia paint brush.
         * @return The skia paint brush
         */
        const SkPaint& getPaint() const;

    private:
        std::vector<float> m_sample;
        DrawingDirection m_direction;
        Point m_start;
        Point m_end;
        float m_rotation;
        SkPaint m_paint;
    };
}

#endif //DRAWINGFUNCTIONARGUMENTS_H
