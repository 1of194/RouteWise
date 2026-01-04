import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios'; // Add this import for the refresh call
import type { User } from './models/UserModel';
import { BACKEND_API_KEY } from './api';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  loginAction: (userData: User, token: string) => void;
  logoutAction: () => void;
}



const AuthContext = createContext<AuthContextType | undefined>(undefined);

/**
 * This manages the global state of the user session.
 */
export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  /**
   * Runs once when the app starts/refreshes to check if the user is already logged in.
   */
  useEffect(() => {
    const initializeAuth = async () => {
      const token = sessionStorage.getItem('token');
      const savedUser = sessionStorage.getItem('user');

      if (token && savedUser) {
        // Token exists, set authenticated
        setIsAuthenticated(true);
        setUser(JSON.parse(savedUser));
        setIsLoading(false);
      } else {
        // No token in storage, try to refresh using the cookie
        try {
            // Stops refresh if user is already on public pages
            if (window.location.pathname !== '/login' && window.location.pathname !== '/register'){
          const response = await axios.post(
            `${BACKEND_API_KEY}/refresh-token`,
            {},
            { withCredentials: true } // Sends the refreshToken cookie
          );
          // Recovery successful: Set the new access token
          if (response.status === 200 && response.data.AccessToken) {
           
            sessionStorage.setItem('token', response.data.AccessToken);
            setIsAuthenticated(true);
      
          } else {
            // Refresh failed, stay unauthenticated
            setIsAuthenticated(false);
          }
        }
        } catch (error) {
          console.error('Token refresh on load failed:', error);
          setIsAuthenticated(false);
        }
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const loginAction = (userData: User, token: string) => {
    sessionStorage.setItem('token', token);
    sessionStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
    setIsAuthenticated(true);
  };

  const logoutAction = () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('login-access');
    setUser(null);
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, isLoading, loginAction, logoutAction }}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook to use the context easily in any component
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};