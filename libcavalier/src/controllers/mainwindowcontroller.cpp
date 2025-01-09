#include "controllers/mainwindowcontroller.h"
#include <ctime>
#include <format>
#include <sstream>
#include <thread>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/helpers/codehelpers.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include <libnick/system/environment.h>
#include "models/configuration.h"
#ifdef _WIN32
#include <windows.h>
#endif

using namespace Nickvision::App;
using namespace Nickvision::Cavalier::Shared::Models;
using namespace Nickvision::Events;
using namespace Nickvision::Filesystem;
using namespace Nickvision::Helpers;
using namespace Nickvision::Notifications;
using namespace Nickvision::System;
using namespace Nickvision::Update;

namespace Nickvision::Cavalier::Shared::Controllers
{
    MainWindowController::MainWindowController(const std::vector<std::string>& args)
        : m_started{ false },
        m_args{ args },
        m_appInfo{ "org.nickvision.cavalier", "Nickvision Cavalier", "Cavalier" },
        m_dataFileManager{ m_appInfo.getName() },
        m_logger{ UserDirectories::get(ApplicationUserDirectory::LocalData, m_appInfo.getName()) / "log.txt", Logging::LogLevel::Info, false }
    {
        m_appInfo.setVersion({ "2025.1.0-next" });
        m_appInfo.setShortName(_("Cavalier"));
        m_appInfo.setDescription(_("Visualize audio with CAVA"));
        m_appInfo.setChangelog("- Initial Release");
        m_appInfo.setSourceRepo("https://github.com/NickvisionApps/Cavalier");
        m_appInfo.setIssueTracker("https://github.com/NickvisionApps/Cavalier/issues/new");
        m_appInfo.setSupportUrl("https://github.com/NickvisionApps/Cavalier/discussions");
        m_appInfo.getExtraLinks()[_("Matrix Chat")] = "https://matrix.to/#/#nickvision:matrix.org";
        m_appInfo.getDevelopers()["Nicholas Logozzo"] = "https://github.com/nlogozzo";
        m_appInfo.getDevelopers()[_("Contributors on GitHub")] = "https://github.com/NickvisionApps/Cavalier/graphs/contributors";
        m_appInfo.getDesigners()["Nicholas Logozzo"] = "https://github.com/nlogozzo";
        m_appInfo.getDesigners()[_("Fyodor Sobolev")] = "https://github.com/fsobolev";
        m_appInfo.getDesigners()["DaPigGuy"] = "https://github.com/DaPigGuy";
        m_appInfo.getArtists()[_("David Lapshin")] = "https://github.com/daudix";
        m_appInfo.setTranslatorCredits(_("translator-credits"));
        Localization::Gettext::init(m_appInfo.getEnglishShortName());
#ifdef _WIN32
        m_updater = std::make_shared<Updater>(m_appInfo.getSourceRepo());
#endif
        m_dataFileManager.get<Configuration>("config").saved() += [this](const EventArgs&)
        {
            m_logger.log(Logging::LogLevel::Info, "Configuration saved.");
        };
    }

    Event<EventArgs>& MainWindowController::configurationSaved()
    {
        return m_dataFileManager.get<Configuration>("config").saved();
    }

    Event<NotificationSentEventArgs>& MainWindowController::notificationSent()
    {
        return m_notificationSent;
    }

    Event<ShellNotificationSentEventArgs>& MainWindowController::shellNotificationSent()
    {
        return m_shellNotificationSent;
    }

    const AppInfo& MainWindowController::getAppInfo() const
    {
        return m_appInfo;
    }

    Theme MainWindowController::getTheme()
    {
        return m_dataFileManager.get<Configuration>("config").getTheme();
    }

    std::string MainWindowController::getDebugInformation(const std::string& extraInformation) const
    {
        std::stringstream builder;
        //Extra
        if(!extraInformation.empty())
        {
            builder << extraInformation << std::endl;
#ifdef __linux__
            builder << Environment::exec("locale");
#endif
        }
        return Environment::getDebugInformation(m_appInfo, builder.str());
    }

    bool MainWindowController::canShutdown() const
    {
        return true;
    }

    std::shared_ptr<PreferencesViewController> MainWindowController::createPreferencesViewController()
    {
        return std::make_shared<PreferencesViewController>(m_dataFileManager.get<Configuration>("config"));
    }

#ifdef _WIN32
    const StartupInformation& MainWindowController::startup(HWND hwnd)
#elif defined(__linux__)
    const StartupInformation& MainWindowController::startup(const std::string& desktopFile)
#else
    const StartupInformation& MainWindowController::startup()
#endif
    {
        static StartupInformation info;
        if (m_started)
        {
            return info;
        }
        //Load configuration
        info.setWindowGeometry(m_dataFileManager.get<Configuration>("config").getWindowGeometry());
        //Load taskbar item
#ifdef _WIN32
        if(m_taskbar.connect(hwnd))
        {
            m_logger.log(Logging::LogLevel::Info, "Connected to Windows taskbar.");
        }
        else
        {
            m_logger.log(Logging::LogLevel::Error, "Unable to connect to Windows taskbar.");
        }
        if (m_dataFileManager.get<Configuration>("config").getAutomaticallyCheckForUpdates())
        {
            checkForUpdates();
        }
#elif defined(__linux__)
        if(m_taskbar.connect(desktopFile))
        {
            m_logger.log(Logging::LogLevel::Info, "Connected to Linux taskbar.");
        }
        else
        {
            m_logger.log(Logging::LogLevel::Error, "Unable to connect to Linux taskbar.");
        }
#endif
        m_started = true;
        return info;
    }

    void MainWindowController::shutdown(const WindowGeometry& geometry)
    {
        Configuration& config{ m_dataFileManager.get<Configuration>("config") };
        config.setWindowGeometry(geometry);
        config.save();
    }

    void MainWindowController::checkForUpdates()
    {
        if(!m_updater)
        {
            return;
        }
        m_logger.log(Logging::LogLevel::Info, "Checking for updates...");
        std::thread worker{ [this]()
        {
            Version latest{ m_updater->fetchCurrentVersion(VersionType::Stable) };
            if (!latest.empty())
            {
                if (latest > m_appInfo.getVersion())
                {
                    m_logger.log(Logging::LogLevel::Info, "Update found: " + latest.str());
                    m_notificationSent.invoke({ _("New update available"), NotificationSeverity::Success, "update" });
                }
                else
                {
                    m_logger.log(Logging::LogLevel::Info, "No updates found.");
                }
            }
            else
            {
                m_logger.log(Logging::LogLevel::Warning, "Unable to fetch latest app version.");
            }
        } };
        worker.detach();
    }

#ifdef _WIN32
    void MainWindowController::windowsUpdate()
    {
        if(m_updater)
        {
            return;
        }
        m_logger.log(Logging::LogLevel::Info, "Fetching Windows app update...");
        std::thread worker{ [this]()
        {
            if (m_updater->windowsUpdate(VersionType::Stable))
            {
                m_logger.log(Logging::LogLevel::Info, "Windows app update started.");
            }
            else
            {
                m_logger.log(Logging::LogLevel::Error, "Unable to fetch Windows app update.");
                m_notificationSent.invoke({ _("Unable to download and install update"), NotificationSeverity::Error });
            }
        } };
        worker.detach();
    }
#endif

    void MainWindowController::log(Logging::LogLevel level, const std::string& message, const std::source_location& source)
    {
        m_logger.log(level, message, source);
    }

    std::string MainWindowController::getGreeting() const
    {
        std::time_t now{ std::time(nullptr) };
        std::tm* cal{ std::localtime(&now) };
        if (cal->tm_hour >= 0 && cal->tm_hour < 6)
        {
            return _p("Night", "Good Morning!");
        }
        else if (cal->tm_hour < 12)
        {
            return _p("Morning", "Good Morning!");
        }
        else if (cal->tm_hour < 18)
        {
            return _("Good Afternoon!");
        }
        else if (cal->tm_hour < 24)
        {
            return _("Good Evening!");
        }
        return _("Good Day!");
    }

    const std::filesystem::path& MainWindowController::getFolderPath() const
    {
        return m_folderPath;
    }

    const std::vector<std::filesystem::path>& MainWindowController::getFiles() const
    {
        return m_files;
    }

    bool MainWindowController::isFolderOpened() const
    {
        return std::filesystem::exists(m_folderPath) && std::filesystem::is_directory(m_folderPath);
    }

    Event<EventArgs>& MainWindowController::folderChanged()
    {
        return m_folderChanged;
    }

    bool MainWindowController::openFolder(const std::filesystem::path& path)
    {
        if (std::filesystem::exists(path) && std::filesystem::is_directory(path))
        {
            m_folderPath = path;
            loadFiles();
            m_notificationSent.invoke({ std::vformat(_("Folder Opened: {}"), std::make_format_args(CodeHelpers::unmove(m_folderPath.string()))), NotificationSeverity::Success, "close" });
            m_folderChanged.invoke({});
            m_taskbar.setCount(static_cast<long>(m_files.size()));
            m_taskbar.setCountVisible(true);
            m_logger.log(Logging::LogLevel::Info, "Folder opened. (" + m_folderPath.string() + ")");
            return true;
        }
        return false;
    }

    void MainWindowController::closeFolder()
    {
        m_logger.log(Logging::LogLevel::Info, "Folder closed. (" + m_folderPath.string() + ")");
        m_folderPath = std::filesystem::path();
        m_files.clear();
        m_notificationSent.invoke({ _("Folder closed"), NotificationSeverity::Warning });
        m_folderChanged.invoke({});
        m_taskbar.setCountVisible(false);
    }

    void MainWindowController::loadFiles()
    {
        m_files.clear();
        if (std::filesystem::exists(m_folderPath))
        {
            for (const std::filesystem::directory_entry& e : std::filesystem::directory_iterator(m_folderPath))
            {
                if (e.is_regular_file())
                {
                    m_files.push_back(e.path());
                }
            }
        }
        m_logger.log(Logging::LogLevel::Info, "Loaded " + std::to_string(m_files.size()) + " file(s). (" + m_folderPath.string() + ")");
    }
}
