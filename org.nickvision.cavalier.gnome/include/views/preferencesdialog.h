#ifndef PREFERENCESDIALOG_H
#define PREFERENCESDIALOG_H

#include <memory>
#include <unordered_map>
#include <adwaita.h>
#include "controllers/preferencesviewcontroller.h"
#include "helpers/dialogbase.h"

namespace Nickvision::Cavalier::GNOME::Views
{
    /**
     * @brief The preferences dialog for the application.
     */
    class PreferencesDialog : public Helpers::DialogBase
    {
    public:
        /**
         * @brief Constructs a PreferencesDialog.
         * @param controller The PreferencesViewController
         * @param parent The GtkWindow object of the parent window
         */
        PreferencesDialog(const std::shared_ptr<Shared::Controllers::PreferencesViewController>& controller, GtkWindow* parent);

    private:
        /**
         * @brief Handles when the dialog is closed.
         */
        void onClosed();
        /**
         * @brief Handles when the theme preference is changed.
         */
        void onThemeChanged();
        /**
         * @brief Prompts the user to create a color profile.
         */
        void addColorProfile();
        /**
         * @brief Prompts the user to edit a color profile.
         * @param index The index of the profile to edit
         */
        void editColorProfile(int index);
        /**
         * @brief Prompts the user to delete a color profile.
         * @param index The index of the profile to delete
         */
        void deleteColorProfile(int index);
        /**
         * @brief Prompts the user to add a background image.
         */
        void addBackgroundImage();
        /**
         * @brief Prompts the user to edit a background image.
         * @param index The index of the image to edit
         */
        void editBackgroundImage(int index);
        /**
         * @brief Prompts the user to delete a background image.
         * @param index The index of the image to delete
         */
        void deleteBackgroundImage(int index);
        /**
         * @brief Loads color profile rows.
         */
        void loadColorProfiles();
        /**
         * @brief Loads background image rows.
         */
        void loadBackgroundImages();
        std::shared_ptr<Shared::Controllers::PreferencesViewController> m_controller;
        std::vector<AdwActionRow*> m_colorProfileRows;
        std::vector<AdwActionRow*> m_backgroundImageRows;
    };
}

#endif //PREFERENCESDIALOG_H
