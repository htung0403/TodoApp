import React from 'react';
import { AuthProvider } from './contexts/AuthContext.jsx';
import AuthWrapper from './components/AuthWrapper';
import './App.css';

function App() {
  return (
    <AuthProvider>
      <div className="App">
        <AuthWrapper />
      </div>
    </AuthProvider>
  );
}

export default App;
