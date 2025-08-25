const API_BASE = 'https://localhost:7131/Authentication/'
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');

document.addEventListener('DOMContentLoaded', async () => {
    const res = await fetch(API_BASE + 'CheckAuthorizeStatus', {
        method: 'GET',
        credentials: 'include'
    })
    if(res.ok){
        chrome.tabs.create({ url: chrome.runtime.getURL('Dashboard/dashboard.html') });
        window.close(); // Close the popup if it's a popup  
    }
     else if (res.status === 401 || res.status === 403) {
        console.log('User is not authenticated (401/403). Redirecting to login.');
    }
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
        credentials: 'include',
        body: JSON.stringify(userLoginInfo)
    })
        .then(res => {
            if (res.ok) {
                console.log(res.text());
                chrome.tabs.create({ url: chrome.runtime.getURL('Dashboard/dashboard.html') });
                window.close(); // Close the popup if it's a popup  
            } else {
                console.error("Login failed");
            }
        })
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