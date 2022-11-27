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
                    System.Numerics.Vector2 CenterBottom;
                    CenterTop.X = 2560 / 2.0f;
                    CenterTop.Y = 0.0f;
                    CenterBottom.X = 2560 / 2.0f;
                    CenterBottom.Y = 1440.0f;
                    while (true)
                    {
                        using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent))
                        {
                            int count = 0;
                            int spectators = 0;
                            int allied_spectators = 0;
                            viewAccessor.Read<int>(0, out count);
                            viewAccessor.Read<int>(4, out spectators);
                            viewAccessor.Read<int>(8, out allied_spectators);
                            if (count != 0)
                            {
                                Box[] boxArray = new Box[count];
                                viewAccessor.ReadArray<Box>(12, boxArray, 0, count);
                                for (int i = 0; i < boxArray.Length; i++)
                                {
                                    Color boxcolor = Colors.White;
                                    Color shieldcolor = Colors.White;
                                    if (boxArray[i].BoxColor == 1){ boxcolor = Colors.Red;}
                                    else if (boxArray[i].BoxColor == 2) { boxcolor = Colors.Yellow; }
                                    else if (boxArray[i].BoxColor == 3) { boxcolor = Colors.Black; }
                                    if (boxArray[i].ShieldColor == 1) { shieldcolor = Colors.Blue; }
                                    else if (boxArray[i].ShieldColor == 2) { shieldcolor = Colors.Purple; }
                                    else if (boxArray[i].ShieldColor == 3) { shieldcolor = Colors.Red; }

                                    ds.DrawRectangle(boxArray[i].X, boxArray[i].Y - boxArray[i].Height, boxArray[i].Width, boxArray[i].Height, boxcolor, 1);
                                    ds.DrawLine(boxArray[i].X - 5, boxArray[i].Y, boxArray[i].X - 5, boxArray[i].Y - boxArray[i].HpHeight, Colors.Red, 5);
                                    ds.DrawLine(boxArray[i].X + boxArray[i].Width / 2 + 5, boxArray[i].Y, boxArray[i].X + boxArray[i].Width / 2 + 5, boxArray[i].Y - boxArray[i].ShieldHeight, shieldcolor, 5);
                                    System.Numerics.Vector2 BoxTop;
                                    BoxTop.X = boxArray[i].X + boxArray[i].Width / 2.0f;
                                    BoxTop.Y = boxArray[i].Y;
                                    ds.DrawLine(CenterBottom, BoxTop, boxcolor, 1);
                                }
                            }
                            ds.DrawText("s:" + spectators + "-" + allied_spectators, CenterTop, Colors.White);
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
            public int BoxColor;
            public int ShieldColor;
        }
    }
}
