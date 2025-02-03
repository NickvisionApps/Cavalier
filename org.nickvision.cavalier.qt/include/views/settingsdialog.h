#ifndef SETTINGSDIALOG_H
#define SETTINGSDIALOG_H

#include <memory>
#include <QCloseEvent>
#include <QDialog>
#include "controllers/preferencesviewcontroller.h"

namespace Ui { class SettingsDialog; }

namespace Nickvision::Cavalier::Qt::Views
{
    /**
     * @brief The settings dialog for the application.
     */
    class SettingsDialog : public QDialog
    {
    public:
        /**
         * @brief Constructs a SettingsDialog.
         * @param controller The PreferencesViewController
         * @param parent The parent widget
         */
        SettingsDialog(const std::shared_ptr<Shared::Controllers::PreferencesViewController>& controller, QWidget* parent = nullptr);
        /**
         * @brief Destructs a SettingsDialog.
         */
        ~SettingsDialog();

    protected:
        /**
         * @brief Handles when the dialog is closed.
         * @param event QCloseEvent
         */
        void closeEvent(QCloseEvent* event) override;

    private Q_SLOTS:
        /**
         * @brief Handles when the navigation row changes.
         */
        void onNavigationChanged();
        /**
         * @brief Handles when the theme combobox changes.
         */
        void onThemeChanged();

    private:
        Ui::SettingsDialog* m_ui;
        std::shared_ptr<Shared::Controllers::PreferencesViewController> m_controller;
    };
}

#endif //SETTINGSDIALOG_H
