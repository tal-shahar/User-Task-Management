import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AppDispatch, RootState } from '../store/store';
import { createTask, updateTask, setSelectedTask } from '../store/taskSlice';
import { Task, CreateTaskDto, UpdateTaskDto, Priority } from '../types/task';

interface TaskFormProps {
  task?: Task | null;
  onSuccess?: () => void;
}

interface FormErrors {
  title?: string;
  description?: string;
  dueDate?: string;
  priority?: string;
}

const TaskForm: React.FC<TaskFormProps> = ({ task, onSuccess }) => {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, error } = useSelector((state: RootState) => state.tasks);

  const [formData, setFormData] = useState<CreateTaskDto>({
    title: '',
    description: '',
    dueDate: new Date().toISOString().split('T')[0],
    priority: Priority.Low,
  });

  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    if (task) {
      setFormData({
        title: task.title,
        description: task.description,
        dueDate: new Date(task.dueDate).toISOString().split('T')[0],
        priority: task.priority,
      });
    }
  }, [task]);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    } else if (formData.title.length > 200) {
      newErrors.title = 'Title must not exceed 200 characters';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    } else if (formData.description.length > 1000) {
      newErrors.description = 'Description must not exceed 1000 characters';
    }

    if (!formData.dueDate) {
      newErrors.dueDate = 'Due date is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      if (task) {
        await dispatch(updateTask({ id: task.id, task: formData as UpdateTaskDto })).unwrap();
      } else {
        await dispatch(createTask(formData)).unwrap();
      }
      dispatch(setSelectedTask(null));
      setFormData({
        title: '',
        description: '',
        dueDate: new Date().toISOString().split('T')[0],
        priority: Priority.Low,
      });
      if (onSuccess) {
        onSuccess();
      }
    } catch (err: any) {
      console.error('Error submitting form:', err);
      console.error('Error response:', err.response?.data);
      console.error('Error details:', err.response?.data?.error);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: name === 'priority' ? parseInt(value, 10) : value,
    }));
    if (errors[name as keyof FormErrors]) {
      setErrors((prev) => ({
        ...prev,
        [name]: undefined,
      }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="task-form">
      <h2>{task ? 'Edit Task' : 'Create New Task'}</h2>

      {error && <div className="error-message">{error}</div>}

      <div className="form-group">
        <label htmlFor="title">Title *</label>
        <input
          type="text"
          id="title"
          name="title"
          value={formData.title}
          onChange={handleChange}
          disabled={loading}
        />
        {errors.title && <span className="error">{errors.title}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="description">Description *</label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          rows={4}
          disabled={loading}
        />
        {errors.description && <span className="error">{errors.description}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="dueDate">Due Date *</label>
        <input
          type="date"
          id="dueDate"
          name="dueDate"
          value={formData.dueDate}
          onChange={handleChange}
          disabled={loading}
        />
        {errors.dueDate && <span className="error">{errors.dueDate}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="priority">Priority *</label>
        <select
          id="priority"
          name="priority"
          value={formData.priority}
          onChange={handleChange}
          disabled={loading}
        >
          <option value={Priority.Low}>Low</option>
          <option value={Priority.Medium}>Medium</option>
          <option value={Priority.High}>High</option>
        </select>
        {errors.priority && <span className="error">{errors.priority}</span>}
      </div>

      <div className="form-actions">
        <button type="submit" disabled={loading}>
          {loading ? 'Saving...' : task ? 'Update Task' : 'Create Task'}
        </button>
        {task && (
          <button
            type="button"
            onClick={() => dispatch(setSelectedTask(null))}
            disabled={loading}
          >
            Cancel
          </button>
        )}
      </div>
    </form>
  );
};

export default TaskForm;

