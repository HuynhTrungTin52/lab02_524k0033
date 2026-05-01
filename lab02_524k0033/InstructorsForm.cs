using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class InstructorsForm : Form
    {
        private string connectionString;

        public InstructorsForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyDataBase"].ConnectionString;
        }

        private void InstructorsForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tblInstructors", connection);
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
                SqlCommand cmd = new SqlCommand("INSERT INTO tblInstructors (InstructorName, DepartmentID) VALUES(@InstructorName, @DepartmentID)", connection);
                cmd.Parameters.AddWithValue("@InstructorName", "New Instructor");
                cmd.Parameters.AddWithValue("@DepartmentID", 1);
                cmd.ExecuteNonQuery();
                LoadData();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int instructorID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["InstructorID"].Value);
                    string instructorName = dataGridView1.SelectedRows[0].Cells["InstructorName"].Value?.ToString();
                    int departmentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DepartmentID"].Value);
                    
                    
                    if (string.IsNullOrWhiteSpace(instructorName))
                    {
                        MessageBox.Show("Instructor name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (departmentID <= 0)
                    {
                        MessageBox.Show("Please select a valid department.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE tblInstructors SET InstructorName = @InstructorName, DepartmentID = @DepartmentID WHERE InstructorID = @InstructorID", 
                            connection);
                        cmd.Parameters.AddWithValue("@InstructorID", instructorID);
                        cmd.Parameters.AddWithValue("@InstructorName", instructorName);
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Instructor updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("No instructor was updated. Please try again.");
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
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    MessageBox.Show("The selected department does not exist. Please choose a valid department.", "Invalid Department", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int instructorID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["InstructorID"].Value);
                    
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this instructor? This will also remove all courses taught by this instructor and their student enrollments.",
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
                                SqlCommand cmdDeleteEnrollments = new SqlCommand(
                                    "DELETE FROM tblStudentCourses WHERE CourseID IN (SELECT CourseID FROM tblCourses WHERE InstructorID = @InstructorID)", 
                                    connection, 
                                    transaction);
                                cmdDeleteEnrollments.Parameters.AddWithValue("@InstructorID", instructorID);
                                int enrollmentsDeleted = cmdDeleteEnrollments.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteCourses = new SqlCommand(
                                    "DELETE FROM tblCourses WHERE InstructorID = @InstructorID", 
                                    connection, 
                                    transaction);
                                cmdDeleteCourses.Parameters.AddWithValue("@InstructorID", instructorID);
                                int coursesDeleted = cmdDeleteCourses.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteInstructor = new SqlCommand(
                                    "DELETE FROM tblInstructors WHERE InstructorID = @InstructorID", 
                                    connection, 
                                    transaction);
                                cmdDeleteInstructor.Parameters.AddWithValue("@InstructorID", instructorID);
                                int rowsAffected = cmdDeleteInstructor.ExecuteNonQuery();
                                
                                transaction.Commit();
                                
                                if (rowsAffected > 0)
                                {
                                    string message = $"Instructor deleted successfully!";
                                    if (coursesDeleted > 0)
                                        message += $" {coursesDeleted} course(s) removed.";
                                    if (enrollmentsDeleted > 0)
                                        message += $" {enrollmentsDeleted} enrollment(s) removed.";
                                    MessageBox.Show(message);
                                }
                                else
                                {
                                    MessageBox.Show("No instructor was deleted. InstructorID might not exist.");
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
