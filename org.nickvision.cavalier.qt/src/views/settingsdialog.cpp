#include "views/settingsdialog.h"
#include "ui_settingsdialog.h"
#include <QApplication>
#include <QStyleHints>
#include <libnick/localization/gettext.h>

using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;

namespace Nickvision::Cavalier::Qt::Views
{
    SettingsDialog::SettingsDialog(const std::shared_ptr<PreferencesViewController>& controller, QWidget* parent)
        : QDialog{ parent },
        m_ui{ new Ui::SettingsDialog() },
        m_controller{ controller }
    {
        m_ui->setupUi(this);
        setWindowTitle(_("Settings"));
        //Localize Strings
        m_ui->lblTheme->setText(_("Theme"));
        m_ui->cmbTheme->addItem(_("Light"));
        m_ui->cmbTheme->addItem(_("Dark"));
        m_ui->cmbTheme->addItem(_("System"));
        m_ui->lblUpdates->setText(_("Automatically Check for Updates"));
        //Add Navigation Items
        m_ui->listNavigation->addItem(new QListWidgetItem(QIcon::fromTheme(QIcon::ThemeIcon::Computer), _("User Interface"), m_ui->listNavigation));
        //Load
        m_ui->listNavigation->setCurrentRow(0);
        m_ui->viewStack->setCurrentIndex(0);
        m_ui->cmbTheme->setCurrentIndex(static_cast<int>(m_controller->getTheme()));
        m_ui->chkUpdates->setChecked(m_controller->getAutomaticallyCheckForUpdates());
        //Signals
        connect(m_ui->listNavigation, &QListWidget::currentRowChanged, this, &SettingsDialog::onNavigationChanged);
        connect(m_ui->cmbTheme, &QComboBox::currentIndexChanged, this, &SettingsDialog::onThemeChanged);
    }

    SettingsDialog::~SettingsDialog()
    {
        delete m_ui;
    }

    void SettingsDialog::closeEvent(QCloseEvent* event)
    {
        m_controller->setTheme(static_cast<Theme>(m_ui->cmbTheme->currentIndex()));
        m_controller->setAutomaticallyCheckForUpdates(m_ui->chkUpdates->isChecked());
        m_controller->saveConfiguration();
        event->accept();
    }

    void SettingsDialog::onNavigationChanged()
    {
        m_ui->viewStack->setCurrentIndex(m_ui->listNavigation->currentRow());
    }

    void SettingsDialog::onThemeChanged()
    {
        switch (static_cast<Theme>(m_ui->cmbTheme->currentIndex()))
        {
        case Theme::Light:
            QApplication::styleHints()->setColorScheme(::Qt::ColorScheme::Light);
            break;
        case Theme::Dark:
            QApplication::styleHints()->setColorScheme(::Qt::ColorScheme::Dark);
            break;
        default:
            QApplication::styleHints()->setColorScheme(::Qt::ColorScheme::Unknown);
            break;
        }
    }
}
