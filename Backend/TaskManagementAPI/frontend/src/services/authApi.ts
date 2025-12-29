import axios from 'axios';
import { LoginRequest, LoginResponse } from '../types/user';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5263/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },
};

export { apiClient };

// Add token to requests
export const setAuthToken = (token: string | null) => {
  if (token) {
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    localStorage.setItem('token', token);
  } else {
    delete apiClient.defaults.headers.common['Authorization'];
    localStorage.removeItem('token');
  }
};

// Get token from localStorage
export const getAuthToken = (): string | null => {
  return localStorage.getItem('token');
};

// Initialize token on app load
const token = getAuthToken();
if (token) {
  setAuthToken(token);
}

