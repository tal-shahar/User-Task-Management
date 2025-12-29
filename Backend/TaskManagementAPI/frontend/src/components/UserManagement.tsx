import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AppDispatch, RootState } from '../store/store';
import { logout } from '../store/authSlice';
import { setAuthToken } from '../services/authApi';
import { userApi } from '../services/userApi';
import { User, CreateUserDto, UpdateUserDto, Role } from '../types/user';
import UserForm from './UserForm';
import './UserManagement.css';

const UserManagement: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { user: currentUser } = useSelector((state: RootState) => state.auth);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  useEffect(() => {
    fetchUsers();
  }, []);

  const fetchUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await userApi.getAllUsers();
      setUsers(data);
    } catch (err: any) {
      setError(err.response?.data || 'Failed to fetch users');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setSelectedUser(null);
    setShowForm(true);
  };

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setShowForm(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this user?')) {
      return;
    }

    try {
      await userApi.deleteUser(id);
      await fetchUsers();
    } catch (err: any) {
      setError(err.response?.data || 'Failed to delete user');
    }
  };

  const handleLogout = () => {
    dispatch(logout());
    setAuthToken(null);
  };

  const handleFormSuccess = () => {
    setShowForm(false);
    setSelectedUser(null);
    fetchUsers();
  };

  const getRoleLabel = (role: Role): string => {
    return role === Role.Admin ? 'Admin' : 'User';
  };

  if (showForm) {
    return (
      <div>
        <UserForm user={selectedUser} onSuccess={handleFormSuccess} onCancel={() => setShowForm(false)} />
      </div>
    );
  }

  return (
    <div className="user-management-container">
      <div className="user-management-header">
        <h1>User Management</h1>
        <div className="header-actions">
          <span className="current-user">Logged in as: {currentUser?.username} ({getRoleLabel(currentUser?.role || Role.User)})</span>
          <button onClick={handleCreate} className="create-button">
            Create New User
          </button>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {loading ? (
        <div className="loading">Loading users...</div>
      ) : (
        <table className="users-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Full Name</th>
              <th>Role</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.id}</td>
                <td>{user.username}</td>
                <td>{user.email}</td>
                <td>{user.fullName}</td>
                <td>
                  <span className={`role-badge ${user.role === Role.Admin ? 'admin' : 'user'}`}>
                    {getRoleLabel(user.role)}
                  </span>
                </td>
                <td>
                  <span className={`status-badge ${user.isActive ? 'active' : 'inactive'}`}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td>
                  <button onClick={() => handleEdit(user)} className="edit-button">
                    Edit
                  </button>
                  <button onClick={() => handleDelete(user.id)} className="delete-button" disabled={user.username === currentUser?.username}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default UserManagement;

