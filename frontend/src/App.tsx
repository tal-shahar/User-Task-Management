import React from 'react';
import { Provider } from 'react-redux';
import { store } from './store/store';
import TaskList from './components/TaskList';
import './App.css';

function App() {
  return (
    <Provider store={store}>
      <div className="App">
        <TaskList />
      </div>
    </Provider>
  );
}

export default App;


