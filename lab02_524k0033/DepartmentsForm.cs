using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class DepartmentsForm : Form
    {
        private string connectionString;

        public DepartmentsForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyDataBase"].ConnectionString;
        }

        private void DepartmentsForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tblDepartments", connection);
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
                SqlCommand cmd = new SqlCommand("INSERT INTO tblDepartments (DepartmentName) VALUES(@DepartmentName)", connection);
                cmd.Parameters.AddWithValue("@DepartmentName", "New Department");
                cmd.ExecuteNonQuery();
                LoadData();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int departmentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DepartmentID"].Value);
                string departmentName = dataGridView1.SelectedRows[0].Cells["DepartmentName"].Value?.ToString();
                
                if (string.IsNullOrWhiteSpace(departmentName))
                {
                    MessageBox.Show("Department name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE tblDepartments SET DepartmentName = @DepartmentName WHERE DepartmentID = @DepartmentID", connection);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                    cmd.Parameters.AddWithValue("@DepartmentName", departmentName);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Department updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No department was updated. Please try again.");
                    }
                    
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Please select a row to update.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int departmentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DepartmentID"].Value);
                    
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this department? This will also remove all instructors in this department, their courses, and student enrollments.",
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
                                    "DELETE FROM tblStudentCourses WHERE CourseID IN (SELECT c.CourseID FROM tblCourses c INNER JOIN tblInstructors i ON c.InstructorID = i.InstructorID WHERE i.DepartmentID = @DepartmentID)", 
                                    connection, 
                                    transaction);
                                cmdDeleteEnrollments.Parameters.AddWithValue("@DepartmentID", departmentID);
                                int enrollmentsDeleted = cmdDeleteEnrollments.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteCourses = new SqlCommand(
                                    "DELETE FROM tblCourses WHERE InstructorID IN (SELECT InstructorID FROM tblInstructors WHERE DepartmentID = @DepartmentID)", 
                                    connection, 
                                    transaction);
                                cmdDeleteCourses.Parameters.AddWithValue("@DepartmentID", departmentID);
                                int coursesDeleted = cmdDeleteCourses.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteInstructors = new SqlCommand(
                                    "DELETE FROM tblInstructors WHERE DepartmentID = @DepartmentID", 
                                    connection, 
                                    transaction);
                                cmdDeleteInstructors.Parameters.AddWithValue("@DepartmentID", departmentID);
                                int instructorsDeleted = cmdDeleteInstructors.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteDepartment = new SqlCommand(
                                    "DELETE FROM tblDepartments WHERE DepartmentID = @DepartmentID", 
                                    connection, 
                                    transaction);
                                cmdDeleteDepartment.Parameters.AddWithValue("@DepartmentID", departmentID);
                                int rowsAffected = cmdDeleteDepartment.ExecuteNonQuery();
                                
                                transaction.Commit();
                                
                                if (rowsAffected > 0)
                                {
                                    string message = $"Department deleted successfully!";
                                    if (instructorsDeleted > 0)
                                        message += $" {instructorsDeleted} instructor(s) removed.";
                                    if (coursesDeleted > 0)
                                        message += $" {coursesDeleted} course(s) removed.";
                                    if (enrollmentsDeleted > 0)
                                        message += $" {enrollmentsDeleted} enrollment(s) removed.";
                                    MessageBox.Show(message);
                                }
                                else
                                {
                                    MessageBox.Show("No department was deleted. DepartmentID might not exist.");
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
