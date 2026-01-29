using System;
using System.Linq;
using System.Windows;

namespace KatanUP01.UI
{
    public partial class EditRequestWindow : Window
    {
        private Entities _context;
        private Requests _request;
        private bool _isEditMode;

        public EditRequestWindow(int? requestId = null)
        {
            InitializeComponent();
            InitializeData(requestId);
        }

        private void InitializeData(int? requestId)
        {
            try
            {
                _context = new Entities();

                // Загружаем списки для ComboBox
                cbAddress.ItemsSource = _context.Addresses.ToList();
                cbAddress.DisplayMemberPath = "FullAddress";

                cbEmployee.ItemsSource = _context.Employees.ToList();
                cbEmployee.DisplayMemberPath = "LastName";

                cbStatus.ItemsSource = new[] { "Открыта", "В работе", "Закрыта" };

                if (requestId.HasValue)
                {
                    // Режим редактирования
                    _request = _context.Requests.Find(requestId.Value);
                    _isEditMode = true;
                    Title = "Редактирование заявки";

                    if (_request != null)
                    {
                        cbAddress.SelectedValue = _request.AddressId;
                        txtApplicantName.Text = _request.ApplicantName;
                        txtPhone.Text = _request.ApplicantPhone;
                        txtDescription.Text = _request.ProblemDescription;
                        cbEmployee.SelectedValue = _request.EmployeeId;
                        cbStatus.SelectedItem = _request.Status;
                    }
                }
                else
                {
                    // Режим добавления
                    _request = new Requests
                    {
                        CreatedDate = DateTime.Now,
                        Status = "Открыта"
                    };
                    _isEditMode = false;
                    Title = "Новая заявка";
                    cbStatus.SelectedItem = "Открыта";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка обязательных полей
                if (cbAddress.SelectedValue == null)
                {
                    MessageBox.Show("Выберите адрес", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtApplicantName.Text))
                {
                    MessageBox.Show("Введите ФИО заявителя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    MessageBox.Show("Введите описание проблемы", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сохраняем данные
                _request.AddressId = (int)cbAddress.SelectedValue;
                _request.ApplicantName = txtApplicantName.Text.Trim();
                _request.ApplicantPhone = txtPhone.Text?.Trim();
                _request.ProblemDescription = txtDescription.Text.Trim();
                _request.EmployeeId = (int?)cbEmployee.SelectedValue;
                _request.Status = cbStatus.SelectedItem as string;

                if (!_isEditMode)
                {
                    _context.Requests.Add(_request);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}