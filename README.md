# FreshMart - Online Grocery Store Management System

A comprehensive .NET Core MVC web application for managing an online grocery store with customer ordering, admin management, and delivery tracking capabilities.

## Features

### Customer Features
- User registration and login with email/password
- Browse products by categories (Fruits, Vegetables, Dairy, Bakery, Beverages, Snacks, Household)
- Search and filter products by name, category, and price range
- Add products to shopping cart with quantity management
- Manage multiple delivery addresses
- Checkout with multiple payment options (Cash on Delivery, UPI, Card)
- Track order status (Pending, Packed, Out for Delivery, Delivered, Cancelled)
- View order history and details
- Receive notifications for order updates

### Admin Features
- Dashboard with sales statistics and analytics
- Product management (Add, Edit, Delete products)
- Category management
- Order management with status updates
- Assign delivery staff to orders
- Generate reports (Sales, Daily Orders, Top Products, Inventory)
- Export data to CSV

### Delivery Staff Features
- View assigned deliveries
- Update delivery status
- Access customer contact information
- Track completed deliveries

## Technology Stack

- **Framework**: .NET 8.0
- **Language**: C#
- **Database**: MySQL with Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery
- **Authentication**: Cookie-based Authentication
- **Charts**: Chart.js for reports

## Project Structure

```
GroceryStore/
├── Controllers/
│   ├── AccountController.cs      # Authentication (Login, Register, Logout)
│   ├── HomeController.cs         # Home page, About, Contact
│   ├── ProductController.cs      # Product browsing and search
│   ├── CartController.cs         # Shopping cart management
│   ├── OrderController.cs        # Order placement and tracking
│   ├── AdminController.cs        # Admin management features
│   ├── DeliveryController.cs     # Delivery staff operations
│   └── ReportsController.cs      # Reports and analytics
├── Models/
│   ├── User.cs                   # User entity
│   ├── Product.cs                # Product entity
│   ├── Category.cs               # Category entity
│   ├── Cart.cs / CartItem.cs     # Shopping cart entities
│   ├── Order.cs / OrderItem.cs   # Order entities
│   ├── Address.cs                # Address entity
│   ├── Delivery.cs               # Delivery entity
│   ├── Payment.cs                # Payment entity
│   └── Notification.cs           # Notification entity
├── ViewModels/                   # View models for forms and displays
├── Views/                        # Razor views
├── Data/
│   └── ApplicationDbContext.cs   # Entity Framework DbContext
└── wwwroot/                      # Static files (CSS, JS, images)
```

## Database Schema

The application uses the following database tables:
- **Users** - Store user information (customers, admins, delivery staff)
- **Addresses** - Customer delivery addresses
- **Categories** - Product categories
- **Products** - Product catalog
- **Carts** / **CartItems** - Shopping cart data
- **Orders** / **OrderItems** - Order information
- **Deliveries** - Delivery tracking
- **Payments** - Payment records
- **Notifications** - User notifications

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- MySQL Server
- Visual Studio 2022 or VS Code

### Installation

1. **Clone or extract the project**
   ```bash
   cd GroceryStore
   ```

2. **Update Database Connection**
   Edit `appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=online_grocery_store;User=your_username;Password=your_password;"
   }
   ```

3. **Create Database**
   The application will automatically create the database and seed initial data on first run.

4. **Run Migrations (if needed)**
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Customer Portal: https://localhost:5001 or http://localhost:5000
   - Default Admin Account: Create through database or register and set IsAdmin = 1 in database

## Default Data

The application seeds the following default data on first run:

### Categories
- Fruits
- Vegetables
- Dairy
- Bakery
- Beverages
- Snacks
- Household

### Sample Products
- Fresh Apples, Bananas, Oranges
- Tomatoes, Potatoes, Onions
- Fresh Milk, Cheese, Butter
- White Bread, Croissants
- Orange Juice, Mineral Water
- Potato Chips, Chocolate Bar
- Dish Soap, Laundry Detergent

## User Roles

### Customer
- Browse and search products
- Manage cart and place orders
- Track orders
- Manage addresses

### Admin
- Full access to admin dashboard
- Manage products and categories
- Manage orders and assign delivery staff
- View reports and analytics

### Delivery Staff
- View assigned deliveries
- Update delivery status
- Access customer information

## Screenshots

The application features a modern, responsive design with:
- Clean and intuitive navigation
- Mobile-friendly interface
- Interactive charts and graphs
- Real-time order tracking
- Professional admin dashboard

## Security Features

- Password hashing with BCrypt
- Cookie-based authentication
- Anti-forgery tokens
- Role-based authorization
- Input validation
- SQL injection protection via Entity Framework

## Future Enhancements

- Email/SMS notifications
- Online payment gateway integration
- Real-time order tracking with GPS
- Mobile app
- Multi-language support
- Advanced analytics and AI recommendations

## License

This project is for educational purposes.

## Support

For any issues or questions, please contact classy.span@gmail.com
