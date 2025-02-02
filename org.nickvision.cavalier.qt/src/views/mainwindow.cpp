#include "views/mainwindow.h"
#include "ui_mainwindow.h"
#include <QDesktopServices>
#include <QFileDialog>
#include <QMessageBox>
#include <QMimeData>
#include <QPushButton>
#include <libnick/helpers/codehelpers.h>
#include <libnick/localization/gettext.h>
#include <libnick/notifications/shellnotification.h>
#include "controls/aboutdialog.h"
#include "helpers/qthelpers.h"
#include "views/settingspage.h"

using namespace Nickvision::App;
using namespace Nickvision::Cavalier::Qt::Controls;
using namespace Nickvision::Cavalier::Qt::Helpers;
using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;
using namespace Nickvision::Events;
using namespace Nickvision::Helpers;
using namespace Nickvision::Notifications;
using namespace Nickvision::Update;

namespace Nickvision::Cavalier::Qt::Views
{
    enum Page
    {
        Home = 0,
        Settings
    };

    MainWindow::MainWindow(const std::shared_ptr<MainWindowController>& controller, QWidget* parent) 
        : QMainWindow{ parent },
        m_ui{ new Ui::MainWindow() },
        m_navigationBar{ new NavigationBar(this) },
        m_controller{ controller }
    {
        m_ui->setupUi(this);
        m_ui->mainLayout->insertLayout(0, m_navigationBar);
        setWindowTitle(m_controller->getAppInfo().getVersion().getVersionType() == VersionType::Stable ? _("Cavalier") : _("Cavalier (Preview)"));
        //Navigation Bar
        QMenu* helpMenu{ new QMenu(this) };
        helpMenu->addAction(_("Check for Updates"), this, &MainWindow::checkForUpdates);
        helpMenu->addSeparator();
        helpMenu->addAction(_("GitHub Repo"), this, &MainWindow::gitHubRepo);
        helpMenu->addAction(_("Report a Bug"), this, &MainWindow::reportABug);
        helpMenu->addAction(_("Discussions"), this, &MainWindow::discussions);
        helpMenu->addSeparator();
        helpMenu->addAction(_("About"), this, &MainWindow::about);
        m_navigationBar->addTopItem("home", _("Home"), QIcon::fromTheme(QIcon::ThemeIcon::GoHome));
        m_navigationBar->addBottomItem("help", _("Help"), QIcon::fromTheme(QIcon::ThemeIcon::HelpAbout), helpMenu);
#ifdef _WIN32
        m_navigationBar->addBottomItem("settings", _("Settings"), QIcon::fromTheme("document-properties"));
#else
        m_navigationBar->addBottomItem("settings", _("Settings"), QIcon::fromTheme(QIcon::ThemeIcon::DocumentProperties));
#endif
        //Home Page
        m_ui->lblHomeGreeting->setText(_("Visualize Your Audio"));
        m_ui->lblHomeDescription->setText(_("Play some music or watch a video and see your sound come to life"));
        //Signals
        connect(m_navigationBar, &NavigationBar::itemSelected, this, &MainWindow::onNavigationItemSelected);
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { QtHelpers::dispatchToMainThread([this, args]() { onNotificationSent(args); }); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { onShellNotificationSent(args); };
    }

    MainWindow::~MainWindow()
    {
        delete m_ui;
    }

    void MainWindow::show()
    {
        QMainWindow::show();
#ifdef _WIN32
        const StartupInformation& info{ m_controller->startup(reinterpret_cast<HWND>(winId())) };
#elif defined(__linux__)
        const StartupInformation& info{ m_controller->startup(m_controller->getAppInfo().getId() + ".desktop") };
#else
        const StartupInformation& info{ m_controller->startup() };
#endif
        if(info.getWindowGeometry().isMaximized())
        {
            showMaximized();
        }
        else
        {
            setGeometry(QWidget::geometry().x(), QWidget::geometry().y(), info.getWindowGeometry().getWidth(), info.getWindowGeometry().getHeight());
        }
        m_navigationBar->selectItem("home");
    }

    void MainWindow::closeEvent(QCloseEvent* event)
    {
        if(!m_controller->canShutdown())
        {
            return event->ignore();
        }
        m_controller->shutdown({ geometry().width(), geometry().height(), isMaximized() });
        event->accept();
    }

    void MainWindow::onNavigationItemSelected(const QString& id)
    {
        //Save and ensure new SettingsPage
        if(m_ui->viewStack->widget(Page::Settings))
        {
            SettingsPage* oldSettings{ qobject_cast<SettingsPage*>(m_ui->viewStack->widget(Page::Settings)) };
            oldSettings->close();
            m_ui->viewStack->removeWidget(oldSettings);
            delete oldSettings;
        }
        m_ui->viewStack->insertWidget(Page::Settings, new SettingsPage(m_controller->createPreferencesViewController(), this));
        //Navigate to new page
        if(id == "home")
        {
            m_ui->viewStack->setCurrentIndex(Page::Home);
        }
        else if(id == "settings")
        {
            m_ui->viewStack->setCurrentIndex(Page::Settings);
        }
    }

    void MainWindow::checkForUpdates()
    {
        m_controller->checkForUpdates();
    }

#ifdef _WIN32
    void MainWindow::windowsUpdate()
    {
        m_controller->windowsUpdate();
    }
#endif

    void MainWindow::gitHubRepo()
    {
        QDesktopServices::openUrl(QString::fromStdString(m_controller->getAppInfo().getSourceRepo()));
    }

    void MainWindow::reportABug()
    {
        QDesktopServices::openUrl(QString::fromStdString(m_controller->getAppInfo().getIssueTracker()));
    }

    void MainWindow::discussions()
    {
        QDesktopServices::openUrl(QString::fromStdString(m_controller->getAppInfo().getSupportUrl()));
    }

    void MainWindow::about()
    {
        std::string extraDebug;
        extraDebug += "Qt " + std::string(qVersion()) + "\n";
        AboutDialog dialog{ m_controller->getAppInfo(), m_controller->getDebugInformation(extraDebug), this };
        dialog.exec();
    }

    void MainWindow::onNotificationSent(const NotificationSentEventArgs& args)
    {
        QMessageBox::Icon icon{ QMessageBox::Icon::NoIcon };
        switch(args.getSeverity())
        {
        case NotificationSeverity::Informational:
        case NotificationSeverity::Success:
            icon = QMessageBox::Icon::Information;
            break;
        case NotificationSeverity::Warning:
            icon = QMessageBox::Icon::Warning;
            break;
        case NotificationSeverity::Error:
            icon = QMessageBox::Icon::Critical;
            break;
        }
        QMessageBox msgBox{ icon, QString::fromStdString(m_controller->getAppInfo().getShortName()), QString::fromStdString(args.getMessage()), QMessageBox::StandardButton::Ok, this };
#ifdef _WIN32
        if(args.getAction() == "update")
        {
            QPushButton* updateButton{ msgBox.addButton(_("Update"), QMessageBox::ButtonRole::ActionRole) };
            connect(updateButton, &QPushButton::clicked, this, &MainWindow::windowsUpdate);
        }
#endif
        msgBox.exec();
    }

    void MainWindow::onShellNotificationSent(const ShellNotificationSentEventArgs& args)
    {
#ifdef _WIN32
        ShellNotification::send(args, reinterpret_cast<HWND>(winId()));
#elif defined(__linux__)
        ShellNotification::send(args, m_controller->getAppInfo().getId(), _("Open"));
#else
        ShellNotification::send(args);
#endif
    }
}
