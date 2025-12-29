import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AppDispatch, RootState } from '../store/store';
import { fetchTasks, deleteTask, setSelectedTask } from '../store/taskSlice';
import { Task, Priority } from '../types/task';
import TaskForm from './TaskForm';

const TaskList: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { tasks, loading, error, selectedTask } = useSelector((state: RootState) => state.tasks);
  const [showForm, setShowForm] = useState(false);
  const [filterPriority, setFilterPriority] = useState<Priority | 'all'>('all');
  const [sortBy, setSortBy] = useState<'dueDate' | 'priority'>('dueDate');

  useEffect(() => {
    dispatch(fetchTasks());
  }, [dispatch]);

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await dispatch(deleteTask(id)).unwrap();
      } catch (err) {
        console.error('Error deleting task:', err);
      }
    }
  };

  const handleEdit = (task: Task) => {
    dispatch(setSelectedTask(task));
    setShowForm(true);
  };

  const handleCreateNew = () => {
    dispatch(setSelectedTask(null));
    setShowForm(true);
  };

  const handleFormSuccess = () => {
    setShowForm(false);
    dispatch(fetchTasks());
  };

  const filteredAndSortedTasks = [...tasks]
    .filter((task) => filterPriority === 'all' || task.priority === filterPriority)
    .sort((a, b) => {
      if (sortBy === 'dueDate') {
        return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      } else {
        return b.priority - a.priority;
      }
    });

  const getPriorityLabel = (priority: Priority): string => {
    switch (priority) {
      case Priority.Low:
        return 'Low';
      case Priority.Medium:
        return 'Medium';
      case Priority.High:
        return 'High';
      default:
        return 'Unknown';
    }
  };

  const getPriorityClass = (priority: Priority): string => {
    switch (priority) {
      case Priority.Low:
        return 'priority-low';
      case Priority.Medium:
        return 'priority-medium';
      case Priority.High:
        return 'priority-high';
      default:
        return '';
    }
  };

  if (showForm) {
    return (
      <div>
        <TaskForm task={selectedTask} onSuccess={handleFormSuccess} />
        <button onClick={() => setShowForm(false)} className="back-button">
          Back to List
        </button>
      </div>
    );
  }

  return (
    <div className="task-list-container">
      <div className="task-list-header">
        <h1>Task Management</h1>
        <button onClick={handleCreateNew} className="create-button">
          Create New Task
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="filters">
        <div className="filter-group">
          <label htmlFor="filterPriority">Filter by Priority:</label>
          <select
            id="filterPriority"
            value={filterPriority}
            onChange={(e) => setFilterPriority(e.target.value as Priority | 'all')}
          >
            <option value="all">All</option>
            <option value={Priority.Low}>Low</option>
            <option value={Priority.Medium}>Medium</option>
            <option value={Priority.High}>High</option>
          </select>
        </div>

        <div className="filter-group">
          <label htmlFor="sortBy">Sort by:</label>
          <select
            id="sortBy"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value as 'dueDate' | 'priority')}
          >
            <option value="dueDate">Due Date</option>
            <option value="priority">Priority</option>
          </select>
        </div>
      </div>

      {loading ? (
        <div className="loading">Loading tasks...</div>
      ) : filteredAndSortedTasks.length === 0 ? (
        <div className="no-tasks">No tasks found.</div>
      ) : (
        <div className="task-grid">
          {filteredAndSortedTasks.map((task) => (
            <div key={task.id} className="task-card">
              <div className="task-header">
                <h3>{task.title}</h3>
                <span className={`priority-badge ${getPriorityClass(task.priority)}`}>
                  {getPriorityLabel(task.priority)}
                </span>
              </div>
              <p className="task-description">{task.description}</p>
              <div className="task-details">
                <p><strong>Due Date:</strong> {new Date(task.dueDate).toLocaleDateString()}</p>
                <p><strong>User:</strong> {task.userFullName}</p>
                <p><strong>Email:</strong> {task.userEmail}</p>
                <p><strong>Phone:</strong> {task.userTelephone}</p>
              </div>
              <div className="task-actions">
                <button onClick={() => handleEdit(task)} className="edit-button">
                  Edit
                </button>
                <button onClick={() => handleDelete(task.id)} className="delete-button">
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default TaskList;


