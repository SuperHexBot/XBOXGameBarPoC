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
                    System.Numerics.Vector2 CenterY1;
                    System.Numerics.Vector2 CenterY2;
                    System.Numerics.Vector2 CenterX1;
                    System.Numerics.Vector2 CenterX2;
                    CenterTop.X = 2560 / 2.0f;
                    CenterTop.Y = 0.0f;
                    CenterBottom.X = 2560 / 2.0f;
                    CenterBottom.Y = 1440.0f;

                    CenterY1.X = 2560 / 2.0f;
                    CenterY1.Y = 1440.0f / 2.0F - 10.0F;
                    CenterY2.X = 2560 / 2.0f;
                    CenterY2.Y = 1440.0f / 2.0F + 10.0F;
                    CenterX1.X = 2560 / 2.0f - 10.0F;
                    CenterX1.Y = 1440.0f / 2.0F;
                    CenterX2.X = 2560 / 2.0f + 10.0F;
                    CenterX2.Y = 1440.0f / 2.0F;
                    while (true)
                    {
                        using (CanvasDrawingSession ds = canvasSwapChainPanel.SwapChain.CreateDrawingSession(Colors.Transparent))
                        {
                            misc_xbox misc_xbox_inst;
                            viewAccessor.Read<misc_xbox>(0, out misc_xbox_inst);
                            if (misc_xbox_inst.esp_draw)
                            {
                                if (misc_xbox_inst.count != 0)
                                {
                                    Box[] boxArray = new Box[misc_xbox_inst.count];
                                    viewAccessor.ReadArray<Box>(24, boxArray, 0, misc_xbox_inst.count);
                                    for (int i = 0; i < boxArray.Length; i++)
                                    {
                                        Color boxcolor = Colors.White;
                                        Color shieldcolor = Colors.White;
                                        if (boxArray[i].BoxColor == 1) { boxcolor = Colors.Red; }
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
                                ds.DrawText(misc_xbox_inst.spectators + "-" + misc_xbox_inst.allied_spectators + "-" + misc_xbox_inst.bone + "-" + misc_xbox_inst.aim_no_recoil + "-" + misc_xbox_inst.aim_mov_x + "-" + misc_xbox_inst.aim_mov_y_down + "-" + misc_xbox_inst.aim_mov_y_up + "-" + misc_xbox_inst.auto_changge_aim, CenterTop, Colors.White);

                                Color FPSCOLOR = Colors.Red;
                                if (misc_xbox_inst.allied_spectators !=0 || misc_xbox_inst.spectators != 0)
                                {
                                    FPSCOLOR = Colors.Green;
                                }
                                ds.DrawLine(CenterY1, CenterY2, FPSCOLOR, 5);
                                ds.DrawLine(CenterX1, CenterX2, FPSCOLOR, 5);
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
            public int BoxColor;
            public int ShieldColor;
        }
        struct misc_xbox
        {
            public int count;
            public int spectators;
            public int allied_spectators;
            public int bone;
            public bool aim_mov_x;
            public bool aim_mov_y_down;
            public bool aim_mov_y_up;
            public bool auto_changge_aim;
            public bool aim_no_recoil;
            public bool esp_draw;
        }
    }
}
