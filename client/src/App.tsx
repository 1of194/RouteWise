import React, { Suspense, useState } from 'react';
import './index.css';
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
// React.lazy takes a function that returns a promise (dynamic import)
// and converts it into a loadable component
const HomePage = React.lazy(() => import('./HomePage'));
const Login = React.lazy(() => import('./Login'));

interface ProtectedRouteProps {
  isAuthenticated: boolean;
  children: React.ReactNode;
}

function ProtectedRoute({ isAuthenticated, children}:ProtectedRouteProps) {
  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" replace />;
  }
  return children;
}

function App() {
   const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => {
    return !!sessionStorage.getItem("accessToken"); // true if token exists
  });
 return (
  <BrowserRouter>
                {/* 
          path = URL path 
          element = what should render for that path
          Suspense shows the fallback UI until the lazy-loaded component finishes loading
        */}
    
      <Routes>
        <Route
          path="/"
          element={
            <Suspense fallback={<div>Loading....</div>}>
              <ProtectedRoute isAuthenticated={isAuthenticated}>
                <HomePage />
              </ProtectedRoute>
            </Suspense>
          }
        />
        <Route
          path="/login"
          element={
            <Suspense fallback={<div>Loading....</div>}>
             <Login
              onRegister={(authenticated) => setIsAuthenticated(authenticated)}
              onLogin={(authenticated) => setIsAuthenticated(authenticated)}
/>
            </Suspense>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;