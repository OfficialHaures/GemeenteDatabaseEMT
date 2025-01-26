using MySql.Data.MySqlClient;
using System.Data;

public partial class Form1 : Form
{
    private readonly string userRole;
    private System.ComponentModel.IContainer components = null;
    private DataGridView gridPeople;
    private DataGridView gridPlots;
    private TabControl tabControl;
    private TextBox txtSearch;
    private Panel searchResultPanel;
    private Panel infoPanel;
    private Button btnAddNew;
    private string connectionString = "Server=185.228.82.169;Database=s25_Gemeente;Uid=u25_DGGe8xteoX;Pwd=V5J9iLo7BWBP^lHjpXIma@SP;Port=3306;SslMode=none;";

    public Form1(string role)
    {
        userRole = role;
        InitializeComponent();
        SetupUI();
        LoadData();
        ApplyUserPermissions();
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1400, 900);
        this.Text = "GemeenteData Management";
        this.BackColor = Color.FromArgb(240, 240, 240);
    }

    private void SetupUI()
    {
        // Header Panel
        var headerPanel = new Panel
        {
            Height = 80,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(41, 128, 185)
        };

        var headerLabel = new Label
        {
            Text = "Gemeente Data Beheer",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };

        var userLabel = new Label
        {
            Text = $"👤 {userRole.ToUpper()}",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F),
            Location = new Point(1200, 30),
            AutoSize = true
        };

        headerPanel.Controls.AddRange(new Control[] { headerLabel, userLabel });

        // Search Container
        var searchContainer = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        txtSearch = new TextBox
        {
            Width = 400,
            Height = 35,
            Font = new Font("Segoe UI", 12F),
            Location = new Point(20, 18),
            PlaceholderText = "🔍 Zoek persoon of plot..."
        };

        // Action Button Panel
        var actionButtonPanel = new Panel
        {
            Height = 60,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        btnAddNew = new Button
        {
            Text = "➕ Nieuwe Toevoegen",
            Width = 200,
            Height = 35,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F),
            Location = new Point(20, 12),
            Cursor = Cursors.Hand
        };

        // Search Results Panel
        searchResultPanel = new Panel
        {
            Width = 400,
            Height = 300,
            Location = new Point(20, 80),
            AutoScroll = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Visible = false
        };

        // Tab Control
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            Padding = new Point(20)
        };

        // DataGridViews
        gridPeople = CreateDataGridView();
        gridPlots = CreateDataGridView();

        // Info Panel
        infoPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 300,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(15)
        };

        // Create Tabs
        var tabPeople = new TabPage("👥 Personen");
        var tabPlots = new TabPage("🏠 Plots");
        tabPeople.Controls.Add(gridPeople);
        tabPlots.Controls.Add(gridPlots);
        tabControl.TabPages.AddRange(new TabPage[] { tabPeople, tabPlots });

        // Events
        txtSearch.TextChanged += SearchTextChanged;
        btnAddNew.Click += BtnAddNew_Click;
        gridPeople.CellClick += (s, e) => ShowPersonDetails();
        gridPlots.CellClick += (s, e) => ShowPlotDetails();

        // Add controls
        actionButtonPanel.Controls.Add(btnAddNew);
        searchContainer.Controls.Add(txtSearch);
        this.Controls.AddRange(new Control[] { 
            headerPanel, 
            searchContainer,
            actionButtonPanel,
            searchResultPanel,
            tabControl, 
            infoPanel 
        });
    }

    private void BtnAddNew_Click(object sender, EventArgs e)
    {
        if (tabControl.SelectedTab.Text.Contains("Personen"))
        {
            var personForm = new PersonForm();
            if (personForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }
        else
        {
            var plotForm = new PlotForm();
            if (plotForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        searchResultPanel.Controls.Clear();
        var searchText = txtSearch.Text.ToLower();
        searchResultPanel.Visible = !string.IsNullOrEmpty(searchText);

        if (string.IsNullOrEmpty(searchText)) return;

        // Search People
        var peopleResults = ((DataTable)gridPeople.DataSource).DefaultView;
        peopleResults.RowFilter = $"voornaam LIKE '%{searchText}%' OR achternaam LIKE '%{searchText}%'";
        
        foreach (DataRowView row in peopleResults)
        {
            AddSearchResultButton(row, true);
        }

        // Search Plots
        var plotResults = ((DataTable)gridPlots.DataSource).DefaultView;
        plotResults.RowFilter = $"naam LIKE '%{searchText}%' OR eigenaar LIKE '%{searchText}%'";
        
        foreach (DataRowView row in plotResults)
        {
            AddSearchResultButton(row, false);
        }

        searchResultPanel.BringToFront();
    }
      private void AddSearchResultButton(DataRowView row, bool isPerson)
      {
          var container = new Panel
          {
              Width = 380,
              Height = 80,
              Dock = DockStyle.Top,
              BackColor = Color.White,
              Padding = new Padding(10)
          };

          var resultButton = new Button
          {
              Width = 280,
              Height = 60,
              FlatStyle = FlatStyle.Flat,
              BackColor = Color.White,
              TextAlign = ContentAlignment.MiddleLeft,
              Font = new Font("Segoe UI", 10F)
          };

          if (isPerson)
          {
              resultButton.Text = $"👤 {row["voornaam"]} {row["achternaam"]}\n📍 {row["adres"]}";
              resultButton.Click += (s, e) => SelectPerson(Convert.ToInt32(row["id"]));

              var addPlotButton = new Button
              {
                  Text = "🏠+",
                  Width = 60,
                  Height = 60,
                  FlatStyle = FlatStyle.Flat,
                  BackColor = Color.FromArgb(46, 204, 113),
                  ForeColor = Color.White,
                  Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                  Location = new Point(300, 0)
              };

              addPlotButton.Click += (s, e) => 
              {
                  var plotForm = new PlotForm(Convert.ToInt32(row["id"]));
                  if (plotForm.ShowDialog() == DialogResult.OK)
                  {
                      LoadData();
                  }
              };

              container.Controls.AddRange(new Control[] { resultButton, addPlotButton });
          }
          else
          {
              resultButton.Width = 360;
              resultButton.Text = $"🏠 {row["naam"]}\n👤 {row["eigenaar"]}";
              resultButton.Click += (s, e) => SelectPlot(Convert.ToInt32(row["id"]));
              container.Controls.Add(resultButton);
          }

          searchResultPanel.Controls.Add(container);
      }
    

    private void LoadData()
    {
        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Load People
                var cmdPeople = new MySqlCommand(
                    "SELECT id, voornaam, achternaam, adres, telefoon, email FROM personen", 
                    conn
                );
                var daPeople = new MySqlDataAdapter(cmdPeople);
                var dtPeople = new DataTable();
                daPeople.Fill(dtPeople);
                gridPeople.DataSource = dtPeople;

                // Load Plots with owner names
                var cmdPlots = new MySqlCommand(@"
                    SELECT p.id, p.naam, p.oppervlakte, 
                           CONCAT(pers.voornaam, ' ', pers.achternaam) as eigenaar,
                           p.status
                    FROM plots p 
                    LEFT JOIN personen pers ON p.eigenaar_id = pers.id", 
                    conn
                );
                var daPlots = new MySqlDataAdapter(cmdPlots);
                var dtPlots = new DataTable();
                daPlots.Fill(dtPlots);
                gridPlots.DataSource = dtPlots;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Database Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyUserPermissions()
    {
        switch (userRole.ToLower())
        {
            case "admin":
                // Full access
                break;
            case "editor":
                // Can view and edit, but not delete
                btnAddNew.Enabled = false;
                break;
            case "viewer":
                // Read-only access
                btnAddNew.Enabled = false;
                gridPeople.ReadOnly = true;
                gridPlots.ReadOnly = true;
                break;
        }
    }

    private DataGridView CreateDataGridView()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(64, 64, 64),
                SelectionBackColor = Color.FromArgb(41, 128, 185),
                SelectionForeColor = Color.White,
                Padding = new Padding(5)
            }
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }


private void ShowPersonDetails()
{
    if (gridPeople.SelectedRows.Count == 0) return;
    var row = gridPeople.SelectedRows[0];

    infoPanel.Controls.Clear();
    infoPanel.Controls.AddRange(new Control[] {
        new Label { 
            Text = $"👤 {row.Cells["voornaam"].Value} {row.Cells["achternaam"].Value}",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true
        },
        new Label { 
            Text = $"📍 {row.Cells["adres"].Value}",
            Location = new Point(0, 40),
            AutoSize = true
        },
        new Label { 
            Text = $"📞 {row.Cells["telefoon"].Value}",
            Location = new Point(0, 70),
            AutoSize = true
        }
    });
}

private void ShowPlotDetails()
{
    if (gridPlots.SelectedRows.Count == 0) return;
    var row = gridPlots.SelectedRows[0];

    infoPanel.Controls.Clear();
    infoPanel.Controls.AddRange(new Control[] {
        new Label { 
            Text = $"🏠 {row.Cells["naam"].Value}",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true
        },
        new Label { 
            Text = $"📏 Oppervlakte: {row.Cells["oppervlakte"].Value} m²",
            Location = new Point(0, 40),
            AutoSize = true
        },
        new Label { 
            Text = $"👤 Eigenaar: {row.Cells["eigenaar"].Value}",
            Location = new Point(0, 70),
            AutoSize = true
        }
    });
}

private void SelectPerson(int personId)
{
    tabControl.SelectedIndex = 0; // Switch to Persons tab
    foreach (DataGridViewRow row in gridPeople.Rows)
    {
        if (Convert.ToInt32(row.Cells["id"].Value) == personId)
        {
            gridPeople.ClearSelection();
            row.Selected = true;
            ShowPersonDetails();
            break;
        }
    }
    searchResultPanel.Visible = false;
    txtSearch.Clear();
}

private void SelectPlot(int plotId)
{
    tabControl.SelectedIndex = 1; // Switch to Plots tab
    foreach (DataGridViewRow row in gridPlots.Rows)
    {
        if (Convert.ToInt32(row.Cells["id"].Value) == plotId)
        {
            gridPlots.ClearSelection();
            row.Selected = true;
            ShowPlotDetails();
            break;
        }
    }
    searchResultPanel.Visible = false;
    txtSearch.Clear();
}
}
