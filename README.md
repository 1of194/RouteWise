# RouteWise

A modern route optimization application that helps delivery drivers and logistics professionals plan efficient routes for multiple delivery addresses. RouteWise uses advanced algorithms to calculate optimal delivery sequences, reducing travel time and fuel costs.

## 🚀 Features

- **Route Optimization**: Calculate the most efficient delivery routes using advanced algorithms
- **User Authentication**: Secure JWT-based authentication system
- **Bookmark Management**: Save and manage frequently used delivery routes
- **Real-time Route Calculation**: Dynamic route optimization with priority levels
- **Interactive Map Integration**: Visual route planning and visualization
- **Responsive Design**: Modern, mobile-friendly user interface

## 🛠️ Technology Stack

### Frontend
- **React 19** - Modern React with hooks and concurrent features
- **TypeScript** - Type-safe JavaScript development
- **Vite** - Fast build tool and development server
- **Tailwind CSS** - Utility-first CSS framework
- **React Router** - Client-side routing
- **Axios** - HTTP client for API calls
- **Lucide React** - Beautiful icon library

### Backend
- **ASP.NET Core 9.0** - High-performance web framework
- **C#** - Modern C# with nullable reference types
- **Entity Framework Core** - ORM for database operations
- **SQLite** - Lightweight database for development
- **JWT Authentication** - Secure token-based authentication
- **Swagger/OpenAPI** - API documentation
- **AutoMapper** - Object-to-object mapping

### Algorithms & Services
- **Haversine Formula** - Accurate distance calculations between coordinates
- **Route Optimization Algorithm** - Custom TSP-inspired route optimization
- **Shortest Path Algorithm** - Graph-based path finding

## 📋 Prerequisites

- **Node.js** (v18 or higher)
- **.NET 9.0 SDK**
- **Git**

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd RouteWise
```

### 2. Backend Setup

```bash
cd server

# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start the backend server
dotnet run
```

The backend will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7056`
- Swagger UI: `https://localhost:7056/swagger`

### 3. Frontend Setup

```bash
cd client

# Install dependencies
npm install

# Start the development server
npm run dev
```

The frontend will be available at: `http://localhost:5173`

### 4. Environment Configuration

Create `appsettings.Development.json` in the `server` directory:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=RouteWise.db"
  },
  "Jwt": {
    "Secret": "your-super-secret-jwt-key-here",
    "Issuer": "RouteWise",
    "Audience": "RouteWiseUsers"
  }
}
```

## 📖 API Documentation

Once the backend is running, visit `https://localhost:7056/swagger` for interactive API documentation.

### Key Endpoints

- `POST /api/auth/login` - User authentication
- `POST /api/auth/register` - User registration
- `POST /api/route` - Calculate optimized route
- `GET /api/bookmark` - Get user bookmarks
- `POST /api/bookmark` - Save route bookmark

## 🏗️ Project Structure

```
RouteWise/
├── client/                 # React frontend
│   ├── src/
│   │   ├── components/     # React components
│   │   ├── models/         # TypeScript interfaces
│   │   ├── api.ts          # API client functions
│   │   └── authContext.tsx # Authentication context
│   ├── package.json
│   └── vite.config.ts
├── server/                 # ASP.NET Core backend
│   ├── Controllers/        # API controllers
│   ├── Models/            # Entity models
│   ├── DTO/               # Data transfer objects
│   ├── Services/          # Business logic services
│   ├── Data/              # Database context
│   └── appsettings.json   # Configuration
└── README.md
```

## 🔧 Development

### Frontend Scripts

```bash
cd client

# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

### Backend Scripts

```bash
cd server

# Run with hot reload
dotnet watch run

# Run tests
dotnet test

# Create migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update
```

## 🧪 Testing

### Backend Testing
```bash
cd server
dotnet test
```

### Frontend Testing
```bash
cd client
npm test
```

## 🚢 Deployment

### Backend Deployment
```bash
cd server
dotnet publish -c Release -o ./publish
```

### Frontend Deployment
```bash
cd client
npm run build
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Saad Qalib** - *Initial work* - [YourGitHub](https://github.com/1of194)

## 🙏 Acknowledgments

- Route optimization algorithms inspired by TSP solutions
- Haversine formula implementation for geographic calculations
- Modern web development practices and best practices
