#include "views/settingspage.h"
#include "ui_settingspage.h"
#include <QApplication>
#include <QStyleHints>
#include <libnick/localization/gettext.h>

using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;

namespace Nickvision::Cavalier::Qt::Views
{
    SettingsPage::SettingsPage(const std::shared_ptr<PreferencesViewController>& controller, QWidget* parent)
        : QWidget{ parent },
        m_ui{ new Ui::SettingsPage() },
        m_controller{ controller }
    {
        m_ui->setupUi(this);
        //Localize Strings
        m_ui->tabs->setTabText(0, _("User Interface"));
        m_ui->lblTheme->setText(_("Theme"));
        m_ui->cmbTheme->addItem(_("Light"));
        m_ui->cmbTheme->addItem(_("Dark"));
        m_ui->cmbTheme->addItem(_("System"));
        m_ui->lblUpdates->setText(_("Automatically Check for Updates"));
        //Load Settings
        m_ui->tabs->setCurrentIndex(0);
        m_ui->cmbTheme->setCurrentIndex(static_cast<int>(m_controller->getTheme()));
        m_ui->chkUpdates->setChecked(m_controller->getAutomaticallyCheckForUpdates());
        //Signals
        connect(m_ui->cmbTheme, &QComboBox::currentIndexChanged, this, &SettingsPage::onThemeChanged);
    }
    
    SettingsPage::~SettingsPage()
    {
        delete m_ui;
    }

    void SettingsPage::onThemeChanged(int index)
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

    void SettingsPage::closeEvent(QCloseEvent* event)
    {
        m_controller->setTheme(static_cast<Theme>(m_ui->cmbTheme->currentIndex()));
        m_controller->setAutomaticallyCheckForUpdates(m_ui->chkUpdates->isChecked());
        m_controller->saveConfiguration();
        event->accept();
    }
}
