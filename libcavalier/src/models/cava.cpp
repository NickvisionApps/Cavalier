#include "models/cava.h"
#include <fstream>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/system/environment.h>

using namespace Nickvision::Events;
using namespace Nickvision::Filesystem;
using namespace Nickvision::System;

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
        return m_isRecevingAudio;
    }

    const CavaOptions& Cava::getOptions() const
    {
        return m_options;
    }

    void Cava::setOptions(const CavaOptions& options)
    {
        m_options = options;
        updateConfigFile();
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
        std::string oldOutput;
        while(m_process->isRunning())
        {
            if(m_process->getOutput().empty() || oldOutput == m_process->getOutput())
            {
                m_isRecevingAudio = false;
                m_outputReceived.invoke({{}});
                std::this_thread::sleep_for(std::chrono::milliseconds(5));
                continue;
            }
            m_isRecevingAudio = true;
            std::string output{ oldOutput.size() == 0 ? m_process->getOutput() : m_process->getOutput().substr(oldOutput.size()) };
            oldOutput = m_process->getOutput();
            unsigned int length{ m_options.getNumberOfBars() * 4 };
            std::vector<float> sample(m_options.getNumberOfBars() * 2);
            std::vector<char> bytes{ output.begin(), output.begin() + length };
            for(unsigned int i = 0; i < length; i += 2)
            {
                std::uint16_t* byte{ reinterpret_cast<uint16_t*>(&bytes[i]) };
                sample[i / 2] = *byte / 65535.0f;
            }
            if(m_options.getReverseBarOrder())
            {
                if(m_options.getChannels() == ChannelType::Stereo)
                {
                    int half{ static_cast<int>(sample.size() / 2) };
                    std::reverse(sample.begin(), sample.begin() + half);
                    std::reverse(sample.begin() + half, sample.end());
                }
                else
                {
                    std::reverse(sample.begin(), sample.end());
                }
            }
            m_outputReceived.invoke({ sample });
        }
        m_isRecevingAudio = false;
    }
}
