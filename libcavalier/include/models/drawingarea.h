#ifndef DRAWINGAREA_H
#define DRAWINGAREA_H

#include <boost/json.hpp>
#include "drawingdirection.h"
#include "drawingmode.h"
#include "drawingshape.h"
#include "mirrormode.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of the area (space) where the audio is drawn.
     */
    class DrawingArea
    {
    public:
        /**
         * @brief Constructs a DrawingArea.
         */
        DrawingArea();
        /**
         * @brief Constructs a DrawingArea from a json object.
         * @param json The json object
         */
        DrawingArea(boost::json::object json);
        /**
         * @brief Gets the drawing mode of the area.
         * @return The area's drawing mode
         */
        DrawingMode getMode() const;
        /**
         * @brief Sets the drawing mode of the area.
         * @param mode The new drawing mode
         */
        void setMode(DrawingMode mode);
        /**
         * @brief Gets the drawing shape of the area.
         * @return The area's drawing shape
         */
        DrawingShape getShape() const;
        /**
         * @brief Sets the drawing shape of the area.
         * @param shape The new drawing shape
         */
        void setShape(DrawingShape shape);
        /**
         * @brief Gets the drawing direction of the area.
         * @return The area's drawing direction
         */
        DrawingDirection getDirection() const;
        /**
         * @brief Sets the drawing direction of the area.
         * @param direction The new drawing direction
         */
        void setDirection(DrawingDirection direction);
        /**
         * @brief Gets whether or not the area should fill in shapes or draw their outlines only.
         * @return True to fill in shape
         * @return False for outlines only
         */
        bool getFillShape() const;
        /**
         * @brief Sets whether or not the area should fill in shapes or draw their outlines only.
         * @param filleShape Set true to fill in shape or false for outlines only
         */
        void setFillShape(bool fillShape);
        /**
         * @brief Gets the mirror mode of the area.
         * @return The area's mirror mode
         */
        MirrorMode getMirrorMode() const;
        /**
         * @brief Sets the mirror mode of the area.
         * @param mode The new mirror mode
         */
        void setMirrorMode(MirrorMode mode);
        /**
         * @brief Gets the margin of the area. (0 - 40 px)
         * @return The area's margin
         */
        unsigned int getMargin() const;
        /**
         * @brief Sets the margin of the area. (0 - 40 px)
         * @param margin The new margin
         */
        void setMargin(unsigned int margin);
        /**
         * @brief Gets the x offset of the area. (-5 - 5)
         * @return The area's x offset
         */
        int getXOffset() const;
        /**
         * @brief Sets the x offset of the area. (-5 - 5)
         * @param offset The new x offset
         */
        void setXOffset(int offset);
        /**
         * @brief Gets the y offset of the area. (-5 - 5)
         * @return The area's y offset
         */
        int getYOffset() const;
        /**
         * @brief Sets the y offset of the area. (-5 - 5)
         * @param offset The new y offset
         */
        void setYOffset(int offset);
        /**
         * @brief Gets the item spacing of the area. (0 - 20 %)
         * @return The area's item spacing
         */
        unsigned int getItemSpacing() const;
        /**
         * @brief Sets the item spacing of the area. (0 - 20 %)
         * @param spacing The new item spacing
         */
        void setItemSpacing(unsigned int spacing);
        /**
         * @brief Gets the item roundness of the area. (0 - 100 %)
         * @return The area's item roundness
         */
        unsigned int getItemRoundness() const;
        /**
         * @brief Sets the item roundness of the area. (0 - 100 %)
         * @param roundness The new item roundness
         */
        void setItemRoundness(unsigned int roundness);
        /**
         * @brief Converts the DrawingArea to a JSON object.
         * @return The JSON object
         */
        boost::json::object toJson() const;

    private:
        DrawingMode m_mode;
        DrawingShape m_shape;
        DrawingDirection m_direction;
        bool m_fillShape;
        MirrorMode m_mirrorMode;
        unsigned int m_margin;
        int m_xOffset;
        int m_yOffset;
        unsigned int m_itemSpacing;
        unsigned int m_itemRoundness;
    };
}

#endif //DRAWINGAREA_H
