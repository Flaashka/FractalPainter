using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FractalPainting.App.Fractals;
using FractalPainting.Infrastructure.Common;
using FractalPainting.Infrastructure.UiActions;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Extensions.Conventions;
// ReSharper disable CommentTypo

namespace FractalPainting.App
{
    public static class DIContainerTask
    {
        public static MainForm CreateMainForm()
        {
            var container = ConfigureContainer();

            return container.Get<MainForm>();
        }

        public static StandardKernel ConfigureContainer()
        {
            var container = new StandardKernel();

            container.Bind<IUiAction>().To<KochFractalAction>();
            container.Bind<IUiAction>().To<DragonFractalAction>();

            container.Bind<IObjectSerializer>().To<XmlObjectSerializer>().WhenInjectedInto<SettingsManager>();
            container.Bind<IBlobStorage>().To<FileBlobStorage>().WhenInjectedInto<SettingsManager>();
            container.Bind<AppSettings>().ToMethod(context => context.Kernel.Get<SettingsManager>().Load()).InSingletonScope();

            container.Bind<IUiAction>().To<SaveImageAction>();

            container.Bind<IUiAction>().To<ImageSettingsAction>();
            container.Bind<ImageSettings>().ToMethod(context => context.Kernel.Get<AppSettings>().ImageSettings).InSingletonScope();

            container.Bind<IUiAction>().To<PaletteSettingsAction>();
            container.Bind<Palette>().To<Palette>().InSingletonScope();

            container.Bind<IImageHolder, PictureBoxImageHolder>().To<PictureBoxImageHolder>().InSingletonScope();
            container.Bind<IDragonPainterFactory>().ToFactory();

            return container;
        }
    }

    public interface IDragonPainterFactory
    {
        DragonPainter CreateDragonPainter(DragonSettings settings);
    }

    public class DragonFractalAction : IUiAction
    {
        private readonly IDragonPainterFactory _dragonPainterFactory;

        public DragonFractalAction(IDragonPainterFactory dragonPainterFactory)
        {
            _dragonPainterFactory = dragonPainterFactory;
        }

        public MenuCategory Category => MenuCategory.Fractals;
        public string Name => "????????????";
        public string Description => "???????????? ??????????????-??????????????";

        public void Perform()
        {
            var dragonSettings = CreateRandomSettings();
            // ?????????????????????? ??????????????????:
            SettingsForm.For(dragonSettings).ShowDialog();
            // ?????????????? painter ?? ???????????? ??????????????????????
            var dragonPainter = _dragonPainterFactory.CreateDragonPainter(dragonSettings);
            dragonPainter.Paint();
        }

        private static DragonSettings CreateRandomSettings()
        {
            return new DragonSettingsGenerator(new Random()).Generate();
        }
    }

    public class KochFractalAction : IUiAction
    {
        private readonly Lazy<KochPainter> _kochPainter;

        public KochFractalAction(Lazy<KochPainter> kochPainter)
        {
            _kochPainter = kochPainter;
        }

        public MenuCategory Category => MenuCategory.Fractals;
        public string Name => "???????????? ????????";
        public string Description => "???????????? ????????";

        public void Perform()
        {
            _kochPainter.Value.Paint();
        }
    }

    public class DragonPainter
    {
        private readonly IImageHolder imageHolder;
        private DragonSettings settings;
        private readonly float size;
        private Size imageSize;
        private readonly Palette _palette;

        public DragonPainter(IImageHolder imageHolder, DragonSettings settings, Palette palette)
        {
            this.imageHolder = imageHolder;
            this.settings = settings;
            imageSize = imageHolder.GetImageSize();
            size = Math.Min(imageSize.Width, imageSize.Height) / 2.1f;
            _palette = palette;
        }

        public void Paint()
        {
            using (var graphics = imageHolder.StartDrawing())
            using (var backgroundBrush = new SolidBrush(_palette.BackgroundColor))
            using (var primaryBrush = new SolidBrush(_palette.PrimaryColor))
            {
                graphics.FillRectangle(backgroundBrush, 0, 0, imageSize.Width, imageSize.Height);
                var r = new Random();
                var cosa = (float)Math.Cos(settings.Angle1);
                var sina = (float)Math.Sin(settings.Angle1);
                var cosb = (float)Math.Cos(settings.Angle2);
                var sinb = (float)Math.Sin(settings.Angle2);
                var shiftX = settings.ShiftX * size * 0.8f;
                var shiftY = settings.ShiftY * size * 0.8f;
                var scale = settings.Scale;
                var p = new PointF(0, 0);
                foreach (var i in Enumerable.Range(0, settings.IterationsCount))
                {
                    graphics.FillRectangle(primaryBrush, imageSize.Width / 3f + p.X, imageSize.Height / 2f + p.Y, 1, 1);
                    if (r.Next(0, 2) == 0)
                        p = new PointF(scale * (p.X * cosa - p.Y * sina), scale * (p.X * sina + p.Y * cosa));
                    else
                        p = new PointF(scale * (p.X * cosb - p.Y * sinb) + shiftX, scale * (p.X * sinb + p.Y * cosb) + shiftY);
                    if (i % 100 == 0) imageHolder.UpdateUi();
                }
            }
            imageHolder.UpdateUi();
        }

        private void SetSettings(DragonSettings settings)
        {
            this.settings = settings;
        }
    }
}
