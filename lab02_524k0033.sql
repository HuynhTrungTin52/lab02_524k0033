USE SElab02 

CREATE TABLE tblUsers (
UserID INT PRIMARY KEY IDENTITY(1,1),
Username NVARCHAR(50) NOT NULL,
Password NVARCHAR(50) NOT NULL);

INSERT INTO tblUsers (Username, Password) VALUES ('user1', 'password1');
INSERT INTO tblUsers (Username, Password) VALUES ('user2', 'password2');

USE SchoolDb

CREATE TABLE tblStudents (
StudentID INT PRIMARY KEY IDENTITY(1,1),
StudentName NVARCHAR(100) NOT NULL,
DateOfBirth DATE NOT NULL,
City NVARCHAR(50) NOT NULL,
Age INT NOT NULL,
YearOfEnroll INT NOT NULL,
Major NVARCHAR(50),
GPA DECIMAL(3, 2)
);

INSERT INTO tblStudents (StudentName, DateOfBirth, City, Age, YearOfEnroll, Major,
GPA) VALUES
('Trung Pham', '2000-01-15', 'HCMC', 24, 2018, 'Computer Science', 3.5),
('Jane Smith', '1999-05-22', 'Los Angeles', 25, 2017, 'Mathematics', 3.8),
('Alice Johnson', '2001-03-10', 'Chicago', 23, 2019, 'Physics', 3.6),
('Bob Brown', '2002-07-30', 'Houston', 22, 2020, 'Chemistry', 3.7);


CREATE TABLE tblCourses (
CourseID INT PRIMARY KEY IDENTITY(1,1),
CourseName NVARCHAR(100) NOT NULL,
Credits INT NOT NULL,
DepartmentID INT NOT NULL,
InstructorID INT NOT NULL
);
INSERT INTO tblCourses (CourseName, Credits, DepartmentID,
InstructorID) VALUES
('Introduction to Computer Science', 3, 1, 1),
('Calculus I', 4, 2, 2),
('Physics I', 4, 3, 3),
('Organic Chemistry', 4, 4, 4);

CREATE TABLE tblStudentCourses (
StudentID INT NOT NULL,
CourseID INT NOT NULL,
PRIMARY KEY (StudentID, CourseID),
FOREIGN KEY (StudentID) REFERENCES tblStudents(StudentID),
FOREIGN KEY (CourseID) REFERENCES tblCourses(CourseID)
);
INSERT INTO tblStudentCourses (StudentID, CourseID) VALUES
(1, 1),
(1, 2),
(2, 3),
(3, 4),
(4, 1),
(4, 3);

CREATE TABLE tblInstructors (
InstructorID INT PRIMARY KEY IDENTITY(1,1),
InstructorName NVARCHAR(100) NOT NULL,
DepartmentID INT NOT NULL
);
INSERT INTO tblInstructors (InstructorName, DepartmentID) VALUES
('Dr. John Smith', 1),
('Dr. Jane Doe', 2),
('Dr. Alice Brown', 3),
('Dr. Bob White', 4);

CREATE TABLE tblDepartments (
DepartmentID INT PRIMARY KEY IDENTITY(1,1),
DepartmentName NVARCHAR(100) NOT NULL
);
INSERT INTO tblDepartments (DepartmentName) VALUES
('Computer Science'),
('Mathematics'),
('Physics'),
('Chemistry');

ALTER TABLE tblCourses
ADD CONSTRAINT FK_Courses_Departments
FOREIGN KEY (DepartmentID) REFERENCES tblDepartments(DepartmentID);
ALTER TABLE tblCourses
ADD CONSTRAINT FK_Courses_Instructors
FOREIGN KEY (InstructorID) REFERENCES tblInstructors(InstructorID);
ALTER TABLE tblStudentCourses
ADD CONSTRAINT FK_StudentCourses_Courses
FOREIGN KEY (CourseID) REFERENCES tblCourses(CourseID);

CREATE PROCEDURE GetCourses
AS
BEGIN
SELECT CourseID, CourseName FROM tblCourses;
END