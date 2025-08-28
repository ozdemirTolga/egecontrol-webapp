# Copilot Instructions for Ege Control Web App

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is an ASP.NET Core 9 web application for Ege Control business management system (www.egecontrol.com).

## Project Overview
- **Framework**: ASP.NET Core 9 with Razor Pages
- **Authentication**: ASP.NET Core Identity
- **Database**: SQL Server with Entity Framework Core
- **PDF Generation**: iText7 library

## Key Features
1. **Admin Authentication System**: Secure login for administrators
2. **Customer Management**: CRUD operations for customer data
3. **Quote/Proposal System**: Create, edit, and manage business proposals
4. **PDF Export**: Generate PDF documents from proposals
5. **Admin Dashboard**: Centralized management interface

## Architecture Guidelines
- Use Razor Pages for UI
- Follow Repository pattern for data access
- Implement proper error handling and validation
- Use Entity Framework Core for database operations
- Apply proper security measures for admin areas

## Business Logic
- Only authenticated admins can access the system
- Customer information should auto-populate when creating quotes
- Quotes should be editable and trackable
- PDF generation should include company branding and professional formatting

## Code Style
- Follow C# naming conventions
- Use async/await for database operations
- Implement proper logging
- Add appropriate comments for complex business logic
- Use dependency injection for services
