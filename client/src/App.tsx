import React, { Suspense } from 'react';
import './index.css';
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import { AuthProvider, useAuth } from './authContext';
// React.lazy takes a function that returns a promise (dynamic import)
// and converts it into a loadable component
const HomePage = React.lazy(() => import('./HomePage'));
const Login = React.lazy(() => import('./Login'));


/**
 * ProtectedRoute Component
 * Acts as a security "gatekeeper" for specific routes.
 * It checks the global auth state before allowing access to children.
 */
interface ProtectedRouteProps {
  children: React.ReactNode;
}

function ProtectedRoute({ children}:ProtectedRouteProps) {
  const { isLoading,isAuthenticated} = useAuth();

  //  While AuthProvider is checking sessionStorage or cookies for a token,
  // we must show a loading state.
   if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-black"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    // Redirect to login if not authenticated
    return <Navigate to="/login" replace />;
  }
  // If authenticated, render the actual page
  return children;
}

function App() {

 return (
  // AuthProvider must wrap everything so useAuth() works in any route
  <AuthProvider>
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
              <ProtectedRoute>
                <HomePage />
              </ProtectedRoute>
            </Suspense>
          }
        />
        <Route
          path="/login"
          element={
            <Suspense fallback={<div>Loading....</div>}>
             <Login />
            </Suspense>
          }
        />
      </Routes>
    </BrowserRouter>
    </AuthProvider>
  );
}

export default App;