#include "models/cava.h"
#include <algorithm>
#include <fstream>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/system/environment.h>

using namespace Nickvision::Events;
using namespace Nickvision::Filesystem;
using namespace Nickvision::Helpers;
using namespace Nickvision::System;

#define CAVA_RAW_MAX 65530.0f

namespace Nickvision::Cavalier::Shared::Models
{
    Cava::Cava(const CavaOptions& options, const std::string& appName)
        : m_options{ options },
        m_isRecevingAudio{ false },
        m_configPath{ UserDirectories::get(ApplicationUserDirectory::Config, appName) / "cava_config" }
    {
        updateConfigFile();
    }

    Cava::~Cava()
    {
        if(m_process && m_process->kill())
        {
            m_process->waitForExit();
        }
        if(m_watcher.joinable())
        {
            m_watcher.join();
        }
    }

    Event<ParamEventArgs<std::vector<float>>>& Cava::outputReceived()
    {
        return m_outputReceived;
    }

    bool Cava::isRecevingAudio() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_isRecevingAudio;
    }

    const CavaOptions& Cava::getOptions() const
    {
        std::lock_guard<std::mutex> lock{ m_mutex };
        return m_options;
    }

    void Cava::setOptions(const CavaOptions& options)
    {
        std::unique_lock<std::mutex> lock{ m_mutex };
        m_options = options;
        updateConfigFile();
        lock.unlock();
        if(m_process->isRunning())
        {
            start();
        }
    }

    void Cava::start()
    {
        if(m_process && m_process->kill())
        {
            m_process->waitForExit();
        }
        if(m_watcher.joinable())
        {
            m_watcher.join();
        }
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_process.reset();
        std::vector<std::string> arguments;
        arguments.push_back("-p");
        arguments.push_back(m_configPath.string());
        m_process = std::make_shared<Process>(Environment::findDependency("cava"), arguments);
        m_process->start();
        m_watcher = std::thread(&Cava::watch, this);
    }

    void Cava::updateConfigFile()
    {
        std::ofstream file{ m_configPath, std::ios::trunc };
        file << m_options.toCavaOptionsString() << std::endl;
    }

    void Cava::watch()
    {
        bool sentEmptyOutput{ false };
        while(m_process->isRunning())
        {
            if(m_process->getOutput().empty()) //Wait for startup
            {
                std::this_thread::sleep_for(std::chrono::milliseconds(5));
                continue;
            }
            std::unique_lock<std::mutex> lock{ m_mutex };
            std::vector<std::string> frames{ StringHelpers::split(m_process->getOutput(), std::string(1, CAVA_FRAME_DELIMITER)) };
            std::string lastFrame{ frames[frames.size() - 1] };
            size_t numberOfBarsInFrame{ static_cast<size_t>(m_options.getNumberOfBars()) * 2 };
            if(std::count(lastFrame.begin(), lastFrame.end(), CAVA_BAR_DELIMITER) != numberOfBarsInFrame) //Frame not complete
            {
                lastFrame = frames[frames.size() - 2];
            }
            std::vector<std::string> bars{ StringHelpers::split(lastFrame, std::string(1, CAVA_BAR_DELIMITER)) };
            std::vector<float> sample(numberOfBarsInFrame);
            for(size_t i = 0; i < numberOfBarsInFrame; i++)
            {
                sample[i] = std::stof(bars[i].empty() ? "0" : bars[i]) / CAVA_ASCII_MAX_RANGE;
            }
            if(std::count(sample.begin(), sample.end(), 0.0f) == numberOfBarsInFrame) //No audio being received
            {
                m_isRecevingAudio = false;
                lock.unlock();
                if(!sentEmptyOutput)
                {
                    m_outputReceived.invoke({{}});
                    sentEmptyOutput = true;
                }
            }
            else
            {
                m_isRecevingAudio = true;
                lock.unlock();
                sentEmptyOutput = false;
                m_outputReceived.invoke({ sample });
            }
        }
        std::lock_guard<std::mutex> lock{ m_mutex };
        m_isRecevingAudio = false;
    }
}
