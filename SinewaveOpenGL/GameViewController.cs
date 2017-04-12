using System;
using System.Diagnostics;

using Foundation;
using GLKit;
using OpenGLES;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace SinewaveOpenGL
{
    [Register("GameViewController")]
    public class GameViewController : GLKViewController, IGLKViewDelegate
    {
        #region DATA
        float[] m_TriangleVertices = null;
        //{
        //    0.0f, 0.9f, 0.0f, // top
        //    0.8f, -0.9f, 0.0f,// right
        //   -0.8f, -0.9f, 0.0f,// left
        //};
        #endregion

        EAGLContext m_Context { get; set; }
        //
        // We will use a VAO
        int m_vaoObject = 0;
        int m_vertexBuffer = 0;
        //
        int m_shaderProgram = 0;
        //
        float m_offsetSin = 0.0f;


        [Export("initWithCoder:")]
        public GameViewController(NSCoder coder) : base(coder)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            m_Context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);

            if (m_Context == null)
            {
                Debug.WriteLine("Failed to create ES context");
            }

            var view = (GLKView)View;
            view.Context = m_Context;
            view.DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;

            SetupGL();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            TearDownGL();

            if (EAGLContext.CurrentContext == m_Context)
                EAGLContext.SetCurrentContext(null);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            if (IsViewLoaded && View.Window == null)
            {
                View = null;

                TearDownGL();

                if (EAGLContext.CurrentContext == m_Context)
                {
                    EAGLContext.SetCurrentContext(null);
                }
            }

            // Dispose of any resources that can be recreated.
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }


        void CreateSineWave()
        {
            float x = -0.9f;
            float y = 0.0f;
            float multiplier = 1.00f;

            for (int i = 0; i < m_TriangleVertices.Length; i++)
            {
                int index = i % 3;
                switch (index)
                {
                    case 0:
                        m_TriangleVertices[i] = x;
                        break;
                    case 1:
                        m_TriangleVertices[i] = (float)Math.Sin((m_offsetSin +  x) * 2 * Math.PI);
                        break;
                    case 2:
                        continue;
                        break;

                }
                x = x + 0.03f;
                //y = y + (multiplier) * 0.08f;
                //if (y >= 0.11f)
                //{
                //    multiplier = -1.00f;
                //}
                //if (y <= -0.90f)
                //{
                //    multiplier = 1.00f;
                //}

                if (x > 0.90f)
                {
                    x = 0.90f;
                    break;
                }
            }
            //
        }
        void SetupGL()
        {
            //
            // m_TriangleVertices = new float[] {
            // 0.0f, 0.9f, 0.0f, // top
            // 0.8f, -0.9f, 0.0f,// right
            //-0.8f, -0.9f, 0.0f,// left
            //  };
            m_TriangleVertices = new float[3*10];
            //
            CreateSineWave();
            //

            EAGLContext.SetCurrentContext(m_Context);

            LoadShaders();

            GL.Enable(EnableCap.DepthTest);

            GL.Oes.GenVertexArrays(1, out m_vaoObject);
            GL.Oes.BindVertexArray(m_vaoObject);

            GL.GenBuffers(1, out m_vertexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)((m_TriangleVertices.Length-1) * sizeof(float)), m_TriangleVertices, BufferUsage.DynamicDraw);

            GL.EnableVertexAttribArray((int)GLKVertexAttrib.Position);
            GL.VertexAttribPointer((int)GLKVertexAttrib.Position, 3, VertexAttribPointerType.Float, false, 0, new IntPtr(0));
            //GL.EnableVertexAttribArray ((int)GLKVertexAttrib.Normal);
            //GL.VertexAttribPointer ((int)GLKVertexAttrib.Normal, 3, VertexAttribPointerType.Float, false, 24, new IntPtr (12));

            GL.Oes.BindVertexArray(0);
        }

        void TearDownGL()
        {
            EAGLContext.SetCurrentContext(m_Context);
            GL.DeleteBuffers(1, ref m_vertexBuffer);
            GL.Oes.DeleteVertexArrays(1, ref m_vaoObject);


            if (m_shaderProgram > 0)
            {
                GL.DeleteProgram(m_shaderProgram);
                m_shaderProgram = 0;
            }
        }

        #region GLKView and GLKViewController delegate methods

        public override void Update()
        {
            m_offsetSin += 0.01f;

            CreateSineWave();
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)( m_TriangleVertices.Length * sizeof(float)), m_TriangleVertices, BufferUsage.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //var aspect = (float)Math.Abs (View.Bounds.Size.Width / View.Bounds.Size.Height);
            //var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView (MathHelper.DegreesToRadians (65.0f), aspect, 0.1f, 100.0f);


            //var baseModelViewMatrix = Matrix4.CreateTranslation (0.0f, 0.0f, -4.0f);
            //baseModelViewMatrix = Matrix4.CreateFromAxisAngle (new Vector3 (0.0f, 1.0f, 0.0f), rotation) * baseModelViewMatrix;

            //// Compute the model view matrix for the object rendered with GLKit
            //var modelViewMatrix = Matrix4.CreateTranslation (0.0f, 0.0f, -1.5f);
            //modelViewMatrix = Matrix4.CreateFromAxisAngle (new Vector3 (1.0f, 1.0f, 1.0f), rotation) * modelViewMatrix;
            //modelViewMatrix = modelViewMatrix * baseModelViewMatrix;

            //effect.Transform.ModelViewMatrix = modelViewMatrix;

            // Compute the model view matrix for the object rendered with ES2
            //modelViewMatrix = Matrix4.CreateTranslation (0.0f, 0.0f, 1.5f);
            //modelViewMatrix = Matrix4.CreateFromAxisAngle (new Vector3 (1.0f, 1.0f, 1.0f), rotation) * modelViewMatrix;
            //modelViewMatrix = modelViewMatrix * baseModelViewMatrix;

            //normalMatrix = new Matrix3 (Matrix4.Transpose (Matrix4.Invert (modelViewMatrix)));

            //modelViewProjectionMatrix = modelViewMatrix * projectionMatrix;

            //rotation += (float)TimeSinceLastUpdate * 0.5f;
        }

        void IGLKViewDelegate.DrawInRect(GLKView view, CoreGraphics.CGRect rect)
        {
            GL.ClearColor(0.65f, 0.65f, 0.65f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Oes.BindVertexArray(m_vaoObject);
            //
            GL.UseProgram(m_shaderProgram);
            //
            GL.DrawArrays(BeginMode.LineStrip, 0, m_TriangleVertices.Length);
            GL.Oes.BindVertexArray(0);
        }

        bool LoadShaders()
        {
            int vertShader, fragShader;

            // Create shader program.
            m_shaderProgram = GL.CreateProgram();

            // Create and compile vertex shader.
            if (!CompileShader(ShaderType.VertexShader, LoadResource("Shader", "vsh"), out vertShader))
            {
                Console.WriteLine("Failed to compile vertex shader");
                return false;
            }
            // Create and compile fragment shader.
            if (!CompileShader(ShaderType.FragmentShader, LoadResource("Shader", "fsh"), out fragShader))
            {
                Console.WriteLine("Failed to compile fragment shader");
                return false;
            }

            // Attach vertex shader to program.
            GL.AttachShader(m_shaderProgram, vertShader);

            // Attach fragment shader to program.
            GL.AttachShader(m_shaderProgram, fragShader);

            // Bind attribute locations.
            // This needs to be done prior to linking.
            GL.BindAttribLocation(m_shaderProgram, (int)GLKVertexAttrib.Position, "position");
            GL.BindAttribLocation(m_shaderProgram, (int)GLKVertexAttrib.Normal, "normal");

            // Link program.
            if (!LinkProgram(m_shaderProgram))
            {
                Console.WriteLine("Failed to link program: {0:x}", m_shaderProgram);

                if (vertShader != 0)
                    GL.DeleteShader(vertShader);

                if (fragShader != 0)
                    GL.DeleteShader(fragShader);

                if (m_shaderProgram != 0)
                {
                    GL.DeleteProgram(m_shaderProgram);
                    m_shaderProgram = 0;
                }
                return false;
            }

            // Get uniform locations.
            //uniforms [(int)Uniform.ModelViewProjection_Matrix] = GL.GetUniformLocation (m_shaderProgram, "modelViewProjectionMatrix");
            //uniforms [(int)Uniform.Normal_Matrix] = GL.GetUniformLocation (m_shaderProgram, "normalMatrix");

            // Release vertex and fragment shaders.
            if (vertShader != 0)
            {
                GL.DetachShader(m_shaderProgram, vertShader);
                GL.DeleteShader(vertShader);
            }

            if (fragShader != 0)
            {
                GL.DetachShader(m_shaderProgram, fragShader);
                GL.DeleteShader(fragShader);
            }

            return true;
        }

        string LoadResource(string name, string type)
        {
            var path = NSBundle.MainBundle.PathForResource(name, type);
            return System.IO.File.ReadAllText(path);
        }

        #endregion

        bool CompileShader(ShaderType type, string src, out int shader)
        {
            shader = GL.CreateShader(type);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);

#if DEBUG
            int logLength = 0;
            GL.GetShader(shader, ShaderParameter.InfoLogLength, out logLength);
            if (logLength > 0)
            {
                Console.WriteLine("Shader compile log:\n{0}", GL.GetShaderInfoLog(shader));
            }
#endif

            int status = 0;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
            {
                GL.DeleteShader(shader);
                return false;
            }

            return true;
        }

        bool LinkProgram(int prog)
        {
            GL.LinkProgram(prog);

#if DEBUG
            int logLength = 0;
            GL.GetProgram(prog, ProgramParameter.InfoLogLength, out logLength);
            if (logLength > 0)
                Console.WriteLine("Program link log:\n{0}", GL.GetProgramInfoLog(prog));
#endif
            int status = 0;
            GL.GetProgram(prog, ProgramParameter.LinkStatus, out status);
            return status != 0;
        }

        bool ValidateProgram(int prog)
        {
            int logLength, status = 0;

            GL.ValidateProgram(prog);
            GL.GetProgram(prog, ProgramParameter.InfoLogLength, out logLength);
            if (logLength > 0)
            {
                var log = new System.Text.StringBuilder(logLength);
                GL.GetProgramInfoLog(prog, logLength, out logLength, log);
                Console.WriteLine("Program validate log:\n{0}", log);
            }

            GL.GetProgram(prog, ProgramParameter.LinkStatus, out status);
            return status != 0;
        }
    }
}
