using MySql.Data.MySqlClient;

public class PersonForm : Form
{
    private int? personId;
    private TextBox txtFirstName, txtLastName, txtAddress, txtPhone, txtEmail;
    private Button btnSave, btnCancel;
    private string connectionString = "Server=185.228.82.169;Database=s25_Gemeente;Uid=u25_DGGe8xteoX;Pwd=V5J9iLo7BWBP^lHjpXIma@SP;Port=3306;SslMode=none;";

    public PersonForm(int? id = null, string firstName = "", string lastName = "", 
                     string address = "", string phone = "", string email = "")
    {
        personId = id;
        InitializeControls();
        
        txtFirstName.Text = firstName;
        txtLastName.Text = lastName;
        txtAddress.Text = address;
        txtPhone.Text = phone;
        txtEmail.Text = email;
    }

    private void InitializeControls()
    {
        this.Text = personId.HasValue ? "Persoon Bewerken" : "Nieuwe Persoon";
        this.Size = new Size(600, 500);
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

        txtFirstName = CreateTextBox("Voornaam", 80);
        txtLastName = CreateTextBox("Achternaam", 140);
        txtAddress = CreateTextBox("Adres", 200);
        txtPhone = CreateTextBox("Telefoon", 260);
        txtEmail = CreateTextBox("Email", 320);

        btnSave = new Button
        {
            Text = "💾 Opslaan",
            Width = 180,
            Height = 45,
            Location = new Point(30, 390),
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
            Location = new Point(250, 390),
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
            txtFirstName, CreateLabel("Voornaam", 80),
            txtLastName, CreateLabel("Achternaam", 140),
            txtAddress, CreateLabel("Adres", 200),
            txtPhone, CreateLabel("Telefoon", 260),
            txtEmail, CreateLabel("Email", 320),
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

    private void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFirstName.Text) || 
            string.IsNullOrWhiteSpace(txtLastName.Text))
        {
            MessageBox.Show("Vul alle verplichte velden in!", "Validatie Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd;

                if (personId.HasValue)
                {
                    cmd = new MySqlCommand(@"
                        UPDATE personen 
                        SET voornaam = @firstName, 
                            achternaam = @lastName, 
                            adres = @address, 
                            telefoon = @phone, 
                            email = @email 
                        WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", personId.Value);
                }
                else
                {
                    cmd = new MySqlCommand(@"
                        INSERT INTO personen 
                        (voornaam, achternaam, adres, telefoon, email) 
                        VALUES 
                        (@firstName, @lastName, @address, @phone, @email)", conn);
                }

                cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@email", txtEmail.Text);

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
