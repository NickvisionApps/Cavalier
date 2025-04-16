#include "application.h"
#include <QStyleHints>

using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;

namespace Nickvision::Cavalier::Qt
{
    Application::Application(int argc, char* argv[])
        : QApplication{ argc, argv },
        m_controller{ std::make_shared<MainWindowController>(std::vector<std::string>(argv, argv + argc)) },
        m_mainWindow{ nullptr }
    {
        //Set Fusion style on Windows 10 for dark mode support
        if (QSysInfo::productType() == "windows" && QSysInfo::productVersion() == "10")
        {
            QApplication::setStyle("Fusion");
        }
    }

    int Application::exec()
    {
        switch (m_controller->getTheme())
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
        m_mainWindow = std::make_shared<Views::MainWindow>(m_controller);
        m_mainWindow->show();
        return QApplication::exec();
    }
}
