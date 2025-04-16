#ifndef IMAGEDIALOG_H
#define IMAGEDIALOG_H

#include <adwaita.h>
#include "helpers/dialogbase.h"
#include "models/backgroundimage.h"

namespace Nickvision::Cavalier::GNOME::Controls
{
    /**
     * @brief A dialog for managing a background image.
     */
    class ImageDialog : public Helpers::DialogBase
    {
    public:
        /**
         * @brief Constructs an ImageDialog.
         * @param image The BackgroundImage object to manage
         * @param parent The GtkWindow object of the parent window
         */
        ImageDialog(const Shared::Models::BackgroundImage& image, GtkWindow* parent);
        /**
         * @brief Gets the background image managed by the dialog.
         * @return The BackgroundImage
         */
        const Shared::Models::BackgroundImage& getImage() const;

    private:
        /**
         * @brief Handles when the dialog closes.
         */
        void onClosed();
        Shared::Models::BackgroundImage m_image;
    };
}

#endif //IMAGEDIALOG_H
