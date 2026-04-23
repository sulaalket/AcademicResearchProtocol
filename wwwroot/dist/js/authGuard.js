// authGuard.js

function isLoggedIn() {
    const token = localStorage.getItem('token');
    return token && token.length > 0;
}

function getToken() {
    return localStorage.getItem('token');
}

// Global API wrapper (handles 401 everywhere)
async function apiFetch(url, options = {}) {
    const token = getToken();

    const headers = {
        ...options.headers,
        'Authorization': 'Bearer ' + token
    };

    const response = await fetch(url, {
        ...options,
        headers
    });

    // 🔥 Auto logout if token is invalid/expired
    if (response.status === 401) {
        localStorage.clear();
        window.location.href = '/pages/login.html';
        return;
    }

    return response;
}

// Protect page on load
function protectPage() {
    if (!isLoggedIn()) {
        window.location.href = '/pages/login.html';
    }
}