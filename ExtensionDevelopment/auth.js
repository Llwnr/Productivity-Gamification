const API_BASE = 'https://localhost:7131/Authentication/'
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');

document.addEventListener('DOMContentLoaded', () => {
    chrome.storage.local.get(['authToken'], (result) => {
        console.log('Token found:');
        if (result.authToken) {
            chrome.tabs.create({ url: chrome.runtime.getURL('Dashboard/dashboard.html') });
            window.close();
        }
    });
});

loginForm.addEventListener('submit', async(e)=>{
    e.preventDefault();

    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;

    const userLoginInfo = {
        username: username,
        password: password
    }

    await fetch(API_BASE + 'Login', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(userLoginInfo)
    })
        .then(res => res.text().then(async token => {
            if(token == null) {
                console.error("Token is null. Wrong login credentials");
                return;
            }
            chrome.storage.local.set({authToken: token}, () => {
                chrome.tabs.create({ url: chrome.runtime.getURL('Dashboard/dashboard.html') });
                window.close();
            });
        }))
        .catch(err => {console.log("Error while logging in: " + err)})
    
});

registerForm.addEventListener('submit', async(e)=>{
    e.preventDefault();

    const username = document.getElementById('register-username').value;
    const password = document.getElementById('register-password').value;
    const confirmPassword = document.getElementById('register-confirm-password').value;
    const email = document.getElementById('register-email').value;
    const goal = document.getElementById('register-goal').value;

    if (password !== confirmPassword) {
        console.error("Passwords do not match");
        return;
    }

    const userRegisterInfo = {
        username: username,
        password: password,
        email: email,
        goal: goal
    }

    await fetch(API_BASE + 'Register', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(userRegisterInfo)
    })
        .then(res => {
            if (res.ok) {
                console.log("Registration successful");
                // switch to login form
                document.getElementById('register-container').classList.add('hidden');
                document.getElementById('login-container').classList.remove('hidden');
            } else {
                console.error("Registration failed");
            }
        })
        .catch(err => {console.log("Error while registering: " + err)})
});