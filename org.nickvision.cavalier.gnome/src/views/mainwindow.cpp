#include "views/mainwindow.h"
#include <filesystem>
#include <format>
#include <libnick/app/appinfo.h>
#include <libnick/helpers/codehelpers.h>
#include <libnick/notifications/shellnotification.h>
#include <libnick/localization/gettext.h>
#include "helpers/dialogptr.h"
#include "helpers/gtkhelpers.h"
#include "views/preferencesdialog.h"

using namespace Nickvision::App;
using namespace Nickvision::Cavalier::GNOME::Helpers;
using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;
using namespace Nickvision::Events;
using namespace Nickvision::Helpers;
using namespace Nickvision::Notifications;
using namespace Nickvision::Update;

namespace Nickvision::Cavalier::GNOME::Views
{
    MainWindow::MainWindow(const std::shared_ptr<MainWindowController>& controller, GtkApplication* app)
        : m_controller{ controller },
        m_app{ app },
        m_builder{ "main_window" },
        m_window{ m_builder.get<AdwApplicationWindow>("root") }
    {
        //Setup Window
        gtk_application_add_window(GTK_APPLICATION(app), GTK_WINDOW(m_window));
        gtk_window_set_title(GTK_WINDOW(m_window), m_controller->getAppInfo().getShortName().c_str());
        gtk_window_set_icon_name(GTK_WINDOW(m_window), m_controller->getAppInfo().getId().c_str());
        if(m_controller->getAppInfo().getVersion().getVersionType() == VersionType::Preview)
        {
            gtk_widget_add_css_class(GTK_WIDGET(m_window), "devel");
        }
        adw_window_title_set_title(m_builder.get<AdwWindowTitle>("title"), m_controller->getAppInfo().getShortName().c_str());
        adw_status_page_set_title(m_builder.get<AdwStatusPage>("pageGreeting"), m_controller->getGreeting().c_str());
        //Register Events
        g_signal_connect(m_window, "close_request", G_CALLBACK(+[](GtkWindow*, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onCloseRequested(); }), this);
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { GtkHelpers::dispatchToMainThread([this, args]() { onNotificationSent(args); }); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { onShellNotificationSent(args); };
        m_controller->folderChanged() += [&](const EventArgs& args) { onFolderChanged(args); };
        //Drop Target
        GtkDropTarget* dropTarget{ gtk_drop_target_new(G_TYPE_FILE, GDK_ACTION_COPY) };
        g_signal_connect(dropTarget, "drop", G_CALLBACK(+[](GtkDropTarget*, const GValue* value, double, double, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onDrop(value); }), this);
        gtk_widget_add_controller(GTK_WIDGET(m_window), GTK_EVENT_CONTROLLER(dropTarget));
        //Quit Action
        GSimpleAction* actQuit{ g_simple_action_new("quit", nullptr) };
        g_signal_connect(actQuit, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->quit(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actQuit));
        GtkHelpers::setAccelForAction(m_app, "win.quit", "<Ctrl>Q");
        //Open Folder Action
        GSimpleAction* actOpenFolder{ g_simple_action_new("openFolder", nullptr) };
        g_signal_connect(actOpenFolder, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->openFolder(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actOpenFolder));
        GtkHelpers::setAccelForAction(m_app, "win.openFolder", "<Ctrl>O");
        //Close Folder Action
        GSimpleAction* actCloseFolder{ g_simple_action_new("closeFolder", nullptr) };
        g_signal_connect(actCloseFolder, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->closeFolder(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actCloseFolder));
        GtkHelpers::setAccelForAction(m_app, "win.closeFolder", "<Ctrl>W");
        //Preferences Action
        GSimpleAction* actPreferences{ g_simple_action_new("preferences", nullptr) };
        g_signal_connect(actPreferences, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->preferences(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actPreferences));
        GtkHelpers::setAccelForAction(m_app, "win.preferences", "<Ctrl>comma");
        //Keyboard Shortcuts Action
        GSimpleAction* actKeyboardShortcuts{ g_simple_action_new("keyboardShortcuts", nullptr) };
        g_signal_connect(actKeyboardShortcuts, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->keyboardShortcuts(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actKeyboardShortcuts));
        GtkHelpers::setAccelForAction(m_app, "win.keyboardShortcuts", "<Ctrl>question");
        //About Action
        GSimpleAction* actAbout{ g_simple_action_new("about", nullptr) };
        g_signal_connect(actAbout, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->about(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actAbout));
        GtkHelpers::setAccelForAction(m_app, "win.about", "F1");
    }

    MainWindow::~MainWindow()
    {
        gtk_window_destroy(GTK_WINDOW(m_window));
    }

    void MainWindow::show()
    {
        gtk_window_present(GTK_WINDOW(m_window));
#ifdef __linux__
        const StartupInformation& info{ m_controller->startup(m_controller->getAppInfo().getId() + ".desktop") };
#else
        const StartupInformation& info{ m_controller->startup() };
#endif
        gtk_window_set_default_size(GTK_WINDOW(m_window), static_cast<int>(info.getWindowGeometry().getWidth()), static_cast<int>(info.getWindowGeometry().getHeight()));
        if(info.getWindowGeometry().isMaximized())
        {
            gtk_window_maximize(GTK_WINDOW(m_window));
        }
    }

    bool MainWindow::onCloseRequested()
    {
        if(!m_controller->canShutdown())
        {
            return true;
        }
        int width;
        int height;
        gtk_window_get_default_size(GTK_WINDOW(m_window), &width, &height);
        m_controller->shutdown({ width, height, static_cast<bool>(gtk_window_is_maximized(GTK_WINDOW(m_window))) });
        return false;
    }

    bool MainWindow::onDrop(const GValue* value)
    {
        if(G_VALUE_HOLDS(value, G_TYPE_FILE))
        {
            m_controller->openFolder(g_file_get_path(G_FILE(g_value_get_object(value))));
            return true;
        }
        return false;
    }

    void MainWindow::onNotificationSent(const NotificationSentEventArgs& args)
    {
        AdwToast* toast{ adw_toast_new(args.getMessage().c_str()) };
        if(args.getAction() == "close")
        {
            adw_toast_set_button_label(toast, _("Close"));
            g_signal_connect(toast, "button-clicked", G_CALLBACK(+[](AdwToast*, gpointer data){ reinterpret_cast<MainWindow*>(data)->closeFolder(); }), this);
        }
        adw_toast_overlay_add_toast(m_builder.get<AdwToastOverlay>("toastOverlay"), toast);
    }

    void MainWindow::onShellNotificationSent(const ShellNotificationSentEventArgs& args)
    {
        m_controller->log(Logging::LogLevel::Info, "ShellNotification sent. (" + args.getMessage() + ")");
#ifdef __linux__
        ShellNotification::send(args, m_controller->getAppInfo().getId(), _("Open"));
#else
        ShellNotification::send(args);
#endif
    }

    void MainWindow::onFolderChanged(const EventArgs& args)
    {
        adw_window_title_set_subtitle(m_builder.get<AdwWindowTitle>("title"), m_controller->isFolderOpened() ? m_controller->getFolderPath().c_str() : "");
        gtk_widget_set_visible(m_builder.get<GtkWidget>("btnOpenFolder"), m_controller->isFolderOpened());
        gtk_widget_set_visible(m_builder.get<GtkWidget>("btnCloseFolder"), m_controller->isFolderOpened());
        adw_view_stack_set_visible_child_name(m_builder.get<AdwViewStack>("viewStack"), m_controller->isFolderOpened() ? "Folder" : "NoFolder");
        adw_status_page_set_description(m_builder.get<AdwStatusPage>("pageFiles"), std::vformat(_n("There is {} file in the folder.", "There are {} files in the folder.", m_controller->getFiles().size()), std::make_format_args(CodeHelpers::unmove(m_controller->getFiles().size()))).c_str());
    }

    void MainWindow::quit()
    {
        if(!onCloseRequested())
        {
            g_application_quit(G_APPLICATION(m_app));
        }
    }

    void MainWindow::openFolder()
    {
        GtkFileDialog* folderDialog{ gtk_file_dialog_new() };
        gtk_file_dialog_set_title(folderDialog, _("Open Folder"));
        gtk_file_dialog_select_folder(folderDialog, GTK_WINDOW(m_window), nullptr, GAsyncReadyCallback(+[](GObject* self, GAsyncResult* res, gpointer data)
        {
            GFile* folder{ gtk_file_dialog_select_folder_finish(GTK_FILE_DIALOG(self), res, nullptr) };
            if(folder)
            {
                reinterpret_cast<MainWindow*>(data)->m_controller->openFolder(g_file_get_path(folder));
            }
        }), this);
    }

    void MainWindow::closeFolder()
    {
        m_controller->closeFolder();
    }

    void MainWindow::preferences()
    {
        DialogPtr<PreferencesDialog> dialog{ m_controller->createPreferencesViewController(), GTK_WINDOW(m_window) };
        dialog->present();
    }

    void MainWindow::keyboardShortcuts()
    {
        Builder builderHelp{ "shortcuts_dialog" };
        GtkShortcutsWindow* shortcuts{ builderHelp.get<GtkShortcutsWindow>("root") };
        gtk_window_set_transient_for(GTK_WINDOW(shortcuts), GTK_WINDOW(m_window));
        gtk_window_set_icon_name(GTK_WINDOW(shortcuts), m_controller->getAppInfo().getId().c_str());
        gtk_window_present(GTK_WINDOW(shortcuts));
    }

    void MainWindow::about()
    {
        std::string extraDebug;
        extraDebug += "GTK " + std::to_string(gtk_get_major_version()) + "." + std::to_string(gtk_get_minor_version()) + "." + std::to_string(gtk_get_micro_version()) + "\n";
        extraDebug += "libadwaita " + std::to_string(adw_get_major_version()) + "." + std::to_string(adw_get_minor_version()) + "." + std::to_string(adw_get_micro_version()) + "\n";
        AdwAboutDialog* dialog{ ADW_ABOUT_DIALOG(adw_about_dialog_new()) };
        adw_about_dialog_set_application_name(dialog, m_controller->getAppInfo().getShortName().c_str());
        adw_about_dialog_set_application_icon(dialog, std::string(m_controller->getAppInfo().getId() + (m_controller->getAppInfo().getVersion().getVersionType() == VersionType::Preview ? "-devel" : "")).c_str());
        adw_about_dialog_set_developer_name(dialog, "Nickvision");
        adw_about_dialog_set_version(dialog, m_controller->getAppInfo().getVersion().str().c_str());
        adw_about_dialog_set_release_notes(dialog, m_controller->getAppInfo().getHtmlChangelog().c_str());
        adw_about_dialog_set_debug_info(dialog, m_controller->getDebugInformation(extraDebug).c_str());
        adw_about_dialog_set_comments(dialog, m_controller->getAppInfo().getDescription().c_str());
        adw_about_dialog_set_license_type(dialog, GTK_LICENSE_GPL_3_0);
        adw_about_dialog_set_copyright(dialog, "© Nickvision 2021-2025");
        adw_about_dialog_set_website(dialog, "https://nickvision.org/");
        adw_about_dialog_set_issue_url(dialog, m_controller->getAppInfo().getIssueTracker().c_str());
        adw_about_dialog_set_support_url(dialog, m_controller->getAppInfo().getSupportUrl().c_str());
        adw_about_dialog_add_link(dialog, _("GitHub Repo"), m_controller->getAppInfo().getSourceRepo().c_str());
        for(const std::pair<std::string, std::string>& pair : m_controller->getAppInfo().getExtraLinks())
        {
            adw_about_dialog_add_link(dialog, pair.first.c_str(), pair.second.c_str());
        }
        std::vector<const char*> urls;
        std::vector<std::string> developers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDevelopers()) };
        for(const std::string& developer : developers)
        {
            urls.push_back(developer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_developers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> designers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDesigners()) };
        for(const std::string& designer : designers)
        {
            urls.push_back(designer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_designers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> artists{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getArtists()) };
        for(const std::string& artist : artists)
        {
            urls.push_back(artist.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_artists(dialog, &urls[0]);
        adw_about_dialog_set_translator_credits(dialog, m_controller->getAppInfo().getTranslatorCredits().c_str());
        adw_dialog_present(ADW_DIALOG(dialog), GTK_WIDGET(m_window));
    }
}
