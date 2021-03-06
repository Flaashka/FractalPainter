//Вам необходимо отрефакторить все классы, унаследованные от IUiAction, 
//так чтобы в них перестал использоваться класс Services(находящийся в файле DIContainerTask.cs), 
//а все необходимые для работы зависимости принимались через единственный конструктор и хранились 
//в private-полях.Обращения к классу Services должны переместится в 
//конструктор без параметров класса MainForm.

using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using FractalPainting.Infrastructure.Common;
using FractalPainting.Infrastructure.UiActions;

namespace FractalPainting.App
{
    public class ImageSettingsAction : IUiAction
    {
        private readonly ImageSettings _imageSettings;
        private readonly IImageHolder _pictureBoxImageHolder;

        public ImageSettingsAction(ImageSettings imageSettings, IImageHolder pictureBoxImageHolder)
        {
            _imageSettings = imageSettings;
            _pictureBoxImageHolder = pictureBoxImageHolder;
        }

        public MenuCategory Category => MenuCategory.Settings;
        public string Name => "Изображение...";
        public string Description => "Размеры изображения";

        public void Perform()
        {
            SettingsForm.For(_imageSettings).ShowDialog();
            _pictureBoxImageHolder.RecreateImage(_imageSettings);
        }
    }

    public class SaveImageAction : IUiAction
    {
        private readonly AppSettings _appSettings;
        private readonly IImageHolder _imageHolder;

        public SaveImageAction(AppSettings appSettings, IImageHolder imageHolder)
        {
            _appSettings = appSettings;
            _imageHolder = imageHolder;
        }

        public MenuCategory Category => MenuCategory.File;
        public string Name => "Сохранить...";
        public string Description => "Сохранить изображение в файл";

        public void Perform()
        {
            var dialog = new SaveFileDialog
            {
                CheckFileExists = false,
                InitialDirectory = Path.GetFullPath(_appSettings.ImagesDirectory),
                DefaultExt = "bmp",
                FileName = "image.bmp",
                Filter = "Изображения (*.bmp)|*.bmp"
            };
            var res = dialog.ShowDialog();
            if (res == DialogResult.OK)
                _imageHolder.SaveImage(dialog.FileName);
        }
    }

    public class PaletteSettingsAction : IUiAction
    {
        private readonly Palette _palette;

        public PaletteSettingsAction(Palette palette)
        {
            _palette = palette;
        }

        public MenuCategory Category => MenuCategory.Settings;
        public string Name => "Палитра...";
        public string Description => "Цвета для рисования фракталов";

        public void Perform()
        {
            SettingsForm.For(_palette).ShowDialog();
        }
    }

    public class MainForm : Form
    {
        public MainForm(IUiAction[] actions, PictureBoxImageHolder pictureBox)
        {
            var imageSettings = CreateSettingsManager().Load().ImageSettings;
            ClientSize = new Size(imageSettings.Width, imageSettings.Height);

            pictureBox.RecreateImage(imageSettings);
            pictureBox.Dock = DockStyle.Fill;
            Controls.Add(pictureBox);

            var mainMenu = new MenuStrip();
            mainMenu.Items.AddRange(actions.ToMenuItems());
            mainMenu.Dock = DockStyle.Top;
            Controls.Add(mainMenu);
        }

        private static SettingsManager CreateSettingsManager()
        {
            return new SettingsManager(new XmlObjectSerializer(), new FileBlobStorage());
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Text = "Fractal Painter";
        }
    }
}
