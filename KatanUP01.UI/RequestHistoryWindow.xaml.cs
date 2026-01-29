using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace KatanUP01.UI
{
    public partial class RequestHistoryWindow : Window
    {
        private Entities _context;
        private RequestHistoryViewModel _viewModel;

        public RequestHistoryWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        // Конструктор с фильтром по сотруднику
        public RequestHistoryWindow(int employeeId) : this()
        {
            _viewModel.SelectedEmployeeId = employeeId;
            LoadHistory();
        }

        // Конструктор с фильтром по адресу
        public RequestHistoryWindow(int addressId, bool isAddress) : this()
        {
            _viewModel.SelectedAddressId = addressId;
            LoadHistory();
        }

        private void InitializeData()
        {
            try
            {
                _context = new Entities();
                _viewModel = new RequestHistoryViewModel();
                DataContext = _viewModel;

                // Загружаем списки для фильтров
                LoadFilterData();
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadFilterData()
        {
            try
            {
                // Загружаем сотрудников
                var employees = _context.Employees.ToList();
                _viewModel.Employees = employees.Select(e => new EmployeeItem
                {
                    EmployeeId = e.EmployeeId,
                    FullName = $"{e.LastName} {e.FirstName} {e.MiddleName ?? ""}".Trim()
                }).ToList();

                // Добавляем "Все сотрудники"
                _viewModel.Employees.Insert(0, new EmployeeItem
                {
                    EmployeeId = null,
                    FullName = "Все сотрудники"
                });

                // Загружаем адреса
                var addresses = _context.Addresses.ToList();
                _viewModel.Addresses = addresses.Select(a => new AddressItem
                {
                    AddressId = a.AddressId,
                    FullAddress = a.FullAddress
                }).ToList();

                // Добавляем "Все адреса"
                _viewModel.Addresses.Insert(0, new AddressItem
                {
                    AddressId = null,
                    FullAddress = "Все адреса"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHistory()
        {
            try
            {
                // Используем LINQ запрос
                var history = GetRequestHistory();

                dgHistory.ItemsSource = history;
                tbRecordCount.Text = $"Записей: {history.Count}";
                tbStatus.Text = $"Данные загружены ({DateTime.Now:HH:mm:ss})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                tbStatus.Text = "Ошибка загрузки";
            }
        }

        private List<RequestHistoryItem> GetRequestHistory()
        {
            var query = _context.Requests.AsQueryable();

            // Применяем фильтры
            if (_viewModel.SelectedEmployeeId.HasValue)
            {
                query = query.Where(r => r.EmployeeId == _viewModel.SelectedEmployeeId.Value);
            }

            if (_viewModel.SelectedAddressId.HasValue)
            {
                query = query.Where(r => r.AddressId == _viewModel.SelectedAddressId.Value);
            }

            // Загружаем данные и преобразуем
            var result = query.ToList()  // Сначала материализуем запрос
                .Select(r => {
                    // Вручную загружаем связанные данные
                    var address = _context.Addresses.Find(r.AddressId);
                    var employee = r.EmployeeId.HasValue
                        ? _context.Employees.Find(r.EmployeeId.Value)
                        : null;

                    return new RequestHistoryItem
                    {
                        RequestId = r.RequestId,
                        CreatedDate = r.CreatedDate,
                        Address = address?.FullAddress ?? "Не указан",
                        ApplicantName = r.ApplicantName,
                        ProblemDescription = r.ProblemDescription,
                        EmployeeFullName = employee != null
                            ? $"{employee.LastName} {employee.FirstName}"
                            : "Не назначен",
                        Status = r.Status,
                        CompletionDate = r.CompletionDate
                    };
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return result;
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _context?.Dispose();
            base.OnClosing(e);
        }
    }

    // ViewModel для формы истории
    public class RequestHistoryViewModel
    {
        public List<EmployeeItem> Employees { get; set; }
        public List<AddressItem> Addresses { get; set; }
        public int? SelectedEmployeeId { get; set; }
        public int? SelectedAddressId { get; set; }
    }

    // Модель для отображения истории
    public class RequestHistoryItem
    {
        public int RequestId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Address { get; set; }
        public string ApplicantName { get; set; }
        public string ProblemDescription { get; set; }
        public string EmployeeFullName { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class EmployeeItem
    {
        public int? EmployeeId { get; set; }
        public string FullName { get; set; }
    }

    public class AddressItem
    {
        public int? AddressId { get; set; }
        public string FullAddress { get; set; }
    }
}