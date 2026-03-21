# ABCClinic System

# 🏥ABC Clinic Management System

The ABC Clinic Management System is a web-based platform designed to improve clinic operations by managing appointments, patient records, and administrative tasks. The system helps streamline communication between patients, doctors, and clinic administrators, reducing manual work and improving efficiency.

This project is developed as part of the System Development Project (COMP 2154) course.

## 📌 Project Overview

The ABC Clinic Management System aims to replace manual clinic management processes with a digital solution that allows clinics to efficiently manage appointments and medical records.
The system focuses on three main user roles:

Clinic Administrator

Doctor

Patient
Key capabilities of the system include:
- *Managing patient information*
- *Scheduling and managing appointments*
- *Storing medical records securely*
- *Providing a simple and responsive web interface for users*

## 🧑‍🤝‍🧑 Team Members (Group 25)

This project is developed by Group 25:

- Rezarta Marku (101402390)
 https://github.com/rezarta5/ABCClinicSystem
- Wasifa Hossain (101594842)
- Isha (101588052)
- Sanzida Islam (101564719)
  https://github.com/Subha06-star/ABCClinicSystem/tree/patch-1
Course: COMP 2154 – System Development Project

## 🚀 Features

### 🔹 *User Authentication*
- Secure login system for patients, doctors, and administrators
- Role-based access control (RBAC)
- Session management for secure access

### 🔹 *Patient Management*
Patients can:
- Register and manage their profile
- View appointment history
- Access medical records

### 🔹 *Doctor Management*
Doctors can:
- View scheduled appointments
- Access patient medical records
- Manage patient treatment information

### 🔹 *Appointment System*
The system allows:
- Patients to book appointments with doctors.
- Doctors to view and manage their schedule.
- Administrators to monitor and manage appointments.

### 🔹 *Medical Records Management*
Doctors can:
- Create and update patient medical records.
- Store diagnosis and treatment information.
- Maintain patient health history.

### 🔹 *Admin Management*
Administrators can:
- Manage users (patients and doctors).
- Monitor system activity.
- Maintain clinic operations.

### 🔹 *Security & Compliance*
- *Authentication & authorization* using secure methods.
- *Error handling and validation* for data security.

## 📌 System Modules
| Module                     | Description                                                   |
| -------------------------- | ------------------------------------------------------------- |
| *Authentication Module*  | Handles login, session management, and role-based access      |
| *Patient Module*         | Patient registration, profile management, appointment viewing |
| *Doctor Module*          | Doctor schedule management and patient medical record access  |
| *Appointment Module*     | Appointment scheduling and management                         |
| *Medical Records Module* | Storage and management of patient health records              |
| *Admin Module*           | User management and system monitoring                         |


## 🔧 *System Architecture*
The system follows a Three-Tier Architecture:
- *Frontend*: HTML, CSS, JavaScript, Bootstrap
- *Backend*: ASP.NET Core MVC
  Handles: Business logic, Authentication, Request processing
- *Database*: MySQL, Entity Framework Core (ORM)
  Responsible for storing: Users, Appointments, Medical Records

## 📌 *Database Structure*
The system follows a *relational database model*. The primary tables include:

- *Users* (Patients, Doctors, Admins)
- *Appointments*
- *Medical Records*
These tables are connected through primary keys and foreign key relationships.

## 🔧 *Technology Used*
| Technology            | Purpose                           |
| --------------------- | --------------------------------- |
| ASP.NET Core MVC      | Backend framework                 |
| Entity Framework Core | Database ORM                      |
| MySQL                 | Database management               |
| HTML                  | Page structure                    |
| CSS                   | Styling                           |
| Bootstrap             | Responsive design                 |
| JavaScript            | Client-side functionality         |
| GitHub                | Version control and collaboration |


## 🛠 *Installation & Setup*
1. *Clone the repository:*
   bash
   git clone https://github.com/rezarta5/ABCClinicSystem
   cd ABCClinicSystem
   
2. *Database Setup:*
   - Open MySQL / phpMyAdmin
   - Create a new database named clinic_management
   - Import the database schema files.

3. *Run the Project:*
   - Open the project in Visual Studio.
   - Configure the database connection 
   - Run the application

## *📚 Course Information*
Course: COMP 2154 – System Development Project

This project demonstrates the process of system planning, architecture design, database design, and implementation planning for a real-world clinic management system.
