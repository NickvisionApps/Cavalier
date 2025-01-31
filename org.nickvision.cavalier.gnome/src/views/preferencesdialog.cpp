#include "views/preferencesdialog.h"

using namespace Nickvision::Cavalier::Shared::Controllers;
using namespace Nickvision::Cavalier::Shared::Models;
using namespace Nickvision::Events;

namespace Nickvision::Cavalier::GNOME::Views
{
    PreferencesDialog::PreferencesDialog(const std::shared_ptr<PreferencesViewController>& controller, GtkWindow* parent)
        : DialogBase{ parent, "preferences_dialog" },
        m_controller{ controller }
    {
        //Load
        DrawingArea drawing{ m_controller->getDrawingArea() };
        CavaOptions cava{ m_controller->getCavaOptions() };
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("themeRow"), static_cast<unsigned int>(m_controller->getTheme()));
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("drawingModeRow"), static_cast<unsigned int>(drawing.getMode()));
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("drawingShapeRow"), static_cast<unsigned int>(drawing.getShape()));
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("drawingDirectionRow"), static_cast<unsigned int>(drawing.getDirection()));
        adw_switch_row_set_active(m_builder.get<AdwSwitchRow>("fillShapeRow"), drawing.getFillShape());
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("mirrowModeRow"), static_cast<unsigned int>(drawing.getMirrorMode()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("drawingMarginRow"), static_cast<double>(drawing.getMargin()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("drawingXOffset"), static_cast<double>(drawing.getXOffset()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("drawingYOffset"), static_cast<double>(drawing.getYOffset()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("itemSpacingRow"), static_cast<double>(drawing.getItemSpacing()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("itemRoundnessRow"), static_cast<double>(drawing.getItemRoundness()));
        adw_combo_row_set_selected(m_builder.get<AdwComboRow>("channelsRow"), static_cast<unsigned int>(cava.getChannels()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("framerateRow"), static_cast<double>(cava.getFramerate()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("numberOfBarsRow"), static_cast<double>(cava.getNumberOfBars()));
        adw_switch_row_set_active(m_builder.get<AdwSwitchRow>("reverseBarOrderRow"), cava.getReverseBarOrder());
        adw_switch_row_set_active(m_builder.get<AdwSwitchRow>("automaticSensitivityRow"), cava.getUseAutomaticSensitivity());
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("sensitivityRow"), static_cast<double>(cava.getSensitivity()));
        adw_switch_row_set_active(m_builder.get<AdwSwitchRow>("monstercatRow"), cava.getUseMonstercatSmoothing());
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("nosieReductionRow"), static_cast<double>(cava.getNoiseReductionFactor()));
        //Signals
        m_closed += [&](const EventArgs&){ onClosed(); };
        g_signal_connect(m_builder.get<GObject>("themeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
    }
    
    void PreferencesDialog::onClosed()
    {
        DrawingArea drawing{ m_controller->getDrawingArea() };
        CavaOptions cava{ m_controller->getCavaOptions() };
        drawing.setMode(static_cast<DrawingMode>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("drawingModeRow"))));

        m_controller->setDrawingArea(drawing);
        m_controller->setCavaOptions(cava);
        m_controller->saveConfiguration();
    }

    void PreferencesDialog::onThemeChanged()
    {
        m_controller->setTheme(static_cast<Theme>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("themeRow"))));
        switch (m_controller->getTheme())
        {
        case Theme::Light:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
            break;
        case Theme::Dark:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
            break;
        default:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_DEFAULT);
            break;
        }
    }
}
