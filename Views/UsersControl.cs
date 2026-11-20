using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class UsersControl : UserControl
    {
        private readonly UserController _controller = new UserController();

        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _searchBox = new TextBox();
        private readonly ComboBox _roleFilter = new ComboBox();

        public UsersControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            UiHelper.StyleGrid(_grid);
            UiHelper.AddColumn(_grid, "Username", "UserName", 100);
            UiHelper.AddColumn(_grid, "E-mail", "Email", 140);
            UiHelper.AddColumn(_grid, "Role", "RoleDisplay", 90);
            UiHelper.AddColumn(_grid, "Company", "CompanyName", 120);
            UiHelper.AddColumn(_grid, "Status", "AccountStatus", 60);
            UiHelper.AddColumn(_grid, "Created", "CreatedAt", 80, "dd MMM yyyy");
            _grid.Dock = DockStyle.Fill;

            _searchBox.Width = 240;
            _searchBox.PlaceholderText = "Search username or e-mail";
            _searchBox.Margin = new Padding(0, 4, 10, 0);
            _searchBox.TextChanged += (s, e) => LoadUsers();

            _roleFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _roleFilter.Width = 180;
            _roleFilter.Margin = new Padding(0, 4, 0, 0);
            _roleFilter.Items.AddRange(new object[]
                { "All roles", "Administrators", "Company managers", "Collection workers", "Residents" });
            _roleFilter.SelectedIndex = 0;
            _roleFilter.SelectedIndexChanged += (s, e) => LoadUsers();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false };
            toolbar.Controls.AddRange(new Control[] { _searchBox, _roleFilter });

            Label title = new Label
            {
                Text = "Registered users",
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(_grid);
            Controls.Add(toolbar);
            Controls.Add(title);

            Load += (s, e) => LoadUsers();
        }

        private void LoadUsers()
        {
            UserRole? role = _roleFilter.SelectedIndex switch
            {
                1 => UserRole.Admin,
                2 => UserRole.CompanyManager,
                3 => UserRole.CollectionWorker,
                4 => UserRole.Resident,
                _ => null
            };

            DataResult<List<User>> result = _controller.GetUsers(_searchBox.Text, role);

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            _grid.DataSource = result.Data;
        }
    }
}