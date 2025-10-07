# TodoApp Frontend

A modern Todo application built with React, ShadcnUI components, and Tailwind CSS.

## 🚀 Features

- ✅ **User Authentication** - Login/Register with JWT
- ✅ **Todo Management** - Create, read, update, delete todos
- ✅ **Modern UI** - Clean interface using ShadcnUI components
- ✅ **Responsive Design** - Works on all devices
- ✅ **Real-time Updates** - Instant UI updates
- ✅ **Progress Tracking** - Visual progress bar and statistics
- ✅ **Form Validation** - Client-side validation with error handling

## 🛠️ Tech Stack

- **React 19** - Frontend framework
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework
- **ShadcnUI** - Modern React components
- **Axios** - HTTP client for API calls
- **Lucide React** - Beautiful icons

## 📋 Prerequisites

Make sure you have the following installed:

- **Node.js** version 20.19+ or 22.12+
- **npm** or **yarn**

## 🏗️ Installation

1. **Navigate to the frontend directory:**
   ```bash
   cd todoapp
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Update API URL:**
   Edit `src/services/api.js` and update the `API_BASE_URL` to match your backend server:
   ```javascript
   const API_BASE_URL = 'https://localhost:7055'; // Your backend URL
   ```

4. **Start the development server:**
   ```bash
   npm run dev
   ```

5. **Open your browser:**
   Navigate to `http://localhost:5173`

## 📁 Project Structure

```
src/
├── components/
│   ├── ui/                  # ShadcnUI components
│   │   ├── button.jsx
│   │   ├── input.jsx
│   │   ├── card.jsx
│   │   └── checkbox.jsx
│   ├── AuthWrapper.jsx      # Authentication wrapper
│   ├── LoginForm.jsx        # Login component
│   ├── RegisterForm.jsx     # Registration component
│   └── TodoApp.jsx          # Main todo application
├── contexts/
│   └── AuthContext.jsx      # Authentication context
├── services/
│   └── api.js              # API service layer
├── lib/
│   └── utils.js            # Utility functions
├── App.jsx                 # Main app component
├── App.css                 # Global styles with Tailwind
├── index.css               # Base Tailwind imports
└── main.jsx               # App entry point
```

## 🎨 UI Components

### Login/Register Forms
- Clean, card-based design
- Form validation with error messages
- Loading states during API calls
- Easy switching between login and register

### Todo Interface
- **Header** - Shows user info, completion stats, and logout button
- **Add Todo Form** - Simple input with add button
- **Todo List** - Interactive todo items with checkboxes and delete buttons
- **Progress Section** - Visual progress bar and statistics

### Key Features
- **Responsive Design** - Works on mobile and desktop
- **Loading States** - Smooth loading indicators
- **Error Handling** - User-friendly error messages
- **Form Validation** - Real-time validation feedback

## 🔧 Configuration

### Backend API Integration
The frontend is configured to work with the .NET backend. Make sure to:

1. **Update API URL** in `src/services/api.js`
2. **Configure CORS** in your backend to allow requests from `http://localhost:5173`

### Authentication Flow
- JWT tokens are stored in localStorage
- Automatic token inclusion in API requests
- Redirect to login on token expiration
- User context managed globally

## 🧪 Available Scripts

```bash
# Development server
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## 🎯 Usage

### Authentication
1. **Register** - Create a new account with username, email, and password
2. **Login** - Sign in with email and password
3. **Logout** - Click the logout button in the header

### Todo Management
1. **Add Todo** - Type in the input field and click "Add" or press Enter
2. **Complete Todo** - Click the checkbox to mark as complete
3. **Delete Todo** - Click the trash icon to delete
4. **View Progress** - Check the progress bar and statistics

## 🚨 Troubleshooting

### Node.js Version Error
If you get a Node.js version error, upgrade to Node.js 20.19+ or 22.12+:
- Download from [nodejs.org](https://nodejs.org/)
- Or use nvm: `nvm install 20.19.0`

### API Connection Issues
- Make sure your backend server is running
- Check the API_BASE_URL in `src/services/api.js`
- Verify CORS configuration in your backend

### Build Issues
- Clear node_modules: `rm -rf node_modules && npm install`
- Clear Vite cache: `rm -rf .vite && npm run dev`

## 📝 API Endpoints

The frontend expects the following API endpoints:

```
POST   /api/auth/login      - User login
POST   /api/auth/register   - User registration
GET    /api/todos           - Get user's todos
POST   /api/todos           - Create new todo
PUT    /api/todos/:id       - Update todo
DELETE /api/todos/:id       - Delete todo
PATCH  /api/todos/:id/toggle - Toggle todo completion
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit changes: `git commit -m 'Add feature'`
4. Push to branch: `git push origin feature-name`
5. Submit a pull request

---

**Happy Coding!** 🎉

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.
