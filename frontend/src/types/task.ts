export enum Priority {
  Low = 1,
  Medium = 2,
  High = 3
}

export interface Task {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  userFullName: string;
  userTelephone: string;
  userEmail: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  userFullName: string;
  userTelephone: string;
  userEmail: string;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: Priority;
  userFullName: string;
  userTelephone: string;
  userEmail: string;
}


