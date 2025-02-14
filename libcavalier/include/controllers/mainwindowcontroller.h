#if (defined(_WIN32) && !defined(_CRT_SECURE_NO_WARNINGS))
#define _CRT_SECURE_NO_WARNINGS
#endif

#if (defined(_WIN32) && !defined(NOMINMAX))
#define NOMINMAX
#endif

#ifndef MAINWINDOWCONTROLLER_H
#define MAINWINDOWCONTROLLER_H

#include <memory>
#include <string>
#include <vector>
#include <libnick/app/appinfo.h>
#include <libnick/app/datafilemanager.h>
#include <libnick/app/windowgeometry.h>
#include <libnick/events/event.h>
#include <libnick/notifications/notificationsenteventargs.h>
#include <libnick/notifications/shellnotificationsenteventargs.h>
#include <libnick/taskbar/taskbaritem.h>
#include <libnick/update/updater.h>
#include "controllers/preferencesviewcontroller.h"
#include "models/cava.h"
#include "models/pngimage.h"
#include "models/renderer.h"
#include "models/startupinformation.h"
#include "models/theme.h"

namespace Nickvision::Cavalier::Shared::Controllers
{
    /**
     * @brief A controller for a MainWindow.
     */
    class MainWindowController
    {
    public:
        /**
         * @brief Constructs a MainWindowController.
         * @param args A list of argument strings for the application
         */
        MainWindowController(const std::vector<std::string>& args);
        /**
         * @brief Gets the Saved event for the application's configuration.
         * @return The configuration Saved event
         */
        Nickvision::Events::Event<Nickvision::Events::EventArgs>& configurationSaved();
        /**
         * @brief Gets the event for when a notification is sent.
         * @return The notification sent event
         */
        Nickvision::Events::Event<Nickvision::Notifications::NotificationSentEventArgs>& notificationSent();
        /**
         * @brief Gets the event for when a shell notification is sent.
         * @return The shell notification sent event
         */
        Nickvision::Events::Event<Nickvision::Notifications::ShellNotificationSentEventArgs>& shellNotificationSent();
        /**
         * @brief Gets the event for when cava stops outputting data.
         * @return The cava output stopped event
         */
        Nickvision::Events::Event<Nickvision::Events::EventArgs>& cavaOutputStopped();
        /**
         * @brief Gets the event for when an image is rendered.
         * @return The image rendered event
         */
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<Models::PngImage>>& imageRendered();
        /**
         * @brief Gets the AppInfo object for the application
         * @return The current AppInfo object
         */
        const Nickvision::App::AppInfo& getAppInfo() const;
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Models::Theme getTheme();
        /**
         * @brief Gets the debugging information for the application.
         * @param extraInformation Extra, ui-specific, information to include in the debug info statement
         * @return The application's debug information
         */
        std::string getDebugInformation(const std::string& extraInformation = "") const;
        /**
         * @brief Gets whether or not the application can be shut down.
         * @return True if can shut down, else false
         */
        bool canShutdown() const;
        /**
         * @brief Gets a PreferencesViewController.
         * @return The PreferencesViewController
         */
        std::shared_ptr<PreferencesViewController> createPreferencesViewController();
        /**
         * @brief Starts the application.
         * @brief Will only have an effect the first time called.
         * @return The StartupInformation for the application
         */
#ifdef _WIN32
        const Models::StartupInformation& startup(HWND hwnd);
#elif defined(__linux__)
        const Models::StartupInformation& startup(const std::string& desktopFile);
#else     
        const Models::StartupInformation& startup();
#endif
        /**
         * @brief Shuts down the application.
         * @param geometry The window geometry to save
         */
        void shutdown(const Nickvision::App::WindowGeometry& geometry);
        /**
         * @brief Checks for an application update and sends a notification if one is available.
         */
        void checkForUpdates();
#ifdef _WIN32
        /**
         * @brief Downloads and installs the latest application update in the background.
         * @brief Will send a notification if the update fails.
         * @brief MainWindowController::checkForUpdates() must be called before this method.
         */
        void windowsUpdate();
#endif
        /**
         * @brief Sets the size of the drawing canvas.
         * @param width The new canvas width
         * @param height The new canvas height
         */
        void setCanvasSize(int width, int height);

    private:
        /**
         * @brief Handles when the configuration is saved.
         */
        void onConfigurationSaved();
        /**
         * @brief Handles when cava receives output.
         * @param args std:vector<float> sample
         */
        void onOutputReceived(const Events::ParamEventArgs<std::vector<float>>& args);
        bool m_started;
        std::vector<std::string> m_args;
        Nickvision::App::AppInfo m_appInfo;
        Nickvision::App::DataFileManager m_dataFileManager;
        std::shared_ptr<Nickvision::Update::Updater> m_updater;
        Nickvision::Taskbar::TaskbarItem m_taskbar;
        Nickvision::Events::Event<Nickvision::Notifications::NotificationSentEventArgs> m_notificationSent;
        Nickvision::Events::Event<Nickvision::Notifications::ShellNotificationSentEventArgs> m_shellNotificationSent;
        Nickvision::Events::Event<Nickvision::Events::EventArgs> m_cavaOutputStopped;
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<Models::PngImage>> m_imageRendered;
        Models::Cava m_cava;
        Models::Renderer m_renderer;
        bool m_createCanvas;
        int m_canvasWidth;
        int m_canvasHeight;
    };
}

#endif //MAINWINDOWCONTROLLER_H
