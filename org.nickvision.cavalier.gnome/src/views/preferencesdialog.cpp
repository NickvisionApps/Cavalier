#include "views/preferencesdialog.h"
#include <utility>
#include <libnick/localization/gettext.h>
#include "controls/imagedialog.h"
#include "helpers/dialogptr.h"
#include "helpers/gtkhelpers.h"

using namespace Nickvision::Cavalier::GNOME::Controls;
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
        loadColorProfiles();
        loadBackgroundImages();
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
        AdwAlertDialog* dialog{ ADW_ALERT_DIALOG(adw_alert_dialog_new(_("Delete Profile?"), _("Are you sure you want to delete this profile?"))) };
        adw_alert_dialog_add_responses(dialog, "yes", _("Yes"), "no", _("No"), nullptr);
        adw_alert_dialog_set_response_appearance(dialog, "yes", ADW_RESPONSE_DESTRUCTIVE);
        adw_alert_dialog_set_default_response(dialog, "no");
        adw_alert_dialog_set_close_response(dialog, "no");
        g_signal_connect_data(dialog, "response", GCallback(+[](AdwAlertDialog*, const char* response, gpointer data)
        {
            std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
            if(std::string(response) == "yes")
            {
                std::vector<ColorProfile> profiles{ pair->first->m_controller->getColorProfiles() };
                profiles.erase(profiles.begin() + pair->second);
                pair->first->m_controller->setColorProfiles(profiles);
                pair->first->loadColorProfiles();
            }
        }), new std::pair<PreferencesDialog*, int>(this, index), GClosureNotify(+[](gpointer data, GClosure*)
        {
            delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
        }), G_CONNECT_DEFAULT);
        adw_dialog_present(ADW_DIALOG(dialog), GTK_WIDGET(m_dialog));
    }

    void PreferencesDialog::addBackgroundImage()
    {
        GtkFileDialog* fileDialog{ gtk_file_dialog_new() };
        gtk_file_dialog_set_title(fileDialog, _("Select Background Image"));
        gtk_file_dialog_open(fileDialog, m_parent, nullptr, GAsyncReadyCallback(+[](GObject* self, GAsyncResult* res, gpointer data)
        {
            GFile* file{ gtk_file_dialog_open_finish(GTK_FILE_DIALOG(self), res, nullptr) };
            if(file)
            {
                PreferencesDialog* dialog{ reinterpret_cast<PreferencesDialog*>(data) };
                DialogPtr<ImageDialog> ptr{ BackgroundImage(g_file_get_path(file)), dialog->m_parent };
                ptr->closed() += [=](const EventArgs&)
                {
                    std::vector<BackgroundImage> images{ dialog->m_controller->getBackgroundImages() };
                    images.push_back(ptr->getImage());
                    dialog->m_controller->setBackgroundImages(images);
                    dialog->loadBackgroundImages();
                };
                ptr->present();
            }
        }), this);
    }

    void PreferencesDialog::editBackgroundImage(int index)
    {
        DialogPtr<ImageDialog> dialog{ m_controller->getBackgroundImages()[index], m_parent };
        dialog->closed() += [=, this](const EventArgs&)
        {
            std::vector<BackgroundImage> images{ m_controller->getBackgroundImages() };
            images[index] = dialog->getImage();
            m_controller->setBackgroundImages(images);
            loadBackgroundImages();
        };
        dialog->present();
    }

    void PreferencesDialog::deleteBackgroundImage(int index)
    {
        AdwAlertDialog* dialog{ ADW_ALERT_DIALOG(adw_alert_dialog_new(_("Delete Image?"), _("Are you sure you want to delete this image?"))) };
        adw_alert_dialog_add_responses(dialog, "yes", _("Yes"), "no", _("No"), nullptr);
        adw_alert_dialog_set_response_appearance(dialog, "yes", ADW_RESPONSE_DESTRUCTIVE);
        adw_alert_dialog_set_default_response(dialog, "no");
        adw_alert_dialog_set_close_response(dialog, "no");
        g_signal_connect_data(dialog, "response", GCallback(+[](AdwAlertDialog*, const char* response, gpointer data)
        {
            std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
            if(std::string(response) == "yes")
            {
                std::vector<BackgroundImage> images{ pair->first->m_controller->getBackgroundImages() };
                images.erase(images.begin() + pair->second);
                pair->first->m_controller->setBackgroundImages(images);
                pair->first->loadBackgroundImages();
            }
        }), new std::pair<PreferencesDialog*, int>(this, index), GClosureNotify(+[](gpointer data, GClosure*)
        {
            delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
        }), G_CONNECT_DEFAULT);
        adw_dialog_present(ADW_DIALOG(dialog), GTK_WIDGET(m_dialog));
    }

    void PreferencesDialog::loadColorProfiles()
    {
        //Clear old rows
        for(AdwActionRow* row : m_colorProfileRows)
        {
            adw_preferences_group_remove(ADW_PREFERENCES_GROUP(m_builder.get<AdwPreferencesGroup>("colorProfilesRow")), GTK_WIDGET(row));
        }
        //Load active profiles
        GtkHelpers::setComboRowModel(m_builder.get<AdwComboRow>("activeColorProfileRow"), m_controller->getColorProfileNames(), m_controller->getActiveColorProfileIndex());
        //Load rows
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
                g_signal_connect_data(btnDelete, "clicked", GCallback(+[](GtkButton*, gpointer data)
                {
                    std::pair<PreferencesDialog*, int>* pair{ reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data) };
                    pair->first->deleteColorProfile(pair->second);
                }), new std::pair<PreferencesDialog*, int>(this, i), GClosureNotify(+[](gpointer data, GClosure*)
                {
                    delete reinterpret_cast<std::pair<PreferencesDialog*, int>*>(data);
                }), G_CONNECT_DEFAULT);
            }
            adw_preferences_group_add(m_builder.get<AdwPreferencesGroup>("colorProfilesGroup"), GTK_WIDGET(row));
            m_colorProfileRows.push_back(row);
            i++;
        }
    }

    void PreferencesDialog::loadBackgroundImages()
    {
        //Clear old rows
        for(AdwActionRow* row : m_backgroundImageRows)
        {
            adw_preferences_group_remove(ADW_PREFERENCES_GROUP(m_builder.get<AdwPreferencesGroup>("backgroundImagesGroup")), GTK_WIDGET(row));
        }
        m_backgroundImageRows.clear();
        //Load active images
        GtkHelpers::setComboRowModel(m_builder.get<AdwComboRow>("activeBackgroundImageRow"), m_controller->getBackgroundImageNames(), m_controller->getActiveBackgroundImageIndex());
        //Load rows
        int i{ 0 };
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
            g_signal_connect_data(btnDelete, "clicked", GCallback(+[](GtkButton*, gpointer data)
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
            m_backgroundImageRows.push_back(row);
            i++;
        }
    }
}
