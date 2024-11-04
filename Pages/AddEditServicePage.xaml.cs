using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Media;
using System;

namespace CarService.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditServicePage.xaml
    /// </summary>
    public partial class AddEditServicePage : Page
    {
        readonly private Entities.Service _currentService = null;
        private byte[] _mainImageData;
        public AddEditServicePage()
        {
            InitializeComponent();
        }
        public AddEditServicePage(Entities.Service service)
        {
            InitializeComponent();

            _currentService = service;
            Title = "Редактировать услугу";
            TBoxTitle.Text = _currentService.Title;
            TBoxCost.Text = _currentService.Cost.ToString("N2");
            TBoxDuration.Text = (_currentService.DurationInSeconds / 60).ToString();
            TBoxDescription.Text = _currentService.Description;
            if (_currentService.Discount > 0)
                TBoxDiscount.Text = (_currentService.Discount.Value * 100).ToString();
            if (_currentService.MainImage != null)
                try
                {
                    ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_currentService.MainImage);
                }
                catch {
                    // MessageBox.Show("Изображение не установлено!");
                    // В случае если картинки не окажется, чтобы программа не крашнулась
                }
        }
        public string CheckErrors()
        {
            var errorBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBoxTitle.Text))
                errorBuilder.AppendLine("Название услуги обязательно для заполнения");

            var serviceFromDB = App.Context.Services.ToList().FirstOrDefault(p => p.Title.ToLower() == TBoxTitle.Text.ToLower());

            if (serviceFromDB != null && serviceFromDB != _currentService) 
                errorBuilder.AppendLine("Такая услуга уже есть в базе данных!");

            if (decimal.TryParse(TBoxCost.Text, out decimal cost) == false || cost <= 0)
                errorBuilder.AppendLine("Стоимость услуги должна быть положительным числом!");

            if (int.TryParse(TBoxDuration.Text, out int durationInMinutes) == false || durationInMinutes <= 0 || durationInMinutes >= 240)
                errorBuilder.AppendLine("Длительность оказания услуги должна быть положительным " +     
                                        "числом (не больше, 4 часа)!");

            if (!string.IsNullOrEmpty(TBoxDiscount.Text)) 
            {
                if (double.TryParse(TBoxDiscount.Text, out double discount) == false || discount < 0 || discount > 100) 
                {
                    errorBuilder.AppendLine("Размер скдики - десятичное число в диапазоне от 0 до 100");
                }
            }

            if (errorBuilder.Length > 0) errorBuilder.Insert(0, "Устраните следующие ошибки: \n");
            return errorBuilder.ToString();
        } 
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();  
           // ofd.Filter = "Image |*.png, *.jpg, *.jpeg";
            if (ofd.ShowDialog() == true) 
            { 
                _mainImageData = File.ReadAllBytes(ofd.FileName);
                ImageService.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(_mainImageData);
            }
        }

        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            var errorMessage = CheckErrors();

            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
            else 
            {
                if (_currentService == null)
                {
                    var service = new Entities.Service
                    {
                        Title = TBoxTitle.Text,
                        Cost = decimal.Parse(TBoxCost.Text),
                        DurationInSeconds = int.Parse(TBoxDuration.Text) * 60,
                        Description = TBoxDescription.Text,
                        Discount = string.IsNullOrWhiteSpace(TBoxDiscount.Text) ? 0 : double.Parse(TBoxDiscount.Text) / 100,
                        MainImage = _mainImageData
                    };
                    App.Context.Services.Add(service);
                    App.Context.SaveChanges();
                }
                else {
                    _currentService.Title = TBoxTitle.Text;
                    _currentService.Cost = decimal.Parse(TBoxCost.Text);
                    _currentService.DurationInSeconds = int.Parse(TBoxDuration.Text) * 60;
                    _currentService.Description = TBoxDescription.Text;
                    _currentService.Discount = string.IsNullOrWhiteSpace(TBoxDiscount.Text) ? 0 : double.Parse(TBoxDiscount.Text) / 100;
                    if (_mainImageData != null) _currentService.MainImage = _mainImageData;
                    App.Context.SaveChanges();
                }
                NavigationService.GoBack();
            }
        }
    }
}
