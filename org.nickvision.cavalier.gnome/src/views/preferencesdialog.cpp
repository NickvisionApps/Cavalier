#include "views/preferencesdialog.h"
#include <utility>
#include <libnick/localization/gettext.h>
#include "helpers/gtkhelpers.h"

using namespace Nickvision::Cavalier::GNOME::Helpers;
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
        GtkHelpers::setComboRowModel(m_builder.get<AdwComboRow>("activeColorProfileRow"), m_controller->getColorProfileNames(), m_controller->getActiveColorProfileIndex());
        int i{ 0 };
        for(const ColorProfile& profile : m_controller->getColorProfiles())
        {
            GtkButton* btnEdit{ GTK_BUTTON(gtk_button_new()) };
            gtk_button_set_icon_name(btnEdit, "document-edit-symbolic");
            gtk_widget_set_valign(GTK_WIDGET(btnEdit), GTK_ALIGN_CENTER);
            gtk_widget_add_css_class(GTK_WIDGET(btnEdit), "flat");
            g_signal_connect_data(btnEdit, "clicked", GCallback(+[](GtkButton*, gpointer data)
            {
                std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
                pair->first->editColorProfile(pair->second);
            }), new std::pair<PreferencesDialog*, int>(this, i), GClosureNotify(+[](gpointer data, GClosure*)
            {
                delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
            }), G_CONNECT_DEFAULT);
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), profile.getName().c_str());
            adw_action_row_add_suffix(row, GTK_WIDGET(btnEdit));
            adw_action_row_set_activatable_widget(row, GTK_WIDGET(btnEdit));
            if(profile.getName() != _("Default"))
            {
                GtkButton* btnDelete{ GTK_BUTTON(gtk_button_new()) };
                gtk_button_set_icon_name(btnDelete, "user-trash-symbolic");
                gtk_widget_set_valign(GTK_WIDGET(btnDelete), GTK_ALIGN_CENTER);
                gtk_widget_add_css_class(GTK_WIDGET(btnDelete), "flat");
                adw_action_row_add_suffix(row, GTK_WIDGET(btnDelete));
                g_signal_connect_data(btnEdit, "clicked", GCallback(+[](GtkButton*, gpointer data)
                {
                    std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
                    pair->first->deleteColorProfile(pair->second);
                }), new std::pair<PreferencesDialog*, int>(this, i), GClosureNotify(+[](gpointer data, GClosure*)
                {
                    delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
                }), G_CONNECT_DEFAULT);
            }
            adw_preferences_group_add(m_builder.get<AdwPreferencesGroup>("colorProfilesGroup"), GTK_WIDGET(row));
            i++;
        }
        GtkHelpers::setComboRowModel(m_builder.get<AdwComboRow>("activeBackgroundImageRow"), m_controller->getBackgroundImageNames(), m_controller->getActiveBackgroundImageIndex());
        i = 0;
        for(const BackgroundImage& image : m_controller->getBackgroundImages())
        {
            GtkButton* btnEdit{ GTK_BUTTON(gtk_button_new()) };
            gtk_button_set_icon_name(btnEdit, "document-edit-symbolic");
            gtk_widget_set_valign(GTK_WIDGET(btnEdit), GTK_ALIGN_CENTER);
            gtk_widget_add_css_class(GTK_WIDGET(btnEdit), "flat");
            g_signal_connect_data(btnEdit, "clicked", GCallback(+[](GtkButton*, gpointer data)
            {
                std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
                pair->first->editBackgroundImage(pair->second);
            }), new std::pair<PreferencesDialog*, int>(this, i), GClosureNotify(+[](gpointer data, GClosure*)
            {
                delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
            }), G_CONNECT_DEFAULT);
            GtkButton* btnDelete{ GTK_BUTTON(gtk_button_new()) };
            gtk_button_set_icon_name(btnDelete, "user-trash-symbolic");
            gtk_widget_set_valign(GTK_WIDGET(btnDelete), GTK_ALIGN_CENTER);
            gtk_widget_add_css_class(GTK_WIDGET(btnDelete), "flat");
            g_signal_connect_data(btnEdit, "clicked", GCallback(+[](GtkButton*, gpointer data)
            {
                std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
                pair->first->deleteBackgroundImage(pair->second);
            }), new std::pair<PreferencesDialog*, int>(this, i), GClosureNotify(+[](gpointer data, GClosure*)
            {
                delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
            }), G_CONNECT_DEFAULT);
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), image.getPath().filename().string().c_str());
            adw_action_row_set_subtitle(row, image.getPath().parent_path().string().c_str());
            adw_action_row_add_suffix(row, GTK_WIDGET(btnEdit));
            adw_action_row_set_activatable_widget(row, GTK_WIDGET(btnEdit));
            adw_action_row_add_suffix(row, GTK_WIDGET(btnDelete));
            adw_preferences_group_add(m_builder.get<AdwPreferencesGroup>("backgroundImagesGroup"), GTK_WIDGET(row));
            i++;
        }
        //Signals
        m_closed += [&](const EventArgs&){ onClosed(); };
        g_signal_connect(m_builder.get<GObject>("themeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
        g_signal_connect(m_builder.get<GObject>("addColorProfileButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->addColorProfile(); }), this);
        g_signal_connect(m_builder.get<GObject>("addBackgroundImageButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->addBackgroundImage(); }), this);
    }
    
    void PreferencesDialog::onClosed()
    {
        DrawingArea drawing{ m_controller->getDrawingArea() };
        CavaOptions cava{ m_controller->getCavaOptions() };
        drawing.setMode(static_cast<DrawingMode>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("drawingModeRow"))));
        drawing.setShape(static_cast<DrawingShape>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("drawingShapeRow"))));
        drawing.setDirection(static_cast<DrawingDirection>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("drawingDirectionRow"))));
        drawing.setFillShape(adw_switch_row_get_active(m_builder.get<AdwSwitchRow>("fillShapeRow")));
        drawing.setMirrorMode(static_cast<MirrorMode>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("mirrowModeRow"))));
        drawing.setMargin(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("drawingMarginRow")));
        drawing.setXOffset(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("drawingXOffset")));
        drawing.setYOffset(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("drawingYOffset")));
        drawing.setItemSpacing(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("itemSpacingRow")));
        drawing.setItemRoundness(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("itemRoundnessRow")));
        cava.setChannels(static_cast<ChannelType>(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("channelsRow"))));
        cava.setFramerate(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("framerateRow")));
        cava.setNumberOfBars(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("numberOfBarsRow")));
        cava.setReverseBarOrder(adw_switch_row_get_active(m_builder.get<AdwSwitchRow>("reverseBarOrderRow")));
        cava.setUseAutomaticSensitivity(adw_switch_row_get_active(m_builder.get<AdwSwitchRow>("automaticSensitivityRow")));
        cava.setSensitivity(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("sensitivityRow")));
        cava.setUseMonstercatSmoothing(adw_switch_row_get_active(m_builder.get<AdwSwitchRow>("monstercatRow")));
        cava.setNoiseReductionFactor(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("nosieReductionRow")));
        m_controller->setDrawingArea(drawing);
        m_controller->setCavaOptions(cava);
        m_controller->setActiveColorProfileIndex(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("activeColorProfileRow")));
        m_controller->setActiveBackgroundImageIndex(adw_combo_row_get_selected(m_builder.get<AdwComboRow>("activeBackgroundImageRow")));
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

    void PreferencesDialog::addColorProfile()
    {

    }

    void PreferencesDialog::editColorProfile(int index)
    {

    }

    void PreferencesDialog::deleteColorProfile(int index)
    {

    }

    void PreferencesDialog::addBackgroundImage()
    {

    }

    void PreferencesDialog::editBackgroundImage(int index)
    {

    }

    void PreferencesDialog::deleteBackgroundImage(int index)
    {

    }
}
