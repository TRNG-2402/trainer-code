import axios from 'axios';

// To avoid cluttering my memory with a ton of axios objects, I can create a Singleton 
// That comes preloaded with a baseURL and is then called upon wherever needed.
// Additionally - we can later come back and attach a request interceptor (later demo) 
// for our JWT auth token that our back end sends us when we log in
export const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL, // Telling Vite to grab this when we npm run dev
    headers: {
        'Content-Type': 'application/json',
    },
});

// Coming back full circle, since we now have access to a token (via localStorage) we can 
// have our api axios object attack the token as a Bearer token for us automatically on every 
// outgoing request. If theres no token, it wont break - it'll just attach nothing. 
api.interceptors.request.use((config) => {
    
    //Grab token from localStorage
    const token = localStorage.getItem('token');

    // Check token
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;

});