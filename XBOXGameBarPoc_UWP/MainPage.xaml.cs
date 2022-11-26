using System.IO.MemoryMappedFiles;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace XBOXGameBarPoC_UWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void canvasSwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            canvasSwapChainPanel.SwapChain = new CanvasSwapChain(device, (float)Window.Current.CoreWindow.Bounds.Width, (float)Window.Current.CoreWindow.Bounds.Height, 96);

            Thread renderThread = new Thread(new ParameterizedThreadStart(RenderThread));
            renderThread.Start(canvasSwapChainPanel.SwapChain);
        }

        private void RenderThread(object parameter)
        {
            CanvasSwapChain swapChain = (CanvasSwapChain)parameter;

            using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("XboxGameBarPoc_SharedMemory", 0x644, MemoryMappedFileAccess.ReadWriteExecute))
            {
                using (MemoryMappedViewAccessor viewAccessor = memoryMappedFile.CreateViewAccessor())
                {
                    System.Numerics.Vector2 CenterTop;
                    CenterTop.X = 2560 / 2.0f;
                    CenterTop.Y = 0.0f;
                    while (true)
                    {
                        using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent))
                        {
                            int count = 0;
                            int spectators = 0;
                            int allied_spectators = 0;
                            viewAccessor.Read<int>(0, out count);
                            if (count != 0)
                            {
                                Box[] boxArray = new Box[count];
                                viewAccessor.ReadArray<Box>(4, boxArray, 0, count);
                                for (int i = 0; i < boxArray.Length; i++)
                                {
                                    ds.DrawRectangle(boxArray[i].X, boxArray[i].Y, boxArray[i].Width, boxArray[i].Height, boxArray[i].BoxColor, 5);
                                    System.Numerics.Vector2 BoxTop;
                                    BoxTop.X = boxArray[i].X + boxArray[i].Width / 2.0f;
                                    BoxTop.Y = boxArray[i].Y;
                                    ds.DrawLine(CenterTop, BoxTop, boxArray[i].LineColor, 5);
                                    spectators = boxArray[i].spectators;
                                    allied_spectators = boxArray[i].allied_spectators;
                                }
                                ds.DrawText("s:" + spectators + "-" + allied_spectators, CenterTop, Colors.White);
                            }
                            canvasSwapChainPanel.SwapChain.Present();
                        }
                    }
                }
            }
        }
        struct Box
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
            public float HpHeight;
            public float ShieldHeight;
            public Color BoxColor;
            public Color LineColor;
            public Color ShieldColor;
            public int spectators;
            public int allied_spectators;
        }
    }
}
