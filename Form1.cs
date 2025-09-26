using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void btnLayAnh_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            openFileDialog.Title = "Select an Image File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Read the image file into a byte array
                    byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);

                    // Save to database
                    using (SqlConnection connection = new SqlConnection("your_connection_string_here"))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("UPDATE Phim SET Image = @Image WHERE PhimID = @PhimID", connection))
                        {
                            command.Parameters.AddWithValue("@Image", imageBytes);
                            command.Parameters.AddWithValue("@PhimID", txtMaPhim.Text); // Assuming you have a textbox for PhimID
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Image saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
} 