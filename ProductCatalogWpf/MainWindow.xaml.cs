using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProductCatalogWpf
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _dbService.InitializeDatabase();
                LoadProducts();
            }
            catch (Exception ex)
            {
                ShowStatus($"Грешка при инициализация на базата: {ex.Message}", isError: true);
            }
        }

        private void LoadProducts(string term = "")
        {
            try
            {
                string normalizedTerm = term.Trim();
                var products = _dbService.GetProducts(normalizedTerm);
                gridProducts.ItemsSource = products;

                if (!string.IsNullOrEmpty(normalizedTerm) && products.Count == 0)
                {
                    ShowStatus($"Няма намерени продукти за „{normalizedTerm}“.", isError: true);
                }
                else
                {
                    ShowStatus($"Заредени записи: {products.Count}.", isError: false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Грешка при зареждане: {ex.Message}", isError: true);
            }
        }

        private void AddProduct()
        {
            if (!ValidateInputs(out string name, out decimal price))
                return;

            try
            {
                _dbService.InsertProduct(name, price);
                ClearInputs();
                LoadProducts();
                ShowStatus($"Успешно добавен продукт: „{name}“ ({price:F2} лв.).", isError: false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Грешка при добавяне: {ex.Message}", isError: true);
            }
        }

        private void UpdateProduct()
        {
            if (gridProducts.SelectedItem is not Product selectedProduct)
            {
                ShowStatus("Моля, изберете продукт от таблицата за редактиране.", isError: true);
                MessageBox.Show("Моля, изберете ред от таблицата за редактиране.", "Липсващ избор", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInputs(out string name, out decimal price))
                return;

            try
            {
                _dbService.UpdateProduct(selectedProduct.Id, name, price);
                ClearInputs();
                LoadProducts(txtSearch.Text);
                ShowStatus($"Продукт с ID {selectedProduct.Id} беше обновен успешно.", isError: false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Грешка при обновяване: {ex.Message}", isError: true);
            }
        }

        private void DeleteProduct()
        {
            if (gridProducts.SelectedItem is not Product selectedProduct)
            {
                ShowStatus("Моля, изберете продукт от таблицата за изтриване.", isError: true);
                MessageBox.Show("Моля, изберете ред от таблицата за изтриване.", "Липсващ избор", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Сигурни ли сте, че искате да изтриете „{selectedProduct.Name}“?",
                "Потвърждение за изтриване",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                _dbService.DeleteProduct(selectedProduct.Id);
                ClearInputs();
                LoadProducts(txtSearch.Text);
                ShowStatus($"Продукт „{selectedProduct.Name}“ беше изтрит.", isError: false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Грешка при изтриване: {ex.Message}", isError: true);
            }
        }

        private bool ValidateInputs(out string name, out decimal price)
        {
            name = txtName.Text.Trim();
            price = 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowStatus("Грешка: Името на продукта не може да бъде празно.", isError: true);
                MessageBox.Show("Въведете име на продукта.", "Невалидни данни", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            // Поддържа въвеждане както с точка (49.90), така и със запетая (49,90)
            string normalizedPriceText = txtPrice.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(normalizedPriceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price) || price <= 0)
            {
                ShowStatus("Грешка: Въведете валидна положителна цена (напр. 49.90).", isError: true);
                MessageBox.Show("Въведете валидна положителна цена.", "Невалидна цена", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtPrice.Clear();
            gridProducts.SelectedItem = null;
        }

        private void ShowStatus(string message, bool isError)
        {
            txtStatus.Text = message;
            txtStatus.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
                : new SolidColorBrush(Color.FromRgb(46, 125, 50));
        }

        // Обработчици на събития от интерфейса
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadProducts();
        }

        private void gridProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridProducts.SelectedItem is Product selectedProduct)
            {
                txtName.Text = selectedProduct.Name;
                txtPrice.Text = selectedProduct.Price.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddProduct();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateProduct();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteProduct();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
            ShowStatus("Полетата са изчистени.", isError: false);
        }
    }
}