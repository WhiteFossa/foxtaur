using System;
using System.IO;
using Foxtaur.LibRenderer.Helpers;
using NLog;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

public class Shader : IDisposable
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        private uint _handle;
        private GL _silkGl;

        public Shader(GL silkGl, string vertexPath, string fragmentPath)
        {
            _silkGl = silkGl ?? throw new ArgumentNullException(nameof(silkGl));

            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            
            _handle = _silkGl.CreateProgram();
            
            _silkGl.AttachShader(_handle, vertex);
            _silkGl.AttachShader(_handle, fragment);
            
            _silkGl.LinkProgram(_handle);
            _silkGl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                RendererHelper.LogAndThrowFatalError(_logger, $"Program failed to link with error: { _silkGl.GetProgramInfoLog(_handle) }");
            }
            
            _silkGl.DetachShader(_handle, vertex);
            _silkGl.DetachShader(_handle, fragment);
            _silkGl.DeleteShader(vertex);
            _silkGl.DeleteShader(fragment);
        }

        public void Use()
        {
            _silkGl.UseProgram(_handle);
        }

        public void SetUniform1i(string name, int value)
        {
            int location = _silkGl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                RendererHelper.LogAndThrowFatalError(_logger, $"{ name } uniform not found on shader.");
            }
            _silkGl.Uniform1(location, value);
        }
        
        public void SetUniform1f(string name, float value)
        {
            int location = _silkGl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                RendererHelper.LogAndThrowFatalError(_logger, $"{ name } uniform not found on shader.");
            }
            _silkGl.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = _silkGl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                RendererHelper.LogAndThrowFatalError(_logger, $"{ name } uniform not found on shader.");
            }
            _silkGl.Uniform1(location, value);
        }

        public void Dispose()
        {
            _silkGl.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            
            uint handle = _silkGl.CreateShader(type);
            _silkGl.ShaderSource(handle, src);
            _silkGl.CompileShader(handle);
            
            string infoLog = _silkGl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                RendererHelper.LogAndThrowFatalError(_logger, $"Error compiling shader of type { type }, failed with error { infoLog }");
            }

            return handle;
        }
    }