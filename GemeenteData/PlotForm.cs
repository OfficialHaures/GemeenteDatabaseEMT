using MySql.Data.MySqlClient;
using System.Data;

public class PlotForm : Form
{
    private int? plotId;
    private TextBox txtName;
    private TextBox txtArea;
    private ComboBox cmbOwner;
    private Button btnSave;
    private Button btnCancel;
    private string connectionString = "";

    private int? selectedPersonId;

    public PlotForm(int? personId = null)
    {
        selectedPersonId = personId;
        InitializeControls();
        LoadOwners();

        if (selectedPersonId.HasValue)
        {
            cmbOwner.SelectedValue = selectedPersonId;
        }
    
}    private void InitializeControls()
    {
        this.Text = plotId.HasValue ? "Plot Bewerken" : "Nieuwe Plot";
        this.Size = new Size(600, 400);
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;

        var formTitle = new Label
        {
            Text = this.Text,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 128, 185),
            Location = new Point(30, 20),
            AutoSize = true
        };

        txtName = CreateTextBox("Naam", 80);
        txtArea = CreateTextBox("Oppervlakte (m²)", 140);
        
        cmbOwner = new ComboBox
        {
            Width = 400,
            Height = 35,
            Location = new Point(30, 200),
            Font = new Font("Segoe UI", 12F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        btnSave = new Button
        {
            Text = "💾 Opslaan",
            Width = 180,
            Height = 45,
            Location = new Point(30, 290),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        btnCancel = new Button
        {
            Text = "❌ Annuleren",
            Width = 180,
            Height = 45,
            Location = new Point(250, 290),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (s, e) => this.Close();

        this.Controls.AddRange(new Control[] { 
            formTitle, 
            txtName, CreateLabel("Naam", 80),
            txtArea, CreateLabel("Oppervlakte (m²)", 140),
            cmbOwner, CreateLabel("Eigenaar", 200),
            btnSave, btnCancel 
        });
    }

    private TextBox CreateTextBox(string placeholder, int yPos)
    {
        return new TextBox
        {
            Width = 400,
            Height = 35,
            Location = new Point(30, yPos),
            Font = new Font("Segoe UI", 12F),
            PlaceholderText = placeholder,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private Label CreateLabel(string text, int yPos)
    {
        return new Label
        {
            Text = text,
            Location = new Point(30, yPos - 25),
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true
        };
    }

    private void LoadOwners()
    {
        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, CONCAT(voornaam, ' ', achternaam) as naam FROM personen", 
                    conn
                );
                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                cmbOwner.DisplayMember = "naam";
                cmbOwner.ValueMember = "id";
                cmbOwner.DataSource = dt;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading owners: {ex.Message}", "Database Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadPlotData()
    {
        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT * FROM plots WHERE id = @id", 
                    conn
                );
                cmd.Parameters.AddWithValue("@id", plotId.Value);
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtName.Text = reader["naam"].ToString();
                        txtArea.Text = reader["oppervlakte"].ToString();
                        cmbOwner.SelectedValue = reader["eigenaar_id"];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading plot data: {ex.Message}", "Database Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || 
            string.IsNullOrWhiteSpace(txtArea.Text))
        {
            MessageBox.Show("Vul alle verplichte velden in!", "Validatie Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtArea.Text, out decimal area))
        {
            MessageBox.Show("Vul een geldig oppervlakte in!", "Validatie Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd;

                if (plotId.HasValue)
                {
                    cmd = new MySqlCommand(@"
                        UPDATE plots 
                        SET naam = @name, 
                            oppervlakte = @area, 
                            eigenaar_id = @ownerId 
                        WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", plotId.Value);
                }
                else
                {
                    cmd = new MySqlCommand(@"
                        INSERT INTO plots 
                        (naam, oppervlakte, eigenaar_id, status) 
                        VALUES 
                        (@name, @area, @ownerId, 'Actief')", conn);
                }

                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@area", area);
                cmd.Parameters.AddWithValue("@ownerId", cmbOwner.SelectedValue);

                cmd.ExecuteNonQuery();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving data: {ex.Message}", "Database Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
