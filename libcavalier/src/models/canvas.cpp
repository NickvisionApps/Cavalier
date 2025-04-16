#include "models/canvas.h"
#include <skia/include/core/SkImageInfo.h>
#include <skia/include/gpu/ganesh/SkSurfaceGanesh.h>
#include <skia/include/gpu/ganesh/gl/GrGLAssembleInterface.h>
#include <skia/include/gpu/ganesh/gl/GrGLBackendSurface.h>
#include <skia/include/gpu/ganesh/gl/GrGLDirectContext.h>
#include <skia/include/gpu/ganesh/gl/GrGLInterface.h>
#include <skia/include/gpu/ganesh/gl/GrGLTypes.h>

namespace Nickvision::Cavalier::Shared::Models
{
    static int s_count = 0;

    Canvas::Canvas(int width, int height)
        : m_glfw{ nullptr },
        m_surface{ nullptr },
        m_isGPUCanvas{ false },
        m_width{ width },
        m_height{ height }
    {
        ++s_count;
        SkImageInfo info{ SkImageInfo::MakeN32Premul(width, height) };
        //Create GPU Renderer
        if(glfwInit() == GLFW_TRUE)
        {
            glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
            GLFWwindow* window{ glfwCreateWindow(width, height, "", nullptr, nullptr) };
            if(window)
            {
                glfwMakeContextCurrent(window);
                sk_sp<const GrGLInterface> interface{ GrGLMakeNativeInterface() };
                if(!interface)
                {
                    interface = GrGLMakeAssembledInterface(nullptr, +[](void*, const char* name)
                    {
                        return glfwGetProcAddress(name);
                    });
                }
                m_context = GrDirectContexts::MakeGL(interface);
                sk_sp<SkSurface> gpuSurface{ SkSurfaces::RenderTarget(m_context.get(), skgpu::Budgeted::kNo, info) };
                if(gpuSurface)
                {
                    m_surface = gpuSurface;
                    m_isGPUCanvas = true;
                    return;
                }
            }
        }
        //Fallback to CPU
        m_surface = SkSurfaces::Raster(info);
    }

    Canvas::~Canvas()
    {
        --s_count;
        if(m_glfw)
        {
            glfwDestroyWindow(m_glfw);
        }
        if(s_count == 0)
        {
            glfwTerminate();
        }
    }

    bool Canvas::isValid() const
    {
        return m_surface.operator bool();
    }
    bool Canvas::isGPUCanvas() const
    {
        return m_isGPUCanvas;
    }

    const sk_sp<SkSurface>& Canvas::getSkiaSurface() const
    {
        return m_surface;
    }

    SkCanvas* Canvas::getSkiaCanvas() const
    {
        return m_surface->getCanvas();
    }

    GrDirectContext* Canvas::getSkiaContext() const
    {
        return m_context.get();
    }

    int Canvas::getWidth() const
    {
        return m_width;
    }

    int Canvas::getHeight() const
    {
        return m_height;
    }

    void Canvas::flush()
    {
        if(m_context)
        {
            m_context->flushAndSubmit(m_surface.get(), GrSyncCpu::kYes);
        }
    }

    SkCanvas* Canvas::operator->()
    {
        return m_surface->getCanvas();
    }

    Canvas::operator bool() const
    {
        return isValid();
    }
}
