#include "controls/imagedialog.h"

#define PICTURE_WIDTH 300
#define PICTURE_HEIGHT 200

using namespace Nickvision::Cavalier::GNOME::Helpers;
using namespace Nickvision::Cavalier::Shared::Models;
using namespace Nickvision::Events;

namespace Nickvision::Cavalier::GNOME::Controls
{
    ImageDialog::ImageDialog(const BackgroundImage& image, GtkWindow* parent)
        : DialogBase{ parent, "image_dialog" },
        m_image{ image }
    {
        //Load
        adw_window_title_set_title(m_builder.get<AdwWindowTitle>("windowTitle"), image.getPath().filename().string().c_str());
        adw_window_title_set_subtitle(m_builder.get<AdwWindowTitle>("windowTitle"), image.getPath().parent_path().string().c_str());
        GdkPixbuf* pixbuf{ gdk_pixbuf_new_from_file_at_scale(image.getPath().string().c_str(), PICTURE_WIDTH, PICTURE_HEIGHT, true, nullptr) };
        GdkTexture* texture{ gdk_texture_new_for_pixbuf(pixbuf) };
        gtk_widget_set_size_request(m_builder.get<GtkWidget>("picture"), PICTURE_WIDTH, PICTURE_HEIGHT);
        gtk_picture_set_paintable(m_builder.get<GtkPicture>("picture"), GDK_PAINTABLE(texture));
        g_object_unref(texture);
        g_object_unref(pixbuf);
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("scaleRow"), static_cast<double>(image.getScale()));
        adw_spin_row_set_value(m_builder.get<AdwSpinRow>("alphaRow"), static_cast<double>(image.getAlpha()));
        //Signals
        m_closed += [this](const EventArgs&){ onClosed(); };
    }

    const BackgroundImage& ImageDialog::getImage() const
    {
        return m_image;
    }

    void ImageDialog::onClosed()
    {
        m_image.setScale(static_cast<unsigned int>(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("scaleRow"))));
        m_image.setAlpha(static_cast<unsigned int>(adw_spin_row_get_value(m_builder.get<AdwSpinRow>("alphaRow"))));
    }
}
