import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import { store } from './store/store';
import { getAuthToken } from './services/authApi';
import { setAuthToken } from './services/authApi';

// Initialize auth token if it exists in localStorage
const token = getAuthToken();
if (token) {
  setAuthToken(token);
}

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
