/**
 * ============================================================
 * AROOBA MARKETPLACE — Admin Dashboard Layout
 * ============================================================
 * 
 * The main layout shell with sidebar navigation.
 * Supports RTL (Arabic) and LTR (English) layouts.
 * Mobile-responsive with collapsible sidebar.
 * ============================================================
 */

import React from 'react';
import { useAppStore } from '../store/app-store';
import { PLATFORM } from '../config/constants';

// Navigation items mapped to business modules
const NAV_SECTIONS = [
  {
    label: 'الرئيسية',
    labelEn: 'Main',
    items: [
      { id: 'dashboard', labelAr: 'لوحة التحكم', labelEn: 'Dashboard', icon: '📊' },
      { id: 'user-analytics', labelAr: 'تحليلات المستخدمين', labelEn: 'User Analytics', icon: '📈' },
    ],
  },
  {
    label: 'إدارة الأعمال',
    labelEn: 'Business Management',
    items: [
      { id: 'vendors', labelAr: 'الموردون', labelEn: 'Vendors', icon: '🏪' },
      { id: 'products', labelAr: 'المنتجات', labelEn: 'Products', icon: '📦' },
      { id: 'orders', labelAr: 'الطلبات', labelEn: 'Orders', icon: '🛒' },
      { id: 'customers', labelAr: 'العملاء', labelEn: 'Customers', icon: '👥' },
    ],
  },
  {
    label: 'المالية والعمليات',
    labelEn: 'Finance & Operations',
    items: [
      { id: 'finance', labelAr: 'المالية', labelEn: 'Finance', icon: '💰' },
      { id: 'logistics', labelAr: 'اللوجستيات', labelEn: 'Logistics', icon: '🚚' },
      { id: 'pricing', labelAr: 'التسعير', labelEn: 'Pricing Engine', icon: '🧮' },
    ],
  },
  {
    label: 'التحكم والموافقات',
    labelEn: 'Control & Approvals',
    items: [
      { id: 'approvals', labelAr: 'طابور الموافقات', labelEn: 'Approval Queue', icon: '📋' },
      { id: 'audit', labelAr: 'سجل المراجعة', labelEn: 'Audit Trail', icon: '📜' },
    ],
  },
  {
    label: 'الأدوات',
    labelEn: 'Tools',
    items: [
      { id: 'monitoring', labelAr: 'المراقبة', labelEn: 'Monitoring', icon: '📊' },
      { id: 'platform-config', labelAr: 'إعدادات المنصة', labelEn: 'Platform Config', icon: '⚙️' },
      { id: 'settings', labelAr: 'الإعدادات العامة', labelEn: 'Settings', icon: '🔧' },
    ],
  },
];

interface DashboardLayoutProps {
  children: React.ReactNode;
}

export function DashboardLayout({ children }: DashboardLayoutProps) {
  const { isSidebarOpen, toggleSidebar, activeSection, setActiveSection, language, setLanguage } = useAppStore();

  return (
    <div className="flex h-screen overflow-hidden bg-earth-50">
      {/* Sidebar */}
      <aside
        className={`
          fixed lg:static inset-y-0 right-0 z-40
          flex flex-col
          bg-white border-l border-earth-200
          transition-all duration-300 ease-in-out
          ${isSidebarOpen ? 'w-[280px]' : 'w-0 lg:w-[72px]'}
          overflow-hidden
        `}
      >
        {/* Logo */}
        <div className="flex items-center gap-3 px-5 h-16 border-b border-earth-100 shrink-0">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-arooba-400 to-arooba-600 flex items-center justify-center text-white font-bold text-lg shrink-0">
            أ
          </div>
          {isSidebarOpen && (
            <div className="animate-fade-in">
              <p className="font-bold text-earth-900 font-display leading-tight">{PLATFORM.name}</p>
              <p className="text-[10px] text-earth-400">{PLATFORM.taglineEn}</p>
            </div>
          )}
        </div>

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto py-4 px-3">
          {NAV_SECTIONS.map((section) => (
            <div key={section.label} className="mb-5">
              {isSidebarOpen && (
                <p className="text-[10px] font-semibold text-earth-400 uppercase tracking-wider px-3 mb-2">
                  {language === 'ar' ? section.label : section.labelEn}
                </p>
              )}
              <div className="space-y-0.5">
                {section.items.map((item) => {
                  const isActive = activeSection === item.id;
                  return (
                    <button
                      key={item.id}
                      onClick={() => setActiveSection(item.id)}
                      className={`
                        w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium
                        transition-all duration-200
                        ${isActive
                          ? 'bg-arooba-50 text-arooba-700 shadow-sm'
                          : 'text-earth-600 hover:bg-earth-50 hover:text-earth-800'
                        }
                      `}
                      title={language === 'ar' ? item.labelAr : item.labelEn}
                    >
                      <span className="text-lg shrink-0">{item.icon}</span>
                      {isSidebarOpen && (
                        <span className="animate-fade-in">
                          {language === 'ar' ? item.labelAr : item.labelEn}
                        </span>
                      )}
                      {isActive && isSidebarOpen && (
                        <span className="mr-auto w-1.5 h-1.5 rounded-full bg-arooba-500" />
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>

        {/* Language Toggle */}
        {isSidebarOpen && (
          <div className="px-4 py-3 border-t border-earth-100 shrink-0">
            <button
              onClick={() => setLanguage(language === 'ar' ? 'en' : 'ar')}
              className="w-full flex items-center justify-center gap-2 py-2 text-xs text-earth-500 hover:text-earth-700 rounded-lg hover:bg-earth-50 transition-colors"
            >
              🌐 {language === 'ar' ? 'English' : 'عربي'}
            </button>
          </div>
        )}
      </aside>

      {/* Mobile overlay */}
      {isSidebarOpen && (
        <div
          className="fixed inset-0 bg-black/20 z-30 lg:hidden"
          onClick={toggleSidebar}
        />
      )}

      {/* Main Content */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Top Header */}
        <header className="flex items-center justify-between px-6 h-16 bg-white border-b border-earth-200 shrink-0">
          <div className="flex items-center gap-3">
            <button
              onClick={toggleSidebar}
              className="p-2 rounded-lg hover:bg-earth-100 transition-colors text-earth-500"
            >
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path d="M3 5h14M3 10h14M3 15h14" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
              </svg>
            </button>
            <div>
              <h1 className="text-lg font-bold text-earth-900 font-display">
                {NAV_SECTIONS.flatMap(s => s.items).find(i => i.id === activeSection)?.[language === 'ar' ? 'labelAr' : 'labelEn'] || 'لوحة التحكم'}
              </h1>
            </div>
          </div>

          <div className="flex items-center gap-3">
            {/* Notification Bell */}
            <button className="relative p-2 rounded-lg hover:bg-earth-100 transition-colors text-earth-500">
              <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                <path d="M15 6.667a5 5 0 00-10 0c0 5.833-2.5 7.5-2.5 7.5h15S15 12.5 15 6.667M11.442 16.667a1.667 1.667 0 01-2.884 0" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              <span className="absolute top-1 left-1 w-2 h-2 bg-red-500 rounded-full" />
            </button>

            {/* User Avatar */}
            <div className="flex items-center gap-2 pr-3 border-r border-earth-200">
              <div className="w-8 h-8 rounded-full bg-arooba-100 text-arooba-600 flex items-center justify-center text-sm font-bold">
                ك
              </div>
              <div className="hidden sm:block">
                <p className="text-xs font-medium text-earth-700">كريم مطاط</p>
                <p className="text-[10px] text-earth-400">مدير النظام</p>
              </div>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {children}
        </div>
      </main>
    </div>
  );
}
