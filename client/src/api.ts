import axios from "axios";
import { BACKEND_API_KEY } from "./HomePage";
import { PageType, type LoginResponse } from "./models/UserModel";


const api = axios.create({
    baseURL: `${BACKEND_API_KEY}`,
    withCredentials: true,
    timeout: 15000,
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json'
    }
});




api.interceptors.request.use(options => {
    const token = sessionStorage.getItem('token');
    if (token) {
        options.headers.Authorization = `Bearer ${token}`;
    }

    

    return options;
});

api.interceptors.response.use(
    response => response,
    async error => {
      const originalRequest = error.config;
      if (error.response?.status === 401 && !originalRequest._retry ) {
        // Redirect to login or refresh token
       originalRequest._retry = true;

       try {
        const reFreshResponse = await api.post('/refresh-token', {
        },
      {
        withCredentials: true
      });
        
        if (reFreshResponse.status === 200) {
          sessionStorage.setItem('token', reFreshResponse.data.accessToken);
          sessionStorage.setItem('login-access', 'true')
          originalRequest.headers.Authorization = `Bearer ${reFreshResponse.data.accessToken}`;
          return api(originalRequest);
        }



       } catch (refreshError) {
        console.error("Token refresh failed", refreshError);
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('login-access')
       
        window.location.href = '/login';
        
       }

        
      }
      return Promise.reject(error);
    }
  );


  export const login = async (
    username: string,
    password: string,
    pageType: PageType
  ): Promise<LoginResponse> => {
    const loginApi = axios.create({
      baseURL: `${BACKEND_API_KEY}`,
      timeout: 15000,
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
      });
  
      // Normalize server response to client shape
      const data = response.data;
      const normalized = {
        Message: data.message ?? data.Message, // server uses 'message'
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

  
  export const logout = async () => {
    try {
      
      const response = await api.post('/logout');
      if (response.status === 200) {
        sessionStorage.removeItem('token');
        return response.data.message;
      }
    } catch (error) {
      console.error("Logout failed", error);
      // Even if logout fails on server, clear session storage
      sessionStorage.removeItem('token');
      return "Logged out successfully";
    }
  };