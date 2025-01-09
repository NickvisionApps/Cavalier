#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <memory>
#include <QCloseEvent>
#include <QDragEnterEvent>
#include <QDropEvent>
#include <QMainWindow>
#include "controllers/mainwindowcontroller.h"
#include "controls/navigationbar.h"

namespace Ui { class MainWindow; }

namespace Nickvision::Cavalier::Qt::Views
{
    /**
     * @brief The main window for the application.
     */
    class MainWindow : public QMainWindow
    {
    Q_OBJECT

    public:
        /**
         * @brief Constructs a MainWindow.
         * @param controller The MainWindowController
         * @param parent The parent widget
         */
        MainWindow(const std::shared_ptr<Shared::Controllers::MainWindowController>& controller, QWidget* parent = nullptr);
        /**
         * @brief Destructs a MainWindow.
         */
        ~MainWindow();
        /**
         * @brief Shows the MainWindow.
         */
        void show();

    protected:
        /**
         * @brief Handles when the window is closed.
         * @param event QCloseEvent
         */
        void closeEvent(QCloseEvent* event) override;
        /**
         * @brief Handles when a file is dragged into the window.
         * @param event QDragEnterEvent
         */
        void dragEnterEvent(QDragEnterEvent* event) override;
        /**
         * @brief Handles when a file is dropped into the window.
         * @param event QDropEvent
         */
        void dropEvent(QDropEvent* event) override;

    private Q_SLOTS:
        /**
         * @brief Handles when a navigation item is selected.
         */
        void onNavigationItemSelected(const QString& id);
        /**
         * @brief Prompts the user to open a folder.
         */
        void openFolder();
        /**
         * @brief Closes the folder if one is open.
         */
        void closeFolder();
        /**
         * @brief Checks for application updates.
         */
        void checkForUpdates();
#ifdef _WIN32
        /**
         * @brief Downloads and installs the latest application update in the background.
         */
        void windowsUpdate();
#endif
        /**
         * @brief Opens the application's GitHub repo in the browser.
         */
        void gitHubRepo();
        /**
         * @brief Opens the application's issue tracker in the browser.
         */
        void reportABug();
        /**
         * @brief Opens the application's discussions page in the browser.
         */
        void discussions();
        /**
         * @brief Displays the about dialog.
         */
        void about();

    private:
        /**
         * @brief Handles when a notification is sent.
         * @param args The NotificationSentEventArgs
         */
        void onNotificationSent(const Notifications::NotificationSentEventArgs& args);
        /**
         * @brief Handles when a shell notification is sent.
         * @param args The ShellNotificationSentEventArgs
         */
        void onShellNotificationSent(const Notifications::ShellNotificationSentEventArgs& args);
        /**
         * @brief Handles when the folder is changed.
         * @param args The EventArgs
         */
        void onFolderChanged(const Events::EventArgs& args);
        Ui::MainWindow* m_ui;
        Controls::NavigationBar* m_navigationBar;
        std::shared_ptr<Shared::Controllers::MainWindowController> m_controller;
    };
}

#endif //MAINWINDOW_H