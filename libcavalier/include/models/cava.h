#ifndef CAVA_H
#define CAVA_H

#include <filesystem>
#include <mutex>
#include <string>
#include <thread>
#include <vector>
#include <libnick/events/parameventargs.h>
#include <libnick/system/process.h>
#include "cavaoptions.h"

namespace Nickvision::Cavalier::Shared::Models
{
    /**
     * @brief A model of a cava process.
     */
    class Cava
    {
    public:
        /**
         * @brief Constructs a Cava.
         * @param options The options to use for cava
         * @param appName The name of the application
         */
        Cava(const CavaOptions& options, const std::string& appName);
        /**
         * @brief Destructs a Cava.
         */
        ~Cava();
        /**
         * @brief Gets the event for when output is received from cava.
         * @return The output received event
         */
        Events::Event<Events::ParamEventArgs<std::vector<float>>>& outputReceived();
        /**
         * @brief Whether or not the cava process is receving audio data.
         * @return True if receving audio data
         * @return False is not receving audio data
         */
        bool isRecevingAudio() const;
        /**
         * @brief Gets the options to use for cava.
         * @return The cava options
         */
        const CavaOptions& getOptions() const;
        /**
         * @brief Sets the options to use for cava.
         * @brief This method will restart the cava process if it is running.
         * @param options The new cava options
         */
        void setOptions(const CavaOptions& options);
        /**
         * @brief Starts the cava process.
         * @brief If the cava process has been already started, it will be restarted (loading the configuration file again).
         */
        void start();

    private:
        /**
         * @brief Writes the cava options to the config file on disk.
         */
        void updateConfigFile();
        /**
         * @brief Watches for output from the cava process.
         */
        void watch();
        mutable std::mutex m_mutex;
        CavaOptions m_options;
        bool m_isRecevingAudio;
        std::filesystem::path m_configPath;
        std::shared_ptr<System::Process> m_process;
        std::thread m_watcher;
        Events::Event<Events::ParamEventArgs<std::vector<float>>> m_outputReceived;
    };
}

#endif //CAVA_H
