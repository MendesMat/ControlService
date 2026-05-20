import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

export const Itinerary: React.FC = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div style={{ padding: '24px' }}>
      <h1>Roteiro de Serviços do Dia Atual</h1>
      <p>Bem-vindo à tela operacional principal.</p>
      
      <div style={{ marginTop: '24px' }}>
        <button onClick={handleLogout} className="btn-primary" style={{ width: 'auto', backgroundColor: 'var(--color-danger)' }}>
          Sair do Sistema
        </button>
      </div>
    </div>
  );
};
