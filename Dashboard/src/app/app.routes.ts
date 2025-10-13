import { Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { TaskManager } from './task-manager/task-manager';

export const routes: Routes = [
    {path: '', redirectTo:'dashboard', pathMatch:'full'},
    {path: 'dashboard', component: Dashboard},
    {path: 'tasks', component: TaskManager},
];
