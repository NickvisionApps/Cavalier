#include "controllers/mainwindowcontroller.h"
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
        m_cava{ m_dataFileManager.get<Configuration>("config").getCavaOptions(), m_appInfo.getName() }
    {
        m_appInfo.setVersion({ "2025.2.0-next" });
        m_appInfo.setShortName(_("Cavalier"));
        m_appInfo.setDescription(_("Visualize audio with CAVA"));
        m_appInfo.setChangelog("- Rewrote Cavalier in C++ for faster and smoother performance\n- Added a Qt version of Cavalier which also runs on Windows\n- Added the ability to specify the `CAVALIER_INPUT_SOURCE` environment variable to change Cavalier's audio source\n- Increased the maximuim number of bars to 200\n- Updated dependencies");
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
        m_dataFileManager.get<Configuration>("config").saved() +=  [this](const EventArgs&){ onConfigurationSaved(); };
        m_cava.outputReceived() += [this](const ParamEventArgs<std::vector<float>>& args){ onOutputReceived(args); };
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

    Event<EventArgs>& MainWindowController::cavaOutputStopped()
    {
        return m_cavaOutputStopped;
    }

    Event<ParamEventArgs<PngImage>>& MainWindowController::imageRendered()
    {
        return m_imageRendered;
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
        m_taskbar.connect(hwnd);
        if (m_dataFileManager.get<Configuration>("config").getAutomaticallyCheckForUpdates())
        {
            checkForUpdates();
        }
#elif defined(__linux__)
        m_taskbar.connect(desktopFile);
#endif
        m_cava.start();
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
        std::thread worker{ [this]()
        {
            Version latest{ m_updater->fetchCurrentVersion(VersionType::Stable) };
            if(!latest.empty())
            {
                if(latest > m_appInfo.getVersion())
                {
                    m_notificationSent.invoke({ _("New update available"), NotificationSeverity::Success, "update" });
                }
            }
        } };
        worker.detach();
    }

#ifdef _WIN32
    void MainWindowController::windowsUpdate()
    {
        if(!m_updater)
        {
            return;
        }
        m_notificationSent.invoke({ _("The update is downloading in the background and will start once it finishes"), NotificationSeverity::Informational });
        std::thread worker{ [this]()
        {
            if(!m_updater->windowsUpdate(VersionType::Stable))
            {
                m_notificationSent.invoke({ _("Unable to download and install update"), NotificationSeverity::Error });
            }
        } };
        worker.detach();
    }
#endif

    void MainWindowController::setCanvas(const std::optional<Canvas>& canvas)
    {
        m_renderer.setCanvas(canvas);
    }

    void MainWindowController::onConfigurationSaved()
    {
        Configuration& config{ m_dataFileManager.get<Configuration>("config") };
        m_cava.setOptions(config.getCavaOptions());
        m_renderer.setDrawingArea(config.getDrawingArea());
        m_renderer.setColorProfile(config.getColorProfiles()[config.getActiveColorProfileIndex()]);
        if(config.getActiveBackgroundImageIndex() != -1)
        {
            m_renderer.setBackgroundImage(config.getBackgroundImages()[config.getActiveBackgroundImageIndex()]);
        }
        else
        {
            m_renderer.setBackgroundImage(std::nullopt);
        }
    }

    void MainWindowController::onOutputReceived(const ParamEventArgs<std::vector<float>>& args)
    {
        if(args.getParam().empty())
        {
            m_cavaOutputStopped.invoke({});
        }
        else
        {
            std::optional<PngImage> image{ m_renderer.draw(args.getParam()) };
            if(image)
            {
                m_imageRendered.invoke({ *image });
            }
        }
    }
}
