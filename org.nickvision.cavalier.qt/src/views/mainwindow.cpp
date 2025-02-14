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
#include "views/settingsdialog.h"

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
    enum MainWindowPages
    {
        Home = 0,
        Render = 1
    };

    MainWindow::MainWindow(const std::shared_ptr<MainWindowController>& controller, QWidget* parent) 
        : QMainWindow{ parent },
        m_ui{ new Ui::MainWindow() },
        m_infoBar{ new InfoBar(this) },
        m_controller{ controller }
    {
        m_ui->setupUi(this);
        setWindowTitle(m_controller->getAppInfo().getVersion().getVersionType() == VersionType::Stable ? _("Cavalier") : _("Cavalier (Preview)"));
        addDockWidget(::Qt::BottomDockWidgetArea, m_infoBar);
        //MenuBar
        m_ui->menuFile->setTitle(_("File"));
        m_ui->actionExit->setText(_("Exit"));
        m_ui->menuEdit->setTitle(_("Edit"));
        m_ui->actionSettings->setText(_("Settings"));
        m_ui->menuHelp->setTitle(_("Help"));
        m_ui->actionCheckForUpdates->setText(_("Check for Updates"));
        m_ui->actionGitHubRepo->setText(_("GitHub Repo"));
        m_ui->actionReportABug->setText(_("Report a Bug"));
        m_ui->actionDiscussions->setText(_("Discussions"));
        m_ui->actionAbout->setText(_("About Cavalier"));
        //Home Page
        m_ui->lblHomeGreeting->setText(_("Visualize Your Audio"));
        m_ui->lblHomeDescription->setText(_("Play some music or watch a video and see your sound come to life"));
        //Signals
        connect(m_ui->actionExit, &QAction::triggered, this, &MainWindow::close);
        connect(m_ui->actionSettings, &QAction::triggered, this, &MainWindow::settings);
        connect(m_ui->actionCheckForUpdates, &QAction::triggered, this, &MainWindow::checkForUpdates);
        connect(m_ui->actionGitHubRepo, &QAction::triggered, this, &MainWindow::gitHubRepo);
        connect(m_ui->actionReportABug, &QAction::triggered, this, &MainWindow::reportABug);
        connect(m_ui->actionDiscussions, &QAction::triggered, this, &MainWindow::discussions);
        connect(m_ui->actionAbout, &QAction::triggered, this, &MainWindow::about);
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { QtHelpers::dispatchToMainThread([this, args]() { onNotificationSent(args); }); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { onShellNotificationSent(args); };
        m_controller->cavaOutputStopped() += [&](const EventArgs&) { QtHelpers::dispatchToMainThread([this]() { onCavaOutputStopped(); }); };
        m_controller->imageRendered() += [&](const ParamEventArgs<PngImage>& args) { QtHelpers::dispatchToMainThread([this, args]() { onImageRendered(args); }); };
    }

    MainWindow::~MainWindow()
    {
        delete m_infoBar;
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
        setGeometry(QWidget::geometry().x(), QWidget::geometry().y(), info.getWindowGeometry().getWidth(), info.getWindowGeometry().getHeight());
        if(info.getWindowGeometry().isMaximized())
        {
            showMaximized();
        }
        m_ui->viewStack->setCurrentIndex(MainWindowPages::Home);
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

    void MainWindow::resizeEvent(QResizeEvent* event)
    {
        QMainWindow::resizeEvent(event);
        m_controller->setCanvasSize(m_ui->lblImage->width(), m_ui->lblImage->height());
    }

    void MainWindow::settings()
    {
        SettingsDialog dialog{ m_controller->createPreferencesViewController(), this };
        dialog.exec();
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
        QString actionText;
        std::function<void()> actionCallback;
#ifdef _WIN32
        if(args.getAction() == "update")
        {
            actionText = _("Update");
            actionCallback = [this]() { windowsUpdate(); };
        }
#endif
        m_infoBar->show(args, actionText, actionCallback);
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

    void MainWindow::onCavaOutputStopped()
    {
        m_ui->lblImage->clear();
        m_ui->viewStack->setCurrentIndex(MainWindowPages::Home);
    }

    void MainWindow::onImageRendered(const ParamEventArgs<PngImage>& args)
    {
        m_ui->viewStack->setCurrentIndex(MainWindowPages::Render);
        QByteArray bytes{ reinterpret_cast<const char*>(&args.getParam().getBytes()[0]), static_cast<qsizetype>(args.getParam().getBytes().size()) };
        QPixmap pixmap{ args.getParam().getWidth(), args.getParam().getHeight() };
        pixmap.loadFromData(bytes, "PNG");
        m_ui->lblImage->setPixmap(pixmap);
    }
}
