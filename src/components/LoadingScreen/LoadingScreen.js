import React from 'react';

const LoadingScreen = () => (
  <div className="flex flex-col h-screen bg-gray-900 items-center justify-center z-50">
    <div className="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500 mb-4" />
    <h2 className="text-white text-xl font-semibold tracking-wider animate-pulse">SİSTEM YÜKLENİYOR...</h2>
    <p className="text-gray-500 text-sm mt-2">Veriler ve Modeller Hazırlanıyor</p>
  </div>
);

export default LoadingScreen;
