using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KatanUP01.UI
{
    public partial class MainWindow : Window
    {
        private Entities _context;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            AddContextMenu();
        }

        private void LoadData()
        {
            try
            {
                _context = new Entities();

                // Загружаем заявки с связанными данными
                var requests = _context.Requests
                    .Include("Addresses")
                    .Include("Employees")
                    .ToList();

                dgRequests.ItemsSource = requests;
                tbRecordCount.Text = $"Записей: {requests.Count}";
                tbStatus.Text = $"Данные загружены ({DateTime.Now:HH:mm:ss})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                tbStatus.Text = "Ошибка загрузки данных";
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new EditRequestWindow();
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем данные после добавления
                    LoadData();
                    MessageBox.Show("Заявка успешно добавлена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem == null)
            {
                MessageBox.Show("Выберите заявку для редактирования",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedRequest = (Requests)dgRequests.SelectedItem;
                var editWindow = new EditRequestWindow(selectedRequest.RequestId);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем данные после редактирования
                    LoadData();
                    MessageBox.Show("Заявка успешно обновлена!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem == null)
            {
                MessageBox.Show("Выберите заявку для удаления",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную заявку?\n\n" +
                                        "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var request = (Requests)dgRequests.SelectedItem;

                    // Показываем подробности удаляемой заявки
                    string message = $"Удалить заявку №{request.RequestId}?\n" +
                                    $"Заявитель: {request.ApplicantName}\n" +
                                    $"Адрес: {request.Addresses?.FullAddress ?? "Не указан"}\n" +
                                    $"Описание: {request.ProblemDescription}";

                    var confirm = MessageBox.Show(message,
                        "Подтвердите удаление",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm == MessageBoxResult.Yes)
                    {
                        _context.Requests.Remove(request);
                        _context.SaveChanges();

                        LoadData(); // Перезагружаем данные
                        tbStatus.Text = $"Заявка №{request.RequestId} удалена";

                        MessageBox.Show("Заявка успешно удалена",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}\n\n" +
                                   "Возможно, заявка связана с другими данными.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var historyWindow = new RequestHistoryWindow();
                historyWindow.Owner = this;
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия истории: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // Двойной клик по строке для редактирования
        private void DgRequests_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        // Добавляем контекстное меню для DataGrid
        private void AddContextMenu()
        {
            var contextMenu = new ContextMenu();

            // Пункт "История по сотруднику"
            var menuItemHistoryByEmployee = new MenuItem
            {
                Header = "📋 История заявок сотрудника",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12,
                ToolTip = "Показать все заявки выбранного сотрудника"
            };
            menuItemHistoryByEmployee.Click += MenuItemHistoryByEmployee_Click;

            // Пункт "История по адресу"
            var menuItemHistoryByAddress = new MenuItem
            {
                Header = "🏠 История заявок по адресу",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12,
                ToolTip = "Показать все заявки по выбранному адресу"
            };
            menuItemHistoryByAddress.Click += MenuItemHistoryByAddress_Click;

            // Разделитель
            var separator = new Separator();

            // Пункт "Быстрое редактирование"
            var menuItemQuickEdit = new MenuItem
            {
                Header = "✏️ Быстрое редактирование",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12,
                ToolTip = "Быстро отредактировать выбранную заявку"
            };
            menuItemQuickEdit.Click += (s, args) => BtnEdit_Click(s, args);

            // Пункт "Быстрое удаление"
            var menuItemQuickDelete = new MenuItem
            {
                Header = "🗑️ Быстрое удаление",
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12,
                ToolTip = "Быстро удалить выбранную заявку"
            };
            menuItemQuickDelete.Click += (s, args) => BtnDelete_Click(s, args);

            // Добавляем пункты в меню
            contextMenu.Items.Add(menuItemHistoryByEmployee);
            contextMenu.Items.Add(menuItemHistoryByAddress);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(menuItemQuickEdit);
            contextMenu.Items.Add(menuItemQuickDelete);

            // Устанавливаем контекстное меню для DataGrid
            dgRequests.ContextMenu = contextMenu;
        }

        // Обработчик для истории по сотруднику
        private void MenuItemHistoryByEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem == null)
            {
                MessageBox.Show("Выберите заявку с назначенным сотрудником",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var request = (Requests)dgRequests.SelectedItem;
                if (request.EmployeeId.HasValue)
                {
                    var historyWindow = new RequestHistoryWindow(request.EmployeeId.Value);
                    historyWindow.Owner = this;
                    historyWindow.Title = $"История заявок сотрудника: {request.Employees?.LastName}";
                    historyWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("У выбранной заявки нет назначенного сотрудника",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик для истории по адресу
        private void MenuItemHistoryByAddress_Click(object sender, RoutedEventArgs e)
        {
            if (dgRequests.SelectedItem == null)
            {
                MessageBox.Show("Выберите заявку",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var request = (Requests)dgRequests.SelectedItem;
                var historyWindow = new RequestHistoryWindow(request.AddressId, true);
                historyWindow.Owner = this;
                historyWindow.Title = $"История заявок по адресу: {request.Addresses?.FullAddress}";
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка для быстрого просмотра истории по сотруднику (можно добавить на панель)
        private void BtnEmployeeHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHistoryByEmployee_Click(sender, e);
        }

        // Кнопка для быстрого просмотра истории по адресу (можно добавить на панель)
        private void BtnAddressHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHistoryByAddress_Click(sender, e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _context?.Dispose();
            base.OnClosing(e);
        }
    }
}