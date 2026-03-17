import React, { createContext, useContext, useState, useCallback } from 'react';

const ToastContext = createContext(null);

const TOAST_DURATION_MS = 3000;

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const toast = useCallback((message, options = {}) => {
    const id = Date.now() + Math.random();
    setToasts((prev) => [...prev, { id, message, ...options }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, options.duration ?? TOAST_DURATION_MS);
  }, []);

  return (
    <ToastContext.Provider value={{ toast }}>
      {children}
      <ToastContainer toasts={toasts} setToasts={setToasts} />
    </ToastContext.Provider>
  );
}

function ToastContainer({ toasts, setToasts }) {
  return (
    <div
      className="fixed bottom-6 right-6 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none"
      aria-live="polite"
    >
      {toasts.map((t) => (
        <div
          key={t.id}
          className="toast-item pointer-events-auto px-4 py-3 rounded-lg shadow-lg border bg-gray-800 border-gray-600 text-white text-sm font-medium"
        >
          {t.message}
        </div>
      ))}
    </div>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) return { toast: () => {} };
  return ctx;
}
