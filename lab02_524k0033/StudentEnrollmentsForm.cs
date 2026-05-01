using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class StudentEnrollmentsForm : Form
    {
        private string connectionString;

        public StudentEnrollmentsForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MyDatabase"].ConnectionString;
        }

        private void StudentEnrollmentsForm_Load(object sender, EventArgs e)
        {
            LoadCourses();
            LoadData();
        }

        private void LoadCourses()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetCourses", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                comboBoxCourses.DisplayMember = "CourseName";
                comboBoxCourses.ValueMember = "CourseID";
                comboBoxCourses.DataSource = table;
            }
        }

        private void LoadData()
        {
            if (comboBoxCourses.SelectedValue != null)
            {
                int courseID = Convert.ToInt32(comboBoxCourses.SelectedValue);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tblStudentCourses WHERE CourseID=@CourseID", connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@CourseID", courseID);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxCourses.SelectedValue == null)
                {
                    MessageBox.Show("Please select a course.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int courseID = Convert.ToInt32(comboBoxCourses.SelectedValue);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO tblStudentCourses (StudentID, CourseID) VALUES (@StudentID, @CourseID)", connection);
                    cmd.Parameters.AddWithValue("@StudentID", 1);
                    cmd.Parameters.AddWithValue("@CourseID", courseID);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Enrollment added successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to add enrollment. Please try again.");
                    }

                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    MessageBox.Show("This student is already enrolled in this course.", "Duplicate Enrollment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ex.Number == 547)
                {
                    MessageBox.Show("Invalid student or course ID. Please verify the data.", "Invalid Reference", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0 && comboBoxCourses.SelectedValue != null)
                {
                    int studentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["StudentID"].Value);
                    int oldCourseID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CourseID"].Value);
                    int newCourseID = Convert.ToInt32(comboBoxCourses.SelectedValue);

                    if (oldCourseID == newCourseID)
                    {
                        MessageBox.Show("The student is already enrolled in the selected course.", "No Change", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE tblStudentCourses SET CourseID=@NewCourseID WHERE StudentID=@StudentID AND CourseID=@OldCourseID", connection);
                        cmd.Parameters.AddWithValue("@StudentID", studentID);
                        cmd.Parameters.AddWithValue("@OldCourseID", oldCourseID);
                        cmd.Parameters.AddWithValue("@NewCourseID", newCourseID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Enrollment updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("No enrollment was updated. Please try again.");
                        }

                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to update and choose a course.");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    MessageBox.Show("This student is already enrolled in the selected course.", "Duplicate Enrollment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ex.Number == 547)
                {
                    MessageBox.Show("Invalid course selection. Please choose a valid course.", "Invalid Course", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    int studentID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["StudentID"].Value);
                    int courseID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CourseID"].Value);

                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to remove this student from the course?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM tblStudentCourses WHERE StudentID=@StudentID AND CourseID=@CourseID", connection);
                            cmd.Parameters.AddWithValue("@StudentID", studentID);
                            cmd.Parameters.AddWithValue("@CourseID", courseID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Enrollment deleted successfully!");
                            }
                            else
                            {
                                MessageBox.Show("No enrollment was deleted. The record might not exist.");
                            }

                            LoadData();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to delete.");
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

        private void comboBoxCourses_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}