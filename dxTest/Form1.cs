using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;

using SharpDX.Windows;

namespace dxTest
{
    public partial class Form1 : Form
    {
        private Control renderControl;

        // directx objects
        private WindowRenderTarget target;
        private SharpDX.Direct2D1.Factory factory;

        // data update metronome
        private System.Windows.Forms.Timer timer;

        // visual tools
        LinearGradientBrush bandBrush;
        
        private Random random;
        private int cycleCount = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void onLoad(object sender, EventArgs e)
        {
            // create directx target
            createDxTarget();

            // initialise timer
            timer = new Timer();
            timer.Interval = 17;
            timer.Tick += tickHandler;

            random = new Random();

            // start visual tick loop
            timer.Start();
        }

        private void tickHandler(object sender, EventArgs e)
        {
            draw();
        }

        private void draw()
        {
            target.BeginDraw();

            if (cycleCount > 0xff)
            {
                cycleCount = 0;
            }

            int phase = 0;
            int center = 100; // 200;// 128;
            int width = 55;// 127;
            double frequency = Math.PI * 2 / 255;

            SharpDX.Color color = new SharpDX.Color(
                (int)Math.Floor(Math.Sin(frequency * cycleCount + 0 + phase) * width + center),
                (int)Math.Floor(Math.Sin(frequency * cycleCount + 2 + phase) * width + center),
                (int)Math.Floor(Math.Sin(frequency * cycleCount + 4 + phase) * width + center),
                0xff
            );

            target.Clear(color);

            cycleCount++;
            
            

            target.Clear(color);

            int bandPercentage;

            int bandCount = 128;
            int gap = 1;
            float bandWidth = (target.Size.Width + gap) / bandCount;

            for (int i = 0; i < bandCount; i++)
            {
                bandPercentage = random.Next(0, 100);

                target.FillRectangle(new SharpDX.RectangleF()
                {
                    Top = target.Size.Height * bandPercentage / 100,
                    Left = i * bandWidth,
                    Width = bandWidth - gap,
                    Bottom = target.Size.Height
                }, bandBrush);
            }

            target.EndDraw();
        }

        private void createDxTarget()
        {
            renderControl = new Control()
            {
                Width = this.ClientSize.Width,
                Height = this.ClientSize.Height,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            renderControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(onDoubleClick);

            this.Controls.Add(renderControl);

            factory = new SharpDX.Direct2D1.Factory();

            RenderTargetProperties targetProperties = new RenderTargetProperties()
            {
                DpiX = 0,
                DpiY = 0,
                MinLevel = FeatureLevel.Level_10,
                PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                Type = RenderTargetType.Default,
                Usage = RenderTargetUsage.None
            };

            HwndRenderTargetProperties windowProperties = new HwndRenderTargetProperties()
            {
                Hwnd = renderControl.Handle,
                PixelSize = new SharpDX.Size2(renderControl.ClientSize.Width, renderControl.ClientSize.Height),
                PresentOptions = PresentOptions.None
            };

            target = new WindowRenderTarget(factory, targetProperties, windowProperties);

            /*
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint
            , true);
            */
            initialiseTarget();
        }

        private void initialiseTarget()
        {
            target.Resize(new SharpDX.Size2(renderControl.Width, renderControl.Height));
            createBandBrush();
        }

        private void createBandBrush()
        {
            LinearGradientBrushProperties properties = new LinearGradientBrushProperties()
            {
                StartPoint = new Vector2(0, target.Size.Height),
                EndPoint = new Vector2(0, 0)
            };

            var points = new GradientStopCollection( target, new GradientStop[] {
                new GradientStop() {Color=SharpDX.Color.Green, Position=0F},
                new GradientStop() {Color=SharpDX.Color.Yellow, Position=0.8F},
                new GradientStop() {Color=SharpDX.Color.Red, Position=1F}
            }, ExtendMode.Clamp);

            bandBrush = new LinearGradientBrush(target, properties, points);
        }

        private void onResize(object sender, EventArgs e)
        {/*
            using (renderControl) {
                Width = this.ClientSize.Width;
                Height = this.ClientSize.Height;
            }*/
            initialiseTarget();
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();

            bandBrush.Dispose();

            target.Dispose();
            factory.Dispose();

            base.Dispose();
        }

        private void onDoubleClick(object sender, MouseEventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
            }

            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }
    }
}
