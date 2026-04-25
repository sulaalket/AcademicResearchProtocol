// config.js - API Configuration for Azure
const API_BASE_URL = 'https://academic-researching-cka7h3guhxb8dre8.centralus-01.azurewebsites.net';

// Helper function for API calls
async function apiRequest(endpoint, options = {}) {
    const token = localStorage.getItem('token');

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` }),
            ...options.headers
        }
    });

    if (response.status === 401) {
        localStorage.clear();
        window.location.href = '/pages/login.html';
        throw new Error('Session expired');
    }

    return response;
}