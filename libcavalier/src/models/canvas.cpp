#include "models/canvas.h"

namespace Nickvision::Cavalier::Shared::Models
{
    Canvas::Canvas(int width, int height)
        : m_surface{ SkSurfaces::Raster(SkImageInfo::MakeN32Premul(width, height)) },
        m_width{ width },
        m_height{ height }
    {

    }

    Canvas::Canvas(const sk_sp<SkSurface>& surface, int width, int height)
        : m_surface{ surface },
        m_width{ width },
        m_height{ height }
    {

    }

    const sk_sp<SkSurface>& Canvas::getSkiaSurface() const
    {
        return m_surface;
    }

    SkCanvas* Canvas::getSkiaCanvas() const
    {
        return m_surface->getCanvas();
    }

    int Canvas::getWidth() const
    {
        return m_width;
    }

    int Canvas::getHeight() const
    {
        return m_height;
    }

    SkCanvas* Canvas::operator->()
    {
        return m_surface->getCanvas();
    }
}
