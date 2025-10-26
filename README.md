<h1 align="center">📝Personal Task Manager</h1>

<p align="center">
  A clean and modular ASP.NET Core Web API built with <b>Clean Architecture</b>, <b>SOLID Principles</b>, <b>JWT Authentication</b>, and <b>EF Core</b>.
</p>

<p align="center">
  <!-- Badges -->
  <!-- <img src="https://img.shields.io/github/actions/workflow/status/MehfCodes/Personal-Task-Manager/dotnet.yml?style=flat-square&logo=github&label=Build" alt="Build"/>
  <img src="https://img.shields.io/github/license/MehfCodes/Personal-Task-Manager?style=flat-square&color=blue" alt="License"/> -->
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 9"/>
  <img src="https://img.shields.io/badge/Entity_Framework_Core-9.0-512BD4?style=flat-square&logo=efcore&logoColor=white" alt="EF Core 9"/>
  <img src="https://img.shields.io/badge/Swagger-UI-85EA2D?style=flat-square&logo=swagger&logoColor=black" alt="Swagger UI"/>
  <img src="https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white" alt="SQL Server"/>
  <img src="https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white" alt="Docker"/>
</p>





<!-- # Project Title -->

<!-- ## 📝Personal Task Manager

A clean and maintainable ASP.NET Core Web API project following SOLID principles and Clean Architecture. -->

---

## Table of Contents

- [Overview](#overview)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Features](#features)
- [Folder Details](#folder-details)
- [Getting Started](#getting-started)
- [Usage Example](#usage-example)
- [Policies & Business Rules](#policies--business-rules)
- [Contributing](#contributing)
- [Contact](#contact)

---

## 🚀Overview

This project is a personal task management system designed with maintainability, scalability, and security in mind. It demonstrates best practices in backend development, including:

- Clean Architecture separation of concerns
- SOLID principles
- JWT & Refresh Token authentication
- Policy-based authorization
- Comprehensive error handling and logging

It can be used as a reference project in a professional portfolio or as a starting point for real-world backend applications.

---

## 💻Technologies

- **Backend:** .NET 8 / ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Authentication:** JWT + Refresh Token
- **Database:** SQL Server (can be replaced with other providers)
- **Dependency Injection:** Built-in ASP.NET Core DI container
- **Logging:** Microsoft.Extensions.Logging
- **Unit Testing:** xUnit / Moq

---

## 🧱Architecture

The project follows **Clean Architecture**, separating concerns into multiple layers:

1. **Domain Layer:** Entities and business models (e.g., `User`, `TaskItem`, `UserPlan`).
2. **Application Layer:** Business logic, policies, services, DTOs, mappers, and interfaces.
3. **Infrastructure Layer:** Implementation of repositories, external services (email, JWT, refresh token handling).
4. **API Layer:** Controllers exposing endpoints for client applications.

This separation allows independent development, testing, and maintainability of each layer.

---

## ⚡Features

- **User Management**
  - Registration and login with JWT authentication
  - Password reset with email verification
  - Password update with token revocation
- **Task Management**
  - Create, update, delete, and retrieve tasks
  - Task status and priority management
  - Policy enforcement based on user plan
- **User Plans**
  - Purchase, activate, deactivate plans
  - Policy validation for task limits
- **Security**
  - JWT access tokens with refresh token rotation
  - Password hashing and verification
  - Policy-based authorization

### 📂Folder Details

- **Application**: Contains services, interfaces, policies, and mapping logic. All business rules are enforced here.
- **Domain**: Core entities and enums that define the domain model.
- **Infrastructure**: Implements repositories, email service, token generation, and other external dependencies.
- **API**: Entry point of the application, defines controllers and endpoints.
- **Contracts**: DTOs used for API request/response.
- **Tests**: Unit and integration tests for services and repositories.

## 👨🏻‍🎤Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/MehfCodes/Personal-Task-Manager.git
   ```
2. Update appsettings.json with your DB and email configurations.
3. Configure SMTP settings for email notifications
4. Run migrations and start the application:
   ```bash
   dotnet ef database update
   dotnet run --project API/PTM.API.csproj
   ```
5. The API will be available at: https://localhost

## Usage Example

Register a User

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "username": "username",
  "phoneNumber" : "phonenumber",
  "password": "YourStrongPassword123"
}
```

Create Plan

```http
POST /api/plan
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

{
    "title":"Premium",
    "description": "premium plan - monthly",
    "price":10,
    "maxTasks": 100,
    "durationDays": 30,
    "isActive": true
}
```

## 🚨Policies & Business Rules

- **User Plan Policies**:
  Ensures users have an active plan before performing actions.

  - Users can only create tasks if they have an active plan.
  - Plans are validated for expiration and activation.

- **Task Item Policies**: Checks max task limits and plan validity before creating tasks.

  - Task creation is limited by the plan's MaxTasks

- **Password Policies**: Enforces secure password reset and update flows.

## 🫱🏻‍🫲🏻Contributing

- Feel free to fork this repository and submit pull requests.
- Please adhere to SOLID principles and Clean Architecture practices when contributing.

## 📞Contact

For questions and business inqueries you can reach me at

<p align="left">
  <a href="https://github.com/MehfCodes" target="_blank">
        <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/github/github-original.svg" width="40" height="40" alt="GitHub" />
  </a>
   &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;
  <a href="https://www.linkedin.com/in/erfan-firouzabadi-09183119a/" target="_blank">
    <img src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/linkedin/linkedin-original.svg" width="40" height="40" alt="LinkedIn" />
  </a>
   &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;
  <a href="mailto:mehfdev@gmail.com" target="_blank">
    <img src="https://upload.wikimedia.org/wikipedia/commons/7/7e/Gmail_icon_%282020%29.svg" width="40" height="40" alt="Email" />
  </a>
</p>
