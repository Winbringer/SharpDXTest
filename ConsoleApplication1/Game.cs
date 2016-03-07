
using SharpDX.D3DCompiler;
using System.Linq;

namespace ConsoleApplication1
{
    class Game : System.IDisposable
    {

        private SharpDX.Windows.RenderForm renderForm;
        private readonly int Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        private readonly int Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.Direct3D11.DeviceContext d3dDeviceContext;
        private SharpDX.DXGI.SwapChain swapChain;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;
        //Координаты для куба
        private SharpDX.Direct3D11.Buffer triangleVertexBuffer;
        private SharpDX.Vector3[] vertices = new SharpDX.Vector3[] { new SharpDX.Vector3(-0.5f, 0.5f, 0.0f),
            new SharpDX.Vector3(0.5f, 0.5f, 0.0f),
            new SharpDX.Vector3(0.0f, -0.5f, 0f) };
        //Индексы
        private int[] indexes = new int[] { 0,1,2 };
        private SharpDX.Direct3D11.Buffer idexBuffer;
        //Шейдеры
        private SharpDX.Direct3D11.VertexShader vertexShader;
        private SharpDX.Direct3D11.PixelShader pixelShader;
        private ShaderSignature inputSignature;
        private SharpDX.Direct3D11.InputLayout inputLayout;
        private SharpDX.Viewport viewport;
        private SharpDX.Direct3D11.InputElement[] inputElements = new SharpDX.Direct3D11.InputElement[]
                 {
                    new SharpDX.Direct3D11.InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)
                 };

        public Game()
        {
            renderForm = new SharpDX.Windows.RenderForm("My first SharpDX game");
            renderForm.ClientSize = new System.Drawing.Size(Width, Height);
            renderForm.IsFullscreen = true;
            renderForm.AllowUserResizing = false;
            InitializeDeviceResources();
            InitializeTriangle();
            InitializeShaders();
            renderForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            //var factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            //factory.MakeWindowAssociation(renderForm.Handle, SharpDX.DXGI.WindowAssociationFlags.IgnoreAll);
            //renderForm.Focus();
            //renderForm.Activate();
        }

        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new SharpDX.Direct3D11.VertexShader(d3dDevice, vertexShaderByteCode);
            }
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new SharpDX.Direct3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }
            // Задать для видеокарты шейдеры и тип примитива
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);
            d3dDeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            inputLayout = new SharpDX.Direct3D11.InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void InitializeTriangle()
        {
            triangleVertexBuffer = SharpDX.Direct3D11.Buffer.Create<SharpDX.Vector3>(d3dDevice, SharpDX.Direct3D11.BindFlags.VertexBuffer, vertices);
            idexBuffer = SharpDX.Direct3D11.Buffer.Create<int>(d3dDevice, SharpDX.Direct3D11.BindFlags.IndexBuffer, indexes);
        }

        private void InitializeDeviceResources()
        {
            SharpDX.DXGI.ModeDescription backBufferDesc = new SharpDX.DXGI.ModeDescription(Width, Height, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm);
            SharpDX.DXGI.SwapChainDescription swapChainDesc = new SharpDX.DXGI.SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };
            SharpDX.Direct3D11.Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;
            using (SharpDX.Direct3D11.Texture2D backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
            {
                renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            // Set viewport
            viewport = new SharpDX.Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);
        }

        public void Run()
        {
            SharpDX.Windows.RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        private void Draw()
        {
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));
            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new SharpDX.Direct3D11.VertexBufferBinding(triangleVertexBuffer, SharpDX.Utilities.SizeOf<SharpDX.Vector3>(), 0));
            d3dDeviceContext.InputAssembler.SetIndexBuffer(idexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            d3dDeviceContext.DrawIndexed(indexes.Count(),0,0);
            swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                    inputLayout.Dispose();
                    inputSignature.Dispose();
                    triangleVertexBuffer.Dispose();
                    vertexShader.Dispose();
                    pixelShader.Dispose();
                    renderTargetView.Dispose();
                    swapChain.Dispose();
                    d3dDevice.Dispose();
                    d3dDeviceContext.Dispose();
                    renderForm.Dispose();
                    triangleVertexBuffer.Dispose();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.
                disposedValue = true;
            }
        }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
