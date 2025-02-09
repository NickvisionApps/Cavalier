#include "models/cavaoptions.h"
#include <cmath>
#include <sstream>
#include <libnick/system/environment.h>

using namespace Nickvision::System;

namespace Nickvision::Cavalier::Shared::Models
{
    CavaOptions::CavaOptions()
        : m_framerate{ 60 },
        m_numberOfBars{ 12 },
        m_reverseBarOrder{ false },
        m_useAutomaticSensitivity{ true },
        m_sensitivity{ 10 },
        m_channels{ ChannelType::Stereo },
        m_useMonstercatSmoothing{ true },
        m_noiseReductionFactor{ 77 }
    {

    }

    CavaOptions::CavaOptions(boost::json::object json)
        : m_framerate{ json["Framerate"].is_uint64() ? json["Framerate"].as_uint64() : 60 },
        m_numberOfBars{ json["NumberOfBars"].is_uint64() ? json["NumberOfBars"].as_uint64() : 12 },
        m_reverseBarOrder{ json["ReverseBarOrder"].is_bool() ? json["ReverseBarOrder"].as_bool() : false },
        m_useAutomaticSensitivity{ json["UseAutomaticSensitivity"].is_bool() ? json["UseAutomaticSensitivity"].as_bool() : true },
        m_sensitivity{ json["Sensitivity"].is_uint64() ? json["Sensitivity"].as_uint64() : 10 },
        m_channels{ json["Channels"].is_int64() ? static_cast<ChannelType>(json["Channels"].as_int64()) : ChannelType::Stereo },
        m_useMonstercatSmoothing{ json["UseMonstercatSmoothing"].is_bool() ? json["UseMonstercatSmoothing"].as_bool() : true },
        m_noiseReductionFactor{ json["NoiseReductionFactor"].is_uint64() ? json["NoiseReductionFactor"].as_uint64() : 77 }
    {

    }

    unsigned int CavaOptions::getFramerate() const
    {
        return m_framerate;
    }

    void CavaOptions::setFramerate(unsigned int framerate)
    {
        m_framerate = framerate;
    }

    unsigned int CavaOptions::getNumberOfBars() const
    {
        return m_numberOfBars;
    }

    void CavaOptions::setNumberOfBars(unsigned int bars)
    {
        if(bars < 6 || bars > 100)
        {
            bars = 12;
        }
        m_numberOfBars = bars;
    }

    bool CavaOptions::getReverseBarOrder() const
    {
        return m_reverseBarOrder;
    }

    void CavaOptions::setReverseBarOrder(bool reverse)
    {
        m_reverseBarOrder = reverse;
    }

    bool CavaOptions::getUseAutomaticSensitivity() const
    {
        return m_useAutomaticSensitivity;
    }

    void CavaOptions::setUseAutomaticSensitivity(bool sensitivity)
    {
        m_useAutomaticSensitivity = sensitivity;
    }

    unsigned int CavaOptions::getSensitivity() const
    {
        return m_sensitivity;
    }

    void CavaOptions::setSensitivity(unsigned int sensitivity)
    {
        if(sensitivity < 10 || sensitivity > 100)
        {
            sensitivity = 10;
        }
        m_sensitivity = sensitivity;
    }

    ChannelType CavaOptions::getChannels() const
    {
        return m_channels;
    }

    void CavaOptions::setChannels(ChannelType type)
    {
        m_channels = type;
    }

    bool CavaOptions::getUseMonstercatSmoothing() const
    {
        return m_useMonstercatSmoothing;
    }

    void CavaOptions::setUseMonstercatSmoothing(bool monstercat)
    {
        m_useMonstercatSmoothing = monstercat;
    }

    unsigned int CavaOptions::getNoiseReductionFactor() const
    {
        return m_noiseReductionFactor;
    }

    void CavaOptions::setNoiseReductionFactor(unsigned int noiseReduction)
    {
        if(noiseReduction < 15 || noiseReduction > 95)
        {
            noiseReduction = 77;
        }
        m_noiseReductionFactor = noiseReduction;
    }

    std::string CavaOptions::toCavaOptionsString() const
    {
        std::stringstream builder;
        builder << "[general]" << std::endl;
        builder << "framerate = " << m_framerate << std::endl;
        builder << "bars = " << m_numberOfBars * 2 << std::endl;
        builder << "autosens = " << (m_useAutomaticSensitivity ? "1" : "0") << std::endl;
        builder << "sensitivity = " << std::pow(m_sensitivity, 2) << std::endl;
        if(Environment::getOperatingSystem() != OperatingSystem::Windows)
        {
            builder << "[input]" << std::endl;
            builder << "method = " << (Environment::hasVariable("CAVALIER_INPUT_METHOD") ? Environment::getVariable("CAVALIER_INPUT_METHOD") : "pulse") << std::endl;
            builder << "source = " << (Environment::hasVariable("CAVALIER_INPUT_SOURCE") ? Environment::getVariable("CAVALIER_INPUT_SOURCE") : "auto") << std::endl;
        }
        builder << "[output]" << std::endl;
        builder << "method = raw" << std::endl;
        builder << "bit_format = 16bit" << std::endl;
        builder << "raw_target = /dev/stdout" << std::endl;
        builder << "data_format = ascii" << std::endl;
        builder << "ascii_max_range = " << CAVA_ASCII_MAX_RANGE << std::endl;
        builder << "bar_delimiter = " << CAVA_BAR_DELIMITER << std::endl;
        builder << "frame_delimiter = " << CAVA_FRAME_DELIMITER << std::endl;
        builder << "channels = " << (m_channels == ChannelType::Mono ? "mono" : "stereo") << std::endl;
        builder << "reverse = " << (m_reverseBarOrder ? "1" : "0") << std::endl;
        builder << "[smoothing]" << std::endl;
        builder << "monstercat = " << (m_useMonstercatSmoothing ? "1" : "0") << std::endl;
        builder << "noise_reduction = " << m_noiseReductionFactor << std::endl;
        return builder.str();
    }

    boost::json::object CavaOptions::toJson() const
    {
        boost::json::object obj;
        obj["Framerate"] = m_framerate;
        obj["NumberOfBars"] = m_numberOfBars;
        obj["ReverseBarOrder"] = m_reverseBarOrder;
        obj["UseAutomaticSensitivity"] = m_useAutomaticSensitivity;
        obj["Sensitivity"] = m_sensitivity;
        obj["Channels"] = static_cast<int>(m_channels);
        obj["UseMonstercatSmoothing"] = m_useMonstercatSmoothing;
        obj["NoiseReductionFactor"] = m_noiseReductionFactor;
        return obj;
    }
}
