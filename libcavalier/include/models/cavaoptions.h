#ifndef CAVAOPTIONS_H
#define CAVAOPTIONS_H

#include <string>
#include <boost/json.hpp>
#include "channeltype.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of options for the cava executable.
     */
    class CavaOptions
    {
    public:
        /**
         * @brief Constructs a CavaOptions.
         */
        CavaOptions();
        /**
         * @brief Constructs a CavaOptions from a json object.
         * @param json The json object
         */
        CavaOptions(boost::json::object json);
        /**
         * @brief Gets the framerate to use for cava.
         * @return The framerate
         */
        unsigned int getFramerate() const;
        /**
         * @brief Sets the framerate to use for cava.
         * @param framerate The new framerate
         */
        void setFramerate(unsigned int framerate);
        /**
         * @brief Gets the number of bars to use for cava. (6 - 100)
         * @return The number of bars
         */
        unsigned int getNumberOfBars() const;
        /**
         * @brief Sets the number of bars to use for cava. (6 - 100)
         * @param bars The new number of bars
         */
        void setNumberOfBars(unsigned int bars);
        /**
         * @brief Gets whether or not to reverse the order of bars from cava.
         * @return True to reverse bar order
         * @return False to not reverse bar order
         */
        bool getReverseBarOrder() const;
        /**
         * @brief Sets whether or not to reverse the order of bars from cava.
         * @param reverse True to reverse bar order, else false
         */
        void setReverseBarOrder(bool reverse);
        /**
         * @brief Gets whether or not to use cava's automatic sensitivity detection.
         * @return True to use automatic sensitivity
         * @return False to not use automatic sensitivity
         */
        bool getUseAutomaticSensitivity() const;
        /**
         * @brief Sets whether or not to use cava's automatic sensitivity detection.
         * @param sensitivity True to use automatic sensitivity, else flase
         */
        void setUseAutomaticSensitivity(bool sensitivity);
        /**
         * @brief Gets the sensitivity for cava. (10 - 100)
         * @return The sensitivity
         */
        unsigned int getSensitivity() const;
        /**
         * @brief Sets the sensitivity for cava. (10 - 100)
         * @param sensitivity The new sensitivity
         */
        void setSensitivity(unsigned int sensitivity);
        /**
         * @brief Gets the channels to use for cava.
         * @return The channels
         */
        ChannelType getChannels() const;
        /**
         * @brief Sets the channels to use for cava.
         * @param type The new channels
         */
        void setChannels(ChannelType type);
        /**
         * @brief Gets whether or not to use cava's monstercat smoothing.
         * @return True to use monstercat smoothing
         * @return False to not use monstercar smoothing
         */
        bool getUseMonstercatSmoothing() const;
        /**
         * @brief Sets whether or not to use cava's monstercat smoothing.
         * @param monstercat True to use monstercat smoothing, else false
         */
        void setUseMonstercatSmoothing(bool monstercat);
        /**
         * @brief Gets the noise reduction factor for cava. (15 - 95)
         * @return The noise reduction factor
         */
        unsigned int getNoiseReductionFactor() const;
        /**
         * @brief Sets the noise reduction factor for cava. (15 - 95)
         * @param noiseReduction The new noise reduction factor
         */
        void setNoiseReductionFactor(unsigned int noiseReduction);
        /**
         * @brief Gets the string to pass to cava for options.
         * @return The options string
         */
        std::string toCavaOptionsString() const;
        /**
         * @brief Converts the CavaOptions to a JSON object.
         * @return The JSON object
         */
        boost::json::object toJson() const;

    private:
        unsigned int m_framerate;
        unsigned int m_numberOfBars;
        bool m_reverseBarOrder;
        bool m_useAutomaticSensitivity;
        unsigned int m_sensitivity;
        ChannelType m_channels;
        bool m_useMonstercatSmoothing;
        unsigned int m_noiseReductionFactor;
    };
}

#endif //CAVAOPTIONS_H
