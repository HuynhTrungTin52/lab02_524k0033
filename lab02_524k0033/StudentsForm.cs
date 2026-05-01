using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class StudentsForm : Form
    {
        private string connectionString;

        public StudentsForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyDataBase"].ConnectionString;
        }

        private void StudentsForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tblStudents", connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridView1.DataSource = table;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO tblStudents (StudentName, DateOfBirth, City, Age, YearOfEnroll, Major, GPA) VALUES(@StudentName, @DateOfBirth, @City, @Age, @YearOfEnroll, @Major, @GPA)", connection);
                cmd.Parameters.AddWithValue("@StudentName", "New Student");
                cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Now);
                cmd.Parameters.AddWithValue("@City", "City");
                cmd.Parameters.AddWithValue("@Age", 20);
                cmd.Parameters.AddWithValue("@YearOfEnroll", 2021);
                cmd.Parameters.AddWithValue("@Major", "Major");
                cmd.Parameters.AddWithValue("@GPA", 4.0);
                cmd.ExecuteNonQuery();
                LoadData();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int studentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["StudentID"].Value);
                    
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this student? This will also remove all their course enrollments.",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            SqlTransaction transaction = connection.BeginTransaction();
                            
                            try
                            {
                                SqlCommand cmdDeleteCourses = new SqlCommand(
                                    "DELETE FROM tblStudentCourses WHERE StudentID = @StudentID", 
                                    connection, 
                                    transaction);
                                cmdDeleteCourses.Parameters.AddWithValue("@StudentID", studentID);
                                int coursesDeleted = cmdDeleteCourses.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteStudent = new SqlCommand(
                                    "DELETE FROM tblStudents WHERE StudentID = @StudentID", 
                                    connection, 
                                    transaction);
                                cmdDeleteStudent.Parameters.AddWithValue("@StudentID", studentID);
                                int rowsAffected = cmdDeleteStudent.ExecuteNonQuery();
                                
                                transaction.Commit();
                                
                                if (rowsAffected > 0)
                                {
                                    string message = coursesDeleted > 0 
                                        ? $"Student deleted successfully! {coursesDeleted} course enrollment(s) also removed."
                                        : "Student deleted successfully!";
                                    MessageBox.Show(message);
                                }
                                else
                                {
                                    MessageBox.Show("No student was deleted. StudentID might not exist.");
                                }
                                
                                LoadData();
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to delete.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int studentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["StudentID"].Value);
                    string studentName = dataGridView1.SelectedRows[0].Cells["StudentName"].Value?.ToString();
                    DateTime dateOfBirth = Convert.ToDateTime(dataGridView1.SelectedRows[0].Cells["DateOfBirth"].Value);
                    string city = dataGridView1.SelectedRows[0].Cells["City"].Value?.ToString();
                    int age = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Age"].Value);
                    int yearOfEnroll = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["YearOfEnroll"].Value);
                    string major = dataGridView1.SelectedRows[0].Cells["Major"].Value?.ToString();
                    decimal gpa = Convert.ToDecimal(dataGridView1.SelectedRows[0].Cells["GPA"].Value);
                    
                    if (string.IsNullOrWhiteSpace(studentName))
                    {
                        MessageBox.Show("Student name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(city))
                    {
                        MessageBox.Show("City cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (age <= 0 || age > 150)
                    {
                        MessageBox.Show("Please enter a valid age.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (gpa < 0 || gpa > 4.0m)
                    {
                        MessageBox.Show("GPA must be between 0 and 4.0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE tblStudents SET StudentName = @StudentName, DateOfBirth = @DateOfBirth, City = @City, Age = @Age, YearOfEnroll = @YearOfEnroll, Major = @Major, GPA = @GPA WHERE StudentID = @StudentID", 
                            connection);
                        cmd.Parameters.AddWithValue("@StudentID", studentID);
                        cmd.Parameters.AddWithValue("@StudentName", studentName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@Age", age);
                        cmd.Parameters.AddWithValue("@YearOfEnroll", yearOfEnroll);
                        cmd.Parameters.AddWithValue("@Major", major ?? "");
                        cmd.Parameters.AddWithValue("@GPA", gpa);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Student updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("No student was updated. Please try again.");
                        }
                        
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to update.");
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid data format. Please check the values in the selected row.", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
