import axios from "axios";

import { PageType, type LoginResponse } from "./models/UserModel";


// Base URL for the backend service
export const BACKEND_API_KEY = 'http://localhost:5000/api';

const api = axios.create({
    baseURL: `${BACKEND_API_KEY}`,
    withCredentials: true,
    timeout: 15000,
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json'
    }
});


/**
 * REQUEST INTERCEPTOR
 * Runs before every request leaves the browser.
 * It checks sessionStorage for an Access Token and attaches it to the Authorization header.
 */
api.interceptors.request.use(options => {
    const token = sessionStorage.getItem('token');
    if (token) {
        options.headers.Authorization = `Bearer ${token}`;
    }
    return options;
});


/**
 * RESPONSE INTERCEPTOR (The "Silent Refresh" Logic)
 * Runs after every response comes back.
 * If a 401 (Unauthorized) error occurs, it tries to refresh the token automatically.
 */
api.interceptors.response.use(
    response => response, // Pass successful responses through
    async error => {
      const originalRequest = error.config;
      // Avoid refreshing if already on login or register pages
      if (error.response?.status === 401 && !originalRequest._retry && window.location.pathname !== '/login' && window.location.pathname !== '/register') {
  
       originalRequest._retry = true;  // Mark as retried to prevent infinite loops

       try {
        // Attempt to get a new Access Token using the Refresh Token cookie
        const reFreshResponse = await api.post('/refresh-token', {},
        {
          withCredentials: true
        });
        
        if (reFreshResponse.status === 200) {
          // 1. Store the new token
          sessionStorage.setItem('token', reFreshResponse.data.AccessToken);
          sessionStorage.setItem('login-access', 'true')

          // 2. Update the header for the original failed request
          originalRequest.headers.Authorization = `Bearer ${reFreshResponse.data.AccessToken}`;
          // 3. Re-run the original request with the new token
          return api(originalRequest);
        }
       } catch (refreshError) {
        // If refresh fails, the user must log in again
        console.error("Token refresh failed", refreshError);
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('login-access')
       
        window.location.href = '/login';
        
       }
      }
      return Promise.reject(error);
    }
  );

/**
 * LOGIN / REGISTER API
 * Uses a separate axios instance to avoid the global interceptors during the login process.
 * pageType 1 = Login, 2 = Registration
 */
  export const login = async (
    username: string,
    password: string,
    pageType: PageType
  ): Promise<LoginResponse> => {
    const loginApi = axios.create({
      baseURL: `${BACKEND_API_KEY}`,
      timeout: 15000,
      withCredentials:true,
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
    });
  
    try {
      const response = await loginApi.post('/login', {
        Username: username,
        Password: password,
        PageType: pageType,
      }, );
  
      // Normalize server response to client shape
      const data = response.data;
      const normalized = {
        Message: data.message ?? data.Message, // server uses 'message'
        user:data.user ?? data.User,
        accessToken: data.accessToken, // server uses PascalCase
        
      };
  
      if (normalized.accessToken) {
        sessionStorage.setItem('token', normalized.accessToken);
      }
  
      return normalized;
    } catch (error: any) {
      const msg =
        error?.response?.data?.message ||
        error?.message ||
        'Login failed';
      // Throw so caller can decide success vs error UI state
      throw new Error(msg);
    }
  };

  /**
 * LOGOUT API
 * Notifies the server to clear the HttpOnly Refresh Token cookie.
 */
  
  export const logout = async () => {
    try {
      
      const response = await api.post('/logout', {});
      if (response.status === 200) {
        return response.data.message;
      }
    } catch (error) {
      console.error("Logout failed", error);
      // Even if logout fails on server, clear session storage
      return "Logged out successfully";
    }
  };