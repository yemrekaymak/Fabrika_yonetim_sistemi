import React, { useState, useEffect } from 'react';
import './App.css';
import Sidebar from 'components/Sidebar/Sidebar';
import LoginScreen from 'components/LoginScreen/LoginScreen';
import RegisterScreen from 'components/RegisterScreen/RegisterScreen';
import LoadingScreen from 'components/LoadingScreen/LoadingScreen';
import { MainContent } from 'components/Layout';
import { ToastProvider } from 'contexts/ToastContext';
import { getStoredUser, logout as apiLogout } from 'services/authService';

const REMEMBER_KEY = 'fabrika_remember';
const THEME_KEY = 'fabrika_theme';

function getInitialTheme() {
  if (typeof window === 'undefined') return true;
  if (!localStorage.getItem(REMEMBER_KEY)) return true;
  return localStorage.getItem(THEME_KEY) !== 'light';
}

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(() => typeof window !== 'undefined' && !!localStorage.getItem(REMEMBER_KEY));
  const [authView, setAuthView] = useState('login');
  const [registerSuccessMessage, setRegisterSuccessMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [loadingStartTime, setLoadingStartTime] = useState(null);
  const [activeTab, setActiveTab] = useState('dashboard');
  const [isDark, setIsDark] = useState(getInitialTheme);
  const [userName, setUserName] = useState(() => {
    const u = getStoredUser();
    if (!u) return '';
    const full = [u.ad, u.soyad].filter(Boolean).join(' ').trim();
    return full || u.email || '';
  });

  const MIN_LOADING_MS = 400; // Çok hızlı PC'de yükleme ekranının çok kısa görünmesini engeller

  const handleContentReady = () => {
    if (!loadingStartTime) return;
    const elapsed = Date.now() - loadingStartTime;
    const remaining = Math.max(0, MIN_LOADING_MS - elapsed);
    if (remaining > 0) {
      setTimeout(() => {
        setIsLoading(false);
        setLoadingStartTime(null);
      }, remaining);
    } else {
      setIsLoading(false);
      setLoadingStartTime(null);
    }
  };

  useEffect(() => {
    if (typeof document === 'undefined') return;
    if (isDark) document.documentElement.classList.add('dark');
    else document.documentElement.classList.remove('dark');
  }, [isDark]);

  const toggleTheme = () => {
    setIsDark((prev) => {
      const next = !prev;
      if (typeof window !== 'undefined' && localStorage.getItem(REMEMBER_KEY)) {
        localStorage.setItem(THEME_KEY, next ? 'dark' : 'light');
      }
      return next;
    });
  };

  const handleLogin = (rememberMe) => {
    setRegisterSuccessMessage('');
    if (rememberMe) localStorage.setItem(REMEMBER_KEY, '1');
    else localStorage.removeItem(REMEMBER_KEY);
    const u = getStoredUser();
    const full = u ? [u.ad, u.soyad].filter(Boolean).join(' ').trim() : '';
    setUserName(full || u?.email || '');
    setLoadingStartTime(Date.now());
    setIsLoading(true);
    setIsLoggedIn(true);
  };

  const handleRegisterSuccess = () => {
    setRegisterSuccessMessage('Hesabınız oluşturuldu. Giriş yapabilirsiniz.');
    setAuthView('login');
  };

  const handleLogout = () => {
    localStorage.removeItem(REMEMBER_KEY);
    setUserName('');
    try { apiLogout(); } catch {}
    setIsLoading(true);
    setTimeout(() => {
      setIsLoggedIn(false);
      setIsLoading(false);
      setActiveTab('dashboard');
    }, 1000);
  };

  if (!isLoggedIn) {
    if (authView === 'register') {
      return (
        <RegisterScreen
          onSuccess={handleRegisterSuccess}
          onBack={() => setAuthView('login')}
        />
      );
    }
    return (
      <LoginScreen
        onLogin={handleLogin}
        onGoToRegister={() => { setRegisterSuccessMessage(''); setAuthView('register'); }}
        successMessage={registerSuccessMessage}
      />
    );
  }

  return (
    <ToastProvider>
      <div className="font-sans relative">
        <Sidebar activeTab={activeTab} setActiveTab={setActiveTab} onLogout={handleLogout} />
        <MainContent
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        isDark={isDark}
        toggleTheme={toggleTheme}
        userName={userName}
        onContentReady={isLoading ? handleContentReady : undefined}
      />
      {isLoading && (
        <div className="fixed inset-0 z-50">
          <LoadingScreen />
        </div>
      )}
      </div>
    </ToastProvider>
  );
}

export default App;
