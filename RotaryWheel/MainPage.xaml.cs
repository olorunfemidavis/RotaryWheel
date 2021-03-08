using RotaryWheel.Helpers;
using RotaryWheel.Models;
using RotaryWheel.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RotaryWheel
{
    public partial class MainPage : ContentPage
    {
        protected MainPageVM viewModel;
        Stopwatch stopwatch = new Stopwatch();

        bool _pageIsActive;
        float _degrees;

        private readonly Random random = new Random();

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainPageVM();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.DataReady += ViewModel_DataReady;
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _pageIsActive = false;
            viewModel.DataReady -= ViewModel_DataReady;
        }
        private void ViewModel_DataReady(object sender, EventArgs e)
        {
            skiaView.InvalidateSurface();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            
        }
        /// <summary>
        /// This manages the animation effect
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        async Task AnimationLoop(int extension = 0)
        {
            if (viewModel.IsSpinning)
                return;
            viewModel.IsSpinning = true;
            priceBox.Text = string.Empty;
            stopwatch.Reset();
            stopwatch.Start();

            double nextDuration = (random.NextDouble() * 10) + 10; //adjust to your taste.
            if (extension != 0)
                nextDuration = extension;
            else
                viewModel.RefreshRate = Constants.DefaultRefreshRate;
            while (_pageIsActive && stopwatch.Elapsed < TimeSpan.FromSeconds(nextDuration))
            {
                skiaView.InvalidateSurface();
                await Task.Delay(TimeSpan.FromMilliseconds(viewModel.RefreshRate)); //fastest is 1 millisecond. 

                //slowdown
                if (stopwatch.Elapsed.TotalSeconds > nextDuration * 3 / 4)
                {
                    viewModel.EnableHaptic = true;
                    viewModel.RefreshRate += viewModel.RefreshRate / 20;
                    if (viewModel.RefreshRate > Constants.DefaultRefreshRate * 25)
                        viewModel.RefreshRate = Constants.DefaultRefreshRate;
                }

            }
            stopwatch.Stop();

            //check if the current angle is okay. 
            int rounder = (int)Math.Round(_degrees + 3.6f, MidpointRounding.AwayFromZero);
            if (viewModel.InvalidPoints.Contains(rounder))
            {
                //spin again for a moment. 
                await Task.Delay(100);
                viewModel.IsSpinning = false;
                AnimationLoop(5);
            }
            else
            {
                GetWinner();
                viewModel.IsSpinning = false;
                viewModel.EnableHaptic = false;
            }
        }
        private void GetWinner()
        {
            //show some pop-up or something. 
            priceBox.Text = viewModel.Prize.Price.ToString();
        }
        /// <summary>
        /// This is triggered whenever the canvas needs to redraw. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            if (viewModel.ChartData == null)
                return;
            if (viewModel.ChartData.Count == 0)
                return;

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            var y = info.Height / 2;

            canvas.Clear();

            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);
            float radius = Math.Min(info.Width / 2, info.Height / 2) - 2 * Constants.ExplodeOffset;
            SKRect rect = new SKRect(center.X - radius, center.Y - radius,
                                     center.X + radius, center.Y + radius);

            float startAngle = -90; //This is alighed to the marker to make tracking the winning prize easier. 


            //for text
            float xCenter = info.Width / 2;
            float yCenter = info.Height / 2;

            foreach (ChartData item in viewModel.ChartData)
            {
                float sweepAngle = 360f / viewModel.ChartData.Count;

                using (SKPath path = new SKPath())
                using (SKPaint fillPaint = new SKPaint())
                using (SKPaint outlinePaint = new SKPaint())
                using (SKPaint textPaint = new SKPaint())
                {
                    path.MoveTo(center);
                    path.ArcTo(rect, startAngle, sweepAngle, false);

                   
                    path.Close();

                    fillPaint.Style = SKPaintStyle.Fill;
                    fillPaint.Color = item.Color;

                    outlinePaint.Style = SKPaintStyle.Stroke;
                    outlinePaint.StrokeWidth = 5;
                    outlinePaint.Color = SKColors.White;

                    #region Text Writer
                    //write text to the screen
                    textPaint.TextSize = 40;
                    textPaint.StrokeWidth = 1;
                    textPaint.Color = SKColors.White;

                    //Adjust text size.
                    SKRect textBounds = new SKRect();
                    textPaint.MeasureText(item.Text, ref textBounds);
                    float yText = yCenter - textBounds.Height / 2 - textBounds.Top;

                    // Adjust TextSize property so text is 95% of the ARC
                    // float textWidth = textPaint.MeasureText(item.Text);
                    // textPaint.TextSize = 0.95f * info.Width * textPaint.TextSize / textWidth;

                    #endregion 

                    canvas.Save();

                    DrawRotatedWithMatrices(canvas, path, fillPaint, outlinePaint, item, _degrees, (int)center.X, y);

                    //Writing Actual texts
                    var test_angle = _degrees + (360 / viewModel.ChartData.Count / 2) - (360 / viewModel.ChartData.Count * 2);

                    float sweepAngleText = 360f / viewModel.ChartData.Count;
                    float startAngleText = sweepAngleText - sweepAngleText / 2;
                    foreach (ChartData itemer in viewModel.ChartData)
                    {
                        canvas.Save();
                        canvas.RotateDegrees(startAngleText + _degrees - 90, xCenter, yCenter);

                        if (itemer.Text.Trim().Length > 6)
                            textPaint.TextSize = 30;
                        else
                            textPaint.TextSize = 40;
                        canvas.DrawText(itemer.Text, xCenter, yText, textPaint);
                        canvas.Restore();
                        test_angle += 360 / viewModel.ChartData.Count;

                        if (test_angle > 360)
                            test_angle = test_angle - 360;

                        if (startAngleText > 360)
                            startAngleText = startAngleText - 360;
                        startAngleText += sweepAngleText;
                    }
                    canvas.Restore();
                }

                startAngle += sweepAngle;
            }

            #region Marker
            //draw the Mark
            using (SKPaint fillMarkCirclePaint = new SKPaint())
            using (SKPaint fillMarkCirclePaintOuter = new SKPaint())
            using (SKPaint fillMarkTrianglePaint = new SKPaint())
            {
                fillMarkCirclePaint.Style = SKPaintStyle.StrokeAndFill;
                fillMarkCirclePaintOuter.Style = SKPaintStyle.StrokeAndFill;
                fillMarkCirclePaintOuter.Color = Color.FromHex("#FFF180").ToSKColor();

                // Define an array of rainbow colors
                List<SKColor> colors = new List<SKColor>();

                foreach (var col in viewModel.Colors)
                {
                    colors.Add(Color.FromHex(col).ToSKColor());
                }

                //draw outer circle
                canvas.DrawCircle(args.Info.Width / 2, args.Info.Height / 2, 60, fillMarkCirclePaintOuter); //outer

                //draw triangle
                fillMarkTrianglePaint.Style = SKPaintStyle.StrokeAndFill;
                fillMarkTrianglePaint.Color = Color.FromHex("#FFF180").ToSKColor();
                SKPath trianglePath = new SKPath();
                trianglePath.MoveTo((args.Info.Width / 2) - 55, args.Info.Height / 2);
                trianglePath.LineTo((args.Info.Width / 2) - 55, args.Info.Height / 2);
                trianglePath.LineTo((args.Info.Width / 2) + 55, args.Info.Height / 2);
                trianglePath.LineTo(args.Info.Width / 2, (float)(args.Info.Height / 2.5));
                trianglePath.Close();
                canvas.DrawPath(trianglePath, fillMarkTrianglePaint);


                //draw inner circle
                SKPoint circle_center = new SKPoint(info.Rect.MidX, info.Rect.MidY);
                fillMarkCirclePaint.Shader = SKShader.CreateSweepGradient(circle_center, colors.ToArray());
                canvas.DrawCircle(args.Info.Width / 2, args.Info.Height / 2, 50, fillMarkCirclePaint); //inner   


            }
            #endregion

          
            //Get the current prize.
            float prize_degree = _degrees + (360 / viewModel.ChartData.Count / 2);

            if (_degrees == 0 || Math.Round(_degrees, MidpointRounding.AwayFromZero) == 360)
                prize_degree = _degrees;

            angleBox.Text = $"{_degrees}°";

            var segment = ((prize_degree / 360f) * viewModel.ChartData.Count);
            var int_segment2 = Math.Round(segment, MidpointRounding.AwayFromZero);
            var realIndex = viewModel.ChartData.Count == viewModel.ChartData.Count - (int)int_segment2 ? 0 : viewModel.ChartData.Count - (int)int_segment2;
            viewModel.Prize = viewModel.ChartData[realIndex].Sector; //add back
            priceBox.Text = viewModel.Prize?.Price.ToString();
            // currentBox.Text = ((int)realIndex).ToString();
            if (viewModel.EnableHaptic)
            {
                TryHaptic();
            }
            IncrementDegrees();

        }

        private void TryHaptic()
        {
            int rounder = (int)Math.Round(_degrees, MidpointRounding.AwayFromZero);
            if (viewModel.InvalidPoints.Contains(rounder))
            {
                //simulate click 
                General.DoHaptic(HapticFeedbackType.Click);
            }
        }

        private void IncrementDegrees()
        {
            if (_degrees >= 360)
            {
                // _degrees = 0;
                _degrees = _degrees - 360;
            }
            _degrees += 3.6f;
        }
        void DrawRotatedWithMatrices(SKCanvas canvas, SKPath path, SKPaint fill, SKPaint outline, ChartData item, float degrees, int cx, int cy)
        {
            var identity = SKMatrix.CreateIdentity();
            var translate = SKMatrix.CreateTranslation(-cx, -cy);
            var rotate = SKMatrix.CreateRotationDegrees(degrees);

            //angleBox.Text = degrees.ToString();
            var translate2 = SKMatrix.CreateTranslation(cx, cy);

            SKMatrix.PostConcat(ref identity, translate);
            SKMatrix.PostConcat(ref identity, rotate);
            SKMatrix.PostConcat(ref identity, translate2);


            path.Transform(identity);
            canvas.DrawPath(path, fill);
            canvas.DrawPath(path, outline);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            General.DoHaptic(HapticFeedbackType.LongPress);
            _pageIsActive = true;
            AnimationLoop();
        }

    }
}
