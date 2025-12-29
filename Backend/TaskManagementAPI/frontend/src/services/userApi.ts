import { User, CreateUserDto, UpdateUserDto } from '../types/user';
import { apiClient } from './authApi';

export const userApi = {
  getAllUsers: async (): Promise<User[]> => {
    const response = await apiClient.get<User[]>('/user');
    return response.data;
  },

  getUserById: async (id: number): Promise<User> => {
    const response = await apiClient.get<User>(`/user/${id}`);
    return response.data;
  },

  createUser: async (user: CreateUserDto): Promise<User> => {
    const response = await apiClient.post<User>('/user', user);
    return response.data;
  },

  updateUser: async (id: number, user: UpdateUserDto): Promise<User> => {
    const response = await apiClient.put<User>(`/user/${id}`, user);
    return response.data;
  },

  deleteUser: async (id: number): Promise<void> => {
    await apiClient.delete(`/user/${id}`);
  },
};

