using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab02_524k0033
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void viewStudentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StudentsForm studentsForm = new StudentsForm();
            studentsForm.Show();
        }

        private void viewCoursesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoursesForm coursesForm = new CoursesForm();
            coursesForm.Show();
        }

        private void viewInstructorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstructorsForm instructorsForm = new InstructorsForm();
            instructorsForm.Show();
        }

        private void viewDepartmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DepartmentsForm departmentsForm = new DepartmentsForm();
            departmentsForm.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
