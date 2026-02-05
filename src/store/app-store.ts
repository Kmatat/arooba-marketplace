/**
 * ============================================================
 * AROOBA MARKETPLACE — Global State Store (Zustand)
 * ============================================================
 * 
 * Centralized state management using Zustand.
 * Each "slice" maps to a business module.
 * ============================================================
 */

import { create } from 'zustand';
import type { User, UserRole } from '../app/shared/types';

// ──────────────────────────────────────────────
// APP-WIDE UI STATE
// ──────────────────────────────────────────────

interface AppState {
  // Sidebar
  isSidebarOpen: boolean;
  toggleSidebar: () => void;
  
  // Language
  language: 'ar' | 'en';
  setLanguage: (lang: 'ar' | 'en') => void;
  
  // Active section
  activeSection: string;
  setActiveSection: (section: string) => void;
  
  // Current user (simulated auth)
  currentUser: User | null;
  currentRole: UserRole;
  setCurrentRole: (role: UserRole) => void;
  
  // Notifications
  notifications: Notification[];
  addNotification: (notification: Omit<Notification, 'id' | 'createdAt'>) => void;
  dismissNotification: (id: string) => void;
}

interface Notification {
  id: string;
  type: 'success' | 'warning' | 'error' | 'info';
  title: string;
  message: string;
  createdAt: string;
}

export const useAppStore = create<AppState>((set) => ({
  isSidebarOpen: true,
  toggleSidebar: () => set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
  
  language: 'ar',
  setLanguage: (lang) => set({ language: lang }),
  
  activeSection: 'dashboard',
  setActiveSection: (section) => set({ activeSection: section }),
  
  currentUser: {
    id: 'admin-001',
    mobileNumber: '+201001234567',
    email: 'admin@aroobh.com',
    fullName: 'كريم مطاط',
    role: 'admin_super',
    isVerified: true,
    createdAt: '2025-10-01T00:00:00Z',
    updatedAt: '2025-12-03T00:00:00Z',
  },
  currentRole: 'admin_super',
  setCurrentRole: (role) => set({ currentRole: role }),
  
  notifications: [],
  addNotification: (notification) =>
    set((state) => ({
      notifications: [
        {
          ...notification,
          id: `notif-${Date.now()}`,
          createdAt: new Date().toISOString(),
        },
        ...state.notifications,
      ],
    })),
  dismissNotification: (id) =>
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    })),
}));
