using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class CoursesForm : Form
    {
        private string connectionString;

        public CoursesForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyDataBase"].ConnectionString;
        }

        private void CoursesForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tblCourses", connection);
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
                SqlCommand cmd = new SqlCommand("INSERT INTO tblCourses (CourseName, Credits, InstructorID) VALUES(@CourseName, @Credits, @InstructorID)", connection);
                cmd.Parameters.AddWithValue("@CourseName", "New Course");
                cmd.Parameters.AddWithValue("@Credits", 3);
                cmd.Parameters.AddWithValue("@InstructorID", 1);
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
                    int courseID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CourseID"].Value);
                    string courseName = dataGridView1.SelectedRows[0].Cells["CourseName"].Value?.ToString();
                    int credits = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Credits"].Value);
                    int instructorID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["InstructorID"].Value);
                    
                    if (string.IsNullOrWhiteSpace(courseName))
                    {
                        MessageBox.Show("Course name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (credits <= 0 || credits > 10)
                    {
                        MessageBox.Show("Credits must be between 1 and 10.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (instructorID <= 0)
                    {
                        MessageBox.Show("Please select a valid instructor.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE tblCourses SET CourseName = @CourseName, Credits = @Credits, InstructorID = @InstructorID WHERE CourseID = @CourseID", 
                            connection);
                        cmd.Parameters.AddWithValue("@CourseID", courseID);
                        cmd.Parameters.AddWithValue("@CourseName", courseName);
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@InstructorID", instructorID);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Course updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("No course was updated. Please try again.");
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
                    MessageBox.Show("The selected instructor does not exist. Please choose a valid instructor.", "Invalid Instructor", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    int courseID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CourseID"].Value);
                    
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this course? This will also remove all student enrollments for this course.",
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
                                    "DELETE FROM tblStudentCourses WHERE CourseID = @CourseID", 
                                    connection, 
                                    transaction);
                                cmdDeleteEnrollments.Parameters.AddWithValue("@CourseID", courseID);
                                int enrollmentsDeleted = cmdDeleteEnrollments.ExecuteNonQuery();
                                
                                SqlCommand cmdDeleteCourse = new SqlCommand(
                                    "DELETE FROM tblCourses WHERE CourseID = @CourseID", 
                                    connection, 
                                    transaction);
                                cmdDeleteCourse.Parameters.AddWithValue("@CourseID", courseID);
                                int rowsAffected = cmdDeleteCourse.ExecuteNonQuery();
                                
                                transaction.Commit();
                                
                                if (rowsAffected > 0)
                                {
                                    string message = enrollmentsDeleted > 0 
                                        ? $"Course deleted successfully! {enrollmentsDeleted} student enrollment(s) also removed."
                                        : "Course deleted successfully!";
                                    MessageBox.Show(message);
                                }
                                else
                                {
                                    MessageBox.Show("No course was deleted. CourseID might not exist.");
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
